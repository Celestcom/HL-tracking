﻿/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Hardlight.SDK.Tracking;

namespace Hardlight.SDK
{

	/// <summary>
	/// HardlightManager provides access to a essential suit functions, 
	/// including enabling/disabling tracking, monitoring suit connection status, 
	/// globally pausing and playing effects, and clearing all playing effects.
	/// 
	/// If you prefer to interact directly with the plugin, you may instantiate and destroy your own
	/// HLVR_Plugin and remove HardlightManager.
	/// </summary>

	[ExecuteInEditMode]
	public sealed class HardlightManager : MonoBehaviour
	{
		public const int HAPTIC_LAYER = 31;

		#region Public Events 
		/// <summary>
		/// Raised when a suit disconnects
		/// </summary>
		public event EventHandler<SuitConnectionArgs> SuitDisconnected;
		/// <summary>
		/// Raised when a suit connects
		/// </summary>
		public event EventHandler<SuitConnectionArgs> SuitConnected;
		/// <summary>
		/// Raised when the plugin establishes connection with the Hardlight VR Runtime
		/// </summary>
		public event EventHandler<ServiceConnectionArgs> ServiceConnected;
		/// <summary>
		/// Raised when the plugin loses connection to the Hardlight VR Runtime
		/// </summary>
		public event EventHandler<ServiceConnectionArgs> ServiceDisconnected;
		#endregion

		/// <summary>
		/// Returns DeviceConnectionStatus.Connected if a suit is connected, else returns DeviceConnectionStatus.Disconnected
		/// </summary>
		public bool IsSuitConnected
		{
			get
			{
				return _DeviceConnectionStatus == DeviceConnectionStatus.Connected;
			}
		}

		/// <summary>
		/// Returns ServiceConnectionStatus.Connected if the plugin is connected to the Hardlight VR Runtime service, else returns ServiceConnectionStatus.Disconnected
		/// </summary>
		public bool IsServiceConnected
		{
			get
			{
				return _ServiceConnectionStatus == ServiceConnectionStatus.Connected;
			}
		}

		/// <summary>
		/// Use the Instance variable to access the HardlightManager object. There should only be one HardlightManager in a scene.
		/// in the scene. 
		/// </summary>
		private static HardlightManager instance;
		public static HardlightManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = FindObjectOfType<HardlightManager>();

					if (FindObjectsOfType<HardlightManager>().Length > 1)
					{
						Debug.LogError("[HardlightManager] There is more than one HardlightManager Singleton\n" +
							"There shouldn't be multiple HardlightManager objects");
						return instance;
					}

					if (instance == null)
					{
						GameObject singleton = new GameObject();
						instance = singleton.AddComponent<HardlightManager>();
						singleton.name = "HardlightManager [Runtime Singleton]";
					}
					else
					{
						//Debug.Log("[Singleton] Using instance already created: " +
						//	_instance.gameObject.name + "\n");
					}
				}
				return instance;
			}
			set { instance = value; }
		}

		#region Suit Options 
		[Header("- Suit Options -")]
		[Tooltip("EXPERIMENTAL: may impact performance of haptics on suit, and data refresh rate may be low")]
		[SerializeField]
		private bool EnableSuitTracking = false;
		//[Tooltip("Creates a suit connection indicator on runtime.")]
		//[SerializeField]
		//private bool CreateDebugDisplay = false;
		#endregion

		private bool _lastSuitTrackingEnabledValue = false;
		private bool _isTrackingCoroutineRunning = false;
		private bool _isFrozen = false;

		private IEnumerator _trackingUpdateLoop;
		private IEnumerator _ServiceConnectionStatusLoop;
		private IEnumerator _DeviceConnectionStatusLoop;

		private DeviceConnectionStatus _DeviceConnectionStatus;
		private ServiceConnectionStatus _ServiceConnectionStatus;

		private HLVR.HLVR_Plugin _plugin;

		/// <summary>
		/// Enable SDK tracking management loop.
		/// </summary>
		public void EnableTracking()
		{
			EnableSuitTracking = true;
			if (!_isTrackingCoroutineRunning)
			{
				StartCoroutine(_trackingUpdateLoop);
				_isTrackingCoroutineRunning = true;
			}
		}

		/// <summary>
		/// Disable SDK tracking management loop.
		/// </summary>
		public void DisableTracking()
		{
			EnableSuitTracking = false;
			StopCoroutine(_trackingUpdateLoop);
			_isTrackingCoroutineRunning = false;
		}

		public TrackingUpdate PollTracking()
		{
			if (_plugin != null)
			{
				var update = _plugin.PollTracking();
				return update;
			}
			return new TrackingUpdate();
		}

		public static VersionInfo GetPluginVersionInfo()
		{
			return HLVR.HLVR_Plugin.GetPluginVersion();
		}
		public Dictionary<AreaFlag, EffectSampleInfo> SamplePlayingStatus()
		{
			throw new NotImplementedException("There is a big problem.\n\tSample Playing Status has not been reimplemented.\n");
			//return _plugin.PollBodyView();
			//return _plugin.SampleCurrentlyPlayingEffects();
			//return _plugin.SampleStrengths();
		}
		/// <summary>
		/// Control the haptic volume of an area directly. 
		/// </summary>
		/// <param name="singleArea">An AreaFlag representing a single area</param>
		/// <param name="strength">Strength to play, from 0.0-1.0</param>
		public void ControlDirectly(AreaFlag singleArea, double strength)
		{
			//_plugin.ControlDirectly(singleArea, strength * .66f);
		}

		/// <summary>
		/// Control the haptic volume of multiple areas directly. 
		/// </summary>
		/// <param name="singleAreas">List of AreaFlags, each representing a single area</param>
		/// <param name="strengths">Strength to play, from 0-255</param>
		public void ControlDirectly(AreaFlag[] singleAreas, ushort[] strengths)
		{
			//_plugin.ControlDirectly(singleAreas, strengths);

		}

		private DeviceConnectionStatus ChangeDeviceConnectionStatus(DeviceConnectionStatus newStatus)
		{
			if (newStatus == DeviceConnectionStatus.Connected)
			{
				OnSuitConnected(new SuitConnectionArgs());
			}
			else
			{
				OnSuitDisconnected(new SuitConnectionArgs());
			}
			return newStatus;
		}

		private ServiceConnectionStatus ChangeServiceConnectionStatus(ServiceConnectionStatus newStatus)
		{
			if (newStatus == ServiceConnectionStatus.Connected)
			{
				OnServiceConnected(new ServiceConnectionArgs());

			}
			else
			{
				OnServiceDisconnected(new ServiceConnectionArgs());
			}

			return newStatus;
		}
		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else if (Instance != this)
			{
				Debug.LogError("There should only be one HardlightManager! Make sure there is only one HardlightManager prefab in the scene\n" +
					"If there is no HardlightManager, one will be created for you if you call any HardlightManager.Instance function.");
			}

			_trackingUpdateLoop = UpdateTracking();
			_ServiceConnectionStatusLoop = CheckServiceConnection();
			_DeviceConnectionStatusLoop = CheckHardlightSuitConnection();

			InitPluginIfNull();
		}
		public void InitPluginIfNull()
		{
			//The plugin needs to load resources from your app's Streaming Assets folder
			if (_plugin == null)
			{
				//Debug.Log("Plugin has been initialized\n", this);
				_plugin = new HLVR.HLVR_Plugin();
			}
		}
		private void DoDelayedAction(float delay, Action action)
		{
			StartCoroutine(DoDelayedActionHelper(delay, action));
		}
		private IEnumerator DoDelayedActionHelper(float delay, Action action)
		{
			yield return new WaitForSeconds(delay);
			action();
		}
		private void OnSuitConnected(SuitConnectionArgs a)
		{
			var handler = SuitConnected;
			if (handler != null) { handler(this, a); }
		}

		private void OnSuitDisconnected(SuitConnectionArgs a)
		{
			var handler = SuitDisconnected;
			if (handler != null) { handler(this, a); }
		}

		private void OnServiceConnected(ServiceConnectionArgs a)
		{
			var handler = ServiceConnected;
			if (handler != null) { handler(this, a); }
		}

		private void OnServiceDisconnected(ServiceConnectionArgs a)
		{
			var handler = ServiceDisconnected;
			if (handler != null) { handler(this, a); }
		}
		public void Start()
		{
			//Begin monitoring the status of the suit
			_lastSuitTrackingEnabledValue = EnableSuitTracking;


			if (EnableSuitTracking)
			{
				StartCoroutine(_trackingUpdateLoop);
				_isTrackingCoroutineRunning = true;
				this.SuitConnected += ActivateImus;
			}


			DoDelayedAction(1.0f, delegate ()
			{
				StartCoroutine(_ServiceConnectionStatusLoop);
			});
			DoDelayedAction(1.0f, delegate ()
			{
				StartCoroutine(_DeviceConnectionStatusLoop);
			});
		}
		/// <summary>
		/// For use in application pause routine. Pauses currently executing haptic effects and is a no-op if called more than once. 
		/// </summary>
		public void PauseAllEffects()
		{
			if (_isFrozen)
			{
				Debug.LogWarning("PauseAllEffects() and ResumePausedEffects() are intended for use in an application's play/pause routines: pause should be paired with a resume.");
				return;
			}
			_plugin.PauseAll();
			_isFrozen = true;
		}

		/// <summary>
		/// For use in an application unpause routine. Resumes effects that were paused by PauseAllEffects(). If the effects were paused by you, i.e. mySequence.Pause(), they will remain paused.
		/// </summary>
		public void ResumePausedEffects()
		{
			_plugin.ResumeAll();
			_isFrozen = false;

		}

		/// <summary>
		/// Cancels and destroys all effects immediately, invalidating any HapticHandles
		/// </summary>
		public void ClearAllEffects()
		{
			_plugin.ClearAll();
		}

		private void ActivateImus(object sender, SuitConnectionArgs e)
		{
			this.EnableTracking();
		}

		private IEnumerator UpdateTracking()
		{
			while (true)
			{
				//_imuCalibrator.ReceiveUpdate(_plugin.PollTracking());
				yield return null;
			}
		}

		private IEnumerator CheckHardlightSuitConnection()
		{
			while (true)
			{
				if(true)
				{
					var devices = _plugin.GetKnownDevices();
					bool hasSuit = false;
					for (int i = 0; i < devices.Count; i++)
					{
						hasSuit = devices[0].Connected && devices[0].Name.Contains("Hardlight");
					}

					var deviceConnected = hasSuit ? DeviceConnectionStatus.Connected : DeviceConnectionStatus.Disconnected;
					if (deviceConnected != _DeviceConnectionStatus)
					{
						_DeviceConnectionStatus = ChangeDeviceConnectionStatus(deviceConnected);
					}

					_DeviceConnectionStatus = deviceConnected;
				}
				yield return new WaitForSeconds(0.5f);
			}
		}

		private IEnumerator CheckServiceConnection()
		{
			while (true)
			{
				ServiceConnectionStatus status = _plugin.IsConnectedToService();
				if (status != _ServiceConnectionStatus)
				{
					_ServiceConnectionStatus = ChangeServiceConnectionStatus(status);
				}

				if (status == ServiceConnectionStatus.Connected)
				{
					//_plugin.GetKnownDevices().Count > 0;
					var suitConnection = _plugin.IsConnectedToService();

					_ServiceConnectionStatus = suitConnection;
					//Debug.Log("Suit/Device connection status is not yet implemented\n");
					//throw new NotImplementedException("Suit/Device connection status is not yet implemented\n");
					//if (suitConnection != _DeviceConnectionStatus)
					//{
					//	_DeviceConnectionStatus = ChangeDeviceConnectionStatus(suitConnection);
					//}
				}
				else
				{

					if (_DeviceConnectionStatus != DeviceConnectionStatus.Disconnected)
					{
						_DeviceConnectionStatus = ChangeDeviceConnectionStatus(DeviceConnectionStatus.Disconnected);
					}

				}
				yield return new WaitForSeconds(0.5f);
			}
		}

		void Update()
		{
			if (_lastSuitTrackingEnabledValue != EnableSuitTracking)
			{
				if (EnableSuitTracking)
				{
					this.EnableTracking();
				}
				else
				{
					this.DisableTracking();
				}

				_lastSuitTrackingEnabledValue = EnableSuitTracking;
			}
		}

		public void Shutdown()
		{
			if (_plugin != null)
			{
				_plugin.Dispose();
			}
			_plugin = null;
		}

		void OnApplicationQuit()
		{
			ClearAllEffects();
			System.Threading.Thread.Sleep(100);
		}
	}
}
