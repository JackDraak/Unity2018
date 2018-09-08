using UnityEngine;

public class FishPool : MonoBehaviour
{
   // TODO this is being used as a bootleg way to keep te play area 'alive', as the fish have a tendency to wander.
   // ... perhaps it would be smart to improve their behaviour in general? collision-avoidance? group-behaviours? Staying in bounds?
   [Tooltip("Lifespan in seconds (0 = do not expire)")]
   [SerializeField] float fishLifeMax = 300;
   [Tooltip("Time before Max Life when fish *may* expire, in seconds [should be 0 or smaller value than fishLifeMin]")]
   [SerializeField] float fishLifeWindow = 60;

   [Tooltip("Allow spawn to populate without a Reset() when true")]
   [SerializeField] bool dynamicSpawn = false;

   [Space(10)][SerializeField] GameObject fishPrefab; // TODO make this an array, get more fish!?
   [Tooltip("Percentage of Spawn Points to Populate")]
   [Range(0, 100)][SerializeField] int spawnPercent = 50;

   struct Fish
   {
      public bool on;
      public float onTime;
      public GameObject fishObject;
      public int poolIndex;
      public Transform xform;
   }

   private Fish[] fishes;
   private FishSpawn[] spawnPoints;
   private int dynamicPoolSize;
   private Transform xform;

   private int SpawnTarget
   {
      get { return Mathf.FloorToInt(spawnPoints.Length * (spawnPercent / 100f)); }
      set { } // Read-only value.
   }

   private void Start()
   {
      if (fishLifeWindow > fishLifeMax)
      {
         Debug.Log("ERROR Fish Life Window MUST be a smaller value that Fish Life Minimum. " +
            "Setting both to zero. Please correct in the inspector.");
         fishLifeWindow = 0;
         fishLifeMax = 0;
      }
      spawnPoints = GetComponentsInChildren<FishSpawn>();
      Debug.Log(spawnPoints.Length + " FishPool spawnPoints identified.");

      dynamicPoolSize = SpawnTarget;
      fishes = new Fish[SpawnTarget];

      // Build initial pool & populate.
      for (int i = 0; i < SpawnTarget; i++) CreateFishObject(i);
      Spawn();
   }

   private void Update()
   {
      PollDebug();
      ExpireFish();
      if (dynamicSpawn && CountUnder()) PartialSpawn();
   }

   private void CorrectPoolSize()
   {   
      // Grow pool to min required size automagically.
      int newTarget = SpawnTarget - CountAll();
      if (newTarget > 0) GrowPool(newTarget);
      // TODO shrink overfull pools?
   }

   // Return number of fish active in the scene.
   private int CountActive()
   {
      int active = 0;
      foreach (Fish fish in fishes) if (fish.on) active++;
      return active;
   }

   private int CountAll() { return fishes.Length; }                        // Returns size of FishPool.
   private bool CountExceeded() { return (CountActive() > SpawnTarget); }  // True when fish# active in scene exceeds SpawnTarget#.
   private bool CountFull() { return (CountActive() == SpawnTarget); }     // True when SpawnTarget# fish are active in scene.
   private bool CountUnder() { return (CountActive() < SpawnTarget); }     // True when fish# active in scene are fewer than SpawnTarget#

   private void CreateFishObject(int i)
   {
      fishes[i].fishObject = Instantiate(fishPrefab, transform.position, Quaternion.identity, transform);
      fishes[i].fishObject.SetActive(false);
      fishes[i].on = false;
      fishes[i].onTime = 0;
      fishes[i].poolIndex = i;
      fishes[i].xform = transform;
   }

   private void CycleInactive()
   {
      for (int i = 0; i < fishes.Length; i++)
      {
         if (!fishes[i].on)
         {
            fishes[i].fishObject.transform.parent = this.transform;
            fishes[i].fishObject.SetActive(false);
         }
      }
   }

   private void CycleLifespan()
   {
      if (fishLifeWindow + fishLifeMax != 0)
      {
         for (int i = 0; i < fishes.Length; i++)
         {
            if (fishes[i].on)
            {
               if (Time.time > fishes[i].onTime)
               {
                  fishes[i].on = false;
                  fishes[i].fishObject.SetActive(false);
               }
            }
         }
      }
   }

   private void Despawn()
   {
      int active = CountActive();
      int targetDelta = active - SpawnTarget;
      while (targetDelta > 0)
      {
         fishes[active].on = false; // CycleInactive does the rest of the work...
         active--;
         targetDelta--;
      }
   }

   private void ExpireFish()
   {
      CycleLifespan();
      CycleInactive();
   }

   private void GrowPool(int growRate)
   {
      // Place current array in temp storage.
      Fish[] temp = new Fish[dynamicPoolSize];
      for (int i = 0; i < dynamicPoolSize; i++) temp[i] = fishes[i];
      dynamicPoolSize += growRate;

      // Copy from temp into newer, larger array.
      fishes = new Fish[dynamicPoolSize];
      for (int i = 0; i < dynamicPoolSize - growRate; i++) fishes[i] = temp[i];

      // Create next pool fish(es) in the series.
      for (int i = 0; i < growRate; i++) CreateFishObject(i + dynamicPoolSize - growRate);
      string debugString = "Pool Expansion Performed, +" + growRate;
      if (growRate == 1) debugString += ". Added fish #: " + dynamicPoolSize;
      else debugString += ". Added fish #'s: " + (dynamicPoolSize - growRate + 1) + "-" + dynamicPoolSize;
   }

   private void PartialSpawn()
   {
      // grow pool, if needed
      if (!CountFull()) CorrectPoolSize();

      int delta = CountActive() - SpawnTarget;
      Debug.Log("PartialSpawn delta: " + delta);
      while (delta < 0)
      {
         var rft = RandomFreeTransform();
         if (rft)
         {
            PlaceFish(rft);
            delta++;
         }
      }
      Debug.Log("Fishes PartialSpawn() TOTAL POOL: " + CountAll() + ". Total active count: " + CountActive());
   }

   private void PlaceFish(Transform xform)
   {
      if (SpawnTarget == CountActive()) return;

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
      int inCount = 0;
      foreach (FishSpawn spawnPoint in spawnPoints)
      {
         if (spawnPoint.transform.childCount == 0)
         {
            emptySpawnPoints[inCount] = spawnPoint.transform;
            inCount++;
         }
      }
      if (inCount > 0) return emptySpawnPoints[Random.Range(0, inCount)];
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
      Debug.Log("Fishes reclaimed: " + reclaimCount + ". Total active count: " + CountActive());
   }

   private void RecycleFish(Transform xform, int poolIndex)
   {
      fishes[poolIndex].on = true;
      if (fishLifeMax + fishLifeWindow != 0) fishes[poolIndex].onTime = Time.time + fishLifeMax - Random.Range(0, fishLifeWindow);
      else fishes[poolIndex].onTime = Time.time;
      fishes[poolIndex].fishObject.transform.parent = xform;
      fishes[poolIndex].fishObject.transform.position = xform.position;
      fishes[poolIndex].fishObject.SetActive(true);
      fishes[poolIndex].xform = xform;
   }

   private int RePlaceFish(int spawnCount) // TODO depreciated.
   {
      Debug.Log("ANACHRONISTIC CALL WARNING RePlaceFish()");
      for (int n = fishes.Length; n >= 0; n--)
      {
         if (!fishes[n].on)
         {
            fishes[n].on = true;
            fishes[n].fishObject.SetActive(true);
            spawnCount++;
         }
      }
      return spawnCount;
   }

   public void Reset()
   {
      Respawn();
   }

   private void Respawn()
   {
      ReclaimAllFish();
      CorrectPoolSize();
      int respawnCount = 0;
      int target = Mathf.FloorToInt(spawnPoints.Length * (spawnPercent / 100f));
      for (int i = 0; i < target; i++)
      {
         RecycleFish(RandomFreeTransform(), i);
         respawnCount++;
      }
      Debug.Log("Fishes respawned: " + respawnCount + ". Total active count: " + CountActive());
   }

   private void Spawn()
   {
      for (int i = 0, spawnCount = 0, spawnCountRFT = 0; i < SpawnTarget; i++)
      {
         var rft = RandomFreeTransform();
         if (rft)
         {
            PlaceFish(rft);
            spawnCountRFT++;
         }
         else
         {
            // Depreciated? Re-places already placed fish for sake of getting enough placements; i.e. Silly.
            spawnCount = RePlaceFish(spawnCount);
         }
         if (i == SpawnTarget - 1)
         {
            Debug.Log("FishPool Spawn-Recovery: " + spawnCount + 
               ". RandomFreeTransform()'s requested: " + spawnCountRFT + 
               ". Total active count: " + CountActive());
         }
      }
   }
}
