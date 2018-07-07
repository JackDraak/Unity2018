using UnityEngine;
using UnityEngine.UI;

public class Display : MonoBehaviour
{
   [SerializeField] Terminal connectedToTerminal;

   // TODO calculate these two if possible?
   [SerializeField] int charactersWide = 58;
   [SerializeField] int charactersHigh = 18;

   Text screenText;

   private void Start()
   {
      screenText = GetComponentInChildren<Text>();
      WarnIfTerminalNotConneced();
   }

   // "Update" is akin to monitor refresh
   private void Update()
   {
      if (connectedToTerminal)
      {
         screenText.text = connectedToTerminal.GetDisplayBuffer(charactersWide, charactersHigh);
      }
   }

   private void WarnIfTerminalNotConneced()
   {
      if (!connectedToTerminal)
      {
         Debug.LogWarning("Display not connected to a terminal");
      }
   }
} 