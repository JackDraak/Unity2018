using UnityEngine;
using TMPro;

public class Pilot_ID_Field : MonoBehaviour
{
   [SerializeField] TextMeshProUGUI tmpUGUI;

   Pilot pilot;
   Records records;
   TMP_InputField inputField;

   void Awake()
   {
      inputField = GetComponent<TMP_InputField>();
      records = FindObjectOfType<Records>();
      pilot = FindObjectOfType<Pilot>();
   }

   //void OnRenderObject()
   //{
   //   if (PilotID != pilot.ID)
   //   {
   //      Debug.Log("OnRenderObject ID mismatch " + inputField.text + " - " + PilotID + " - " + pilot.ID);
   //      //PilotID = pilot.ID; // too greedy
   //   }
   //}

   public bool Enable { get { return inputField.interactable; } set { inputField.interactable = value; } }

   public string PilotID { get { return tmpUGUI.text;  } set { tmpUGUI.text = value; } }

   public void SetID()
   {
      // TODO need to fix reversion to 'Pilot ID' when changing resolultion? LOWPRI
      pilot.ID = PilotID;
      records.Parse();
      Enable = false;
   }

   public void Toggle() { inputField.interactable = !inputField.interactable; }
}
