using UnityEngine;

public class FishPool : MonoBehaviour
{
   // TODO this is being used as a bootleg way to keep te play area 'alive', as the fish have a tendency to wander.
   // ... perhaps it would be smart to improve their behaviour in general? collision-avoidance? group-behaviours? Staying in bounds?
   [Tooltip("Lifespan in seconds (0 = do not expire)")]
   [SerializeField] float fishLifeMax = 300;
   [Tooltip("Time before Fish Life Max when fish *may* expire, in seconds [should be 0 or smaller value than fishLifeMin]")]
   [SerializeField] float fishLifeWindow = 60;

   [Tooltip("Allow spawn to populate without a Reset() when true.")]
   [SerializeField] bool dynamicSpawn = false;

   [Space(10)][SerializeField] GameObject fishPrefab; // TODO make this an array, get more fish!?
   [Tooltip("Percentage of Spawn Points to Populate.")]
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
   }

   private void Start()
   {
      if (fishLifeWindow > fishLifeMax)
      {
         Debug.Log("FishPool Start() ERROR: fishLifeWindow MUST be a smaller value that fishLifeMax. " +
            "Setting both to zero. Please correct this issue in the inspector. (FishPool object/script");
         fishLifeWindow = 0;
         fishLifeMax = 0;
      }
      spawnPoints = GetComponentsInChildren<FishSpawn>();
      dynamicPoolSize = SpawnTarget;
      fishes = new Fish[SpawnTarget];

      // Build initial pool & populate.
      for (int i = 0; i < SpawnTarget; i++) CreateFishObject(i);
      //Debug.Log("FishPool Start() Spawn-Points identified: " + CountPool);
      Spawn();
   }

   private void Update()
   {
      PollDebug();
      ExpireFish();
      if (dynamicSpawn && CountUnder) PartialSpawn();
      if (CountOver) CorrectPoolSize();
   }

   private void CorrectPoolSize()
   {   
      int newTarget = SpawnTarget - CountPool;
      if (newTarget > 0) GrowPool(newTarget);
      // TODO shrink overfull pools? i.e. GrowPool but in reverse....
      // else if (newTarget < 0) ShrinkPool(-newTarget);
   }

   private int CountActive
   {
      get {
         int active = 0;
         foreach (Fish fish in fishes) if (fish.on) active++;
         return active;
      }
   }

   private bool CountFull { get { return (CountActive == SpawnTarget); } }
   private bool CountOver { get { return (CountPool > SpawnTarget); } }
   private int CountPool { get { return fishes.Length; } }
   private bool CountUnder { get { return (CountActive < SpawnTarget); } } 

   private void CreateFishObject(int index)
   {
      fishes[index].fishObject = Instantiate(fishPrefab, transform.position, Quaternion.identity, transform);
      fishes[index].fishObject.SetActive(false);
      fishes[index].on = false;
      fishes[index].onTime = 0;
      fishes[index].poolIndex = index;
      fishes[index].xform = transform;
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
      int active = CountActive;
      int targetDelta = active - SpawnTarget;
      while (targetDelta > 0)
      {
         fishes[active].on = false; // CycleInactive() does the rest of the work...
         active--;
         targetDelta--;
      }
   }

   private void ExpireFish()
   {
      CycleLifespan();
      CycleInactive();
   }

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
      string debugString = "FishPool GrowPool() execution result, +" + delta;
      if (delta == 1) debugString += ". Added fish #: " + dynamicPoolSize;
      else debugString += ". Added fish #'s: " + (dynamicPoolSize - delta + 1) + "-" + dynamicPoolSize;
      Debug.Log(debugString);
   }

   private void PartialSpawn()
   {
      if (!CountFull) CorrectPoolSize();

      int delta = CountActive - SpawnTarget;
      // Debug.Log("PartialSpawn delta: " + delta);
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
      //Debug.Log("FishPool ReclaimAllFish() count: " + reclaimCount + ". Total active count: " + CountActive);
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

   public void Reset()
   {
      Respawn();
   }

   private void Respawn()
   {
      ReclaimAllFish();
      CorrectPoolSize();
      Spawn();
   }

   private void ShrinkPool(int delta) // TODO depreciate? still causing problems....
   {
      // Place current array in temp storage.
      Fish[] temp = new Fish[dynamicPoolSize];
      for (int i = 0; i < dynamicPoolSize; i++) temp[i] = fishes[i];
      dynamicPoolSize -= delta;

      // Copy from temp into newer, smaller array.
      fishes = new Fish[dynamicPoolSize];
      for (int i = 0; i < dynamicPoolSize; i++) fishes[i] = temp[i];

      string debugString = "FishPool ShrinkPool() Performed, -" + delta;
      if (delta == 1) debugString += ". Removed fish #: " + (dynamicPoolSize + 1);
      else debugString += ". Removed fish #'s: " + (dynamicPoolSize - delta) + "-" + (dynamicPoolSize + 1);
      Debug.Log(debugString);
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
      Debug.Log("FishPool Spawn() RandomFreeTransform's requested: " + spawnCountRFT + 
         ". Total active count: " + CountActive);
   }
}
