using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using JsonPolimi_Core_nf.Data;
using JsonPolimi_Core_nf.Enums;

namespace JsonPolimi_Core_nf.Tipi;

[Serializable]
public class Gruppo
{
    public static WebClient clientDownload = new();
    private string? classe_hidden;

    public string? Degree;
    public string? Id; // esempio: FB/2018/2019/LEONARDO/21432583243205
    public string? IdLink; // esempio: 21432583243205
    public string? Language;
    public DateTime? LastUpdateInviteLinkTime;
    public bool? LinkFunzionante;
    public ListaStringhePerJSON? Office; // esempio: LEONARDO

    public string? PermanentId; //per telegram, esempio -1000345953
    public string? Platform; // esempio: FB
    public string? School;
    public string? Tipo;
    public TipoLink TipoLink = TipoLink.UNKNOWN;
    public string? Year; // esempio: 2018/2019

    public string? Classe
    {
        get => classe_hidden;
        set => classe_hidden = value;
    }

    public string? IDCorsoPolimi { get; set; }
    public List<string?>? GruppoTabellaInsegnamenti { get; set; }
    public InfoManifesto? Manifesto { get; set; }
    public int? AnnoCorsoStudio { get; set; }
    public ListaStringhePerJSON? CCS { get; set; }
    public string? PianoDiStudi { get; set; }
    public string? NomeCorso { get; set; }

    public string? GetHTML_DataRow(string textBoxAnno, string textBoxPiattaforma)
    {
        if (!string.IsNullOrEmpty(textBoxAnno))
            if (textBoxAnno != Year)
                return "";

        if (!string.IsNullOrEmpty(textBoxPiattaforma))
            if (textBoxPiattaforma != Platform)
                return "";

        var html = "";
        html += "<tr>";

        html += "<td>";
        html += Id;
        html += "</td>";

        html += "<td>";
        html += Platform;
        html += "</td>";

        html += "<td>";
        html += Classe;
        html += "</td>";

        html += "<td>";
        html += Degree;
        html += "</td>";

        html += "<td>";
        html += Language;
        html += "</td>";

        html += "<td>";
        html += Office;
        html += "</td>";

        html += "<td>";
        html += School;
        html += "</td>";

        html += "<td>";
        html += Tipo;
        html += "</td>";

        html += "<td>";
        html += Year;
        html += "</td>";

        html += "<td>";
        html += IdLink;
        html += "</td>";

        html += "<td>";
        html += PermanentId;
        html += "</td>";

        html += "<td>";
        html += NomeCorso;
        html += "</td>";

        html += "<td>";
        html += GetLink();
        html += "</td>";

        html += "<td>";
        html += LinkFunzionante;
        html += "</td>";

        html += "<td>";
        html += TipoLink;
        html += "</td>";

        html += "</tr>";
        return html;
    }

    public void Aggiusta(bool aggiustaAnno, bool creaid)
    {
        Classe = string.IsNullOrEmpty(Classe) ? "" : Classe.Replace('\n', ' ');

        if (string.IsNullOrEmpty(Tipo)) Tipo = "S";

        if (aggiustaAnno)
            AggiustaAnno();

        if (Tipo != "G")
            switch (string.IsNullOrEmpty(Year))
            {
                case false when !string.IsNullOrEmpty(Classe) && !string.IsNullOrEmpty(Degree) &&
                                !string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(IdLink) &&
                                !string.IsNullOrEmpty(Language) &&
                                !IsEmpty(Office):
                {
                    if (string.IsNullOrEmpty(Tipo)) Tipo = "S";
                    break;
                }
                case false:
                    Tipo = "S";
                    break;
            }

        if (string.IsNullOrEmpty(Language)) Language = IndovinaLaLinguaDalNome();

        if (string.IsNullOrEmpty(School))
            School = IndovinaLaSchool();

        if (string.IsNullOrEmpty(Degree))
            Degree = IndovinaIlDegree();

        if (string.IsNullOrEmpty(IdLink))
            IdLink = CreaIdLink();

        if (!string.IsNullOrEmpty(IdLink)) LastUpdateInviteLinkTime ??= DateTime.Now;

        if (creaid)
            Id = CreaId();
    }

    public static bool IsEmpty(List<string> office)
    {
        return office == null || office.Count == 0 || office.Any(string.IsNullOrEmpty);
    }

    public void AggiustaAnno()
    {
        if (!string.IsNullOrEmpty(Year)) return;

        var title = Classe?.Replace("/", "-");
        title = title?.Replace(" ", "-");
        var t2 = title?.Split('-');

        var a = AnnoInTitolo(t2);
        if (a < 0) return;

        Year = t2?[a] + "/" + t2?[a + 1];
    }

    private static int AnnoInTitolo(IReadOnlyList<string>? t)
    {
        if (t?.Count <= 1) return -1;

        for (var i = 0; i < t?.Count - 1; i++)
            try
            {
                var a = Convert.ToInt32(t?[i]);
                var b = Convert.ToInt32(t?[i + 1]);
                if (a >= 2016 && b >= 2016)
                    return i;
            }
            catch
            {
                ;
            }

        return -1;
    }

    private string? CreaIdLink()
    {
        string? r2;
        try
        {
            string?[]? r = Id?.Split('/');
            r2 = r?[3];
        }
        catch
        {
            return null;
        }

        return r2;
    }

    private string? CreaId()
    {
        if (string.IsNullOrEmpty(PianoDiStudi))
        {
        }

        return Platform + "/" +
               Year + "/" +
               Office + "/" +
               IdLink + "/" +
               StringNotEmpty(IDCorsoPolimi) + "/" +
               CCS?.GetCCSCode() + "/" +
               PianoDiStudi;
    }

    private static string StringNotEmpty(string? a)
    {
        return a ?? "";
    }

    private static string? IndovinaIlDegree()
    {
        //throw new NotImplementedException();
        return null;
    }

    private static string? IndovinaLaSchool()
    {
        //throw new NotImplementedException();
        return null;
    }

    private string? IndovinaLaLinguaDalNome(string? defaultLanguage = "ITA")
    {
        var c = Classe?.ToLower();

        if (c?.Contains("and") ?? false)
            return "ENG";
        return c?.Contains("for") ?? false ? "ENG" : defaultLanguage;
    }

    public string? To_json(CheckGruppo.E v)
    {
        var json = "{";

        switch (v)
        {
            case CheckGruppo.E.TUTTO:
            {
                json += "\"class\":";
                json += StringCheckNull(EscapeQuotes(Classe));
                json += ",\"office\":";
                json += StringCheckNull(Office);
                json += ",\"id\":";
                json += StringCheckNull(Id);
                json += ",\"degree\":";
                json += StringCheckNull(Degree);
                json += ",\"school\":";
                json += StringCheckNull(School);
                json += ",\"annocorso\":";
                json += StringCheckNull(AnnoCorsoStudio);
                json += ",\"nomecorso\":";
                json += StringCheckNull(NomeCorso);
                json += ",\"idcorso\":";
                json += StringCheckNull(IDCorsoPolimi);
                json += ",\"pianostudi\":";
                json += StringCheckNull(PianoDiStudi);
                json += ",\"id_link\":";
                json += StringCheckNull(IdLink);
                json += ",\"language\":";
                json += StringCheckNull(Language);
                json += ",\"type\":";
                json += StringCheckNull(Tipo);
                json += ",\"year\":";
                json += StringCheckNull(Year);
                json += ",\"ccs\":";
                json += StringCheckNull(CCS);
                json += ",\"permanentId\":";
                json += StringCheckNull(PermanentId);
                json += ",\"LastUpdateInviteLinkTime\":";
                json += StringCheckNull(GetTelegramTime());
                json += ",\"platform\":";
                json += StringCheckNull(Platform);
                json += ",\"LinkType\":";
                json += TipoLinkCheckNull(TipoLink);

                break;
            }

            case CheckGruppo.E.RICERCA_SITO_V3:
            case CheckGruppo.E.VECCHIA_RICERCA:
            {
                json += "\"class\":";
                json += StringCheckNull(EscapeQuotes(Classe));
                json += ",\"office\":";
                json += StringCheckNull(Office);
                json += ",\"id\":";
                json += StringCheckNull(Id);
                json += ",\"degree\":";
                json += StringCheckNull(Degree);
                json += ",\"school\":";
                json += StringCheckNull(School);
                json += ",\"id_link\":";
                json += StringCheckNull(IdLink);
                json += ",\"language\":";
                json += StringCheckNull(Language);
                json += ",\"type\":";
                json += StringCheckNull(Tipo);
                json += ",\"year\":";
                json += StringCheckNull(Year);
                json += ",\"platform\":";
                json += StringCheckNull(Platform);
                json += ",\"permanentId\":";
                json += StringCheckNull(PermanentId);
                json += ",\"LastUpdateInviteLinkTime\":";
                json += StringCheckNull(GetTelegramTime());
                json += ",\"linkfunzionante\":";
                json += BoolCheckNotNull(LinkFunzionante);
                json += ",\"LinkType\":";
                json += TipoLinkCheckNull(TipoLink);
                break;
            }
            case CheckGruppo.E.NUOVA_RICERCA:
            {
                json += "\"class\":";
                json += StringCheckNull(EscapeQuotes(Classe));
                json += ",\"office\":";
                json += StringCheckNull(Office);
                json += ",\"id\":";
                json += StringCheckNull(Id);
                json += ",\"degree\":";
                json += StringCheckNull(Degree);
                json += ",\"school\":";
                json += StringCheckNull(School);
                json += ",\"annocorso\":";
                json += StringCheckNull(AnnoCorsoStudio);
                json += ",\"nomecorso\":";
                json += StringCheckNull(NomeCorso);
                json += ",\"idcorso\":";
                json += StringCheckNull(IDCorsoPolimi);
                json += ",\"pianostudi\":";
                json += StringCheckNull(PianoDiStudi);
                json += ",\"id_link\":";
                json += StringCheckNull(IdLink);
                json += ",\"language\":";
                json += StringCheckNull(Language);
                json += ",\"type\":";
                json += StringCheckNull(Tipo);
                json += ",\"year\":";
                json += StringCheckNull(Year);
                json += ",\"ccs\":";
                json += StringCheckNull(CCS);
                json += ",\"platform\":";
                json += StringCheckNull(Platform);
                json += ",\"LinkType\":";
                json += TipoLinkCheckNull(TipoLink);
                break;
            }
        }

        json += "}";

        return json;
    }

    private static string? BoolCheckNotNull(bool? linkFunzionante)
    {
        return linkFunzionante == null ? "null" : linkFunzionante.Value ? '"' + "Y" + '"' : '"' + "N" + '"';
    }

    private static string? StringCheckNull(int? annoCorsoStudio)
    {
        return annoCorsoStudio == null ? "null" : StringCheckNull(annoCorsoStudio.Value.ToString());
    }

    private static string? StringCheckNull(ListaStringhePerJSON? office)
    {
        if (office == null)
            return "null";

        return '"' + office.StringNotNull() + '"';
    }

    private static string? StringCheckNull(string? s)
    {
        if (string.IsNullOrEmpty(s))
            return "null";

        return '"' + s + '"';
    }

    private static string? TipoLinkCheckNull(TipoLink s)
    {
        if (string.IsNullOrEmpty(s.ToString()))
            return "null";

        return '"' + s.ToString() + '"';
    }

    private static string? EscapeQuotes(string? s)
    {
        if (s == null)
            return null;

        for (var i = 0; i < 3; i++) s = UnEscapeQuotes(s);

        var s2 = "";
        if (s == null) return s2;
        foreach (var t in s)
            if (t == '"')
            {
                s2 += '\\';
                s2 += '"';
                //  =>    \"
            }
            else
            {
                s2 += t;
            }

        return s2;
    }

    private static string? UnEscapeQuotes(string? s)
    {
        if (s == null)
            return null;

        var s2 = "";
        var i = 0;
        while (i < s.Length - 1)
            if (s[i] == '\\' && s[i + 1] == '"')
            {
                s2 += '"';
                i += 2;
            }
            else
            {
                s2 += s[i];
                i++;
            }

        while (i < s.Length)
        {
            s2 += s[i];
            i++;
        }

        return s2;
    }

    public static void AggiungiInformazioneAmbigua(string? v, ref InsiemeDiGruppi g)
    {
        if (string.IsNullOrEmpty(v))
            return;

        var vUpper = v.ToUpper().Trim();

        //bisogna capire che tipo di informazione stiamo ricevendo
        if (v.StartsWith("https://", StringComparison.Ordinal) || v.StartsWith("http://", StringComparison.Ordinal))
        {
            AggiungiLink(v, ref g);
        }
        else if (IsSede(vUpper))
        {
            AggiungiSede(v, ref g);
        }
        else if (vUpper is "FACEBOOK" or "TELEGRAM" or "NON ANCORA CREATO" or "CORSI" or "LUOGO" ||
                 vUpper.StartsWith("LAUREE", StringComparison.Ordinal))
        {
            //è una cella inutile
            ;
        }
        else if (vUpper == "<=")
        {
            //è una cella inutile
            ;
        }
        else if (v.StartsWith("<text:a", StringComparison.Ordinal))
        {
            var n1 = v.IndexOf("xlink:href", StringComparison.Ordinal);
            var s1 = v[(n1 + 12)..];
            string?[] s2 = s1.Split('"');

            var s3 = s2[1]?.Split('>');
            string?[]? s4 = s3?[1]?.Split('<');

            var nome = s4?[0];

            if (nome?.StartsWith("http", StringComparison.Ordinal) ?? false)
            {
                AggiungiLink(s2[0], ref g);
            }
            else
            {
                AggiungiNome(nome, ref g);
                AggiungiLink(s2[0], ref g);
            }
        }
        else
        {
            AggiungiAltro(ref vUpper, ref g, ref v);
        }
    }

    private static bool IsSede(string? vUpper)
    {
        return vUpper is "LEONARDO" or "MANTOVA" or "BOVISA" or "PIACENZA" or "LECCO" or "COMO" or "CREMONA"
            or "LEONARDO-CREMONA" or "LEONARDO*";
    }

    private static void AggiungiAltro(ref string? vUpper, ref InsiemeDiGruppi g, ref string? v)
    {
        switch (vUpper)
        {
            case "LT":
            case "LM":
            case "LU":
                AggiungiTriennaleMagistrale(vUpper, ref g);
                break;

            case "3I":
            case "DES":
            case "AUIC":
            case "ICAT":
            case "3I+AUIC":
            case "ICAT+3I":
            case "DES+3I":
                AggiungiScuola(vUpper, ref g);
                break;

            case "ITA":
            case "ENG":
            case "ITA-ENG":
                AggiungiLingua(vUpper, ref g);
                break;

            default:
                //altrimenti è il nome
                AggiungiNome(v, ref g);
                break;
        }
    }

    /*
        return (o1 == o2)  =>  0
        return (o1 >  o2)  => +1
        return (o1 <  o2)  => -1
    */

    public static int Confronta(ListaStringhePerJSON? o1, ListaStringhePerJSON? o2)
    {
        return ListaStringhePerJSON.Confronta(o1, o2);
    }

    private static void AggiungiLingua(string? vUpper, ref InsiemeDiGruppi g)
    {
        g.GruppoDiBase.Language = vUpper;
        g.NomeOld.Language = vUpper;
    }

    private static void AggiungiScuola(string? vUpper, ref InsiemeDiGruppi g)
    {
        g.GruppoDiBase.School = vUpper;
        g.NomeOld.School = vUpper;
    }

    private static void AggiungiTriennaleMagistrale(string? vUpper, ref InsiemeDiGruppi g)
    {
        g.GruppoDiBase.Degree = vUpper;
        g.NomeOld.Degree = vUpper;
    }

    private static void AggiungiNome(string? v, ref InsiemeDiGruppi g)
    {
        if (v == "<=")
            return;

        if (string.IsNullOrEmpty(v))
            return;

        if (v.Contains("<="))
            return;

        if (v.Contains("=>"))
            return;

        if (v.Contains(" vedasi PoliExtra"))
            return;

        if (string.IsNullOrEmpty(g.GruppoDiBase.Classe))
        {
            g.GruppoDiBase.Classe = v;
        }
        else
        {
            g.GruppoDiBase.Classe += " ";
            g.GruppoDiBase.Classe += v;
        }

        g.NomeOld.Classe = g.GruppoDiBase.Classe;
    }

    private static void AggiungiSede(string? v, ref InsiemeDiGruppi g)
    {
        g.GruppoDiBase.Office = new ListaStringhePerJSON(v);
        g.NomeOld.Office = new ListaStringhePerJSON(v);
    }

    private static void AggiungiLink(string? v, ref InsiemeDiGruppi g)
    {
        var g2 = new Gruppo();

        var n1 = v?.IndexOf("://", StringComparison.Ordinal);
        var s1 = v?[((n1 ?? 0) + 3)..];

        var n2 = s1?.IndexOf("www.", StringComparison.Ordinal);
        if (n2 >= 0 && n2 < s1?.Length) s1 = s1[4..];

        switch (s1?[0])
        {
            // facebook
            case 'f':
            {
                g2.Platform = "FB";

                string?[] s2 = s1.Split('/');
                g2.IdLink = s2[1] == "groups" ? s2[2] : s2[1];
                break;
            }
            // telegram
            case 't':
            {
                g2.Platform = "TG";

                string?[] s2 = s1.Split('/');
                g2.IdLink = s2[1] == "joinchat" ? s2[2] : s2[1];
                break;
            }
            // instagram
            case 'i':
            {
                g2.Platform = "IG";

                string?[] s2 = s1.Split('/');

                g2.IdLink = s2[1];
                break;
            }
            //whatsapp
            case 'c':
            {
                g2.Platform = "WA";

                string?[] s2 = s1.Split('/');

                g2.IdLink = s2[1];
                break;
            }
            default:
                ;
                break;
        }

        g.L.Add(g2);
    }

    public override string ToString()
    {
        return To_json(CheckGruppo.E.TUTTO) + " " + base.ToString();
    }

    public void Merge(Gruppo? gruppo)
    {
        if (!string.IsNullOrEmpty(gruppo?.Classe) && string.IsNullOrEmpty(Classe))
            Classe = gruppo.Classe;

        if (!string.IsNullOrEmpty(gruppo?.Degree) && string.IsNullOrEmpty(Degree))
            Degree = gruppo.Degree;

        if (!string.IsNullOrEmpty(gruppo?.Id) && string.IsNullOrEmpty(Id))
            Id = gruppo.Id;

        if (!string.IsNullOrEmpty(gruppo?.IdLink))
        {
            if (string.IsNullOrEmpty(IdLink))
            {
                IdLink = gruppo.IdLink;
                TipoLink = gruppo.TipoLink;
                LastUpdateInviteLinkTime = gruppo.LastUpdateInviteLinkTime;
            }
            else
            {
                switch (LastUpdateInviteLinkTime)
                {
                    case null when gruppo.LastUpdateInviteLinkTime == null:
                        IdLink = gruppo.IdLink;
                        TipoLink = gruppo.TipoLink;
                        LastUpdateInviteLinkTime = gruppo.LastUpdateInviteLinkTime;
                        break;

                    case null:
                        IdLink = gruppo.IdLink;
                        TipoLink = gruppo.TipoLink;
                        LastUpdateInviteLinkTime = gruppo.LastUpdateInviteLinkTime;
                        break;

                    default:
                    {
                        if (gruppo.LastUpdateInviteLinkTime != null)
                        {
                            var r = DateTime.Compare(LastUpdateInviteLinkTime.Value,
                                gruppo.LastUpdateInviteLinkTime.Value);
                            if (r < 0)
                            {
                                IdLink = gruppo.IdLink;
                                TipoLink = gruppo.TipoLink;
                                LastUpdateInviteLinkTime = gruppo.LastUpdateInviteLinkTime;
                            }
                        }

                        break;
                    }
                }
            }
        }

        LastUpdateInviteLinkTime ??= DateTime.Now;

        if (!string.IsNullOrEmpty(gruppo?.Language) && string.IsNullOrEmpty(Language))
            Language = gruppo.Language;

        if (!IsEmpty(gruppo?.Office) && IsEmpty(Office))
            Office = gruppo?.Office;

        if (!string.IsNullOrEmpty(gruppo?.PermanentId) && string.IsNullOrEmpty(PermanentId))
            PermanentId = gruppo.PermanentId;

        if (!string.IsNullOrEmpty(gruppo?.Platform) && string.IsNullOrEmpty(Platform))
            Platform = gruppo.Platform;

        if (!string.IsNullOrEmpty(gruppo?.School) && string.IsNullOrEmpty(School))
            School = gruppo.School;

        if (!string.IsNullOrEmpty(gruppo?.Tipo) && string.IsNullOrEmpty(Tipo))
            Tipo = gruppo.Tipo;

        if (!string.IsNullOrEmpty(gruppo?.Year) && string.IsNullOrEmpty(Year))
            Year = gruppo.Year;
    }

    public static bool IsEmpty(ListaStringhePerJSON? office)
    {
        return office == null || office.IsEmpty();
    }

    [Obsolete("DEPRECATED")]
    public string? To_json_Tg()
    {
        /*
         {"Chat": {"id": -1001452418598, "type": "supergroup", "title": "Polimi Piacenza \ud83c\uddee\ud83c\uddf9\ud83d\udc48",
         "invite_link": "https://t.me/joinchat/LclXl1aSJiYbzl7wCW5WZg"}, "LastUpdateInviteLinkTime": "2019-08-20 08:47:55.368966", "we_are_admin": true}
        */

        if (string.IsNullOrEmpty(PermanentId))
            return null;

        if (string.IsNullOrEmpty(Classe))
            return null;

        if (string.IsNullOrEmpty(Platform))
            return null;

        if (Platform != "TG")
            return null;

        var json = "{" + '"' + "Chat" + '"' + ":{";

        json += '"' + "id" + '"' + ": ";
        json += PermanentId;
        json += ", " + '"' + "type" + '"' + ": \"supergroup\", \"title\": ";
        json += '"';
        json += Escape(Classe);
        json += '"';
        json += ", \"invite_link\": ";
        json += '"';
        json += GetLink();
        json += '"';
        json += "}, ";
        json += '"' + "LastUpdateInviteLinkTime" + '"';
        json += ": ";
        json += '"';
        json += GetTelegramTime();
        json += '"';
        json += ", ";
        json += '"' + "we_are_admin" + '"';
        json += ": true}";
        return json;
    }

    private static string? Escape(string? classe)
    {
        var a = "" + '\\' + '"';
        var b = "" + '"';
        classe = classe?.Replace(a, b);
        classe = classe?.Replace(a, b);
        classe = classe?.Replace(b, a);
        return classe;
    }

    private string? GetTelegramTime()
    {
        if (LastUpdateInviteLinkTime == null)
            return null;

        //   2019-08-20 08:47:55.368966
        return LastUpdateInviteLinkTime.Value.Year.ToString().PadLeft(4, '0') + "-" +
               LastUpdateInviteLinkTime.Value.Month.ToString().PadLeft(2, '0') + "-" +
               LastUpdateInviteLinkTime.Value.Day.ToString().PadLeft(2, '0') + " " +
               LastUpdateInviteLinkTime.Value.Hour.ToString().PadLeft(2, '0') + ":" +
               LastUpdateInviteLinkTime.Value.Minute.ToString().PadLeft(2, '0') + ":" +
               LastUpdateInviteLinkTime.Value.Second.ToString().PadLeft(2, '0') + "." +
               LastUpdateInviteLinkTime.Value.Millisecond.ToString().PadLeft(3, '0');
    }

    private string? GetLink()
    {
        if (string.IsNullOrEmpty(Platform))
            return "";

        return Platform switch
        {
            "TG" => TipoLink switch
            {
                TipoLink.JOINCHAT => "https://t.me/joinchat/" + IdLink,
                TipoLink.PLUS => "https://t.me/" + IdLink,
                TipoLink.UNKNOWN => "https://t.me/joinchat/" + IdLink,
                _ => "https://t.me/joinchat/" + IdLink
            },
            "WA" => "https://chat.whatsapp.com/" + IdLink,
            "FB" => "https://www.facebook.com/groups/" + IdLink,
            _ => ""
        };
    }

    public static Gruppo? FromInfoParteList(List<InfoParteDiGruppo?>? infoParteDiGruppo_list, string pLAT2)
    {
        if (infoParteDiGruppo_list == null)
            return null;

        switch (infoParteDiGruppo_list.Count)
        {
            case 0:
                return null;
            case < 3 and 2:
            {
                if (infoParteDiGruppo_list[0] == null && infoParteDiGruppo_list[1] == null) return null; //sono sicuro

                if (infoParteDiGruppo_list[0] == null)
                {
                    if (infoParteDiGruppo_list[1] == null) return null;
                    if (string.IsNullOrEmpty(infoParteDiGruppo_list[1]?.testo_selvaggio))
                        return null;
                    if (infoParteDiGruppo_list[1]?.testo_selvaggio is "Insegnamenti in Sequenza"
                        or "Insegnamento completamente offerto in lingua italiana"
                        or "Insegnamento completamente offerto in lingua inglese"
                        or "Insegnamento offerto in lingua italiana e inglese"
                        or "Visualizza offerta non diversificata (***)")
                        return null; //sicuro
                    return null;
                }

                if (string.IsNullOrEmpty(infoParteDiGruppo_list[0]?.testo_selvaggio) &&
                    string.IsNullOrEmpty(infoParteDiGruppo_list[1]?.testo_selvaggio))
                    return null;

                if (infoParteDiGruppo_list[1] == null)
                {
                    if (infoParteDiGruppo_list[0] == null) return null;

                    if (string.IsNullOrEmpty(infoParteDiGruppo_list[0]?.testo_selvaggio))
                        return null;
                    return null;
                }

                if (infoParteDiGruppo_list[1]?.testo_selvaggio is "Primo Semestre" or "Secondo Semestre"
                    or "Insegnamento Annuale" or "Corso Integrato" or "Monodisciplinare" or "Prova Finale"
                    or "Non definita") return null; //sicuro

                if (infoParteDiGruppo_list[0]?.testo_selvaggio is "Preside" or "Livello" or "Classe di Laurea"
                    or "Coordinatore CCS" or "Lingua/e ufficiali") return null; //sicuro

                if (infoParteDiGruppo_list[1]?.testo_selvaggio is "Tirocinio"
                    or "Gli insegnamenti possono essere scelti nell'anno di corso precedente"
                    or "Gli insegnamenti NON possono essere scelti nell'anno di corso precedente" or "Non significativo"
                    or "Ordine di scelta insegnamenti in fase di composizione piano") return null; //sicuro

                if (infoParteDiGruppo_list[0]?.testo_selvaggio == "Scuola")
                {
                    if (Variabili.ParametriCondivisiItem?.infoManifesto != null)
                        Variabili.ParametriCondivisiItem.infoManifesto.Scuola =
                            infoParteDiGruppo_list[1]?.testo_selvaggio?.Trim();
                    return null; //sicuro
                }

                if (infoParteDiGruppo_list[1]?.testo_selvaggio is "Gruppo di insegnamenti a preferenza"
                    or "Italiano/Inglese" or "Laboratorio" or "Mutuabile") return null; //sicuro

                if (infoParteDiGruppo_list[1]?.testo_selvaggio != null &&
                    (infoParteDiGruppo_list[1]?.testo_selvaggio?.StartsWith("Insegnamento") ?? false))
                    return null; //sicuro

                if (infoParteDiGruppo_list[0]?.testo_selvaggio?.StartsWith("Classe di Laurea") ?? false)
                    return null; //sicuro

                if (infoParteDiGruppo_list[1]?.testo_selvaggio?.StartsWith("Workshop") ?? false) return null; //sicuro

                return null;
            }
            case < 3 when infoParteDiGruppo_list.Count != 1:
            //sono sicuro
            case < 3 when infoParteDiGruppo_list[0] == null:
            //sono sicuro
            case < 3 when infoParteDiGruppo_list[0]?.link != null:
            case < 3 when string.IsNullOrEmpty(infoParteDiGruppo_list[0]?.testo_selvaggio):
                return null;
            case < 3:
            {
                switch (infoParteDiGruppo_list[0]?.testo_selvaggio)
                {
                    case "Legenda":
                    //sicuro
                    case "Insegnamenti del Gruppo  TABA":
                        return null; //sicuro
                }

                if (infoParteDiGruppo_list[0]?.testo_selvaggio?.StartsWith("(¹) Il corso di laurea offre ") ?? false)
                    return null; //sicuro
                if (infoParteDiGruppo_list[0]?.testo_selvaggio?.StartsWith("Nessun insegnamento per") ?? false)
                    return null; //sicuro
                if (infoParteDiGruppo_list[0]?.testo_selvaggio?.StartsWith("4<sup>") ?? false)
                    return null; //sicuro
                if (infoParteDiGruppo_list[0]?.testo_selvaggio?.StartsWith("5<sup>") ?? false)
                    return null; //sicuro
                if (infoParteDiGruppo_list[0]?.testo_selvaggio?.StartsWith("(¹) Lo studente a") ?? false)
                    return null; //sicuro
                if (infoParteDiGruppo_list[0]?.testo_selvaggio?.StartsWith("Insegnamenti del Grupp") ?? false)
                    return null; //sicuro
                if (infoParteDiGruppo_list[0]?.testo_selvaggio?.StartsWith("Corso di Studi") ?? false)
                    return null; //sicuro
                return null;
            }
        }

        if (infoParteDiGruppo_list[0] == null && infoParteDiGruppo_list[1] == null)
            return null;

        switch (infoParteDiGruppo_list.Count)
        {
            case 4:
            {
                if (infoParteDiGruppo_list[0]?.testo_selvaggio == "Corso di Studio")
                {
                    var x1 = infoParteDiGruppo_list[1]?.testo_selvaggio?.Trim();
                    var x2 = x1?.Replace('\r', '\t');
                    x2 = x2?.Replace('\n', '\t');
                    var x3 = x2?.Split('\t');
                    var x4 = (from x3A in x3 where !string.IsNullOrEmpty(x3A.Trim()) select x3A.Trim()).ToList();

                    if (Variabili.ParametriCondivisiItem?.infoManifesto != null)
                        Variabili.ParametriCondivisiItem.infoManifesto.Corso_di_studio = x4;
                }

                if (infoParteDiGruppo_list[2]?.testo_selvaggio == "Sede del corso")
                {
                    var x1 = infoParteDiGruppo_list[3]?.testo_selvaggio?.Trim();
                    if (Variabili.ParametriCondivisiItem?.infoManifesto != null)
                        Variabili.ParametriCondivisiItem.infoManifesto.Sede_del_corso = x1?.Split(',');
                }

                if (infoParteDiGruppo_list[0]?.testo_selvaggio == "Anni di Corso Attivi")
                {
                    var x1 = infoParteDiGruppo_list[1]?.testo_selvaggio?.Trim();
                    if (Variabili.ParametriCondivisiItem?.infoManifesto != null)
                        Variabili.ParametriCondivisiItem.infoManifesto.Anni_di_corso_attivi = x1?.Split(',');
                }

                if (infoParteDiGruppo_list[0]?.testo_selvaggio == "Anno Accademico")
                {
                    string? x1 = null;
                    try
                    {
                        x1 = infoParteDiGruppo_list[1]?.testo_selvaggio?.Trim();
                    }
                    catch
                    {
                        ;
                    }

                    if (Variabili.ParametriCondivisiItem?.infoManifesto != null)
                        Variabili.ParametriCondivisiItem.infoManifesto.Anno_accademico = x1;
                }

                if (infoParteDiGruppo_list[2]?.testo_selvaggio == "Sede")
                {
                    string? x1 = null;
                    try
                    {
                        x1 = infoParteDiGruppo_list[3]?.testo_selvaggio?.Trim();
                    }
                    catch
                    {
                        ;
                    }

                    if (Variabili.ParametriCondivisiItem?.infoManifesto != null)
                        Variabili.ParametriCondivisiItem.infoManifesto.Sede = x1;
                }

                if (infoParteDiGruppo_list[2]?.testo_selvaggio != "Durata nominale del Corso")
                    return null; //info interessanti

                var x11 = infoParteDiGruppo_list[3]?.testo_selvaggio?.Trim();
                if (Variabili.ParametriCondivisiItem?.infoManifesto != null)
                    Variabili.ParametriCondivisiItem.infoManifesto.Durata_nominale_corso = x11;


                return null; //info interessanti
            }
            case 10:
            case 9:
            case 11:
            {
                if (infoParteDiGruppo_list[0]?.testo_selvaggio == "--" &&
                    infoParteDiGruppo_list[1]?.testo_selvaggio == "--" &&
                    infoParteDiGruppo_list[2]?.testo_selvaggio == "--" &&
                    infoParteDiGruppo_list[4]?.testo_selvaggio == "--" &&
                    infoParteDiGruppo_list[5]?.testo_selvaggio == "--" &&
                    infoParteDiGruppo_list[6]?.testo_selvaggio == "--")
                    return null; //sicuro

                string? classe = null;
                var n1 = 0;
                if (infoParteDiGruppo_list[3]?.link != null &&
                    !string.IsNullOrEmpty(infoParteDiGruppo_list[3]?.link?.v))
                {
                    classe = infoParteDiGruppo_list[3]?.link?.v;
                    n1 = 3;
                }
                else if (infoParteDiGruppo_list[4]?.link != null &&
                         !string.IsNullOrEmpty(infoParteDiGruppo_list[4]?.link?.v))
                {
                    classe = infoParteDiGruppo_list[4]?.link?.v;
                    n1 = 4;
                }

                string? lang = null;
                try
                {
                    var lingua1 = infoParteDiGruppo_list[n1 + 1]?.lingua;
                    if (lingua1 != null)
                    {
                        var lingua = lingua1.Value;
                        lang = lingua.ToString();
                    }
                }
                catch
                {
                    lang = "??";
                }

                if (string.IsNullOrEmpty(classe)) return null;
                Gruppo? g = new()
                {
                    Classe = classe,
                    IDCorsoPolimi = infoParteDiGruppo_list[0]?.testo_selvaggio,
                    GruppoTabellaInsegnamenti = GetGruppoTabellaInsegnamenti(infoParteDiGruppo_list[1]),
                    Office = new ListaStringhePerJSON(GetSede(infoParteDiGruppo_list[5])),
                    Language = lang,
                    Tipo = "C",
                    AnnoCorsoStudio = Variabili.ParametriCondivisiItem?.anno,
                    Platform = pLAT2,
                    PianoDiStudi = Variabili.ParametriCondivisiItem?.pianostudi2,
                    NomeCorso = classe,
                    IdLink = null
                };
                g.Aggiusta(false, true);
                if (g.IdLink != null)
                {
                }

                return g;
            }
        }

        if (infoParteDiGruppo_list.Count != 3) return null;
        if (infoParteDiGruppo_list[1] == null && infoParteDiGruppo_list[2] == null)
        {
            if (infoParteDiGruppo_list[0] == null) return null;

            if (string.IsNullOrEmpty(infoParteDiGruppo_list[0]?.testo_selvaggio))
                return null; //sicuro
            return null;
        }

        if (infoParteDiGruppo_list[1] != null && infoParteDiGruppo_list[1]?.testo_selvaggio == "Area Servizi ICT")
            return null; //sicuro
        return null;
    }

    public void AggiungiInfoDaManifesto(InfoManifesto infoManifesto)
    {
        Manifesto = infoManifesto;
    }

    public Gruppo Clone()
    {
        Gruppo g = new()
        {
            Classe = Classe,
            Degree = Degree,
            Id = Id,
            IDCorsoPolimi = IDCorsoPolimi,
            IdLink = IdLink,
            TipoLink = TipoLink,
            Language = Language,
            LastUpdateInviteLinkTime = LastUpdateInviteLinkTime,
            Office = Office,
            PermanentId = PermanentId,
            Platform = Platform,
            School = School,
            GruppoTabellaInsegnamenti = GruppoTabellaInsegnamenti,
            Tipo = Tipo,
            Year = Year,
            Manifesto = Manifesto,
            AnnoCorsoStudio = AnnoCorsoStudio,
            CCS = CCS,
            PianoDiStudi = PianoDiStudi,
            NomeCorso = NomeCorso
        };

        return g;
    }

    private static List<string?>? GetGruppoTabellaInsegnamenti(InfoParteDiGruppo? infoParteDiGruppo)
    {
        if (infoParteDiGruppo == null)
            return null;

        List<string?>? l = new();
        if (string.IsNullOrEmpty(infoParteDiGruppo.testo_selvaggio) && infoParteDiGruppo.sottopezzi != null)
        {
            var collection = infoParteDiGruppo.sottopezzi.Select(x1 => x1.testo_selvaggio);
            l.AddRange(collection);
            return l;
        }

        l.Add(infoParteDiGruppo.testo_selvaggio);
        return l;
    }

    private static List<string?>? GetSede(InfoParteDiGruppo? infoParteDiGruppo)
    {
        if (infoParteDiGruppo == null)
            return null;

        if (string.IsNullOrEmpty(infoParteDiGruppo.testo_selvaggio))
            return null;

        return infoParteDiGruppo.testo_selvaggio switch
        {
            "BV" => new List<string?> { "Bovisa" },
            "MI" => new List<string?> { "Leonardo" },
            "--" => null,
            "MN" => new List<string?> { "Mantova" },
            "PC" => new List<string?> { "Piacenza" },
            "LC" => new List<string?> { "Lecco" },
            "CR" => new List<string?> { "Cremona" },
            "CO" => new List<string?> { "Como" },
            "BV, MI" => new List<string?> { "Bovisa", "Leonardo" },
            "LC, MI" => new List<string?> { "Lecco", "Leonardo" },
            _ => null
        };
    }

    public bool IsValido()
    {
        return !string.IsNullOrEmpty(Classe);
        //todo: fare altri controlli per vedere se il gruppo è valido
    }

    public void RicreaId()
    {
        Id = CreaId();
    }

    public void AggiustaNomeDoppio()
    {
        if (string.IsNullOrEmpty(Classe))
            return;

        for (var k = 0; k < Classe.Length - 1; k++)
            if (Classe[k] == ' ' && Classe[k + 1] == ' ')
            {
                Classe = Classe.Remove(k, 1);
                k--;
            }

        var s2 = AggiustaNomeDoppio2(Classe);
        if (s2 != Classe) Classe = s2;

        //AggiustaNomeDoppio3();
    }

#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

    private void AggiustaNomeDoppio3()
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
    {
        var uguale = "";

        var ugualeMax = Classe?.Length / 3f;
        if (ugualeMax < 5)
            ugualeMax = 5;

        var i = 0;
        var j = 1;

        while (true)
        {
            if (i >= Classe?.Length || j >= Classe?.Length)
            {
                ;

                if (!(uguale.Length > ugualeMax)) return;
                Classe = uguale;
                return;
            }

            if (string.Equals(Classe?[i].ToString(), Classe?[j].ToString(), StringComparison.CurrentCultureIgnoreCase))
            {
                uguale += Classe?[i];
                i++;
                j++;
            }
            else
            {
                ;

                if (uguale.Length > ugualeMax)
                {
                    Classe = uguale;
                    return;
                }

                uguale = "";
                j++;
                i = 0;
            }
        }
    }

    private string? AggiustaNomeDoppio2(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        text = text.Trim();

        string?[] s = text.Trim().Split(' ');
        ;

        var s2 = s.Select(s3 => s3?.ToLower()).ToList();

        switch (s2.Count)
        {
            case 0:
                return "";
            case 1:
                return text;
            case 2:
            {
                if (s2[0] == s2[1])
                    text = s[0];

                return text;
            }
        }

        ;

        //s2.Count > 2
        for (var i = 0; i < s2.Count; i++)
        for (var rip = 1; rip < s2.Count; rip++)
            if (i + rip < s2.Count)
            {
                var uguali = FindSeUguali(s2, i, rip);
                if (!uguali) continue;
                ;

                List<string?> r = new();
                var k = 0;
                for (; k < i + rip; k++) r.Add(s[k]);
                k += rip;
                for (; k < s2.Count; k++) r.Add(s[k]);

                var text2 = r.Aggregate("", (current, t) => current + t + " ");

                text2 = text2.Trim();

                return AggiustaNomeDoppio2(text2);
            }

        return text;
    }

    private static bool FindSeUguali(IReadOnlyList<string?>? s2, int i, int rip)
    {
        if (i >= s2?.Count || rip >= s2?.Count) return false;

        for (var k = 0; k < rip; k++)
        {
            var l1 = k + i;
            var l2 = k + i + rip;
            if (l1 < s2?.Count && l2 < s2.Count)
            {
                if (s2[l2] != s2[l1])
                    return false;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public bool? CheckSeIlLinkVa(bool saltaQuelliGiaFunzionanti, EventoConLog? eventoConLog)
    {
        switch (Platform)
        {
            case "TG":
            {
                LinkFunzionante = CheckSeIlLinkVa3_Telegram(saltaQuelliGiaFunzionanti, 2, 2, eventoConLog);
                return LinkFunzionante;
            }
        }

        return null;
    }

    private bool? CheckSeIlLinkVa3_Telegram(bool saltaQuelliGiaFunzionanti, int tentativi1 = 2, int tentativi2 = 2,
        EventoConLog? eventoConLog = null)
    {
        if (saltaQuelliGiaFunzionanti)
            if (LinkFunzionante == true)
                return true;

        bool? works = null;
        var link = GetLink();
        for (var i = 0; i < tentativi1; i++)
        {
            works = CheckSeIlLinkVa2_Telegram(link, tentativi2, eventoConLog);
            if (works != null && works.Value)
                return true;
        }

        return works;
    }

    public static bool? CheckSeIlLinkVa2_Telegram(string? link, int tentativi = 2, EventoConLog? eventoConLog = null)
    {
        string? content = null;
        var i = 0;
        while (i <= tentativi && string.IsNullOrEmpty(content))
        {
            content = Download(link, eventoConLog);


            i++;
        }

        return string.IsNullOrEmpty(content) ? null : content.Contains("tgme_page_title");
    }

    public static string? Download(string? uri, EventoConLog? eventoConLog)
    {
        try
        {
            if (uri != null)
            {
                var htmlCode = clientDownload.DownloadString(uri);
                return htmlCode;
            }
        }
        catch (Exception e)
        {
            var s = "Exception in download: link " + uri + "\n" + e;
            Console.WriteLine(s);
            eventoConLog?.Log(s);
        }

        return null;
    }
}