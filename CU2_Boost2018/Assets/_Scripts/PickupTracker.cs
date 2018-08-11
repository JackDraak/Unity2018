using UnityEngine;
using UnityEngine.UI;

public class PickupTracker : MonoBehaviour {

   private bool complete;
   private GameObject[] pickups;
   private int count = 0;
   private Text readout;
   private Timekeeper timeKeeper;

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

   private void Start ()
   {
      timeKeeper = FindObjectOfType<Timekeeper>();
      pickups = GameObject.FindGameObjectsWithTag("GoodObject_01");
      readout = GetComponent<Text>();
      complete = false;
   }
	
	private void Update ()
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
}
