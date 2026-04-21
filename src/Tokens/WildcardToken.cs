using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_grep.src.Tokens;

public class WildcardToken : IToken
{
    public bool Matches(char c)
    {
        return true;
    }
}
