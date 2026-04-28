using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_grep.src.Tokens;

public class NQuantifierToken : IToken
{
    private int _number;
    private IToken _token;
    public NQuantifierToken(int number, IToken token)
    {
        _number = number;
        _token = token;
    }

    public bool Matches(char c)
    {
        return _token.Matches(c);
    }

    //public int Number => _number;
    public int Number { get; set; }
}
