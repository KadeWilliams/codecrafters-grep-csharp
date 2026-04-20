using codecrafters_grep.src.Tokens;

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
            Console.WriteLine($"Token: {value}");
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
        Console.WriteLine($"Input Point: {inputLine[inputPointer]}");
        if (!endAnchorPresent)
        {
            if (patternPointer == tokenList.Count())
            {
                return true;
            }
        }
        else if (endAnchorPresent && inputPointer == inputLine.Length - 1) // if $ is present and we're at the end of input 
        {
            return true;
        }
        else if (endAnchorPresent && inputPointer != inputLine.Length - 1)
        {
            return false;
        }

        if (tokenList[patternPointer].Matches(inputLine[inputPointer]))
        {
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
