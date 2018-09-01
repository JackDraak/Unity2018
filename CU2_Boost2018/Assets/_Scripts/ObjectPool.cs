using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
   [Tooltip("Allow pool to grow as needed (if checked)")]
   [SerializeField] bool dynamicPool = false;

   [SerializeField] int initialPoolSize = 16;

   [Range(1, 8)]
   [SerializeField] int poolGrowthRate = 8;

   [Space(10)] [SerializeField] GameObject myObject;

   [Tooltip("0 = do not expire")]
   [SerializeField] float ObjectLifespan = 0;

   [Range(0, 100)]
   [SerializeField] int spawnPercent = 50;

   struct MyObject
   {
      public bool on;
      public float onTime;
      public int poolIndex;
      public GameObject gameObject;
      public Transform xform;
   }

   private Dictionary<GameObject, int> objectTable = new Dictionary<GameObject,int>();
   private Dictionary<int, Transform> transformTable = new Dictionary<int, Transform>();
   private FishSpawn[] spawnPoints;
   private int dynamicPoolSize;
   private int poolIndex;
   private MyObject[] myObjects;
   private Transform xform;

   private void Start()
   {
      dynamicPoolSize = initialPoolSize;
      myObjects = new MyObject[initialPoolSize];
      poolIndex = initialPoolSize;
      spawnPoints = GetComponentsInChildren<FishSpawn>();
      Debug.Log(spawnPoints.Length + " ObjectPool spawnPoints identified.");

      // Build initial pool
      for (int i = 0; i < initialPoolSize; i++) CreateObject(i);

      Spawn();
   }

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.K) && Debug.isDebugBuild) ReclaimAllObjects();
      if (Input.GetKeyDown(KeyCode.L) && Debug.isDebugBuild) Respawn();
      ExpireObjects();
   }
   private void CreateObject(int i)
   {
      myObjects[i].gameObject = Instantiate(myObject, transform.position, Quaternion.identity, transform);
      myObjects[i].onTime = 0;
      myObjects[i].poolIndex = i;
      myObjects[i].on = false;
      myObjects[i].xform = transform;
      myObjects[i].gameObject.SetActive(false);
      objectTable.Add(myObjects[i].gameObject, i);
      transformTable.Add(i, transform);
   }

   private void ExpireObjects()
   {
      CycleLifespan();
      CycleInactive();
   }

   private void CycleInactive()
   {
      for (int i = 0; i < dynamicPoolSize; i++)
      {
         if (!myObjects[i].on)
         {
            myObjects[i].on = false;
            myObjects[i].gameObject.SetActive(false);

            // get transform from transformTable
            Transform xform;
            if (transformTable.TryGetValue(i, out xform))
            {
               myObjects[i].xform = xform;
            }
         }
      }
   }

   private void CycleLifespan()
   {
      if (ObjectLifespan != 0)
      {
         for (int i = 0; i < dynamicPoolSize; i++)
         {
            if (myObjects[i].on)
            {
               if (Time.time > myObjects[i].onTime + ObjectLifespan)
               {
                  myObjects[i].on = false;
                  myObjects[i].gameObject.SetActive(false);
               }
            }
         }
      }
   }

   private void GrowPool()
   {
      MyObject[] temp = new MyObject[dynamicPoolSize];
      for (int i = 0; i < dynamicPoolSize; i++)
      {
         temp[i] = myObjects[i];
      }
      dynamicPoolSize += poolGrowthRate;

      myObjects = new MyObject[dynamicPoolSize];
      for (int i = 0; i < dynamicPoolSize - poolGrowthRate; i++)
      {
         myObjects[i] = temp[i];
      }
      for (int i = 0; i < poolGrowthRate; i++)
      {
         CreateObject(i + dynamicPoolSize - poolGrowthRate);
      }
   }

   private void PopObject(Transform transform)
   {
      poolIndex++;
      if (poolIndex >= dynamicPoolSize) poolIndex = 0;

      if (myObjects[poolIndex].on && dynamicPool)
      {
         GrowPool();
         poolIndex++;
      }
      RecycleObject(transform, poolIndex);
   }

   private Transform RandomFreeTransform()
   {
      Transform[] emptySpawnPoints = new Transform[spawnPoints.Length];
      int inCount = 0;
      foreach (FishSpawn spawnPoint in spawnPoints)
      {
         if (spawnPoint.transform.childCount == 0) // TODO this needs to recycle 
         {
            emptySpawnPoints[inCount] = spawnPoint.transform;
            inCount++;
         }
         else
         {
            // get object from objectTable
            int index;
            if (objectTable.TryGetValue(spawnPoint.gameObject, out index))
            {
               if (!myObjects[index].on)
               {
                  emptySpawnPoints[inCount] = spawnPoint.transform;
                  inCount++;
               }
            }
         }

      }
      if (inCount > 0) return emptySpawnPoints[Random.Range(0, inCount)];
      else return null;
   }

   private void ReclaimAllObjects()
   {
      int reclaimCount = 0;
      for (int i = 0; i < dynamicPoolSize; i++)
      {
         if (myObjects[i].on)
         {
            myObjects[i].on = false;
            myObjects[i].gameObject.SetActive(false);
            reclaimCount++;
         }
      }
      Debug.Log("Fishes reclaimed: " + reclaimCount);
   }

   private void RecycleObject(Transform xform, int poolIndex)
   {
      myObjects[poolIndex].on = true;
      myObjects[poolIndex].onTime = Time.time;
      myObjects[poolIndex].gameObject.transform.position = xform.position;
      myObjects[poolIndex].gameObject.transform.parent = xform;

      // TODO more stuff

      // doing a pre-emtive toggle-off ensures that even non-dynamic use gives a
      // more dynamic appearance, but depending on your application, it might not
      // be optimal -- it's safe to comment-out the following "false" line.
      //myObjects[position].gameObject.SetActive(false);
      myObjects[poolIndex].gameObject.SetActive(true);
   }

   private void Respawn()
   {
      foreach (MyObject mo in myObjects)
      {
         mo.gameObject.SetActive(true);
         // mo.on = true;
      }
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
            PopObject(rft);
            spawnCount++;
         }
         else
         {
            for (int n = dynamicPoolSize; n >= 0; n--)
            {
               if (!myObjects[n].on)
               {
                  myObjects[n].on = true;
                  myObjects[n].gameObject.SetActive(true); // TODO do something similiar elsewhere as needed?
                  spawnCount++;
               }
            }
         }
      }
      Debug.Log("SpawnCount: " + spawnCount);
   }
}