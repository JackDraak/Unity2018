using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Records : MonoBehaviour {

   private List<float> records = new List<float>();
   private Text readout;

   public void Add(float record)
   {
      records.Add(record);
   }

   private void Start()
   {
      readout = GetComponent<Text>();
      records.Clear();
   }

   public void Update()
   {
      readout.text = "[R to restart]\nTime Records\n(in seconds per canister)\n";
      foreach (float record in records)
      {
         if (record < 1)
         {
            readout.text += "Cheater! " + record.ToString() + "\n";
         }
         else if (record > 10)
         {
            readout.text += "I bet you can do better! " + record.ToString() + "\n";
         }
         else readout.text += record.ToString() + "\n";
      }
   }
}
