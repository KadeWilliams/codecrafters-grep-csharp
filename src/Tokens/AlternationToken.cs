using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_grep.src.Tokens;

public class AlternationToken : IToken
{
    private List<List<IToken>> _tokens;
    public AlternationToken(List<List<IToken>> tokens)
    {
        _tokens = tokens;
    }

    public bool Matches(char c)
    {
        return false;
    }
}
