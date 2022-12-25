using UnityEngine;

public class FriendHandlerCrew : MonoBehaviour
{
	public GameObject FriendPrefab;

	public GameObject InvitePrefab;

	public GameObject FacebookLoginPrefab;

	public GameObject FacebookLoginNoBonusPrefab;

	public GameObject GameCenterLoginPrefab;

	public GameObject GameCenterLoginNoBonusPrefab;

	public UILabel CrewHeader;

	public UILabel NoFriends;

	private UIGrid _grid;

	private void Awake()
	{
		_grid = GetComponent<UIGrid>();
	}

	public void InitCrew()
	{
		if (_grid == null)
		{
			_grid = GetComponent<UIGrid>();
		}
		foreach (Transform item in base.transform)
		{
			NGUITools.SetActive(item.gameObject, false);
			Object.Destroy(item.gameObject);
		}
		Friend[] array = SocialManager.instance.FriendsSortedByCash();
		Debug.Log("number of friends: " + array.Length);
		bool flag = false;
		bool flag2 = false;
		int num = -1;
		if (!flag && (bool)FacebookLoginPrefab && (bool)FacebookLoginNoBonusPrefab && !SocialManager.instance.facebookIsLoggedIn)
		{
			GameObject gameObject = ((!PlayerInfo.Instance.hasPayedOutFacebook) ? NGUITools.AddChild(base.gameObject, FacebookLoginPrefab) : NGUITools.AddChild(base.gameObject, FacebookLoginNoBonusPrefab));
			gameObject.name = string.Format("{0:000}fb", num);
			flag = true;
		}
		if (!flag2 && !Social.localUser.authenticated && (bool)GameCenterLoginPrefab && (bool)GameCenterLoginNoBonusPrefab)
		{
			GameObject gameObject2 = ((!PlayerInfo.Instance.hasPayedOutGameCenter) ? NGUITools.AddChild(base.gameObject, GameCenterLoginPrefab) : NGUITools.AddChild(base.gameObject, GameCenterLoginNoBonusPrefab));
			gameObject2.name = string.Format("{0:000}gc", num);
			flag2 = true;
		}
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject3 = NGUITools.AddChild(base.gameObject, FriendPrefab);
			gameObject3.name = string.Format("{0:000000}", i);
			FriendHelperCrew component = gameObject3.GetComponent<FriendHelperCrew>();
			component.InitFriend(array[i], i % 2 == 0);
			num = i;
		}
		if (SocialManager.instance.facebookIsLoggedIn)
		{
			GameObject gameObject4 = NGUITools.AddChild(base.gameObject, InvitePrefab);
			gameObject4.name = "invite";
		}
		if (num == -1)
		{
			NoFriends.alpha = 1f;
			NoFriends.gameObject.active = true;
		}
		else
		{
			NoFriends.alpha = 0f;
			NoFriends.gameObject.active = false;
		}
		CrewHeader.text = "Friends (" + (num + 1) + ")";
		_grid.sorted = false;
		_grid.SendMessage("Start", SendMessageOptions.DontRequireReceiver);
		base.gameObject.BroadcastMessage("CreatePanel", SendMessageOptions.DontRequireReceiver);
	}
}
