using System.Collections;
using UnityEngine;

public class FishPool : MonoBehaviour
{
   [SerializeField] GameObject fishPrefab; // TODO make this an array, get more fish!? (Low priority).

   private Fish[] fishes;
   private FishSpawn[] spawnPoints;
   private int dynamicPoolSize;
   private int spawnPercent;
   private int updatedFrameRate;
   private Transform xform;

   private struct Fish
   {
      public bool on;
      public float onTime;
      public GameObject fishObject;
      public int poolIndex; // TODO depreciate this?
   }

   private void Start()
   {
      spawnPoints = GetComponentsInChildren<FishSpawn>();
      dynamicPoolSize = SpawnCap;
      fishes = new Fish[SpawnCap];
      StartCoroutine(TunedSpawn());
   }

   private void CorrectPoolSize()
   {
      int newTarget = SpawnCap - CountPool;
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

   private bool CountFull { get { return (CountActive == SpawnCap); } }
   private bool CountOver { get { return (CountPool > SpawnCap); } }
   private bool CountUnder { get { return (CountActive < SpawnCap); } }
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
      ///Debug.Log(debugString);
   }

   public void PartialSpawn()
   {
      if (!CountFull) CorrectPoolSize();

      int delta = CountActive - SpawnCap;
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
      if (SpawnCap == CountActive) return;

      int poolIndex = 0;
      while (fishes[poolIndex].on)
      {
         poolIndex++;
         if (poolIndex >= dynamicPoolSize) poolIndex = 0;
      }
      RecycleFish(xform, poolIndex);
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

   public void ReclaimAllFish()
   {
      for (int i = 0; i < fishes.Length; i++)
      {
         if (fishes[i].on)
         {
            fishes[i].on = false;
            fishes[i].fishObject.SetActive(false);
            fishes[i].fishObject.transform.parent = this.transform;
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

   public void Reset() { Respawn(); }

   public void Respawn()
   {
      ReclaimAllFish();
      StartCoroutine(TunedSpawn());
   }

   private void Spawn()
   {
      for (int i = 0; i < SpawnCap; i++)
      {
         var rft = RandomFreeTransform();
         if (rft) PlaceFish(rft);
         else return;
      }
   }

   private int SpawnCap { get { return Mathf.FloorToInt(spawnPoints.Length * (spawnPercent / 100f)); } }

   public IEnumerator TunedSpawn()
   {
      // Local setup.
      float spawnFactor = 1.555f;   // Fudge-factor for spawnrate/framerate conversion ratio.
      int frameGap = 2;             // Must be 1 or greater. # of frames to skip between samples.
      int testSamples = 4;          // # of sample framerates to use to calculate averageFrameRate.

      // Set spawnPercent based on FrameRate.
      int averageFrameRate = 0;
      int divisor = testSamples;
      while (testSamples > 0)
      {
         yield return StartCoroutine(WaitFor.Frames(frameGap));
         averageFrameRate += FrameRate;
         testSamples--;
      }
      spawnPercent = Bound(Mathf.FloorToInt((averageFrameRate/divisor) * spawnFactor)); 
      Debug.Log("FishPool.cs:TunedSpawn() spawnPercent = " + spawnPercent);
      
      // Grow pool when needed, and spawn.
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