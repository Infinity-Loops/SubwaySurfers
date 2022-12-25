using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ColliderQueues : MonoBehaviour
{
	private Queue<Collider> activationQueue = new Queue<Collider>();

	private Queue<Collider> deactivationQueue = new Queue<Collider>();

	private int dequeueBatchSize = 5;

	public int activated;

	public int activatedQueued;

	public int deactivated;

	public int deactivatedQueued;

	public void Activate(Collider collider)
	{
		bool flag = activationQueue.Count == 0;
		activationQueue.Enqueue(collider);
		activatedQueued++;
		if (flag)
		{
			StartCoroutine(DequeueCoroutine(activationQueue, _003CActivate_003Em__1D));
		}
	}

	public void Deactivate(Collider collider)
	{
		bool flag = deactivationQueue.Count == 0;
		deactivationQueue.Enqueue(collider);
		deactivatedQueued++;
		if (flag)
		{
			StartCoroutine(DequeueCoroutine(deactivationQueue, _003CDeactivate_003Em__1E));
		}
	}

	private IEnumerator DequeueCoroutine(Queue<Collider> queue, Action<Collider> action)
	{
		int count = dequeueBatchSize;
		while (queue.Count > 0)
		{
			if (count == 0)
			{
				yield return 0;
				count = dequeueBatchSize;
			}
			Collider collider = queue.Dequeue();
			action(collider);
			count--;
		}
	}

	public void Flush()
	{
		foreach (Collider item in activationQueue)
		{
			item.enabled = true;
			activated++;
		}
		activationQueue.Clear();
		foreach (Collider item2 in deactivationQueue)
		{
			item2.enabled = false;
			deactivated++;
		}
		deactivationQueue.Clear();
	}

	[CompilerGenerated]
	private void _003CActivate_003Em__1D(Collider c)
	{
		c.enabled = true;
		activated++;
	}

	[CompilerGenerated]
	private void _003CDeactivate_003Em__1E(Collider c)
	{
		c.enabled = false;
		deactivated++;
	}
}
