﻿/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using System.Collections;

namespace Hardlight.SDK.Demos
{
	/// <summary>
	/// Largely Empty SuitDemo
	/// Sharing an empty version over the incomplete & buggy one.
	/// </summary>
	public class SuitTrackingDemo : SuitDemo
	{
		//Turn on my needed things
		public override void ActivateDemo()
		{
			HandleRequiredObjects(true);
			HardlightManager.Instance.EnableTracking();
		}

		//Turn off my needed things
		public override void DeactivateDemo()
		{
			HandleRequiredObjects(false);
			HardlightManager.Instance.DisableTracking();
		}

		public override void OnSuitClicked(HardlightCollider clicked, RaycastHit hit)
		{
			//Click to recalibrate Suit

			//Click to play that pad?
		}

		public override void OnSuitClicking(HardlightCollider suit, RaycastHit hit)
		{ }
		public override void OnSuitNoInput()
		{ }
	}
}