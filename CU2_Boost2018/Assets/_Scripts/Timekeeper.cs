using UnityEngine;
using UnityEngine.UI;

public class Timekeeper : MonoBehaviour {

   private bool finished;
   private bool started;
   private float elapsed;
   private float endTime;
   private float startTime;
   private Records records;
   private Text readout; 

   public void Begin()
   {
      startTime = Time.time;
      started = true;
   }

   public void Cease(int count)
   {
      endTime = Time.time;
      elapsed = (Mathf.FloorToInt((endTime - startTime) * 10)) / 10f;
      finished = true;
      float ratio = (elapsed / count) * 100f;
      ratio = Mathf.FloorToInt(ratio) / 100f;
      records.Add(ratio);
   }

   public void Restart()
   {
      started = false;
      finished = false;
      endTime = 0f;
      startTime = 0f;
   }

   private void Start()
   {
      readout = GetComponent<Text>();
      records = FindObjectOfType<Records>();
      Restart();
   }

   private void Update()
   {
      if (!started && !finished)
      {
         readout.text = "Touching the Controls Will Start the Timer: 0.0 seconds";
      }
      else if (started && !finished)
      {
         float elapsed = (Mathf.FloorToInt((Time.time - startTime) * 10)) / 10f;
         readout.text = "Elapsed Time: " + elapsed.ToString() + " seconds";
      }
      else if (finished)
      {
         elapsed = (Mathf.FloorToInt((endTime - startTime) * 10)) / 10f;
         readout.text = "tap 'R' to retry; your prior run took: " + elapsed.ToString() + " seconds";
      }
   }
}
