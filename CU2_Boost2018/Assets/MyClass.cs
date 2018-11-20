///
///   This code is to help a peer student understand local and global variables;
///   it also *hints at 'public' variables.
///   
///   Note how the variable ambiguousString is, well, ambiguous. Best-practices 
///   advise against re-using a global variable name in a local context, but 
///   the compiler probably won't stop you (this depends on compiler configuration).
///   
///   It will, however, absolutely complain about trying to use a variable that is 
///   "out of context" (refer to line 40, when un-commented).
///   

using UnityEngine;

public class MyClass : MonoBehaviour
{
   // any variables declared here are 'global', ie.e. visible to the entire class.
   // if they are private, they are only visible within the class (default).
   // if they are public, they are visible outside of the class.
   string ambiguousString = "Default global ambiguous string"; // Visible to entire class... or is it?
   string privateGlobalString = "Default private global string value"; // Visible to entire class, i.e. 'global'.
   public string publicGlobalString = "Default public global string value"; // *Visible to entire program.

   void AmbiguousVariableFunction()
   {
      string hiddenString = "Default string variable only 'visible' within the MyFunction() method, i.e. 'local'";

      Debug.Log(ambiguousString); // This outputs "Default global ambiguous string" to the console.
      Debug.Log(hiddenString); // Output as expected.
      Debug.Log(privateGlobalString); // Output as expected.
      Debug.Log(publicGlobalString); // Output as expected.
   }

   float Speed(float distance, float time)
   {
      if (time > 0) return distance / time;
      else return -1;
   }

   float FeetAsMiles(float feet) { return feet / 5280; }

   float SecondsAsHours(float seconds) { return seconds / 3600; }

   void CalculateSpeeds()
   {
      float someDistance = 45.0f; // Shall we say, feet?
      float someTime = 37.6f; // Shall we say, seconds?
      float someSpeed = Speed(someDistance, someTime); // The result would be in feet per second.
      Debug.Log(someSpeed + " feet/second");

      float speedAsMPH = Speed(FeetAsMiles(someDistance), SecondsAsHours(someTime)); // Convert to MPH.
      Debug.Log(speedAsMPH + " MPH");
   }

   int Factorial(int n)
   {
      if (n <= 1) return 1;
      return n * Factorial(n - 1);
   }

   void DoFactorials()
   {
      for (int i = 3; i < 32; i = i + 2)
      {
         Debug.Log("Factorial("+i+") = "+ Factorial(i));
      }
   }

   string randomLossString()
   {
      string[] lossStrings = { "string one. ", "string two. ", "string three. " };
      return lossStrings[Random.Range(0,lossStrings.Length)];
   }

   void Start()
   {
      string ambiguousString = "Replacement local ambiguous string"; // local to the Start() method.
      AmbiguousVariableFunction(); // This will output the 4 'default' strings to the console.
      Debug.Log(ambiguousString); // This outputs the local ambiguousString value, "Replacement local ambiguous string".
      ///Debug.Log(hiddenString); // This generates an error, "The name hiddenString does not exist in this context".

      CalculateSpeeds(); // Demonstration of functions as parameters to functions.

      DoFactorials(); // Demonstraion of a recursive function (a function that calls itself).

      Debug.Log(randomLossString() + randomLossString() + randomLossString() + randomLossString() + randomLossString());
   }

}