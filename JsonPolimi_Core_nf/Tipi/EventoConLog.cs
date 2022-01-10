using System;
using System.Collections.Generic;
using System.Threading;

namespace JsonPolimi_Core_nf.Tipi
{
    public class EventoConLog
    {
        public System.Action<object, object> action;
        private readonly List<string> logs = new List<string>();
        private readonly Result result = new Result();

        public void RunAction()
        {
            this.action.Invoke(null, null);
        }

        public void Log(string s)
        {
            lock (logs)
            {
                logs.Add(s);
            }
        }

        public Tuple<List<string>,int> GetLog()
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
            return this.result;
        }
    }

}