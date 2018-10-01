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

   private enum task { task_0, task_1, task_2, task_3, task_4, task_5, task_6, task_7 }
   private enum level { level_0, level_1, level_2, level_3, level_4, level_5, level_6, level_7, bonusLevel_1 }

   private task objective = task.task_0;
   private level myLevel = level.level_0;

   private bool               complete, spawning; // task1, task2;
   private float              maxPower, priorPercent;
   private float              pickupPercent = 0;
   private int                count = 0;
   private int                countdownDelay;
   private int                highCount;
   private GameObject[]       pickupsArray;
   private GameObject[]       spawnPointsArray;
   private List<Collider>     claimedPickups;
   //private List<GameObject>   spawnedPickups;
   private TextMeshProUGUI    text_tracker;
   private Timekeeper         timeKeeper;

   private void Awake()
   {
      text_tracker = GetComponent<TextMeshProUGUI>();
      if (!text_tracker) Debug.Log("FAIL: no text_tracker object reference!!");

      spawnPointsArray = GameObject.FindGameObjectsWithTag("Spawn_Good");
   
      pickupsArray = GameObject.FindGameObjectsWithTag("GoodObject_01");

      claimedPickups = new List<Collider>();
      //spawnedPickups = new List<GameObject>();
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

   public void DespawnAll()
   {
      foreach (GameObject spawnPoint in spawnPointsArray)
      {
         if (spawnPoint.transform.childCount != 0) { Destroy(spawnPoint.transform.GetChild(0).gameObject); }
      }
   }

   private IEnumerator DoCountdown()
   {
      int n = countdownDelay - 1;
      Debug.Log(player.casualMode);
      if (player.casualMode) text_subCountdown.text = "...prepare for reset..."; // TODO why isnt this working?
      else text_subCountdown.text = "...in <i>casual-mode</i> progress manually with a 'R'eset...";
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
      yield return new WaitForSeconds(0.05f); // TODO too short/long? needed at all?
      SpawnPercent(33); // 33 // TODO decide how to use this dynamically?
      //SpawnAll();

      Array.Clear(pickupsArray, 0, pickupsArray.Length);
      pickupsArray = GameObject.FindGameObjectsWithTag("GoodObject_01");

      text_tracker.text = count.ToString() + " Gas Canisters Remaining";

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

   public float PickupPercent() { return pickupPercent; }

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

   public void Restart() { StartCoroutine("DoSpawn"); }

   private void ReviewObjectives()
   {
      if (count == 0)
      {
         WinRound();
         if (myLevel == level.level_0)
         {
            // "Level 1 Goal: Raise Thrust Cap to 60%\nMini-Goal: Collect a full set of gas canisters\n(for a small boost to Thrust Cap)";
            if (objective == task.task_0)// (!task1)
            {
               objective = task.task_1; //task1 = true;
               text_tasklist.text = "Level 1 Goal: Raise Thrust Cap to 60%, 'R'eset and do\n\nMini-Goal: Collect a full set of gas canisters\n(for a small boost to Thrust Cap)";
            }
            if (maxPower >= 0.6f && objective == task.task_1)
            {
               objective = task.task_2; //task2 = true;
               text_tasklist.text = "Level One Complete! Congratulations!\n\nPlease 'stay tuned' for future developments, thanks for playing!"; // TODO update this when apropriate
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
      if (freePos)
      {
         FillPosition(freePos.transform);
         //spawnedPickups.Add(freePos);
      }
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
      complete = spawning = false;
      text_tasklist.text = "Level 1 Goal: Raise Thrust Cap to 60%\n\nMini-Goal: Collect a full set of gas canisters\n(for a small boost to Thrust Cap)";
      text_countdown.text = "";
      text_subCountdown.text = "";
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

   public void TriggerSpawn() { StartCoroutine("DoSpawn"); }

   public void TriggerCountdown(int delay)
   {
      countdownDelay = delay;
      StartCoroutine("DoCountdown", delay);
   }

   private void Update() { TrackPickups(); }

   private void WinRound()
   {
      complete = true;
      timeKeeper.Cease(highCount);
      maxPower = player.BoostMaxPower(0.1f); // 0.1f = 10% boost
      player.AutoRestart();
   }
}
