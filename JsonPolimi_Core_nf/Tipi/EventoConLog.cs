using System;
using System.Collections.Generic;

namespace JsonPolimi_Core_nf.Tipi;

public class EventoConLog
{
    private readonly List<string> logs = new();
    private readonly Result result = new();
    public Action<object, object> action;

    public void RunAction()
    {
        action.Invoke(null, null);
    }

    public void Log(string s)
    {
        lock (logs)
        {
            logs.Add(s);
        }
    }

    public Tuple<List<string>, int> GetLog()
    {
        lock (logs)
        {
            return new Tuple<List<string>, int>(logs, logs.Count);
        }
    }

    public void SetResult(object value)
    {
        result.SetResult(value);
    }

    public Result GetResult()
    {
        return result;
    }
}