using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Records : MonoBehaviour
{
   [SerializeField] TextMeshProUGUI highScoreText;

   private const int RECORD_LIMIT = 6; // Limit personal records to limit clutter.
   private const int NETWORK_DELAY = 1;

   private bool global = false;
   private dreamloLeaderBoard leaderBoard;
   private List<dreamloLeaderBoard.Score> highScores;
   private Pilot pilot;
   private Pilot_ID_Field pilot_ID_Field;
   private TextMeshProUGUI readout;
   private Timekeeper timeKeeper;

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
      Parse();
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
      pilot_ID_Field = FindObjectOfType<Pilot_ID_Field>();
      readout = GetComponent<TextMeshProUGUI>();
      timeKeeper = FindObjectOfType<Timekeeper>();

      records.Clear();
      leaderBoard.LoadScores();
      Parse();
   }

   private int highScore = 0;
   public int HighScore { get { return highScore; } set { highScore = value; } }

   private int globalHighScore = 0;
   public int GlobalHighScore { get { return globalHighScore; } set { globalHighScore = value; } }

   private string globalScorer = null;
   public string GlobalScorer { get { return globalScorer; } set { globalScorer = value; } }

   public bool Parse()
   {
      pilot_ID_Field.SetID(); // TODO recent change
      StartCoroutine(ParseScores());
      if (HighScore > 0) return true;
      else return false;
   }

   public IEnumerator GetHighScores()
   {
      yield return WaitFor.Frames(15);
      global = false;
      highScores = leaderBoard.ToListHighToLow();
      int rank = 0;
      string[] highStrings = new string[11];
      highStrings[0] = ApplyColour.Blue + "#. Top-Ten HighScore" + ApplyColour.Close + "\n";
      foreach (dreamloLeaderBoard.Score record in highScores)
      {
         string[] temp = record.playerName.Split(timeKeeper.Splitter); // TODO deal with code duplication
         if (!global) // capture global high
         {
            GlobalHighScore = record.score;
            //foreach (string str in temp) Debug.Log(str);
            GlobalScorer = temp[0];
            global = true;
         }

         rank++;
         highStrings[rank] = (rank).ToString() + ". " + temp[0]; //pilot.ID;
         highStrings[rank] += " " + record.score + "\n"; //temp[1]; //pilot.Unique
         if (rank == 10) break;
      }
      Debug.Log("Rank: " + rank + " HS: " + HighScore); // TODO do something with rank?
      highScoreText.text = "";
      foreach (string highScore in highStrings) { highScoreText.text += highScore; }
   }

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
         StartCoroutine(GetHighScores());
         yield return new WaitForSeconds(NETWORK_DELAY); // TODO get rid of this alltogether?
         int rank = 0;
         foreach (dreamloLeaderBoard.Score record in scores)
         {
            rank++;
            string[] temp = record.playerName.Split(timeKeeper.Splitter);
            //if (!global) // capture global high
            //{
            //   GlobalHighScore = record.score;
            //   foreach (string str in temp) Debug.Log(str);
            //   GlobalScorer = temp[0];
            //   global = true;
            //}
            if (temp[0] == pilot.ID && temp[1] == pilot.Unique)
            {
               Debug.Log(temp[0] + " <-> " + record.score);
               HighScore = record.score;
               break;
            }
         }
         Debug.Log("Rank: " + rank + " HS: " + HighScore); // TODO do something with rank?
      }
      yield return new WaitForSeconds(NETWORK_DELAY); // TODO get rid of this alltogether?
      PrintRecords();
   }
}
