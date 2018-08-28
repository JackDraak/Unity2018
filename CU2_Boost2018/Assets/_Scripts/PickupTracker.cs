using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickupTracker : MonoBehaviour {

   [SerializeField] Color goalHigh = Color.clear;
   [SerializeField] Color goalLow = Color.clear;
   [SerializeField] GameObject bonusPrefab;
   [SerializeField] Image goalFill;
   [SerializeField] Player player;
   [SerializeField] Slider goalSlider;
   [SerializeField] TextMeshProUGUI text_tasklist;

   private bool            complete, task1, task2;
   private float           maxPower;
   private int             count, highCount;
   private GameObject[]    pickupsArray;
   private GameObject[]    spawnPointsArray;
   private List<GameObject> pickups;
   private List<GameObject> spawnPoints;
   private TextMeshProUGUI text_tracker;
   private Timekeeper      timeKeeper;

   private void Awake()
   {
      text_tracker = GetComponent<TextMeshProUGUI>();
      if (!text_tracker) Debug.Log("FAIL: no text_tracker object variable!!");

      spawnPointsArray = GameObject.FindGameObjectsWithTag("Spawn_Good");
      Debug.Log("Number of spawnPoints: " + spawnPointsArray.Length);

      spawnPoints = new List<GameObject>();

      //var pickupsArray = GameObject.FindGameObjectsWithTag("GoodObject_01");

      //count = highCount = 0;
      //SpawnAllSpawnpoints();
      //highCount = count;
      //CountPickups();
   }

   private void Start()
   {
      timeKeeper = FindObjectOfType<Timekeeper>();
      complete = task1 = task2 = false;
      text_tasklist.text = "Goal: Raise Thrust Cap to 60%\nMini-Goal: Collect Gas Canisters";

      //while (!SpawnPointsAreFull()) SpawnRandomSpawnpoint();
      SpawnRandomSpawnpoint();
      SpawnRandomSpawnpoint();
      SpawnRandomSpawnpoint();
      SpawnRandomSpawnpoint();
      SpawnRandomSpawnpoint();
      SpawnRandomSpawnpoint();

      pickupsArray = GameObject.FindGameObjectsWithTag("GoodObject_01");
      Debug.Log("Number of pickups: " + pickupsArray.Length);

      CountPickups();
   }

   public void Restart()
   {
      DespawnAll();
      //while (SpawnPointsAreEmpty()) SpawnRandomSpawnpoint();
      //while (!SpawnPointsAreFull()) SpawnRandomSpawnpoint();
      SpawnRandomSpawnpoint();
      SpawnRandomSpawnpoint();
      SpawnRandomSpawnpoint();
      SpawnRandomSpawnpoint();
      SpawnRandomSpawnpoint();

      pickupsArray = GameObject.FindGameObjectsWithTag("GoodObject_01");
      Debug.Log("Number of pickups: " + pickupsArray.Length);
      count = highCount = 0;
      CountPickups();
      //SpawnAllSpawnpoints();
      //highCount = count;
      //spawnPoints = GameObject.FindGameObjectsWithTag("Spawn_Good");
      //foreach (GameObject pickup in pickups) pickup.SetActive(true);
      complete = false;
   }

   public void DespawnAll()
   {
      foreach (GameObject spawnPoint in spawnPointsArray)
      {
         if (spawnPoint.transform.childCount != 0)
         {
            spawnPoints.Remove(spawnPoint);
            Destroy(spawnPoint.transform.GetChild(0).gameObject);
         }
      }
   }

   private void FillPosition(Transform position)
   {
      // determine # of half of spawn points, render value +/-2 to fill
      //var m = Random.Range(n / 2, n / 2 + Random.Range(-2, 3));
      // var n = spawnPoints.Count;
      var b = position.transform.position;
      var c = Quaternion.identity;
      GameObject spawnedObject = Instantiate(bonusPrefab, b, c) as GameObject;
      spawnedObject.transform.parent = position;
      spawnedObject.SetActive(true);

      spawnPoints.Add(spawnedObject);

      //pickups[count] = spawnedObject;
      //count++;
   }

   private GameObject RandomFreePosition()
   {
      GameObject[] emptySpawnPoints = new GameObject[spawnPointsArray.Length];
      int inCount = 0;
      foreach (GameObject spawnPoint in spawnPointsArray)
      {
         if (spawnPoint.transform.childCount == 0)
         {
            emptySpawnPoints[inCount] = spawnPoint;
            inCount++;
         }
      }
      if (inCount > 0) return emptySpawnPoints[Random.Range(0, inCount)];
      else return null;
   }

   public void SpawnAllSpawnpoints()
   {
      GameObject freePos = RandomFreePosition();
      if (freePos) FillPosition(freePos.transform);

      float delayBetweenSpawn = 0.001f;
      if (RandomFreePosition()) Invoke("SpawnAllSpawnpoints", delayBetweenSpawn);
      else if (SpawnPointsAreFull())// && thisWave <= numberOfWaves)
      {
         Debug.Log("SpawnAllSpawnpoints() ");
   //      thisWave++;
   //      respawn = false;
      }
   }

   public bool SpawnPointsAreEmpty()
   {
      foreach (GameObject spawnPoint in spawnPointsArray)
      {
         if (spawnPoint.transform.childCount > 0) return false;
      }
      return true;
   }

   public bool SpawnPointsAreFull()
   {
      foreach (GameObject spawnPoint in spawnPoints)
      {
         if (spawnPoint.transform.childCount == 0) return false;
      }
      return true;
   }

   private int SpawnCount()
   {
      int spawnCount = 0;
      foreach (GameObject spawnPoint in spawnPoints)
      {
         if (spawnPoint.transform.childCount > 0) spawnCount++;
      }
      return spawnCount;
   }

   public void SpawnRandomSpawnpoint()
   {
      GameObject freePos = RandomFreePosition();
      if (freePos) FillPosition(freePos.transform);
   }

   private void TrackPickups()
   {
      CountPickups();

      if (!complete)
      {
         if (count == 0)
         {
            complete = true;
            timeKeeper.Cease(highCount); // TODO
            // timeKeeper.Cease(pickupsArray.Length); // TODO
            maxPower = player.BoostMaxPower();
            if (!task1)
            {
               task1 = true;
               text_tasklist.text = "Goal: Raise Thrust Cap to 60%, 'R'eset and do\nMini-Goal: Collect More Gas Canisters";
            }
            if (maxPower >= 0.6f && !task2)
            {
               task2 = true;
               text_tasklist.text = "Level One Complete.\nWatch for future development, thanks for playing!";
            }
         }
      }

      float fillLerp = (float)((float)(highCount - count) / (float)highCount);
      Color fillColor = Vector4.Lerp(goalLow, goalHigh, fillLerp);
      goalFill.color = fillColor;
      goalSlider.value = fillLerp;
   }

   private void CountPickups()
   {
      count = 0;
      foreach (GameObject pickup in pickupsArray)
      {
         if (pickup.activeSelf) count++;
         if (count > highCount) highCount = count;
      }
      text_tracker.text = count.ToString() + " Gas Canisters Remaining";
   }

   private void Update()
   {
      TrackPickups();
   }
}
