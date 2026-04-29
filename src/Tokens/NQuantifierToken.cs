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
    private bool _atLeastNTimes = false;
    private int? _maxNumber;
    public NQuantifierToken(int number, IToken token, bool atLeastNTimes, int? maxNumber = null)
    {
        _number = number;
        _token = token;
        _atLeastNTimes = atLeastNTimes;
        _maxNumber = maxNumber;
    }

    public bool Matches(char c)
    {
        return false;
    }

    public int Number
    {
        get => _number;
        set => _number = value;
    }
    public IToken InnerToken => _token;
    public bool AtLeastNTimes => _atLeastNTimes;
    public int? MaxNumber
    {
        get => _maxNumber;
        set => _maxNumber = value;
    }
}
