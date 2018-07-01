public class InputBuffer
{
    string currentInputLine; // todo private

    public delegate void OnCommandSentHandler(string command);
    public event OnCommandSentHandler onCommandSent;

    public void ReceiveFrameInput(string input)
    {
        foreach (char c in input)
        {
            if (c == '\b') // backspace
            {
               // solve the "greedy backspace key" issue:
               if (currentInputLine.Length > 0)
               {
                  currentInputLine = currentInputLine.Remove(currentInputLine.Length - 1);
                  break;  
               }
               // else 'beep' for backspace on blank line ???
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

    public string GetCurrentInputLine()
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
}
