using System;
using System.Collections;
using System.Globalization;
using DG.Tweening;

namespace Mapbox.Examples
{
	using UnityEngine;
	using Mapbox.Utils;
	using Mapbox.Unity.Map;
	using Mapbox.Unity.MeshGeneration.Factories;
	using Mapbox.Unity.Utilities;
	using System.Collections.Generic;

	public class SpawnOnMap : MonoBehaviour
	{
		[SerializeField]
		AbstractMap _map;

		[SerializeField] [Geocode] string[] _locationStrings;
		
		Vector2d[] _locations;

		[SerializeField] float _spawnScale = 100f;
		[SerializeField] GameObject _markerPrefab;
		
		[Header("move stuff")]
		[SerializeField] private float _moveSpeed = 1f; 
		[SerializeField] private float _mouseVelocityThreshold;
		[SerializeField] private bool mouseDown;
		[SerializeField] private float mouseDownTime;

		private float _moveSpeedScaleFactor = 0;
		List<GameObject> _spawnedObjects;

		void Start()
		{
			_locations = new Vector2d[_locationStrings.Length];
			_spawnedObjects = new List<GameObject>();
			for (int i = 0; i < _locationStrings.Length; i++)
			{
				var locationString = _locationStrings[i];
				_locations[i] = Conversions.StringToLatLon(locationString);
				var instance = Instantiate(_markerPrefab);
				instance.transform.localPosition = _map.GeoToWorldPosition(_locations[i], true);
				instance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
				_spawnedObjects.Add(instance);
			}
		}

		private void Update()
		{
			if (Input.GetMouseButtonDown(0))
				mouseDown = true;
			else if (Input.GetMouseButtonUp(0))
				mouseDown = false;
			
			if (mouseDown)
			{
				mouseDownTime += 1 * Time.deltaTime;
			}
			else
			{
				mouseDownTime = 0;
			}

			int count = _spawnedObjects.Count;
			for (int i = 0; i < count; i++)
			{
				var spawnedObject = _spawnedObjects[i];
				var location = _locations[i];
				
				spawnedObject.transform.localPosition =_map.GeoToWorldPosition(location, true);
				Vector3 _newPos = _map.GeoToWorldPosition(location, true);
				
				var _mouseVelocity = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")).magnitude;
				if (_mouseVelocity > _mouseVelocityThreshold && mouseDown && mouseDownTime > 0.1f)
				{
					_moveSpeedScaleFactor = 10;
					spawnedObject.transform.DOScale(new Vector3(0, 0, 0), 0.25f);
				}
				else if(!mouseDown)
				{
					_moveSpeedScaleFactor = 1;
					spawnedObject.transform.DOScale(new Vector3(1, 1, 1), 0.5f);				
				}
			}
		}
	}
}