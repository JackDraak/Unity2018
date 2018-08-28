using System.Collections.Generic;
using System;
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

   private bool            complete, spawning, task1, task2;
   private float           maxPower;
   private int             count, highCount;
   private GameObject[]    pickupsArray;
   private GameObject[]    spawnPointsArray;
   private List<GameObject> pickups;
   private TextMeshProUGUI text_tracker;
   private Timekeeper      timeKeeper;

   private void Awake()
   {
      text_tracker = GetComponent<TextMeshProUGUI>();
      if (!text_tracker) Debug.Log("FAIL: no text_tracker object variable!!");

      spawnPointsArray = GameObject.FindGameObjectsWithTag("Spawn_Good");
      Debug.Log("Number of spawnPoints: " + spawnPointsArray.Length);

      pickupsArray = GameObject.FindGameObjectsWithTag("GoodObject_01");
   }

   private void Start()
   {
      timeKeeper = FindObjectOfType<Timekeeper>();
      complete = spawning = task1 = task2 = false;
      text_tasklist.text = "Goal: Raise Thrust Cap to 60%\nMini-Goal: Collect Gas Canisters";

      DoSpawn();
   }

   private void DoSpawn()
   {
      Debug.Log("Spawntime: " + Time.time.ToString("F2"));
      spawning = true;

      // despawn all
      foreach (GameObject spawnPoint in spawnPointsArray)
      {
         if (spawnPoint.transform.childCount != 0)
         {
            //spawnPoints.Remove(spawnPoint);
            Destroy(spawnPoint.transform.GetChild(0).gameObject);
         }
      }

      int sp = 0;
      do
      {
         SpawnRandomSpawnpoint();
         sp++;
      }
      while (sp < spawnPointsArray.Length); // TDOO
      //while (!SpawnPointsAreFull());

      Array.Clear(pickupsArray, 0, pickupsArray.Length);
      pickupsArray = GameObject.FindGameObjectsWithTag("GoodObject_01");
      Debug.Log("Number of pickups: " + pickupsArray.Length);

      // count here
      count = highCount = 0;
      foreach (GameObject pickup in pickupsArray)
      {
         if (pickup.activeSelf) count++; // TODO no longer using active/inactive

         if (count > highCount) highCount = count;
      }
      text_tracker.text = count.ToString() + " Gas Canisters Remaining";

      complete = false;
      spawning = false;
   }

   public void Restart()
   {
      DoSpawn();
   }

   // On average, return 'True' ~half the time, and 'False' ~half the time.
   private bool FiftyFifty()
   {
      return true;
      //if (Mathf.FloorToInt(UnityEngine.Random.Range(0, 2)) == 1) return true;
      //else return false;
   }

   public void DespawnAll()
   {
      // despawn all
      foreach (GameObject spawnPoint in spawnPointsArray)
      {
         if (spawnPoint.transform.childCount != 0)
         {
            //spawnPoints.Remove(spawnPoint);
            Destroy(spawnPoint.transform.GetChild(0).gameObject);
         }
      }
   }

   private void FillPosition(Transform position)
   {
      var b = position.transform.position;
      var c = Quaternion.identity;
      GameObject spawnedObject = Instantiate(bonusPrefab, b, c) as GameObject;
      spawnedObject.transform.parent = position;
      spawnedObject.SetActive(true);
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
      if (inCount > 0) return emptySpawnPoints[UnityEngine.Random.Range(0, inCount)];
      else return null;
   }

   public void SpawnAllSpawnpoints()
   {
      GameObject freePos = RandomFreePosition();
      if (freePos) FillPosition(freePos.transform);

      float delayBetweenSpawn = 0.001f;
      if (RandomFreePosition()) Invoke("SpawnAllSpawnpoints", delayBetweenSpawn);
      else if (SpawnPointsAreFull())
      {
         Debug.Log("SpawnAllSpawnpoints() ");
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
      //   foreach (GameObject spawnPoint in spawnPointsArray)
      //   {
      //      if (spawnPoint.transform.childCount == 0) return false;
      //   }
      //   return true;
      return !SpawnPointsAreEmpty();
   }

   private int SpawnCount()
   {
      int spawnCount = 0;
      foreach (GameObject spawnPoint in spawnPointsArray)
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
      CountPickups(); // TODO maybe not?

      if (!complete && !spawning)
      {
         if (count == 0)
         {
            complete = true;
            timeKeeper.Cease(highCount); // TODO
            //timeKeeper.Cease(pickupsArray.Length); // TODO
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

   public int ClaimPickup()
   {
      count -= 1;
      text_tracker.text = count.ToString() + " Gas Canisters Remaining";
      return count;
   }

   private void CountPickups()
   {
    //  var pickupsArr = GameObject.FindGameObjectsWithTag("GoodObject_01"); // TODO - really?
    //  count = highCount = 0;
    // foreach (GameObject pickup in pickupsArr)
    //  {
    //     if (pickup.activeSelf) count++; // TODO no longer using active/inactive
//
   //      if (count > highCount) highCount = count;
   //   }
      text_tracker.text = count.ToString() + " Gas Canisters Remaining";
   }

   private void Update()
   {
      TrackPickups();
   }
}
