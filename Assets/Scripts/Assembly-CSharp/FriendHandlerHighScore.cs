using System.Collections;
using UnityEngine;

public class FriendHandlerHighScore : MonoBehaviour
{
	public GameObject FriendPrefab;

	public GameObject FacebookLoginPrefab;

	public GameObject FacebookLoginNoBonusPrefab;

	public GameObject GameCenterLoginPrefab;

	public GameObject GameCenterLoginNoBonusPrefab;

	private Color myColor = new Color(1f / 15f, 0.39607844f, 0.6156863f, 1f);

	private UIGrid _grid;

	private Vector4 defaultPanelClipping = new Vector4(0f, 240f, 292f, 300f);

	private void Awake()
	{
		_grid = GetComponent<UIGrid>();
	}

	public void LoadHighScore()
	{
		LoadFriends();
	}

	private void LoadFriends()
	{
		base.transform.parent.GetComponent<UIPanel>().widgetsAreStatic = false;
		if (_grid == null)
		{
			_grid = GetComponent<UIGrid>();
		}
		foreach (Transform item in base.transform)
		{
			NGUITools.SetActive(item.gameObject, false);
			Object.Destroy(item.gameObject);
		}
		Friend[] array = SocialManager.instance.FriendsSortedByScore();
		Transform transform2 = base.transform;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		int num = 1;
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject = NGUITools.AddChild(base.gameObject, FriendPrefab);
			gameObject.name = string.Format("{0:000}", num);
			FriendHelperHighScore component = gameObject.GetComponent<FriendHelperHighScore>();
			if (!flag && PlayerInfo.Instance.highestScore >= array[i].score)
			{
				component.InitLocalUser(num, num % 2 == 1);
				num++;
				flag = true;
				if (i == 0)
				{
					transform2 = gameObject.transform;
				}
				gameObject = null;
				component = null;
				if (!flag2 && (bool)FacebookLoginPrefab && (bool)FacebookLoginNoBonusPrefab && !SocialManager.instance.facebookIsLoggedIn)
				{
					GameObject gameObject2 = ((!PlayerInfo.Instance.hasPayedOutFacebook) ? NGUITools.AddChild(base.gameObject, FacebookLoginPrefab) : NGUITools.AddChild(base.gameObject, FacebookLoginNoBonusPrefab));
					gameObject2.name = string.Format("{0:000}fb", num);
					flag2 = true;
				}
				if (!flag3 && !Social.localUser.authenticated && (bool)GameCenterLoginPrefab && (bool)GameCenterLoginNoBonusPrefab)
				{
					GameObject gameObject3 = ((!PlayerInfo.Instance.hasPayedOutGameCenter) ? NGUITools.AddChild(base.gameObject, GameCenterLoginPrefab) : NGUITools.AddChild(base.gameObject, GameCenterLoginNoBonusPrefab));
					gameObject3.name = string.Format("{0:000}gc", num);
					flag3 = true;
				}
				gameObject = NGUITools.AddChild(base.gameObject, FriendPrefab);
				component = gameObject.GetComponent<FriendHelperHighScore>();
				gameObject.name = string.Format("{0:000}", num);
			}
			if (!flag)
			{
				transform2 = gameObject.transform;
			}
			component.InitFriend(array[i], num, num % 2 == 1);
			num++;
		}
		if (!flag)
		{
			GameObject gameObject4 = NGUITools.AddChild(base.gameObject, FriendPrefab);
			gameObject4.name = string.Format("{0:000}", num);
			FriendHelperHighScore component2 = gameObject4.GetComponent<FriendHelperHighScore>();
			transform2 = gameObject4.transform;
			component2.InitLocalUser(num, num % 2 == 1);
			num++;
			flag = true;
			if (!flag2 && (bool)FacebookLoginPrefab && (bool)FacebookLoginNoBonusPrefab && !SocialManager.instance.facebookIsLoggedIn)
			{
				GameObject gameObject5 = ((!PlayerInfo.Instance.hasPayedOutFacebook) ? NGUITools.AddChild(base.gameObject, FacebookLoginPrefab) : NGUITools.AddChild(base.gameObject, FacebookLoginNoBonusPrefab));
				gameObject5.name = string.Format("{0:000}fb", num);
				flag2 = true;
			}
			if (!flag3 && !Social.localUser.authenticated && (bool)GameCenterLoginPrefab && (bool)GameCenterLoginNoBonusPrefab)
			{
				GameObject gameObject6 = ((!PlayerInfo.Instance.hasPayedOutGameCenter) ? NGUITools.AddChild(base.gameObject, GameCenterLoginPrefab) : NGUITools.AddChild(base.gameObject, GameCenterLoginNoBonusPrefab));
				gameObject6.name = string.Format("{0:000}gc", num);
				flag3 = true;
			}
		}
		UIPanel component3 = _grid.transform.parent.GetComponent<UIPanel>();
		Vector3 localPosition = _grid.transform.parent.localPosition;
		component3.clipRange = defaultPanelClipping;
		_grid.sorted = false;
		_grid.SendMessage("Start", SendMessageOptions.DontRequireReceiver);
		component3.transform.localPosition = new Vector3(localPosition.x, 0f - transform2.localPosition.y, localPosition.z);
		component3.clipRange = new Vector4(defaultPanelClipping.x, defaultPanelClipping.y + transform2.localPosition.y, defaultPanelClipping.z * 1f, defaultPanelClipping.w);
		component3.GetComponent<UIDraggablePanel>().RestrictWithinBounds(true);
		base.gameObject.BroadcastMessage("CreatePanel", SendMessageOptions.DontRequireReceiver);
		StartCoroutine(SetStatic());
	}

	private IEnumerator SetStatic()
	{
		yield return null;
		base.transform.parent.GetComponent<UIPanel>().widgetsAreStatic = true;
	}
}
