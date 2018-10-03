using UnityEngine;

public class Pilot : MonoBehaviour
{
   private int       highScore;
   private string    id;
   private string    unique;

   public int     HighScore { get { return highScore; }  set { highScore = value; } }
   public string  ID        { get { return id; }         set { id = value; } }
   public string  Unique    { get { return unique; }     set { unique = value; } }

   private void Awake()
   {
      ID = "Bubblenaut-" + System.Environment.UserName;
      Unique = SystemInfo.deviceUniqueIdentifier;
      if (Unique == "na" || Unique == null) Unique = "WebPlayer";
   }
}
