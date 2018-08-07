using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Records : MonoBehaviour {

   private Text readout;
   private List<string> records = new List<string>();
   private string[] comOne = { "Cheater! ", "I don't believe it: ", "riiiiiight: ", "That's one for the record books! " };
   private string[] comTwo = { "Is it even possible? ", "Amazing: ", "Stupendous: ", "Lucky! "};
   private string[] comThree = { "Remarkable: ", "Off the charts! ", "Better than me... ", "Fantastic: "};
   private string[] comFour = { "Pretty good: ", "Better than average: ", "Hanging in there: ", "Solid: " };
   private string[] comFive = { "Not bad: ", "Average: ", "Nominal: ", "Acceptable: " };
   private string[] comSix = { "Keep practicing: ", "You're getting there: ", "not bad, but not great: ", "Mediocre: " };
   private string[] comSeven = { "I bet you can do better: ", "Practice makes perfect: ", "Details, details... ", "This could be improved: " };
   private string[] comEight = { "Are you even trying? ", "SMH: ", "Seriously? ", "Yeup.... " };

   public void Add(float record)
   {
      if (record <= 1)
      {
         records.Add(comOne[OneOf(comOne)] + record.ToString() + "\n");
      }
      else if (record > 1 && record <= 3)
      {
         records.Add(comTwo[OneOf(comTwo)] + record.ToString() + "\n");
      }
      else if (record > 3 && record <= 4)
      {
         records.Add(comThree[OneOf(comThree)] + record.ToString() + "\n");
      }
      else if (record > 4 && record <= 5)
      {
         records.Add(comFour[OneOf(comFour)] + record.ToString() + "\n");
      }
      else if (record > 5 && record <= 6)
      {
         records.Add(comFive[OneOf(comFive)] + record.ToString() + "\n");
      }
      else if (record > 6 && record <= 8)
      {
         records.Add(comSix[OneOf(comSix)] + record.ToString() + "\n");
      }
      else if (record > 8 && record <= 10)
      {
         records.Add(comSeven[OneOf(comSeven)] + record.ToString() + "\n");
      }
      else if (record > 10)
      {
         records.Add(comEight[OneOf(comEight)] + record.ToString() + "\n");
      }
   }

   private int OneOf(string[] comment)
   {
      return Mathf.FloorToInt(Random.Range(0, comment.Length));
   }

   private void Start()
   {
      readout = GetComponent<Text>();
      records.Clear();
   }

   public void Update()
   {
      readout.text = "Time Records\n";
      foreach (string record in records)
      {
         readout.text += record;
      }
      readout.text += "(seconds per canister)";
   }
}
