using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FacebookAndroid
{
    private static AndroidJavaObject _facebookPlugin;

    [CompilerGenerated]
    private static Action _003C_003Ef__am_0024cache1;

    static FacebookAndroid()
    {
#if ENABLE_FACEBOOK
		if (Application.platform == RuntimePlatform.Android)
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.prime31.FacebookPlugin"))
			{
				_facebookPlugin = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
			}
			if (_003C_003Ef__am_0024cache1 == null)
			{
				_003C_003Ef__am_0024cache1 = _003CFacebookAndroid_003Em__0;
			}
			FacebookManager.preLoginSucceededEvent += _003C_003Ef__am_0024cache1;
		}
#endif
    }

    public static void init(string appId)
    {
#if ENABLE_FACEBOOK
		if (Application.platform == RuntimePlatform.Android)
		{
			_facebookPlugin.Call("init", appId);
			Facebook.instance.accessToken = getAccessToken();
		}
#endif
    }

    public static bool isSessionValid()
    {
#if ENABLE_FACEBOOK
		if (Application.platform != RuntimePlatform.Android)
		{
			return false;
		}
		return _facebookPlugin.Call<bool>("isSessionValid", new object[0]);
#else
        return false;
#endif
    }

    public static string getAccessToken()
    {
#if ENABLE_FACEBOOK
		if (Application.platform != RuntimePlatform.Android)
		{
			return string.Empty;
		}
		return _facebookPlugin.Call<string>("getSessionToken", new object[0]);
#else
        return string.Empty;
#endif
    }

    public static void extendAccessToken()
    {
#if ENABLE_FACEBOOK
		if (Application.platform == RuntimePlatform.Android)
		{
			_facebookPlugin.Call("extendAccessToken");
		}
#endif
    }

    public static void login()
    {
#if ENABLE_FACEBOOK
        loginWithRequestedPermissions(new string[0]);
#endif
    }

    public static void loginWithRequestedPermissions(string[] permissions, string urlSchemeSuffix)
    {
#if ENABLE_FACEBOOK
        loginWithRequestedPermissions(permissions);
#endif
    }

    public static void loginWithRequestedPermissions(string[] permissions)
    {
#if ENABLE_FACEBOOK
		if (Application.platform == RuntimePlatform.Android)
		{
			IntPtr methodID = AndroidJNI.GetMethodID(_facebookPlugin.GetRawClass(), "showLoginDialog", "([Ljava/lang/String;)V");
			AndroidJNI.CallVoidMethod(_facebookPlugin.GetRawObject(), methodID, AndroidJNIHelper.CreateJNIArgArray(new object[1] { permissions }));
		}
#endif
    }

    public static void logout()
    {
#if ENABLE_FACEBOOK
        if (Application.platform == RuntimePlatform.Android)
        {
            _facebookPlugin.Call("logout");
            Facebook.instance.accessToken = string.Empty;
        }
#endif
    }

    public static void showPostMessageDialog()
    {
#if ENABLE_FACEBOOK
        showDialog("stream.publish", null);
#endif
    }

    public static void showPostMessageDialogWithOptions(string link, string linkName, string linkToImage, string caption)
    {
#if ENABLE_FACEBOOK
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        dictionary.Add("link", link);
        dictionary.Add("name", linkName);
        dictionary.Add("picture", linkToImage);
        dictionary.Add("caption", caption);
        Dictionary<string, string> parameters = dictionary;
        showDialog("stream.publish", parameters);
#endif
    }

    public static void showDialog(string dialogType, Dictionary<string, string> parameters)
    {
#if ENABLE_FACEBOOK
		if (Application.platform != RuntimePlatform.Android)
		{
			return;
		}
		using (AndroidJavaObject androidJavaObject = new AndroidJavaObject("android.os.Bundle"))
		{
			IntPtr methodID = AndroidJNI.GetMethodID(androidJavaObject.GetRawClass(), "putString", "(Ljava/lang/String;Ljava/lang/String;)V");
			object[] array = new object[2];
			if (parameters != null)
			{
				foreach (KeyValuePair<string, string> parameter in parameters)
				{
					array[0] = new AndroidJavaObject("java.lang.String", parameter.Key);
					array[1] = new AndroidJavaObject("java.lang.String", parameter.Value);
					AndroidJNI.CallVoidMethod(androidJavaObject.GetRawObject(), methodID, AndroidJNIHelper.CreateJNIArgArray(array));
				}
			}
			_facebookPlugin.Call("showDialog", dialogType, androidJavaObject);
		}
#endif
    }

    public static void restRequest(string restMethod, string httpMethod, Dictionary<string, string> parameters)
    {
#if ENABLE_FACEBOOK
		if (Application.platform != RuntimePlatform.Android)
		{
			return;
		}
		if (parameters == null)
		{
			parameters = new Dictionary<string, string>();
		}
		parameters.Add("method", restMethod);
		using (AndroidJavaObject androidJavaObject = new AndroidJavaObject("android.os.Bundle"))
		{
			IntPtr methodID = AndroidJNI.GetMethodID(androidJavaObject.GetRawClass(), "putString", "(Ljava/lang/String;Ljava/lang/String;)V");
			object[] array = new object[2];
			foreach (KeyValuePair<string, string> parameter in parameters)
			{
				array[0] = new AndroidJavaObject("java.lang.String", parameter.Key);
				array[1] = new AndroidJavaObject("java.lang.String", parameter.Value);
				AndroidJNI.CallVoidMethod(androidJavaObject.GetRawObject(), methodID, AndroidJNIHelper.CreateJNIArgArray(array));
			}
			_facebookPlugin.Call("restRequest", httpMethod, androidJavaObject);
		}
#endif
    }

    public static void graphRequest(string graphPath, string httpMethod, Dictionary<string, string> parameters)
    {
#if ENABLE_FACEBOOK
		if (Application.platform != RuntimePlatform.Android)
		{
			return;
		}
		using (AndroidJavaObject androidJavaObject = new AndroidJavaObject("android.os.Bundle"))
		{
			IntPtr methodID = AndroidJNI.GetMethodID(androidJavaObject.GetRawClass(), "putString", "(Ljava/lang/String;Ljava/lang/String;)V");
			object[] array = new object[2];
			if (parameters != null)
			{
				foreach (KeyValuePair<string, string> parameter in parameters)
				{
					array[0] = new AndroidJavaObject("java.lang.String", parameter.Key);
					array[1] = new AndroidJavaObject("java.lang.String", parameter.Value);
					AndroidJNI.CallObjectMethod(androidJavaObject.GetRawObject(), methodID, AndroidJNIHelper.CreateJNIArgArray(array));
				}
			}
			_facebookPlugin.Call("graphRequest", graphPath, httpMethod, androidJavaObject);
		}
#endif
    }

    [CompilerGenerated]
    private static void _003CFacebookAndroid_003Em__0()
    {
        Facebook.instance.accessToken = getAccessToken();
    }
}
