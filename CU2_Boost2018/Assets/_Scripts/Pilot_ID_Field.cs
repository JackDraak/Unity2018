using UnityEngine;
using TMPro;

public class Pilot_ID_Field : MonoBehaviour
{
   [SerializeField] TextMeshProUGUI tmpUGUI;

   Pilot pilot;
   Records records;

   public string PilotID { get { return tmpUGUI.text;  } set { tmpUGUI.text = value; } }

   private void Start()
   {
      pilot = FindObjectOfType<Pilot>();
      records = FindObjectOfType<Records>();
   }

   public void UpdateID() { pilot.ID = PilotID; records.Parse(); }
}
