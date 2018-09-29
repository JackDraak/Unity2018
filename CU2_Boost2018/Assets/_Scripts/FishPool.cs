using System.Collections;
using UnityEngine;

public class FishPool : MonoBehaviour
{
   // TODO -- setup a NavMesh and use it to help define a "spawn area" along with the AI boundary colliders and scenery colliders?

   [SerializeField] GameObject fishPrefab; // TODO make this an array, get more fish!? (Low priority).
   private int spawnPercent;

   struct Fish
   {
      public bool on;
      public float onTime;
      public GameObject fishObject;
      public int poolIndex;
   }

   private Fish[] fishes;
   private FishSpawn[] spawnPoints;
   private int dynamicPoolSize;
   private int updatedFrameRate;
   private Transform xform;

   private void Start()
   {
      spawnPoints = GetComponentsInChildren<FishSpawn>();
      dynamicPoolSize = SpawnTarget;
      fishes = new Fish[SpawnTarget];
      StartCoroutine(TunedSpawn());
   }

   private void Update()
   {
      PollDebug();
   }

   private void CorrectPoolSize()
   {
      int newTarget = SpawnTarget - CountPool;
      if (newTarget > 0) GrowPool(newTarget);
   }

   private int CountActive
   {
      get
      {
         int active = 0;
         foreach (Fish fish in fishes) if (fish.on) active++;
         return active;
      }
   }

   private bool CountFull { get { return (CountActive == SpawnTarget); } }
   private bool CountOver { get { return (CountPool > SpawnTarget); } }
   private bool CountUnder { get { return (CountActive < SpawnTarget); } }
   private int CountPool { get { return fishes.Length; } }

   private void CreateFishObject(int index)
   {
      fishes[index].fishObject = Instantiate(fishPrefab, transform.position, Quaternion.identity, transform);
      fishes[index].fishObject.SetActive(false);
      fishes[index].on = false;
      fishes[index].onTime = 0;
      fishes[index].poolIndex = index;
   }

   private int FrameRate { get { return (int)(1.0f / Time.smoothDeltaTime); } }

   private void GrowPool(int delta)
   {
      // Place current array in temp storage.
      Fish[] temp = new Fish[dynamicPoolSize];
      for (int i = 0; i < dynamicPoolSize; i++) temp[i] = fishes[i];
      dynamicPoolSize += delta;

      // Copy from temp into newer, larger array.
      fishes = new Fish[dynamicPoolSize];
      for (int i = 0; i < dynamicPoolSize - delta; i++) fishes[i] = temp[i];

      // Create next pool fish(es) in the series.
      for (int i = 0; i < delta; i++) CreateFishObject(i + dynamicPoolSize - delta);

      //Debug info.
      string debugString = "FishPool GrowPool() execution result, +" + delta;
      if (delta == 1) debugString += ". Added fish #: " + dynamicPoolSize;
      else debugString += ". Added fish #'s: " + (dynamicPoolSize - delta + 1) + "-" + dynamicPoolSize;
      Debug.Log(debugString);
   }

   private void PartialSpawn()
   {
      if (!CountFull) CorrectPoolSize();

      int delta = CountActive - SpawnTarget;
      while (delta < 0)
      {
         var rft = RandomFreeTransform();
         if (rft)
         {
            PlaceFish(rft);
            delta++;
         }
      }
      Debug.Log("FishPool PartialSpawn() pool size: " + CountPool + ". Total active count: " + CountActive);
   }

   private void PlaceFish(Transform xform)
   {
      if (SpawnTarget == CountActive) return;

      int poolIndex = 0;
      while (fishes[poolIndex].on)
      {
         poolIndex++;
         if (poolIndex >= dynamicPoolSize) poolIndex = 0;
         // TODO do/don't spawn fish that are "in view"?
         // maybe have fishdrone script check when it's turned on if it's in view and delay or something....
      }
      RecycleFish(xform, poolIndex);
   }

   private void PollDebug()
   {
      if (Input.GetKeyDown(KeyCode.K) && Debug.isDebugBuild) ReclaimAllFish();
      if (Input.GetKeyDown(KeyCode.L) && Debug.isDebugBuild) Respawn();
      if (Input.GetKeyDown(KeyCode.J) && Debug.isDebugBuild) PartialSpawn();
   }

   private Transform RandomFreeTransform()
   {
      Transform[] emptySpawnPoints = new Transform[spawnPoints.Length];
      int emptyCount = 0;
      foreach (FishSpawn spawnPoint in spawnPoints)
      {
         if (spawnPoint.transform.childCount == 0)
         {
            emptySpawnPoints[emptyCount] = spawnPoint.transform;
            emptyCount++;
         }
      }
      if (emptyCount > 0) return emptySpawnPoints[Random.Range(0, emptyCount)];
      else return null;
   }

   private void ReclaimAllFish()
   {
      int reclaimCount = 0;
      for (int i = 0; i < fishes.Length; i++)
      {
         if (fishes[i].on)
         {
            fishes[i].on = false;
            fishes[i].fishObject.SetActive(false);
            fishes[i].fishObject.transform.parent = this.transform;
            reclaimCount++;
         }
      }
   }

   private void RecycleFish(Transform xform, int poolIndex)
   {
      fishes[poolIndex].on = true;
      fishes[poolIndex].onTime = Time.time;
      fishes[poolIndex].fishObject.transform.parent = xform;
      fishes[poolIndex].fishObject.transform.position = xform.position;
      fishes[poolIndex].fishObject.SetActive(true);
   }

   public void Reset()
   {
      Respawn();
   }

   private void Respawn()
   {
      ReclaimAllFish();
      StartCoroutine(TunedSpawn());
   }

   private void Spawn()
   {
      int spawnCountRFT = 0;
      for (int i = 0; i < SpawnTarget; i++)
      {
         var rft = RandomFreeTransform();
         if (rft)
         {
            PlaceFish(rft);
            spawnCountRFT++;
         }
      }
      //Debug.Log("FishPool Spawn() RandomFreeTransform's requested: " + spawnCountRFT +
      //   ". Total active count: " + CountActive +
      //   ". Pool size: " + fishes.Length +
      //   ". Available spawn points (net): " + spawnPoints.Length);
   }

   private int SpawnTarget
   {
      get { return Mathf.FloorToInt(spawnPoints.Length * (spawnPercent / 100f)); }
   }

   public IEnumerator TunedSpawn()
   {
      // Local setup.
      float spawnFactor = 1.555f;   // Fudge-factor for spawnrate/framerate conversion ratio.
      int frameGap = 2;             // Must be 1 or greater. # of frames to skip between samples.
      int testSamples = 4;          // # of sample framerates to use to calculate averageFrameRate.

      // Loop setup.
      int averageFrameRate = 0;
      int divisor = testSamples;
      while (testSamples > 0)
      {
         yield return StartCoroutine(WaitFor.Frames(frameGap));
         averageFrameRate += FrameRate;
         testSamples--;
      }
      // Will the real spawnRate, please stand up!?
      spawnPercent = Bound(Mathf.FloorToInt((averageFrameRate/divisor) * spawnFactor)); 
      Debug.Log("FishPool.cs:TunedSpawn() spawnPercent = " + spawnPercent);
                          
      CorrectPoolSize();
      Spawn();
   }

   private int Bound(int test)
   {
      if (test > 100) test = 100;
      else if (test < 0) test = 0;
      return test;
   }

   // Some other ways to overload Bound, for fun.. not presently being used
   private int Bound(int low, int high, int test)
   {
      if (test > high) test = high;
      else if (test < low) test = low;
      return test;
   }

   private float Bound(float low, float high, float test)
   {
      if (test > high) test = high;
      else if (test < low) test = low;
      return test;
   }
}

public static class WaitFor
{
   public static IEnumerator Frames(int frameCount)
   {
      while (frameCount > 0)
      {
         frameCount--;
         yield return null;
      }
   }
}

