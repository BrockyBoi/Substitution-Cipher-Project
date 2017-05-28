using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class EncryptionController : MonoBehaviour {
    /// <summary>
    /// Dictionary of all words allowing easy access to see if word exists
    /// </summary>
    Dictionary<string, string> existingWords;
    /// <summary>
    /// All words in base file
    /// </summary>
    string[] wordArray;
    /// <summary>
    /// Array of integers of each spot in wordArray where the next length of words begins
    /// </summary>
    int[] wordLengthStarts;
    /// <summary>
    /// Dictionary of chars to see if char is already solved and able to swap in for mapping
    /// </summary>
    Dictionary<char, char> solvedCharValues = new Dictionary<char, char>();
    /// <summary>
    /// This dictionary will simply be the inverse of solvedCharValues, to allow me to easily encrypt a message using the same pattern I solve for
    /// </summary>
    Dictionary<char, char> encryptionValues = new Dictionary<char, char>();

    /// <summary>
    /// Only possible one letter words
    /// </summary>
    readonly char[] oneLetterWords = { 'a', 'i', 'o' };
    /// <summary>
    /// Most common two letter words
    /// </summary>
    readonly string[] commonTwoLetterWords = { "of", "to", "in", "it", "is", "be", "as", "at", "so", "we", "my" };
    /// <summary>
    /// Most common three letter words
    /// </summary>
    readonly string[] commonThreeLetterWords = { "the", "and", "for", "was", "are", "but", "not", "you", "all", "his" };
    /// <summary>
    /// Most common four letter words
    /// </summary>
    readonly string[] commonFourLetterWord = { "that", "with", "have", "this", "will", "your", "from", "they", "know", "want" };

    void Start()
    {
        string dictionaryPath = Application.dataPath + "/plain.txt";
        string encryptedPath = Application.dataPath + "/encrypted.txt";
        InitializeDictionary();

        wordArray = GetWordArray(dictionaryPath, out existingWords);
        int longestWord = wordArray[wordArray.Length - 1].Length;
        wordLengthStarts = new int[longestWord];
        wordLengthStarts = FillInLengthArray(longestWord);

        DecryptFile(encryptedPath, Application.dataPath + "/DecryptedFile.txt");
        EncryptMessage("Hi there, my name is Brock Soicher and this message will look super weird", Application.dataPath + "/MyEncryption.txt");
        //DecryptFileBasic(encryptedPath, Application.dataPath + "/DecryptedBasic.txt");
    }

    /// <summary>
    /// Initializes dictionary for later usage (Dictionary in this case being a hash table)
    /// </summary>
    void InitializeDictionary()
    {
        for (char c = 'a'; c <= 'z'; c++)
        {
            solvedCharValues.Add(c, '0');
            encryptionValues.Add(c, '0');
        }
    }
    /// <summary>
    /// Fill in solvedCharValues with cipher results that I had gotten by hand
    /// </summary>
    void FillDictionaryWithHandValues()
    {
        solvedCharValues['z'] = 'a';
        solvedCharValues['h'] = 'b';
        solvedCharValues['m'] = 'd';
        solvedCharValues['n'] = 'e';
        solvedCharValues['e'] = 'f';
        solvedCharValues['x'] = 'h';
        solvedCharValues['w'] = 'i';
        solvedCharValues['u'] = 'k';
        solvedCharValues['t'] = 'l';
        solvedCharValues['s'] = 'm';
        solvedCharValues['r'] = 'n';
        solvedCharValues['q'] = 'o';
        solvedCharValues['p'] = 'p';
        solvedCharValues['l'] = 'r';
        solvedCharValues['k'] = 's';
        solvedCharValues['j'] = 't';
        solvedCharValues['d'] = 'w';
        solvedCharValues['b'] = 'y';
        solvedCharValues['y'] = 'g';
        solvedCharValues['v'] = 'j';
        solvedCharValues['g'] = 'u';
        solvedCharValues['i'] = 'c';
        solvedCharValues['f'] = 'v';
        solvedCharValues['a'] = 'z';
    }

    /// <summary>
    /// Returns an array of strings to be our dictionary
    /// </summary>
    /// <param name="filePath">The string path to the dictionary</param>
    /// <returns></returns>
    string[] GetWordArray(string filePath,out Dictionary<string,string> possibleWords)
    {
        StreamReader input = new StreamReader(filePath);
        string[] words;

        Dictionary<string, string> uniqueWords = new Dictionary<string, string>();
        int spot = 0;
        char c;

        while(!input.EndOfStream)
        {
            string tempWord = "";
            do
            {
                spot = input.Read();
                c = char.ToLower((char)spot);
                if(c >= 'a' && c <= 'z')
                tempWord += c;
            } while (c >= 'a' && c <= 'z');

            if(tempWord != "" && !uniqueWords.ContainsKey(tempWord))
            {
                if(tempWord.Length == 1)
                {
                    //Need this case because apostrophes tend to split up words into one letters, but only these 3 words should be valid
                    if (tempWord == "a" || tempWord == "i" || tempWord == "o")
                    {
                        uniqueWords.Add(tempWord, tempWord);
                    }
                }
                else
                 uniqueWords.Add(tempWord, tempWord);
            }
        }
        input.Close();

        words = new string[uniqueWords.Count];
        int count = 0;
        foreach(KeyValuePair<string,string> entry in uniqueWords)
        {
            words[count] = entry.Value;
            count++;
        }

        //Words are sorted by length
        Array.Sort(words, (x, y) => x.Length.CompareTo(y.Length));
        possibleWords = uniqueWords;
        return words;
    }

    /// <summary>
    /// Adds letter to solvedCharValue dictionary that can be used later to replace encrypted letters
    /// </summary>
    /// <param name="encryptedLetter">Letter that is encrypted</param>
    /// <param name="decryptedLetter">Letter that is solution for encrypted letter</param>
    void MapLetter(char encryptedLetter, char decryptedLetter)
    {
        if (solvedCharValues.ContainsValue(decryptedLetter))
        {
            Debug.LogError("Dictionary already contains mapping for " + decryptedLetter);
            return;
        }
        Debug.Log(encryptedLetter + " is mapped to " + decryptedLetter);
        solvedCharValues[encryptedLetter] = decryptedLetter;
        encryptionValues[decryptedLetter] = encryptedLetter;
    }

    /// <summary>
    /// Retruns true if mapping of character already exists
    /// </summary>
    /// <param name="c">Character you want to check</param>
    /// <returns></returns>
    bool IsMapped(char c)
    {
        if( c < 'a' || c > 'z')
        {
            //Just in case
            Debug.LogError("Trying to find map for value that is not a character");
            return false;
        }
        return solvedCharValues[c] != '0';
        
    }

    /// <summary>
    /// Returns true if the character has already been mapped to the dictionary
    /// </summary>
    /// <param name="c">Char to check</param>
    /// <returns></returns>
    bool ValueSolvedFor(char c)
    {
        if (c < 'a' || c > 'z')
        {
            //Just in case
            Debug.LogError("Trying to find map for value that is not a character");
            return false;
        }
        return solvedCharValues.ContainsValue(c);
    }

    /// <summary>
    /// Returns true if no values are 0, and therefore all values have been solved for
    /// </summary>
    /// <returns></returns>
    bool AllValuesSolved()
    {
        return !solvedCharValues.ContainsValue('0');
    }

    /// <summary>
    /// Function used to decrypt file using the cipher I solved by hand
    /// </summary>
    /// <param name="filePath">String name for file to decrypt</param>
    /// <param name="newFile">String name for new file to store decrypted text</param>
    void DecryptFileBasic(string filePath, string newFile)
    {
        StreamReader read = new StreamReader(filePath);
        StreamWriter write = new StreamWriter(newFile);
        FillDictionaryWithHandValues();
        while(!read.EndOfStream)
        {
            int spot = read.Read();
            char c = char.ToLower((char)spot);
            if(c >= 'a' && c <= 'z')
            {
                if (IsMapped(c))
                {
                    write.Write(solvedCharValues[c]);
                }
                else write.Write(c);
            }
            else
            {
                write.Write(c);
            }
        }
        write.Close();
    }
    /// <summary>
    /// Function to read in encrypted file and call other functions to figure out the mappings
    /// </summary>
    /// <param name="filePath">String name for file to decrypt</param>
    /// <param name="newFile">String name for new file to store decrypted text</param>
    void DecryptFile(string filePath, string newFile)
    {
        StreamReader read = new StreamReader(filePath);

        int count = 0;
        //Only go through first 2500 words
        while (count < 500)
        {
            int input;
            char c;
            string currentWord = "";
            do
            {
                input = read.Read();
                c = char.ToLower((char)input);
                if (c >= 'a' && c <= 'z')
                    currentWord += c;

            } while (c >= 'a' && c <= 'z');
            Debug.Log("Word to decrypt: " + currentWord + " Decrypted to: "+ DecryptWord(currentWord, filePath));

            count++;
            if (AllValuesSolved())
                break;
        }
        read.Close();
        WriteDecryption(filePath, newFile);
    }

    /// <summary>
    /// Function that actually writes the words to the new file
    /// </summary>
    /// <param name="filePath">Path of file to read in encrypted data</param>
    /// <param name="newFile">Path of file to write the decrypted data to</param>
    void WriteDecryption(string filePath, string newFile)
    {
        StreamReader read = new StreamReader(filePath);
        StreamWriter write = new StreamWriter(newFile);

        while (!read.EndOfStream)
        {
            int spot = read.Read();
            char c = char.ToLower((char)spot);
            if (c >= 'a' && c <= 'z')
            {
                if (IsMapped(c))
                {
                    write.Write(solvedCharValues[c]);
                }
                else write.Write(c);
            }
            else
            {
                write.Write(c);
            }
        }
        write.Close();
    }

    /// <summary>
    /// Checks word for each letter to see if word is already mapped, and if not begin to break word down to begin finding char values to test on
    /// </summary>
    /// <param name="word">Word desired to be tested</param>
    /// <param name="filePath">File name to begin tests on</param>
    string DecryptWord(string word, string filePath)
    {
        int length = word.Length;
        bool mapped = true;
        string tempString = word;
        List<char> letters = new List<char>();
        foreach(char c in word)
        {
            if (IsMapped(c))
            {
                continue;
            }
            else
            {
                mapped = false;
                letters.Add(c);
            }
        }
        if(!mapped)
        {
            switch (length)
            {
                
                case 0:
                    return "";
                #region Case1
                case 1:
                    {
                        for (int i = 0; i < oneLetterWords.Length; i++)
                        {
                            Dictionary<char, char> tempDict = new Dictionary<char, char>();
                            char letter = letters[0];
                            char testLetter = oneLetterWords[i];
                            tempDict.Add(letter, testLetter);
                            if (TestLetters(2, tempDict, filePath))
                            {
                                MapLetter(letter, testLetter);
                                break;
                            }
                        }
                    }
                    break;
                #endregion
                #region Case2
                case 2:
                    {
                        int iteration = 0;
                        while (true)
                        {
                            Dictionary<char, char> tempDict = new Dictionary<char, char>();
                            string testWord;
                            if (iteration >= commonTwoLetterWords.Length && iteration - commonTwoLetterWords.Length < wordLengthStarts[2])
                            {
                                testWord = wordArray[wordLengthStarts[1] + iteration - commonTwoLetterWords.Length];
                            }
                            else if (iteration < commonTwoLetterWords.Length)
                                testWord = commonTwoLetterWords[iteration];
                            else return word;

                            int count = 0;
                            foreach (char ch in word)
                            {
                                if(!tempDict.ContainsKey(ch) && !IsMapped(ch))
                                    tempDict.Add(ch, testWord[count]);
                                count++;
                            }
                            if (TestLetters(2, tempDict, filePath))
                            {
                                foreach (KeyValuePair<char, char> entry in tempDict)
                                {
                                    MapLetter(entry.Key, entry.Value);
                                }
                                break;
                            }
                            else iteration++;
                        }
                        break;
                    }
                #endregion
                #region Case3
                case 3:
                    {
                        int iteration = 0;
                        while (true)
                        {
                            Dictionary<char, char> tempDict = new Dictionary<char, char>();
                            string testWord;
                            if (iteration >= commonThreeLetterWords.Length && iteration - commonThreeLetterWords.Length < wordLengthStarts[3])
                            {
                                testWord = wordArray[wordLengthStarts[2] + iteration - commonThreeLetterWords.Length];
                            }
                            else if (iteration < commonThreeLetterWords.Length)
                                testWord = commonThreeLetterWords[iteration];
                            else return word;

                            int count = 0;
                            foreach (char ch in word)
                            {
                                if (!tempDict.ContainsKey(ch) && !IsMapped(ch))
                                    tempDict.Add(ch, testWord[count]);
                                count++;
                            }

                            if (TestLetters(3, tempDict, filePath))
                            {
                                foreach (KeyValuePair<char, char> entry in tempDict)
                                {
                                    MapLetter(entry.Key, entry.Value);
                                }
                                break;
                            }
                            else iteration++;
                        }
                        break;
                    }
                #endregion
                #region Case4
                case 4:
                    {
                        int iteration = 0;
                        while (true)
                        {
                            Dictionary<char, char> tempDict = new Dictionary<char, char>();
                            string testWord;
                            if (iteration >= commonFourLetterWord.Length && iteration - commonFourLetterWord.Length < wordLengthStarts[4])
                            {
                                testWord = wordArray[wordLengthStarts[3] + iteration - commonFourLetterWord.Length];
                            }
                            else if (iteration < commonFourLetterWord.Length)
                                testWord = commonFourLetterWord[iteration];
                            else return word;

                            int count = 0;
                            foreach (char ch in word)
                            {
                                if (!tempDict.ContainsKey(ch) && !IsMapped(ch))
                                    tempDict.Add(ch, testWord[count]);
                                count++;
                            }
                            if (TestLetters(4, tempDict, filePath))
                            {
                                Debug.Log("Case 4 solved");
                                foreach (KeyValuePair<char, char> entry in tempDict)
                                {
                                    MapLetter(entry.Key, entry.Value);
                                }
                                break;
                            }
                            else iteration++;
                        }
                        break;
                    }
                #endregion
                #region Default
                default:
                    {
                        int iteration = 0;
                        while (true)
                        {
                            Dictionary<char, char> tempDict = new Dictionary<char, char>();
                            string testWord;
                            if (wordLengthStarts[length - 1] + iteration < wordLengthStarts[length])
                            {
                                testWord = wordArray[wordLengthStarts[length - 1] + iteration];
                            }
                            else return word;
                            //Debug.Log("TestWord: " + testWord);
                            int count = 0;
                            foreach (char ch in word)
                            {
                                if (!tempDict.ContainsKey(ch) && !IsMapped(ch))
                                {
                                    tempDict.Add(ch, testWord[count]);
                                }
                                count++;
                            }

                            if (TestLetters(length, tempDict, filePath))
                            {
                                foreach (KeyValuePair<char, char> entry in tempDict)
                                {
                                    MapLetter(entry.Key, entry.Value);
                                }                        
                                break;
                            }
                            else iteration++;
                        }
                    }
                    break;
                    #endregion
            }
        }
        string resultWord = "";
        foreach(char ch in word)
        {
            if (IsMapped(ch))
                resultWord += solvedCharValues[ch];
            else resultWord += ch;
        }
        
       // Debug.Log("Returned string of length " + length + " : " + resultWord);
        return resultWord;
    }

    /// <summary>
    /// Test guessed valeus across the decrypted file to see if changing the char values would make sense
    /// </summary>
    /// <param name="wordLength">Length of word that this file will only test on</param>
    /// <param name="check">Dictionary of values to test</param>
    /// <param name="filePath">File string for file to test on</param>
    /// <returns></returns>
    bool TestLetters(int wordLength, Dictionary<char,char> check, string filePath)
    {
        StreamReader read = new StreamReader(filePath);

        for (int i = 0; i < 1500; i++)
        {
            //Gets a word
            int spot = read.Read();
            char c = char.ToLower((char)spot);
            string word = "";
            while (c >= 'a' && c <= 'z')
            {
                word += c;
                spot = read.Read();
                c = char.ToLower((char)spot);
            }
            //If word is not blank
            if (word != "" && word.Length == wordLength)
            {
               // Debug.Log("Word to test on: " + word);
                string tempWord = "";
                int switchCount = 0;
                List<int> spotsChanged = new List<int>();
                int count = 0;

                int charCount = 0;
                
                //Make sure word has all the letters we want to test
                foreach(char ch in word)
                {
                    if (check.ContainsKey(ch))
                        charCount++;
                }
                //If this word doesn't have all the letters we're testing then there's no point in trying it
                if (charCount != check.Count)
                    continue;
                //For each character in the current word check to see if it has a letter that we're testing, and if so then add the new change to the temporary string
                foreach (char ch in word)
                {
                    if (check.ContainsKey(ch))
                    {
                        tempWord += check[ch];
                        switchCount++;
                        spotsChanged.Add(count);
                    }
                    else if(IsMapped(ch))
                    {
                        tempWord += solvedCharValues[ch];
                        switchCount++;
                        spotsChanged.Add(count);
                    }
                    else tempWord += ch;

                    count++;
                }
                //If every letter in the word ended up being changed, simply check to see if that word exists at all
                if (switchCount == wordLength)
                {
                    if (existingWords.ContainsKey(tempWord))
                    {
                        foreach(KeyValuePair<char,char> entry in check)
                        {
                            if (ValueSolvedFor(entry.Value))
                            {
                                read.Close();
                                return false;
                            }
                        }
                        read.Close();
                        return true;
                    }
                    read.Close();
                    return false;
                }
                //If only specific letters got changed and not the whole word then do this
                else
                {
                    //Go through every word in the word array of the correct size
                    for (int j = wordLengthStarts[tempWord.Length - 1]; j < wordLengthStarts[tempWord.Length]; j++)
                    {
                        string currentWord = wordArray[j];
                        bool possible = false;

                        //Does the word we're testing have the character at all?  If not then continue to next one
                        bool hasString = false;
                        foreach(KeyValuePair<char,char> entry in check)
                        {
                            if(currentWord.Contains(entry.Key.ToString()))
                            {
                                hasString = true;
                                break;
                            }
                        }

                        if (!hasString)
                            continue;

                        foreach (int changedSpot in spotsChanged)
                        {
                            Debug.Log("Does " + currentWord[changedSpot] + " == " + tempWord[changedSpot] +  " ? " + (currentWord[changedSpot] == tempWord[changedSpot]));
                            if (currentWord[changedSpot] != tempWord[changedSpot])
                            {
                                //If there are spots that are different in lettering in the words then we know the word must not exist
                                possible = false;
                                break;
                            }
                            possible = true;
                        }
                        if (possible)
                            return true;
                        else continue;
                    }
                }
            }
        }
        read.Close();
        return true;
    }

    /// <summary>
    /// Go through sorted word array and fill in values to be able to easily jump to desired length of words in array
    /// </summary>
    /// <param name="longestWord">The length of the longest word possible, so you know when to stop counting</param>
    /// <returns></returns>
    int[] FillInLengthArray(int longestWord)
    {
        int[] arr = new int[longestWord];
        int max = longestWord;
        int currentLength = 0;

        for(int i = 0; i < wordArray.Length; i++)
        {
            int length = wordArray[i].Length;
            if(length > currentLength)
            {
                currentLength = length;
                arr[currentLength - 1] = i;

                if (currentLength == max)
                    break;
            }
        }
        return arr;
    }

    /// <summary>
    /// Function to encrypt a message using the same key values solved for in DecryptFile
    /// </summary>
    /// <param name="message">The message you wish to be encrypted</param>
    /// <param name="filePath">The file path you wish the encryption to be written</param>
    void EncryptMessage(string message, string filePath)
    {
        StreamWriter write = new StreamWriter(filePath);

        foreach(char c in message)
        {
            char ch = char.ToLower(c);
            Debug.Log(encryptionValues.ContainsKey(ch));
            if (encryptionValues.ContainsKey(ch))
            {
                write.Write(encryptionValues[ch]);
            }
            else write.Write(c);
        }
        write.Close();
    }
}
