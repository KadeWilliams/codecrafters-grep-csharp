using System;
using System.IO;
using System.Text.RegularExpressions;

static bool MatchPattern(string inputLine, string pattern)
{
    Console.WriteLine(inputLine);
    Console.WriteLine(pattern);

    if (pattern.Length == 1)
    {
        return inputLine.Contains(pattern);
    } 
    else if (pattern == "\\d")
    {
        return inputLine.Any(char.IsDigit);
    }
    else if (pattern == "\\w")
    {
        return inputLine.Any(char.IsLetterOrDigit) || inputLine.Contains("_");
    } 
    else if (pattern.StartsWith("[") && pattern.EndsWith("]"))
    {
        var validCharacters = pattern.Substring(pattern.IndexOf('[') + 1, pattern.IndexOf(']') - pattern.IndexOf('[') - 1);
        return validCharacters.Any(c => inputLine.Contains(c));
    }
    else
    {
        throw new ArgumentException($"Unhandled pattern: {pattern}");
    }
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
