using System.Collections.Generic;
using UnityEngine;

public class FishPool : MonoBehaviour
{
   [Tooltip("Allow pool to grow as needed (if checked)")]
   [SerializeField] bool dynamicPool = false;

   [Tooltip("Works best when set to the same # of spawn-points; unexpected results when larger than.")]
   [SerializeField] int initialPoolSize = 45;

   [Range(1, 8)]
   [SerializeField] int poolGrowthRate = 8;

   [Space(10)] [SerializeField] GameObject fishPrefab;

   [Tooltip("0 = do not expire")]
   [SerializeField] float fishLifespan = 0;

   [Range(0, 100)]
   [SerializeField] int spawnPercent = 50;

   struct Fish
   {
      public bool on;
      public float onTime;
      public int poolIndex;
      public GameObject fishObject;
      public Transform xform;
   }

   private FishSpawn[] spawnPoints;
   private int dynamicPoolSize;
   private int poolIndex;
   private Fish[] fishes;
   private Transform xform;

   private void Start()
   {
      dynamicPoolSize = initialPoolSize;
      fishes = new Fish[initialPoolSize];
      poolIndex = initialPoolSize; // ?? Why did I do that?!? lol...
      spawnPoints = GetComponentsInChildren<FishSpawn>();
      Debug.Log(spawnPoints.Length + " FishPool spawnPoints identified.");
      for (int i = 0; i < initialPoolSize; i++) CreateFishObject(i);  // Build initial pool
      Spawn();
   }

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.K) && Debug.isDebugBuild) ReclaimAllFish();
      if (Input.GetKeyDown(KeyCode.L) && Debug.isDebugBuild) Respawn();
      ExpireFish();
   }

   private int CountActive()
   {
      int active = 0;
      foreach (Fish mo in fishes)
      {
         if (mo.on) active++;
      }
      return active;
   }

   private void CreateFishObject(int i)
   {
      fishes[i].fishObject = Instantiate(fishPrefab, transform.position, Quaternion.identity, transform);
      fishes[i].onTime = 0;
      fishes[i].poolIndex = i;
      fishes[i].on = false;
      fishes[i].xform = transform;
      fishes[i].fishObject.SetActive(false);
   }

   private void CycleInactive()
   {
      for (int i = 0; i < dynamicPoolSize; i++)
      {
         if (!fishes[i].on) fishes[i].fishObject.transform.parent = this.transform;
      }
   }

   private void CycleLifespan()
   {
      if (fishLifespan != 0)
      {
         for (int i = 0; i < dynamicPoolSize; i++)
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

   private void ExpireFish()
   {
      CycleLifespan();
      CycleInactive();
   }

   private void GrowPool()
   {
      Fish[] temp = new Fish[dynamicPoolSize];
      for (int i = 0; i < dynamicPoolSize; i++)
      {
         temp[i] = fishes[i];
      }
      dynamicPoolSize += poolGrowthRate;

      fishes = new Fish[dynamicPoolSize];
      for (int i = 0; i < dynamicPoolSize - poolGrowthRate; i++)
      {
         fishes[i] = temp[i];
      }
      for (int i = 0; i < poolGrowthRate; i++)
      {
         CreateFishObject(i + dynamicPoolSize - poolGrowthRate);
      }
   }

   private void PlaceFish(Transform xform)
   {
      poolIndex++;
      if (poolIndex >= dynamicPoolSize) poolIndex = 0;

      if (fishes[poolIndex].on && dynamicPool)
      {
         GrowPool();
         poolIndex++;
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
      int respawnCount = 0;
      ReclaimAllFish();
      int target = Mathf.FloorToInt(fishes.Length * (spawnPercent / 100f));
      for (int i = 0; i < target; i++)
      {
         RecycleFish(RandomFreeTransform(), i);
         respawnCount++;
      }
      Debug.Log("Fishes respawned: " + respawnCount + ". Total active count: " + CountActive().ToString());
   }

   private void Spawn()
   {
      int spawnCount = 0;
      int target = Mathf.FloorToInt(spawnPoints.Length * (spawnPercent / 100f));
      for (int i = 0; i < target; i++)
      {
         var rft = RandomFreeTransform();
         if (rft)
         {
            PlaceFish(rft);
            spawnCount++;
         }
         else
         {
            for (int n = dynamicPoolSize; n >= 0; n--)
            {
               if (!fishes[n].on)
               {
                  fishes[n].on = true;
                  fishes[n].fishObject.SetActive(true);
                  spawnCount++;
               }
            }
         }
      }
      Debug.Log("FishPool SpawnCount: " + spawnCount + ". Total active count: " + CountActive().ToString());
   }
}