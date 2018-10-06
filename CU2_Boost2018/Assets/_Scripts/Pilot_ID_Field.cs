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

   public bool Enable { set { inputField.interactable = value; } get { return inputField.interactable; } }

   public string PilotID { get { return tmpUGUI.text;  } set { tmpUGUI.text = value; } }

   public void SetID()
   {
      pilot.ID = PilotID;
      records.Parse();
      Enable = false;
   }

   public void Toggle() { inputField.interactable = !inputField.interactable; }
}
