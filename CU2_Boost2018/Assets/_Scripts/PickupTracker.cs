using UnityEngine.UI;
using UnityEngine;

public class PickupTracker : MonoBehaviour {

   private bool complete;
   private int count = 0;
   private GameObject[] pickups;
   private Text readout;
   private Timekeeper timeKeeper;

   void Start ()
   {
      timeKeeper = FindObjectOfType<Timekeeper>();
      pickups = GameObject.FindGameObjectsWithTag("GoodObject_01");
      readout = GetComponent<Text>();
      complete = false;
   }
	
	void Update ()
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

   public void Restart()
   {
      complete = false;
      count = 0;
      foreach (GameObject pickup in pickups)
      {
         count++;
         pickup.SetActive(true);
      }
   }
}
