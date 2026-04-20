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
            switch (pattern[i+1])
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
            if (pattern[i+1] == '^')
            {
                isNegative = true;
                i+=2;
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
            else if (value == '$' && i ==  pattern.Length - 1)
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
        if (patternPointer == tokenList.Count())
        {
            return true;
        }
        if (tokenList[patternPointer].Matches(inputLine[inputPointer]))
        {
            
            Console.WriteLine("Matched token");
            matchedCharacters.Add(inputLine[inputPointer].ToString());
            inputPointer++;
            patternPointer++;
            continue;
        } 
        else if (startAnchorPresent)
        {
            return false;
        }
        else if (endAnchorPresent)
        {
            Console.WriteLine("End anchor present");
            Console.WriteLine($"Input Pointer: {inputPointer}");
            Console.WriteLine($"Input Line: {inputLine.Length}");
            if (inputPointer == inputLine.Length - 1)
                return true;
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
