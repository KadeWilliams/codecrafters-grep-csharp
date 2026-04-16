using System;
using System.IO;
using System.Text.RegularExpressions;

static bool MatchPattern(string inputLine, string pattern)
{
    var patternList = new List<string>();
    foreach (var (value, index) in pattern.Select((v, i) => (v, i)))
    {
        patternList.Add(value.ToString());
        if (value == '\\')
        {
            patternList.Add(pattern[index + 1].ToString());
        }
    }

    var patternPointer = 0;
    var inputPointer = 0;
    var recheckPointer = 0;

    while (inputPointer <= inputLine.Length - 1)
    {
        if (patternPointer == patternList.Count() - 1)
            return true;

        if (patternList[patternPointer] == "\\d")
        {
            if (char.IsDigit(inputLine[inputPointer]))
            {
                inputPointer++;
                patternPointer++;
                continue;
            }
        }
        else if (inputLine[inputPointer].ToString() == patternList[patternPointer])
        {
            inputPointer++;
            patternPointer++;
            continue;

        }
        else
        {
            inputPointer = recheckPointer;
            patternPointer = 0;
            recheckPointer++;
            continue;
        }
    }
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
