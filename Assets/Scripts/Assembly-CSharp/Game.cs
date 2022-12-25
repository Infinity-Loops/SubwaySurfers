using System;
using System.Collections;
using UnityEngine;

public class Game : MonoBehaviour
{
    public Game()
    {
        this.isInGame = new Variable<bool>(false);
    }

    public void TriggerPause(bool pauseGame)
    {
        this._paused = pauseGame;
        if (pauseGame)
        {
            this.ingameTouchDetection = false;
            Time.timeScale = 0f;
        }
        else
        {
            this.ingameTouchDetection = true;
            Time.timeScale = 1f;
        }
        if (this.OnPauseChange != null)
        {
            this.OnPauseChange(this._paused);
        }
    }

    public bool isPaused
    {
        get
        {
            return this._paused;
        }
    }

    public void StartNewRun()
    {
        this.isInGame.Value = true;
        this.ChangeState(null, this.Intro());
        Action onGameStarted = this.OnGameStarted;
        if (onGameStarted != null)
        {
            onGameStarted();
        }
    }

    public void Awake()
    {
        Game.HasLoaded = true;
        this.character = Character.Instance;
        this.characterAnimation = this.character.characterAnimation;
        this.guardAnimation = this.character.guardAnimation;
        this.track = Track.Instance;
        this.characterCamera = CharacterCamera.Instance;
        this.characterCameraTransform = this.characterCamera.transform;
        this.distort = this.FindObject<Distort>();
        this.running = Running.Instance;
        this.jetpack = Jetpack.Instance;
        this.enemies = FollowingGuard.Instance;
        this.modifiers = new CharacterModifierCollection();
        Character character = this.character;
        character.OnStumble = (Character.OnStumbleDelegate)Delegate.Combine(character.OnStumble, new Character.OnStumbleDelegate(this.OnStumble));
        Character character2 = this.character;
        character2.OnCriticalHit = (Character.OnCriticalHitDelegate)Delegate.Combine(character2.OnCriticalHit, new Character.OnCriticalHitDelegate(this.OnCriticalHit));
        this.currentLevelSpeed = this.Speed(0f);
        this.player = PlayerInfo.Instance;
        this.stats = GameStats.Instance;
        this.character.SetAnimations();
        this._testStats = base.GetComponent<TestStats>();
        this.awakeDone = true;
    }

    public void Start()
    {
        this.track.Restart();
        this.currentThread = this.GameIntro();
        this.currentThread.MoveNext();
    }

    public void Update()
    {
        float num = Time.time - this.startTime;
        this.currentLevelSpeed = this.Speed(num);
        this.currentThread.MoveNext();
        if (this.characterState != null)
        {
            this.modifiers.Update();
        }
        GameStats.Instance.UpdatePowerupTimes(Time.deltaTime);
    }

    public void LayTrackChunks()
    {
        this.track.LayTrackChunks(this.character.z);
    }
    private void HandleDebugControls()
    {
        this.DebugTimeControl();
        if (Input.GetKeyDown(KeyCode.S))
        {
            this.modifiers.Add(this.modifiers.SuperSneakes);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            this.modifiers.Add(this.modifiers.CoinMagnet);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            this.modifiers.Add(this.modifiers.Hoverboard);
        }
        if (this.characterState != null)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                this.characterState.HandleSwipe(SwipeDir.Up);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                this.characterState.HandleSwipe(SwipeDir.Down);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                this.characterState.HandleSwipe(SwipeDir.Left);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                this.characterState.HandleSwipe(SwipeDir.Right);
            }
        }
    }

    public void UpdateMeters()
    {
        this.stats.meters = (float)Mathf.RoundToInt(this.character.z / this.distancePerMeter);
    }

    public float CalcTime(float z)
    {
        if (z <= this.Position(this.speed.rampUpDuration))
        {
            float num = this.speed.min * this.speed.min + 2f * ((this.speed.max - this.speed.min) / this.speed.rampUpDuration) * z;
            return (-this.speed.min + Mathf.Sqrt(num)) / ((this.speed.max - this.speed.min) / this.speed.rampUpDuration);
        }
        return (z - this.Position(this.speed.rampUpDuration)) * 1f / this.speed.max + this.speed.rampUpDuration;
    }

    public void ChangeState(CharacterState state)
    {
        this.characterState = state;
        if (state != null)
        {
            this.currentThread = state.Begin();
        }
    }

    public void ChangeState(CharacterState state, IEnumerator thread)
    {
        this.characterState = state;
        this.currentThread = thread;
    }

    public void ActivateJetpack()
    {
        if (this.characterState != this.Jetpack)
        {
            this.ChangeState(this.Jetpack);
        }
    }

    private float Speed(float t)
    {
        if (t < this.speed.rampUpDuration)
        {
            return t * (this.speed.max - this.speed.min) / this.speed.rampUpDuration + this.speed.min;
        }
        return this.speed.max;
    }

    private float Position(float t)
    {
        if (t < this.speed.rampUpDuration)
        {
            return 0.5f * ((this.speed.max - this.speed.min) / this.speed.rampUpDuration) * t * t + this.speed.min * t;
        }
        return (t - this.speed.rampUpDuration) * this.speed.max + 0.5f * (this.speed.max - this.speed.min) * this.speed.rampUpDuration + this.speed.min * this.speed.rampUpDuration;
    }

    public void Die()
    {
        if (this.modifiers.IsActive(this.modifiers.Hoverboard))
        {
            this.enemies.MuteProximityLoop();
            this.enemies.ResetCatchUp();
            this.character.stumble = false;
            this.enemies.Restart(false);
            this.modifiers.Hoverboard.Stop = CharacterModifier.StopSignal.STOP;
            GameStats.Instance.RemoveHoverBoardPowerup();
        }
        else if (this.track.IsRunningOnTutorialTrack)
        {
            if (!this.goingBackToCheckpoint)
            {
                base.StartCoroutine(this.BackToCheckPointSequence());
            }
        }
        else
        {
            GameStats.Instance.ClearPowerups();
            this.isDead = true;
            MovingTrain.ActivateAutoPilot();
            MovingCoin.ActivateAutoPilot();
            if (this.enemies.isShowing)
            {
                if (this.characterAnimation["death_movingTrain"].enabled)
                {
                    this.enemies.HitByTrainSequence();
                }
                else
                {
                    this.enemies.CatchPlayer(this.character.x - this.character.GetTrackX());
                }
            }
            this.stats.duration = this.GetDuration();
            Missions.Instance.PlayerDidThis(Missions.MissionTarget.TimeDeath, Mathf.FloorToInt(GameStats.Instance.duration));
            this.enemies.enabled = false;
            if (this.OnGameOver != null)
            {
                this.OnGameOver(this.stats);
            }
            Action onGameEnded = this.OnGameEnded;
            if (onGameEnded != null)
            {
                onGameEnded();
            }
            base.StopAllCoroutines();
            this.ChangeState(null, this.SwitchToDieStateWhenGrounded());
        }
    }

    public float GetDuration()
    {
        return Time.time - this.startTime;
    }

    private IEnumerator SwitchToDieStateWhenGrounded()
    {
        while (!this.character.characterController.isGrounded)
        {
            this.character.MoveWithGravity();
            yield return 0;
        }
        this.ChangeState(null, this.DieSequence());
        yield break;
    }

    public void OnCriticalHit()
    {
        if (this.characterState != null)
        {
            So.Instance.playSound(this.DieSound);
            this.characterState.HandleCriticalHit();
        }
    }

    private IEnumerator StumbleDeathSequence()
    {
        this.currentSpeed = this.speed.min;
        yield return new WaitForSeconds(0.2f);
        if (this.characterState != this.Jetpack)
        {
            this.characterAnimation.CrossFade("stumbleFall", 0.2f);
            this.characterState.HandleCriticalHit();
        }
        yield break;
    }

    public void OnStumble()
    {
        if (this.character.Stumble && this.characterState != null)
        {
            base.StartCoroutine(this.StumbleDeathSequence());
        }
    }

    public void StartJetpack()
    {
        this.Jetpack.headStart = false;
        this.Jetpack.powerType = PowerupType.jetpack;
        this.ChangeState(this.Jetpack);
    }

    public void PickupJetpack()
    {
        Game.Instance.StartJetpack();
        GameStats.Instance.jetpackPickups++;
    }

    public void StartTopMenu()
    {
        this.ChangeState(null, this.TopMenu());
    }

    public void StartHeadStart2000()
    {
        if (this.isDead)
        {
            return;
        }
        float powerupDuration = PlayerInfo.Instance.GetPowerupDuration(PowerupType.headstart2000);
        this.Jetpack.headStart = true;
        this.Jetpack.powerType = PowerupType.headstart2000;
        this.Jetpack.headStartDistance = powerupDuration * this.distancePerMeter;
        this.Jetpack.headStartSpeed = 1000f;
        this.ChangeState(this.Jetpack);
        PlayerInfo.Instance.UseUpgrade(PowerupType.headstart2000);
        Missions.Instance.PlayerDidThis(Missions.MissionTarget.Headstart, 1);
        Missions.Instance.PlayerDidThis(Missions.MissionTarget.HaveHeadStartLarge, -1);
    }

    public void StartHeadStart500()
    {
        if (this.isDead)
        {
            return;
        }
        float powerupDuration = PlayerInfo.Instance.GetPowerupDuration(PowerupType.headstart500);
        this.Jetpack.headStart = true;
        this.Jetpack.powerType = PowerupType.headstart500;
        this.Jetpack.headStartDistance = powerupDuration * this.distancePerMeter;
        this.Jetpack.headStartSpeed = 1000f;
        this.ChangeState(this.Jetpack);
        PlayerInfo.Instance.UseUpgrade(PowerupType.headstart500);
        Missions.Instance.PlayerDidThis(Missions.MissionTarget.Headstart, 1);
    }

    private IEnumerator DieSequence()
    {
        float wait = Time.time + 2f;
        while (Time.time < wait - 1.5f)
        {
            yield return 0;
        }
        while (Time.time < wait)
        {
            if (Input.GetMouseButtonUp(0))
            {
                break;
            }
            if (Input.touchCount > 0)
            {
                Touch touch = Input.touches[0];
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    break;
                }
            }
            yield return 0;
        }
        this.ingameTouchDetection = false;
        UIScreenController.Instance.GameOverTriggered();
        this.ChangeState(null, this.TopMenu());
        yield break;
    }

    private void StageMenuSequence()
    {
        this.characterAnimation[this.character.animations.stumbleDeath].enabled = false;
        this.character.shadow.active = true;
        this.enemies.enabled = false;
        this.enemies.ShowEnemies(false);
        this.enemies.StopAllCoroutines();
        this.character.StopAllCoroutines();
        this.character.transform.position = Vector3.zero + new Vector3(0f, 0.8f, 0f);
        this.characterAnimation.transform.rotation = Quaternion.identity;
        this.character.sprayCanModel.GetComponent<Renderer>().enabled = true;
        foreach (ParticleSystem particleSystem in this.character.sprayCanModel.GetComponentsInChildren<ParticleSystem>())
        {
            particleSystem.enableEmission = false;
        }
        this.characterCamera.enabled = false;
        this.characterCamera.GetComponent<Camera>().fieldOfView = this.Running.cameraFOV;
        this.characterCameraTransform.localPosition = this.character.transform.position + this.Running.cameraOffset + Vector3.up * 0.8f;
        this.characterCameraTransform.localRotation = Quaternion.Euler(21.50143f, 0f, 0f);
        this.characterAnimation.Play("idlePaint");
    }

    private IEnumerator GameIntro()
    {
        UIScreenController.Instance.ShowMainMenu();
        this.ChangeState(null, this.TopMenu());
        yield break;
    }

    private IEnumerator TopMenu()
    {
        this.isInGame.Value = false;
        this.audioStateLoop.ChangeLoop(AudioState.Menu);
        this.enemies.MuteProximityLoop();
        this.track.DeactivateTrackChunks();
        this.modifiers.StopWithNoEnding();
        this.modifiers.Update();
        GameStats.Instance.ClearPowerups();
        this.jetpack.coinsManager.ReleaseCoins();
        this.distort.Reset();
        this.enemies.ShowEnemies(false);
        this.StageMenuSequence();
        this.characterCamera.transform.parent.GetComponent<Animation>().CrossFade("menuIdle", 0.1f);
        if (this.OnTopMenu != null)
        {
            this.OnTopMenu();
        }
        yield return null;
        yield break;
    }

    private IEnumerator Intro()
    {
        this.stats.Reset();
        this.audioStateLoop.ChangeLoop(AudioState.Ingame);
        this.enemies.MuteProximityLoop();
        this.isDead = false;
        this.ingameTouchDetection = true;
        this.character.CharacterPickupParticleSystem.CoinEFX.transform.localPosition = Vector3.zero;
        foreach (ParticleSystem ps in this.character.sprayCanModel.GetComponentsInChildren<ParticleSystem>())
        {
            ps.enableEmission = false;
        }
        this.StageMenuSequence();
        this.enemies.ShowEnemies(true);
        this.enemies.PlayIntro();
        this.currentLevelSpeed = this.Speed(0f);
        this.startTime = Time.time;
        this.character.Restart();
        SpawnPointManager.Instance.Restart();
        this.track.Restart();
        this.track.LayTrackChunks(0f);
        this.distort.Reset();
        this.characterCamera.transform.parent.GetComponent<Animation>().CrossFade("startPan", 0.2f);
        this.characterAnimation.CrossFade("introRun", 0.2f);
        IEnumerator cameraMovement = pTween.To(this.characterAnimation.GetComponent<Animation>()["introRun"].length, delegate (float t)
        {
        });
        while (cameraMovement.MoveNext())
        {
            yield return 0;
        }
        this.stats.Reset();
        this.character.sprayCanModel.GetComponent<Renderer>().enabled = false;
        this.enemies.enabled = true;
        if (this.track.IsRunningOnTutorialTrack)
        {
            this.enemies.ResetCatchUp();
            this.character.stumble = false;
        }
        this.isReadyForHeadStart = true;
        this.ChangeState(this.Running);
        yield return 0;
        yield break;
    }

    private bool HandleTap()
    {
        bool flag = false;
        if (Time.time < this.lastTapTime + this.swipe.doubleTapDuration && this.characterState != null)
        {
            this.characterState.HandleDoubleTap();
            flag = true;
        }
        this.lastTapTime = Time.time;
        return flag;
    }

    public void HandleControls()
    {
#if !UNITY_EDITOR
        if (this._paused)
        {
            return;
        }
        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];
            if (touch.phase == TouchPhase.Began)
            {
                this.currentSwipe = new Swipe();
                this.currentSwipe.start = touch.position;
                this.currentSwipe.startTime = Time.time;
            }
            if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && this.currentSwipe != null)
            {
                this.currentSwipe.endTime = Time.time;
                this.currentSwipe.end = touch.position;
                SwipeDir swipeDir = this.AnalyzeSwipe(this.currentSwipe);
                if (swipeDir != SwipeDir.None)
                {
                    if (this.characterState != null)
                    {
                        this.characterState.HandleSwipe(swipeDir);
                    }
                    this.currentSwipe = null;
                }
            }
            if (touch.phase == TouchPhase.Ended && this.currentSwipe != null)
            {
                this.currentSwipe.endTime = Time.time;
                this.currentSwipe.end = touch.position;
                SwipeDir swipeDir2 = this.AnalyzeSwipe(this.currentSwipe);
                if (swipeDir2 == SwipeDir.None && this.characterState != null)
                {
                    this.HandleTap();
                }
            }
        }
#else
        HandleDebugControls();
#endif
    }

    private void DebugTimeControl()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Time.timeScale = 0f;
            this.PrintTimeScale();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Time.timeScale = Mathf.Clamp01(Time.timeScale - 0.1f);
            this.PrintTimeScale();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Time.timeScale += 0.1f;
            this.PrintTimeScale();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Time.timeScale = 1f;
            this.PrintTimeScale();
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Time.timeScale = Mathf.Clamp01(Time.timeScale * 0.9f);
            this.PrintTimeScale();
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            Time.timeScale *= 1.11111116f;
            this.PrintTimeScale();
        }
    }

    private void PrintTimeScale()
    {
        Debug.Log("Time scale = " + Time.timeScale);
    }

    private SwipeDir AnalyzeSwipe(Swipe swipe)
    {
        Vector3 vector = Camera.main.ScreenToWorldPoint(new Vector3(swipe.start.x, swipe.start.y, 2f));
        Vector3 vector2 = Camera.main.ScreenToWorldPoint(new Vector3(swipe.end.x, swipe.end.y, 2f));
        float num = Vector3.Distance(vector2, vector);
        if (num < this.swipe.distanceMin)
        {
            return SwipeDir.None;
        }
        Vector3 vector3 = swipe.end - swipe.start;
        SwipeDir swipeDir = SwipeDir.None;
        float num2 = 0f;
        float num3 = Vector3.Dot(vector3, Vector3.up);
        if (num3 > num2)
        {
            num2 = num3;
            swipeDir = SwipeDir.Up;
        }
        num3 = Vector3.Dot(vector3, Vector3.down);
        if (num3 > num2)
        {
            num2 = num3;
            swipeDir = SwipeDir.Down;
        }
        num3 = Vector3.Dot(vector3, Vector3.left);
        if (num3 > num2)
        {
            num2 = num3;
            swipeDir = SwipeDir.Left;
        }
        num3 = Vector3.Dot(vector3, Vector3.right);
        if (num3 > num2)
        {
            swipeDir = SwipeDir.Right;
        }
        return swipeDir;
    }

    private IEnumerator BackToCheckPointSequence()
    {
        this.goingBackToCheckpoint = true;
        this.ChangeState(null);
        yield return new WaitForSeconds(this.backToCheckpointDelayTime);
        this.character.SetBackToCheckPoint(this.backToCheckpointZoomTime);
        yield return new WaitForSeconds(this.backToCheckpointZoomTime);
        this.goingBackToCheckpoint = false;
        yield break;
    }

    public Character Character
    {
        get
        {
            return this.character;
        }
    }

    public CharacterState CharacterState
    {
        get
        {
            return this.characterState;
        }
    }

    public CharacterModifierCollection Modifiers
    {
        get
        {
            return this.modifiers;
        }
    }

    public Running Running
    {
        get
        {
            return this.running;
        }
    }

    public Jetpack Jetpack
    {
        get
        {
            return this.jetpack;
        }
    }

    public bool IsInJetpackMode
    {
        get
        {
            return this.characterState == this.Jetpack;
        }
    }

    public bool HasSuperSneakers
    {
        get
        {
            return this.modifiers.SuperSneakes.isActive;
        }
    }

    public float NormalizedGameSpeed
    {
        get
        {
            return this.currentSpeed / this.speed.min;
        }
    }

    public static Game Instance
    {
        get
        {
            Game game;
            if ((game = Game.instance) == null)
            {
                game = (Game.instance = UnityEngine.Object.FindObjectOfType(typeof(Game)) as Game);
            }
            return game;
        }
    }

    public static CharacterController Charactercontroller
    {
        get
        {
            CharacterController characterController;
            if ((characterController = Game.characterController) == null)
            {
                characterController = (Game.characterController = UnityEngine.Object.FindObjectOfType(typeof(CharacterController)) as CharacterController);
            }
            return characterController;
        }
    }

    [HideInInspector]
    public bool isDead;

    public bool ingameTouchDetection = true;

    [HideInInspector]
    public float currentSpeed;

    public float currentLevelSpeed = 30f;

    public float distancePerMeter = 8f;

    public Game.SwipeInfo swipe;

    public Game.SpeedInfo speed;

    public float backToCheckpointDelayTime = 0.7f;

    public float backToCheckpointZoomTime = 1f;

    private bool goingBackToCheckpoint;

    public Transform introAnimation;

    private IEnumerator currentThread;

    private CharacterState characterState;

    [HideInInspector]
    public CharacterModifierCollection modifiers;

    private Swipe currentSwipe;

    private float lastTapTime = float.MinValue;

    public static bool HasLoaded;

    private static CharacterController characterController;

    public Character character;

    public Animation characterAnimation;

    public Animation guardAnimation;

    public Track track;

    private CharacterCamera characterCamera;

    private Transform characterCameraTransform;

    private Distort distort;

    private FollowingGuard enemies;

    public Running running;

    private Jetpack jetpack;

    private static Game instance;

    private float startTime;

    private float currentRunTime;

    private PlayerInfo player;

    private GameStats stats;

    public Action OnGameStarted;

    public Action OnGameEnded;

    public Game.OnGameOverDelegate OnGameOver;

    public Game.OnPauseChangeDelegate OnPauseChange;

    public Game.OnTopMenuDelegate OnTopMenu;

    public Variable<bool> isInGame;

    private TestStats _testStats;

    public AudioStateLoop audioStateLoop;

    public AudioClipInfo DieSound;

    public bool awakeDone;

    public bool isReadyForHeadStart;

    private bool _paused;

    [Serializable]
    public class SwipeInfo
    {
        public float distanceMin = 0.1f;

        public float doubleTapDuration = 0.3f;
    }

    [Serializable]
    public class SpeedInfo
    {
        public float min = 110f;

        public float max = 220f;

        public float rampUpDuration = 200f;
    }

    public delegate void OnGameOverDelegate(GameStats gameStats);

    public delegate void OnPauseChangeDelegate(bool pause);

    public delegate void OnTopMenuDelegate();
}
