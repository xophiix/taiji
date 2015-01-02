package com.xophiix;

import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.util.Log;

public class AndroidUtils {
	public static void sendMail(Context context, String address) {
		Log.d("xophiix", "send mail " + address);
		
		String url = "mailto:" + address;
		Intent intent = new Intent(Intent.ACTION_VIEW);
		intent.setData(Uri.parse(url));
		context.startActivity(intent);
	}
}
