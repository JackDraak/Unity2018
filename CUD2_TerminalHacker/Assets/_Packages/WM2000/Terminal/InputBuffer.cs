using UnityEngine;

public class InputBuffer
{
   string currentInputLine; // todo private
   AudioSource audioSource;

   public delegate void OnCommandSentHandler(string command);
   public event OnCommandSentHandler onCommandSent;
   public event OnCommandSentHandler onBadKeySent;

   public void ReceiveFrameInput(string input)
   {
      foreach (char c in input)
      {
         if (c == '\b') // backspace
         {
            if (currentInputLine.Length > 0) // TODO > length of prompt
            {
               currentInputLine = currentInputLine.Remove(currentInputLine.Length - 1);
               break;  // ...solved the "greedy backspace key" issue.
            }
            // else 'beep' for backspace on "blank line" ??? How to beep?
            else if (currentInputLine.Length == 0) // TODO length of prompt
            {
               SendBadKey();
               SendCommand(currentInputLine);
               break;
            }
      }
      UpdateCurrentInputLine(c);
      }
   }

   public void ReceiveFauxInput(string input)
   {
      foreach (char c in input)
      {
         UpdateCurrentInputLine(c);
      }
   }

   public void SetPrompt(string input)
   {
      foreach (char c in input)
      {
         UpdateCurrentInputLine(c);
      }
   }

    public string GetCurrentInputLine() // TODO complete code or depreciate properly
    {
        return currentInputLine;
        // unless password
    }

    private void UpdateCurrentInputLine(char c)
    {
        if (c == '\n' || c == '\r')
        {
            SendCommand(currentInputLine); 
        }
        else
        {
            currentInputLine += c;
        }
    }

    private void SendCommand(string command)
    {
        onCommandSent(command);
        currentInputLine = "";
    }

   private void SendBadKey()
   {
      onBadKeySent("");
   }
}
