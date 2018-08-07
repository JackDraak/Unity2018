using UnityEngine.UI;
using UnityEngine;

public class Timekeeper : MonoBehaviour {

   private float elapsed, endTime, startTime;
   private bool finished, started;
   private Text readout;
   private Records records;

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

   public void Init()
   {
      started = false;
      finished = false;
      endTime = 0f;
      startTime = 0f;
   }

   void Start()
   {
      readout = GetComponent<Text>();
      records = FindObjectOfType<Records>();
      Init();
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
         elapsed = (Mathf.FloorToInt((endTime - startTime) * 10)) / 10f;
         readout.text = "Collected in: " + elapsed.ToString() + " seconds";
      }
   }
}
