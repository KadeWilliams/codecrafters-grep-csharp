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

        if (tokens[tokenPosition] is WildcardToken)
        {
            return MatchHere(inputLine, ++inputPosition, tokens, curTok, endAchorPresent);
        }
        return MatchHere(inputLine, ++inputPosition, tokens, ++tokenPosition, endAchorPresent);
    }
    else if (tokens[tokenPosition] is ZeroOrOneToken)
    {
        return MatchHere(inputLine, inputPosition, tokens, ++tokenPosition, endAchorPresent);
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

static IToken WrapIfQuantifier(string pattern, int index, IToken token)
{
    if (index >= pattern.Length)
    {
        return token;
    }

    switch (pattern[index])
    {
        case '+':
            return new OneOrMoreToken(token);
        case '?':
            return new ZeroOrOneToken(token);
        case '.':
            return new OneOrMoreToken(token);
    }

    return token;
}

static bool MatchPattern(string inputLine, string pattern)
{
    var startAnchorPresent = false;
    var endAnchorPresent = false;
    var tokens = new List<IToken>();
    for (var i = 0; i <= pattern.Length - 1; i++)
    {
        var value = pattern[i];

        if (value == '\\')
        {
            switch (pattern[i + 1])
            {
                case 'w':
                    var ant = new AlphaNumericToken();
                    var qAnt = WrapIfQuantifier(pattern, i + 2, ant);
                    if (ant.GetType() != qAnt.GetType())
                    {
                        i++;
                    }
                    tokens.Add(qAnt);
                    break;
                case 'd':
                    var dt = new DigitToken();
                    var qDt = WrapIfQuantifier(pattern, i + 2, dt);
                    if (dt.GetType() != qDt.GetType())
                    {
                        i++;
                    }
                    tokens.Add(qDt);
                    break;
            }
            i++;
        }
        else if (value == '[')
        {
            var isNegative = false;
            if (pattern[i + 1] == '^')
            {
                isNegative = true;
                i += 2;
            }
            else
            {
                i++;
            }

            var groupList = new List<char>();
            while (true)
            {
                if (pattern[i] == ']')
                    break;

                groupList.Add(pattern[i]);
                i++;
            }
            var tokenGroup = new CharacterGroupToken(groupList, isNegative);
            tokens.Add(tokenGroup);
        }
        else
        {
            if (value == '^' && i == 0)
            {
                startAnchorPresent = true;
                continue;
            }

            if (value == '$' && i == pattern.Length - 1)
            {
                endAnchorPresent = true;
                continue;
            }

            if (value == '.')
            {
                var wct = new WildcardToken();
                var qWct = WrapIfQuantifier(pattern, i + 1, wct);
                if (wct.GetType() != qWct.GetType())
                {
                    i++;
                }
                continue;
            }
            var lt = new LiteralToken(value);
            var qLt = WrapIfQuantifier(pattern, i + 1, lt);
            if (lt.GetType() != qLt.GetType())
            {
                i++;
            }
            tokens.Add(qLt);
        }
    }

    if (startAnchorPresent)
    {
        return MatchHere(inputLine, 0, tokens, 0, endAnchorPresent);
    }

    for (int i = 0; i <= inputLine.Length - 1; i++)
    {
        if (MatchHere(inputLine, i, tokens, 0, endAnchorPresent))
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
/*
static bool MatchPattern(string inputLine, string pattern)
{
    var startAnchorPresent = false;
    var endAnchorPresent = false;
    var tokenList = new List<IToken>();

    for (var i = 0; i <= pattern.Length - 1; i++)
    {
        var value = pattern[i];

        if (value == '\\')
        {
            switch (pattern[i + 1])
            {
                case 'w':
                    tokenList.Add(new AlphaNumericToken());
                    break;
                case 'd':
                    tokenList.Add(new DigitToken());
                    break;
            }
            i++;
        }
        else if (value == '[')
        {
            var isNegative = false;
            if (pattern[i + 1] == '^')
            {
                isNegative = true;
                i += 2;
            }
            else
            {
                i++;
            }

            var groupList = new List<char>();
            while (true)
            {
                if (pattern[i] == ']')
                    break;

                groupList.Add(pattern[i]);
                i++;
            }
            var tokenGroup = new CharacterGroupToken(groupList, isNegative);
            tokenList.Add(tokenGroup);
        }
        else
        {
            if (value == '^' && i == 0)
            {
                startAnchorPresent = true;
                continue;
            }

            if (value == '$' && i == pattern.Length - 1)
            {
                endAnchorPresent = true;
                continue;
            }
            tokenList.Add(new LiteralToken(value));
        }
    }

    var patternPointer = 0;
    var inputPointer = 0;
    var recheckPointer = 0;
    var matchedCharacters = new List<string>();

    while (inputPointer <= inputLine.Length - 1)
    {
        if (!endAnchorPresent)
        {
            if (patternPointer == tokenList.Count())
            {
                return true;
            }
        }

        // if we're checking the end and we've reached the end of the token list and we still have more characters to check then the end anchor does not pass
        if (endAnchorPresent && patternPointer == tokenList.Count() && inputPointer < inputLine.Length - 1)
        {
            return false;
        }

        if (tokenList[patternPointer].Matches(inputLine[inputPointer]))
        {
            if (endAnchorPresent && inputPointer == inputLine.Length - 1)
            {
                return true;
            }
            matchedCharacters.Add(inputLine[inputPointer].ToString());
            inputPointer++;
            patternPointer++;
            continue;
        }
        else if (startAnchorPresent)
        {
            return false;
        }

        inputPointer = recheckPointer;
        patternPointer = 0;
        recheckPointer++;
        continue;
    }
    if (patternPointer == tokenList.Count())
    {
        return true;
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
*/
