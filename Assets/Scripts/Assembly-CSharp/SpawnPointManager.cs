using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SpawnPointManager
{
	private class PickupType
	{
		public Func<SpawnPoint, GameObject> ExtractGameObject;

		public float spawnProbability;

		public float spawnDistanceMin;

		public float spawnZ;
	}

	[CompilerGenerated]
	private sealed class _003CPerformSelection_003Ec__AnonStorey53
	{
		internal float z;

		internal bool _003C_003Em__25(PickupType p)
		{
			return p.spawnZ < z;
		}
	}

	private static SpawnPointManager instance;

	private PickupType dailyLetter;

	private PickupType doubleScoreMultiplier;

	private PickupType jetpackPickup;

	private PickupType jumpBooster;

	private PickupType magnetBooster;

	private PickupType mysteryBox;

	private PickupType[] pickups;

	private float spawnZ;

	private float spawnSpacing;

	private float totalProbability;

	private float[] accumulatedProbability;

	[CompilerGenerated]
	private static Func<SpawnPoint, GameObject> _003C_003Ef__am_0024cacheC;

	[CompilerGenerated]
	private static Func<SpawnPoint, GameObject> _003C_003Ef__am_0024cacheD;

	[CompilerGenerated]
	private static Func<SpawnPoint, GameObject> _003C_003Ef__am_0024cacheE;

	[CompilerGenerated]
	private static Func<SpawnPoint, GameObject> _003C_003Ef__am_0024cacheF;

	[CompilerGenerated]
	private static Func<SpawnPoint, GameObject> _003C_003Ef__am_0024cache10;

	[CompilerGenerated]
	private static Func<SpawnPoint, GameObject> _003C_003Ef__am_0024cache11;

	public static SpawnPointManager Instance
	{
		get
		{
			return instance ?? (instance = new SpawnPointManager());
		}
	}

	public SpawnPointManager()
	{
		float distancePerMeter = Game.Instance.distancePerMeter;
		Upgrade upgrade = Upgrades.upgrades[PowerupType.letters];
		dailyLetter = new PickupType();
		dailyLetter.spawnDistanceMin *= distancePerMeter;
		dailyLetter.spawnProbability = upgrade.spawnProbability;
		PickupType pickupType = dailyLetter;
		if (_003C_003Ef__am_0024cacheC == null)
		{
			_003C_003Ef__am_0024cacheC = _003CSpawnPointManager_003Em__1F;
		}
		pickupType.ExtractGameObject = _003C_003Ef__am_0024cacheC;
		Upgrade upgrade2 = Upgrades.upgrades[PowerupType.doubleMultiplier];
		doubleScoreMultiplier = new PickupType();
		doubleScoreMultiplier.spawnDistanceMin = (float)upgrade2.minimumMeters * distancePerMeter;
		doubleScoreMultiplier.spawnProbability = upgrade2.spawnProbability;
		PickupType pickupType2 = doubleScoreMultiplier;
		if (_003C_003Ef__am_0024cacheD == null)
		{
			_003C_003Ef__am_0024cacheD = _003CSpawnPointManager_003Em__20;
		}
		pickupType2.ExtractGameObject = _003C_003Ef__am_0024cacheD;
		Upgrade upgrade3 = Upgrades.upgrades[PowerupType.jetpack];
		jetpackPickup = new PickupType();
		jetpackPickup.spawnDistanceMin = (float)upgrade3.minimumMeters * distancePerMeter;
		jetpackPickup.spawnProbability = upgrade3.spawnProbability;
		PickupType pickupType3 = jetpackPickup;
		if (_003C_003Ef__am_0024cacheE == null)
		{
			_003C_003Ef__am_0024cacheE = _003CSpawnPointManager_003Em__21;
		}
		pickupType3.ExtractGameObject = _003C_003Ef__am_0024cacheE;
		Upgrade upgrade4 = Upgrades.upgrades[PowerupType.supersneakers];
		jumpBooster = new PickupType();
		jumpBooster.spawnDistanceMin = (float)upgrade4.minimumMeters * distancePerMeter;
		jumpBooster.spawnProbability = upgrade4.spawnProbability;
		PickupType pickupType4 = jumpBooster;
		if (_003C_003Ef__am_0024cacheF == null)
		{
			_003C_003Ef__am_0024cacheF = _003CSpawnPointManager_003Em__22;
		}
		pickupType4.ExtractGameObject = _003C_003Ef__am_0024cacheF;
		Upgrade upgrade5 = Upgrades.upgrades[PowerupType.coinmagnet];
		magnetBooster = new PickupType();
		magnetBooster.spawnDistanceMin = (float)upgrade5.minimumMeters * distancePerMeter;
		magnetBooster.spawnProbability = upgrade5.spawnProbability;
		PickupType pickupType5 = magnetBooster;
		if (_003C_003Ef__am_0024cache10 == null)
		{
			_003C_003Ef__am_0024cache10 = _003CSpawnPointManager_003Em__23;
		}
		pickupType5.ExtractGameObject = _003C_003Ef__am_0024cache10;
		Upgrade upgrade6 = Upgrades.upgrades[PowerupType.mysterybox];
		mysteryBox = new PickupType();
		mysteryBox.spawnDistanceMin = (float)upgrade6.minimumMeters * distancePerMeter;
		mysteryBox.spawnProbability = upgrade6.spawnProbability;
		PickupType pickupType6 = mysteryBox;
		if (_003C_003Ef__am_0024cache11 == null)
		{
			_003C_003Ef__am_0024cache11 = _003CSpawnPointManager_003Em__24;
		}
		pickupType6.ExtractGameObject = _003C_003Ef__am_0024cache11;
		pickups = new PickupType[6] { dailyLetter, doubleScoreMultiplier, jetpackPickup, jumpBooster, magnetBooster, mysteryBox };
	}

	public void PerformSelection(SpawnPoint spawnPoint, List<GameObject> objectsToVisit)
	{
		_003CPerformSelection_003Ec__AnonStorey53 _003CPerformSelection_003Ec__AnonStorey = new _003CPerformSelection_003Ec__AnonStorey53();
		_003CPerformSelection_003Ec__AnonStorey.z = spawnPoint.transform.position.z;
		PickupType pickupType = null;
		if (_003CPerformSelection_003Ec__AnonStorey.z > spawnZ)
		{
			List<PickupType> list = new List<PickupType>(pickups).FindAll(_003CPerformSelection_003Ec__AnonStorey._003C_003Em__25);
			if (list.Count > 0)
			{
				float[] array = new float[list.Count];
				float num = 0f;
				for (int i = 0; i < list.Count; i++)
				{
					num = (array[i] = num + list[i].spawnProbability);
				}
				float num2 = UnityEngine.Random.Range(0f, num);
				for (int j = 0; j < array.Length; j++)
				{
					if (num2 < array[j])
					{
						pickupType = list[j];
						pickupType.spawnZ = _003CPerformSelection_003Ec__AnonStorey.z + pickupType.spawnDistanceMin;
						break;
					}
				}
				spawnZ = _003CPerformSelection_003Ec__AnonStorey.z + spawnSpacing;
			}
		}
		for (int k = 0; k < pickups.Length; k++)
		{
			PickupType pickupType2 = pickups[k];
			GameObject gameObject = pickupType2.ExtractGameObject(spawnPoint);
			if (pickupType2 == pickupType)
			{
				objectsToVisit.Add(gameObject);
			}
			else
			{
				gameObject.SetActiveRecursively(false);
			}
		}
	}

	public void Restart()
	{
		float distancePerMeter = Game.Instance.distancePerMeter;
		spawnZ = Upgrades.UpgradeFirstSpawnMeters * distancePerMeter;
		spawnSpacing = Upgrades.UpgradeSpawnSpacingMeters * distancePerMeter;
		PickupType[] array = pickups;
		foreach (PickupType pickupType in array)
		{
			pickupType.spawnZ = float.MinValue;
		}
	}

	[CompilerGenerated]
	private static GameObject _003CSpawnPointManager_003Em__1F(SpawnPoint spawnPoint)
	{
		return spawnPoint.dailyLetter;
	}

	[CompilerGenerated]
	private static GameObject _003CSpawnPointManager_003Em__20(SpawnPoint spawnPoint)
	{
		return spawnPoint.doubleScoreMultiplier;
	}

	[CompilerGenerated]
	private static GameObject _003CSpawnPointManager_003Em__21(SpawnPoint spawnPoint)
	{
		return spawnPoint.jetpackPickup;
	}

	[CompilerGenerated]
	private static GameObject _003CSpawnPointManager_003Em__22(SpawnPoint spawnPoint)
	{
		return spawnPoint.jumpBooster;
	}

	[CompilerGenerated]
	private static GameObject _003CSpawnPointManager_003Em__23(SpawnPoint spawnPoint)
	{
		return spawnPoint.magnetBooster;
	}

	[CompilerGenerated]
	private static GameObject _003CSpawnPointManager_003Em__24(SpawnPoint spawnPoint)
	{
		return spawnPoint.mysteryBox;
	}
}
