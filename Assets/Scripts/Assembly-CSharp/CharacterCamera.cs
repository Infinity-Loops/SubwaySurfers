using System;
using UnityEngine;

public class CharacterCamera : MonoBehaviour
{
    public void Shake()
    {
        Vector3 diff = Vector3.zero;
        float amplitude = 100f;
        base.StartCoroutine(pTween.To(0.3f, delegate (float t)
        {
            diff += UnityEngine.Random.insideUnitSphere;
            this.shake = (1f - t) * diff * amplitude * Time.deltaTime;
        }));
    }

    public void LateUpdate()
    {
        base.transform.position = this.position + this.shake;
        base.transform.LookAt(this.target + this.shake);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(this.position, this.target);
    }

    public static CharacterCamera Instance
    {
        get
        {
            CharacterCamera characterCamera;
            if ((characterCamera = CharacterCamera.instance) == null)
            {
                characterCamera = (CharacterCamera.instance = UnityEngine.Object.FindObjectOfType(typeof(CharacterCamera)) as CharacterCamera);
            }
            return characterCamera;
        }
    }

    public Vector3 position;

    public Vector3 target;

    private Vector3 shake = Vector3.zero;

    private static CharacterCamera instance;
}
