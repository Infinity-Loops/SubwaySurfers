using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Track : MonoBehaviour
{
    public bool IsRunningOnTutorialTrack { get; set; }

    public void Awake()
    {
        this.trackSpacing = (this.trackRight.position - this.trackLeft.position).magnitude / (float)(this.numberOfTracks - 1);
        this.trackChunks = new TrackChunkCollection();
        this.hoverboard = Hoverboard.Instance;
        this.tutorial = !PlayerInfo.Instance.tutorialCompleted;
    }

    public Vector3 GetPosition(float x, float z)
    {
        return Vector3.forward * z + this.trackLeft.position + x * Vector3.right;
    }

    public float GetTrackX(int trackIndex)
    {
        return this.trackSpacing * (float)trackIndex;
    }

    public float LayJetpackChunks(float characterZ, float flyLength)
    {
        this.LayTracksUpTo(characterZ, flyLength, true);
        float num = this.trackChunkZ - characterZ;
        this.LayTrackChunk(this.jetpackLandingChunk);
        return num;
    }

    public void LayEmptyChunks(float characterZ, float removeDistance)
    {
        this.RemoveChunkObstacles(characterZ + removeDistance);
    }

    public void RemoveChunkObstacles(float removeDistance)
    {
        foreach (TrackChunk trackChunk in this.activeTrackChunks)
        {
            trackChunk.DeactivateObstacles(removeDistance);
        }
    }

    public void Initialize(float characterZ)
    {
        this.trackChunks.Initialize(characterZ);
    }

    public void LayTrackChunks(float characterZ)
    {
        this.LayTracksUpTo(characterZ, this.trackAheadDistance, false);
    }

    public void LayTracksUpTo(float characterZ, float trackAheadDistance, bool isJetpack)
    {
        if (!this.trackChunks.CanDeliver())
        {
            return;
        }
        float num = characterZ + trackAheadDistance;
        float num2 = 200f;
        Debug.DrawLine(Vector3.forward * num + Vector3.left * num2, Vector3.forward * num + -Vector3.left * num2, Color.white);
        if (this.trackChunkZ < num)
        {
            this.CleanupTrackChunks(characterZ);
        }
        int num3 = 0;
        while (this.trackChunkZ < num)
        {
            this.trackChunks.MoveForward(this.trackChunkZ);
            TrackChunk trackChunk;
            if (this.firstTrackChunk && this.tutorial)
            {
                trackChunk = this.tutorialTrackChunk;
                this.firstTrackChunk = false;
                if (trackChunk.CheckPoints.Count > 0)
                {
                    this.IsRunningOnTutorialTrack = true;
                }
                this.hoverboard.isAllowed = false;
            }
            else if (isJetpack)
            {
                trackChunk = this.trackChunks.GetJetPakChunk(num3);
                num3++;
            }
            else
            {
                trackChunk = this.trackChunks.GetRandomActive();
                int num4 = 0;
                while (this.activeTrackChunks.Contains(trackChunk) && num4 < 1000)
                {
                    trackChunk = this.trackChunks.GetRandomActive();
                    num4++;
                }
                if (num4 == 1000)
                {
                    Debug.Log("active track chunks");
                    Debug.Log("active: " + string.Join(", ", this.activeTrackChunks.ConvertAll<string>((TrackChunk chunk) => chunk.gameObject.name).ToArray()));
                    Debug.LogError("infinite loop. not track chunks to select.");
                }
            }
            this.LayTrackChunk(trackChunk);
        }
    }

    private void LayTrackChunk(TrackChunk trackChunk)
    {
        base.StartCoroutine(this.LayTrackChunkAsync(trackChunk));
    }

    private IEnumerator LayTrackChunkAsync(TrackChunk trackChunk)
    {
        trackChunk.gameObject.transform.position = Vector3.forward * this.trackChunkZ;
        this.trackChunkZ += trackChunk.zSize;
        this.activeTrackChunks.Add(trackChunk);
        trackChunk.RestoreHiddenObstacles();
        yield return base.StartCoroutine(this.PerformRecursiveSelection(trackChunk.gameObject, true));
        int i = 0;
        Array.Sort<TrackObject>(trackChunk.objects, (TrackObject g1, TrackObject g2) => g1.transform.position.z.CompareTo(g2.transform.position.z));
        foreach (TrackObject o in trackChunk.objects)
        {
            if (o.gameObject.active)
            {
                o.Activate();
                i++;
            }
            if (i == 1)
            {
                yield return null;
                i = 0;
            }
        }
        yield break;
    }

    private IEnumerator ActivateGameObjects(List<GameObject> objects)
    {
        objects.Sort((GameObject g1, GameObject g2) => g1.transform.position.z.CompareTo(g2.transform.position.z));
        int i = 0;
        foreach (GameObject gameObject in objects)
        {
            gameObject.active = true;
            i++;
            if (i == 4)
            {
                yield return null;
                i = 0;
            }
        }
        yield break;
    }

    private IEnumerator PerformRecursiveSelection(GameObject parent, bool sortSpawnPoints = true)
    {
        List<GameObject> objectsToActivate = new List<GameObject>();
        List<GameObject> objectsToVisit = new List<GameObject>();
        List<Track.SpawnPointWrapper> spawnPoints = new List<Track.SpawnPointWrapper>();
        objectsToVisit.Add(parent);
        while (objectsToVisit.Count > 0)
        {
            GameObject gameObject = objectsToVisit[0];
            objectsToVisit.RemoveAt(0);
            if (sortSpawnPoints)
            {
                SpawnPoint spawnPoint = gameObject.GetComponent<SpawnPoint>();
                if (spawnPoint != null)
                {
                    spawnPoints.Add(new Track.SpawnPointWrapper(spawnPoint));
                    continue;
                }
            }
            RandomizeOffset randomizeOffset = gameObject.GetComponent<RandomizeOffset>();
            if (randomizeOffset != null)
            {
                randomizeOffset.ChooseRandomOffset();
            }
            Transform gameObjectTransform = gameObject.transform;
            objectsToActivate.Add(gameObject);
            Selector selector = gameObject.GetComponent<Selector>();
            if (selector != null)
            {
                selector.PerformSelection(objectsToVisit);
            }
            else if (gameObject.GetComponent<Group>() == null)
            {
                for (int i = 0; i < gameObjectTransform.childCount; i++)
                {
                    GameObject child = gameObjectTransform.GetChild(i).gameObject;
                    objectsToVisit.Add(child);
                }
            }
        }
        List<GameObject> LowPriority = new List<GameObject>();
        LowPriority = objectsToActivate.Where((GameObject x) => this.IsLowPriority(x)).ToList<GameObject>();
        objectsToActivate = objectsToActivate.Where((GameObject x) => !this.IsLowPriority(x)).ToList<GameObject>();
        objectsToActivate.Sort((GameObject g1, GameObject g2) => g1.transform.position.z.CompareTo(g2.transform.position.z));
        LowPriority.Sort((GameObject g1, GameObject g2) => g1.transform.position.z.CompareTo(g2.transform.position.z));
        objectsToActivate.AddRange(LowPriority);
        int j = 0;
        foreach (GameObject gameObject2 in objectsToActivate)
        {
            gameObject2.active = true;
            j++;
            if (j == 4)
            {
                yield return null;
                j = 0;
            }
        }
        if (spawnPoints.Count > 0)
        {
            spawnPoints.Sort((Track.SpawnPointWrapper x, Track.SpawnPointWrapper y) => x.Z.CompareTo(y.Z));
            foreach (SpawnPoint spawnPoint2 in spawnPoints.ConvertAll<SpawnPoint>((Track.SpawnPointWrapper wrapper) => wrapper.SpawnPoint))
            {
                yield return base.StartCoroutine(this.PerformRecursiveSelection(spawnPoint2.gameObject, false));
            }
        }
        yield break;
    }

    private bool IsLowPriority(GameObject g)
    {
        return g.layer != 16;
    }

    public void CleanupTrackChunks(float characterZ)
    {
        float num = characterZ - this.cleanUpDistance;
        foreach (TrackChunk trackChunk in this.activeTrackChunks)
        {
            if (trackChunk.transform.position.z + trackChunk.zSize < num)
            {
                this.trackChunksForDeactivation.Add(trackChunk);
            }
        }
        foreach (TrackChunk trackChunk2 in this.trackChunksForDeactivation)
        {
            if (!trackChunk2.isTutorial)
            {
                trackChunk2.Deactivate();
            }
            this.activeTrackChunks.Remove(trackChunk2);
        }
        this.trackChunksForDeactivation.Clear();
    }

    public void DeactivateTrackChunks()
    {
        base.StopAllCoroutines();
        foreach (TrackChunk trackChunk in this.activeTrackChunks)
        {
            trackChunk.Deactivate();
        }
    }

    public void Restart()
    {
        foreach (TrackChunk trackChunk in this.trackChunks.TrackChunks)
        {
            Vector3 position = trackChunk.transform.position;
            position.y = -1000f;
            trackChunk.transform.position = position;
        }
        this.trackChunkZ = 0f;
        this.trackChunks.Initialize(0f);
        foreach (TrackChunk trackChunk2 in this.activeTrackChunks)
        {
            trackChunk2.Deactivate();
        }
        this.activeTrackChunks.Clear();
        this.firstTrackChunk = true;
    }

    private void TrackPositionGizmos()
    {
        for (int i = 0; i < this.numberOfTracks; i++)
        {
            Vector3 vector = Vector3.Lerp(this.trackLeft.position, this.trackRight.position, (float)i / (float)(this.numberOfTracks - 1));
            Gizmos.DrawLine(vector, vector + Vector3.forward * 5f);
        }
    }

    public void OnDrawGizmos()
    {
        this.TrackPositionGizmos();
    }

    public float GetLastCheckPoint(float characterZ)
    {
        foreach (TrackChunk trackChunk in this.activeTrackChunks)
        {
            if (this.IsRunningOnTutorialTrack && trackChunk == this.tutorialTrackChunk)
            {
                return trackChunk.GetLastCheckPoint(characterZ);
            }
        }
        Debug.Log("No checkpoints in track");
        return 0f;
    }

    public static Track Instance
    {
        get
        {
            Track track;
            if ((track = Track.instance) == null)
            {
                track = (Track.instance = UnityEngine.Object.FindObjectOfType(typeof(Track)) as Track);
            }
            return track;
        }
    }

    private const int ActiveTruePerFrame = 4;

    private const int ActivatePerFrame = 1;

    public Transform trackLeft;

    public Transform trackRight;

    public int numberOfTracks = 3;

    public float cleanUpDistance = 2000f;

    public float trackAheadDistance = 700f;

    public Transform levelChunksParent;

    public TrackChunk jetpackLandingChunk;

    public bool tutorial;

    public TrackChunk tutorialTrackChunk;

    private bool firstTrackChunk = true;

    private TrackChunkCollection trackChunks;

    private float trackSpacing;

    private float trackChunkZ;

    private List<TrackChunk> activeTrackChunks = new List<TrackChunk>(5);

    private List<TrackChunk> trackChunksForDeactivation = new List<TrackChunk>(5);

    private Hoverboard hoverboard;

    private static Track instance;

    private struct SpawnPointWrapper
    {
        public SpawnPointWrapper(SpawnPoint spawnPoint)
        {
            this.SpawnPoint = spawnPoint;
            this.Z = spawnPoint.transform.position.z;
        }

        public SpawnPoint SpawnPoint;

        public float Z;
    }
}
