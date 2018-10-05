using UnityEngine;

public class Pilot : MonoBehaviour
{
   private static int       highScore;
   private static int       rank;
   private static string    id;
   private static string    unique;

   public int    HighScore { get { return highScore; }  set { highScore = value; } }
   public int    Rank      { get { return rank; }       set { rank = value; } }
   public string ID        { get { return id; }         set { id = value; } }
   public string Unique    { get { return unique; }     set { unique = value; } }

   private void Awake()
   {
      highScore = 0;
      rank = 0;
      id = "Bubblenaut-" + System.Environment.UserName;
      unique = SystemInfo.deviceUniqueIdentifier;
      if (unique.Length < 16) unique = "Error_Generating_Device_Unique_Identifier"; 
   }
}
