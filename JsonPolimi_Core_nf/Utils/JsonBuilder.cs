using System;
using JsonPolimi_Core_nf.Data;
using JsonPolimi_Core_nf.Tipi;

namespace JsonPolimi_Core_nf.Utils;

public static class JsonBuilder
{
    public static string? GetJson(CheckGruppo v, bool entrambi_index)
    {
        if (Variabili.L == null)
            return null;
        Variabili.L.Sort();
        Aggiusta();
        Variabili.L.Sort();

        var n = Variabili.L.GetCount();

        var json = "{";

        if (entrambi_index)
        {
            json += "\"info_data\":{";

            for (var i = 0; i < n; i++)
            {
                var elem = Variabili.L.GetElem(i);

                var tenere = DoCheckGruppo(v, elem);
                if (!tenere) continue;
                json += '\n';
                json += '"';
                json += elem?.Id;
                json += '"' + ":";
                json += elem?.To_json(v.n);
                json += ',';
            }

            if (json.EndsWith(",")) json = json[..^1];

            json += "}";
            json += ",";
        }

        if (true)
        {
            json += '\n';
            json += "    \"index_data\": [";
            for (var i = 0; i < n; i++)
            {
                var elem = Variabili.L.GetElem(i);
                var tenere = DoCheckGruppo(v, elem);
                if (!tenere) continue;
                json += '\n';
                json += "        ";
                json += elem?.To_json(v.n);
                json += ',';
            }

            if (json.EndsWith(",")) json = json[..^1];

            json += "    ]";
            json += "\n";
        }

        json += "}";

        return json;
    }

    private static void Aggiusta()
    {
        Variabili.L ??= new ListaGruppo();

        var n = Variabili.L.GetCount();
        for (var i = 0; i < n; i++)
        {
            var elem = Variabili.L.GetElem(i);
            if (!string.IsNullOrEmpty(elem?.Id)) continue;
            Variabili.L.Remove(i);

            i--;
            n = Variabili.L.GetCount();
        }

        n = Variabili.L.GetCount();
        for (var i = 0; i < n; i++)
        {
            var elem = Variabili.L.GetElem(i);

            var nome = AggiustaNome(elem?.Classe);
            if (elem != null)
            {
                elem.Classe = nome;
            }            
            Variabili.L.SetElem(i, elem);
        }

        Variabili.L.Sort();
    }

    private static string? AggiustaNome(string? s)
    {
        if (s == null)
            return null;

        if (s.Contains("<="))
        {
            var n = s.IndexOf("<=", StringComparison.Ordinal);
            var r = "";
            r += s[..n];
            r += s[(n + 2)..];
            return r;
        }

        if (s.Contains("&lt;="))
        {
            var n = s.IndexOf("&lt;=", StringComparison.Ordinal);
            var r = "";
            r += s[..n];
            r += s[(n + 5)..];
            return r;
        }

        s = s.Replace("&apos;", "'");
        s = s.Replace("&amp;", "&");

        return s;
    }

    private static bool DoCheckGruppo(CheckGruppo? v, Gruppo? elem)
    {
        switch (v?.n)
        {
            case CheckGruppo.E.RICERCA_SITO_V3:
            case CheckGruppo.E.VECCHIA_RICERCA:
            {
                if (string.IsNullOrEmpty(elem?.Classe))
                    return false;
                if (string.IsNullOrEmpty(elem.IdLink))
                    return false;
                break;
            }
            case CheckGruppo.E.NUOVA_RICERCA:
            {
                if (Empty(elem?.CCS))
                    return false;

                break;
            }
            case CheckGruppo.E.TUTTO:
            {
                break;
            }
        }

        return true;
    }

    private static bool Empty(ListaStringhePerJSON? cCS)
    {
        return cCS == null || cCS.IsEmpty();
    }
}