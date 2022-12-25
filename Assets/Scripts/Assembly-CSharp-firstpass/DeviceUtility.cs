using UnityEngine;

public class DeviceUtility
{
	public static int GetVersionCode()
	{
		return 1;
	}

	public static string GetVersionName()
	{
		return Application.version;
    }
    public static string GetBundleVersion()
    {
		return "n/a";
    }
}
