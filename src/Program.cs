using codecrafters_grep.src.Tokens;
using System.Reflection.Metadata.Ecma335;

/*
    iter1: 
        MatchHere("1 apples", 0, "\d", 0)
            is token position equal to tokens count 
                no; 0 != 1
            is input position greater than or equal to inputLine.Length
                no; 0 ! >= 1
            is "\d" equal to "1"
                yes; 
    iter2: 
        MatchHere("1 apples", 1, "\d", 1)
            is token position equal to tokens count 
                yes; 1 != 1; return 1
 */

static bool MatchHere(string inputLine, int inputPosition, List<IToken> tokens, int tokenPosition, bool endAchorPresent = false)
{
    // we've gotten through all the tokens without failing
    if (tokenPosition == tokens.Count())
    {
        // we've completed the tokens but not reached the end of the input and the end of the input has to match 
        if (endAchorPresent && inputPosition < inputLine.Length)
        {
            return false;
        }
        return true;
    }

    // we've gotten through all of the input characters without passing
    if (inputPosition >= inputLine.Length)
    {
        return false;
    }

    Console.WriteLine($"Currently checking: {inputLine[inputPosition]}");
    // if token matches recurse through again; iterating one for both token and input positions
    if (tokens[tokenPosition].Matches(inputLine[inputPosition]))
    {
        int curInp = inputPosition;
        int curTok = tokenPosition;
        if (tokens[tokenPosition] is OneOrMoreToken)
        {
            return MatchHere(inputLine, curInp + 1, tokens, curTok, endAchorPresent) || MatchHere(inputLine, curInp + 1, tokens, curTok + 1, endAchorPresent);
        }

        if (tokens[tokenPosition] is ZeroOrOneToken)
        {
            return MatchHere(inputLine, curInp + 1, tokens, curTok + 1, endAchorPresent) || MatchHere(inputLine, curInp, tokens, curTok + 1, endAchorPresent);
        }

        return MatchHere(inputLine, ++inputPosition, tokens, ++tokenPosition, endAchorPresent);
    }
    else if (tokens[tokenPosition] is ZeroOrOneToken)
    {
        return MatchHere(inputLine, inputPosition, tokens, ++tokenPosition, endAchorPresent);
    }
    else if (tokens[tokenPosition] is AlternationToken alt)
    {
        foreach (var tokenList in alt.GetTokens)
        {
            return MatchHere(inputLine, inputPosition, tokenList, ++tokenPosition, endAchorPresent);
        }
    }

    return false;
}

/*
    iter1: 
        MatchHere("1 apples", 0, "\d", 0)
            is token position equal to tokens count 
                no; 0 != 1
            is input position greater than or equal to inputLine.Length
                no; 0 ! >= 1
            is "\d" equal to "1"
                yes; 
    iter2: 
        MatchHere("1 apples", 1, "\d", 1)
            is token position equal to tokens count 
                yes; 1 != 1; return 1
 */

static IToken WrapIfQuantifier(string pattern, int index, IToken token, out int newIndex)
{
    newIndex = index;
    if (index >= pattern.Length)
    {
        return token;
    }

    switch (pattern[index])
    {
        case '+':
            newIndex++;
            return new OneOrMoreToken(token);
        case '?':
            newIndex++;
            return new ZeroOrOneToken(token);
    }
    return token;
}

static IToken CreateToken(string pattern, int index, out int newIndex)
{
    Console.WriteLine($"Checking index: {index}");
    newIndex = index;
    Console.WriteLine($"Checking token: {pattern[newIndex]}");
    if (pattern[newIndex] == '\\')
    {
        switch (pattern[newIndex + 1])
        {
            case 'w':
                newIndex += 2;
                return new AlphaNumericToken();
            case 'd':
                newIndex += 2;
                return new DigitToken();
        }
    }
    else if (pattern[newIndex] == '[')
    {
        var isNegative = false;
        if (pattern[newIndex + 1] == '^')
        {
            isNegative = true;
            newIndex += 2;
        }
        else
        {
            newIndex++;
        }

        var groupList = new List<char>();
        while (true)
        {
            if (pattern[newIndex] == ']')
            {
                newIndex++;
                break;
            }

            groupList.Add(pattern[newIndex]);
            newIndex++;
        }
        var tokenGroup = new CharacterGroupToken(groupList, isNegative);
        return tokenGroup;
    }
    else if (pattern[newIndex] == '(')
    {
        newIndex++;
        var altOptions = new List<List<IToken>>();
        var altOption = new List<IToken>();
        while (true)
        {
            if (pattern[newIndex] == ')')
            {
                altOptions.Add(altOption);
                break;
            }

            if (pattern[newIndex] == '|')
            {
                altOptions.Add(altOption);
                altOption = new List<IToken>();
                newIndex++;
            }
            else
            {
                altOption.Add(CreateToken(pattern, newIndex, out newIndex));
            }
        }
        return new AlternationToken(altOptions);

    }
    Console.WriteLine($"Adding token literal: {pattern[newIndex]}");
    var lt = new LiteralToken(pattern[newIndex]);
    newIndex++;
    return lt;
}

static bool MatchPattern(string inputLine, string pattern)
{
    var startAnchorPresent = false;
    var endAnchorPresent = false;
    var tokens = new List<IToken>();
    int i = 0;
    while (i < pattern.Length)
    {
        var value = pattern[i];
        Console.WriteLine($"Value: {value}");
        if (value == '^' && i == 0)
        {
            startAnchorPresent = true;
            i++;
            continue;
        }

        if (value == '$' && i == pattern.Length - 1)
        {
            endAnchorPresent = true;
            i++;
            continue;
        }

        var ct = CreateToken(pattern, i, out i);

        tokens.Add(WrapIfQuantifier(pattern, i, ct, out i));
        Console.WriteLine($"i after wrap: {i}");
    }

    if (startAnchorPresent)
    {
        return MatchHere(inputLine, 0, tokens, 0, endAnchorPresent);
    }

    for (int j = 0; j <= inputLine.Length - 1; j++)
    {
        if (MatchHere(inputLine, j, tokens, 0, endAnchorPresent))
        {
            return true;
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
