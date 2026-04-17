using System;
using System.IO;
using System.Text.RegularExpressions;

static bool MatchPattern(string inputLine, string pattern)
{
    var patternList = new List<string>();
    for (var i = 0; i<=pattern.Length - 1; i++)
    {
        var value = pattern[i];
        if (value == '\\')
        {
            patternList.Add($"{value.ToString()}{pattern[i+1].ToString()}");
            i++;
        } 
        else
        {
            patternList.Add(value.ToString());
        }
    }

    Console.Error.WriteLine(string.Join(", ", patternList));
    var patternPointer = 0;
    var inputPointer = 0;
    var recheckPointer = 0;
    var matchedCharacters = new List<string>();

    while (inputPointer <= inputLine.Length - 1)
    {
        if (patternPointer == patternList.Count())
        {
            Console.WriteLine(string.Join(", ", matchedCharacters));
            return true;
        }

        if (patternList[patternPointer] == "\\d")
        {
            if (char.IsDigit(inputLine[inputPointer]))
            {
                matchedCharacters.Add(inputLine[inputPointer].ToString());
                inputPointer++;
                patternPointer++;
                continue;
            }
        }
        else if (patternList[patternPointer] == "\\w")
        {
            if (char.IsLetterOrDigit(inputLine[inputPointer]))
            {
                matchedCharacters.Add(inputLine[inputPointer].ToString());
                inputPointer++;
                patternPointer++;
                continue;
            }
        }
        else if (inputLine[inputPointer].ToString() == patternList[patternPointer])
        {
            matchedCharacters.Add(inputLine[inputPointer].ToString());
            inputPointer++;
            patternPointer++;
            continue;

        }
        inputPointer = recheckPointer;
        patternPointer = 0;
        recheckPointer++;
        continue;
    }
    Console.WriteLine(string.Join(", ", matchedCharacters));
    return false;
}

if (args[0] != "-E")
{
    Console.WriteLine("Expected first argument to be '-E'");
    Environment.Exit(2);
}

string pattern = args[1];
string inputLine = Console.In.ReadToEnd();

// You can use print statements as follows for debugging, they'll be visible when running tests.
// Console.Error.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage

if (MatchPattern(inputLine, pattern))
{
    Environment.Exit(0);
}
else
{
    Environment.Exit(1);
}
