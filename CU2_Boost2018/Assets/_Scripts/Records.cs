using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Records : MonoBehaviour
{
   private const int RECORD_LIMIT = 11; // Limit personal records to limit clutter.

   private dreamloLeaderBoard leaderBoard;
   private Pilot pilot;
   private TextMeshProUGUI readout;

   private List<string> records = new List<string>();
   private readonly string[] comOne =     { "Cheater! ", "I don't believe it: ", "riiiiiight: ", "That's one for the record books! " };
   private readonly string[] comTwo =     { "Is it even possible? ", "Amazing: ", "Stupendous: ", "Lucky! "};
   private readonly string[] comThree =   { "Remarkable: ", "Off the charts! ", "Better than me... ", "Fantastic: "};
   private readonly string[] comFour =    { "Pretty good: ", "Better than average: ", "Hanging in there: ", "Solid: " };
   private readonly string[] comFive =    { "Not bad: ", "Average: ", "Nominal: ", "Acceptable: " };
   private readonly string[] comSix =     { "Keep practicing: ", "You're getting there: ", "not bad, but not great: ", "Mediocre: " };
   private readonly string[] comSeven =   { "I bet you can do better: ", "Practice makes perfect: ", "Details, details... ", "This could be improved: " };
   private readonly string[] comEight =   { "Are you even trying? ", "SMH: ", "Seriously? ", "Yeup.... " };

   public void AddRecord(float record)
   {
      string stringRecord = record.ToString("F2");

      // Tag each new record with a comment related to how fast it is:
      if (record <= 1) records.Add(comOne[OneOf(comOne)] + stringRecord + "\n");
      else if (record > 1 && record <= 3) records.Add(comTwo[OneOf(comTwo)] + stringRecord + "\n");
      else if (record > 3 && record <= 4) records.Add(comThree[OneOf(comThree)] + stringRecord + "\n");
      else if (record > 4 && record <= 5) records.Add(comFour[OneOf(comFour)] + stringRecord + "\n");
      else if (record > 5 && record <= 6) records.Add(comFive[OneOf(comFive)] + stringRecord + "\n");
      else if (record > 6 && record <= 8) records.Add(comSix[OneOf(comSix)] + stringRecord + "\n");
      else if (record > 8 && record <= 10) records.Add(comSeven[OneOf(comSeven)] + stringRecord + "\n");
      else if (record > 10) records.Add(comEight[OneOf(comEight)] + stringRecord + "\n");

      PrintRecords();
   }

   private int OneOf(string[] commentArray) { return Mathf.FloorToInt(Random.Range(0, commentArray.Length)); }

   private void PrintRecords()
   {
      int count = 0;
      readout.text = "<i><b>Time Records</b></i>\n";

      for (int i = 0; i < records.Count; i++)
      {
         if (records[i] != null) readout.text += records[i];
         count++;
         if (count >= RECORD_LIMIT) records.RemoveAt(0);
      }
      if (count > 0) readout.text += "<i>(seconds per canister)</i>\n";
      else readout.text += "<i>...gather all canisters to see your first record...</i>\n";
      readout.text += "Global High Score: " + ApplyColour.Blue + GlobalHighScore + ApplyColour.Close + " by: " + GlobalScorer;
      readout.text += "\nPersonal High Score: " + ApplyColour.Blue + HighScore + ApplyColour.Close;
   }

   private void Start()
   {
      leaderBoard = dreamloLeaderBoard.GetSceneDreamloLeaderboard();
      pilot = FindObjectOfType<Pilot>();
      readout = GetComponent<TextMeshProUGUI>();
      records.Clear();
      PrintRecords();
      //leaderBoard.LoadScores();
   }

   private int highScore = 0;
   public int HighScore { get { return highScore; } set { highScore = value; } }

   private int globalHighScore = 0;
   public int GlobalHighScore { get { return globalHighScore; } set { globalHighScore = value; } }

   private string globalScorer = null;
   public string GlobalScorer { get { return globalScorer; } set { globalScorer = value; } }

   public bool Parse()
   {
      StartCoroutine(ParseScores());
      if (HighScore > 0) return true;
      else return false;
   }

   private bool global = false;

   private IEnumerator ParseScores()
   {
      List<dreamloLeaderBoard.Score> scores = leaderBoard.ToListHighToLow();
      if (scores == null)
      {
         HighScore = -1;
         Debug.Log("null scores");
      }
      else
      {
         int rank = 0;
         foreach (dreamloLeaderBoard.Score record in scores)
         {
            rank++;
            string[] temp = record.playerName.Split('_');
            if (!global) // capture global high
            {
               GlobalHighScore = record.score;
               foreach (string str in temp) Debug.Log(str);
               GlobalScorer = temp[0];
               global = true;
               //Debug.Log("temp[0] == pilot.ID || " + temp[0] + " == " + pilot.ID);
            }
            if (temp[0] == pilot.ID && temp[1] == pilot.Unique)
            {
               Debug.Log(temp[0] + " <-> " + record.score);
               HighScore = record.score;
               break;
            }
         }
         Debug.Log("Rank: " + rank + " HS: " + HighScore); // TODO do something with rank?
      }
      yield return new WaitForSeconds(1.6f); // TODO get rid of magic number
      PrintRecords();
   }
}
