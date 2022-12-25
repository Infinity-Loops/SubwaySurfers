using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TrackChunkCollection
{
	[CompilerGenerated]
	private sealed class _003CMoveForward_003Ec__AnonStorey55
	{
		internal float z;

		internal bool _003C_003Em__31(TrackChunk trackChunk)
		{
			return trackChunk.zMaximum < z;
		}
	}

	public static List<TrackChunk> trackChunks = new List<TrackChunk>();

	private List<TrackChunk> activeTrackChunks = new List<TrackChunk>();

	private int lastAddedIndex = -1;

	private List<int> randomSpace = new List<int>();

	public TrackChunk[] TrackChunks
	{
		get
		{
			return trackChunks.ToArray();
		}
	}

	public static void AddToChunks(TrackChunk newTrackChunk)
	{
		int count = trackChunks.Count;
		if (count == 0)
		{
			trackChunks.Add(newTrackChunk);
			return;
		}
		int num = 0;
		while (trackChunks[num].zMinimum < newTrackChunk.zMinimum)
		{
			num++;
			if (num == count)
			{
				break;
			}
		}
		trackChunks.Insert(num, newTrackChunk);
	}

	public void Initialize(float z)
	{
		activeTrackChunks.Clear();
		lastAddedIndex = -1;
		for (int i = 0; i < trackChunks.Count; i++)
		{
			TrackChunk trackChunk = trackChunks[i];
			if (trackChunk.zMinimum <= z && z < trackChunk.zMaximum)
			{
				activeTrackChunks.Add(trackChunk);
				lastAddedIndex = i;
			}
		}
		Recalculate();
	}

	public void MoveForward(float z)
	{
		_003CMoveForward_003Ec__AnonStorey55 _003CMoveForward_003Ec__AnonStorey = new _003CMoveForward_003Ec__AnonStorey55();
		_003CMoveForward_003Ec__AnonStorey.z = z;
		int num = 0;
		for (int i = lastAddedIndex + 1; i < trackChunks.Count; i++)
		{
			TrackChunk trackChunk = trackChunks[i];
			if (trackChunk.zMinimum > _003CMoveForward_003Ec__AnonStorey.z)
			{
				break;
			}
			activeTrackChunks.Add(trackChunk);
			num++;
			lastAddedIndex = i;
		}
		int num2 = activeTrackChunks.RemoveAll(_003CMoveForward_003Ec__AnonStorey._003C_003Em__31);
		if (num > 0 || num2 > 0)
		{
			Recalculate();
		}
	}

	private void Recalculate()
	{
		randomSpace.Clear();
		for (int i = 0; i < activeTrackChunks.Count; i++)
		{
			TrackChunk trackChunk = activeTrackChunks[i];
			for (int j = 0; j < trackChunk.probability; j++)
			{
				randomSpace.Add(i);
			}
		}
	}

	public bool CanDeliver()
	{
		return randomSpace.Count > 0;
	}

	public TrackChunk GetRandomActive()
	{
		int index = Random.Range(0, randomSpace.Count);
		int index2 = randomSpace[index];
		return activeTrackChunks[index2];
	}

	public TrackChunk GetJetPakChunk(int index)
	{
		TrackChunk trackChunk = trackChunks[trackChunks.Count - 1 - index];
		if (trackChunk.zMaximum > 0f || trackChunk.zMinimum < 1000000f)
		{
			Debug.Log("Illegal TrackChunk used in jetpack mode. Index=" + index + " Name : " + trackChunk.name + " zmax : " + trackChunk.zMaximum + " zmin : " + trackChunk.zMinimum);
			Debug.Break();
		}
		return trackChunk;
	}
}
