﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Extensions;
using Firebase.Storage;
using Unity.VisualScripting;
using UnityEngine.Networking;
using UnityEngine.UI;

//using to simulate taking a photo on a phone (pick a file from desktop)
using SimpleFileBrowser;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;
    
    //Firebase References
    private DependencyStatus dependencyStatus;
    private FirebaseAuth auth;    
    private FirebaseUser fbUser;
    private DatabaseReference DBref;
    private FirebaseStorage storage;
    private StorageReference storageRef;
    
    //custom user refrence
    private User traceUser;
    
    [Header("Firebase")]
    [SerializeField] private String _storageReferenceUrl;

    [Header("ScreenManager")] 
    [SerializeField] private ScreenManager _screenManager;
    
    [Header("UserData")] 
    [SerializeField] private String _baseUserPhotoUrl = "https://randomuser.me/api/portraits/men/95.png";
    public Texture userImageTexture;
   

    [Header("DatabaseTest")]
    public RawImage rawImage;
    public RawImage testRawImage;
    public TMP_InputField _searchBar;
    
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
        StartCoroutine(AutoLogin());
        
        //This is so I can test from computer
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png"), new FileBrowser.Filter("Text Files", ".txt", ".pdf"));
        FileBrowser.SetDefaultFilter(".jpg");
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");
    }
    public void LogOutOfAccount()
    {
        StartCoroutine(LogOut());
    }
    
    public IEnumerator AutoLogin()
    {
        //Todo: figure out which wait until to use...
        yield return new WaitForSeconds(0.4f); //has to wait until firebase async task is finished... (is there something better?)
        String savedUsername = PlayerPrefs.GetString("Username");
        String savedPassword = PlayerPrefs.GetString("Password");
        if (savedUsername != "null" && savedPassword != "null")
        {
            Debug.Log("auto logging in");
            StartCoroutine(FirebaseManager.instance.Login(savedUsername, savedPassword, (myReturnValue) => {
                if (myReturnValue != null)
                {
                    Debug.LogError("failed to autoLogin");
                    StartCoroutine(LogOut());
                }
                else
                {
                    _screenManager.Login();
                }
            }));
        }
        else
        {
            Debug.Log("pulling up login options");
            _screenManager.PullUpOnboardingOptions();
        }
    }
    public IEnumerator Login(string _email, string _password,  System.Action<String> callback)
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
            yield break;
        }

        fbUser = LoginTask.Result;
        Debug.LogFormat("User signed in successfully: {0} ({1})", fbUser.DisplayName, fbUser.Email);
        Debug.Log("logged In: user profile photo is: " + fbUser.PhotoUrl);
        
        //Load User Profile Texture
        StartCoroutine(FirebaseManager.instance.GetUserProfileImage((myReturnValue) => {
            if (myReturnValue != null)
            {
                userImageTexture = myReturnValue;
            }
        }));
        
        //all database things that need to be activated
        var DBTaskSetIsOnline = DBref.Child("users").Child(fbUser.UserId).Child("isOnline").SetValueAsync(true);
        yield return new WaitUntil(predicate: () => DBTaskSetIsOnline.IsCompleted);
        
        //stay logged in
        PlayerPrefs.SetString("Username", _email);
        PlayerPrefs.SetString("Password", _password);
        PlayerPrefs.Save();
        
        callback(null);
    }
    private void writeNewUser(string userId, string username, string name, string userPhotoLink, string email, string phone) {
        User user = new User(username, name, userPhotoLink, email, phone);
        string json = JsonUtility.ToJson(user);
        Debug.Log("this is the json:" + json);
        DBref.Child("users").Child(userId).SetRawJsonValueAsync(json);
    }
    
    private IEnumerator LogOut()
    {
        PlayerPrefs.SetString("Username", "null");
        PlayerPrefs.SetString("Password", "null");
        userImageTexture = null;
        auth.SignOut();
        yield return new WaitForSeconds(0.8f);
        _screenManager.PullUpOnboardingOptions();
    }
    
    //register events
    public IEnumerator RegisterNewUser(string _email, string _password, string _username, string _phoneNumber,  System.Action<String> callback)
    {
        if (_username == "")
        {
            callback("Missing Username"); //having a blank nickname is not really a DB error so I return a error here
            yield break;
        }

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
                case AuthError.WeakPassword:
                    message = "Weak Password";
                    break;
                case AuthError.EmailAlreadyInUse:
                    message = "Email Already In Use";
                    break;
            }
            Debug.LogWarning(message);
            callback(message);
            yield break;
        }

        //User has now been created
        fbUser = RegisterTask.Result;

        if (fbUser == null)
        {
            Debug.LogWarning("User Null");
            yield break;
        }

        //Create a user profile and set the username todo: set user profile image dynamically
        UserProfile profile = new UserProfile{DisplayName = _username, PhotoUrl = new Uri("https://firebasestorage.googleapis.com/v0/b/geosnapv1.appspot.com/o/ProfilePhotos%2FEmptyPhoto.jpg?alt=media&token=fbc8b18c-4bdf-44fd-a4ba-7ae881d3f063")};
        var ProfileTask = fbUser.UpdateUserProfileAsync(profile);
        yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

        if (ProfileTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
            Debug.LogWarning("Username Set Failed!");
            callback("Something Went Wrong, Sorry");
            yield break;
        }

        var user = auth.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("User Null");
            yield break;
        }

        Firebase.Auth.UserProfile userProfile = new Firebase.Auth.UserProfile
        {
            DisplayName = user.DisplayName,
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
            }
        });
        
        //Todo: All of these could be put into one json request? less cost?
        
        

        //LOOK THESE HAVE BEEN PUT IN ONE JSON REQUEST
        
        // var DBTaskSetUsername = DBref.Child("users").Child(User.UserId).Child("Username").SetValueAsync(_username);
        // yield return new WaitUntil(predicate: () => DBTaskSetUsername.IsCompleted);
        
        // var DBTaskSetPhoneNumber = DBref.Child("users").Child(User.UserId).Child("PhoneNumber").SetValueAsync(_phoneNumber);
        // yield return new WaitUntil(predicate: () => DBTaskSetPhoneNumber.IsCompleted);
        
        // var DBTaskSetIsOnline = DBref.Child("users").Child(User.UserId).Child("IsOnline").SetValueAsync(false);
        // yield return new WaitUntil(predicate: () => DBTaskSetIsOnline.IsCompleted);
        
        // var DBTaskSetFreindCount = DBref.Child("users").Child(User.UserId).Child("FriendCount").SetValueAsync(0);
        // yield return new WaitUntil(predicate: () => DBTaskSetFreindCount.IsCompleted);
        
        // var DBTaskSetTraceScore = DBref.Child("users").Child(User.UserId).Child("TraceScore").SetValueAsync(0);
        // yield return new WaitUntil(predicate: () => DBTaskSetTraceScore.IsCompleted);
        
        // var DBTaskSetLocation = DBref.Child("users").Child(User.UserId).Child("Location").SetValueAsync(null); // todo: get user location (later move to be under my friends)
        // yield return new WaitUntil(predicate: () => DBTaskSetLocation.IsCompleted);
        //ALL THIS ^ is now this:
        writeNewUser(fbUser.UserId.ToString(), _username, "null", "null",_email, _phoneNumber);
        
        //external to user (still there must be a better way)
        var DBTaskSetUsernameLinkToId = DBref.Child("usernames").Child(_username).SetValueAsync(fbUser.UserId);
        yield return new WaitUntil(predicate: () => DBTaskSetUsernameLinkToId.IsCompleted);
        
        var DBTaskSetPhoneNumberLinkToId = DBref.Child("phoneNumbers").Child(fbUser.UserId).Child(_phoneNumber).SetValueAsync(fbUser.UserId);
        yield return new WaitUntil(predicate: () => DBTaskSetPhoneNumberLinkToId.IsCompleted);

        var DBTaskSetUserFriends = DBref.Child("friends").Child(fbUser.UserId).Child("null").SetValueAsync("null");
        yield return new WaitUntil(predicate: () => DBTaskSetUserFriends.IsCompleted);
        
        
        
        
        //if nothing has gone wrong try logging in with new users information
        StartCoroutine(Login(_email, _password, (myReturnValue) => {
            if (myReturnValue != null)
            {
                Debug.LogWarning("failed to login");
            }
            else
            {
                Debug.Log("Logged In!");
            }
        }));
        callback(null);
    }

    //set firebase real time database
    public IEnumerator SetUsername(string _username, System.Action<String> callback)
    {
        Debug.Log("Db SetUsername to :" + _username);
        //Set the currently logged in user nickName in the database
        var DBTask = DBref.Child("users").Child(fbUser.UserId).Child("Username").SetValueAsync(_username);
        
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            callback("successfully updated _username");
        }
    }
    public IEnumerator SetUserProfilePhotoOld(Image _image, System.Action<String> callback)
        {
            String _profilePhotoUrl = "profileUrl";
            var user = auth.CurrentUser;
        
            if (user != null)
            {
                //this shouldn't be done through firebase Auth
                Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
                {
                    DisplayName = user.DisplayName,
                    PhotoUrl = new Uri("https://firebasestorage.googleapis.com/v0/b/geosnapv1.appspot.com/o/ProfilePhotos%2FEmptyPhoto.jpg?alt=media&token=fbc8b18c-4bdf-44fd-a4ba-7ae881d3f063")
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
        
                    fbUser = auth.CurrentUser;
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
    public IEnumerator SetUserProfilePhotoNew(string _photoUrl, System.Action<String> callback)
    {
        Debug.Log("Db update photoUrl to :" + _photoUrl);
        //Set the currently logged in user nickName in the database
        var DBTask = DBref.Child("users").Child(fbUser.UserId).Child("userPhotoUrl").SetValueAsync(_photoUrl);
        
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            callback("success");
        }
    }
    public IEnumerator SetUserNickName(string _nickName, System.Action<String> callback)
    {
        Debug.Log("Db update nick to :" + _nickName);
        //Set the currently logged in user nickName in the database
        var DBTask = DBref.Child("users").Child(fbUser.UserId).Child("NickName").SetValueAsync(_nickName);
        
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
    public IEnumerator SetUserPhoneNumber(string _phoneNumber, System.Action<String> callback)
    {
        var DBTask = DBref.Child("users").Child(fbUser.UserId).Child("phoneNumber").SetValueAsync(_phoneNumber);
        
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            callback("success");
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
    
    public void AddFriend(String _username)
    {
        String _nickName = "null";
        StartCoroutine(FirebaseManager.instance.TryAddFriend(_username, _nickName, (myReturnValue) => {
            if (myReturnValue != "Success")
            {
                Debug.LogError("failed to update freinds");
            }
            else
            {
                Debug.Log("updated friends");
            }
        }));
    }
    private IEnumerator TryAddFriend(string _username, string _nickName, System.Action<String> callback)
    {
        var DBTask = DBref.Child("users").Child(fbUser.UserId).Child("Friends").Child(_username).SetValueAsync(_nickName);
        
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        
        if (DBTask.IsFaulted)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            callback("Success");
        }
    }

    //get firebase real time database
    private IEnumerator GetUserProfileImage(System.Action<Texture> callback)
    {
        var request = new UnityWebRequest();
        var url = "";
        
        Debug.Log("test:");
        StorageReference pathReference = storage.GetReference("ProfilePhoto/"+fbUser.UserId+"/profile.png");
        Debug.Log("path refrence:" + pathReference);

        pathReference.GetDownloadUrlAsync().ContinueWithOnMainThread(task => {
            if (!task.IsFaulted && !task.IsCanceled) {
                Debug.Log("Download URL: " + task.Result);
                url = task.Result + "";
                Debug.Log("Actual  URL: " + url);
            }
            else
            {
                Debug.Log("task failed:" + task.Result);
            }
        });
        
        yield return new WaitForSecondsRealtime(0.5f); //hmm not sure why (needs to wait for GetDownloadUrlAsync to complete)
        
        request = UnityWebRequestTexture.GetTexture((url)+"");
        
        yield return request.SendWebRequest(); //Wait for the request to complete
        
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("error:" + request.error);
        }
        else
        {
            callback(((DownloadHandlerTexture)request.downloadHandler).texture);
        }
    }

    
    //get image database test
    public void getTestImage()
    {
        StartCoroutine(GetTestImage((myReturnValue) => {
            if (myReturnValue != null)
            {
                testRawImage.texture = myReturnValue;
            }
        }));
    }
    private IEnumerator GetTestImage(System.Action<Texture> callback)
    {
        var request = new UnityWebRequest();
        
        request = UnityWebRequestTexture.GetTexture("https://firebasestorage.googleapis.com/v0/b/geosnapv1.appspot.com/o/ProfilePhoto%2FPVNKPFYFrWVRoPdhsTs0aAYH5cA3%2Fprofile.png?alt=media&token=894e50e6-7a46-4dec-aca7-20945d1bca58"); //Create a request

        yield return request.SendWebRequest(); //Wait for the request to complete
        
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("error:" + request.error);
        }
        else
        {
            callback(((DownloadHandlerTexture)request.downloadHandler).texture);
        }
    }
    //

    private IEnumerator GetUserNickName(System.Action<String> callback)
    {
        var DBTask = DBref.Child("users").Child(fbUser.UserId).Child("Friends").Child("nickName").GetValueAsync();
        
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        
        if (DBTask.IsFaulted)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            callback(DBTask.Result.ToString());
        }
    }
    private IEnumerator GetUserPhoneNumber(System.Action<String> callback)
    {
        var DBTask = DBref.Child("users").Child(fbUser.UserId).Child("Friends").Child("nickName").GetValueAsync();
        
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        
        if (DBTask.IsFaulted)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            callback(DBTask.Result.ToString());
        }
    }
    
    
    //set firebase storage database
    public void UploadImage()
    {
        StartCoroutine(ShowUpLoadDialogCoroutine());
    }
    IEnumerator ShowUpLoadDialogCoroutine() 
    {

        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Load Files and Folders", "Load");

        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
            for (int i = 0; i < FileBrowser.Result.Length; i++)
                Debug.Log(FileBrowser.Result[i]);

            Debug.Log("File Selected");
            byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);
            //Editing Metadata
            var newMetadata = new MetadataChange();
            newMetadata.ContentType = "image/jpg";

            //Create a reference to where the file needs to be uploaded
            StorageReference uploadRef = storageRef.Child("ProfilePhoto/"+fbUser.UserId+"/profile.png");
            Debug.Log("File upload started");
            uploadRef.PutBytesAsync(bytes, newMetadata).ContinueWithOnMainThread((task) => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log(task.Exception.ToString());
                }
                else
                {
                    Debug.Log("File Uploaded Successfully!");
                }
            });
        }
    }

    
    //get firebase storage database
    public void DownloadImage() {
        StartCoroutine(GetUserProfileImage((myReturnValue) => {
            if (myReturnValue != null)
            {
                rawImage.texture = myReturnValue;
            }
        }));
    }
    
    
    private IEnumerator TryLoadImage(string MediaUrl, System.Action<Texture> callback) {
        // UnityWebRequest request = UnityWebRequestTexture.GetTexture("https://firebasestorage.googleapis.com/v0/b/geosnapv1.appspot.com/o/ProfilePhotos%2Frocket.png?alt=media"); //Create a request// https://firebasestorage.googleapis.com/v0/b/geosnapv1.appspot.com/o/7Lk0e2Yd5SXJzBE9BoK6occeS5l2%2FnewFile.jpeg?alt=media&token=bd7d6c80-3e68-4640-8f54-3b7db4bacb79
        UnityWebRequest request = UnityWebRequestTexture.GetTexture("https://firebasestorage.googleapis.com/v0/b/geosnapv1.appspot.com/o/"+ fbUser.UserId +"%2FnewFile.jpeg?alt=media"); //Create a request

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
}
