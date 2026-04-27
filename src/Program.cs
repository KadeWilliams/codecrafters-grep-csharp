using codecrafters_grep.src.Tokens;
using System.ComponentModel.Design;
using System.Reflection.Metadata.Ecma335;

static bool MatchHere(
    string inputLine,
    int inputPosition,
    List<IToken> tokens,
    int tokenPosition,
    ref List<string> matchedCapture,
    bool endAchorPresent = false)
{
    Console.Error.WriteLine($"ENTER: input='{inputLine}' inputPos={inputPosition} tokPos={tokenPosition} tokCount={tokens.Count}");

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
        if (tokens[tokenPosition] is not ZeroOrOneToken && tokens[tokenPosition] is not OneOrMoreToken)
        {
            return false;
        }
        return MatchHere(inputLine, inputPosition, tokens, tokenPosition + 1, ref matchedCapture, endAchorPresent);
    }

    // if token matches recurse through again; iterating one for both token and input positions
    if (tokens[tokenPosition].Matches(inputLine[inputPosition]))
    {
        int curInp = inputPosition;
        int curTok = tokenPosition;
        if (tokens[tokenPosition] is OneOrMoreToken)
        {
            return MatchHere(inputLine, curInp + 1, tokens, curTok, ref matchedCapture, endAchorPresent) || MatchHere(inputLine, curInp + 1, tokens, curTok + 1, ref matchedCapture, endAchorPresent);
        }

        if (tokens[tokenPosition] is ZeroOrOneToken)
        {
            return MatchHere(inputLine, curInp + 1, tokens, curTok + 1, ref matchedCapture, endAchorPresent) || MatchHere(inputLine, curInp, tokens, curTok + 1, ref matchedCapture, endAchorPresent);
        }

        return MatchHere(inputLine, ++inputPosition, tokens, ++tokenPosition, ref matchedCapture, endAchorPresent);
    }
    else if (tokens[tokenPosition] is ZeroOrOneToken)
    {
        return MatchHere(inputLine, inputPosition, tokens, ++tokenPosition, ref matchedCapture, endAchorPresent);
    }
    else if (tokens[tokenPosition] is AlternationToken alt)
    {
        foreach (var tokenList in alt.GetTokens)
        {
            var combined = new List<IToken>(tokenList);
            combined.AddRange(tokens.Skip(tokenPosition + 1));
            if (MatchHere(inputLine, inputPosition, combined, 0, ref matchedCapture, endAchorPresent))
                return true;
        }
    }
    else if (tokens[tokenPosition] is CaptureGroupToken cgt)
    {
        var capGroupTokens = new List<IToken>(cgt.GetTokens);
        for (int i = inputPosition; i <= inputLine.Length; i++)
        {

            if (MatchHere(inputLine.Substring(inputPosition, i - inputPosition), 0, capGroupTokens, 0, ref matchedCapture, endAchorPresent))
            {
                matchedCapture.Add(inputLine.Substring(inputPosition, i - inputPosition));
                var remainingTokens = new List<IToken>(tokens.Skip(tokenPosition + 1));
                if (MatchHere(inputLine, i, remainingTokens, 0, ref matchedCapture, endAchorPresent))
                {
                    return true;
                }
                else
                {
                    matchedCapture.RemoveAt(matchedCapture.Count - 1);
                }
            }
        }
        return false;
    }
    else if (tokens[tokenPosition] is BackreferenceToken brt)
    {
        var capturedString = matchedCapture.ElementAt(brt.Position - 1);
        var peekDistance = inputPosition + capturedString.Length;
        if (peekDistance > inputLine.Length)
        {
            return false;
        }

        if (inputLine.Substring(inputPosition, peekDistance) == capturedString)
        {
            return MatchHere(inputLine.Substring(peekDistance), inputPosition + capturedString.Length, tokens, ++tokenPosition, ref matchedCapture, endAchorPresent);
        }
        return false;
    }

    return false;
}

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
    newIndex = index;
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
            default:
                var value = int.Parse(pattern[newIndex + 1].ToString());
                newIndex += 2;
                return new BackreferenceToken(value);

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
        return new CharacterGroupToken(groupList, isNegative);
    }
    else if (pattern[newIndex] == '(')
    {
        newIndex++;
        bool pipeVisited = false;
        var altOptions = new List<List<IToken>>();
        List<IToken> altOption = new List<IToken>();
        while (true)
        {
            if (pattern[newIndex] == ')')
            {
                if (pipeVisited)
                {
                    altOptions.Add(altOption);
                    newIndex++;
                    break;
                }
                newIndex++;
                return new CaptureGroupToken(altOption);
            }
            else if (pattern[newIndex] == '|')
            {
                altOptions.Add(altOption);
                altOption = new List<IToken>();
                pipeVisited = true;
                newIndex++;
            }
            else
            {
                var innerToken = CreateToken(pattern, newIndex, out newIndex);
                altOption.Add(WrapIfQuantifier(pattern, newIndex, innerToken, out newIndex));
            }
        }
        return new AlternationToken(altOptions);
    }
    else if (pattern[newIndex] == '.')
    {
        newIndex++;
        return new WildcardToken();
    }
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
        Console.WriteLine($"pattern: {pattern} i: {i}");
        var ct = CreateToken(pattern, i, out i);

        tokens.Add(WrapIfQuantifier(pattern, i, ct, out i));
    }

    var consumedChars = new List<string>();
    if (startAnchorPresent)
    {
        return MatchHere(inputLine, 0, tokens, 0, ref consumedChars, endAnchorPresent);
    }

    for (int j = 0; j <= inputLine.Length - 1; j++)
    {
        if (MatchHere(inputLine, j, tokens, 0, ref consumedChars, endAnchorPresent))
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

if (MatchPattern(inputLine, pattern))
{
    Environment.Exit(0);
}
else
{
    Environment.Exit(1);
}
