using UnityEngine;
using TMPro;

public class Pilot_ID_Field : MonoBehaviour
{
   [SerializeField] TextMeshProUGUI tmpUGUI;

   public string PilotID { get { return tmpUGUI.text;  } set { tmpUGUI.text = value; } }

   private void GetID() { pilot.ID = PilotID; Debug.Log(pilot.ID); }

   public void SetID() { GetID(); } 

   private Pilot pilot;

   private void Awake() // changed from Start due to odd missing reference errors from SetID
   {
      pilot = FindObjectOfType<Pilot>();
      if (!pilot) Debug.LogWarning("Pilot_ID no Pilot Reference");
      if (!tmpUGUI) Debug.LogWarning("Pilot_ID no tmpUGUI Reference");
      Debug.Log(PilotID);
   }
}
