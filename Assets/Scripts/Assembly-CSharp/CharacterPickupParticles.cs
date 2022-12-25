using System;
using System.Collections;
using UnityEngine;

public class CharacterPickupParticles : MonoBehaviour
{
    public void Awake()
    {
        this.lastCoinPosition = base.transform.position.z;
        this.offset = base.transform.position - this.master.position;
    }

    public void PickedUpCoin(Pickup pickup)
    {
        if (80f < pickup.transform.position.y)
        {
            this.coinStairway = 0;
            this.CoinPickup.maxPitch = Mathf.Pow(2f, (float)this.flyWay / 48f);
            this.CoinPickup.minPitch = Mathf.Pow(2f, (float)this.flyWay / 48f);
            this.flyWay++;
        }
        else if (pickup.transform.position.y < 0.1f || (8.795f < pickup.transform.position.y && pickup.transform.position.y < 8.805f) || (9.95f < pickup.transform.position.y && pickup.transform.position.y < 10.05f) || (28.95f < pickup.transform.position.y && pickup.transform.position.y < 29.05f) || (34.95f < pickup.transform.position.y && pickup.transform.position.y < 35.05f))
        {
            this.flyWay = 0;
            this.coinStairway = 0;
            this.CoinPickup.maxPitch = Mathf.Pow(2f, (float)this.pentatonicScale[this.coinStairway % this.pentatonicScale.Length] / 12f) * 0.5f;
            this.CoinPickup.minPitch = Mathf.Pow(2f, (float)this.pentatonicScale[this.coinStairway % this.pentatonicScale.Length] / 12f) * 0.5f;
        }
        else
        {
            this.flyWay = 0;
            if (this.coinStairway < this.pentatonicScale.Length - 1)
            {
                this.coinStairway++;
            }
            this.CoinPickup.maxPitch = Mathf.Pow(2f, (float)this.pentatonicScale[this.coinStairway % this.pentatonicScale.Length] / 12f) * 0.5f;
            this.CoinPickup.minPitch = Mathf.Pow(2f, (float)this.pentatonicScale[this.coinStairway % this.pentatonicScale.Length] / 12f) * 0.5f;
        }
        So.Instance.playSound(this.CoinPickup);
        this.DoCoinEFX();
        this.lastCoinPosition = pickup.transform.position.y;
    }

    private void DoCoinEFX()
    {
        float num = UnityEngine.Random.Range(0f, 360f);
        this.CoinEFX.transform.Rotate(0f, 0f, num);
        this.CoinEFX.GetComponent<Animation>().Stop("pickup");
        this.CoinEFX.GetComponent<Animation>().Play("pickup");
        base.StartCoroutine(this.AnimateAlpha(this.CoinEFX, this.CoinEFX.GetComponent<Animation>()["pickup"].length));
    }

    public void PickedUpPowerUp()
    {
        So.Instance.playSound(this.PowerUpPickup);
        this.PickedUpDefaultPowerUp();
    }

    public void PickedUpDefaultPowerUp()
    {
        this.DoCoinEFX();
        float num = UnityEngine.Random.Range(0f, 360f);
        this.PowerUpEFX.transform.Rotate(0f, 0f, num);
        this.PowerUpEFX.GetComponent<Animation>().Stop("pickup");
        this.PowerUpEFX.GetComponent<Animation>().Play("pickup");
        base.StartCoroutine(this.AnimateAlpha(this.PowerUpEFX, this.PowerUpEFX.GetComponent<Animation>()["pickup"].length));
    }

    private IEnumerator AnimateAlpha(GameObject efx, float time)
    {
        return pTween.To(time, delegate (float t)
        {
            this.transform.position = this.master.position + this.offset;
            efx.GetComponent<Renderer>().material.SetColor("_MainColor", Color.Lerp(Color.white, Color.black, t));
        });
    }
    private IEnumerator TimeScaleTest(float time)
    {
        Time.timeScale = 0.5f;
        yield return new WaitForSeconds(time);
        Time.timeScale = 1f;
        yield break;
    }

    public GameObject CoinEFX;

    public GameObject PowerUpEFX;

    public Transform master;

    private Vector3 offset;

    public AudioClipInfo CoinPickup;

    public AudioClipInfo PowerUpPickup;

    public float CoinDistanceForStairway;

    private float lastCoinPosition;

    private int coinStairway;

    private int flyWay;

    private int[] pentatonicScale = new int[]
    {
        12, 13, 14, 15, 16, 17, 18, 19, 20, 21,
        22, 23, 24, 25, 26, 27, 28
    };
}
