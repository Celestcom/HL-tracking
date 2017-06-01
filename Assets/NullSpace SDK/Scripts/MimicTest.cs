﻿using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	public class MimicTest : MonoBehaviour
	{
		public LayerMask ValidLayers;
		//HapticSequence seq;
		void Start()
		{
			//seq = new HapticSequence();
			//seq.LoadFromAsset("Haptics/pulse");
		}

		void Update()
		{
			#region Hide BodyMimic
			if (Input.GetKeyDown(KeyCode.F3))
			{
				//for (int i = 0; i < 5000; i++)
				//{
				//	seq.CreateHandle(AreaFlag.All_Areas).Play();
				//}
			}
			#endregion
			#region Hide BodyMimic
			if (Input.GetKeyDown(KeyCode.F4))
			{
				//This sets up a base body. It hands in the camera and the layer to hide.
				BodyMimic.Initialize(Camera.main, NSManager.HAPTIC_LAYER);
			}
			#endregion
			#region Setup Camera Mimic
			if (Input.GetKeyDown(KeyCode.F5))
			{
				Debug.Log(VRObjectMimic.Holder.Camera.transform.position + "\n");
			}
			#endregion
			#region Initialize BodyMimic
			if (Input.GetKeyDown(KeyCode.F6))
			{
				BodyMimic.Initialize();
			}
			#endregion
			#region Do single point-to-nearest haptic
			if (Input.GetKeyDown(KeyCode.F7))
			{
				PlayerBody body = PlayerBody.Find();
				if (body != null)
				{
					body.Hit(body.transform.position + Random.onUnitSphere / 2, "pulse");
				}
			}
			#endregion
			#region Do 500 point-to-nearest haptic. For testing to see pad hit rates
			if (Input.GetKeyDown(KeyCode.F8))
			{
				PlayerBody body = PlayerBody.Find();
				if (body != null)
				{
					for (int i = 0; i < 500; i++)
					{
						body.Hit(body.transform.position + Random.onUnitSphere / 2, "pulse");
					}
				}
			}
			#endregion
			#region Line of Sight Find Nearest
			if (Input.GetKeyDown(KeyCode.F9))
			{
				PlayerBody body = PlayerBody.Find();
				if (body != null)
				{
					body.FindNearbyLocation(body.transform.position + Random.onUnitSphere / 2, true, ValidLayers);
				}
			}
			#endregion
			#region Request Random Location
			if (Input.GetKeyDown(KeyCode.F10))
			{
				PlayerBody body = PlayerBody.Find();
				if (body != null)
				{
					Debug.DrawLine(body.transform.position + Random.onUnitSphere / 2, body.FindRandomLocation().transform.position, Color.cyan, 6.0f);
				}
			}
			#endregion
			#region Test NumberOfArea flag counting and IsSingleArea
			if (Input.GetKeyDown(KeyCode.F11))
			{
				AreaFlag flag = (AreaFlag.All_Areas).RemoveArea(AreaFlag.Back_Both);
				Debug.Log(flag.NumberOfAreas() + "\n" + flag.IsSingleArea());

				flag = AreaFlag.Back_Both;
				Debug.Log(flag.NumberOfAreas() + "\n" + flag.IsSingleArea());

				flag = AreaFlag.Right_All;
				Debug.Log(flag.NumberOfAreas() + "\n" + flag.IsSingleArea());

				flag = AreaFlag.Mid_Ab_Left;
				Debug.Log(flag.NumberOfAreas() + "\n" + flag.IsSingleArea());
			}
			#endregion
		}
	}
}