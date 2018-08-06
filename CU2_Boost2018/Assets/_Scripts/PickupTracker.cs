using UnityEngine.UI;
using UnityEngine;

public class PickupTracker : MonoBehaviour {
   GameObject[] pickups;
   Text readout;

   void Start ()
   {
      pickups = GameObject.FindGameObjectsWithTag("GoodObject_01");
      readout = GetComponent<Text>();
   }
	
	void Update ()
   {
      int count = 0;
      foreach (GameObject pickup in pickups)
      {
         if (pickup.activeSelf) count++;
      }
      readout.text = "Gas Canisters Remaining: " + count.ToString();
	}
}
