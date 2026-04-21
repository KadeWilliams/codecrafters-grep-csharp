using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_grep.src.Tokens;

public class ZeroOrOneToken : IToken
{
    private IToken _token;
    public ZeroOrOneToken(IToken token)
    {
        _token = token;
    }
    public bool Matches(char c)
    {
        return _token.Matches(c);
    }
}
