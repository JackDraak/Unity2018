﻿using UnityEngine;

public class Pilot : MonoBehaviour
{
   private static bool      masterPilot = false;
   private static int       highScore;
   private static int       rank;
   private static string    id;
   private static string    unique;

   private Pilot_ID_Field pilot_ID_Field;

   private void Awake()
   {
      highScore = 0;
      rank = 0;
      id = "Bubblenaut-" + System.Environment.UserName;
      if (Application.platform == RuntimePlatform.WebGLPlayer) unique = "WebGL_Player_NOID";
      else unique = SystemInfo.deviceUniqueIdentifier;
      if (unique.Length < 16) unique = "Error_Generating_Device_Unique_Identifier";
   }

   public int HighScore { get { return highScore; } set { highScore = value; } }

   public string ID
   {
      get { return id; }
      set
      {
         Debug.Log("Pilot.ID(" + value + ")");
         if (value == "")
         {
            Debug.Log("EmptyID-Return");
            return;
         }
         else 
         {
            Debug.Log("SetID: " + value);
            id = value;
            pilot_ID_Field.PilotID = value;
         }
      }
   }

   public bool MasterPilot { get { return masterPilot; } set { masterPilot = value; } }

   public int Rank { get { return rank; } set { rank = value; } }

   private void Start() { pilot_ID_Field = FindObjectOfType<Pilot_ID_Field>(); }

   public string Unique { get { return unique; } }
}
