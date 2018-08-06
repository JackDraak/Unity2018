using UnityEngine.UI;
using UnityEngine;

public class PickupTracker : MonoBehaviour {
   private GameObject[] pickups;
   private Text readout;
   private Timekeeper timeKeeper;
   private bool complete;

   void Start ()
   {
      timeKeeper = FindObjectOfType<Timekeeper>();
      pickups = GameObject.FindGameObjectsWithTag("GoodObject_01");
      readout = GetComponent<Text>();
      complete = false;
   }
	
	void Update ()
   {
      int count = 0;
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
            timeKeeper.Cease();
         }
      }
	}
}
