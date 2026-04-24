using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_grep.src.Tokens;

public class BackreferenceToken : IToken
{
    private int _position;
    public BackreferenceToken(int position)
    {
        _position = position;
    }

    public bool Matches(char c)
    {
        return false;
    }
    public int Position => _position;
}
