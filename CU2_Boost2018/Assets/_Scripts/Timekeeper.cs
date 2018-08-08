using UnityEngine;
using UnityEngine.UI;

public class Timekeeper : MonoBehaviour {

   private float elapsed;
   private float endTime;
   private bool finished;
   private Text readout;
   private Records records;
   private bool started;
   private float startTime;

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
         readout.text = "Use Controls to Start Timer: 0.0 seconds";
      }
      else if (started && !finished)
      {
         float elapsed = (Mathf.FloorToInt((Time.time - startTime) * 10)) / 10f;
         readout.text = "Elapsed Time: " + elapsed.ToString() + " seconds";
      }
      else if (finished)
      {
         elapsed = (Mathf.FloorToInt((endTime - startTime) * 10)) / 10f;
         readout.text = "'R' to retry, last run took: " + elapsed.ToString() + " seconds";
      }
   }
}
