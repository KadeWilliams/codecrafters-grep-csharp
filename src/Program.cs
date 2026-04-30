using codecrafters_grep.src.Tokens;
using System.ComponentModel.Design;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

static bool MatchHere(
    string inputLine,
    int inputPosition,
    List<IToken> tokens,
    int tokenPosition,
    ref Dictionary<int, string> matchedCapture,
    bool endAchorPresent = false)
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
        if (tokens[tokenPosition] is not ZeroOrOneToken
            && tokens[tokenPosition] is not ZeroOrMoreToken
            && tokens[tokenPosition] is not NQuantifierToken)
        {
            return false;
        }

        if (tokens[tokenPosition] is NQuantifierToken q)
        {
            if (q.Number > 0)
            {
                return false;
            }
            else if (q.Number == 0)
            {
                return true;
            }
        }
        return MatchHere(inputLine, inputPosition, tokens, tokenPosition + 1, ref matchedCapture, endAchorPresent);
    }

    if (tokens[tokenPosition] is NQuantifierToken n)
    {
        if (n.Number == 0 && n.AtLeastNTimes)
        {
            if (n.MaxNumber is not null) // it has a max value
            {
                if (n.MaxNumber == 0) // meaning it's been exhausted
                {
                    var maxInner = new List<IToken> { n.InnerToken }; // grab the inner token to check 
                    for (int i = 0; i < inputLine.Length; i++) // check from the beginning until it fails, return false?
                    {
                        if (MatchHere(inputLine.Substring(inputPosition, i - inputPosition), 0, maxInner, 0, ref matchedCapture, endAchorPresent))
                        {
                            return false;
                        }
                    }
                    return MatchHere(inputLine, inputPosition, tokens, tokenPosition + 1, ref matchedCapture, endAchorPresent);
                }
                else
                {
                    //    // return MatchHere() || MatchHere();
                    var newTokens = new List<IToken>(tokens);
                    newTokens[tokenPosition] = new NQuantifierToken(n.Number, n.InnerToken, n.AtLeastNTimes, n.MaxNumber - 1);
                    //    if (n.InnerToken.Matches(inputLine[inputPosition]))
                    //    {
                    //        return MatchHere(inputLine, inputPosition, newTokens, tokenPosition, ref matchedCapture, endAchorPresent)
                    //            || MatchHere(inputLine, inputPosition, newTokens, tokenPosition + 1, ref matchedCapture, endAchorPresent);
                    //    }
                    for (int i = inputPosition; i <= inputLine.Length; i++)
                    {
                        if (MatchHere(inputLine.Substring(inputPosition, i - inputPosition), 0, newTokens, 0, ref matchedCapture, endAchorPresent))
                        {
                            newTokens[tokenPosition] = new NQuantifierToken(n.Number - 1, n.InnerToken, n.AtLeastNTimes, n.MaxNumber);
                            return MatchHere(inputLine, i, newTokens, tokenPosition, ref matchedCapture, endAchorPresent);
                        }
                    }
                }
            }
            else
            {
                var newTokens = new List<IToken>(tokens);
                newTokens[tokenPosition] = new ZeroOrMoreToken(n.InnerToken);
                return MatchHere(inputLine, inputPosition, newTokens, tokenPosition, ref matchedCapture, endAchorPresent);
            }
        }

        if (n.Number == 0)
        {
            return MatchHere(inputLine, inputPosition, tokens, tokenPosition + 1, ref matchedCapture, endAchorPresent);
        }

        var innerTokens = new List<IToken> { n.InnerToken };
        for (int i = inputPosition; i <= inputLine.Length; i++)
        {
            if (MatchHere(inputLine.Substring(inputPosition, i - inputPosition), 0, innerTokens, 0, ref matchedCapture, endAchorPresent))
            {
                var newTokens = new List<IToken>(tokens);
                newTokens[tokenPosition] = new NQuantifierToken(n.Number - 1, n.InnerToken, n.AtLeastNTimes, n.MaxNumber);
                return MatchHere(inputLine, i, newTokens, tokenPosition, ref matchedCapture, endAchorPresent);
            }
        }
        return false;
    }

    if (tokens[tokenPosition].Matches(inputLine[inputPosition]))
    {
        int curInp = inputPosition;
        int curTok = tokenPosition;
        if (tokens[tokenPosition] is OneOrMoreToken)
        {
            return MatchHere(inputLine, curInp + 1, tokens, curTok, ref matchedCapture, endAchorPresent) || MatchHere(inputLine, curInp + 1, tokens, curTok + 1, ref matchedCapture, endAchorPresent);
        }

        if (tokens[tokenPosition] is ZeroOrMoreToken)
        {
            return MatchHere(inputLine, curInp + 1, tokens, curTok, ref matchedCapture, endAchorPresent) || MatchHere(inputLine, curInp, tokens, curTok + 1, ref matchedCapture, endAchorPresent);
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
    else if (tokens[tokenPosition] is ZeroOrMoreToken)
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
    //else if (tokens[tokenPosition] is NQuantifierToken nqt)
    //{
    //    var newTokens = new List<IToken>(tokens);
    //    newTokens[tokenPosition] = new NQuantifierToken(nqt.Number - 1, nqt.InnerToken);
    //    return MatchHere(inputLine, curInp + 1, newTokens, curTok, ref matchedCapture, endAchorPresent);
    //}
    else if (tokens[tokenPosition] is CaptureGroupToken cgt)
    {
        var capGroupTokens = new List<IToken>(cgt.GetTokens);
        for (int i = inputPosition; i <= inputLine.Length; i++)
        {
            if (MatchHere(inputLine.Substring(inputPosition, i - inputPosition), 0, capGroupTokens, 0, ref matchedCapture, endAchorPresent))
            {
                matchedCapture[cgt.GroupNumber] = inputLine.Substring(inputPosition, i - inputPosition);
                var remainingTokens = new List<IToken>(tokens.Skip(tokenPosition + 1));
                if (MatchHere(inputLine, i, remainingTokens, 0, ref matchedCapture, endAchorPresent))
                {
                    return true;
                }
                else
                {
                    matchedCapture.Remove(cgt.GroupNumber);
                }
            }
        }
        return false;
    }
    else if (tokens[tokenPosition] is BackreferenceToken brt)
    {
        if (!matchedCapture.ContainsKey(brt.Position))
            return false;

        var capturedString = matchedCapture[brt.Position];
        var peekDistance = inputPosition + capturedString.Length;
        if (peekDistance > inputLine.Length)
        {
            return false;
        }

        if (inputLine.Substring(inputPosition, capturedString.Length) == capturedString)
        {
            return MatchHere(inputLine.Substring(peekDistance), 0, tokens, ++tokenPosition, ref matchedCapture, endAchorPresent);
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
        case '*':
            newIndex++;
            return new ZeroOrMoreToken(token);
        case '{':
            newIndex++;// to be on the number
            int num = int.Parse(pattern[newIndex].ToString());
            // need to check if the next char is a ',' it means it's a different type of n quantifier 
            // then I'd need to advance 3 characters to get to the next token on the next iteration
            // else advance 2 characters like before
            newIndex++;//to be on either the ',' or the '}'

            if (pattern[newIndex] == ',')
            {
                //advance beyond the '}'
                newIndex++;
                int? maxNumber = null;
                var nextChar = pattern[newIndex];
                if (char.IsDigit(nextChar))
                {
                    maxNumber = int.Parse(nextChar.ToString());
                }
                newIndex++;
                return new NQuantifierToken(num, token, true, maxNumber);
            }
            newIndex++;// advance to next token
            return new NQuantifierToken(num, token, false);
    }
    return token;
}

static IToken CreateToken(string pattern, int index, out int newIndex, ref int groupNumberCount)
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
            case '\\':
                newIndex += 2;
                return new LiteralToken('\\');
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
        groupNumberCount++;
        int myGroupNumber = groupNumberCount;
        newIndex++;
        var altOptions = new List<List<IToken>>();
        List<IToken> altOption = new List<IToken>();
        var pipeVisited = false;
        while (true)
        {
            if (pattern[newIndex] == ')')
            {
                altOptions.Add(altOption);
                var altToken = new AlternationToken(altOptions);
                altOption = [altToken];
                newIndex++;

                if (pipeVisited)
                {
                    return new CaptureGroupToken(new List<IToken> { altToken }, myGroupNumber);
                }
                return new CaptureGroupToken(altOption, myGroupNumber);
            }
            else if (pattern[newIndex] == '|')
            {
                pipeVisited = true;
                altOptions.Add(altOption);
                altOption = new List<IToken>();
                newIndex++;
            }
            else
            {
                var innerToken = CreateToken(pattern, newIndex, out newIndex, ref groupNumberCount);
                altOption.Add(WrapIfQuantifier(pattern, newIndex, innerToken, out newIndex));
            }
        }
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
    int groupNumber = 0;
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
        var ct = CreateToken(pattern, i, out i, ref groupNumber);
        tokens.Add(WrapIfQuantifier(pattern, i, ct, out i));
    }

    var consumedChars = new Dictionary<int, string>();
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

static bool ProcessFiles(IEnumerable<string> files, string pattern, bool includeFileName)
{
    bool lineFound = false;
    foreach (var file in files)
    {
        var inputLines = File.ReadAllLines(file);
        foreach (var line in inputLines)
        {
            if (MatchPattern(line, pattern))
            {
                lineFound = true;
                if (includeFileName)
                {
                    Console.WriteLine($"{file}:{line}");
                }
                else
                {
                    Console.WriteLine($"{line}");
                }
            }
        }
    }
    return lineFound;
}

static List<string> GetDirectories(string directory)
{
    return Directory.GetDirectories(directory).ToList();
}

if (args[0] == "-r")
{
    var directory = args.Skip(3);
    var dirs = new List<string>
    {
        directory.First()
    };

    dirs.AddRange(GetDirectories(directory.First()));

    var files = new List<string>();
    foreach (var dir in dirs)
    {
        foreach (var file in Directory.GetFiles(dir))
        {
            files.Add(file);
        }
    }

    string pattern = args[2];
    bool lineFound = ProcessFiles(files, pattern, true);
    if (lineFound)
    {
        Environment.Exit(0);
    }
    else
    {
        Environment.Exit(1);
    }

}
else if (args[0] == "-E")
{
    string pattern = args[1];
    if (args.Length > 2)
    {
        var files = args.Skip(2);
        bool includeFileName = false;
        if (files.Count() > 1)
            includeFileName = true;
        bool lineFound = ProcessFiles(files, pattern, includeFileName);

        if (lineFound)
        {
            Environment.Exit(0);
        }
        else
        {
            Environment.Exit(1);
        }
    }
    else
    {
        string inputLine = Console.In.ReadToEnd();
        if (MatchPattern(inputLine, pattern))
        {
            Environment.Exit(0);
        }
        else
        {
            Environment.Exit(1);
        }
    }
}
else
{
    Environment.Exit(2);
}
