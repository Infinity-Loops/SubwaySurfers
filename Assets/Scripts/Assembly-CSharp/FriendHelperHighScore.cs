using UnityEngine;

public class FriendHelperHighScore : MonoBehaviour
{
	private Color _localPlayerColor = new Color(1f / 15f, 0.39607844f, 0.6156863f, 1f);

	private Color _friendColor = new Color(0.043137256f, 0.16862746f, 0.09019608f, 1f);

	public UILabel friendRank;

	public UILabel friendName;

	public UILabel friendScore;

	public UITexture friendPicture;

	public UISlicedSprite friendBackground;

	public Texture2D dummyImage;

	private bool _imageSet;

	private bool _isLocalUser;

	private Friend _friend;

	private bool _initialized;

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
		friendRank.text = ranking.ToString();
		friendRank.color = _localPlayerColor;
		friendName.text = SocialManager.instance.localUserName;
		friendName.color = _localPlayerColor;
		friendScore.text = PlayerInfo.Instance.highestScore.ToString();
		friendScore.color = _localPlayerColor;
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
		_initialized = true;
		_isLocalUser = true;
	}

	public void InitFriend(Friend friend, int ranking, bool backgroundActive = false)
	{
		_friend = friend;
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
		friendScore.text = friend.score.ToString();
		friendPicture.material = new Material(Shader.Find("Unlit/Transparent Colored"));
		if (_friend.image != null)
		{
			friendPicture.material.mainTexture = friend.image;
			_imageSet = true;
		}
		else
		{
			friendPicture.material.mainTexture = dummyImage;
		}
		_initialized = true;
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void OnScoreUpdated()
	{
		friendScore.text = _friend.score.ToString();
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
