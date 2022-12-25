using System;
using System.Collections;
using UnityEngine;

public class TutorialEvent : MonoBehaviour
{
    public void Awake()
    {
        this.game = Game.Instance;
        this.hoverboard = Hoverboard.Instance;
    }

    public void Update()
    {
        if (this.game == null)
        {
            return;
        }
        if (!this.Initialiseret)
        {
            this.character = this.game.character;
            this.track = this.game.track;
            this.Initialiseret = true;
        }
    }

    private IEnumerator ShowArrow()
    {
        this.mesh.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
        this.mesh.transform.Rotate(new Vector3(0f, 0f, 1f), this.direction);
        this.mesh.active = true;
        Vector3 pos = new Vector3(0f, 0f, 20f);
        yield return base.StartCoroutine(pTween.To(this.time, delegate (float t)
        {
            this.mesh.transform.localPosition = Vector3.Lerp(pos - this.mesh.transform.up * 5f, pos + this.mesh.transform.up * 5f, t);
            this.mesh.GetComponent<Renderer>().material.mainTextureOffset = Vector2.Lerp(Vector2.zero, new Vector2(0f, -0.02f), t);
        }));
        this.mesh.active = false;
        yield break;
    }

    private void OnTriggerExit(Collider collider)
    {
        if (!this.character.stopColliding && collider.gameObject.name.Equals("Character"))
        {
            if (this.displayText)
            {
                UIScreenController.Instance.QueueMessage(this.text);
            }
            if (this.displayMesh)
            {
                base.StartCoroutine(this.ShowArrow());
            }
            if (this.allowHoverboard)
            {
                this.hoverboard.isAllowed = true;
            }
            if (this.endTutorial)
            {
                this.track.IsRunningOnTutorialTrack = false;
                PlayerInfo.Instance.tutorialCompleted = true;
                this.track.tutorial = false;
            }
        }
    }

    private Game game;

    public bool displayText;

    public string text;

    public bool displayMesh;

    public GameObject mesh;

    public float direction;

    public float time = 1f;

    public bool endTutorial;

    public bool allowHoverboard;

    private Hoverboard hoverboard;

    private Character character;

    private Track track;

    private bool Initialiseret;
}
