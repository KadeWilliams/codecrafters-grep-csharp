using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_grep.src.Tokens;

public class CharacterGroupToken : IToken
{

    private List<char> _tokens;
    private bool _isNegative;
    public CharacterGroupToken(List<char> tokens, bool isNegative)
    {
        _tokens = tokens;
        _isNegative = isNegative;
    }
    public bool Matches(char c)
    {
        if (_isNegative)
        {
            return !_tokens.Contains(c);
        }
        return _tokens.Contains(c);
    }
}
