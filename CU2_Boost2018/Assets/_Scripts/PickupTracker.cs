using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PickupTracker : MonoBehaviour {

   [SerializeField] Color goalHigh = Color.clear;
   [SerializeField] Color goalLow = Color.clear;
   [SerializeField] Image goalFill;
   [SerializeField] Player player;
   [SerializeField] Slider goalSlider;
   [SerializeField] TextMeshProUGUI text_tasklist;

   private bool            complete, task1, task2;
   private float           maxPower;
   private int             count, highCount;
   private GameObject[]    pickups;
   private TextMeshProUGUI text_tracker;
   private Timekeeper      timeKeeper;

   public void Restart()
   {
      foreach (GameObject pickup in pickups) pickup.SetActive(true);
      complete = false;
      highCount = 0;
   }

   private void Start ()
   {
      pickups = GameObject.FindGameObjectsWithTag("GoodObject_01");
      timeKeeper = FindObjectOfType<Timekeeper>();
      text_tracker = GetComponent<TextMeshProUGUI>();
      complete = task1 = task2 = false;
      text_tasklist.text = "Goal: Raise Thrust Cap to 60%\nMini-Goal: Collect Gas Canisters";
   }

   private void TrackPickups()
   {
      count = 0;
      foreach (GameObject pickup in pickups)
      {
         if (pickup.activeSelf) count++;
         if (count > highCount) highCount = count;
      }
      text_tracker.text = count.ToString() + " Gas Canisters Remaining";

      if (!complete)
      {
         if (count == 0)
         {
            complete = true;
            timeKeeper.Cease(pickups.Length);
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

   private void Update()
   {
      TrackPickups();
   }
}
