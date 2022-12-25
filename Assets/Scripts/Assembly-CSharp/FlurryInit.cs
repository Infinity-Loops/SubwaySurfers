using System;
using System.Collections.Generic;
using UnityEngine;

public class FlurryInit : MonoBehaviour
{
	private const string FLURRY_ALLOW_NEW_SESSION = "flurry_allow_new_ss";

	private const int FLURRY_MINUTES_DELAY = 2;

	public const string EVENT_UISCREEN_CHANGED_PREFIX = "UI Screen ";

	public const string EVENT_ANDROID_DEVICE_TOKEN_LENGHT = "Android device token";

	public const string EVENT_10_SOCIAL_ACTIONS_TAKEN = "10 social actions taken";

	public const string EVENT_FIRST_GAMECENTER_LOGIN = "First GameCenter Login";

	public const string EVENT_FIRST_FACEBOOK_LOGIN = "First Facebook Login";

	public const string EVENT_SOCIAL_POKE = "Social friend poked";

	public const string EVENT_SOCIAL_BRAG = "Social bragged";

	public const string EVENT_SOCIAL_BRAGFACEBOOK = "Social bragged Facebook";

	public const string EVENT_MYSTERY_BOX_OPENED = "Mystery Box opened";

	public const string EVENT_INAPPPURCHASE_COMPLETED = "InApp purchase completed";

	public const string EVENT_INAPPPURCHASE_COINPACK1 = "InApp Coin Pack 1 purchased";

	public const string EVENT_INAPPPURCHASE_COINPACK2 = "InApp Coin Pack 2 purchased";

	public const string EVENT_INAPPPURCHASE_COINPACK3 = "InApp Coin Pack 3 purchased";

	public const string EVENT_CHARACTER_UNLOCKED = "Character unlocked";

	public const string EVENT_AUTOMESSAGE_TURNED_OFF = "AutoBrag turned off";

	public const string EVENT_MISSIONSET_COMPLETED = "Mission Set completed";

	public const string EVENT_DAILY_CHALLENGE_COMPLETED = "Daily Challenge completed";

	public const string EVENT_NEWVERSIONPOPUP_RESULT = "New Version Popup Result";

	public const string EVENT_EARN_COINS = "Earn coins item clicked";

	public const string EVENT_BOOST_HEADSTART500_PURCHASED = "Boost Headstart500 purchased";

	public const string EVENT_BOOST_HEADSTART2000_PURCHASED = "Boost Headstart2000 purchased";

	public const string EVENT_BOOST_HOVERBOARD_PURCHASED = "Boost Hoverboard purchased";

	public const string EVENT_BOOST_COINMAGNET_PURCHASED = "Boost Coinmagnet purchased";

	public const string EVENT_BOOST_DOUBLEMULTIPLIER_PURCHASED = "Boost 2x multiplier purchased";

	public const string EVENT_BOOST_JETPACK_PURCHASED = "Boost jetpack purchased";

	public const string EVENT_BOOST_LETTERS_PURCHASED = "Boost letters purchased";

	public const string EVENT_BOOST_SUPERSNEAKERS_PURCHASED = "Boost supersneakers purchased";

	public const string EVENT_BOOST_MYSTERYBOX_PURCHASED = "Boost MysteryBox purchased";

	public const string EVENT_BOOST_MISSION_SKIP_PURCHASED = "Boost Mission Skip purchased";

	public const string EVENT_ARGKEY_ID = "Id";

	public const string EVENT_ARGKEY_TIER = "Tier";

	public const string EVENT_ARGKEY_UI_SCREENNAME = "Screen Name";

	public const string EVENT_ARGKEY_MISSIONSET = "Mission Set";

	public const string EVENT_ARGKEY_MISSIONSET_AND_INDEX = "Mission Set and Index";

	public const string EVENT_ARGKEY_TOTAL = "Total";

	public const string EVENT_ARGKEY_POPUPRESULT = "Result";

	private const string API_KEY = "YR898G65YFPWNMQ6X5H5";

	private void Awake()
	{
		FlurryAndroid.onStartSession("YR898G65YFPWNMQ6X5H5");
	}

	private void OnDestroy()
	{
		FlurryAndroid.onEndSession();
	}

	public static void LogGameCenterLogin()
	{
		if (!PlayerPrefs.HasKey("flurry_has_logged_gc"))
		{
			FlurryAndroid.logEvent("First GameCenter Login");
			PlayerPrefs.SetInt("flurry_has_logged_gc", 1);
		}
	}

	public static void LogFacebookLogin()
	{
		if (!PlayerPrefs.HasKey("flurry_has_logged_fb"))
		{
			FlurryAndroid.logEvent("First Facebook Login");
			PlayerPrefs.SetInt("flurry_has_logged_fb", 1);
		}
	}

	public static void LogGenericSocialAction()
	{
		int @int = PlayerPrefs.GetInt("flurry_social_total", 0);
		int int2 = PlayerPrefs.GetInt("flurry_social_unlogged", 0);
		@int++;
		int2++;
		if (int2 == 10)
		{
			int2 = 0;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Total", @int.ToString());
			FlurryAndroid.logEvent("10 social actions taken", dictionary);
		}
		PlayerPrefs.SetInt("flurry_social_total", @int);
		PlayerPrefs.SetInt("flurry_social_unlogged", int2);
	}

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			PlayerPrefs.SetString("flurry_allow_new_ss", (DateTime.Now + new TimeSpan(0, 2, 0)).Ticks.ToString());
			return;
		}
		DateTime dateTime;
		if (PlayerPrefs.HasKey("flurry_allow_new_ss"))
		{
			string @string = PlayerPrefs.GetString("flurry_allow_new_ss");
			long result;
			if (!long.TryParse(@string, out result))
			{
				result = DateTime.Now.Ticks;
			}
			dateTime = new DateTime(result);
		}
		else
		{
			dateTime = DateTime.Now + new TimeSpan(0, 2, 0);
			PlayerPrefs.SetString("flurry_allow_new_ss", dateTime.Ticks.ToString());
		}
		DateTime now = DateTime.Now;
		bool flag = false;
		if (now >= dateTime)
		{
			flag = true;
		}
		if (flag)
		{
			FlurryAndroid.onEndSession();
			FlurryAndroid.onStartSession("YR898G65YFPWNMQ6X5H5");
		}
	}
}
