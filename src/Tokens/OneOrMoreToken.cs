using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_grep.src.Tokens;

public class OneOrMoreToken : IToken
{
    private IToken _token;
    public OneOrMoreToken(IToken token)
    {
        _token = token;
    }
    public bool Matches(char c)
    {
        throw new NotImplementedException();
    }
}
