using UnityEngine;

public class ChartBoostAndroid
{
    private static AndroidJavaObject _plugin;

    static ChartBoostAndroid()
    {
#if ENABLE_CHARTBOOST
		if (Application.platform != RuntimePlatform.Android)
		{
			return;
		}
		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.prime31.ChartBoostPlugin"))
		{
			_plugin = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
		}
#endif
    }

    public static void init(string appId, string appSignature)
    {
#if ENABLE_CHARTBOOST
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("init", appId, appSignature);
		}
#endif
    }

    public static void cacheInterstitial(string location)
    {
#if ENABLE_CHARTBOOST
		if (Application.platform == RuntimePlatform.Android)
		{
			if (location == null)
			{
				location = string.Empty;
			}
			_plugin.Call("cacheInterstitial", location);
		}
#endif
    }

    public static bool hasCachedInterstitial(string location)
    {
#if ENABLE_CHARTBOOST
		if (Application.platform != RuntimePlatform.Android)
		{
			return false;
		}
		if (location == null)
		{
			location = string.Empty;
		}
		return _plugin.Call<bool>("hasCachedInterstitial", new object[1] { location });
#else
        return false;
#endif
    }

    public static void showInterstitial(string location)
    {
#if ENABLE_CHARTBOOST
		if (Application.platform == RuntimePlatform.Android)
		{
			if (location == null)
			{
				location = string.Empty;
			}
			_plugin.Call("showInterstitial", location);
		}
#endif
    }

    public static void cacheMoreApps()
    {
#if ENABLE_CHARTBOOST
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("cacheMoreApps");
		}
#endif
    }

    public static bool hasCachedMoreApps()
    {
#if ENABLE_CHARTBOOST
		if (Application.platform != RuntimePlatform.Android)
		{
			return false;
		}
		return _plugin.Call<bool>("hasCachedMoreApps", new object[0]);
#else
        return false;
#endif
    }

    public static void showMoreApps()
    {
#if ENABLE_CHARTBOOST
		if (Application.platform == RuntimePlatform.Android)
		{
			_plugin.Call("showMoreApps");
		}
#endif
    }
}
