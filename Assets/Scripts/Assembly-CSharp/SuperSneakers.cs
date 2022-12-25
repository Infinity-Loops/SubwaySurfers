using System;
using System.Collections;
using UnityEngine;

public class SuperSneakers : CharacterModifier
{
    public void Awake()
    {
        this.character = Character.Instance;
        this.characterAnimation = this.character.characterAnimation;
        this.objects = UnityEngine.Object.FindObjectsOfType(typeof(SuperSneakersGroup)) as SuperSneakersGroup[];
        this.characterController = this.character.characterController;
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
        this.character.Stumble = false;
        this.isActive = true;
        this.powerupMesh.active = true;
        this.character.ChangeAnimations();
        this.Powerup = GameStats.Instance.TriggerPowerup(PowerupType.supersneakers);
        float duration = this.Powerup.timeLeft;
        this.coinMagnetCollider.OnEnter = new OnTriggerObject.OnEnterDelegate(this.CoinHit);
        this.coinMagnetCollider.GetComponent<Collider>().enabled = true;
        this.character.jumpHeight = this.character.jumpHeightSuperSneakers;
        this.stop = CharacterModifier.StopSignal.DONT_STOP;
        foreach (SuperSneakersGroup o in this.objects)
        {
            o.GroupActive = true;
        }
        while (this.Powerup.timeLeft > 0f && this.stop == CharacterModifier.StopSignal.DONT_STOP)
        {
            yield return 0;
            this.ratio = this.Powerup.timeLeft / duration;
        }
        this.coinMagnetCollider.GetComponent<Collider>().enabled = false;
        OnTriggerObject onTriggerObject = this.coinMagnetCollider;
        onTriggerObject.OnEnter = (OnTriggerObject.OnEnterDelegate)Delegate.Remove(onTriggerObject.OnEnter, new OnTriggerObject.OnEnterDelegate(this.CoinHit));
        this.ratio = 0f;
        foreach (SuperSneakersGroup o2 in this.objects)
        {
            o2.GroupActive = false;
        }
        this.character.jumpHeight = this.character.jumpHeightNormal;
        this.powerupMesh.active = false;
        this.isActive = false;
        this.character.ChangeAnimations();
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
        Pickup pickup = coin.GetComponent<Pickup>();
        this.character.NotifyPickup(pickup);
        yield break;
    }

    public override bool ShouldPauseInJetpack
    {
        get
        {
            return true;
        }
    }

    private float duration;

    public GameObject powerupMesh;

    private Animation characterAnimation;

    [HideInInspector]
    public bool isActive;

    public OnTriggerObject coinMagnetCollider;

    public float pullSpeed = 200f;

    private CharacterController characterController;

    private float ratio;

    private Character character;

    private SuperSneakersGroup[] objects;

    private Game game;

    public AudioClipInfo powerDownSound;

    public ActivePowerup Powerup;
}
