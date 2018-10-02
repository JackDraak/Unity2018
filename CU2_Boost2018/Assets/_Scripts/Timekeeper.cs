using TMPro;
using UnityEngine;

public class Timekeeper : MonoBehaviour
{
   public bool Running { get { return started; } }

   private bool finished = false;
   private bool started = false;
   private dreamloLeaderBoard leaderboard;
   private float elapsed = 0f;
   private float endTime = 0f;
   private float startTime = 0f;
   private Records records = null;
   private TextMeshProUGUI readout = null;
   private TMP_InputField inputField;

   public void Begin()
   {
      startTime = Time.time;
      started = true;
   }

   public void Cease(int count)
   {
      endTime = Time.time;
      elapsed = (Mathf.FloorToInt((endTime - startTime) * 10)) / 10f; // Get 1 decimal place.
      finished = true;
      float ratio = (elapsed / count) * 100f;
      //leaderboard.LoadScores();
      //List<dreamloLeaderBoard> scores = new List<dreamloLeaderBoard>();
      var myScores = leaderboard.ToListHighToLow();
      foreach (var score in myScores)
      {
         string myOut = score.playerName;
         myOut += " Name: ";
         myOut += score.score;
         myOut += " Score: ";
         Debug.Log(myOut);
      }
      int boardScore = 5000 - Mathf.FloorToInt(ratio);
      if (boardScore < 0) boardScore = 0;
      Debug.Log(boardScore);
      ratio = Mathf.FloorToInt(ratio) / 100f; // Get 2 decimal places.
      leaderboard.AddScore(inputField.text, boardScore, Mathf.FloorToInt(ratio * 100)); // TODO allow players to enter initials or something
      records.AddRecord(ratio);
   }

   public void Restart()
   {
      endTime = 0f;
      finished = false;
      started = false;
      startTime = 0f;
   }

   private void Start()
   {
      inputField = FindObjectOfType<TMP_InputField>();
      inputField.text = "Mr. Bo Demo";
      inputField.ActivateInputField();
      inputField.enabled = true;
      inputField.interactable = true;
      // get pilot name

      readout = GetComponent<TextMeshProUGUI>();
      records = FindObjectOfType<Records>();
      leaderboard = dreamloLeaderBoard.GetSceneDreamloLeaderboard();
      Restart();
   }

   private void Update()
   {
      if (!started && !finished) readout.text = "Touching the Controls Will Start the Timer: 0.0 seconds";
      else if (started && !finished)
      {
         float elapsed = (Mathf.FloorToInt((Time.time - startTime) * 10)) / 10f; // Get 1 decimal place.
         readout.text = elapsed.ToString("F1");
         readout.text += " seconds (Elapsed time)";
      }
      else if (finished)
      {
         elapsed = (Mathf.FloorToInt((endTime - startTime) * 10)) / 10f; // Get 1 decimal place.
         readout.text = "tap 'R' to retry; your prior run took: " + elapsed.ToString("F1") + " seconds";
      }
   }
}
