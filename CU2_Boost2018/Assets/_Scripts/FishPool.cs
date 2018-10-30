using System.Collections;
using UnityEngine;

public class FishPool : MonoBehaviour
{
   [SerializeField] GameObject fishPrefab; // TODO make this an array, get more fish!? (Low priority).

   Fish[] fishes;
   FishSpawn[] spawnPoints;
   int dynamicPoolSize;
   int spawnPercent;
   int updatedFrameRate;
   Transform xform;

   struct Fish
   {
      public bool on;
      public float onTime;
      public GameObject fishObject;
   }

   int Bound(int test)
   {
      if (test > 100) test = 100;
      else if (test < 0) test = 0;
      return test;
   }

   void CorrectPoolSize()
   {
      int newTarget = SpawnCap - CountPool;
      if (newTarget > 0) GrowPool(newTarget);
   }

   int CountActive
   {
      get
      {
         int active = 0;
         foreach (Fish fish in fishes) if (fish.on) active++;
         return active;
      }
   }

   bool CountFull { get { return (CountActive == SpawnCap); } }
   bool CountOver { get { return (CountPool > SpawnCap); } }
   int CountPool { get { return fishes.Length; } }
   bool CountUnder { get { return (CountActive < SpawnCap); } }

   void CreateFishObject(int index)
   {
      fishes[index].fishObject = Instantiate(fishPrefab, transform.position, Quaternion.identity, transform);
      fishes[index].fishObject.SetActive(false);
      fishes[index].on = false;
      fishes[index].onTime = 0;
   }

   int FrameRate { get { return (int)(1.0f / Time.smoothDeltaTime); } }

   void GrowPool(int delta)
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

   void PlaceFish(Transform xform)
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

   Transform RandomFreeTransform()
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

   void RecycleFish(Transform xform, int poolIndex)
   {
      fishes[poolIndex].on = true;
      fishes[poolIndex].onTime = Time.time;
      fishes[poolIndex].fishObject.transform.parent = xform;
      fishes[poolIndex].fishObject.transform.position = xform.position;
      fishes[poolIndex].fishObject.SetActive(true);
      fishes[poolIndex].fishObject.GetComponent<FishDrone>().SetID(poolIndex); // For debugging/interest.
   }

   public void Reset() { Respawn(); }

   public void Respawn()
   {
      ReclaimAllFish();
      StartCoroutine(TunedSpawn());
   }

   void Spawn()
   {
      for (int i = 0; i < SpawnCap; i++)
      {
         var rft = RandomFreeTransform();
         if (rft) PlaceFish(rft);
         else return;
      }
   }

   int SpawnCap { get { return Mathf.FloorToInt(spawnPoints.Length * (spawnPercent / 100f)); } }

   void Start()
   {
      spawnPoints = GetComponentsInChildren<FishSpawn>();
      dynamicPoolSize = SpawnCap;
      fishes = new Fish[SpawnCap];
      StartCoroutine(TunedSpawn());
   }

   public IEnumerator TunedSpawn()
   {
      // Local setup.
      float spawnFactor = 1.222f;   // Fudge-factor for spawnrate/framerate conversion ratio.
      int frameGap = 3;             // Must be 1 or greater. # of frames to skip between samples.
      int testSamples = 5;          // # of sample framerates to use to calculate averageFrameRate.

      // Set spawnPercent based on FrameRate.
      int averageFrameRate = 0;
      int divisor = testSamples;
      while (testSamples > 0)
      {
         yield return WaitFor.Frames(frameGap);
         averageFrameRate += FrameRate;
         testSamples--;
      }
      spawnPercent = Bound(Mathf.FloorToInt((averageFrameRate/divisor) * spawnFactor)); 
      Debug.Log("FishPool.cs:TunedSpawn() spawnPercent = " + spawnPercent);
      
      // Grow pool when needed, and spawn.
      CorrectPoolSize();
      Spawn();
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
