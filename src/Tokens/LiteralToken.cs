namespace codecrafters_grep.src.Tokens;

public class LiteralToken : IToken
{
    private char _character;
    public LiteralToken(char character)
    {
        _character = character;
    }
    public bool Matches(char c)
    {
        Console.WriteLine(c);
        Console.WriteLine(_character);
        return c == _character;
    }
}
