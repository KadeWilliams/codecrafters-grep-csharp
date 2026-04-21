using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_grep.src.Tokens;

public class WildcardToken : IToken
{
    private IToken _token;
    public WildcardToken(IToken token)
    {
        _token = token;
    }
    public bool Matches(char c)
    {
        return true;
    }
}
