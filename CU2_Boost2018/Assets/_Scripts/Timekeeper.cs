using TMPro;
using UnityEngine;

public class Timekeeper : MonoBehaviour
{
   public bool Running { get { return started; } }
   public char Splitter { get { return '~'; } }

   bool finished = false;
   bool started = false;
   dreamloLeaderBoard leaderboard;
   float elapsed = 0f;
   float endTime = 0f;
   float startTime = 0f;
   Pilot pilot;
   Player player;
   Records records = null;
   TextMeshProUGUI timerText = null;

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
      int boardScore = 5000 - Mathf.FloorToInt(ratio);
      if (boardScore < 0) boardScore = 1;
      Debug.Log("New score: " + boardScore);
      ratio = Mathf.FloorToInt(ratio) / 100f; // Get 2 decimal places.
      string customName = pilot.ID + Splitter + pilot.Unique;
      if (Application.platform != RuntimePlatform.WebGLPlayer)
         leaderboard.AddScore(customName, boardScore, Mathf.FloorToInt(ratio * 100), pilot.Unique); 
      records.AddRecord(ratio);
      records.Parse();
   }

   void DoTimerUpdate()
   {
      if (!started && !finished) timerText.text = 
            "Touching the Controls Will Start the Timer: " + ApplyColour.Green + "0.0 seconds" + ApplyColour.Close;
      else if (started && !finished)
      {
         float elapsed = (Mathf.FloorToInt((Time.time - startTime) * 10)) / 10f; // Get 1 decimal place.
         timerText.text = ApplyColour.Green + elapsed.ToString("F1") + ApplyColour.Close;
         timerText.text += " seconds (Elapsed time)";
      }
      else if (finished)
      {
         elapsed = (Mathf.FloorToInt((endTime - startTime) * 10)) / 10f; // Get 1 decimal place.
         if (player.casualMode)
            timerText.text = "tap " + ApplyColour.Green + "R" + ApplyColour.Close + " to retry; your prior run took: "
            + ApplyColour.Green + elapsed.ToString("F1") + ApplyColour.Close + " seconds";
         else
            timerText.text = "Your prior run took: " + ApplyColour.Green + elapsed.ToString("F1") + ApplyColour.Close + " seconds";
      }
   }

   public void Restart()
   {
      endTime = 0f;
      finished = false;
      started = false;
      startTime = 0f;
   }

   void Start()
   {
      leaderboard = dreamloLeaderBoard.GetSceneDreamloLeaderboard();
      pilot = FindObjectOfType<Pilot>();
      player = FindObjectOfType<Player>();
      records = FindObjectOfType<Records>();
      timerText = GetComponent<TextMeshProUGUI>();
      Restart();
   }

   void Update() { DoTimerUpdate(); }
}
