using System;
using System.Collections.Generic;
using UnityEngine;

public class FlurryAndroid
{
    private static AndroidJavaClass _flurryAgent;

    private static AndroidJavaObject _plugin;

    static FlurryAndroid()
    {
#if ENABLE_FLURRY
		if (Application.platform != RuntimePlatform.Android)
		{
			return;
		}
		_flurryAgent = new AndroidJavaClass("com.flurry.android.FlurryAgent");
		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.prime31.FlurryPlugin"))
		{
			_plugin = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
		}
#endif
    }

    public static string getAndroidId()
    {
#if ENABLE_FLURRY
		if (Application.platform != RuntimePlatform.Android)
		{
			return string.Empty;
		}
		return _plugin.Call<string>("getAndroidId", new object[0]);
#else
        return String.Empty;
#endif
    }

    public static void onStartSession(string apiKey)
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _plugin.Call("onStartSession", apiKey);
        }
#endif
    }

    public static void onEndSession()
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _plugin.Call("onEndSession");
        }
#endif
    }

    public static void setContinueSessionMillis(long milliseconds)
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _flurryAgent.CallStatic("setContinueSessionMillis", milliseconds);
        }
#endif
    }

    public static void logEvent(string eventName)
    {
#if ENABLE_FLURRY
        logEvent(eventName, false);
#endif
    }

    public static void logEvent(string eventName, bool isTimed)
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            if (isTimed)
            {
                _plugin.Call("logTimedEvent", eventName);
            }
            else
            {
                _plugin.Call("logEvent", eventName);
            }
        }
#endif
    }

    public static void logEvent(string eventName, Dictionary<string, string> parameters)
    {
#if ENABLE_FLURRY
        logEvent(eventName, parameters, false);
#endif
    }

    public static void logEvent(string eventName, Dictionary<string, string> parameters, bool isTimed)
    {
#if ENABLE_FLURRY
        if (Application.platform != RuntimePlatform.Android)
        {
            return;
        }
        using (AndroidJavaObject androidJavaObject = new AndroidJavaObject("java.util.HashMap"))
        {
            IntPtr methodID = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
            object[] array = new object[2];
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                using (AndroidJavaObject androidJavaObject2 = new AndroidJavaObject("java.lang.String", parameter.Key))
                {
                    using (AndroidJavaObject androidJavaObject3 = new AndroidJavaObject("java.lang.String", parameter.Value))
                    {
                        array[0] = androidJavaObject2;
                        array[1] = androidJavaObject3;
                        AndroidJNI.CallObjectMethod(androidJavaObject.GetRawObject(), methodID, AndroidJNIHelper.CreateJNIArgArray(array));
                    }
                }
            }
            if (isTimed)
            {
                _plugin.Call("logTimedEventWithParams", eventName, androidJavaObject);
            }
            else
            {
                _plugin.Call("logEventWithParams", eventName, androidJavaObject);
            }
        }
#endif
    }

    public static void endTimedEvent(string eventName)
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _plugin.Call("endTimedEvent", eventName);
        }
#endif
    }

    public static void onPageView()
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _flurryAgent.CallStatic("onPageView");
        }
#endif
    }

    public static void onError(string errorId, string message, string errorClass)
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _flurryAgent.CallStatic("onError", errorId, message, errorClass);
        }
#endif
    }

    public static void setUserID(string userId)
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _flurryAgent.CallStatic("setUserId", userId);
        }
#endif
    }

    public static void setAge(int age)
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _flurryAgent.CallStatic("setAge", age);
        }
#endif
    }

    public static void setLogEnabled(bool enable)
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _flurryAgent.CallStatic("setLogEnabled", enable);
        }
#endif
    }

    public static void enableAppCircle(string intentName)
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _plugin.Call("enableAppCircle", intentName);
        }
#endif
    }

    public static float getScreenDensity()
    {
#if ENABLE_FLURRY
        if (Application.platform != RuntimePlatform.Android)
        {
            return 1f;
        }
        return _plugin.Call<float>("getScreenDensity", new object[0]);
#else
        return 1f;
#endif
    }

    public static void showBanner(string hookname, FlurryAdPlacement placement)
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _plugin.Call("showBanner", hookname, (int)placement);
        }
#endif
    }

    public static void destroyBanner()
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _plugin.Call("destroyBanner");
        }
#endif
    }

    public static void addUserCookie(string key, string value)
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _plugin.Call("addUserCookie", key, value);
        }
#endif
    }

    public static void clearUserCookies()
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _plugin.Call("clearUserCookies");
        }
#endif
    }

    public static void openCatalog(string hookname)
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _plugin.Call("openCatalog", hookname);
        }
#endif
    }

    public static void launchCatalogOnBannerClicked(bool shouldLaunch)
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _plugin.Call("launchCatalogOnBannerClicked", shouldLaunch);
        }
#endif
    }

    public static void setDefaultNoAdsMessage(string msg)
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _plugin.Call("setDefaultNoAdsMessage", msg);
        }
#endif
    }

    public static FlurryOffer getOffer(string hookname)
    {
#if ENABLE_FLURRY
        if (Application.platform != RuntimePlatform.Android)
        {
            return null;
        }
        string json = _plugin.Call<string>("getOffer", new object[1] { hookname });
        List<FlurryOffer> list = FlurryOffer.fromJSON(json);
        if (list.Count > 0)
        {
            return list[0];
        }
        return null;
#else
        return null;
#endif
    }

    public static List<FlurryOffer> getAllOffers(string hookname)
    {
#if ENABLE_FLURRY
        if (Application.platform != RuntimePlatform.Android)
        {
            return null;
        }
        string json = _plugin.Call<string>("getAllOffers", new object[1] { hookname });
        return FlurryOffer.fromJSON(json);
#else
        return null;
#endif
    }

    public static void acceptOffer(long offerId)
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _plugin.Call("acceptOffer", offerId);
        }
#endif
    }

    public static void removeOffer(long offerId)
    {
#if ENABLE_FLURRY
        if (Application.platform == RuntimePlatform.Android)
        {
            _plugin.Call("removeOffer", offerId);
        }
#endif
    }
}
