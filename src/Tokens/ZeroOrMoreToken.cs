using codecrafters_grep.src.Tokens;

public class ZeroOrMoreToken : IToken
{
    private IToken _token;
    public ZeroOrMoreToken(IToken token)
    {
        _token = token;
    }
    public bool Matches(char c)
    {
        return _token.Matches(c);
    }
}