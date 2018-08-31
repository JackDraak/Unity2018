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
      public GameObject gameObject;
   }

   private Transform xform;
   private FishSpawn[] spawnPoints;
   private int dynamicPoolSize;
   private int poolPosition;
   private MyObject[] myObjects;

   private void Start()
   {
      dynamicPoolSize = initialPoolSize;
      myObjects = new MyObject[initialPoolSize];
      poolPosition = initialPoolSize;
      spawnPoints = GetComponentsInChildren<FishSpawn>();
      Debug.Log(spawnPoints.Length + " ObjectPool spawnPoints identified.");

      // Build initial pool
      for (int i = 0; i < initialPoolSize; i++) CreateObject(i);

      Spawn();
   }

   private void Spawn()
   {
      int target = Mathf.FloorToInt(spawnPoints.Length * (spawnPercent / 100f));
      for (int i = 0; i < target; i++)
      {
         var rft = RandomFreeTransform();
         if (rft) PopObject(rft);
      }
   }

   private void Update()
   {
      ExpireObjects();
   }

   private void ExpireObjects()
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

   private void CreateObject(int i)
   {
      myObjects[i].gameObject = Instantiate(myObject, transform.position, Quaternion.identity, transform);
      myObjects[i].onTime = 0;
      myObjects[i].on = false;
      //myObjects[i].gameObject.transform.parent = 
      myObjects[i].gameObject.SetActive(false);
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
      poolPosition++;
      if (poolPosition >= dynamicPoolSize) poolPosition = 0;

      if (myObjects[poolPosition].on && dynamicPool)
      {
         GrowPool();
         poolPosition++;
      }
      RecycleObject(transform, poolPosition);
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

   private void RecycleObject(Transform transform, int position)
   {
      myObjects[position].on = true;
      myObjects[position].onTime = Time.time;
      myObjects[position].gameObject.transform.position = transform.position;
      myObjects[position].gameObject.transform.parent = transform;

      // doing a pre-emtive toggle-off ensures that even non-dynamic use gives a
      // more dynamic appearance, but depending on your application, it might not
      // be optimal -- it's safe to comment-out the following "false" line.
      //myObjects[position].gameObject.SetActive(false);
      myObjects[position].gameObject.SetActive(true);
   }
}