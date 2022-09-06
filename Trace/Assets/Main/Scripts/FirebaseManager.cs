using System;
using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Linq;
using Firebase.Extensions;
using Firebase.Storage;
using Unity.VisualScripting;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;
    
    //Firebase References
    private DependencyStatus dependencyStatus;
    private FirebaseAuth auth;    
    private FirebaseUser User;
    private DatabaseReference DBref;
    private FirebaseStorage storage;
    private StorageReference storageRef;
    
    [Header("Firebase")]
    [SerializeField] private String _storageReferenceUrl;

    [Header("ScreenManager")] 
    [SerializeField] private ScreenManager _screenManager;
    
    [Header("UserData")] 
    [SerializeField] private String _baseUserPhotoUrl = "https://randomuser.me/api/portraits/men/95.jpg";
    public Texture userImageTexture;
    
    
    //initializer
    void Awake()
    {
        if (instance != null)
        {Destroy(gameObject);}
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.GetReferenceFromUrl(_storageReferenceUrl);
        
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }
    private void InitializeFirebase()
    {
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
        DBref = FirebaseDatabase.DefaultInstance.RootReference;
    }
    private void Start()
    {
        StartCoroutine(TryAutoLogin());
    }
    private IEnumerator TryAutoLogin()
    {
        //Todo: figure out which wait until to use...
        yield return new WaitForSeconds(0.3f); //has to wait until firebase async task is finished... (is there something better?)
        String savedUsername = PlayerPrefs.GetString("Username");
        String savedPassword = PlayerPrefs.GetString("Password");
        if (savedUsername != "null" && savedPassword != "null")
        {
            Debug.Log("auto logging in");
            StartCoroutine(FirebaseManager.instance.TryLogin(savedUsername, savedPassword, (myReturnValue) => {
                if (myReturnValue != null)
                {
                    Debug.LogError("failed to autoLogin");
                }
                else
                {
                    _screenManager.GetComponent<ScreenManager>().Login();
                }
            }));
        }
        else
        {
            Debug.Log("changing page to login options screen");
            _screenManager.ChangeScreen("LockScreenHome");
        }
    }
    public IEnumerator TryLogin(string _email, string _password,  System.Action<String> callback)
    {
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            callback(message); //return errors
        }
        else
        {
            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            Debug.Log("logged In: user profile photo is: " + User.PhotoUrl);
            
            //Load User Profile Texture
            StartCoroutine(FirebaseManager.instance.TryLoadUserProfileImage((myReturnValue) => {
                if (myReturnValue != null)
                {
                    userImageTexture = myReturnValue;
                }
            }));
            
            //stay logged in
            PlayerPrefs.SetString("Username", _email);
            PlayerPrefs.SetString("Password", _password);
            PlayerPrefs.Save();
            
            yield return null;
            callback(null);
        }
    }
    public IEnumerator TryRegister(string _email, string _password, string _username,  System.Action<String> callback)
    {
        if (_username == "")
        {
            callback("Missing Username");
        }
        else 
        {
            //Call the Firebase auth signin function passing the email and password
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    // case AuthError.WeakPassword:
                    //     message = "Weak Password";
                    //     break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                Debug.LogWarning(message);
                callback(message);
            }
            else
            {
                //User has now been created
                //Now get the result
                User = RegisterTask.Result;

                if (User != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile{DisplayName = _username};
                    
                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        // warningRegisterText.text = "Username Set Failed!";
                        Debug.LogWarning("Username Set Failed!");
                        
                        callback("Something Went Wrong, Sorry");
                    }
                    else
                    {
                        //log user in
                        StartCoroutine(TryLogin(_email, _password, (myReturnValue) => {
                            if (myReturnValue != null)
                            {
                                Debug.LogWarning("failed to login");
                            }
                            else
                            {
                                Debug.Log("Logged In!");
                            }
                        }));
                        
                        //set base user profile photo
                        var user = auth.CurrentUser;
                        if (user != null)
                        {
                            Firebase.Auth.UserProfile userProfile = new Firebase.Auth.UserProfile
                            {
                                DisplayName = user.DisplayName,
                                PhotoUrl = new System.Uri("https://www.htgtrading.co.uk/wp-content/uploads/2016/03/no-user-image-square.jpg")
                            };
                            user.UpdateUserProfileAsync(userProfile).ContinueWith(task =>
                            {
                                if (task.IsCanceled)
                                {
                                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                                    return;
                                }
    
                                if (task.IsFaulted)
                                {
                                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                                    return;
                                }
    
                                User = auth.CurrentUser;
                                Debug.Log(user.DisplayName);
                                Debug.Log(user.PhotoUrl);
    
                                Debug.Log("User profile updated successfully.");
    
                                Debug.Log("current user url:" + user.PhotoUrl);
                                // userProfile.GetProfile();
                            });
    
                            yield return null;
                        }
                    }
                }
                callback(null);
            }
        }
    }
    public IEnumerator TryUpdateUserNickName(string _nickName, System.Action<String> callback)
    {
        //Set the currently logged in user nickName in the database
        var DBTask = DBref.Child("users").Child(User.UserId).Child("nickName").SetValueAsync(_nickName);
        
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            callback("successfully updated nickName");
        }
    }
    public IEnumerator TryUpdateProfilePhoto(Image _image, System.Action<String> callback)
    {
        String _profilePhotoUrl = "profileUrl";
        var user = auth.CurrentUser;
    
        if (user != null)
        {
            Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
            {
                DisplayName = user.DisplayName,
                PhotoUrl = new System.Uri(_baseUserPhotoUrl)
            };
            user.UpdateUserProfileAsync(profile).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
    
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }
    
                User = auth.CurrentUser;
                Debug.Log(user.DisplayName);
                Debug.Log(user.PhotoUrl);
    
                Debug.Log("User profile updated successfully.");
    
                Debug.Log("current user url:" + user.PhotoUrl);
                _profilePhotoUrl = user.PhotoUrl.ToString();
                // userProfile.GetProfile();
            });
    
            yield return null;
            callback(_profilePhotoUrl);
        }
    }
    private IEnumerator TryLoadUserProfileImage(System.Action<Texture> callback)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(User.PhotoUrl); //Create a request
        yield return request.SendWebRequest(); //Wait for the request to complete
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogWarning(request.error);
        }
        else
        {
            callback(((DownloadHandlerTexture)request.downloadHandler).texture);
        }
    }
    public void SignOut()
    {
        PlayerPrefs.SetString("Username", "null");
        PlayerPrefs.SetString("Password", "null");
        auth.SignOut();
    }
    
    //NOT USED YET (work in progress)
    private IEnumerator TryUpdateUsernameAuth(string _username)
    {
        //Create a user profile and set the username
        UserProfile profile = new UserProfile { DisplayName = _username }; //Todo: add other user data that shouldn't be changed
    
        //Call the Firebase auth update user profile function passing the profile with the username
        var ProfileTask = User.UpdateUserProfileAsync(profile);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);
    
        if (ProfileTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
        }
        else
        {
            //Auth username is now updated
        }        
    }
    private IEnumerator TryLoadImage(string MediaUrl, System.Action<Texture> callback)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl); //Create a request
        yield return request.SendWebRequest(); //Wait for the request to complete
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            callback(((DownloadHandlerTexture)request.downloadHandler).texture);
        }
    }
    private IEnumerator TryUploadImage(string MediaUrl, System.Action<Texture> callback)
    {
        
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl); //Create a request
        yield return request.SendWebRequest(); //Wait for the request to complete
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            callback(((DownloadHandlerTexture)request.downloadHandler).texture);
        }
    }
    private IEnumerator LoadUserData()
    {
        //Get the currently logged in user data
        var DBTask = DBref.Child("users").Child(User.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            //No data exists yet
            
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;
            // xpField.text = snapshot.Child("xp").Value.ToString();
            // killsField.text = snapshot.Child("kills").Value.ToString();
            // deathsField.text = snapshot.Child("deaths").Value.ToString();
        }
    }
    private void DeleteFile(String _location)
    {
        storageRef = storageRef.Child(_location);
        storageRef.DeleteAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted) {
                Debug.Log("File deleted successfully.");
            }
            else {
                // Uh-oh, an error occurred!
            }
        });
    }
}
