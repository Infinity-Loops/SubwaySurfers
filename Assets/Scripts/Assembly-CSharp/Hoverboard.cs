using System;
using System.Collections;
using UnityEngine;

public class Hoverboard : CharacterModifier
{
    public void Awake()
    {
        this.character = Character.Instance;
        this.track = Track.Instance;
    }

    public override void Reset()
    {
        this.character.immuneToCriticalHit = false;
        this.character.characterController.enabled = true;
        this.character.characterCollider.enabled = true;
        this.powerupMesh.active = false;
        this.isActive = false;
        Time.timeScale = 1f;
        this.character.hoverboardCrashParticleSystem.gameObject.SetActiveRecursively(false);
    }

    public override IEnumerator Begin()
    {
        float timeSinceLastActivation = Time.time - this.lastEndActivationTime;
        if (!this.isAllowed || timeSinceLastActivation < this.WaitForParticlesDelay + PlayerInfo.Instance.GetHoverBoardCoolDown())
        {
            yield break;
        }
        GameStats.Instance.usePowerups++;
        PlayerInfo.Instance.UseUpgrade(PowerupType.hoverboard);
        Missions.Instance.PlayerDidThis(Missions.MissionTarget.HoverBoard, 1);
        this.Paused = false;
        this.character.Stumble = false;
        this.isActive = true;
        this.character.ChangeAnimations();
        this.character.characterAnimation.CrossFade("h_skate_on", 0.06f);
        this.character.characterAnimation.CrossFadeQueued("h_run", 0.2f);
        So.Instance.playSound(this.StartSound);
        this.character.CharacterPickupParticleSystem.PickedUpDefaultPowerUp();
        this.character.immuneToCriticalHit = true;
        this.stop = CharacterModifier.StopSignal.DONT_STOP;
        this.Powerup = GameStats.Instance.TriggerPowerup(PowerupType.hoverboard);
        this.duration = this.Powerup.timeLeft;
        this.powerupMesh.active = true;
        while (this.Powerup.timeLeft > 0f && this.stop == CharacterModifier.StopSignal.DONT_STOP)
        {
            yield return 0;
        }
        if (this.stop == CharacterModifier.StopSignal.DONT_STOP)
        {
            Missions.Instance.PlayerDidThis(Missions.MissionTarget.HoverBoardExpire, 1);
            So.Instance.playSound(this.powerDownSound);
        }
        this.powerupMesh.active = false;
        this.character.immuneToCriticalHit = false;
        this.isActive = false;
        this.character.ChangeAnimations();
        this.lastEndActivationTime = Time.time;
        if (this.stop == CharacterModifier.StopSignal.STOP)
        {
            this.isActive = false;
            this.character.immuneToCriticalHit = false;
            this.character.hoverboardCrashParticleSystem.gameObject.SetActiveRecursively(true);
            this.character.hoverboardCrashParticleSystem.Play();
            this.PlayCrashSound();
            float timeLeft = this.WaitForParticlesDelay;
            while (timeLeft > 0f)
            {
                timeLeft -= Time.deltaTime;
                yield return 0;
            }
            this.track.LayEmptyChunks(this.character.z, this.RemoveObstaclesDistance * Game.Instance.NormalizedGameSpeed);
            this.character.jumping = true;
            this.character.falling = false;
            this.character.verticalSpeed = this.character.CalculateJumpVerticalSpeed(10f);
            this.character.characterAnimation.CrossFade(this.character.animations.jump, 0.05f);
            float newSlowMotionDistance = this.slowMotionDistance * Game.Instance.NormalizedGameSpeed;
            float newCoolDownDist = this.cooldownDstance * Game.Instance.NormalizedGameSpeed;
            float distanceLeft = newSlowMotionDistance;
            bool didStopCooldown = false;
            while (distanceLeft > 0f)
            {
                distanceLeft -= Game.Instance.currentLevelSpeed * Time.deltaTime;
                newCoolDownDist -= Game.Instance.currentLevelSpeed * Time.deltaTime;
                if (newCoolDownDist < 0f && !didStopCooldown)
                {
                    this.character.immuneToCriticalHit = false;
                    didStopCooldown = true;
                }
                yield return 0;
            }
            this.character.hoverboardCrashParticleSystem.gameObject.SetActiveRecursively(false);
        }
        yield break;
    }

    public void PlayCrashSound()
    {
        So.Instance.playSound(this.CrashSound);
    }

    public override void Pause()
    {
        this.powerupMesh.active = false;
    }

    public override void Resume()
    {
        this.powerupMesh.active = true;
    }

    public override bool ShouldPauseInJetpack
    {
        get
        {
            return true;
        }
    }

    public static Hoverboard Instance
    {
        get
        {
            Hoverboard hoverboard;
            if ((hoverboard = Hoverboard.instance) == null)
            {
                hoverboard = (Hoverboard.instance = UnityEngine.Object.FindObjectOfType(typeof(Hoverboard)) as Hoverboard);
            }
            return hoverboard;
        }
    }

    public AudioClipInfo powerDownSound;

    private float duration;

    public float cooldownDstance = 50f;

    public float slowMotionDistance = 90f;

    public float slowDownToScale = 0.3f;

    public bool isAllowed = true;

    public GameObject powerupMesh;

    public float WaitForParticlesDelay;

    public float RemoveObstaclesDistance = 250f;

    private Game game;

    private Character character;

    private Track track;

    private float lastEndActivationTime;

    [HideInInspector]
    public bool isActive;

    public AudioClipInfo CrashSound;

    public AudioClipInfo StartSound;

    public ActivePowerup Powerup;

    private static Hoverboard instance;
}
