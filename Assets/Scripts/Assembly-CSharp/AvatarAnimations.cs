using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarAnimations : MonoBehaviour
{
    private void Start()
    {
        this.Target = this.FindAnimationInParent(base.gameObject);
        if (this.Target == null)
        {
            Debug.Log(" No animation component for avatar animations");
            return;
        }
        if (this.PlayIdleAnimations)
        {
            this.StartIdleAnimations();
        }
    }

    private Animation FindAnimationInParent(GameObject current)
    {
        Animation component = current.GetComponent<Animation>();
        if (component != null)
        {
            return component;
        }
        if (current.transform.parent != null)
        {
            return this.FindAnimationInParent(current.transform.parent.gameObject);
        }
        return null;
    }

    private void Update()
    {
        if (this.PlayIdleAnimations && this.animationRoutine != null && !this.Paused)
        {
            this.animationRoutine.MoveNext();
        }
    }

    public void StartIdleAnimations()
    {
        this.PlayIdleAnimations = true;
        this.Paused = false;
        this.Target.AddClip(this.Breath, this.Breath.name);
        foreach (AnimationClip animationClip in this.Idles)
        {
            this.Target.AddClip(animationClip, animationClip.name);
        }
        this.animationRoutine = this.Play();
        this.animationRoutine.MoveNext();
    }

    public void StopIdleAnimations()
    {
        this.PlayIdleAnimations = false;
        this.Target.AddClip(this.Breath, this.Breath.name);
        foreach (AnimationClip animationClip in this.Idles)
        {
            foreach (object obj in this.Target)
            {
                AnimationState animationState = (AnimationState)obj;
                if (animationState.clip == animationClip)
                {
                    this.Target.RemoveClip(animationClip);
                }
            }
        }
        this.animationRoutine = null;
    }

    public void PauseIdleAnimations()
    {
        this.Paused = true;
        foreach (object obj in this.Target.GetComponent<Animation>())
        {
            AnimationState animationState = (AnimationState)obj;
            animationState.speed = 0f;
        }
    }

    public void ResumeIdleAnimations()
    {
        this.Paused = false;
        foreach (object obj in this.Target.GetComponent<Animation>())
        {
            AnimationState animationState = (AnimationState)obj;
            animationState.speed = 1f;
        }
    }

    private IEnumerator Play()
    {
        int index = 0;
        List<AnimationClip> tmpList = new List<AnimationClip>();
        while (this.PlayIdleAnimations)
        {
            int count = UnityEngine.Random.Range(this.MinIdleTimes, this.MaxIdleTimes);
            for (int i = 0; i < count; i++)
            {
                this.Target.Play(this.Breath.name);
                this.nextAnimationTime = this.Breath.length;
                while (this.nextAnimationTime > 0f)
                {
                    this.nextAnimationTime -= Time.deltaTime;
                    yield return 0;
                }
            }
            tmpList = this.Idles.FindAll((AnimationClip a) => a != this.Idles[index]);
            index = UnityEngine.Random.Range(0, tmpList.Count);
            this.Target.Play(tmpList[index].name);
            this.nextAnimationTime = tmpList[index].length;
            while (this.nextAnimationTime > 0f)
            {
                this.nextAnimationTime -= Time.deltaTime;
                yield return 0;
            }
        }
        this.animationRoutine = null;
        yield break;
    }

    public Animation Target;

    public bool PlayIdleAnimations;

    public int MinIdleTimes;

    public int MaxIdleTimes;

    public AnimationClip Breath;

    public List<AnimationClip> Idles;

    public bool Paused;

    private IEnumerator animationRoutine;

    private float nextAnimationTime;
}
