using System;
using System.Collections;
using UnityEngine;

public class FollowingGuard : MonoBehaviour
{
    private void Awake()
    {
        this.game = Game.Instance;
        this.character = Character.Instance;
        this.characterTransform = this.character.transform;
        this.enemyRenderers = base.gameObject.GetComponentsInChildren<Renderer>();
        this.enemiesStartPos = new Vector3[this.enemies.Length];
        for (int i = 0; i < this.enemies.Length; i++)
        {
            this.enemiesStartPos[i] = this.enemies[i].position;
        }
        this.x = new SmoothDampFloat(0f, this.xSmoothTime);
        GetComponent<AudioSource>().volume = this.guardProximityLoopVolume;
        Game game = this.game;
        game.OnPauseChange = (Game.OnPauseChangeDelegate)Delegate.Combine(game.OnPauseChange, new Game.OnPauseChangeDelegate(this.HandleOnPauseChange));
    }

    private void HandleOnPauseChange(bool pause)
    {
        if (pause)
        {
            if (GetComponent<AudioSource>().isPlaying)
            {
                GetComponent<AudioSource>().Pause();
            }
            this.isPaused = true;
        }
        if (!pause)
        {
            if (this.isPaused)
            {
                GetComponent<AudioSource>().Play();
            }
            this.isPaused = false;
        }
    }

    public void Restart(bool closeToCharacter)
    {
        base.StopAllCoroutines();
        this.closeToCharacter = closeToCharacter;
        this.distanceToCharacter = ((!closeToCharacter) ? this.distanceToCharacterMax : this.distanceToCharacterMin);
    }

    public void OnEnable()
    {
        this.lastGroundedSmooth = this.character.lastGroundedY;
        this.lastGroundedVelocity = 0f;
        this.y = this.character.lastGroundedY;
        this.x.Value = this.character.transform.position.x;
        this.distanceToCharacter = this.distanceToCharacterMin;
        this.closeToCharacter = true;
        this.verticalSpeed = 0f;
        bool flag = false;
        this.guardAnimation["Guard_Run"].enabled = flag;
        if (flag)
        {
            this.guardAnimation.Play("Guard_Run");
            this.dogRightAnimation.Play("Dog_Fast Run");
        }
        Character character = this.character;
        character.OnJump = (Character.OnJumpDelegate)Delegate.Combine(character.OnJump, new Character.OnJumpDelegate(this.Jump));
    }

    public void OnDisable()
    {
        Character character = this.character;
        character.OnJump = (Character.OnJumpDelegate)Delegate.Remove(character.OnJump, new Character.OnJumpDelegate(this.Jump));
    }

    public void CatchUp()
    {
        this.CatchUp(this.catchUpDuration);
    }

    public void CatchUp(float duration)
    {
        if (!this.closeToCharacter)
        {
            float distanceFrom = this.distanceToCharacter;
            this.ShowEnemies(true);
            base.StopAllCoroutines();
            this.guardAnimation.Play("Guard_grap after");
            this.guardAnimation.PlayQueued("Guard_Run");
            GetComponent<AudioSource>().timeSamples = UnityEngine.Random.Range(0, GetComponent<AudioSource>().timeSamples);
            GetComponent<AudioSource>().Play();
            GetComponent<AudioSource>().pitch = UnityEngine.Random.Range(0.9f, 1.05f);
            base.StartCoroutine(pTween.To(duration, delegate (float t)
            {
                this.distanceToCharacter = Mathf.SmoothStep(distanceFrom, this.distanceToCharacterMin, t);
            }));
            base.StartCoroutine(pTween.To(duration, delegate (float t)
            {
                GetComponent<AudioSource>().volume = Mathf.SmoothStep(0f, this.guardProximityLoopVolume, t);
            }));
            this.closeToCharacter = true;
        }
    }

    public void ResetCatchUp()
    {
        this.ResetCatchUp(this.resetCatchUpDuration);
    }

    public void ResetCatchUp(float duration)
    {
        base.StartCoroutine(this.ResetCatchUpCoroutine(duration));
    }

    public IEnumerator ResetCatchUpCoroutine(float duration)
    {
        if (this.closeToCharacter)
        {
            float distanceFrom = this.distanceToCharacter;
            this.closeToCharacter = false;
            base.StartCoroutine(pTween.To(duration, delegate (float t)
            {
                this.distanceToCharacter = Mathf.SmoothStep(distanceFrom, this.distanceToCharacterMax, t);
            }));
            yield return base.StartCoroutine(pTween.To(duration * 2f, delegate (float t)
            {
                GetComponent<AudioSource>().volume = Mathf.SmoothStep(this.guardProximityLoopVolume, 0f, t);
            }));
            GetComponent<AudioSource>().Stop();
            if (!this.game.isDead)
            {
                this.ShowEnemies(false);
            }
        }
        yield break;
    }

    public void MuteProximityLoop()
    {
        GetComponent<AudioSource>().Stop();
    }

    public void PlayIntro()
    {
        base.gameObject.transform.position = new Vector3(0f, 0f, -10f);
        for (int i = 0; i < this.enemies.Length; i++)
        {
            this.enemies[i].position = this.enemiesStartPos[i];
            this.enemies[i].rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        this.guardAnimation.Play("playIntro");
        this.dogRightAnimation.Play("playIntro");
        this.guardAnimation.CrossFadeQueued("Guard_Run", 0.2f);
        this.dogRightAnimation.CrossFadeQueued("Dog_Fast Run", 0.2f);
    }

    public void CatchPlayer(float pos)
    {
        GetComponent<AudioSource>().Stop();
        base.StopAllCoroutines();
        this.character.characterAnimation.Stop("caught");
        this.character.characterAnimation.Stop("caught2");
        if (pos < 20f)
        {
            this.guardAnimation.CrossFade("catch2", 0.2f);
            this.dogRightAnimation.CrossFade("catch2", 0.2f);
            this.character.animations.stumbleDeath = "caught2";
        }
        else
        {
            this.guardAnimation.CrossFade("catch", 0.2f);
            this.dogRightAnimation.CrossFade("catch", 0.2f);
            this.character.animations.stumbleDeath = "caught";
        }
        this.character.characterAnimation[this.character.animations.stumbleDeath].weight = 0f;
        this.character.characterAnimation[this.character.animations.stumbleDeath].enabled = true;
        float num = 0.68f;
        base.StartCoroutine(pTween.To(num, delegate (float t)
        {
            for (int i = 0; i < this.enemies.Length; i++)
            {
                this.enemies[i].position = Vector3.Lerp(this.enemies[i].position, this.character.transform.position, t);
            }
        }));
        base.StartCoroutine(this.CatchPlayerAnimStarter(num));
    }

    private IEnumerator CatchPlayerAnimStarter(float delay)
    {
        yield return new WaitForSeconds(delay);
        base.StartCoroutine(pTween.To(0.2f, delegate (float t)
        {
            this.character.characterAnimation[this.character.animations.stumbleDeath].weight = Mathf.Lerp(0f, 1f, t);
        }));
        yield break;
    }

    public void HitByTrainSequence()
    {
        GetComponent<AudioSource>().Stop();
        base.StartCoroutine(this.HitByTrainSequenceCoroutine());
    }

    public IEnumerator HitByTrainSequenceCoroutine()
    {
        GameStats.Instance.guardHitScreen++;
        float catchUpTime = 0.2f;
        yield return base.StartCoroutine(pTween.To(catchUpTime, delegate (float t)
        {
            for (int i = 0; i < this.enemies.Length; i++)
            {
                this.enemies[i].position = Vector3.Lerp(this.enemies[i].position, this.character.transform.position, t);
            }
        }));
        this.dogRightAnimation.Play("Dog_death_movingTrain");
        yield return new WaitForSeconds(0.4f);
        Vector3 charPos = this.characterTransform.position;
        base.StartCoroutine(pTween.To(1f, delegate (float t)
        {
            this.characterTransform.position = Vector3.Lerp(charPos, new Vector3(charPos.x, -5f, charPos.z), t);
        }));
        yield return new WaitForSeconds(0.2f);
        this.guardAnimation.Play("Guard_death_movingTrain");
        yield break;
    }

    public void ShowEnemies(bool vis)
    {
        this.isShowing = vis;
        foreach (Renderer renderer in this.enemyRenderers)
        {
            renderer.gameObject.active = vis;
        }
    }

    public void LateUpdate()
    {
        this.x.Target = this.character.transform.position.x;
        this.x.Update();
        this.lastGroundedSmooth = Mathf.SmoothDamp(this.lastGroundedSmooth, this.character.lastGroundedY, ref this.lastGroundedVelocity, this.lastGroundedSmoothTime);
        if (this.y > this.lastGroundedSmooth)
        {
            this.verticalSpeed -= this.gravity * Time.deltaTime;
        }
        this.y += this.verticalSpeed * Time.deltaTime;
        this.y = Mathf.Max(this.y, this.lastGroundedSmooth);
        Vector3 vector = this.characterTransform.position - Vector3.forward * this.distanceToCharacter;
        vector.y = this.y;
        vector.x = this.x.Value;
        base.transform.position = vector;
    }

    private void Jump()
    {
        this.Jump(this.distanceToCharacter / this.game.currentSpeed);
    }

    public void Jump(float delay)
    {
        if (this.distanceToCharacter <= this.distanceToCharacterMin)
        {
            Missions.Instance.PlayerDidThis(Missions.MissionTarget.GuardJump, 1);
        }
        base.StartCoroutine(this.JumpCoroutine(delay));
    }

    private IEnumerator JumpCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        this.guardAnimation.Play("Guard_jump");
        this.guardAnimation.CrossFadeQueued("Guard_Run", 0.2f);
        this.dogRightAnimation.Play("Dog_jump");
        this.dogRightAnimation.CrossFadeQueued("Dog_Fast Run", 0.2f);
        this.verticalSpeed = this.character.CalculateJumpVerticalSpeed() * 0.6f;
        yield break;
    }

    public static FollowingGuard Instance
    {
        get
        {
            FollowingGuard followingGuard;
            if ((followingGuard = FollowingGuard.instance) == null)
            {
                followingGuard = (FollowingGuard.instance = UnityEngine.Object.FindObjectOfType(typeof(FollowingGuard)) as FollowingGuard);
            }
            return followingGuard;
        }
    }

    public float distanceToCharacterMin = 10f;

    public float distanceToCharacterMax = 50f;

    public float catchUpDuration = 0.7f;

    public float resetCatchUpDuration = 1.5f;

    public float lastGroundedSmoothTime = 0.3f;

    public float xSmoothTime = 0.1f;

    public float gravity = 200f;

    public bool isShowing;

    public Animation guardAnimation;

    public Animation dogRightAnimation;

    private Renderer[] enemyRenderers;

    public Transform[] enemies;

    private Vector3[] enemiesStartPos;

    private float y;

    private bool closeToCharacter;

    private float distanceToCharacter;

    private float lastGroundedSmooth;

    private float lastGroundedVelocity;

    private SmoothDampFloat x;

    private Game game;

    private Character character;

    private Transform characterTransform;

    private float verticalSpeed;

    public float guardProximityLoopVolume = 0.9f;

    private static FollowingGuard instance;

    private bool isPaused = true;
}
