using System.Collections.Generic;

namespace JsonPolimi_Core_nf.Tipi;

public class ParametriFunzione
{
    public Dictionary<string, object> _params;

    internal object GetParam(string v)
    {
        if (_params == null)
            return null;

        return _params[v];
    }

    public void AddParam(object value, string key)
    {
        if (_params == null)
            _params = new Dictionary<string, object>();

        _params[key] = value;
    }
}