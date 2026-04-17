namespace codecrafters_grep.src.Tokens;

public class DigitToken : IToken
{
    public bool Matches(char c)
    {
        return char.IsDigit(c);
    }
}