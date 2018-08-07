using UnityEngine.UI;
using UnityEngine;

public class Timekeeper : MonoBehaviour {

   private float endTime = 0f;
   private bool finished, started;
   private float startTime = 0f;
   private Text readout;

   void Start()
   {
      readout = GetComponent<Text>();
      finished = false;
      started = false;
   }

   void Update()
   {
      if (!started && !finished)
      {
         readout.text = "Use Controls to Begin Timer";
      }
      else if (started && !finished)
      {
         float elapsed = (Mathf.FloorToInt((Time.time - startTime) * 10)) / 10f;
         readout.text = "Elapsed Time: " + elapsed.ToString() + " seconds";
      }
      else if (finished)
      {
         float elapsed = (Mathf.FloorToInt((endTime - startTime) * 10)) / 10f;
         readout.text = "Collected in: " + elapsed.ToString() + " seconds";
      }
   }

   public void Begin()
   {
      startTime = Time.time;
      started = true;
   }

   public void Cease()
   {
      endTime = Time.time;
      finished = true;
   }

   public void Reset()
   {
      started = false;
      finished = false;
   }
}
