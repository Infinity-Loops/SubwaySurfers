using System;
using UnityEngine;

public class UIIngameUpdater : IgnoreTimeScale
{
    public static bool isCountingDown()
    {
        return UIIngameUpdater.countingDown;
    }

    public void Awake()
    {
        PlayerInfo instance = PlayerInfo.Instance;
        instance.onScoreMultiplierChanged = (Action)Delegate.Combine(instance.onScoreMultiplierChanged, new Action(this.readMultiplier));
        GameStats instance2 = GameStats.Instance;
        instance2.OnCoinsChanged = (Action)Delegate.Combine(instance2.OnCoinsChanged, new Action(this.OnCoinsChanged));
        Game instance3 = Game.Instance;
        instance3.OnGameStarted = (Action)Delegate.Combine(instance3.OnGameStarted, new Action(this.OnGameStarted));
        this.readMultiplier();
        this.scoreLabel.text = string.Empty + GameStats.Instance.score;
        this._cachedScoreBGTransform = this.scoreBG.cachedTransform;
        this._cachedCoinBGTransform = this.coinBG.cachedTransform;
        this.countdownStartingLabel.text = string.Empty;
        this.countdownLabel.text = string.Empty;
    }

    public void OnDestroy()
    {
        PlayerInfo instance = PlayerInfo.Instance;
        instance.onScoreMultiplierChanged = (Action)Delegate.Remove(instance.onScoreMultiplierChanged, new Action(this.readMultiplier));
        GameStats instance2 = GameStats.Instance;
        instance2.OnCoinsChanged = (Action)Delegate.Remove(instance2.OnCoinsChanged, new Action(this.OnCoinsChanged));
        Game instance3 = Game.Instance;
        instance3.OnGameStarted = (Action)Delegate.Remove(instance3.OnGameStarted, new Action(this.OnGameStarted));
    }

    private void OnDisable()
    {
        UIIngameUpdater.countingDown = false;
        this.countdownStartingLabel.text = string.Empty;
        this.countdownLabel.text = string.Empty;
    }

    public void TriggerInGameUI()
    {
        if (Game.Instance != null)
        {
            if (Game.Instance.isPaused)
            {
                this.countdownSeconds = 3f;
                UIIngameUpdater.countingDown = true;
            }
        }
        else
        {
            Debug.LogError("You must be running the GUI scene");
        }
    }

    private void readMultiplier()
    {
        this.multiplierLabel.text = "x" + PlayerInfo.Instance.scoreMultiplier;
    }

    private void Update()
    {
        if (Game.Instance.isReadyForHeadStart && !Game.Instance.track.IsRunningOnTutorialTrack)
        {
            Game.Instance.isReadyForHeadStart = false;
            this.headstartHelper.ShowHeadStart();
        }
        GameStats.Instance.CalculateScore();
        if (this.score != GameStats.Instance.score)
        {
            this.SetScoreLabel();
        }
        if (UIIngameUpdater.countingDown)
        {
            float num = base.UpdateRealTimeDelta();
            num *= 1.75f;
            this.countdownSeconds -= num;
            this.countdownStartingLabel.text = "Starting in";
            this.countdownLabel.text = Mathf.CeilToInt(this.countdownSeconds).ToString();
            if (this.cachedCountdownLabelScale == Vector3.zero)
            {
                this.cachedCountdownLabelScale = this.countdownLabel.cachedTransform.localScale;
            }
            this.countdownLabel.cachedTransform.localScale = this.cachedCountdownLabelScale * ((1f - this.countdownSeconds % 1f) * 0.5f + 1f);
            if (this.countdownSeconds < 0f)
            {
                UIIngameUpdater.countingDown = false;
                this.countdownStartingLabel.text = string.Empty;
                this.countdownLabel.text = string.Empty;
                if (Game.Instance != null)
                {
                    Game.Instance.TriggerPause(false);
                }
            }
        }
    }

    private void OnCoinsChanged()
    {
        this.coinLabel.text = string.Empty + GameStats.Instance.coins;
        this.ResizeCoinBox();
    }

    private void OnGameStarted()
    {
        if (!Game.Instance.isReadyForHeadStart)
        {
            this.headstartHelper.HideHeadStart(true);
        }
    }

    private void SetScoreLabel()
    {
        this.score = GameStats.Instance.score;
        string text;
        switch (Utility.NumberOfDigits(this.score))
        {
            case 1:
                text = "00000";
                break;
            case 2:
                text = "0000";
                break;
            case 3:
                text = "000";
                break;
            case 4:
                text = "00";
                break;
            case 5:
                text = "0";
                break;
            default:
                text = string.Empty;
                break;
        }
        this.scoreLabel.text = text + this.score.ToString();
        this.ResizeScoreBox();
    }

    private void ResizeScoreBox()
    {
        int length = this.scoreLabel.text.Length;
        float num = 99f;
        if (length > 6)
        {
            num += (float)(13 * (length - 6));
        }
        if (this._cachedScoreBGTransform.localScale.x != num)
        {
            this._cachedScoreBGTransform.localScale = new Vector3(num, this._cachedScoreBGTransform.localScale.y, this._cachedScoreBGTransform.localScale.z);
        }
    }

    private void ResizeCoinBox()
    {
        int length = this.coinLabel.text.Length;
        float num = 64f;
        if (length > 1)
        {
            num += (float)(13 * (length - 1));
        }
        if (this._cachedCoinBGTransform.localScale.x != num)
        {
            this._cachedCoinBGTransform.localScale = new Vector3(num, this._cachedCoinBGTransform.localScale.y, this._cachedCoinBGTransform.localScale.z);
        }
    }

    private void ResizeMultiplierBox()
    {
        int length = this.multiplierLabel.text.Length;
        float num = 50f;
        if (length > 2)
        {
            num += (float)(10 * (length - 2));
        }
        if (this.multiplierBG.transform.localScale.x != num)
        {
            this.multiplierBG.transform.localScale = new Vector3(num, this.multiplierBG.transform.localScale.y, this.multiplierBG.transform.localScale.z);
        }
    }

    public UILabel scoreLabel;

    public UILabel multiplierLabel;

    public UILabel coinLabel;

    public UISlicedSprite scoreBG;

    private Transform _cachedScoreBGTransform;

    public UISlicedSprite multiplierBG;

    public UISlicedSprite coinBG;

    private Transform _cachedCoinBGTransform;

    public UIHeadStartHelper headstartHelper;

    public UILabel countdownStartingLabel;

    public UILabel countdownLabel;

    private float countdownSeconds;

    private static bool countingDown;

    private Vector3 cachedCountdownLabelScale = Vector3.zero;

    private int score = -1;
}
