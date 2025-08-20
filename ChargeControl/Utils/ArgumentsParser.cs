namespace ChargeControl;

public class ArgumentsParser
{
    private List<string> _args;

    public ArgumentsParser(string[] args)
    {
        _args = args.ToList();
    }

    private string? GetArgument(string name, bool hasValue)
    {
        var index = _args.FindIndex(s => s == "-" + name);
        if (index != -1)
        {
            if (!hasValue)
                return _args.ElementAt(index);
            
            if (_args.Count >= index)
            {
                return _args.ElementAt(index + 1);
            }

            throw new ArgumentException($"Option {name} requires value");
        }

        return null;
    }

    public string? GetString(string name, bool hasValue)
    {
        return GetArgument(name, hasValue);
    }
    
    public double? GetDouble(string name, bool hasValue)
    {
        var value = GetArgument(name, hasValue);
        return value == null ? null : Convert.ToDouble(value.Trim());
    }
    
    public int? GetInt(string name, bool hasValue)
    {
        var value = GetArgument(name, hasValue);
        return value == null ? null : Convert.ToInt32(value.Trim());
    }
}