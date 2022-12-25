using UnityEngine;

public class FacebookLoginButton : MonoBehaviour
{
	private void OnClick()
	{
		EtceteraAndroid.showProgressDialog(string.Empty, "Connecting...");
		SocialManager.instance.FacebookLogin(UIScreenController.Instance.FacebookLogIn);
	}
}
