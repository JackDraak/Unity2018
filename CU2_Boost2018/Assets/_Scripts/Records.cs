using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Records : MonoBehaviour
{
   [SerializeField] TextMeshProUGUI highScoreText;

   private const int RECORD_LIMIT = 6; // Limit personal records to limit clutter.

   private bool global = false;
   private bool webGL = false;
   private dreamloLeaderBoard leaderBoard;
   private int totalRankings;
   private List<dreamloLeaderBoard.Score> highScores;
   private Pilot pilot;
   private Pilot_ID_Field pilot_ID_Field;
   private string[] highStrings;
   private TextMeshProUGUI readout;
   private Timekeeper timeKeeper;

   private int globalHighScore = 0;
   public int GlobalHighScore { get { return globalHighScore; } set { globalHighScore = value; } }

   private string globalScorer = null;
   public string GlobalScorer { get { return globalScorer; } set { globalScorer = value; } }

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
      // Tag each new record with a comment related to how fast it is:
      string stringRecord = record.ToString("F2");
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

   public IEnumerator GetHighScores()
   {
      // TODO deal w/ empty player names more gracefully
      // TODO deal with profanity in usernames?

      if (pilot_ID_Field.PilotID.Length <= 2)
      {
         pilot_ID_Field.PilotID = "TheUnknownComic";
         pilot.ID = "FrenchConnection";

      }
      else if (pilot_ID_Field.PilotID == "BukarooBonzai") pilot.MasterPilot = true;
      else pilot.MasterPilot = false;

      yield return new WaitForSeconds(1.0f); // TODO Optimize this delay?
      highStrings = new string[11];
      int rank = 0;
      if (!webGL)
      {
         highScores = leaderBoard.ToListHighToLow();
         highStrings[0] = ApplyColour.Blue + "Rank - Name - Score" + ApplyColour.Close + "\n";
         global = false;
         GlobalHighScore = 0;
         GlobalScorer = "";
         pilot.HighScore = 0;
         pilot.Rank = 0;
         foreach (dreamloLeaderBoard.Score record in highScores)
         {
            string[] temp = record.playerName.Split(timeKeeper.Splitter);
            rank++;
            if (!global)
            {
               GlobalHighScore = record.score;
               GlobalScorer = temp[0];
               global = true;
            }
            if (temp[0] == pilot.ID && temp[1] == pilot.Unique)
            {
               pilot.HighScore = record.score;
               pilot.Rank = rank;
            }
            if (rank <= 10)
            {
               highStrings[rank] = (rank).ToString() + ". " + temp[0];
               highStrings[rank] += " " + record.score + "\n";
            }
         }
         Debug.Log("Records:GetHighScores() records: " + rank + " playerHigh: " + pilot.HighScore 
            + ", Player Rank #" + pilot.Rank + ", ID: " + pilot.ID + " fieldID: " 
            + pilot_ID_Field.PilotID + " Master Pilot: " + pilot.MasterPilot);
         totalRankings = rank;
      }
      else highStrings[0] = "No Global Leader Board\nin WebGL Version";
      highScoreText.text = "";
      foreach (string highScore in highStrings) highScoreText.text += highScore;
      PrintRecords();
   }

   private int OneOf(string[] commentArray) { return Mathf.FloorToInt(Random.Range(0, commentArray.Length)); }

   // Called-by: Timekeeper.Cease() & Pilot_ID_Field.SetID()
   public void Parse() { StartCoroutine(GetHighScores()); }

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
      string rankText;
      if (pilot.Rank == 0) rankText = "Unranked";
      else rankText = pilot.Rank.ToString();
      if (!webGL)
      {
         readout.text += "Global High Score: " + ApplyColour.Blue + GlobalHighScore + ApplyColour.Close + " by: " + GlobalScorer;
         readout.text += "\nPersonal High Score: " + ApplyColour.Blue + pilot.HighScore + ApplyColour.Close + " (rank: "
            + ApplyColour.Blue + rankText + ApplyColour.Close + " of " + ApplyColour.Blue + totalRankings + ApplyColour.Close + ")";
      }
      else readout.text += "Global Rankings Disabled for WebGL Version.";
   }

   private void Start()
   {
      leaderBoard = dreamloLeaderBoard.GetSceneDreamloLeaderboard();
      pilot = FindObjectOfType<Pilot>();
      pilot_ID_Field = FindObjectOfType<Pilot_ID_Field>();
      readout = GetComponent<TextMeshProUGUI>();
      timeKeeper = FindObjectOfType<Timekeeper>();

      if (Application.platform == RuntimePlatform.WebGLPlayer) webGL = true;
      else leaderBoard.LoadScores();

      records.Clear();
      pilot_ID_Field.PilotID = pilot.ID;
      pilot_ID_Field.SetID(); // triggers a parse
   }
}
