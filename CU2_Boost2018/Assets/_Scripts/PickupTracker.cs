using UnityEngine;
using UnityEngine.UI;

public class PickupTracker : MonoBehaviour {

   private bool         complete;
   private int          count;
   private GameObject[] pickups;
   private Text         readout;
   private Timekeeper   timeKeeper;

   public void Restart()
   {
      foreach (GameObject pickup in pickups) pickup.SetActive(true);
      complete = false;
   }

   private void Start ()
   {
      pickups = GameObject.FindGameObjectsWithTag("GoodObject_01");
      timeKeeper = FindObjectOfType<Timekeeper>();
      readout = GetComponent<Text>();
      complete = false;
   }

   private void TrackPickups()
   {
      count = 0;
      foreach (GameObject pickup in pickups)
      {
         if (pickup.activeSelf) count++;
      }
      readout.text = "Gas Canisters Remaining: " + count.ToString();

      if (!complete)
      {
         if (count == 0)
         {
            complete = true;
            timeKeeper.Cease(pickups.Length);
         }
      }
   }

   private void Update()
   {
      TrackPickups();
   }
}
