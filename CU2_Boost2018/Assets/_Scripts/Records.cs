using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Records : MonoBehaviour {

   private List<float> records = new List<float>();
   private Text readout;

   private void Start()
   {
      readout = GetComponent<Text>();
      records.Clear();
   }

   public void Add(float record)
   {
      records.Add(record);
   }

   public void Update()
   {
      readout.text = "Time per canister records\n{tap R to restart)\n";
      foreach (float record in records)
      {
         if (record < 1)
         {
            readout.text += "Cheater! " + record.ToString() + "\n";
         }
         else readout.text += record.ToString() + "\n";
      }
   }
}
