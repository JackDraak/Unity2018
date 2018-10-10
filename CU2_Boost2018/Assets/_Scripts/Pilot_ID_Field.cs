using UnityEngine;
using TMPro;

public class Pilot_ID_Field : MonoBehaviour
{
   [SerializeField] TextMeshProUGUI tmpUGUI;

   private Pilot pilot;
   private Records records;
   private TMP_InputField inputField;

   private void Awake()
   {
      inputField = GetComponent<TMP_InputField>();
      records = FindObjectOfType<Records>();
      pilot = FindObjectOfType<Pilot>();
   }

   public bool Enable { get { return inputField.interactable; } set { inputField.interactable = value; } }

   public string PilotID { get { return tmpUGUI.text;  } set { tmpUGUI.text = value; } }

   public void SetID()
   {
      // TODO deal w/ empty player names more gracefully
      // TODO deal with profanity?
      // TODO need to fix reversion to 'Pilot ID' when changign resolultion?
      if (PilotID == "") PilotID = "TheUnknownComic";
      pilot.ID = PilotID;
      records.Parse();
      Enable = false;
   }

   public void Toggle() { inputField.interactable = !inputField.interactable; }
}
