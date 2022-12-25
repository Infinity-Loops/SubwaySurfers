using System;
using UnityEngine;

public class FriendGhostHandler : MonoBehaviour
{
    private void Init()
    {
        Game instance = Game.Instance;
        instance.OnGameStarted = (Action)Delegate.Combine(instance.OnGameStarted, new Action(this.NewGame));
        Game instance2 = Game.Instance;
        instance2.OnGameEnded = (Action)Delegate.Combine(instance2.OnGameEnded, new Action(this.GameOver));
        this.inited = true;
    }

    private void OnDestroy()
    {
        Game instance = Game.Instance;
        instance.OnGameStarted = (Action)Delegate.Remove(instance.OnGameStarted, new Action(this.NewGame));
        Game instance2 = Game.Instance;
        instance2.OnGameEnded = (Action)Delegate.Remove(instance2.OnGameEnded, new Action(this.GameOver));
    }

    private void Awake()
    {
        this.Init();
        this.NewGame();
    }

    public void NewGame()
    {
        if (this._gameRunning)
        {
            return;
        }
        this._gameRunning = true;
        this._localUserInserted = false;
        this._currentThreshold = 0;
        GameStats.Instance.ResetScore();
        if (SocialManager.instance.consolidatedFriendsCompleted)
        {
            this.friendsDescending = SocialManager.instance.FriendsSortedByScore();
            if (this.friendsDescending != null)
            {
                this._currentFriend = this.friendsDescending.Length - 1;
            }
        }
        this.helper.NewGame();
        this.SetNewFriend();
        this.helper.AnimateIn();
    }

    private void Update()
    {
        if (this.helper.animatingNow || !this._gameRunning || this.helper.noFriendsLeftToGhost)
        {
            return;
        }
        if (GameStats.Instance.score > this._currentThreshold)
        {
            this.PassThreshold();
        }
    }

    private void PassThreshold()
    {
        this.helper.AnimateOut();
    }

    public void FinishedAnimatingOut()
    {
        if (this._gameRunning)
        {
            this.SetNewFriend();
        }
    }

    public void SetNewFriend()
    {
        Debug.Log("CurrentPlayer: " + this._currentFriend);
        if (!this._gameRunning)
        {
            return;
        }
        bool flag = false;
        if (this.friendsDescending != null || this._currentFriend != -1)
        {
            int num = -1;
            for (int i = this._currentFriend; i >= 0; i--)
            {
                if (this.friendsDescending[i].score > GameStats.Instance.score && this.friendsDescending[i].score > PlayerInfo.Instance.highestScore)
                {
                    num = i;
                    break;
                }
            }
            this._currentFriend = num;
            if (this._currentFriend == -1)
            {
                if (PlayerInfo.Instance.highestScore > GameStats.Instance.score && this.InsertLocalUser())
                {
                    flag = true;
                }
            }
            else if (PlayerInfo.Instance.highestScore < this.friendsDescending[this._currentFriend].score)
            {
                if (this.InsertLocalUser())
                {
                    flag = true;
                }
                else if (this.InsertFriend())
                {
                    flag = true;
                }
            }
            else if (this.InsertFriend())
            {
                flag = true;
            }
        }
        else if (!this._localUserInserted && this.InsertLocalUser())
        {
            flag = true;
        }
        if (flag)
        {
            this.helper.AnimateIn();
        }
        else
        {
            this.helper.NoFriendsLeft();
        }
    }

    public void GameOver()
    {
        this._gameRunning = false;
        this.helper.GameOver();
    }

    private bool InsertLocalUser()
    {
        if (PlayerInfo.Instance.highestScore > GameStats.Instance.score && !this._localUserInserted)
        {
            if (SocialManager.instance.localUserImage != null && SocialManager.instance.consolidatedFriendsCompleted)
            {
                this.helper.picture.material.mainTexture = SocialManager.instance.localUserImage;
            }
            else
            {
                this.helper.picture.material.mainTexture = this.dummyImage;
            }
            this.helper.points.text = PlayerInfo.Instance.highestScore.ToString();
            this.helper.points.color = this.localPlayerColor;
            this._localUserInserted = true;
            this._currentThreshold = PlayerInfo.Instance.highestScore;
            Debug.Log("Inserted local player: " + PlayerInfo.Instance.highestScore);
            return true;
        }
        return false;
    }

    private bool InsertFriend()
    {
        int currentFriend = this._currentFriend;
        if (currentFriend < 0 || currentFriend >= this.friendsDescending.Length)
        {
            Debug.LogError("Tried to insert a friend outside the array");
            return false;
        }
        Missions.Instance.PlayerDidThis(Missions.MissionTarget.BeatFriends, 1);
        Debug.Log("Inserted friend: " + currentFriend);
        Friend friend = this.friendsDescending[currentFriend];
        if (friend.score > GameStats.Instance.score)
        {
            if (friend.image != null)
            {
                this.helper.picture.material.mainTexture = friend.image;
            }
            else
            {
                this.helper.picture.material.mainTexture = this.dummyImage;
            }
            this.helper.points.text = friend.score.ToString();
            this.helper.points.color = this.friendColor;
            this._currentThreshold = friend.score;
            return true;
        }
        return false;
    }

    public FriendGhostHelper helper;

    public Texture dummyImage;

    private Friend[] friendsDescending;

    private bool _localUserInserted;

    private bool inited;

    private bool _gameRunning;

    private int _currentThreshold;

    private int _currentFriend = -1;

    private Color localPlayerColor = new Color(1f, 0.858823538f, 0f, 1f);

    private Color friendColor = Color.white;
}
