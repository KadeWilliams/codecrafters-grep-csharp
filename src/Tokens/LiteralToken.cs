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
        return c == _character;
    }
}
