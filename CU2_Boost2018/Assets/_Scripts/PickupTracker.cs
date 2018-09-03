using System; 
using System.Collections;
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

   private bool complete, debugMode, spawning, task1, task2;
   private float              maxPower;
   private int                count, highCount;
   private GameObject[]       pickupsArray;
   private GameObject[]       spawnPointsArray;
   private List<Collider>     others;
   private List<GameObject>   pickups;
   private TextMeshProUGUI    text_tracker;
   private Timekeeper         timeKeeper;

   private void Awake()
   {
      debugMode = Debug.isDebugBuild;

      text_tracker = GetComponent<TextMeshProUGUI>();
      if (!text_tracker) Debug.Log("FAIL: no text_tracker object variable!!");

      spawnPointsArray = GameObject.FindGameObjectsWithTag("Spawn_Good");
      Debug.Log("Number of spawnPoints: " + spawnPointsArray.Length);

      pickupsArray = GameObject.FindGameObjectsWithTag("GoodObject_01");

      others = new List<Collider>();
   }

   private void Start()
   {
      timeKeeper = FindObjectOfType<Timekeeper>();
      complete = spawning = task1 = task2 = false;
      text_tasklist.text = "Goal: Raise Thrust Cap to 60%\nMini-Goal: Collect Gas Canisters";

      StartCoroutine("DoSpawn");
   }

   public bool ClaimPickup(Collider other)
   {
      if (!others.Contains(other))
      {
         count -= 1;
         others.Add(other);
         text_tracker.text = count.ToString() + " Gas Canisters Remaining";
         return true;
      }
      return false;
   }

   private void DespawnAll()
   {
      foreach (GameObject spawnPoint in spawnPointsArray)
      {
         if (spawnPoint.transform.childCount != 0)
         {
            Destroy(spawnPoint.transform.GetChild(0).gameObject);
         }
      }
   }

   private IEnumerator DoSpawn()
   {
      Debug.Log("Spawntime: " + Time.time.ToString("F2"));
      spawning = true;

      DespawnAll();
      yield return new WaitForSeconds(0.2f);
      SpawnPercent(33); // TODO decide how to use this dynamically
      //SpawnAll();

      Array.Clear(pickupsArray, 0, pickupsArray.Length);
      pickupsArray = GameObject.FindGameObjectsWithTag("GoodObject_01");
      Debug.Log("Number of pickups: " + pickupsArray.Length);

      text_tracker.text = count.ToString() + " Gas Canisters Remaining";

      others = new List<Collider>();
      spawning = false;
      complete = false;
   }

   private bool FiftyFifty()
   {
      // On average, return 'True' ~half the time, and 'False' ~half the time.
      if (Mathf.FloorToInt(UnityEngine.Random.Range(0, 2)) == 1) return true;
      else return false;
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

   public void Restart()
   {
      StartCoroutine("DoSpawn");
   }

   private void SpawnAll()
   {
      count = highCount = 0;
      do { SpawnRandomSpawnpoint(); count++; }
      while (SpawnPointIsEmpty());
      highCount = count;
   }

   private int SpawnTally()
   {
      int spawnTally = 0;
      foreach (GameObject spawnPoint in spawnPointsArray)
      {
         if (spawnPoint.transform.childCount > 0) spawnTally++;
      }
      return spawnTally;
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

   private void TrackPickups()
   {
      count = highCount - (highCount - SpawnTally());
      if (!complete && !spawning)
      {
         if (count == 0)
         {
            complete = true;
            timeKeeper.Cease(highCount);
            maxPower = player.BoostMaxPower(0.1f); // 10% boost
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

   private void Update()
   {
      TrackPickups();
      if (Input.GetKeyDown(KeyCode.N) && debugMode) StartCoroutine("DoSpawn");
      if (Input.GetKeyDown(KeyCode.M) && debugMode) DespawnAll();
   }
}
