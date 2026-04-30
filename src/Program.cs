using codecrafters_grep.src.Tokens;
using System.ComponentModel.Design;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text.Json;

static (bool, int) MatchHere(
    string inputLine,
    int inputPosition,
    List<IToken> tokens,
    int tokenPosition,
    ref Dictionary<int, string> matchedCapture,
    bool endAchorPresent = false)
{
    if (tokenPosition == tokens.Count())
    {
        if (endAchorPresent && inputPosition < inputLine.Length)
        {
            return (false, -1);
        }
        return (true, inputPosition);
    }

    if (inputPosition >= inputLine.Length)
    {
        if (tokens[tokenPosition] is not ZeroOrOneToken
            && tokens[tokenPosition] is not ZeroOrMoreToken
            && tokens[tokenPosition] is not NQuantifierToken)
        {
            return (false, -1);
        }

        if (tokens[tokenPosition] is NQuantifierToken q)
        {
            if (q.Number > 0)
            {
                return (false, -1);
            }
            //else if (q.Number == 0 && q.MaxNumber > 0)
            //{
            //    return true;
            //}
        }
        return MatchHere(inputLine, inputPosition, tokens, tokenPosition + 1, ref matchedCapture, endAchorPresent);
    }

    if (tokens[tokenPosition] is NQuantifierToken n)
    {
        if (n.Number == 0)
        {
            if (!n.AtLeastNTimes) // {n}
            {
                return MatchHere(inputLine, inputPosition, tokens, tokenPosition + 1, ref matchedCapture, endAchorPresent);
            }

            if (n.AtLeastNTimes && n.MaxNumber is null) // {n,}
            {
                var newTokens = new List<IToken>(tokens);
                newTokens[tokenPosition] = new ZeroOrMoreToken(n.InnerToken);
                return MatchHere(inputLine, inputPosition, newTokens, tokenPosition, ref matchedCapture, endAchorPresent);
            }

            if (n.AtLeastNTimes && n.MaxNumber is not null) // {n,m}
            {
                if (n.MaxNumber == 0) // {n,0}
                {
                    return MatchHere(inputLine, inputPosition, tokens, tokenPosition + 1, ref matchedCapture, endAchorPresent);
                }
                if (n.MaxNumber > 0) // {n,m>0}
                {
                    var inn = new List<IToken> { n.InnerToken };
                    if (MatchHere(inputLine.Substring(inputPosition, 1), 0, inn, 0, ref matchedCapture, endAchorPresent).Item1)
                    {
                        var nt = new List<IToken>(tokens);
                        nt[tokenPosition] = new NQuantifierToken(n.Number, n.InnerToken, n.AtLeastNTimes, n.MaxNumber - 1);
                        return MatchHere(inputLine, inputPosition + 1, nt, tokenPosition, ref matchedCapture, endAchorPresent);
                    }
                    else
                    {
                        var result = MatchHere(inputLine, inputPosition, tokens, tokenPosition + 1, ref matchedCapture, endAchorPresent);
                        return result;
                    }

                }
            }
        }


        var innerTokens = new List<IToken> { n.InnerToken };
        for (int i = inputPosition + 1; i <= inputLine.Length; i++)
        {
            if (MatchHere(inputLine.Substring(inputPosition, i - inputPosition), 0, innerTokens, 0, ref matchedCapture, endAchorPresent).Item1)
            {
                var maxNumber = n?.MaxNumber;

                if (maxNumber is not null)
                {
                    maxNumber--;
                }

                var newTokens = new List<IToken>(tokens);
                newTokens[tokenPosition] = new NQuantifierToken(n.Number - 1, n.InnerToken, n.AtLeastNTimes, maxNumber);
                return MatchHere(inputLine, i, newTokens, tokenPosition, ref matchedCapture, endAchorPresent);
            }

        }
        return (false, -1);
    }

    if (tokens[tokenPosition].Matches(inputLine[inputPosition]))
    {
        int curInp = inputPosition;
        int curTok = tokenPosition;
        if (tokens[tokenPosition] is OneOrMoreToken)
        {
            var left = MatchHere(inputLine, curInp + 1, tokens, curTok, ref matchedCapture, endAchorPresent);
            if (left.Item1)
                return left;
            return MatchHere(inputLine, curInp + 1, tokens, curTok + 1, ref matchedCapture, endAchorPresent);
        }

        if (tokens[tokenPosition] is ZeroOrMoreToken)
        {
            var left = MatchHere(inputLine, curInp + 1, tokens, curTok, ref matchedCapture, endAchorPresent);
            if (left.Item1)
                return left;
            return MatchHere(inputLine, curInp, tokens, curTok + 1, ref matchedCapture, endAchorPresent);
        }

        if (tokens[tokenPosition] is ZeroOrOneToken)
        {
            var left = MatchHere(inputLine, curInp + 1, tokens, curTok + 1, ref matchedCapture, endAchorPresent);
            if (left.Item1)
                return left;

            return MatchHere(inputLine, curInp, tokens, curTok + 1, ref matchedCapture, endAchorPresent);
        }

        return MatchHere(inputLine, ++inputPosition, tokens, ++tokenPosition, ref matchedCapture, endAchorPresent);
    }
    else if (tokens[tokenPosition] is ZeroOrOneToken)
    {
        return MatchHere(inputLine, inputPosition, tokens, ++tokenPosition, ref matchedCapture, endAchorPresent);
    }
    else if (tokens[tokenPosition] is ZeroOrMoreToken z)
    {
        var t = new List<IToken> { z.Token };
        var left = MatchHere(inputLine, inputPosition, t, 0, ref matchedCapture, endAchorPresent);
        if (left.Item1)
        {
            return MatchHere(inputLine, left.Item2, tokens, tokenPosition, ref matchedCapture, endAchorPresent);
        }
        return MatchHere(inputLine, inputPosition, tokens, ++tokenPosition, ref matchedCapture, endAchorPresent);
    }
    else if (tokens[tokenPosition] is AlternationToken alt)
    {
        foreach (var tokenList in alt.GetTokens)
        {
            var combined = new List<IToken>(tokenList);
            combined.AddRange(tokens.Skip(tokenPosition + 1));
            var (matched, ind) = MatchHere(inputLine, inputPosition, combined, 0, ref matchedCapture, endAchorPresent);
            if (matched)
                return (true, ind);
        }
    }
    else if (tokens[tokenPosition] is CaptureGroupToken cgt)
    {
        var capGroupTokens = new List<IToken>(cgt.GetTokens);
        for (int i = inputPosition; i <= inputLine.Length; i++)
        {
            if (MatchHere(inputLine.Substring(inputPosition, i - inputPosition), 0, capGroupTokens, 0, ref matchedCapture, endAchorPresent).Item1)
            {
                matchedCapture[cgt.GroupNumber] = inputLine.Substring(inputPosition, i - inputPosition);
                var remainingTokens = new List<IToken>(tokens.Skip(tokenPosition + 1));
                var (matched, ind) = MatchHere(inputLine, i, remainingTokens, 0, ref matchedCapture, endAchorPresent);
                if (matched)
                {
                    return (true, ind);
                }
                else
                {
                    matchedCapture.Remove(cgt.GroupNumber);
                }
            }
        }
        return (false, -1);
    }
    else if (tokens[tokenPosition] is BackreferenceToken brt)
    {
        if (!matchedCapture.ContainsKey(brt.Position))
        {
            return (false, -1);
        }

        //PrintComplexObject(matchedCapture);

        var capturedString = matchedCapture[brt.Position];
        var peekDistance = inputPosition + capturedString.Length;
        if (peekDistance > inputLine.Length)
        {
            return (false, -1);
        }

        if (inputLine.Substring(inputPosition, capturedString.Length) == capturedString)
        {
            return MatchHere(inputLine, peekDistance, tokens, ++tokenPosition, ref matchedCapture, endAchorPresent);
        }
        return (false, -1);
    }

    return (false, -1);
}

static void PrintComplexObject(object v)
{
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(v));
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
            newIndex++;
            int num = int.Parse(pattern[newIndex].ToString());
            newIndex++;

            if (pattern[newIndex] == ',')
            {

                newIndex++;
                int? maxNumber = null;
                var nextChar = pattern[newIndex];
                if (char.IsDigit(nextChar))
                {
                    maxNumber = int.Parse(nextChar.ToString());
                    newIndex++;
                }
                newIndex++;
                return new NQuantifierToken(num, token, true, maxNumber);
            }
            newIndex++;
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

static string? MatchPattern(string inputLine, string pattern)
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
        var (matched, ind) = MatchHere(inputLine, 0, tokens, 0, ref consumedChars, endAnchorPresent);
        if (matched)
        {
            return inputLine.Substring(0, ind);
        }
        return null;
    }

    var listOfMatches = new List<string>();
    for (int j = 0; j <= inputLine.Length - 1; j++)
    {
        var (matched, ind) = MatchHere(inputLine, j, tokens, 0, ref consumedChars, endAnchorPresent);
        if (matched)
        {
            var str = inputLine.Substring(j, ind - j);
            PrintComplexObject(new { matched, ind, str });
            listOfMatches.Add(inputLine.Substring(j, ind - j));
            //return inputLine.Substring(j, ind - j);
        }
    }

    foreach (var match in listOfMatches)
    {
        Console.WriteLine(match);
    }

    return null;
}

static bool ProcessFiles(IEnumerable<string> files, string pattern, bool includeFileName)
{
    bool lineFound = false;
    foreach (var file in files)
    {
        var inputLines = File.ReadAllLines(file);
        foreach (var line in inputLines)
        {
            if (!string.IsNullOrEmpty(MatchPattern(line, pattern)))
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
    bool curFound = false;
    bool found = false;
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
        if (inputLine.Contains("\n"))
        {
            var inputs = inputLine.Split('\n');
            foreach (var i in inputs)
            {
                curFound = !string.IsNullOrEmpty(MatchPattern(i, pattern));
                if (curFound)
                {
                    found = true;
                    Console.WriteLine($"{i}");
                }
            }
        }
        else
        {
            curFound = !string.IsNullOrEmpty(MatchPattern(inputLine, pattern));
            if (curFound)
            {
                found = true;
                Console.WriteLine($"{inputLine}");
            }
        }
    }

    if (found)
    {
        Environment.Exit(0);
    }
    else
    {
        Environment.Exit(1);
    }
}
else if (args[0] == "-o")
{
    string pattern = args[2];
    bool curFound = false;
    bool found = false;
    string inputLine = Console.In.ReadToEnd();
    if (inputLine.Contains("\n"))
    {
        var inputs = inputLine.Split('\n');
        foreach (var i in inputs)
        {
            var foundString = MatchPattern(i, pattern);
            curFound = !string.IsNullOrEmpty(foundString);
            if (curFound)
            {
                found = true;
                Console.WriteLine($"{foundString}");
            }
        }
    }
    else
    {
        var foundString = MatchPattern(inputLine, pattern);
        curFound = !string.IsNullOrEmpty(foundString);
        Console.WriteLine(curFound);
        if (curFound)
        {
            found = true;
            Console.WriteLine($"{foundString}");
        }
    }

    if (found)
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
    Environment.Exit(2);
}
