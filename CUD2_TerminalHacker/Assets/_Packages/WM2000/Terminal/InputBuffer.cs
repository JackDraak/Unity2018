using UnityEngine;

public class InputBuffer
{
   string currentInputLine;
   string localPrompt = "";
   int promptLength = 0;
   int logLine = 0;

   public delegate void OnCommandSentHandler(string command);
   public event OnCommandSentHandler onCommandSent;
   public event OnCommandSentHandler onBadKeySent;

   public string GetCurrentInputLine() // TODO complete code or depreciate properly
   {
      return currentInputLine;
      // unless password
   }

   public void PrintPrompt()
   {
      foreach (char c in localPrompt)
      {
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

   public void ReceiveFrameInput(string input)
   {
      if (input.Length > 0)
      {
         logLine++;
         Debug.Log("InputBuffer:ReceiveFrameInput_" + logLine.ToString() + "_" + input + " PL:" + promptLength.ToString());
      }
      foreach (char c in input)
      {
         if (c == '\b') // <-- backspace
         {
            if (currentInputLine.Length > (0 + promptLength))
            {
               currentInputLine = currentInputLine.Remove(currentInputLine.Length - 1);
               break;  // ...solved the "greedy backspace key" issue.
            }
            // 'beep' for backspace on "blank line"
            else if (currentInputLine.Length == (0 + promptLength))
            {
               SendBadKey();
               currentInputLine = "";
               if (promptLength > 0) PrintLocalPrompt();
               break;
            }
         }
         UpdateCurrentInputLine(c);
      }
   }

   private void SendBadKey()
   {
      onBadKeySent("");
   }

   private void SendCommand(string command)
   {
      if (promptLength > 0) onCommandSent(command.Substring(promptLength));
      else
      {
         onCommandSent(command);
      }
      currentInputLine = "";
      if (promptLength > 0) PrintLocalPrompt();
   }

   public void PrintFakePrompt(string input)
   {
      foreach (char c in input)
      {
         UpdateCurrentInputLine(c);
      }
   }

   public void PrintLocalPrompt()
   {
      foreach (char c in localPrompt) 
      {
         UpdateCurrentInputLine(c);
      }
   }

   public void SetPrompt(string input)
   {
      localPrompt = input;
      SetPromptLength(true);
      currentInputLine = localPrompt + currentInputLine;
   }

   public void SetPromptLength()
   {
      promptLength = 0;
   }

   public void SetPromptLength(int length)
   {
      promptLength = length;
   }

   public void SetPromptLength(bool torf)
   {
      promptLength = localPrompt.Length;
   }

   private void UpdateCurrentInputLine(char c)
   {
      if (c == '\n' || c == '\r')
      {
         SendCommand(currentInputLine);
      //   if (promptLength > 0) PrintLocalPrompt();
      }
      else currentInputLine += c;
   }
}
