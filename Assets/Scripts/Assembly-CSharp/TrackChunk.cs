using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TrackChunk : MonoBehaviour
{
	[Serializable]
	public class TrackCheckPoint
	{
		public int TrackNumber;

		public float Z;
	}

	[CompilerGenerated]
	private sealed class _003CGetLastCheckPoint_003Ec__AnonStorey54
	{
		internal float characterZ;

		internal bool _003C_003Em__30(TrackCheckPoint c)
		{
			return c.Z <= characterZ;
		}
	}

	public float zSize = 40f;

	public int probability = 1;

	public float zMinimum;

	public bool zMaximumActive;

	public float zMaximum;

	public List<TrackCheckPoint> CheckPoints;

	public TrackObject[] objects;

	public bool wasDisabledDueToHoverBoard;

	public bool isTutorial;

	private Dictionary<Transform, Vector3> hiddenObstacles = new Dictionary<Transform, Vector3>();

	[CompilerGenerated]
	private static Func<TrackCheckPoint, float> _003C_003Ef__am_0024cacheA;

	public void Awake()
	{
		objects = GetComponentsInChildren<TrackObject>(true);
		if (!zMaximumActive)
		{
			zMaximum = float.MaxValue;
		}
		TrackChunkCollection.AddToChunks(this);
	}

	public void Deactivate()
	{
		TrackObject[] array = objects;
		foreach (TrackObject trackObject in array)
		{
			trackObject.Deactivate();
		}
	}

	public void DeactivateObstacles(float maxZ)
	{
		wasDisabledDueToHoverBoard = true;
		foreach (Transform item in base.transform)
		{
			DeactiveObstaclesRecursive(item, maxZ);
		}
	}

	private void DeactiveObstaclesRecursive(Transform target, float maxZ)
	{
		float num = ((!(target.GetComponent<Collider>() != null)) ? target.transform.position.z : target.GetComponent<Collider>().bounds.min.z);
		if (target.GetComponent<SnapObject>() == null)
		{
			foreach (Transform item in target)
			{
				DeactiveObstaclesRecursive(item, maxZ);
			}
			return;
		}
		if (num < maxZ && target.gameObject.layer != 16)
		{
			Vector3 localPosition = target.localPosition;
			if (!hiddenObstacles.ContainsKey(target))
			{
				hiddenObstacles.Add(target, localPosition);
			}
			target.localPosition = new Vector3(localPosition.x, -1000f, localPosition.z);
		}
	}

	public void RestoreHiddenObstacles()
	{
		foreach (KeyValuePair<Transform, Vector3> hiddenObstacle in hiddenObstacles)
		{
			hiddenObstacle.Key.localPosition = hiddenObstacle.Value;
		}
		hiddenObstacles.Clear();
	}

	public float GetLastCheckPoint(float characterZ)
	{
		_003CGetLastCheckPoint_003Ec__AnonStorey54 _003CGetLastCheckPoint_003Ec__AnonStorey = new _003CGetLastCheckPoint_003Ec__AnonStorey54();
		_003CGetLastCheckPoint_003Ec__AnonStorey.characterZ = characterZ;
		List<TrackCheckPoint> checkPoints = CheckPoints;
		if (_003C_003Ef__am_0024cacheA == null)
		{
			_003C_003Ef__am_0024cacheA = _003CGetLastCheckPoint_003Em__2F;
		}
		TrackCheckPoint trackCheckPoint = checkPoints.OrderBy(_003C_003Ef__am_0024cacheA).Where(_003CGetLastCheckPoint_003Ec__AnonStorey._003C_003Em__30).LastOrDefault();
		if (trackCheckPoint == null)
		{
			Debug.Log(" No checkpoint found in track chunk");
			return 0f;
		}
		return trackCheckPoint.Z;
	}

	private void DrawCheckPointGizmos()
	{
		foreach (TrackCheckPoint checkPoint in CheckPoints)
		{
			Vector3 position = base.transform.position;
			position.z = checkPoint.Z;
			Gizmos.DrawSphere(position + Vector3.up * 5f, 5f);
		}
	}

	public void OnDrawGizmos()
	{
		DrawCheckPointGizmos();
	}

	[CompilerGenerated]
	private static float _003CGetLastCheckPoint_003Em__2F(TrackCheckPoint c)
	{
		return c.Z;
	}
}
