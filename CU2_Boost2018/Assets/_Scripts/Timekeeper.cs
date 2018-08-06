using UnityEngine.UI;
using UnityEngine;

public class Timekeeper : MonoBehaviour {

   private float startTime = 0f;
   Text readout;

   void Start()
   {
      readout = GetComponent<Text>();
   }

   void Update()
   {
      if (startTime != 0)
      {
         float elapsed = (Mathf.FloorToInt((Time.time - startTime) * 10)) / 10f;
         readout.text = "Elapsed Time: " + elapsed.ToString();
      }
      else
      {
         readout.text = "Elapsed Time: 0.0";
      }
   }

   public void Begin()
   {
      startTime = Time.time;
   }
}
