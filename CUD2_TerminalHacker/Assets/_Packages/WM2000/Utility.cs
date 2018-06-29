public static class StringExtension
{
    public static string Anagram(this string str) // Note use of this
    {
        string attempt = Shuffle(str);
        while (attempt == str)
        {
            attempt = Shuffle(str);
        }
        return attempt;
    }

    // Based on something we got from the web, not re-written for clarity
    private static string Shuffle(string str)
    {
        char[] characters = str.ToCharArray();
        System.Random randomRange = new System.Random();
        int numberOfCharacters = characters.Length;
        while (numberOfCharacters > 1)
        {
            numberOfCharacters--;                                 // decrement 'end position', repeat through all chars in string:
            int index = randomRange.Next(numberOfCharacters + 1); // select any char in string before the 'end'
            var value = characters[index];                        // store that char (temporary)
            characters[index] = characters[numberOfCharacters];   // store the 'end char' in-place of the char pulled for shuffle
            characters[numberOfCharacters] = value;               // drop selected char into the 'end position'
      }
        return new string(characters);
    }
}