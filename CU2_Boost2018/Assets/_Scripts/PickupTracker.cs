﻿using System; 
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickupTracker : MonoBehaviour
{
   [SerializeField] Color colourGoalHigh = Color.clear;
   [SerializeField] Color colourGoalLow = Color.clear;
   [SerializeField] GameObject bonusPrefab;
   [SerializeField] Image goalFill;
   [SerializeField] Player player;
   [SerializeField] Slider goalSlider;
   [SerializeField] TextMeshProUGUI text_tasklist;

   private bool               complete, debugMode, spawning, task1, task2;
   private float              maxPower, priorPercent;
   private float              pickupPercent = 0;
   private int                count = 0;
   private int                highCount;
   private GameObject[]       pickupsArray;
   private GameObject[]       spawnPointsArray;
   private List<Collider>     claimedPickups;
   private readonly List<GameObject> pickups;
   private TextMeshProUGUI    text_tracker;
   private Timekeeper         timeKeeper;

   private void Awake()
   {
      debugMode = Debug.isDebugBuild;

      text_tracker = GetComponent<TextMeshProUGUI>();
      if (!text_tracker) Debug.Log("FAIL: no text_tracker object reference!!");

      spawnPointsArray = GameObject.FindGameObjectsWithTag("Spawn_Good");
   
      pickupsArray = GameObject.FindGameObjectsWithTag("GoodObject_01");

      claimedPickups = new List<Collider>();
   }

   public bool ClaimPickup(Collider other)
   {
      if (!claimedPickups.Contains(other))
      {
         count -= 1;
         claimedPickups.Add(other);
         text_tracker.text = count.ToString() + " Gas Canisters Remaining";
         return true;
      }
      return false;
   }

   private void DespawnAll()
   {
      foreach (GameObject spawnPoint in spawnPointsArray)
      {
         if (spawnPoint.transform.childCount != 0) { Destroy(spawnPoint.transform.GetChild(0).gameObject);  }
      }
   }

   private IEnumerator DoSpawn()
   {
      //Debug.Log("PickupTracker spawntime: " + Time.time.ToString("F2"));
      spawning = true;

      DespawnAll();
      yield return new WaitForSeconds(0.2f);
      //SpawnAll();
      SpawnPercent(33); // TODO decide how to use this dynamically?

      Array.Clear(pickupsArray, 0, pickupsArray.Length);
      pickupsArray = GameObject.FindGameObjectsWithTag("GoodObject_01");
      //Debug.Log("PickupTracker number of pickups spawned: " + pickupsArray.Length);

      text_tracker.text = count.ToString() + " Gas Canisters Remaining";

      claimedPickups = new List<Collider>();
      spawning = false;
      complete = false;
   }

   private void FillPosition(Transform position)
   {
      var p = position.transform.position;
      var q = Quaternion.identity;
      GameObject spawnedObject = Instantiate(bonusPrefab, p, q) as GameObject;
      spawnedObject.transform.parent = position;
      spawnedObject.SetActive(true);
   }

   public float PickupPercent()
   {
      return pickupPercent;
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

   public void Restart()
   {
      StartCoroutine("DoSpawn");
   }

   private void ReviewObjectives()
   {
      if (count == 0)
      {
         WinRound();
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

   private void SpawnAll()
   {
      count = 0;
      do { SpawnRandomSpawnpoint(); count++; }
      while (SpawnPointIsEmpty());
      highCount = count;
   }

   private void SpawnPercent(int percent)
   {
      if (percent < 0) return;
      if (percent > 100) percent = 100;
      else
      {
         float floatPercent = percent / 100f;
         int target = Mathf.FloorToInt(spawnPointsArray.Length * floatPercent);
         for (int i = 0; i < target; i++) SpawnRandomSpawnpoint();
         count = highCount = target;
      }
   }

   private bool SpawnPointIsEmpty()
   {
      if (RandomFreePosition()) return true;
      else return false;
   }

   private void SpawnRandomSpawnpoint()
   {
      GameObject freePos = RandomFreePosition();
      if (freePos) FillPosition(freePos.transform);
   }

   private int SpawnTally()
   {
      int spawnTally = 0;
      foreach (GameObject spawnPoint in spawnPointsArray) { if (spawnPoint.transform.childCount > 0) spawnTally++; }
      return spawnTally;
   }

   private void Start()
   {
      timeKeeper = FindObjectOfType<Timekeeper>();
      complete = spawning = task1 = task2 = false;
      text_tasklist.text = "Goal: Raise Thrust Cap to 60%\nMini-Goal: Collect Gas Canisters";

      StartCoroutine("DoSpawn");
   }

   private void TrackPickups()
   {
      // While in play, check tasklist objectives:
      if (!complete && !spawning) ReviewObjectives();

      // Goal-UI updates, colour & value: 
      float fillLerp = (float)((float)(highCount - count) / (float)highCount);
      Color fillColor = Vector4.Lerp(colourGoalLow, colourGoalHigh, fillLerp);
      goalFill.color = fillColor;
      goalSlider.value = fillLerp;
      pickupPercent = Mathf.FloorToInt(fillLerp * 100);
      if (priorPercent != pickupPercent)
      {
         priorPercent = pickupPercent;
         player.DoGoalUpdate();
      }
   }

   private void Update()
   {
      TrackPickups();
      if (Input.GetKeyDown(KeyCode.N) && debugMode) StartCoroutine("DoSpawn");
      if (Input.GetKeyDown(KeyCode.M) && debugMode) DespawnAll();
   }

   private void WinRound()
   {
      complete = true;
      timeKeeper.Cease(highCount);
      maxPower = player.BoostMaxPower(0.1f); // 0.1f = 10% boost
   }
}
