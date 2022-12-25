using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class KeyframeBehaviour : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CDoKeyframeAnimation_003Ec__AnonStorey4E
	{
		internal KeyFrameAction info;

		internal void _003C_003Em__0(ParticleSystem t)
		{
			t.enableEmission = info.state;
		}
	}

	public Animation TargetAnimation;

	public List<ParticleSystem> TargetObjects;

	public List<KeyFrameAction> Actions;

	private AnimationEvent[] animationEvents;

	private void Start()
	{
		animationEvents = new AnimationEvent[Actions.Count];
		int num = 0;
		foreach (KeyFrameAction action in Actions)
		{
			AnimationEvent animationEvent = new AnimationEvent();
			animationEvent.messageOptions = SendMessageOptions.RequireReceiver;
			animationEvent.time = (float)action.KeyFrame / TargetAnimation[action.clip].clip.frameRate;
			animationEvent.intParameter = num;
			animationEvent.functionName = "DoKeyframeAnimation";
			TargetAnimation[action.clip].clip.AddEvent(animationEvent);
			num++;
		}
	}

	public void DoKeyframeAnimation(int soundIndex)
	{
		_003CDoKeyframeAnimation_003Ec__AnonStorey4E _003CDoKeyframeAnimation_003Ec__AnonStorey4E = new _003CDoKeyframeAnimation_003Ec__AnonStorey4E();
		_003CDoKeyframeAnimation_003Ec__AnonStorey4E.info = Actions[soundIndex];
		TargetObjects.ForEach(_003CDoKeyframeAnimation_003Ec__AnonStorey4E._003C_003Em__0);
	}
}
