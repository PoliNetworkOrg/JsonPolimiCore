using System;
using System.Collections.Generic;
using System.Threading;

namespace JsonPolimi_Core_nf.Tipi
{
    public class EventoConLog<T>
    {
        public System.Action<object, object> action;
        private readonly List<string> logs = new List<string>();
        public Result<T> result = new Result<T>();

        

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
    }

}