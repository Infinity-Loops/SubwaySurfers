using UnityEngine;

public class FriendHelperBrag : MonoBehaviour
{
	private Color localPlayerColor = new Color(1f / 15f, 0.39607844f, 0.6156863f, 1f);

	public UILabel friendRank;

	public UILabel friendName;

	public UILabel friendScore;

	public UITexture friendPicture;

	public UISlicedSprite friendBackground;

	public UICheckbox bragCheckbox;

	private bool bragActive;

	public Texture2D dummyImage;

	private bool _imageSet;

	private bool _isLocalUser;

	private Friend _friend;

	private bool _initialized;

	private bool _braggable;

	private FriendHandlerBrag _bragHandler;

	private void Start()
	{
		_bragHandler = NGUITools.FindInParents<FriendHandlerBrag>(base.gameObject);
	}

	public void InitLocalUser(int ranking, bool backgroundActive = false)
	{
		if (!backgroundActive)
		{
			friendBackground.alpha = 0f;
		}
		else
		{
			friendBackground.alpha = 0.2f;
		}
		_braggable = false;
		friendRank.text = ranking.ToString();
		friendRank.color = localPlayerColor;
		friendName.text = SocialManager.instance.localUserName;
		friendName.color = localPlayerColor;
		friendScore.text = PlayerInfo.Instance.highestScore.ToString();
		friendScore.color = localPlayerColor;
		friendPicture.material = new Material(Shader.Find("Unlit/Transparent Colored"));
		if (SocialManager.instance.localUserImage != null)
		{
			friendPicture.material.mainTexture = SocialManager.instance.localUserImage;
			_imageSet = true;
		}
		else
		{
			friendPicture.material.mainTexture = dummyImage;
		}
		if (bragCheckbox != null)
		{
			NGUITools.SetActive(bragCheckbox.gameObject, false);
			Object.Destroy(bragCheckbox.gameObject);
		}
		_initialized = true;
		_isLocalUser = true;
	}

	public void InitFriend(Friend friend, int ranking, bool braggable = false, bool backgroundActive = false)
	{
		_friend = friend;
		_braggable = braggable;
		if (!backgroundActive)
		{
			friendBackground.alpha = 0f;
		}
		else
		{
			friendBackground.alpha = 0.2f;
		}
		friendRank.text = ranking.ToString();
		friendName.text = _friend.name;
		friendScore.text = _friend.score.ToString();
		friendPicture.material = new Material(Shader.Find("Unlit/Transparent Colored"));
		if (_friend.image != null)
		{
			friendPicture.material.mainTexture = _friend.image;
			_imageSet = true;
		}
		else
		{
			friendPicture.material.mainTexture = dummyImage;
		}
		if (bragCheckbox != null)
		{
			if (_braggable)
			{
				NGUITools.AddWidgetCollider(base.gameObject);
				NGUITools.SetActive(bragCheckbox.gameObject, true);
				bragCheckbox.isChecked = true;
				bragActive = true;
			}
			else
			{
				NGUITools.SetActive(bragCheckbox.gameObject, false);
			}
		}
		_initialized = true;
	}

	private void OnClick()
	{
		if (_braggable)
		{
			if (bragActive)
			{
				bragActive = false;
				_bragHandler.RemoveBragFriend(_friend);
				bragCheckbox.isChecked = false;
			}
			else
			{
				bragActive = true;
				_bragHandler.AddBragFriend(_friend);
				bragCheckbox.isChecked = true;
			}
		}
	}

	private void CompletedBragging()
	{
		if (_braggable && base.gameObject.GetComponent<Collider>() != null)
		{
			Object.Destroy(base.gameObject.GetComponent<Collider>());
		}
	}

	private void Update()
	{
		if (!_initialized || _imageSet)
		{
			return;
		}
		if (_isLocalUser)
		{
			if (SocialManager.instance.localUserImage != null)
			{
				friendPicture.material.mainTexture = SocialManager.instance.localUserImage;
				_imageSet = true;
			}
		}
		else if (_friend.image != null)
		{
			friendPicture.material.mainTexture = _friend.image;
			_imageSet = true;
		}
	}
}
