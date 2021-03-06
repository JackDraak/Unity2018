﻿/*  Hacker.cs
 *  Written for Complete Unity Developer 2.0 course in Unity 2017 (Section 2)
 *  Resources: 
 *      https://community.gamedev.tv/
 *      https://www.udemy.com/unitycourse2/
 *      WM2000 unitypackage (accessible via the Udemy course)
 *      https://fonts2u.com/dotrice-condensed.font/
 *  Author: @JackDraak
 *  2017-Oct-18
 */

using System.Collections;                    // Required for IEnumerator co-routines
using UnityEngine;                           // Unity requirement

public class Hacker : MonoBehaviour {
   // Game-Data:
   const int unlockFee_3 = 15;               // TOA cost to unlock level 3
   const int unlockFee_4 = 20;               // TOA cost to unlock level 4
   string[] wordsOne = 
      { "blue", "pink", "green", "yellow", "purple", "orange", "grey", "black", "white",
      "brown", "beige", "tan", "teal" };     // Level One words
   string[] wordsTwo = 
      { "duke", "coco", "tiger", "buddy", "bandit", "sunny", "shadow", "muffy", "lassie",
      "flipper", "rosco" };                  // Level Two words
   string[] wordsThree = 
      { "wells", "clarke", "gibson", "asimov", "bradbury", "heinlein", "stephenson",
      "sagan", "anthony" };                  // Level Three words
   string[] wordsFour = 
      { "maypole", "wellwood", "spadina", "townsend", "nostrand", "divisadero", "thames",
      "leaside", "marshall" };               // Level Four words

   // Need control of Keyboard game object (to disable user input as needed).
   // Use Unity inspector to drag/drop the keyboard in the scene into script field.
   [SerializeField] GameObject keyboard;     // We want to turn this off/on

   // Initial settings:
   enum Screen { Menu, Help, Guess, Pass, Fail, Egg, Login, Exit } // Game state enum
   Screen currentScreen;                           // Game state placeholder

   enum Access { Locked, Unlocked }                // Access state enum
   Access levelThree = Access.Locked;              // Access-state
   Access levelFour = Access.Locked;               // Access-state

   int currentLevel;                               // Effective difficulty level
   string scrambleWord;                            // Placeholder for scramble-word
   int tokens = 10;                                // Game currency

   string userLogin, userCredential;

   [SerializeField] AudioClip[] keyStrokeSounds;
   [SerializeField] AudioClip[] badKeySound;
   AudioSource audioSource;

   // Obligatory Unity 'Start()' function; 'OnUserInput()' is the primary game controller.
   void Start ()
   {
      audioSource = GetComponent<AudioSource>();
      Terminal.SetPromptLength();
      StartCoroutine(ShowLoad());   // begin the light show.
   }

   void OnUserInput(string input) // Primary controller. User input handled here.
   {
      // This function is explicity for "traffic control" with user input. The order
      // of these checks is very important to proper game-flow, use caution when
      // playing with this function. Most-significant checks go to the top. 

      if (input.ToLower() == "load \"*\",8,1") StartCoroutine(ShowEasterEgg());
      else if (currentScreen == Screen.Egg) HandleEggInput(input);
      else if (currentScreen == Screen.Exit) HandleExitInput(input);
      else if (input.ToLower() == "quit") ShowExit();
      else if (tokens <= 0) ShowFail();
      else if (input.ToLower() == "menu") ShowMenu();
      else if (input == "?") ShowHelp();
      else if (currentScreen == Screen.Menu) HandleMenuInput(input);
      else if (currentScreen == Screen.Guess) HandleGuessInput(input);
      else if (currentScreen == Screen.Pass) HandlePassInput(input);
      else if (currentScreen == Screen.Help) HandleHelpInput(input);
      else if (currentScreen == Screen.Fail) HandleFailInput(input);
   }

   private void BadUserInput(string parameters)
   {
      audioSource.clip = badKeySound[0];
      audioSource.volume = 0.2f;
      audioSource.Play();
   }

   void HandleEggInput(string input)
   {
      int g;
      if (input.ToLower() == "help") ShowEggHelp();
      else if (input.ToLower() == "exit") ShowMenu();
      else if (int.TryParse(input, out g)) // If input is a #, apply to tokens.
      {
         tokens += int.Parse(input);
         Terminal.WriteLine("ENTER COMMAND:");
         UpdatePrompt();
      }
      else ShowSyntaxError(input);
   }

   void HandleExitInput(string input) // Do nothing after exit (backdoor still avail.)
   {
      return;
   }

   void HandleFailInput(string input)
   {
      ShowFail();
      ShowSyntaxError(input);
   }

   void HandleGuessInput(string input)
   {
      if (input == "")
      {
         Terminal.WriteLine("Please provide a valid input:");
         return;
      }
      if (tokens < currentLevel)
      {
         Terminal.WriteLine("Insufficient tokens to guess in this category.");
         return;
      }
      Terminal.WriteLine("Guess Input: " + ReformatInput(input));
      if (input.ToLower() == scrambleWord)
      {
         tokens += (currentLevel * 2);
         UpdatePrompt();
         currentScreen = Screen.Pass;
         ShowReward(currentLevel);
         Terminal.WriteLine("Congratulations! You've earned " + (currentLevel * 2) + " TOA.");
         Terminal.WriteLine("(reminder, you may enter 'menu' or '?' at any time)");
      }
      else
      {
         tokens -= currentLevel;
         UpdatePrompt();
         //                 |<<<----  ----  -- MAXIMUM COULMN WIDTH --  ----  ---->>>|                                                                                             |
         Terminal.WriteLine("Yikes! You've lost " + currentLevel + " TOA!");
         Terminal.WriteLine("...be sure to not lose them all!");
         Terminal.WriteLine("(reminder, you may enter 'menu' or '?' at any time)");
      }
   }

   void HandleHelpInput(string input)
   {
      ShowHelp();
      ShowSyntaxError(input);
   }

   void HandleMenuInput(string input)
   {
      if (currentScreen == Screen.Fail) return;
      else if (input == "1") Level(1);
      else if (input == "2") Level(2);
      else if (input == "3")
      {
         // Access-control here: if locked, try to unlock (Level 3).
         if (levelThree == Access.Locked)
         {
            if (tokens == unlockFee_3)
            {
               ShowMenu();
               Terminal.WriteLine("Unable to deplete TOA to zero.");
            }
            else if (tokens > unlockFee_3)
            {
               tokens -= unlockFee_3;
               levelThree = Access.Unlocked;
               ShowMenu();
            }
            else
            {
               ShowMenu();
               Terminal.WriteLine("Insufficient tokens, " + unlockFee_3.ToString() + " required.");
            }
         }
         else Level(3); // if unlocked, proceed.
      }
      // Access-control here: if locked, try to unlock (Level 4).
      else if (input == "4")
      {
         if (levelFour == Access.Locked) 
         {
            if (tokens == unlockFee_4)
            {
               ShowMenu();
               Terminal.WriteLine("Unable to deplete TOA to zero.");
            }
            else if (tokens > unlockFee_4)
            {
               tokens -= unlockFee_4;
               levelFour = Access.Unlocked;
               ShowMenu();
            }
            else
            {
               ShowMenu();
               Terminal.WriteLine("Insufficient tokens, " + unlockFee_4.ToString() + " required.");
            }
         }
         else Level(4); // If unlocked, proceed.
      }
      else
      {
         ShowMenu();
         ShowSyntaxError(input);
      }
   }

   void HandlePassInput(string input)
   {
      Terminal.WriteLine("\nYou solved the active scramble. Directive unkown: " + ReformatInput(input));
      Terminal.WriteLine("\nPlease enter 'menu' at any time, or '?' for help.\n" +
                         "Otherwise use the menu, then enter a selection '#'.\n\n");
   }

   void Level(int level) // Display challenge or explain error (lack of TOA).
   {
      currentLevel = level;
      if (tokens < currentLevel)
      {
         ShowMenu();
         Terminal.WriteLine("You lack the tokens to make any guesses at level " + level.ToString() + ".");
         return;
      }
      currentScreen = Screen.Guess;
      Terminal.ClearScreen();
      Terminal.WriteLine("GTHDB Level " + currentLevel + " | TOA: " + tokens);
      Terminal.WriteLine("This group is worth " + currentLevel + " TOA for each guess.");
      Terminal.WriteLine("Unscramble the answer to the security question:");
      Terminal.WriteLine("");
      if (currentLevel == 1) Terminal.WriteLine("What is your favourite colour?");
      else if (currentLevel == 2) Terminal.WriteLine("What is the name of your first pet?");
      else if (currentLevel == 3) Terminal.WriteLine("Who is your favourite SciFi author?");
      else if (currentLevel == 4) Terminal.WriteLine("What is the name of the street you grew up on?");
      Terminal.WriteLine("Scramble Level " + level + ": " + SelectScramble(level));
   }

   private void PlayRandomSound()
   {
      System.Random randomRange = new System.Random();
      int index = randomRange.Next(keyStrokeSounds.Length);
      audioSource.clip = keyStrokeSounds[index];
      audioSource.Play();
   }

   string ReformatInput(string input) // Replace <space> with '_' for user-feedback.
   {
      string s = "";
      int l = input.Length;
      if (l > 0)
      {
         for (int i = 0; i < l; i++)
         {
            char c = input[i];
            if (c == ' ')
            {
               s = string.Concat(s, '_');
            }
            else s = string.Concat(s, input[i]);
         }
      }
      return s;
   }

   // Select (as member-variable 'scrambleWord') a random word from a specific level.
   string SelectScramble(int level)
   {
      System.Random randomRange = new System.Random();
      if (level == 1) scrambleWord = wordsOne[randomRange.Next(wordsOne.Length)];
      else if (level == 2) scrambleWord = wordsTwo[randomRange.Next(wordsTwo.Length)];
      else if (level == 3) scrambleWord = wordsThree[randomRange.Next(wordsThree.Length)];
      else if (level == 4) scrambleWord = wordsFour[randomRange.Next(wordsFour.Length)];
      return scrambleWord.Anagram(); // Give the word a scramble!  
   }

   IEnumerator SendFauxInput(string input)
   {
      foreach (char c in input)
      {
         Terminal.ReceiveFauxInput(c.ToString());
         PlayRandomSound();
         yield return new WaitForSeconds(Random.Range(0.066f, 0.33f));
      }
      Terminal.ReceiveFauxInput("\n");
   }

   private void SetPrompt(string prompt)
   {
      Terminal.SetPrompt(prompt);
   }

   IEnumerator ShowEasterEgg()
   {
      SetPrompt("");
      currentScreen = Screen.Egg;
      keyboard.SetActive(false);
      Terminal.ClearScreen();
      yield return new WaitForSeconds(.3f);
      Terminal.WriteLine("LOAD \"*\",8,1");
      yield return new WaitForSeconds(1.4f);
      Terminal.WriteLine("");
      yield return new WaitForSeconds(.7f);
      Terminal.WriteLine("SEARCHING FOR *");
      yield return new WaitForSeconds(.8f);
      Terminal.WriteLine("LOADING");
      Terminal.ShowCursor(false);
      yield return new WaitForSeconds(.6f);
      Terminal.WriteLine(".");
      yield return new WaitForSeconds(1.4f);
      Terminal.WriteLine(".");
      yield return new WaitForSeconds(1.4f);
      Terminal.WriteLine(".");
      yield return new WaitForSeconds(1.4f);
      Terminal.ClearScreen();
      //                 |<<<----  ----  -- MAXIMUM COULMN WIDTH --  ----  ---->>>|
      Terminal.WriteLine("        Distributed Social Hacking Tool v3.95f02");
      Terminal.WriteLine("          [enter 'help' or 'exit' at any time]");
      Terminal.WriteLine("");
      Terminal.WriteLine("");
      UpdatePrompt();
      Terminal.WriteLine("ENTER COMMAND:");
      Terminal.ShowCursor(true);
      keyboard.SetActive(true);
   }

   void ShowEggHelp()
   {
      Terminal.ClearScreen();
      Terminal.WriteLine("        Distributed Social Hacking Tool v3.95f02");
      Terminal.WriteLine("");
      Terminal.WriteLine("Did you forget what you put me for here, boss?");
      Terminal.WriteLine("Okay, okay, I'll give you a hint... Do you have enough");
      Terminal.WriteLine("TOA? If you forgot, you can always get back to the main");
      //                 |<<<----  ----  -- MAXIMUM COULMN WIDTH --  ----  ---->>>|
      Terminal.WriteLine("menu at any time by entering 'exit', otherwise, gimme a");
      Terminal.WriteLine("number, already!");
      Terminal.WriteLine("");
      Terminal.WriteLine("ENTER COMMAND:");
   }

   void ShowExit()
   {
      currentScreen = Screen.Exit;
      Terminal.ClearScreen();
      Application.Quit(); // Can't close a user browser, so explain:
      Terminal.WriteLine("Thank you for playing! You may now close your browser");
      Terminal.WriteLine("tab.");
   }

   void ShowFail()
   {
      Terminal.WriteLine("\nIt seems you have run out of TOA. If I were in\n" +
                         "your shoes, I'd be making myself well, scarce...\n" +
                         "now!\n\n");
   }

   void ShowHelp()
   {
      currentScreen = Screen.Help;
      Terminal.ClearScreen();
      Terminal.WriteLine("New user assistance:");
      Terminal.WriteLine("This terminal can be controlled by entering --");
      Terminal.WriteLine("");
      Terminal.WriteLine("   ?    - will display this user help.");
      Terminal.WriteLine("   menu - will display the Main Menu.");
      Terminal.WriteLine("   quit - will end the simulation.");
      Terminal.WriteLine("   {#}  - select from displayed menu options.");
      Terminal.WriteLine("");
      Terminal.WriteLine(" * While descrambling security question answers it costs");
      //                 |<<<----  ----  -- MAXIMUM COULMN WIDTH --  ----  ---->>>|
      Terminal.WriteLine("   specific TOA to make a guess, but if you are correct,");
      Terminal.WriteLine("   then you will double your TOA! Note: If you manage to");
      Terminal.WriteLine("   deplete your cache of TOA, you will be flagged for");
      Terminal.WriteLine("   de-resolution.");
      Terminal.WriteLine("\nPlease enter '?' at any time to review this help, or\n" +
                         "enter 'menu' then select a category by number, '#'.\n\n");
   }

   IEnumerator ShowLoad()
   {
      currentScreen = Screen.Login;
      keyboard.SetActive(false);    // disable user input during 'boot-up sequence'.
      Terminal.ShowCursor(false);   // ...and hide the cursor, because...
      Terminal.ClearScreen();
      //                 |<<<----  ----  -- MAXIMUM COULMN WIDTH --  ----  ---->>>|
      yield return new WaitForSeconds(.2f);
      Terminal.WriteLine("           **** COMMODORE 64 BASIC v4.20 ****");
      yield return new WaitForSeconds(1.8f);
      Terminal.WriteLine("");
      Terminal.WriteLine("         64K RAM SYSTEM  33710 BASIC BYTES FREE");
      yield return new WaitForSeconds(1.3f);
      Terminal.WriteLine("");
      Terminal.WriteLine("READY.");
      Terminal.ShowCursor(true);
      yield return new WaitForSeconds(1.4f);
      StartCoroutine(SendFauxInput("LOAD \"GTHDB.PRG\",8,1"));
      yield return new WaitForSeconds(5.0f); // needs to be long enough for SendFauxInput() coroutine
      Terminal.WriteLine("");
      Terminal.WriteLine("SEARCHING FOR GTHDB.PRG");
      yield return new WaitForSeconds(0.3f);
      Terminal.WriteLine("LOADING");
      Terminal.ShowCursor(false);
      yield return new WaitForSeconds(0.6f);
      Terminal.WriteLine(".");
      yield return new WaitForSeconds(0.8f);
      Terminal.WriteLine(".");
      yield return new WaitForSeconds(1.0f);
      Terminal.WriteLine(".");
      yield return new WaitForSeconds(1.2f);
      Terminal.WriteLine(".");
      yield return new WaitForSeconds(1.4f);
      StartCoroutine(ShowLogin());
   }

   IEnumerator ShowLogin()
   {
      currentScreen = Screen.Login;
      Terminal.ClearScreen();
      //                 |<<<----  ----  -- MAXIMUM COULMN WIDTH --  ----  ---->>>|
      Terminal.WriteLine("GTHDB release 13.2.7 (GrubbyPaws Update 3r)");
      Terminal.WriteLine("Kernel 2.6.5-21.EL compiled on VIC-20");
      yield return new WaitForSeconds(1.2f);
      Terminal.ShowCursor(true);
      Terminal.PrintFakePrompt("LOGIN (guest): ");
      yield return new WaitForSeconds(0.6f);
      Terminal.ReceiveFauxInput("\n");
      PlayRandomSound();
      yield return new WaitForSeconds(0.3f);
      Terminal.PrintFakePrompt("PASSWORD: ");
      yield return new WaitForSeconds(0.6f);
      Terminal.ReceiveFauxInput("\n");
      PlayRandomSound();
      yield return new WaitForSeconds(1.6f);
      keyboard.SetActive(true); // Reactivate the keyboard!
      ShowMenu(); // The Light-Show is over, start the game now.
   }

   void ShowMenu()
   {
      currentScreen = Screen.Menu;
      Terminal.ClearScreen();
      Terminal.WriteLine("                    GTHDB: Main Menu");
      Terminal.WriteLine("          [access at anytime by entering 'menu']");
      Terminal.WriteLine("");
      Terminal.WriteLine("Earn tokens of appreciation (TOA) as rewards for your");
      //                 |<<<----  ----  -- MAXIMUM COULMN WIDTH --  ----  ---->>>|
      Terminal.WriteLine("success. Please work dilligently however, as failures");
      Terminal.WriteLine("will not be accommodated.");
      Terminal.WriteLine("");
      Terminal.WriteLine("  1) What is your favourite colour?");
      Terminal.WriteLine("  2) What is the name of your first pet?");
      if (levelThree == Access.Locked)
      {
         Terminal.WriteLine("  3) Unlock with " + unlockFee_3 + " TOA.");
      }
      else Terminal.WriteLine("  3) Who is your favourite SciFi author?");
      if (levelFour == Access.Locked)
      {
         Terminal.WriteLine("  4) Unlock with " + unlockFee_4 + " TOA.");
      }
      else Terminal.WriteLine("  4) What is the name of the street you grew up on?");
      Terminal.WriteLine("");
      Terminal.WriteLine("Please enter '?' any time for help, otherwise, please");
      Terminal.WriteLine("select a security question to descramble their answers.");
      Terminal.WriteLine("");
      UpdatePrompt();
   }

   void ShowReward(int level)
   {
      switch (level)
      {
         case 1:
            Terminal.WriteLine("                        .----,");
            Terminal.WriteLine("                       /--._(");
            Terminal.WriteLine("                       |____|");
            Terminal.WriteLine("                       [____] .=======.");
            Terminal.WriteLine("                         YY   q.     .p");
            Terminal.WriteLine("                         ||   | `---' |");
            Terminal.WriteLine("                         []   |_______|");
            Terminal.WriteLine("");
            break;
         case 2:
            Terminal.WriteLine("                        |\\__/|");
            Terminal.WriteLine("                        (_^-^)");
            Terminal.WriteLine("                   _     )  (");
            Terminal.WriteLine("                  ((  __/    \\   ( ( (");
            Terminal.WriteLine("                   (   ) ||  ||   ) ) )");
            Terminal.WriteLine("                   '---''--''--'  >+++°>");
            Terminal.WriteLine("");
            break;
         case 3:
            Terminal.WriteLine("                            *");
            Terminal.WriteLine("                           /_\\");
            Terminal.WriteLine("                           | |");
            Terminal.WriteLine("                           |_|");
            Terminal.WriteLine("                           | |");
            Terminal.WriteLine("                           )_(");
            Terminal.WriteLine("                          /| |\\");
            Terminal.WriteLine("                         /_|_|_\\");
            Terminal.WriteLine("");
            break;
         case 4:
            Terminal.WriteLine("                        ,dP\"\"d8b,");
            Terminal.WriteLine("                       d\"   d88\"8b");
            Terminal.WriteLine("                      I8    Y88a88)");
            Terminal.WriteLine("                      `Y, a  )888P");
            Terminal.WriteLine("                        \"b,,a88P\"");
            Terminal.WriteLine("");
            Terminal.WriteLine("                  You are at peak Zen!");
            Terminal.WriteLine("        Now would be a good time for a vacation!");
            //                 |<<<----  ----  -- MAXIMUM COULMN WIDTH --  ----  ---->>>|
            Terminal.WriteLine("");
            break;
         default:
            Debug.Log("WARNING: fell to default case in ShowReward().");
            break;
      }
   }

   void ShowSyntaxError(string input)
   {
      Terminal.WriteLine("Syntax Error: " + ReformatInput(input));
   }

   private void UpdatePrompt()
   {
      SetPrompt("[Earnings: " + tokens + " TOA] ");
   }
}
