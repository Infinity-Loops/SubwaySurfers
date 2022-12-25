using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadLevelCtrl : MonoBehaviour
{
    private void Awake()
    {
        Object.DontDestroyOnLoad(base.transform.gameObject);
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1.30f);
        yield return SceneManager.LoadSceneAsync("Merge");
        Debug.Log("Merge Level Loaded " + Time.frameCount);
        yield return SceneManager.LoadSceneAsync("LazyLoad", LoadSceneMode.Additive);
        Debug.Log("Chunks Level Loaded " + Time.frameCount);
    }
}
