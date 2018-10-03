using UnityEngine;
using TMPro;

public class Pilot_ID_Field : MonoBehaviour
{
   [SerializeField] TextMeshProUGUI tmpUGUI;

   Pilot pilot;

   public string PilotID { get { return tmpUGUI.text;  } set { tmpUGUI.text = value; } }

   public void SetID() { pilot.ID = tmpUGUI.text; }

   private void Start() { pilot = FindObjectOfType<Pilot>(); }

   public void UpdateID() { pilot.ID = PilotID; }
}
