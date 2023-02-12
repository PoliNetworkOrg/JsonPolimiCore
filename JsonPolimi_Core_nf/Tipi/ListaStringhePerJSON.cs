using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonPolimi_Core_nf.Tipi;

[Serializable]
public class ListaStringhePerJSON
{
    public List<string> o;

    public ListaStringhePerJSON(List<string> office)
    {
        o = office;
    }

    public ListaStringhePerJSON(string v)
    {
        o = new List<string> { v };
    }

    public override string ToString()
    {
        if (o == null)
            return null;

        var r = "";
        foreach (var v in o)
        {
            r += v;
            r += ", ";
        }

        r = r.Substring(0, r.Length - 2);
        return r;
    }

    public bool IsEmpty()
    {
        return o == null || o.Count == 0 || o.Any(string.IsNullOrEmpty);
    }

    public string StringNotNull()
    {
        return o == null ? null : ToString();
    }

    public bool Contains_In_Uno(string v)
    {
        return o != null && o.Any(s2 => s2.Contains(v));
    }

    public string GetCCSCode()
    {
        if (o == null)
            return null;

        if (o.Count < 2)
        {
            var s2 = o[0].Split(' ');

            foreach (var x1 in s2)
                if (x1.StartsWith("(") && x1.EndsWith("),"))
                {
                    var s3 = x1.Substring(0, x1.Length - 1);
                    return GetCcsCode2(s3);
                }
                else if (x1.StartsWith("(") && x1.EndsWith(")"))
                {
                    return GetCcsCode2(x1);
                }

            return GetCcsCode2(s2[^1]);
        }

        foreach (var x1 in o)
            if (x1.StartsWith("(") && x1.EndsWith("),"))
            {
                var s3 = x1.Substring(0, x1.Length - 1);
                return GetCcsCode2(s3);
            }
            else if (x1.StartsWith("(") && x1.EndsWith(")"))
            {
                return GetCcsCode2(x1);
            }

        return null;
    }

    private static string GetCcsCode2(string v)
    {
        var s = v.Trim();
        if (!s.StartsWith("(") || !s.EndsWith(")")) return null;
        s = s[1..];
        s = s[..^1];

        return s;

    }

    /*
        return (o1 == o2)  =>  0
        return (o1 >  o2)  => +1
        return (o1 <  o2)  => -1
    */

    public static int Confronta(ListaStringhePerJSON o1, ListaStringhePerJSON o2)
    {
        switch (o1)
        {
            case null when o2 == null:
                return 0;
            case null:
                return -1;
        }

        if (o2 == null)
            return 1;

        var contained = true;
        foreach (var contains in o1.o.Select(i1 => o2.o.Contains(i1)).Where(contains => !contains))
        {
            contained = false;
        }

        if (contained)
        {
            if (o1.o.Count == o2.o.Count)
                return 0;

            return -1;
        }

        foreach (var contains in o2.o.Select(i2 => o1.o.Contains(i2)).Where(contains => !contains)) contained = false;

        if (contained) return o1.o.Count == o2.o.Count ? 0 : 1;

        ;

        return 1;
    }

    public static bool IsEmpty(ListaStringhePerJSON o)
    {
        return o == null || o.IsEmpty();
    }
}