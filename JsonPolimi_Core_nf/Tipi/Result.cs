using System;

namespace JsonPolimi_Core_nf.Tipi
{

    public class Result
    {
        public object result = null;
        public bool isReady = false;

        internal void SetResult(object value)
        {
            this.result = value;
            this.isReady = true;
        }
    }
}