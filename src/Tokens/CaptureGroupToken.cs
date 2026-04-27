using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_grep.src.Tokens;

public class CaptureGroupToken : IToken
{
    private List<IToken> _tokens;
    private int _groupNumber;
    public CaptureGroupToken(List<IToken> tokens, int groupNumber)
    {
        _tokens = tokens;
        _groupNumber = groupNumber;
    }
    public bool Matches(char c)
    {
        return false;
    }
    public List<IToken> GetTokens => _tokens;
    public int GroupNumber => _groupNumber;
}
