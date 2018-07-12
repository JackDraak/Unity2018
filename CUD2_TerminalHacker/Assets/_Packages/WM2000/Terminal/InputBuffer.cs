using UnityEngine;

public class InputBuffer
{
   string currentInputLine;
   string localPrompt = "";
   int promptLength = 0;

   public delegate void OnCommandSentHandler(string command);

   public event OnCommandSentHandler onCommandSent;
   public event OnCommandSentHandler onBadKeySent;

   public string GetCurrentInputLine() // TODO complete code or depreciate properly
   {
      return currentInputLine;
      // unless password
   }

   public void PrintFakePrompt(string input)
   {
      foreach (char c in input)
      {
         UpdateCurrentInputLine(c);
      }
   }

 /*  public void PrintLocalPrompt()
   {
      foreach (char c in localPrompt)
      {
         UpdateCurrentInputLine(c);
      }
   } */

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
      foreach (char c in input)
      {
         if (c == '\b')
         {
            if (currentInputLine.Length > (0 + promptLength))
            {
               currentInputLine = currentInputLine.Remove(currentInputLine.Length - 1);
               // fix the 'greedy backspace key' issue with this break:
               break;
            }
            // 'beep' for backspace keypresses on "blank lines" (don't backspace over any prompt, either).
            else if (currentInputLine.Length == (0 + promptLength))
            {
               SendBadKey();
               currentInputLine = "";
               if (promptLength > 0) PrintPrompt();
               //if (promptLength > 0) PrintLocalPrompt();
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
      if (promptLength > 0) PrintPrompt();
      //if (promptLength > 0) PrintLocalPrompt();
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

   public void SetPromptLength(bool torf)
   {
      promptLength = localPrompt.Length;
   }

   public void SetPromptLength(int length)
   {
      promptLength = length;
   }

   private void UpdateCurrentInputLine(char c)
   {
      if (c == '\n' || c == '\r')
      {
         SendCommand(currentInputLine);
      }
      else currentInputLine += c;
   }
}
