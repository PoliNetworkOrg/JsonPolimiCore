using System;
using System.Collections.Generic;

namespace JsonPolimi_Core_nf.Tipi
{
    public class ParametriFunzione
    {
        public Dictionary<string,object> _params;

        internal object GetParam(string v)
        {
            return _params[v];
        }

        public void AddParam(object value, string key)
        {
            _params[key] = value;
        }
    }
}