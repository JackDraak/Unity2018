using UnityEngine;

public class FishPool : MonoBehaviour
{
   [Tooltip("In seconds (0 = do not expire)")]
   [SerializeField] float fishLifespan = 0;

   [Tooltip("Allow pool to grow as needed (if checked)")]
   [SerializeField] bool dynamicSpawn = false; // TODO this is not working (for despawning), Or is there a good reason to keep it/make it work?

   [SerializeField] int initialPoolSize = 16; // TODO depreciate?

   [Space(10)][SerializeField] GameObject fishPrefab;
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

   private int SpawnDelta
   {
      get { return Mathf.FloorToInt(spawnPoints.Length * (spawnPercent / 100f)); }
      set { }
   }

   private void Start()
   {
      dynamicPoolSize = initialPoolSize;
      fishes = new Fish[initialPoolSize];
      spawnPoints = GetComponentsInChildren<FishSpawn>();
      Debug.Log(spawnPoints.Length + " FishPool spawnPoints identified.");

      // Build initial pool... somewhat depreciated at the moment....
      for (int i = 0; i < initialPoolSize; i++) CreateFishObject(i);
      CorrectPoolSize();
      Spawn();
   }

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.K) && Debug.isDebugBuild) ReclaimAllFish();
      if (Input.GetKeyDown(KeyCode.L) && Debug.isDebugBuild) Respawn();
      if (Input.GetKeyDown(KeyCode.J) && Debug.isDebugBuild) PartialSpawn();
      if (dynamicSpawn)
      {
         if (CountUnder()) PartialSpawn();
         //else if (CountExceeded()) PartialDespawn(); // TODO well that didn't work
      }
      ExpireFish();
   }

   private void CorrectPoolSize()
   {   
      // Grow pool to min required size automagically.
      int newTarget = SpawnDelta - CountAll();
      if (newTarget > 0) GrowPool(newTarget);
      // TODO shrink overfull pools?
   }

   private int CountActive()
   {
      int active = 0;
      foreach (Fish mo in fishes) if (mo.on) active++;
      return active;
   }

   private int CountAll()
   {
      return fishes.Length;
   }

   private bool CountFull()
   {
      return (CountActive() == SpawnDelta);
   }

   private bool CountUnder()
   {
      return (CountActive() < SpawnDelta);
   }

   private bool CountExceeded()
   {
      return (CountActive() > SpawnDelta);
   }

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
      if (fishLifespan != 0)
      {
         for (int i = 0; i < fishes.Length; i++)
         {
            if (fishes[i].on)
            {
               if (Time.time > fishes[i].onTime + fishLifespan)
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
      int targetDelta = active - SpawnDelta;
      while (targetDelta > 0)
      {
         fishes[active].on = false; // CycleInactive should do the rest of the work....
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
      // Place old array in temp storage.
      Fish[] temp = new Fish[dynamicPoolSize];
      for (int i = 0; i < dynamicPoolSize; i++) temp[i] = fishes[i];
      dynamicPoolSize += growRate;

      // Copy from temp onto newer, larger array.
      fishes = new Fish[dynamicPoolSize];
      for (int i = 0; i < dynamicPoolSize - growRate; i++) fishes[i] = temp[i];

      // Create next pool fish(es) in the series.
      for (int i = 0; i < growRate; i++) CreateFishObject(i + dynamicPoolSize - growRate);
      string debugString = "Pool Expansion Performed, +" + growRate;
      if (growRate == 1) debugString += ". Added fish #: " + dynamicPoolSize.ToString();
      else debugString += ". Added fish #'s: " + (dynamicPoolSize - growRate + 1).ToString() + "-" + dynamicPoolSize.ToString();
   }

   private void PlaceFish(Transform xform)
   {
      if (SpawnDelta == CountActive()) return; // do not try to place fish if spawndelta attained

      // TODO deal with 'dynamic pool' boolean or depreciate it?

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
      Debug.Log("Fishes reclaimed: " + reclaimCount + ". Total active count: " + CountActive().ToString());
   }

   private void RecycleFish(Transform xform, int poolIndex)
   {
      fishes[poolIndex].on = true;
      fishes[poolIndex].onTime = Time.time;
      fishes[poolIndex].fishObject.transform.parent = xform;
      fishes[poolIndex].fishObject.transform.position = xform.position;
      fishes[poolIndex].fishObject.SetActive(true);
      fishes[poolIndex].xform = xform;
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
      Debug.Log("Fishes respawned: " + respawnCount + ". Total active count: " + CountActive().ToString());
   }

   private void PartialDespawn()
   {
      if (CountExceeded())
      {
         /*         int active = CountActive();
                  int targetDelta = active - SpawnDelta;
                  while (targetDelta > 0)
                  {
                     try
                     {
                        fishes[active].on = false; // CycleInactive should do the rest of the work....
                        active--;
                        targetDelta--;
                     }
                     catch
                     {
                        Debug.Log("PartialDespawn() ERROR fishes[" + (active + 2) + "]");
                        //return;
                     }
                  }
               */
         //const int MAX_DESPAWN_ATTEMPTS = 
         int attempts = 0;
         int reclaimCount = 0;
         int active = CountActive();
         int targetDelta = active - SpawnDelta;
         for (int i = targetDelta; i > 0; i--)
         {
            int index = SpawnDelta + i;
            while (reclaimCount < targetDelta || attempts < SpawnDelta * 2)
            {
               if (fishes[index].on)
               {
                  fishes[index].on = false;
                  fishes[index].fishObject.SetActive(false);
                  fishes[index].fishObject.transform.parent = this.transform;
                  reclaimCount++;
               }
               else
               {
                  attempts++;
                  index++;
               }
            }
         }
      }
      // TODO shrink pool?
   }

   private void PartialSpawn()
   {
      // grow pool, if needed
      if (!CountFull()) CorrectPoolSize();

      int delta = CountActive() - SpawnDelta;
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
      Debug.Log("Fishes PartialSpawn() TOTAL POOL: " + CountAll() + ". Total active count: " + CountActive().ToString());
   }

   private void Spawn()
   {
      for (int i = 0, spawnCount = 0, spawnCountRFT = 0; i < SpawnDelta; i++)
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
         if (i == SpawnDelta - 1)
         {
            Debug.Log("FishPool Spawn-Recovery: " + spawnCount + 
               " (RFTs requested: " + spawnCountRFT + 
               "). Total active count: " + CountActive().ToString());
         }
      }
   }

   private int RePlaceFish(int spawnCount)
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
}
