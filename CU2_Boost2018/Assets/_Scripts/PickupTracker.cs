using System; 
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
   [SerializeField] TextMeshProUGUI text_countdown;
   [SerializeField] TextMeshProUGUI text_subCountdown;

   private enum Task { _0, _1, _2, _3, _4, _5, _6, _7 }
   private enum Level { _0, _1, _2, _3, _4, _5, _6, _7, bonusLevel }

   private Task task = Task._0;
   private Level level = Level._0;

   private bool complete, spawning;
   private float maxPower, priorPercent;
   private float pickupPercent = 0;
   private int count = 0;
   private int highCount;
   private GameObject[] pickupsArray;
   private GameObject[] spawnPointsArray;
   private List<Collider> claimedPickups;
   private TextMeshProUGUI text_tracker;
   private Timekeeper timeKeeper;

   private void Awake()
   {
      text_tracker = GetComponent<TextMeshProUGUI>();
      spawnPointsArray = GameObject.FindGameObjectsWithTag("Spawn_Good");
      pickupsArray = new GameObject[0];
      claimedPickups = null;
   }

   public bool ClaimPickup(Collider other)
   {
      if (!claimedPickups.Contains(other))
      {
         count -= 1;
         claimedPickups.Add(other);
         text_tracker.text = ApplyColour.Green + count + ApplyColour.Close + " Gas Canisters Remaining";
         return true;
      }
      return false;
   }

   // TODO update this when apropriate
   private IEnumerator Congratulations()
   {
      int flashCycles = 12;
      float flashPause = 0.666f;
      for (int n = 0; n < flashCycles; n++)
      {
         string congratulations = "Level One Complete! Congratulations!\n\nPlease " +
            ApplyColour.NextColour + "'stay tuned'" + ApplyColour.Close + " for future developments, thanks for playing!";
         ApplyColour.Toggle();
         text_tasklist.text = ApplyColour.Colour + congratulations + ApplyColour.Close;
         yield return new WaitForSeconds(flashPause);
      }
   }

   public int Count { get { return count; } }

   public void DespawnAll()
   {
      foreach (GameObject spawnPoint in spawnPointsArray)
         if (spawnPoint.transform.childCount != 0) Destroy(spawnPoint.transform.GetChild(0).gameObject); 
   }

   private IEnumerator DoCountdown(int downCount)
   {
      int n = downCount - 1;
      if (player.casualMode) text_subCountdown.text = "...prepare for reset...";
      else text_subCountdown.text = "To stop automatic progression, switch to <i>" + ApplyColour.Green + "C" 
            + ApplyColour.Close + "asual-mode</i>\nand progress manually with a " + ApplyColour.Green 
            + "R" + ApplyColour.Close + "eset...";
      while (n >= 0)
      {
         text_countdown.text = n.ToString();
         yield return new WaitForSeconds(0.99f);
         n--;
      }
      text_countdown.text = "";
      text_subCountdown.text = "";
   }

   private IEnumerator DoSpawn()
   {
      spawning = true;
      DespawnAll();
      yield return null; // Let Despawn() complete, wait one frame.
      SpawnPercent(33); //SpawnAll();
      Array.Clear(pickupsArray, 0, pickupsArray.Length);
      pickupsArray = GameObject.FindGameObjectsWithTag("GoodObject_01");
      text_tracker.text = ApplyColour.Green + count + ApplyColour.Close + " Gas Canisters Remaining";
      claimedPickups = new List<Collider>();
      spawning = false;
      complete = false;
   }

   private void FillPosition(Transform position)
   {
      Vector3 p = position.transform.position;
      Quaternion q = Quaternion.identity;
      GameObject spawnedObject = Instantiate(bonusPrefab, p, q) as GameObject;
      spawnedObject.transform.parent = position;
      spawnedObject.SetActive(true);
   }

   public float PickupPercent { get { return pickupPercent; } }

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

   public void Restart() { StartCoroutine(DoSpawn()); }

   private void ReviewObjectives()
   {
      if (count == 0)
      {
         WinRound();
         if (level == Level._0)
         {
            if (task == Task._0)
            {
               task = Task._1;
               text_tasklist.text = "Level 1 Goal: Raise Thrust Cap to 45%, " + 
                  ApplyColour.Green + "R" + ApplyColour.Close + "eset and do:\n" +
                  "   Mini-Goal: Collect a full set of gas canisters\n" +
                  "   (for a small boost to Thrust Cap)";
            }
            if (maxPower >= 0.45f && task == Task._1)
            {
               task = Task._2;
               StartCoroutine(Congratulations());
               text_tasklist.text = "Level One Complete! Congratulations!\n\n" +
                  "Please 'stay tuned' for future developments, thanks for playing!"; // TODO update this when apropriate
            }
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
      else Debug.LogWarning("PickupTracker:SpawnRandomSpawnpoint: unable to find freePos for FillPosition().");
   }

   private int SpawnTally()
   {
      int spawnTally = 0;
      foreach (GameObject spawnPoint in spawnPointsArray) if (spawnPoint.transform.childCount > 0) spawnTally++;
      return spawnTally;
   }

   private void Start()
   {
      timeKeeper = FindObjectOfType<Timekeeper>();
      complete = spawning = false;
      text_tasklist.text = 
         "Level 1 Goal: Raise Thrust Cap to 60%\n\nMini-Goal: Collect a full set of gas canisters\n" +
         "(for a small boost to Thrust Cap)";
      text_countdown.text = "";
      text_subCountdown.text = "";
      StartCoroutine(DoSpawn());
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

   public void TriggerSpawn() { StartCoroutine(DoSpawn()); }

   public void TriggerCountdown(int delay) { StartCoroutine(DoCountdown(delay)); }

   private void Update() { TrackPickups(); }

   private void WinRound()
   {
      complete = true;
      timeKeeper.Cease(highCount);
      maxPower = player.BoostMaxPower(0.05f); // 0.05f = 5% power-cap boost per win
      if (!player.casualMode) player.AutoRestart();
   }
}
