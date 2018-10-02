using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PilotName : MonoBehaviour
{
   private TMP_InputField pilotName;
   private string pilot = "Mr. Demo";

   private void OnMouseDown() { GetPilotName(); }

   public string Pilot { get { return pilot; } set { pilot = value; } }

   public void GetPilotName() { pilot = pilotName.text; }

   public void SetPilotName(string name) { pilotName.text = name; }

   private void Start() { pilotName = gameObject.GetComponent<TMP_InputField>(); }
}
