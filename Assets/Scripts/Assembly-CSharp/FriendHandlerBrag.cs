using System;
using System.Collections.Generic;
using UnityEngine;

public class FriendHandlerBrag : MonoBehaviour
{
	public enum LogInState
	{
		_notset = 0,
		Both = 1,
		Facebook = 2,
		GameCenter = 3
	}

	public GameObject friendBragPrefab;

	public GameObject friendNoBragPrefab;

	public GameObject facebookLoginPrefab;

	public GameObject facebookLoginNoBonusPrefab;

	public GameObject gameCenterLoginPrefab;

	public GameObject gameCenterLoginNoBonusPrefab;

	public BragButtonHelper bragButton;

	public GameObject facebookBonus;

	public GameObject gameCenterBonus;

	public GameObject OfflineParent;

	public GameObject OnlineFacebookParent;

	public GameObject OnlineGameCenterParent;

	public GameObject OnlineBothBragParent;

	public GameObject OnlineBothNoBragParent;

	public UIGrid OnlineFacebookGrid;

	public UIGrid OnlineGameCenterGrid;

	public UIGrid OnlineBothBragGrid;

	public UIGrid OnlineBothNoBragGrid;

	public GameObject FacebookLoginButtonParent;

	public GameObject GameCenterLoginButtonParent;

	public UILabel gettingLabel;

	private FriendHelperBrag playerHelper;

	private Color myColor = new Color(1f / 15f, 0.39607844f, 0.6156863f, 1f);

	private UIGrid _currentGrid;

	private List<Friend> _bragList = new List<Friend>();

	private Vector4 defaultPanelClipping = new Vector4(0f, 142f, 292.5f, 109f);

	private Vector4 defaultPanelClipping4Elements = new Vector4(0f, 160f, 292.5f, 145f);

	[NonSerialized]
	public bool bragNotifyDone;

	[NonSerialized]
	public bool bragFacebookDone;

	[NonSerialized]
	public string preBragPopupString = string.Empty;

	private bool haveShownPreBragPopupThisRun;

	private static FriendHandlerBrag _instance;

	public List<Friend> bragList
	{
		get
		{
			if (_bragList != null)
			{
				return _bragList;
			}
			return null;
		}
	}

	public static FriendHandlerBrag instance
	{
		get
		{
			if (_instance == null)
			{
				Debug.LogError("FriendHandlerBrag _instance was null when you tried to access it!");
			}
			return _instance;
		}
	}

	private void ClearAllGrids()
	{
		foreach (Transform item in OnlineBothBragGrid.transform)
		{
			NGUITools.SetActive(item.gameObject, false);
			UnityEngine.Object.Destroy(item.gameObject);
		}
		foreach (Transform item2 in OnlineBothNoBragGrid.transform)
		{
			NGUITools.SetActive(item2.gameObject, false);
			UnityEngine.Object.Destroy(item2.gameObject);
		}
		foreach (Transform item3 in OnlineFacebookGrid.transform)
		{
			NGUITools.SetActive(item3.gameObject, false);
			UnityEngine.Object.Destroy(item3.gameObject);
		}
		foreach (Transform item4 in OnlineGameCenterGrid.transform)
		{
			NGUITools.SetActive(item4.gameObject, false);
			UnityEngine.Object.Destroy(item4.gameObject);
		}
	}

	public void ShowGettingReadyLabel()
	{
		ClearEverything();
		NGUITools.SetActive(gettingLabel.gameObject, true);
		haveShownPreBragPopupThisRun = false;
	}

	private void ClearEverything()
	{
		ClearAllGrids();
		NGUITools.SetActive(OnlineBothBragParent, false);
		NGUITools.SetActive(OnlineBothNoBragParent, false);
		NGUITools.SetActive(OnlineFacebookParent, false);
		NGUITools.SetActive(OnlineGameCenterParent, false);
		NGUITools.SetActive(OfflineParent, false);
		NGUITools.SetActive(gettingLabel.gameObject, false);
		foreach (Transform item in FacebookLoginButtonParent.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		foreach (Transform item2 in GameCenterLoginButtonParent.transform)
		{
			UnityEngine.Object.Destroy(item2.gameObject);
		}
	}

	public void GoOffline(bool enableButtons)
	{
		ClearEverything();
		NGUITools.SetActive(OfflineParent, true);
		OfflineParent.BroadcastMessage("CreatePanel", SendMessageOptions.DontRequireReceiver);
		if (enableButtons)
		{
			OfflineParent.GetComponent<GameOverOfflineHelper>().EnableButtons();
		}
		else
		{
			OfflineParent.GetComponent<GameOverOfflineHelper>().DisableButtons();
		}
		if (PlayerInfo.Instance.hasPayedOutFacebook)
		{
			NGUITools.SetActive(facebookBonus, false);
		}
		if (PlayerInfo.Instance.hasPayedOutGameCenter)
		{
			NGUITools.SetActive(gameCenterBonus, false);
		}
	}

	public void ShowFriendList()
	{
		Transform parent = base.transform.parent;
		ClearEverything();
		Friend[] array = SocialManager.instance.FriendsSortedByScore();
		LogInState logInState = LogInState._notset;
		if (Social.localUser.authenticated)
		{
			logInState = LogInState.GameCenter;
		}
		if (SocialManager.instance.facebookIsLoggedIn)
		{
			logInState = LogInState.Both;
		}
		switch (logInState)
		{
		case LogInState.Both:
		{
			bool flag = false;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].score <= PlayerInfo.Instance.highestScore && array[i].score > PlayerInfo.Instance.oldHighestScore)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				_currentGrid = OnlineBothBragGrid;
				NGUITools.SetActive(OnlineBothBragParent, true);
			}
			else
			{
				_currentGrid = OnlineBothNoBragGrid;
				NGUITools.SetActive(OnlineBothNoBragParent, true);
			}
			break;
		}
		case LogInState.GameCenter:
			_currentGrid = OnlineGameCenterGrid;
			NGUITools.SetActive(OnlineGameCenterParent, true);
			if ((bool)facebookLoginPrefab && (bool)facebookLoginNoBonusPrefab && !SocialManager.instance.facebookIsLoggedIn)
			{
				GameObject gameObject = ((!PlayerInfo.Instance.hasPayedOutFacebook) ? NGUITools.AddChild(FacebookLoginButtonParent, facebookLoginPrefab) : NGUITools.AddChild(FacebookLoginButtonParent, facebookLoginNoBonusPrefab));
				gameObject.name = "facebookLoginButton";
			}
			break;
		case LogInState.Facebook:
			_currentGrid = OnlineFacebookGrid;
			NGUITools.SetActive(OnlineFacebookParent, true);
			break;
		default:
			Debug.LogError("Tried to show friends while offline.", this);
			return;
		}
		bool flag2 = false;
		int num = 1;
		GameObject prefab = friendNoBragPrefab;
		if (_currentGrid == OnlineBothBragGrid)
		{
			prefab = friendBragPrefab;
		}
		for (int j = 0; j < array.Length; j++)
		{
			GameObject gameObject2 = NGUITools.AddChild(_currentGrid.gameObject, prefab);
			gameObject2.name = string.Format("{0:000}", num);
			FriendHelperBrag component = gameObject2.GetComponent<FriendHelperBrag>();
			if (!flag2 && PlayerInfo.Instance.highestScore >= array[j].score)
			{
				if (j == 0)
				{
					parent = gameObject2.transform;
				}
				component.InitLocalUser(num, num % 2 == 1);
				num++;
				flag2 = true;
				playerHelper = component;
				gameObject2 = NGUITools.AddChild(_currentGrid.gameObject, prefab);
				gameObject2.name = string.Format("{0:000}", num);
				component = gameObject2.GetComponent<FriendHelperBrag>();
			}
			bool flag3 = _currentGrid == OnlineBothBragGrid && array[j].score <= PlayerInfo.Instance.highestScore && array[j].score > PlayerInfo.Instance.oldHighestScore && (bool)_currentGrid;
			component.InitFriend(array[j], num, flag3, num % 2 == 1);
			if (!flag2)
			{
				parent = gameObject2.transform;
			}
			if (flag3 && !_bragList.Contains(array[j]))
			{
				AddBragFriend(array[j]);
			}
			num++;
		}
		if (!flag2)
		{
			GameObject gameObject3 = NGUITools.AddChild(_currentGrid.gameObject, prefab);
			gameObject3.name = string.Format("{0:000}", num);
			FriendHelperBrag friendHelperBrag = (playerHelper = gameObject3.GetComponent<FriendHelperBrag>());
			parent = gameObject3.transform;
			friendHelperBrag.InitLocalUser(num, num % 2 == 1);
			num++;
			flag2 = true;
		}
		if (bragList.Count == 0)
		{
			bragButton.DisableButton();
		}
		else
		{
			bragButton.EnableButton();
		}
		if (_currentGrid == OnlineBothNoBragGrid)
		{
			Debug.Log("Should show 4 players now.");
			UIPanel component2 = _currentGrid.transform.parent.GetComponent<UIPanel>();
			Vector3 zero = Vector3.zero;
			component2.transform.localPosition = zero;
			Vector3 vector = zero;
			component2.clipRange = defaultPanelClipping4Elements;
			_currentGrid.sorted = false;
			_currentGrid.gameObject.SendMessage("Start");
			component2.transform.localPosition = new Vector3(vector.x, 0f - parent.localPosition.y, vector.z);
			component2.clipRange = new Vector4(defaultPanelClipping4Elements.x, defaultPanelClipping4Elements.y + parent.localPosition.y, defaultPanelClipping4Elements.z, defaultPanelClipping4Elements.w);
			component2.GetComponent<UIDraggablePanel>().RestrictWithinBounds(true);
			_currentGrid.gameObject.BroadcastMessage("CreatePanel", SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			UIPanel component3 = _currentGrid.transform.parent.GetComponent<UIPanel>();
			Vector3 zero2 = Vector3.zero;
			component3.transform.localPosition = zero2;
			Vector3 vector2 = zero2;
			component3.clipRange = defaultPanelClipping;
			_currentGrid.sorted = false;
			_currentGrid.gameObject.SendMessage("Start");
			component3.transform.localPosition = new Vector3(vector2.x, 0f - parent.localPosition.y, vector2.z);
			component3.clipRange = new Vector4(defaultPanelClipping.x, defaultPanelClipping.y + parent.localPosition.y, defaultPanelClipping.z, defaultPanelClipping.w);
			component3.GetComponent<UIDraggablePanel>().RestrictWithinBounds(true);
			_currentGrid.gameObject.BroadcastMessage("CreatePanel", SendMessageOptions.DontRequireReceiver);
		}
		bragNotifyDone = false;
		bragFacebookDone = false;
		if (_currentGrid == OnlineBothBragGrid)
		{
			if (Settings.optionAutoMessage)
			{
				SocialManager.instance.BragNotify(PlayerInfo.Instance.highestScore, bragList);
				bragNotifyDone = true;
			}
			preBragPopupString = "You beat";
			int num2 = 0;
			string text;
			if (_bragList.Count == 1)
			{
				preBragPopupString = preBragPopupString + " " + _bragList[0].name;
			}
			else if (_bragList.Count == 2)
			{
				text = preBragPopupString;
				preBragPopupString = text + " " + _bragList[0].name + " and " + _bragList[1].name;
			}
			else
			{
				int num3 = bragList.Count - 1;
				text = preBragPopupString;
				preBragPopupString = text + " " + _bragList[0].name + " and " + num3 + " other friend" + ((num3 <= 1) ? string.Empty : "s");
			}
			text = preBragPopupString;
			preBragPopupString = text + " with a score of " + PlayerInfo.Instance.highestScore + "!";
			if (!haveShownPreBragPopupThisRun)
			{
				UIScreenController.Instance.QueuePopup("PreBragPopup");
				haveShownPreBragPopupThisRun = true;
			}
		}
	}

	public void AddBragFriend(Friend friend)
	{
		if (!_bragList.Contains(friend))
		{
			_bragList.Add(friend);
			if (!bragButton.buttonEnabled)
			{
				bragButton.EnableButton();
			}
		}
	}

	public void RemoveBragFriend(Friend friend)
	{
		if (_bragList.Contains(friend))
		{
			_bragList.Remove(friend);
			if (_bragList.Count == 0)
			{
				bragButton.DisableButton();
			}
		}
	}

	public void CompletedBrag()
	{
		bragButton.DisableButton();
		_currentGrid.gameObject.BroadcastMessage("CompletedBragging", SendMessageOptions.DontRequireReceiver);
		_bragList.Clear();
		PlayerInfo.Instance.BragCompleted();
		ShowFriendList();
	}

	private void MovedAwayFromGameOverScreenClicked()
	{
		Debug.Log("Moving away from gameOverScreen");
		PlayerInfo.Instance.BragCompleted();
		_bragList.Clear();
	}

	private void OnDisable()
	{
		Debug.Log("Disable called on gameover screen");
		MovedAwayFromGameOverScreenClicked();
	}

	private void Awake()
	{
		_instance = this;
	}
}
