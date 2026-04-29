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
        //foreach (var tokenss in _tokens)
        //{
        //    foreach (var token in tokenss)
        //    {
        //        Console.WriteLine(token.GetType());
        //    }
        //}
        Console.WriteLine($"Class Input: {c}");
        return false;
    }

    public List<List<IToken>> GetTokens => _tokens;
}
