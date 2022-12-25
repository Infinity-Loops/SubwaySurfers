using System;
using System.Collections;
using UnityEngine;

public class CoinMagnet : CharacterModifier
{
    private void Awake()
    {
        this.character = Character.Instance;
        this.characterController = this.character.characterController;
        this.coinEFX = this.character.CharacterPickupParticleSystem.CoinEFX.transform;
        this.characterAnimation = this.character.characterAnimation;
        this.characterAnimation["hold_magnet"].AddMixingTransform(this.shoulder);
        this.characterAnimation["hold_magnet"].layer = 3;
        this.characterAnimation["hold_magnet"].weight = 0.9f;
        this.characterAnimation["hold_magnet"].enabled = false;
        this.game = Game.Instance;
    }

    public override void Reset()
    {
        this.ratio = 0f;
        this.Paused = false;
    }

    public override IEnumerator Begin()
    {
        GameStats.Instance.usePowerups++;
        this.Paused = false;
        this.audioStateLoop.ChangeLoop(AudioState.Magnet);
        this.character.Stumble = false;
        this.powerupMesh.active = true;
        this.characterAnimation["hold_magnet"].enabled = true;
        this.characterAnimation.Play("hold_magnet");
        this.Powerup = GameStats.Instance.TriggerPowerup(PowerupType.coinmagnet);
        this.duration = this.Powerup.timeLeft;
        this.coinMagnetCollider.OnEnter = new OnTriggerObject.OnEnterDelegate(this.CoinHit);
        this.coinMagnetCollider.GetComponent<Collider>().enabled = true;
        base.enabled = true;
        this.stop = CharacterModifier.StopSignal.DONT_STOP;
        while (this.Powerup.timeLeft > 0f && this.stop == CharacterModifier.StopSignal.DONT_STOP)
        {
            yield return 0;
            this.ratio = this.Powerup.timeLeft / this.duration;
        }
        this.coinMagnetCollider.GetComponent<Collider>().enabled = false;
        base.enabled = false;
        this.powerupMesh.active = false;
        this.coinEFX.localPosition = Vector3.zero;
        this.characterAnimation["hold_magnet"].enabled = false;
        this.audioStateLoop.ChangeLoop(AudioState.MagnetStop);
        if (this.Powerup.timeLeft <= 0f)
        {
            So.Instance.playSound(this.powerDownSound);
        }
        yield break;
    }

    public void CoinHit(Collider collider)
    {
        Coin component = collider.GetComponent<Coin>();
        if (component != null)
        {
            component.GetComponent<Collider>().enabled = false;
            base.StartCoroutine(this.Pull(component));
        }
    }

    private IEnumerator Pull(Coin coin)
    {
        Transform pivot = coin.pivot.transform;
        Vector3 position = pivot.position;
        float distance = (position - this.characterController.transform.position).magnitude;
        yield return base.StartCoroutine(pTween.To(distance / (this.pullSpeed * this.game.NormalizedGameSpeed), delegate (float t)
        {
            pivot.position = Vector3.Lerp(position, this.powerupMesh.transform.position, t * t);
        }));
        this.coinEFX.position = this.powerupMesh.transform.position;
        Pickup pickup = coin.GetComponent<Pickup>();
        this.character.NotifyPickup(pickup);
        GameStats.Instance.coinsCoinMagnet++;
        yield break;
    }

    private float duration;

    public OnTriggerObject coinMagnetCollider;

    public float pullSpeed = 150f;

    public GameObject powerupMesh;

    private CharacterController characterController;

    private Animation characterAnimation;

    private Character character;

    private Transform coinEFX;

    public Transform shoulder;

    private float ratio;

    private Game game;

    public AudioStateLoop audioStateLoop;

    public AudioClipInfo powerDownSound;

    public ActivePowerup Powerup;
}
