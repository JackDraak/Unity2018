using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Pilot : MonoBehaviour
{
   private int       highScore;
   private List<int> scores;
   private string    id;
   private string    unique;

   public int     HighScore { get { return highScore; }  set { highScore = value; } }
   public string  ID        { get { return id; }         set { id = value; } }
   public string  Unique    { get { return unique; }     set { unique = value; } }

  public void AddScore(int score) { scores.Add(1); } // TODO do something here?

   private void Start()
   {
      ID = "Bubbler_" + System.Environment.UserName;
      Unique = SystemInfo.deviceUniqueIdentifier;
      Debug.Log(ID);
      scores = new List<int>();
      //pilots = new List<Pilot>();
      //pilots.Add(player);
   }
}
