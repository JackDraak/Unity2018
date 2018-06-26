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

    public string GetCurrentInputLine()
    {
        return currentInputLine;
        // unless password
    }

    private void UpdateCurrentInputLine(char c)
    {
        // depreciated code block.... functionality moved into RecieveFrameInput to prevent excessive backspace-deletion.
        // (now, maxes at 1 deleted character per frame, rather than N).
        //if (c == '\b')
        //{
            //Debug.Log("ucil-backspace ");
            //DeleteCharacters();
        //}
        if (c == '\n' || c == '\r')
        {
            SendCommand(currentInputLine); 
        }
        else
        {
            currentInputLine += c;
        }
    }

 // depreciated
 //   private void DeleteCharacters()
 //   {
 //       if (currentInputLine.Length > 0)
 //       {
 //           currentInputLine = currentInputLine.Remove(currentInputLine.Length - 1);
 //       }
 //       else
 //       {
 //           // do nothing on delete at start of line
 //       }
 //   }

    private void SendCommand(string command)
    {
        onCommandSent(command);
        currentInputLine = "";
    } 
}
