using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
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

	public static void writeObject(string path, object data) {
		if (null == data) {
			return;
		}

		System.Xml.Serialization.XmlSerializer writer = 
			new System.Xml.Serialization.XmlSerializer(data.GetType());

		System.IO.FileStream file = System.IO.File.Create(path);		
		writer.Serialize(file, data);
		file.Close();
		file.Dispose();
	}

	public static object readObject(string path, Type type) {
		System.Xml.Serialization.XmlSerializer reader = 
			new System.Xml.Serialization.XmlSerializer(type);
		System.IO.StreamReader file = new System.IO.StreamReader(
			path);

		object result = reader.Deserialize(file);
		file.Close();
		file.Dispose();

		return result;
	}
}