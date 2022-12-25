using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Configurator
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void Run()
    {
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
        GameObject SaveManager = new GameObject("Save Manager");
        SaveManager.AddComponent<SaveManager>();
    }
}
public class SaveManager : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
    private void OnApplicationFocus(bool focus)
    {
        PlayerInfo.Instance.Save();
    }
    private void OnApplicationPause(bool pause)
    {
        PlayerInfo.Instance.Save();
    }
    private void OnApplicationQuit()
    {
        PlayerInfo.Instance.Save();
    }
}