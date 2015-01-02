using System;
using UnityEngine;

public class PlatformUtils {
	public static void sendMail(string address) {
		if (Application.platform == RuntimePlatform.Android) {
			AndroidJavaClass unitPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject mainActivity = unitPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");			
			AndroidJavaClass androidUtils = new AndroidJavaClass("com.xophiix.AndroidUtils");
			androidUtils.CallStatic("sendMail", new object[]{mainActivity, address});
		} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
			// TODO: 
		} else {
			Application.OpenURL("mailto:" + address);

		}
	}
}