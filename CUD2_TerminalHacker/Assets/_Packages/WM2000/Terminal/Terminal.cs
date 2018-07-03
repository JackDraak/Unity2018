using UnityEngine;
using System.Reflection;

public class Terminal : MonoBehaviour
{
    DisplayBuffer displayBuffer;
    InputBuffer inputBuffer;

    static Terminal primaryTerminal;

   private void Awake()
   {
      if (primaryTerminal == null) { primaryTerminal = this; } // Be the one
      inputBuffer = new InputBuffer();
      displayBuffer = new DisplayBuffer(inputBuffer);
      inputBuffer.onCommandSent += NotifyCommandHandlers;
      inputBuffer.onBadKeySent += NotifyBadKeyHandlers;
   }

   public string GetDisplayBuffer(int width, int height)
   {
      return displayBuffer.GetDisplayBuffer(Time.time, width, height);
   }

   public static void ShowCursor(bool condition)
   {
      primaryTerminal.displayBuffer.ShowCursor(condition);
   }

   public static void ReceiveFauxInput(string input)
   {
      primaryTerminal.inputBuffer.ReceiveFauxInput(input);
   }

   public static void SetPrompt(string input)
   {
      primaryTerminal.inputBuffer.SetPrompt(input);
   }

   public static void ReceiveFauxEndOfLine() // not really needed....
   {
      primaryTerminal.inputBuffer.ReceiveFauxInput("\n");
   }

   public void ReceiveFrameInput(string input)
   {
      inputBuffer.ReceiveFrameInput(input);
   }

    public static void ClearScreen()
    {
       primaryTerminal.displayBuffer.Clear();
    }

   public static void WriteChar(char c) // TODO finish or depreciate this code
   {
      // make a way to print single character to buffer....
   }

   public static void WriteLine(string line)
   {
      primaryTerminal.displayBuffer.WriteLine(line);
   }

   public void NotifyCommandHandlers(string input)
   {
      var allGameObjects = FindObjectsOfType<MonoBehaviour>();
      foreach (MonoBehaviour mb in allGameObjects)
      {
         var flags = BindingFlags.NonPublic | BindingFlags.Instance; 
         var targetMethod = mb.GetType().GetMethod("OnUserInput", flags);
         if (targetMethod != null)
         {
            object[] parameters = new object[1];
            parameters[0] = input;
            targetMethod.Invoke(mb, parameters);
         }
      }
   }

   public void NotifyBadKeyHandlers(string input)
   {
      var allGameObjects = FindObjectsOfType<MonoBehaviour>();
      foreach (MonoBehaviour mb in allGameObjects)
      {
         var flags = BindingFlags.NonPublic | BindingFlags.Instance;
         // hacking in a way to get beep-directives from inputBuffer
         var targetMethod = mb.GetType().GetMethod("BadUserInput", flags);
         if (targetMethod != null)
         {
            object[] parameters = new object[1];
            parameters[0] = input;
            targetMethod.Invoke(mb, parameters);
         }
      }
   }
}