﻿using UnityEngine;
using TMPro;

public class Pilot_ID_Field : MonoBehaviour
{
   [SerializeField] TextMeshProUGUI tmpUGUI;
   public string PilotID { get { return tmpUGUI.text;  } set { tmpUGUI.text = value; } }
   public void SetID() { GetID(); } 

   private void GetID() { pilot.ID = PilotID; records.Parse(); Debug.Log("(GetID)Pilot_ID_Field "+ pilot.ID + " assigned from _Field to Pilot.ID"); }

   private Pilot pilot;
   private Records records;

   private void Awake() // changed from Start due to odd missing reference errors from SetID
   {
      records = FindObjectOfType<Records>();
      pilot = FindObjectOfType<Pilot>();
      if (!pilot) Debug.LogWarning("Pilot_ID no Pilot Reference");
      if (!tmpUGUI) Debug.LogWarning("Pilot_ID no tmpUGUI Reference");
      //Debug.Log("(Awake)Pilot_ID_Field: " + PilotID);
   }
}
