using codecrafters_grep.src.Tokens;
using System.ComponentModel.Design;
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

static bool MatchHere(
    string inputLine,
    int inputPosition,
    List<IToken> tokens,
    int tokenPosition,
    ref List<string> matchedCapture,
    bool endAchorPresent = false)
{
    // At the top of MatchHere
    //Console.Error.WriteLine($"MatchHere: inputPos={inputPosition} tokPos={tokenPosition} input='{(inputPosition < inputLine.Length ? inputLine[inputPosition].ToString() : "END")}' token={(tokenPosition < tokens.Count ? tokens[tokenPosition].GetType().Name : "END")}");

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
        //// Inside AlternationToken branch
        //Console.Error.WriteLine($"Alternation: trying sequence of {tokenList.Count} tokens");
        foreach (var tokenList in alt.GetTokens)
        {
            var combined = new List<IToken>(tokenList);
            combined.AddRange(tokens.Skip(tokenPosition + 1));
            if (MatchHere(inputLine, inputPosition, combined, 0, ref matchedCapture, endAchorPresent))
                return true;
        }
    }
    // there's a group of characters we have to check
    else if (tokens[tokenPosition] is CaptureGroupToken capGroupToken)
    {
        // Inside CaptureGroupToken branch, before the loop
        //Console.Error.WriteLine($"CaptureGroup: trying positions {inputPosition} to {inputLine.Length}");

        // combine the tokens within the group with the rest of characters in the token list

        // start at the first character in the input and increment
        for (int i = inputPosition; i <= inputLine.Length; i++)
        {
            // Inside the loop, before first MatchHere
            Console.Error.WriteLine($"CaptureGroup: trying substring '{inputLine.Substring(inputPosition, i - inputPosition)}'");

            // pass the substring starting from the input position that's passed to the curent loop minus the input position 
            // if the capture group is at position 0 then it'd be from 0 to 0 - 0 for the first loop
            // subsequent loops would be 0, i + 1 - 0
            // we only need to pass the capture group tokens through this list because they're the only ones that matter until this passes or fails
            // pass the referenced list of matched captures
            Console.Error.WriteLine($"endAnchorPresent={endAchorPresent} substring check");
            var capResult = MatchHere(inputLine.Substring(inputPosition, i - inputPosition), 0, capGroupToken.GetTokens, 0, ref matchedCapture, endAchorPresent);
            Console.Error.WriteLine($"substring='{inputLine.Substring(inputPosition, i - inputPosition)}' result={capResult}");
            if (capResult)
            {
                // When capture succeeds
                //Console.Error.WriteLine($"CaptureGroup: captured '{inputLine.Substring(inputPosition, i - inputPosition)}'");

                Console.Error.WriteLine($"CaptureGroup: matched! captured='{inputLine.Substring(inputPosition, i - inputPosition)}'");

                // if the capture group passes we capture that string and store it in the matched capture list from the starting position to the end of the loop
                matchedCapture.Add(inputLine.Substring(inputPosition, i - inputPosition));

                // When captures list is written to
                //Console.Error.WriteLine($"Captures list: [{string.Join(", ", matchedCapture)}]");

                // we then continue with the rest of the input starting from the end of this loop
                // we pass the combined list and start at 0 
                var combined = new List<IToken>(tokens.Skip(tokenPosition + 1));

                if (MatchHere(inputLine, i, combined, 0, ref matchedCapture, endAchorPresent))
                {
                    return true;
                }
                matchedCapture.RemoveAt(matchedCapture.Count - 1);
            }
        }
        return false;
    }
    else if (tokens[tokenPosition] is BackreferenceToken backRef)
    {
        // if input is "cat and cat" and pattern is "(cat) and \1"
        // if we're in this branch we're at \1
        // we need to check from the last inputToken that passed? 

        var capString = matchedCapture[backRef.Position - 1];
        Console.WriteLine($"Captured String: {capString}");
        if (inputLine.Substring(inputPosition, capString.Length) == capString)
        {
            Console.WriteLine($"Substring: {inputLine.Substring(inputPosition, capString.Length)}");
            inputPosition += capString.Length;
            return MatchHere(inputLine, inputPosition, tokens, ++tokenPosition, ref matchedCapture, endAchorPresent);
        }
        return false;
    }

    return false;
}

/*
    iter0: 
        MatchHere("cat and cat", 0, "(cat) and \1", 0, null)
            is token position equal to tokens counts   
                no 
            is input position greater than or equal to inputLine Length
                no
            does 
                
    ----------------
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
                altOption.Add(CreateToken(pattern, newIndex, out newIndex));
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

        var ct = CreateToken(pattern, i, out i);

        tokens.Add(WrapIfQuantifier(pattern, i, ct, out i));
    }

    /*
     * TESTING WHAT'S BEING STORED
     */
    //foreach (var (value, index) in tokens.Select((v, i) => (v, i)))
    //{
    //    Console.WriteLine(value.GetType().Name);
    //}

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
