namespace JsonPolimi_Core_nf.Tipi;

public class Result
{
    public bool isReady;
    public object result;

    internal void SetResult(object value)
    {
        result = value;
        isReady = true;
    }
}