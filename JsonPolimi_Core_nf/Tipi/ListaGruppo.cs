using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Forms;
using JsonPolimi_Core_nf.Enums;
using static System.String;

namespace JsonPolimi_Core_nf.Tipi;

public class ListaGruppo : IEnumerable
{
    private readonly List<Gruppo> _l;

    public ListaGruppo()
    {
        _l = new List<Gruppo>();
    }

    public IEnumerator GetEnumerator()
    {
        return _l.GetEnumerator();
    }

    public List<Gruppo> GetGroups()
    {
        return _l;
    }

    public int GetCount()
    {
        return _l.Count;
    }

    public Gruppo GetElem(int i)
    {
        return _l[i];
    }

    public void Add(Gruppo g, bool merge)
    {
        if (merge)
        {
            var (item1, item2) = Contiene(g);

            if (!item1)
            {
                _l.Add(g);
                return;
            }

            Merge(item2, g);
        }
        else
        {
            _l.Add(g);
        }
    }

    private void Merge(int i, Gruppo g)
    {
        _l[i].Merge(g);
    }

    private Tuple<bool, int> Contiene(Gruppo g)
    {
        for (var i = 0; i < _l.Count; i++)
        {
            if (!IsNullOrEmpty(_l[i].PermanentId) && !IsNullOrEmpty(g.PermanentId))
                if (_l[i].PermanentId == g.PermanentId && _l[i].Platform == g.Platform)
                    return new Tuple<bool, int>(true, i);

            if (_l[i].Id == g.Id)
                return new Tuple<bool, int>(true, i);

            var bt = _l[i].Platform == "TG" && g.Platform == "TG" && _l[i].Classe == g.Classe;
            bt &= !IsNullOrEmpty(_l[i].PermanentId) || !IsNullOrEmpty(g.PermanentId);
            var bt2 = IsNullOrEmpty(_l[i].Year) || IsNullOrEmpty(g.Year);

            if (!IsNullOrEmpty(_l[i].Year) && !IsNullOrEmpty(g.Year))
                if (_l[i].Year != g.Year)
                    bt2 = false;

            if (bt && bt2)
                return new Tuple<bool, int>(true, i);
        }

        return new Tuple<bool, int>(false, 0);
    }

    public void Remove(int i)
    {
        _l.RemoveAt(i);
    }

    public void SetElem(int i, Gruppo elem)
    {
        _l[i] = elem;
    }

    public void Sort()
    {
        IComparer<Gruppo> cc = new CoordinatesBasedComparer();
        _l.Sort(cc.Compare);
    }

    private static int CompareOrdinal2(ListaStringhePerJSON office1, ListaStringhePerJSON office2)
    {
        switch (office1)
        {
            case null when office2 == null:
                return 0;
            case null:
                return -1;
        }

        if (office2 == null)
            return +1;

        return office1.StringNotNull() == office2.StringNotNull() ? 0 : CompareOrdinal23(office1.o, office2.o);
    }

    private static int CompareOrdinal23(List<string> office1, List<string> office2)
    {
        if (office1 == null && office2 == null)
            return 0;

        if (office1 == null)
            return -1;

        if (office2 == null)
            return 1;

        office1.Sort();
        office2.Sort();

        if (office1.Count == office2.Count)
            return office1.Select((t, i) => CompareOrdinal(t.ToLower(), office2[i].ToLower()))
                .FirstOrDefault(i1 => i1 != 0);

        if (office1.Count > office2.Count)
        {
            for (var i = 0; i < office2.Count; i++)
            {
                var i1 = CompareOrdinal(office1[i], office2[i]);
                if (i1 != 0)
                    return i1;
            }

            return 1;
        }

        for (var i = 0; i < office1.Count; i++)
        {
            var i1 = CompareOrdinal(office1[i], office2[i]);
            if (i1 != 0)
                return i1;
        }

        return -1;
    }

    public void MergeUnione(decimal v1, decimal v2)
    {
        Merge((int)v1, GetElem((int)v2));
        _l.RemoveAt((int)v2);
    }

    public void MergeLink(int v1, int v2)
    {
        _l[v1].IdLink = _l[v2].IdLink;

        switch (_l[v1].LastUpdateInviteLinkTime)
        {
            case null when _l[v2].LastUpdateInviteLinkTime == null:
                _l[v1].LastUpdateInviteLinkTime = _l[v2].LastUpdateInviteLinkTime;
                break;

            case null:
                _l[v1].LastUpdateInviteLinkTime = _l[v2].LastUpdateInviteLinkTime;
                break;

            default:
            {
                if (_l[v2].LastUpdateInviteLinkTime != null)
                {
                    var r = DateTime.Compare(_l[v1].LastUpdateInviteLinkTime.Value,
                        _l[v2].LastUpdateInviteLinkTime.Value);
                    if (r < 0) _l[v1].LastUpdateInviteLinkTime = _l[v2].LastUpdateInviteLinkTime;
                }

                break;
            }
        }

        _l[v1].LastUpdateInviteLinkTime ??= DateTime.Now;

        _l[v1].Aggiusta(true, true);
    }

    public void ProvaAdUnire()
    {
        for (var i = 0; i < _l.Count; i++)
        {
            _l[i].Aggiusta(true, true);
            for (var j = _l.Count - 1; j >= 0; j--)
            {
                if (i == j) continue;
                var r = Equivalenti(i, j, true);

                var doThat = false;
                switch (r.Item1.somiglianzaEnum)
                {
                    case SomiglianzaEnum.IDENTITICI:
                    case SomiglianzaEnum.DUBBIO:
                        doThat = true;
                        break;
                }

                if (r.Item2 == null) doThat = false;

                if (!doThat) continue;
                if (i > j)
                {
                    _l.RemoveAt(i);
                    _l.RemoveAt(j);
                }
                else
                {
                    _l.RemoveAt(j);
                    _l.RemoveAt(i);
                }

                _l.Add(r.Item2);

                i--;
                j++;

                if (i < 0)
                    i = 0;

                if (j > _l.Count - 1)
                    j = _l.Count - 1;
            }
        }

        ;
    }

    private Tuple<SomiglianzaClasse, Gruppo> Equivalenti(int i, int j, bool aggiustaAnno)
    {
        return Equivalenti2(i, new Tuple<Gruppo, int>(_l[j], j), aggiustaAnno);
    }

    private Tuple<SomiglianzaClasse, Gruppo> Equivalenti2(int i, Tuple<Gruppo, int> j, bool aggiustaAnnno)
    {
        var a1 = _l[i];
        var a2 = j.Item1;

        if (a1.Platform != a2.Platform)
            return new Tuple<SomiglianzaClasse, Gruppo>(new SomiglianzaClasse(SomiglianzaEnum.DIVERSI), null);

        if (a1.Platform == a2.Platform)
        {
            var c1 = a1.Classe.ToLower().Trim();
            var c2 = a2.Classe.ToLower().Trim();
            if (c1 == c2)
                if ((JsonEmpty(a1.Office) && !JsonEmpty(a2.Office)) || (!JsonEmpty(a1.Office) && JsonEmpty(a2.Office)))
                    if ((a1.Tipo == "C" && a2.Tipo == "S") || (a1.Tipo == "S" && a2.Tipo == "C"))
                    {
                        var r7 = Unisci4(i, j, aggiustaAnnno);
                        r7.Item2.Tipo = "C";
                        return new Tuple<SomiglianzaClasse, Gruppo>(
                            new SomiglianzaClasse(SomiglianzaEnum.IDENTITICI, a1, a2), r7.Item2);
                    }
        }

        if (a1.IdLink != a2.IdLink && a1.LinkFunzionante == true && a2.LinkFunzionante == true)
            return new Tuple<SomiglianzaClasse, Gruppo>(new SomiglianzaClasse(SomiglianzaEnum.DIVERSI), null);

        var eq = Equivalenti3(a1, a2);
        if (eq.somiglianzaEnum != SomiglianzaEnum.IDENTITICI && eq.somiglianzaEnum != SomiglianzaEnum.DUBBIO)
            return new Tuple<SomiglianzaClasse, Gruppo>(new SomiglianzaClasse(SomiglianzaEnum.DIVERSI), null);

        var somiglianzaEnum2 = Equivalenti6(a1, a2, eq);
        switch (somiglianzaEnum2)
        {
            case SomiglianzaEnum.IDENTITICI:
            {
                ;
                break;
            }
            case SomiglianzaEnum.DIVERSI:
            {
                eq.somiglianzaEnum = SomiglianzaEnum.DIVERSI;
                return new Tuple<SomiglianzaClasse, Gruppo>(eq, null);
            }
            case SomiglianzaEnum.DUBBIO:
            {
                ;

                var somiglianzaEnum3 = SciogliDubbio(a1, a2);
                switch (somiglianzaEnum3)
                {
                    case SomiglianzaEnum.IDENTITICI:
                    {
                        ; //todo
                        break;
                    }
                    case SomiglianzaEnum.DIVERSI:
                    {
                        return new Tuple<SomiglianzaClasse, Gruppo>(eq, null);
                    }
                    case SomiglianzaEnum.DUBBIO:
                    {
                        ;

                        if (a1.Id == a2.Id)
                        {
                            var r7 = Unisci4(i, j, aggiustaAnnno);
                            eq.somiglianzaEnum = SomiglianzaEnum.IDENTITICI;
                            return new Tuple<SomiglianzaClasse, Gruppo>(eq, r7.Item2);
                        }

                        ;

                        break;
                    }
                }

                break;
            }
        }

        var r2 = Unisci4(i, j, aggiustaAnnno);
        if (r2.Item1 == false) return new Tuple<SomiglianzaClasse, Gruppo>(eq, null);

        ;

        return new Tuple<SomiglianzaClasse, Gruppo>(eq, r2.Item2);
    }

    private static bool JsonEmpty(ListaStringhePerJSON office)
    {
        if (office == null)
            return true;

        return office.IsEmpty();
    }

    private static SomiglianzaEnum SciogliDubbio(Gruppo a1, Gruppo a2)
    {
        ;

        var s1 = a1.Classe?.ToLower();
        var s2 = a2.Classe?.ToLower();

        if (IsNullOrEmpty(s1) && IsNullOrEmpty(s2))
            return SomiglianzaEnum.DUBBIO;

        if (IsNullOrEmpty(s1))
            return SomiglianzaEnum.DIVERSI;

        if (IsNullOrEmpty(s2))
            return SomiglianzaEnum.DIVERSI;

        if (s1 != s2) return SomiglianzaEnum.DUBBIO;

        if (!IsNullOrEmpty(a1.Platform) && !IsNullOrEmpty(a2.Platform) && a1.Platform != a2.Platform)
            return SomiglianzaEnum.DIVERSI;

        //	a1	{{"class":"Information Theory","office":null,"id":"TG///LclXl1g8Xq-OwrFr4JbInA///","degree":null,"school":null,"annocorso":null,"nomecorso":null,"idcorso":null,"pianostudi":null,"id_link":"LclXl1g8Xq-OwrFr4JbInA","language":"ENG","type":"C","year":null,"ccs":null,"permanentId":"-1001480351407","LastUpdateInviteLinkTime":"2020-03-09 14:10:18.000","platform":"TG"} JsonPolimi.Gruppo}	JsonPolimi.Gruppo
        //  a2	{{"class":"INFORMATION THEORY","office":"Leonardo","id":"TG//Leonardo//054322/474/Z2E - MICROWAVES AND PHOTONICS","degree":null,"school":null,"annocorso":"2","nomecorso":null,"idcorso":"054322","pianostudi":"Z2E - MICROWAVES AND PHOTONICS","id_link":null,"language":"EN","type":"C","year":null,"ccs":"(474), Telecommunication Engineering - Ingegneria delle Telecomunicazioni","permanentId":null,"LastUpdateInviteLinkTime":null,"platform":"TG"} JsonPolimi.Gruppo}	JsonPolimi.Gruppo

        if (!IsNullOrEmpty(a1.PermanentId) && !IsNullOrEmpty(a2.PermanentId) && a1.PermanentId != a2.PermanentId)
            return SomiglianzaEnum.DIVERSI;

        if (!Gruppo.IsEmpty(a1.Office) && !Gruppo.IsEmpty(a2.Office) && Gruppo.Confronta(a1.Office, a2.Office) != 0)
            return SomiglianzaEnum.DIVERSI;

        if (!IsNullOrEmpty(a1.PianoDiStudi) && !IsNullOrEmpty(a2.PianoDiStudi) && a1.PianoDiStudi != a2.PianoDiStudi)
            return SomiglianzaEnum.DIVERSI;

        if (!IsNullOrEmpty(a1.Tipo) && !IsNullOrEmpty(a2.Tipo) && a1.Tipo != a2.Tipo) return SomiglianzaEnum.DIVERSI;

        if (!IsNullOrEmpty(a1.Tipo) && !IsNullOrEmpty(a2.Tipo) && a1.Tipo != a2.Tipo) return SomiglianzaEnum.DIVERSI;

        if (!IsNullOrEmpty(a1.School) && !IsNullOrEmpty(a2.School) && a1.School != a2.School)
            return SomiglianzaEnum.DIVERSI;

        if (!ListaStringhePerJSON.IsEmpty(a1.CCS) && !ListaStringhePerJSON.IsEmpty(a2.CCS) &&
            ListaStringhePerJSON.Confronta(a1.CCS, a2.CCS) != 0) return SomiglianzaEnum.DIVERSI;

        return SomiglianzaEnum.DUBBIO;
    }

    private static SomiglianzaEnum Equivalenti6(Gruppo a1, Gruppo a2, SomiglianzaClasse somiglianzaEnumOld)
    {
        ;

        if (!IsNullOrEmpty(a1.Classe) && !IsNullOrEmpty(a2.Classe))
        {
            var a1_cl = a1.Classe.ToLower();
            var a2_cl = a2.Classe.ToLower();

            if (a1_cl.Contains("data") && a2_cl.Contains("data"))
            {
                if (a1_cl.Contains("bip") && !a2_cl.Contains("bip"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("bip") && !a1_cl.Contains("bip"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("economia") && a2_cl.Contains("economia"))
            {
                if (a1_cl.Contains("aerospaziale") && !a2_cl.Contains("aerospaziale"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("aerospaziale") && !a1_cl.Contains("aerospaziale"))
                    return SomiglianzaEnum.DIVERSI;
            }

            var checkToExit = CheckIfToExit(a1, a2);
            if (checkToExit) return SomiglianzaEnum.DIVERSI;

            if (a1_cl.Contains("theory") && a2_cl.Contains("theory"))
            {
                if (a1_cl.Contains("traffic") && !a2_cl.Contains("traffic"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("traffic") && !a1_cl.Contains("traffic"))
                    return SomiglianzaEnum.DIVERSI;

                if (a1_cl.Contains("information") && !a2_cl.Contains("information"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("information") && !a1_cl.Contains("information"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("information") && a2_cl.Contains("information"))
            {
                if (a1_cl.Contains("system") && !a2_cl.Contains("system"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("system") && !a1_cl.Contains("system"))
                    return SomiglianzaEnum.DIVERSI;

                if (a1_cl.Contains("theory") && !a2_cl.Contains("theory"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("theory") && !a1_cl.Contains("theory"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl == a2_cl) return somiglianzaEnumOld.somiglianzaEnum;

            if (a1_cl.Contains("mobility") && a2_cl.Contains("mobility"))
            {
                if (a1_cl.Contains("systems") && !a2_cl.Contains("systems"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("systems") && !a1_cl.Contains("systems"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (!IsNullOrEmpty(a1.IdLink) && !IsNullOrEmpty(a2.IdLink) && a1.IdLink != a2.IdLink)
                return SomiglianzaEnum.DIVERSI;

            if ((a1_cl.Contains("tecnologie") && a2_cl.Contains("workshop")) ||
                (a2_cl.Contains("tecnologie") && a1_cl.Contains("workshop")))
            {
                if (a1_cl.Contains("digitali") && !a2_cl.Contains("digitali"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("digitali") && !a1_cl.Contains("digitali"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("tecnologie") || a2_cl.Contains("tecnologie"))
            {
                if (a1_cl.Contains("software") && !a2_cl.Contains("software"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("software") && !a1_cl.Contains("software"))
                    return SomiglianzaEnum.DIVERSI;

                if (a1_cl.Contains("idraulica") && !a2_cl.Contains("idraulica"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("idraulica") && !a1_cl.Contains("idraulica"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("computing") && a2_cl.Contains("computing"))
            {
                if (a1_cl.Contains("parallel") && !a2_cl.Contains("parallel"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("parallel") && !a1_cl.Contains("parallel"))
                    return SomiglianzaEnum.DIVERSI;

                if (a1_cl.Contains("systems") && !a2_cl.Contains("systems"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("systems") && !a1_cl.Contains("systems"))
                    return SomiglianzaEnum.DIVERSI;

                if (a1_cl.Contains("genomic") && !a2_cl.Contains("genomic"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("genomic") && !a1_cl.Contains("genomic"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("progetto") && a2_cl.Contains("progetto"))
            {
                if (a1_cl.Contains("ambiente") && !a2_cl.Contains("ambiente"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("ambiente") && !a1_cl.Contains("ambiente"))
                    return SomiglianzaEnum.DIVERSI;

                if (a1_cl.Contains("api") && !a2_cl.Contains("api"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("api") && !a1_cl.Contains("api"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.EndsWith(" a") && a2_cl.EndsWith(" a"))
            {
                if (a1_cl.Contains("informatica") && !a2_cl.Contains("informatica"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("informatica") && !a1_cl.Contains("informatica"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("automazione") && a2_cl.Contains("automazione"))
            {
                if (a1_cl.Contains("processi") && !a2_cl.Contains("processi"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("processi") && !a1_cl.Contains("processi"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("systems") && a2_cl.Contains("systems"))
            {
                if (a1_cl.Contains("robotic") && !a2_cl.Contains("robotic"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("robotic") && !a1_cl.Contains("robotic"))
                    return SomiglianzaEnum.DIVERSI;

                if (a1_cl.Contains("advanced") && !a2_cl.Contains("advanced"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("advanced") && !a1_cl.Contains("advanced"))
                    return SomiglianzaEnum.DIVERSI;

                if (a1_cl.Contains("computing") && !a2_cl.Contains("computing"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("computing") && !a1_cl.Contains("computing"))
                    return SomiglianzaEnum.DIVERSI;

                if (a1_cl.Contains("control") && !a2_cl.Contains("control"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("control") && !a1_cl.Contains("control"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("dispositivi") && a2_cl.Contains("dispositivi"))
            {
                if (a1_cl.Contains("biomateriali") && !a2_cl.Contains("biomateriali"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("biomateriali") && !a1_cl.Contains("biomateriali"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("economics") && a2_cl.Contains("economics"))
            {
                if (a1_cl.Contains("business") && !a2_cl.Contains("business"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("business") && !a1_cl.Contains("business"))
                    return SomiglianzaEnum.DIVERSI;

                if (a1_cl.Contains("internet") && !a2_cl.Contains("internet"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("internet") && !a1_cl.Contains("internet"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("business") && a2_cl.Contains("business"))
            {
                if (a1_cl.Contains("t2d") && !a2_cl.Contains("t2d"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("t2d") && !a1_cl.Contains("t2d"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("networks") && a2_cl.Contains("networks"))
            {
                if (a1_cl.Contains("transport") && !a2_cl.Contains("transport"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("transport") && !a1_cl.Contains("transport"))
                    return SomiglianzaEnum.DIVERSI;

                if (a1_cl.Contains("radio") && !a2_cl.Contains("radio"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("radio") && !a1_cl.Contains("radio"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("processing") && a2_cl.Contains("processing"))
            {
                if (a1_cl.Contains("big") && !a2_cl.Contains("big"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("big") && !a1_cl.Contains("big"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("data") && a2_cl.Contains("data"))
            {
                if (a1_cl.Contains("quality") && !a2_cl.Contains("quality"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("quality") && !a1_cl.Contains("quality"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("quality") && a2_cl.Contains("quality"))
            {
                if (a1_cl.Contains("data") && !a2_cl.Contains("data"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("data") && !a1_cl.Contains("data"))
                    return SomiglianzaEnum.DIVERSI;

                if (a1_cl.Contains("information") && !a2_cl.Contains("information"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("information") && !a1_cl.Contains("information"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("economia") && a2_cl.Contains("economia"))
            {
                if (a1_cl.Contains("ambientale") && !a2_cl.Contains("ambientale"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("ambientale") && !a1_cl.Contains("ambientale"))
                    return SomiglianzaEnum.DIVERSI;

                if (a1_cl.Contains("trasporti") && !a2_cl.Contains("trasporti"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("trasporti") && !a1_cl.Contains("trasporti"))
                    return SomiglianzaEnum.DIVERSI;

                if (a1_cl.Contains("aerospaziali") && !a2_cl.Contains("aerospaziali"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("aerospaziali") && !a1_cl.Contains("aerospaziali"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.EndsWith(" d") && !a2_cl.EndsWith(" d"))
                return SomiglianzaEnum.DIVERSI;

            if (a2_cl.EndsWith(" d") && !a1_cl.EndsWith(" d"))
                return SomiglianzaEnum.DIVERSI;

            if (a1_cl.Contains("systems") && a2_cl.Contains("systems"))
            {
                if (a1_cl.Contains("intelligent") && !a2_cl.Contains("intelligent"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("intelligent") && !a1_cl.Contains("intelligent"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("reti") && a2_cl.Contains("reti"))
            {
                if (a1_cl.Contains("informatica") && !a2_cl.Contains("informatica"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("informatica") && !a1_cl.Contains("informatica"))
                    return SomiglianzaEnum.DIVERSI;

                if (a1_cl.Contains("monitoraggio") && !a2_cl.Contains("monitoraggio"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("monitoraggio") && !a1_cl.Contains("monitoraggio"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("calcolatori") && a2_cl.Contains("calcolatori"))
            {
                if (a1_cl.Contains("reti") && !a2_cl.Contains("reti"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("reti") && !a1_cl.Contains("reti"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("elettrotecnica") && a2_cl.Contains("elettrotecnica"))
            {
                if (a1_cl.Contains("automazione") && !a2_cl.Contains("automazione"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("automazione") && !a1_cl.Contains("automazione"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (!Gruppo.IsEmpty(a1.Office) && !Gruppo.IsEmpty(a2.Office) && Gruppo.Confronta(a1.Office, a2.Office) != 0)
                return SomiglianzaEnum.DIVERSI;

            if (a1_cl.Contains("food") && a2_cl.Contains("food"))
            {
                if (a1_cl.Contains("security") && !a2_cl.Contains("security"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("security") && !a1_cl.Contains("security"))
                    return SomiglianzaEnum.DIVERSI;

                if (a1_cl.Contains("trends") && !a2_cl.Contains("trends"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("trends") && !a1_cl.Contains("trends"))
                    return SomiglianzaEnum.DIVERSI;

                if (a1_cl.Contains("process") && !a2_cl.Contains("process"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("process") && !a1_cl.Contains("process"))
                    return SomiglianzaEnum.DIVERSI;

                if (a1_cl.Contains("engineering") && !a2_cl.Contains("engineering"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("engineering") && !a1_cl.Contains("engineering"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("urbanistica") && a2_cl.Contains("urbanistica"))
            {
                if (a1_cl.Contains("polimi") && !a2_cl.Contains("polimi"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("polimi") && !a1_cl.Contains("polimi"))
                    return SomiglianzaEnum.DIVERSI;
            }

            if (a1_cl.Contains("test") && a2_cl.Contains("test"))
            {
                if (a1_cl.Contains("gruppo") && !a2_cl.Contains("gruppo"))
                    return SomiglianzaEnum.DIVERSI;
                if (a2_cl.Contains("gruppo") && !a1_cl.Contains("gruppo"))
                    return SomiglianzaEnum.DIVERSI;
            }

            ;
        }

        return somiglianzaEnumOld.somiglianzaEnum;
    }

    private static SomiglianzaClasse Equivalenti3(Gruppo a1, Gruppo a2)
    {
        var r1 = Equivalenti5(a1, a2);

        if (r1.somiglianzaEnum == SomiglianzaEnum.IDENTITICI)
        {
            if (IsNullOrEmpty(a1.IDCorsoPolimi) && !IsNullOrEmpty(a2.IDCorsoPolimi))
                r1.somiglianzaEnum = SomiglianzaEnum.DUBBIO;
            else if (!IsNullOrEmpty(a1.IDCorsoPolimi) && IsNullOrEmpty(a2.IDCorsoPolimi))
                r1.somiglianzaEnum = SomiglianzaEnum.DUBBIO;
        }

        if (r1.somiglianzaEnum != SomiglianzaEnum.DUBBIO) return r1;
        if (!IsNullOrEmpty(a1.Tipo) && !IsNullOrEmpty(a2.Tipo) &&
            !string.Equals(a1.Tipo, a2.Tipo, StringComparison.CurrentCultureIgnoreCase))
            r1.somiglianzaEnum = SomiglianzaEnum.DIVERSI;

        return r1;
    }

    private static SomiglianzaClasse Equivalenti5(Gruppo a1, Gruppo a2)
    {
        if (!IsNullOrEmpty(a1.IDCorsoPolimi) && !IsNullOrEmpty(a2.IDCorsoPolimi) &&
            a1.IDCorsoPolimi == a2.IDCorsoPolimi)
        {
            var i1 = CompareOrdinal2(a1.CCS, a2.CCS);
            if (i1 != 0) return new SomiglianzaClasse(SomiglianzaEnum.DIVERSI, a1, a2);
            var i2 = CompareOrdinal(a1.PianoDiStudi, a2.PianoDiStudi);
            return i2 == 0
                ? new SomiglianzaClasse(SomiglianzaEnum.IDENTITICI)
                : new SomiglianzaClasse(SomiglianzaEnum.DIVERSI, a1, a2);
        }

        if (!IsNullOrEmpty(a1.IDCorsoPolimi) && !IsNullOrEmpty(a2.IDCorsoPolimi) &&
            a1.IDCorsoPolimi != a2.IDCorsoPolimi)
            return new SomiglianzaClasse(SomiglianzaEnum.DIVERSI);

        if (a1.PermanentId == a2.PermanentId && !IsNullOrEmpty(a1.PermanentId))
            return new SomiglianzaClasse(SomiglianzaEnum.IDENTITICI);

        if (!IsNullOrEmpty(a1.PermanentId))
            if (!IsNullOrEmpty(a2.PermanentId))
                if (a1.PermanentId != a2.PermanentId)
                    return new SomiglianzaClasse(SomiglianzaEnum.DIVERSI);

        if (a1.Id == a2.Id && !IsNullOrEmpty(a1.Id))
        {
            if (a1.Classe.ToLower().Contains("cartografia") && !a2.Classe.ToLower().Contains("cartografia"))
                return new SomiglianzaClasse(SomiglianzaEnum.DIVERSI, a1, a2);
            if (a2.Classe.ToLower().Contains("cartografia") && !a1.Classe.ToLower().Contains("cartografia"))
                return new SomiglianzaClasse(SomiglianzaEnum.DIVERSI, a1, a2);

            return new SomiglianzaClasse(SomiglianzaEnum.DUBBIO, a1, a2);
        }

        if (!IsNullOrEmpty(a1.Year))
            if (!IsNullOrEmpty(a2.Year))
                if (a1.Year != a2.Year)
                    return new SomiglianzaClasse(SomiglianzaEnum.DIVERSI);

        if (!IsNullOrEmpty(a1.Platform))
            if (!IsNullOrEmpty(a2.Platform))
                if (a1.Platform != a2.Platform)
                    return new SomiglianzaClasse(SomiglianzaEnum.DIVERSI);

        if (!Gruppo.IsEmpty(a1.Office))
            if (!Gruppo.IsEmpty(a2.Office))
            {
                var i1 = CompareOrdinal2(a1.Office, a2.Office);
                if (i1 != 0)
                    return new SomiglianzaClasse(SomiglianzaEnum.DIVERSI, a1, a2);
            }

        if (!IsNullOrEmpty(a1.Degree))
            if (!IsNullOrEmpty(a2.Degree))
                if (a1.Degree != a2.Degree)
                    return new SomiglianzaClasse(SomiglianzaEnum.DIVERSI);

        if (IsNullOrEmpty(a1.Classe))
            return new SomiglianzaClasse(SomiglianzaEnum.DUBBIO, a1, a2);

        if (IsNullOrEmpty(a2.Classe))
            return new SomiglianzaClasse(SomiglianzaEnum.DUBBIO, a1, a2);

        var s1 = a1.Classe.ToLower().Split(' ');
        var s2 = a2.Classe.ToLower().Split(' ');
        var sa1 = new List<string>();
        var sa2 = new List<string>();
        sa1.AddRange(s1);
        sa2.AddRange(s2);
        if (sa1.Contains("magistrale"))
        {
            if (IsNullOrEmpty(a2.Degree))
                return new SomiglianzaClasse(SomiglianzaEnum.DIVERSI);
            if (a2.Degree.ToLower() != "lm")
                return new SomiglianzaClasse(SomiglianzaEnum.DIVERSI);
        }
        else if (sa1.Contains("triennale"))
        {
            if (IsNullOrEmpty(a2.Degree))
                return new SomiglianzaClasse(SomiglianzaEnum.DIVERSI);
            if (a2.Degree.ToLower() != "lt")
                return new SomiglianzaClasse(SomiglianzaEnum.DIVERSI);
        }
        else if (sa2.Contains("magistrale"))
        {
            if (IsNullOrEmpty(a1.Degree))
                return new SomiglianzaClasse(SomiglianzaEnum.DIVERSI);
            if (a1.Degree.ToLower() != "lm")
                return new SomiglianzaClasse(SomiglianzaEnum.DIVERSI);
        }
        else if (sa2.Contains("triennale"))
        {
            if (IsNullOrEmpty(a1.Degree))
                return new SomiglianzaClasse(SomiglianzaEnum.DIVERSI);
            if (a1.Degree.ToLower() != "lt")
                return new SomiglianzaClasse(SomiglianzaEnum.DIVERSI);
        }

        var b1 = NomiSimili(a1.Classe, a2.Classe);

        if (b1 == SomiglianzaEnum.DUBBIO) return new SomiglianzaClasse(SomiglianzaEnum.DUBBIO, a1, a2);

        if (b1 != SomiglianzaEnum.IDENTITICI)
            return new SomiglianzaClasse(SomiglianzaEnum.DIVERSI);
        if (!IsNullOrEmpty(a1.IdLink) && IsNullOrEmpty(a2.IdLink) &&
            IsNullOrEmpty(a1.IDCorsoPolimi) && !IsNullOrEmpty(a2.IDCorsoPolimi))
            return new SomiglianzaClasse(SomiglianzaEnum.DUBBIO, a1, a2);

        return new SomiglianzaClasse(SomiglianzaEnum.IDENTITICI);
    }

    private static SomiglianzaEnum NomiSimili(string n1, string n2)
    {
        if (n1.Length == 0 || n2.Length == 0)
            return SomiglianzaEnum.DIVERSI;

        if (n1 == n2)
            return SomiglianzaEnum.IDENTITICI;

        if (string.Equals(n1, n2, StringComparison.CurrentCultureIgnoreCase))
            return SomiglianzaEnum.DUBBIO;

        try
        {
            var s1 = n1.Split(' ');
            var s2 = n2.Split(' ');

            var l1 = new List<string>();
            var l2 = new List<string>();

            l1.AddRange(s1);
            l2.AddRange(s2);

            //lower words
            for (var i = 0; i < l1.Count; i++) l1[i] = l1[i].ToLower();
            for (var i = 0; i < l2.Count; i++) l2[i] = l2[i].ToLower();

            TryRename(ref l1, ref l2);

            //remove duplicate
            var la1 = new List<string>();
            var la2 = new List<string>();
            foreach (var t in l1.Where(t => !la1.Contains(t)))
                la1.Add(t);

            foreach (var t in l2.Where(t => !la2.Contains(t)))
                la2.Add(t);

            l1 = la1;
            l2 = la2;

            var noMerge = new List<string> { "analisi", "vehicles", "440" };
            if (noMerge.Any(noMergeS => l1.Contains(noMergeS) || l2.Contains(noMergeS))) return SomiglianzaEnum.DIVERSI;

            var noMerge2 = new List<Tuple<string, string>>
            {
                new("chimica", "elettrica")
            };

            foreach (var noMergeS2 in noMerge2)
            {
                if (l1.Contains(noMergeS2.Item1) && l1.Contains(noMergeS2.Item2))
                    return SomiglianzaEnum.DIVERSI;
                if (l2.Contains(noMergeS2.Item1) && l2.Contains(noMergeS2.Item2))
                    return SomiglianzaEnum.DIVERSI;
            }

            //remove useless words
            var useless = new List<string>
            {
                "polimi", "politecnico", "and",
                "engineering", "in",
                "generale", "gruppo", "for", "the",
                "e", "delle", "dei",
                "ingegneria", "della", "-", "", "per", "le"
            };
            TryRemove(ref l1, ref l2, useless);

            var r2 = ManualCheck(n1, n2);
            if (r2 != null) return r2.Value ? SomiglianzaEnum.IDENTITICI : SomiglianzaEnum.DIVERSI;

            //count how many words are in common
            var quanti = new List<string>();
            for (var i = 0; i < l1.Count; i++)
                if (l2.Contains(l1[i]))
                    quanti.Add(l1[i]);

            var minimo = 0;
            if (l1.Count + l2.Count > 14)
                minimo = 5;
            else if (l1.Count + l2.Count > 11)
                minimo = 4;
            else if (l1.Count + l2.Count > 5)
                minimo = 2;
            else
                minimo = 1;

            if (quanti.Count == 1)
            {
                if (quanti[0] == "design")
                    return SomiglianzaEnum.DIVERSI;
                if (quanti[0] == "architettura")
                    return SomiglianzaEnum.DIVERSI;
            }

            if (quanti.Count == minimo)
                return SomiglianzaEnum.DUBBIO;
            if (quanti.Count == minimo + 1)
                return SomiglianzaEnum.DUBBIO;
            if (quanti.Count >= minimo)
                return SomiglianzaEnum.IDENTITICI;
        }
        catch
        {
            return SomiglianzaEnum.DUBBIO;
        }

        return SomiglianzaEnum.DIVERSI;
    }

    private static void TryRename(ref List<string> l1, ref List<string> l2)
    {
        var rename = new List<Tuple<string, string>>
        {
            new("informatica", "info")
        };

        foreach (var ren in rename)
        {
            for (var i = 0; i < l1.Count; i++)
                if (l1[i] == ren.Item1)
                    l1[i] = ren.Item2;

            for (var i = 0; i < l2.Count; i++)
                if (l2[i] == ren.Item1)
                    l2[i] = ren.Item2;
        }
    }

    private static bool? ManualCheck(string n1, string n2)
    {
        if (n1[^1] == ' ')
            n1 = n1.Remove(n1.Length - 1);

        if (n2[^1] == ' ')
            n2 = n2.Remove(n2.Length - 1);

        n1 = n1.ToLower();
        n2 = n2.ToLower();

        if (n1 == "civil engineering magistrale" && n2 == "civil engineering for risk mitigation")
            return false;

        if (n2 == "civil engineering magistrale" && n1 == "civil engineering for risk mitigation")
            return false;

        if (n1 == "telecomunicazioni - informatica polimi" && n2 == "ingegneria informatica")
            return false;

        if (n2 == "telecomunicazioni - informatica polimi" && n1 == "ingegneria informatica")
            return false;

        if (n1 == "computer science and engineering - ingegneria informatica" &&
            n2 == "telecomunicazioni - informatica polimi")
            return false;

        if (n2 == "computer science and engineering - ingegneria informatica" &&
            n1 == "telecomunicazioni - informatica polimi")
            return false;

        if (n1 == "design & engineering - progetto e ingegnerizzazione del prodotto industriale" &&
            n2 == "design del prodotto industriale")
            return false;

        if (n2 == "design & engineering - progetto e ingegnerizzazione del prodotto industriale" &&
            n1 == "design del prodotto industriale")
            return false;

        if (n1 == "ingegneria per l'ambiente e il territorio - environmental and land planning engineering" &&
            n2 == "urban planning and policy design - pianificazione urbana e politiche territoriali")
            return false;

        if (n2 == "ingegneria per l'ambiente e il territorio - environmental and land planning engineering" &&
            n1 == "urban planning and policy design - pianificazione urbana e politiche territoriali")
            return false;

        if (n1 == "architettura e disegno urbano - architecture and urban design" &&
            n2 == "urban planning and policy design - pianificazione urbana e politiche territoriali")
            return false;

        if (n2 == "architettura e disegno urbano - architecture and urban design" &&
            n1 == "urban planning and policy design - pianificazione urbana e politiche territoriali")
            return false;

        if (n1 == "design & engineering - progetto e ingegnerizzazione del prodotto industriale" &&
            n2 == "product service system design - design per il sistema prodotto servizio")
            return false;

        if (n2 == "design & engineering - progetto e ingegnerizzazione del prodotto industriale" &&
            n1 == "product service system design - design per il sistema prodotto servizio")
            return false;

        if (n1 == "design for the fashion system - design per il sistema moda" &&
            n2 ==
            "design & engineering - progetto e ingegnerizzazione del prodotto industriale product service system design - design per il sistema prodotto servizio")
            return false;

        if (n2 == "design for the fashion system - design per il sistema moda" &&
            n1 ==
            "design & engineering - progetto e ingegnerizzazione del prodotto industriale product service system design - design per il sistema prodotto servizio")
            return false;

        if (n1 == "integrated product design" &&
            n2 ==
            "design for the fashion system - design per il sistema moda design & engineering - progetto e ingegnerizzazione del prodotto industriale product service system design - design per il sistema prodotto servizio")
            return false;

        if (n2 == "integrated product design" &&
            n1 ==
            "design for the fashion system - design per il sistema moda design & engineering - progetto e ingegnerizzazione del prodotto industriale product service system design - design per il sistema prodotto servizio")
            return false;

        if (n1 == "design for the fashion system - design per il sistema moda" &&
            n2 == "product service system design - design per il sistema prodotto servizio")
            return false;

        if (n2 == "design for the fashion system - design per il sistema moda" &&
            n1 == "product service system design - design per il sistema prodotto servizio")
            return false;

        if (n1 == "integrated product design" && n2 ==
            "design for the fashion system - design per il sistema moda product service system design - design per il sistema prodotto servizio")
            return false;

        if (n2 == "integrated product design" && n1 ==
            "design for the fashion system - design per il sistema moda product service system design - design per il sistema prodotto servizio")
            return false;

        if (n1 == "integrated product design" &&
            n2 == "product service system design - design per il sistema prodotto servizio")
            return false;

        if (n2 == "integrated product design" &&
            n1 == "product service system design - design per il sistema prodotto servizio")
            return false;

        if (n1 ==
            "integrated product design design for the fashion system - design per il sistema moda product service system design - design per il sistema prodotto servizio" &&
            n2 == "design & engineering - progetto e ingegnerizzazione del prodotto industriale")
            return false;

        if (n2 ==
            "integrated product design design for the fashion system - design per il sistema moda product service system design - design per il sistema prodotto servizio" &&
            n1 == "design & engineering - progetto e ingegnerizzazione del prodotto industriale")
            return false;

        if (n1 == "computer science and engineering - ingegneria informatica" && n2 == "polimi info 2019/2020")
            return false;

        if (n2 == "computer science and engineering - ingegneria informatica" && n1 == "polimi info 2019/2020")
            return false;

        if (n1 == "ingegneria per l'ambiente e il territorio - polimi" &&
            n2 == "product service system design - design per il sistema prodotto servizio")
            return false;

        if (n2 == "ingegneria per l'ambiente e il territorio - polimi" &&
            n1 == "product service system design - design per il sistema prodotto servizio")
            return false;

        if (n1 == "digital and interaction design")
            if (n2.Contains("systems"))
                return false;

        if (n2 == "digital and interaction design")
            if (n1.Contains("systems"))
                return false;

        if (n1 == "economia e organiz. aziendale b - elettrica")
            return false;

        if (n2 == "economia e organiz. aziendale b - elettrica")
            return false;

        if (n1 == "fisica 1" || n2 == "fisica 1")
            return false;

        if (n1 == "fisica tecnica" || n2 == "fisica tecnica")
            return false;

        if (n1 == "fisica tecnica e macchine" || n2 == "fisica tecnica e macchine")
            return false;

        if (n1 == "fondamenti di chimica - elettrotecnica" || n2 == "fondamenti di chimica - elettrotecnica")
            return false;

        if (n1 == "fondamenti di elettronica" || n2 == "fondamenti di elettronica")
            return false;

        if (n1 == "fondamenti di informatica" || n2 == "fondamenti di comunicazioni e internet")
            return false;

        if (n1 == "fondamenti di automatica" || n2 == "fondamenti di informatica")
            return false;

        if (n1 == "communication network design" || n2 == "communication network design")
            return false;

        if (n1 == "informatica online polimi" || n2 == "informatica online polimi")
            return false;

        if (n1 == "informatica e diritto" || n2 == "informatica e diritto")
            return false;

        if (n1 == "meccanica (per ing informatica)" || n2 == "meccanica (per ing informatica)")
            return false;

        if (n1 == "philosophical issues of computer science" || n2 == "philosophical issues of computer science")
            return false;

        if (n1 == "progetto di ingegneria del software" || n2 == "progetto di ingegneria del software")
            return false;

        if (n1 == "progetto di ingegneria informatica" || n2 == "progetto di ingegneria informatica")
            return false;

        if (n1 == "sistemi informatici" || n2 == "sistemi informatici")
            return false;

        if (n1 == "informatica" || n2 == "informatica")
            return false;

        if (n1 == "theoretical computer science" || n2 == "theoretical computer science")
            return false;

        if (n1 == "algebra and mathematical logic" || n2 == "algebra and mathematical logic")
            return false;

        if (n1 == "computer ethics" || n2 == "computer ethics")
            return false;

        if (n1 == "computer graphics" || n2 == "computer graphics")
            return false;

        if (n1 == "data management for the web" || n2 == "data management for the web")
            return false;

        if (n1 == "digital project management" || n2 == "digital project management")
            return false;

        if (n1 == "progetto sistemi informativi" || n2 == "progetto sistemi informativi")
            return false;

        if (n1 == "computer security" || n2 == "computer security")
            return false;

        return null;
    }

    public List<ImportaReturn> Importa(IEnumerable<Tuple<Gruppo>> l2, bool aggiustaAnno, Chiedi chiedi2)
    {
        return l2.Select(t => t.Item1).Select((l3, i) => Importa2(new Tuple<Gruppo, int>(l3, i), aggiustaAnno, chiedi2))
            .ToList();
    }

    private ImportaReturn Importa2(Tuple<Gruppo, int> l3, bool aggiustaAnno, Chiedi chiedi2)
    {
        var simili = new List<Tuple<int, Tuple<SomiglianzaClasse, Gruppo>>>();

        var ciSonoSimiliDaChiedere = false;

        int i;
        for (i = 0; i < _l.Count; i++)
        {
            var r = Equivalenti2(i, l3, aggiustaAnno);
            var doThat = false;
            switch (r.Item1.somiglianzaEnum)
            {
                case SomiglianzaEnum.IDENTITICI:
                    doThat = true;
                    break;
                case SomiglianzaEnum.DUBBIO:
                {
                    var toShow = true;

                    //todo: E' TEMPORANEA QUESTA COSA
                    if (r.Item1.a2 == null)
                        toShow = true;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Architettura"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Edile"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Urbanistica"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Design"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Architectural"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Edilizi"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Architecture"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Management"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Aerospaziale"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Biomedica"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Chimica"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Elettrica"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Elettronica"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Energetica"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Fisica"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Gestionale"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Aero"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Automation"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Prevenzione"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Materials"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Mathematical"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Mechanical"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Nuclear"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Space"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Civil"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Ambiente"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Matematica"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Meccanica"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Materiali"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Automazione"))
                        toShow = false;
                    else if (r.Item1.a2.CCS.Contains_In_Uno("Produzione")) toShow = false;

                    switch (chiedi2)
                    {
                        case Chiedi.SI when toShow:
                            simili.Add(new Tuple<int, Tuple<SomiglianzaClasse, Gruppo>>(i, r));
                            doThat = false;
                            ciSonoSimiliDaChiedere = true;
                            break;
                        //FINE TEMP
                        case Chiedi.SI:
                        case Chiedi.NO_DIVERSI:
                            doThat = false;
                            break;
                        case Chiedi.NO_IDENTICI:
                            doThat = true;
                            break;
                    }

                    break;
                }
            }

            if (!doThat) continue;

            Importa3(i, r);
            return new ImportaReturn(ActionDoneImport.IMPORTED);
        }

        if (ciSonoSimiliDaChiedere) return new ImportaReturn(ActionDoneImport.SIMILARITIES_FOUND, simili);

        ;
        _l.Add(l3.Item1);
        return new ImportaReturn(ActionDoneImport.ADDED);
    }

    public void Importa3(int i, Tuple<SomiglianzaClasse, Gruppo> r)
    {
        _l[i] = r.Item2;
    }

    private static void TryRemove(ref List<string> l1, ref List<string> l2, List<string> to_remove)
    {
        foreach (var s in to_remove)
        {
            try
            {
                l1.Remove(s);
            }
            catch
            {
                ;
            }

            try
            {
                l2.Remove(s);
            }
            catch
            {
                ;
            }
        }
    }

    private Tuple<bool, Gruppo> Unisci4(int i, Tuple<Gruppo, int> j, bool aggiusta_anno)
    {
        var g = Unisci2(i, j, aggiusta_anno, false);
        if (g == null)
            return new Tuple<bool, Gruppo>(false, null);

        return new Tuple<bool, Gruppo>(true, g);
    }

    private Gruppo Unisci2(int i, Tuple<Gruppo, int> j, bool aggiusta_anno, bool forced)
    {
        var a1 = _l[i];
        var a2 = j.Item1;

        if (!forced)
        {
            var toExit = CheckIfToExit(a1, a2);
            if (toExit)
                return null;
        }

        if (!IsNullOrEmpty(a1.Classe) && !IsNullOrEmpty(a2.Classe) &&
            string.Equals(a1.Classe, a2.Classe, StringComparison.CurrentCultureIgnoreCase))
        {
            //good
        }
        else
        {
            ;
        }

        ;

        if (IsNullOrEmpty(a1.Classe))
        {
            a1.Classe = a2.Classe;
        }
        else
        {
            if (!IsNullOrEmpty(a2.Classe))
            {
                var done = false;
                if (IsNullOrEmpty(a1.Year))
                {
                    if (!IsNullOrEmpty(a2.Year))
                        if (a2.Classe.Length > a1.Classe.Length)
                        {
                            a1.Classe = a2.Classe;
                            done = true;
                        }
                }
                else if (IsNullOrEmpty(a2.Year))
                {
                    if (!IsNullOrEmpty(a1.Year))
                        if (a1.Classe.Length > a2.Classe.Length)
                            done = true;
                }

                if (!done) a1.Classe = UnisciNomi(a1.Classe, a2.Classe);
            }
        }

        if (IsNullOrEmpty(a1.Degree))
            a1.Degree = a2.Degree;
        if (IsNullOrEmpty(a1.Id))
            a1.Id = a2.Id;
        if (IsNullOrEmpty(a1.IdLink))
        {
            a1.IdLink = a2.IdLink;
            a1.LinkFunzionante = a2.LinkFunzionante;
        }

        if (IsNullOrEmpty(a1.Language))
            a1.Language = a2.Language;
        if (Gruppo.IsEmpty(a1.Office))
            a1.Office = a2.Office;
        if (IsNullOrEmpty(a1.PermanentId))
            a1.PermanentId = a2.PermanentId;
        if (IsNullOrEmpty(a1.Platform))
            a1.Platform = a2.Platform;
        if (IsNullOrEmpty(a1.School))
            a1.School = a2.School;
        if (IsNullOrEmpty(a1.Tipo))
            a1.Tipo = a2.Tipo;
        if (IsNullOrEmpty(a1.Year))
            a1.Year = a2.Year;
        if (a1.LastUpdateInviteLinkTime == null)
        {
            a1.LastUpdateInviteLinkTime = a2.LastUpdateInviteLinkTime;
        }
        else
        {
            if (a2.LastUpdateInviteLinkTime != null)
                if (DateTime.Compare(a1.LastUpdateInviteLinkTime.Value, a2.LastUpdateInviteLinkTime.Value) < 0)
                    a1.LastUpdateInviteLinkTime = a2.LastUpdateInviteLinkTime;
        }

        if (a1.LinkFunzionante is false &&
            a2.LinkFunzionante is not false)
        {
            a1.IdLink = a2.IdLink;
            a1.LinkFunzionante = a2.LinkFunzionante;
        }

        if (!IsNullOrEmpty(a1.Year))
            if (!IsNullOrEmpty(a1.Tipo) && !IsNullOrEmpty(a2.Tipo))
                if (a1.Tipo.ToLower() == "s" || a2.Tipo.ToLower() == "s")
                    a1.Tipo = "S";

        a1.Aggiusta(aggiusta_anno, true);

        return a1;
    }

    private static string UnisciNomi(string classe1, string classe2)
    {
        var s1 = classe1.ToLower().Trim();
        var s2 = classe2.ToLower().Trim();
        if (s1 == s2)
            return classe1;

        return classe1.Trim() + " " + classe2.Trim();
    }

    public bool VediSeCeGiaDaURL(string url)
    {
        foreach (var i in _l)
            if (i.IdLink == url)
                return true;
        return false;
    }

    private static bool CheckIfToExit(Gruppo a1, Gruppo a2)
    {
        if (!IsNullOrEmpty(a1.Classe) && !IsNullOrEmpty(a2.Classe))
        {
            if (a1.Classe.ToLower().Contains("cazzeggio") && !a2.Classe.ToLower().Contains("cazzeggio"))
                return true;

            if (a2.Classe.ToLower().Contains("cazzeggio") && !a1.Classe.ToLower().Contains("cazzeggio"))
                return true;

            if (a1.Classe.ToLower().Contains("theory") && a2.Classe.ToLower().Contains("theory"))
            {
                if (a1.Classe.ToLower() == "information theory" &&
                    a2.Classe.ToLower() == "advanced circuit theory") return true;

                if (a2.Classe.ToLower() == "information theory" &&
                    a1.Classe.ToLower() == "advanced circuit theory") return true;
            }

            if (a1.Classe.ToLower().Contains("mobility") && a2.Classe.ToLower().Contains("mobility"))
            {
                if (a1.Classe.ToLower().Contains("safety") && !a2.Classe.ToLower().Contains("safety")) return true;
                if (a2.Classe.ToLower().Contains("safety") && !a1.Classe.ToLower().Contains("safety")) return true;

                if (a2.Classe.ToLower().Contains("data") && !a1.Classe.ToLower().Contains("data")) return true;
                if (a1.Classe.ToLower().Contains("data") && !a2.Classe.ToLower().Contains("data")) return true;
            }

            if (a1.Classe.ToLower().Contains("meccanica") && a2.Classe.ToLower().Contains("meccanica"))
            {
                if (a2.Classe.ToLower().Contains("tecnologia") && !a1.Classe.ToLower().Contains("tecnologia"))
                    return true;
                if (a1.Classe.ToLower().Contains("tecnologia") && !a2.Classe.ToLower().Contains("tecnologia"))
                    return true;
            }

            if (a1.Classe.ToLower().Contains("scaglione") && !a2.Classe.ToLower().Contains("scaglione"))
                return true;

            if (a2.Classe.ToLower().Contains("scaglione") && !a1.Classe.ToLower().Contains("scaglione"))
                return true;

            if (a1.Classe.ToLower().Contains("design") && a2.Classe.ToLower().Contains("design"))
            {
                if (a2.Classe.ToLower().Contains("accessory") && !a1.Classe.ToLower().Contains("accessory"))
                    return true;
                if (a1.Classe.ToLower().Contains("accessory") && !a2.Classe.ToLower().Contains("accessory"))
                    return true;

                if (a2.Classe.ToLower().Contains("structural") && !a1.Classe.ToLower().Contains("structural"))
                    return true;
                if (a1.Classe.ToLower().Contains("structural") && !a2.Classe.ToLower().Contains("structural"))
                    return true;
            }

            if (a1.Classe.ToLower().Contains("ambientale") && a2.Classe.ToLower().Contains("ambientale"))
            {
                if (a2.Classe.ToLower().Contains("acustica") && !a1.Classe.ToLower().Contains("acustica")) return true;
                if (a1.Classe.ToLower().Contains("acustica") && !a2.Classe.ToLower().Contains("acustica")) return true;

                if (a2.Classe.ToLower().Contains("polimi") && !a1.Classe.ToLower().Contains("polimi")) return true;
                if (a1.Classe.ToLower().Contains("polimi") && !a2.Classe.ToLower().Contains("polimi")) return true;
            }

            if (a1.Classe.ToLower().Contains("acustica") && a2.Classe.ToLower().Contains("acustica"))
            {
                if (a2.Classe.ToLower().Contains("applicata") && !a1.Classe.ToLower().Contains("applicata"))
                    return true;
                if (a1.Classe.ToLower().Contains("applicata") && !a2.Classe.ToLower().Contains("applicata"))
                    return true;

                if (a2.Classe.ToLower().Contains("illuminotecnica") &&
                    !a1.Classe.ToLower().Contains("illuminotecnica")) return true;
                if (a1.Classe.ToLower().Contains("illuminotecnica") &&
                    !a2.Classe.ToLower().Contains("illuminotecnica")) return true;
            }

            if (a1.Classe.ToLower().Contains("edifici") && a2.Classe.ToLower().Contains("edifici"))
            {
                if (a2.Classe.ToLower().Contains("caratteri") && !a1.Classe.ToLower().Contains("caratteri"))
                    return true;
                if (a1.Classe.ToLower().Contains("caratteri") && !a2.Classe.ToLower().Contains("caratteri"))
                    return true;
            }

            if (a1.Classe.ToLower().Contains("tirocinio") && a2.Classe.ToLower().Contains("tirocinio"))
            {
                if (a2.Classe.ToLower().Contains("avviamento") && !a1.Classe.ToLower().Contains("avviamento"))
                    return true;
                if (a1.Classe.ToLower().Contains("avviamento") && !a2.Classe.ToLower().Contains("avviamento"))
                    return true;
            }

            if (a1.Classe.ToLower().Contains("game") && a2.Classe.ToLower().Contains("game"))
            {
                if (a2.Classe.ToLower().Contains("business") && !a1.Classe.ToLower().Contains("business")) return true;
                if (a1.Classe.ToLower().Contains("business") && !a2.Classe.ToLower().Contains("business")) return true;
            }

            if (a1.Classe.ToLower().Contains("business") && a2.Classe.ToLower().Contains("business"))
            {
                if (a2.Classe.ToLower().Contains("game") && !a1.Classe.ToLower().Contains("game")) return true;
                if (a1.Classe.ToLower().Contains("game") && !a2.Classe.ToLower().Contains("game")) return true;
            }

            if (a1.Classe.ToLower().Contains("systems") && a2.Classe.ToLower().Contains("systems"))
            {
                if (a2.Classe.ToLower().Contains("business") && !a1.Classe.ToLower().Contains("business")) return true;
                if (a1.Classe.ToLower().Contains("business") && !a2.Classe.ToLower().Contains("business")) return true;
            }

            if (a1.Classe.ToLower().Contains("strutture") && a2.Classe.ToLower().Contains("strutture"))
            {
                if (a2.Classe.ToLower().Contains("prova") && !a1.Classe.ToLower().Contains("prova")) return true;
                if (a1.Classe.ToLower().Contains("prova") && !a2.Classe.ToLower().Contains("prova")) return true;
            }

            if (a1.Classe.ToLower().Contains("methods") && a2.Classe.ToLower().Contains("methods"))
            {
                if (a2.Classe.ToLower().Contains("random") && !a1.Classe.ToLower().Contains("random")) return true;
                if (a1.Classe.ToLower().Contains("random") && !a2.Classe.ToLower().Contains("random")) return true;
            }

            if (a1.Classe.ToLower().Contains("aziendale") && a2.Classe.ToLower().Contains("aziendale"))
            {
                if (a2.Classe.ToLower().Contains("economia") && !a1.Classe.ToLower().Contains("economia")) return true;
                if (a1.Classe.ToLower().Contains("economia") && !a2.Classe.ToLower().Contains("economia")) return true;
            }

            if (a1.Classe.ToLower().Contains("fondamenti") && a2.Classe.ToLower().Contains("fondamenti"))
            {
                if (a2.Classe.ToLower().Contains("automatica") && !a1.Classe.ToLower().Contains("automatica"))
                    return true;
                if (a1.Classe.ToLower().Contains("automatica") && !a2.Classe.ToLower().Contains("automatica"))
                    return true;

                if (a2.Classe.ToLower().Contains("internet") && !a1.Classe.ToLower().Contains("internet")) return true;
                if (a1.Classe.ToLower().Contains("internet") && !a2.Classe.ToLower().Contains("internet")) return true;

                if (a2.Classe.ToLower().Contains("segnali") && !a1.Classe.ToLower().Contains("segnali")) return true;
                if (a1.Classe.ToLower().Contains("segnali") && !a2.Classe.ToLower().Contains("segnali")) return true;
            }

            if (a1.Classe.ToLower().Contains("laboratorio") && a2.Classe.ToLower().Contains("laboratorio"))
            {
                if (a2.Classe.ToLower().Contains("finale") && !a1.Classe.ToLower().Contains("finale")) return true;
                if (a1.Classe.ToLower().Contains("finale") && !a2.Classe.ToLower().Contains("finale")) return true;

                if (a2.Classe.ToLower().Contains("fisica") && !a1.Classe.ToLower().Contains("fisica")) return true;
                if (a1.Classe.ToLower().Contains("fisica") && !a2.Classe.ToLower().Contains("fisica")) return true;
            }

            if (a1.Classe.ToLower().Contains("communication") && a2.Classe.ToLower().Contains("communication"))
            {
                if (a2.Classe.ToLower().Contains("and") && !a1.Classe.ToLower().Contains("and")) return true;
                if (a1.Classe.ToLower().Contains("and") && !a2.Classe.ToLower().Contains("and")) return true;

                if (a2.Classe.ToLower().Contains("branding") && !a1.Classe.ToLower().Contains("branding")) return true;
                if (a1.Classe.ToLower().Contains("branding") && !a2.Classe.ToLower().Contains("branding")) return true;
            }

            if (a1.Classe.ToLower().Contains("advanced") && a2.Classe.ToLower().Contains("advanced"))
            {
                if (a2.Classe.ToLower().Contains("multivariable") &&
                    !a1.Classe.ToLower().Contains("multivariable")) return true;
                if (a1.Classe.ToLower().Contains("multivariable") &&
                    !a2.Classe.ToLower().Contains("multivariable")) return true;

                if (a2.Classe.ToLower().Contains("topic") && !a1.Classe.ToLower().Contains("topic")) return true;
                if (a1.Classe.ToLower().Contains("topic") && !a2.Classe.ToLower().Contains("topic")) return true;

                if (a2.Classe.ToLower().Contains("random") && !a1.Classe.ToLower().Contains("random")) return true;
                if (a1.Classe.ToLower().Contains("random") && !a2.Classe.ToLower().Contains("random")) return true;

                if (a2.Classe.ToLower().Contains("materials") && !a1.Classe.ToLower().Contains("materials"))
                    return true;
                if (a1.Classe.ToLower().Contains("materials") && !a2.Classe.ToLower().Contains("materials"))
                    return true;

                if (a2.Classe.ToLower().Contains("circuit") && !a1.Classe.ToLower().Contains("circuit")) return true;
                if (a1.Classe.ToLower().Contains("circuit") && !a2.Classe.ToLower().Contains("circuit")) return true;

                if (a2.Classe.ToLower().Contains("computer") && !a1.Classe.ToLower().Contains("computer")) return true;
                if (a1.Classe.ToLower().Contains("computer") && !a2.Classe.ToLower().Contains("computer")) return true;
            }

            if (a1.Classe.ToLower().Contains("manufacturing") && a2.Classe.ToLower().Contains("manufacturing"))
            {
                if (a2.Classe.ToLower().Contains("advanced") && !a1.Classe.ToLower().Contains("advanced")) return true;
                if (a1.Classe.ToLower().Contains("advanced") && !a2.Classe.ToLower().Contains("advanced")) return true;

                if (a2.Classe.ToLower().Contains("space") && !a1.Classe.ToLower().Contains("space")) return true;
                if (a1.Classe.ToLower().Contains("space") && !a2.Classe.ToLower().Contains("space")) return true;

                if (a2.Classe.ToLower().EndsWith(" b") && !a1.Classe.ToLower().EndsWith(" b")) return true;
                if (a1.Classe.ToLower().EndsWith(" b") && !a2.Classe.ToLower().EndsWith(" b")) return true;
            }

            if (a1.Classe.ToLower().Contains("food") && a2.Classe.ToLower().Contains("food"))
            {
                if (a2.Classe.ToLower().Contains("materials") && !a1.Classe.ToLower().Contains("materials"))
                    return true;
                if (a1.Classe.ToLower().Contains("materials") && !a2.Classe.ToLower().Contains("materials"))
                    return true;
            }

            if (a1.Classe.ToLower().Contains("materials") && a2.Classe.ToLower().Contains("materials"))
            {
                if (a2.Classe.ToLower().Contains("nuclear") && !a1.Classe.ToLower().Contains("nuclear")) return true;
                if (a1.Classe.ToLower().Contains("nuclear") && !a2.Classe.ToLower().Contains("nuclear")) return true;
            }

            if (a1.Classe.ToLower().Contains("fisica") && a2.Classe.ToLower().Contains("fisica"))
            {
                if (a2.Classe.ToLower().Contains("sperimentale") && !a1.Classe.ToLower().Contains("sperimentale"))
                    return true;
                if (a1.Classe.ToLower().Contains("sperimentale") && !a2.Classe.ToLower().Contains("sperimentale"))
                    return true;
            }

            if (a1.Classe.ToLower().Contains("per") && a2.Classe.ToLower().Contains("per"))
            {
                if (a2.Classe.ToLower().Contains("gruppo") && !a1.Classe.ToLower().Contains("gruppo")) return true;
                if (a1.Classe.ToLower().Contains("gruppo") && !a2.Classe.ToLower().Contains("gruppo")) return true;
            }

            if (a1.Classe.ToLower().Contains("metodi") && a2.Classe.ToLower().Contains("metodi"))
            {
                if (a2.Classe.ToLower().Contains("dispositivi") && !a1.Classe.ToLower().Contains("dispositivi"))
                    return true;
                if (a1.Classe.ToLower().Contains("dispositivi") && !a2.Classe.ToLower().Contains("dispositivi"))
                    return true;
            }

            if (a1.Classe.ToLower().Contains("sicurezza") && a2.Classe.ToLower().Contains("sicurezza"))
            {
                if (a2.Classe.ToLower().Contains("processo") && !a1.Classe.ToLower().Contains("processo")) return true;
                if (a1.Classe.ToLower().Contains("processo") && !a2.Classe.ToLower().Contains("processo")) return true;
            }

            if (a1.Classe.ToLower().Contains("rischio") && a2.Classe.ToLower().Contains("rischio"))
            {
                if (a2.Classe.ToLower().Contains("mitigazione") && !a1.Classe.ToLower().Contains("mitigazione"))
                    return true;
                if (a1.Classe.ToLower().Contains("mitigazione") && !a2.Classe.ToLower().Contains("mitigazione"))
                    return true;
            }

            if (a1.Classe.ToLower().Contains("final") && a2.Classe.ToLower().Contains("final"))
            {
                if (a2.Classe.ToLower().Contains("work") && !a1.Classe.ToLower().Contains("work")) return true;
                if (a1.Classe.ToLower().Contains("work") && !a2.Classe.ToLower().Contains("work")) return true;
            }

            if (a1.Classe.ToLower().Contains("reti") && a2.Classe.ToLower().Contains("reti"))
            {
                if (a2.Classe.ToLower().Contains("logiche") && !a1.Classe.ToLower().Contains("logiche")) return true;
                if (a1.Classe.ToLower().Contains("logiche") && !a2.Classe.ToLower().Contains("logiche")) return true;
            }

            if (a1.Classe.ToLower().Contains("safety") && a2.Classe.ToLower().Contains("safety"))
            {
                if (a2.Classe.ToLower().Contains("mobility") && !a1.Classe.ToLower().Contains("mobility")) return true;
                if (a1.Classe.ToLower().Contains("mobility") && !a2.Classe.ToLower().Contains("mobility")) return true;
            }

            if (a1.Classe.ToLower().Contains("sistemi") && a2.Classe.ToLower().Contains("sistemi"))
            {
                if (a2.Classe.ToLower().Contains("edilizi") && !a1.Classe.ToLower().Contains("edilizi")) return true;
                if (a1.Classe.ToLower().Contains("edilizi") && !a2.Classe.ToLower().Contains("edilizi")) return true;
            }

            if (a1.Classe.ToLower().Contains("comunicazione") && a2.Classe.ToLower().Contains("comunicazione"))
            {
                if (a2.Classe.ToLower().Contains("design") && !a1.Classe.ToLower().Contains("design")) return true;
                if (a1.Classe.ToLower().Contains("design") && !a2.Classe.ToLower().Contains("design")) return true;
            }

            if (a1.Classe.ToLower().Contains("magistrale") && a2.Classe.ToLower().Contains("magistrale"))
            {
                if (a2.Classe.ToLower().Contains("tesi") && !a1.Classe.ToLower().Contains("tesi")) return true;
                if (a1.Classe.ToLower().Contains("tesi") && !a2.Classe.ToLower().Contains("tesi")) return true;
            }

            if (a1.Classe.ToLower().Contains("tesi") && a2.Classe.ToLower().Contains("tesi"))
            {
                if (a2.Classe.ToLower().Contains("tesi") && !a1.Classe.ToLower().Contains("tesi")) return true;
                if (a1.Classe.ToLower().Contains("tesi") && !a2.Classe.ToLower().Contains("tesi")) return true;
            }

            if (a1.Classe.ToLower().Contains(" e ") && a2.Classe.ToLower().Contains(" e "))
            {
                if (a2.Classe.ToLower().Contains("tesi") && !a1.Classe.ToLower().Contains("tesi")) return true;
                if (a1.Classe.ToLower().Contains("tesi") && !a2.Classe.ToLower().Contains("tesi")) return true;
            }

            if (a2.Classe.ToLower().EndsWith(" a") && !a1.Classe.ToLower().EndsWith(" a")) return true;
            if (a1.Classe.ToLower().EndsWith(" a") && !a2.Classe.ToLower().EndsWith(" a")) return true;

            if (a2.Classe.ToLower().EndsWith(" b") && !a1.Classe.ToLower().EndsWith(" b")) return true;
            if (a1.Classe.ToLower().EndsWith(" b") && !a2.Classe.ToLower().EndsWith(" b")) return true;

            if (a2.Classe.ToLower().EndsWith(" c") && !a1.Classe.ToLower().EndsWith(" c")) return true;
            if (a1.Classe.ToLower().EndsWith(" c") && !a2.Classe.ToLower().EndsWith(" c")) return true;

            if (a2.Classe.ToLower().EndsWith(" 1") && !a1.Classe.ToLower().EndsWith(" 1")) return true;
            if (a1.Classe.ToLower().EndsWith(" 1") && !a2.Classe.ToLower().EndsWith(" 1")) return true;

            if (a2.Classe.ToLower().EndsWith(" 2") && !a1.Classe.ToLower().EndsWith(" 2")) return true;
            if (a1.Classe.ToLower().EndsWith(" 2") && !a2.Classe.ToLower().EndsWith(" 2")) return true;

            if (a2.Classe.ToLower().EndsWith(" 3") && !a1.Classe.ToLower().EndsWith(" 3")) return true;
            if (a1.Classe.ToLower().EndsWith(" 3") && !a2.Classe.ToLower().EndsWith(" 3")) return true;

            if (a2.Classe.ToLower().StartsWith("al ") && !a1.Classe.ToLower().StartsWith("al ")) return true;
            if (a1.Classe.ToLower().StartsWith("al ") && !a2.Classe.ToLower().StartsWith("al ")) return true;

            if (a1.Classe.ToLower().Contains('~') && !a2.Classe.ToLower().Contains('~'))
                return true;

            if (a2.Classe.ToLower().Contains('~') && !a1.Classe.ToLower().Contains('~'))
                return true;
        }

        return false;
    }

    public void AggiustaGruppiDoppiIDTelegramUguale()
    {
        var count = 0;
        for (var i = 0; i < _l.Count; i++)
        for (var j = 0; j < _l.Count; j++)
            if (i != j)
                if (!IsNullOrEmpty(_l[i].PermanentId) && !IsNullOrEmpty(_l[j].PermanentId))
                    if (_l[i].PermanentId == _l[j].PermanentId)
                    {
                        count++;

                        _l[i] = Unisci2(i, new Tuple<Gruppo, int>(_l[j], j), true, true);
                        _l.RemoveAt(j);
                        j--;
                    }

        MessageBox.Show("AggiustaGruppiDoppiIDTelegramUguale\n" + count);
    }

    public void StampaWhatsapp()
    {
        var s = "";
        var i = 0;
        foreach (var g in _l)
            if (g.Platform.ToLower() == "wa")
            {
                s += i + " https://chat.whatsapp.com/" + g.IdLink + "\n\n";
                i++;
            }

        var saveFileDialog = new SaveFileDialog();
        var r = saveFileDialog.ShowDialog();
        if (r == DialogResult.OK) File.WriteAllText(saveFileDialog.FileName, s);
    }

    public void ImportaGruppiDaTabellaTelegramGruppiBot_PuntoBin()
    {
        var openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Bin files (*.bin)|*.bin";
        var r = openFileDialog.ShowDialog();
        if (r != DialogResult.OK)
            return;

        var obj = BinaryDeserializeObject(openFileDialog.FileName);

        HandleSerializedObject(obj);
    }

    public void HandleSerializedObject(object obj)
    {
        if (obj == null)
            return;

        if (obj is DataTable dt)
        {
            if (dt.Rows == null)
                return;

            if (dt.Rows.Count == 0)
                return;

            ;

            foreach (DataRow dr in dt.Rows) ImportaGruppoDaDataRow(dr);
        }
    }

    private void ImportaGruppoDaDataRow(DataRow dr)
    {
        var gruppo = CreaGruppo(dr);

        gruppo.Aggiusta(true, true);
        Add(gruppo, true);
    }

    public static Gruppo CreaGruppo(DataRow dr)
    {
        var idlink1 = dr.ItemArray[3].ToString().Split('/');
        var idlink2 = idlink1[^1];
        TipoLink tipoLink;
        if (dr.ItemArray[3].ToString().StartsWith("https://t.me/+"))
            tipoLink = TipoLink.PLUS;
        else if (dr.ItemArray[3].ToString().StartsWith("https://t.me/joinchat/"))
            tipoLink = TipoLink.JOINCHAT;
        else
            tipoLink = TipoLink.UNKNOWN;

        var date = dr.ItemArray[4];
        DateTime? date2 = null;
        try
        {
            date2 = DateTime.Parse(date?.ToString() ?? Empty);
        }
        catch
        {
            ;
        }

        var gruppo = new Gruppo
        {
            Platform = "TG",
            PermanentId = Convert.ToInt64(dr.ItemArray[0]).ToString(),
            IdLink = idlink2,
            Classe = dr.ItemArray[6].ToString(),
            LastUpdateInviteLinkTime = date2,
            TipoLink = tipoLink
        };
        return gruppo;
    }

    public static object BinaryDeserializeObject(string path)
    {
        StreamReader streamReader = null;
        try
        {
            streamReader = new StreamReader(path);
        }
        catch
        {
            ;
        }

        if (streamReader == null)
            return null;

        var binaryFormatter = new BinaryFormatter();
        object obj;
        try
        {
            obj = binaryFormatter.Deserialize(streamReader.BaseStream);
        }
        catch (SerializationException ex)
        {
            throw new SerializationException(ex + "\n" + ex.Source);
        }

        return obj;
    }

    public void ImportaGruppiDalComandoDelBotTelegram_UpdateLinkFromJson()
    {
        var openFileDialog = new OpenFileDialog();
        var r = openFileDialog.ShowDialog();
        if (r != DialogResult.OK)
            return;

        var l = File.ReadAllLines(openFileDialog.FileName);

        ;

        var l2 = SplitPerStringaVuota(l);

        ;

        foreach (var l3 in l2) ImportaGruppiDalComandoDelBotTelegram_UpdateLinkFromJson2(l3);

        MessageBox.Show("Finito l'importazione dei gruppi da Telegram bot!");
    }

    private void ImportaGruppiDalComandoDelBotTelegram_UpdateLinkFromJson2(List<string> l3)
    {
        if (l3 == null || l3.Count == 0)
            return;

        if (l3.Count < 4)
            return;

        var permanentId = l3[3]["PermanentId: ".Length..].Trim();
        if (permanentId == "null" || permanentId == "[null]")
            permanentId = null;

        string idlink = null;
        string newlink = null;
        var nome = l3[9].Substring("Nome: ".Length).Trim();
        List<string> oldlinks_list = null;

        bool? linkFunzionante;

        if (l3[0] == "Success: N")
        {
            linkFunzionante = false;
        }
        else
        {
            linkFunzionante = true;

            idlink = l3[1].Substring("IdLink: ".Length).Trim();
            newlink = l3[2].Substring("NewLink: ".Length).Trim();
            var n2 = newlink.Split('/');
            newlink = n2[^1];

            var oldlinks = l3[4].Substring("OldLink: ".Length).Trim();
            oldlinks_list = GetOldLinks(oldlinks);

            var exceptionmessage = l3[5].Substring("ExceptionMessage: ".Length).Trim();
            var q1 = l3[6]["q1: ".Length..].Trim();
            var q2 = l3[7]["q2: ".Length..].Trim();
            var q3 = l3[8]["q3: ".Length..].Trim();
        }

        var i = TrovaGruppo(idlink, nome, permanentId, oldlinks_list);

        if (i == null || i.Count == 0)
        {
            var g = new Gruppo { Classe = nome, IdLink = idlink, Platform = "TG", PermanentId = permanentId };
            g.Aggiusta(true, true);
            Add(g, false);
            return;
        }

        foreach (var i2 in i)
        {
            if (linkFunzionante != null && linkFunzionante.Value)
            {
                _l[i2].IdLink = newlink;
                _l[i2].LastUpdateInviteLinkTime = DateTime.Now;
                _l[i2].LinkFunzionante = true;
            }

            _l[i2].PermanentId = permanentId;
            _l[i2].Aggiusta(false, true);
        }
    }

    private static List<string> GetOldLinks(string oldlinks)
    {
        if (IsNullOrEmpty(oldlinks))
            return null;

        oldlinks = oldlinks.Trim();

        if (oldlinks.StartsWith("["))
            oldlinks = oldlinks.Substring(1);
        else
            return null;

        if (oldlinks.EndsWith("]"))
            oldlinks = oldlinks.Remove(oldlinks.Length - 1);
        else
            return null;

        if (oldlinks.Contains(','))
        {
            var r = new List<string>();

            var o = oldlinks.Split('\'');
            foreach (var o2 in o)
            {
                var o3 = GetOldLinks2(o2);
                if (IsNullOrEmpty(o3))
                    ;
                else
                    r.Add(o3);
            }

            return r;
        }

        if (!oldlinks.StartsWith("'"))
            return null;

        oldlinks = oldlinks[1..];

        if (oldlinks.EndsWith("'"))
            oldlinks = oldlinks.Remove(oldlinks.Length - 1);
        else
            return null;

        return new List<string> { oldlinks };
    }

    private static string GetOldLinks2(string o2)
    {
        //'feuwbbggqwgqwwg'

        if (IsNullOrEmpty(o2))
            return null;

        o2 = o2.Trim();

        if (o2.StartsWith("'"))
            o2 = o2[1..];
        else
            return null;

        if (o2.EndsWith("'"))
            o2 = o2.Remove(o2.Length - 1);
        else
            return null;

        return o2.Trim();
    }

    private List<int> TrovaGruppo(string idlink, string nome, string permanentId, List<string> oldlinks_list)
    {
        var r = new List<int>();
        for (var i = 0; i < _l.Count; i++)
            if (_l[i].Platform == "TG")
            {
                if (_l[i].IdLink == idlink && !IsNullOrEmpty(idlink))
                    r.Add(i);
                else if (_l[i].Classe == nome && !IsNullOrEmpty(nome))
                    r.Add(i);
                else if (_l[i].PermanentId == permanentId && !IsNullOrEmpty(permanentId))
                    r.Add(i);
                else if (oldlinks_list != null && oldlinks_list.Contains(_l[i].IdLink))
                    r.Add(i);
            }

        return r;
    }

    private static List<List<string>> SplitPerStringaVuota(string[] l)
    {
        List<List<string>> r = new();
        List<string> r2 = null;
        for (var i = 0; i < l.Length; i++)
            if (!IsNullOrEmpty(l[i]))
            {
                if (r2 == null)
                    r2 = new List<string>();

                r2.Add(l[i]);
            }
            else
            {
                if (r2 != null)
                {
                    r.Add(r2);
                    r2 = null;
                }
            }

        return r;
    }

    public void SalvaTelegramIdDeiGruppiLinkCheNonVanno(string anno)
    {
        List<Gruppo> l = new();
        foreach (var x in _l)
            if (x.Platform == "TG")
                if (x.LinkFunzionante == null || x.LinkFunzionante.Value == false)
                {
                    if (IsNullOrEmpty(anno))
                    {
                        l.Add(x);
                    }
                    else
                    {
                        if (x.Year == anno) l.Add(x);
                    }
                }

        var s = "[";

        foreach (var l2 in l)
        {
            s += l2.To_json(CheckGruppo.E.RICERCA_SITO_V3) + ",";
            s += "\n";
        }

        s = s.Remove(s.Length - 1);

        s += "]";

        var saveFileDialog = new SaveFileDialog();
        var r = saveFileDialog.ShowDialog();
        if (r == DialogResult.OK) File.WriteAllText(saveFileDialog.FileName, s);
    }

    public EventoConLog CheckSeILinkVanno(ParametriFunzione parametriFunzione)
    {
        EventoConLog eventoConLog = new();

        eventoConLog.action = (sender, e) => CheckSeILinkVanno2(
            eventoConLog,
            (int)parametriFunzione.GetParam("volteCheCiRiprova"),
            (bool)parametriFunzione.GetParam("laPrimaVoltaControllaDaCapo"),
            (int)parametriFunzione.GetParam("waitOgniVoltaCheCiRiprova")
        );

        return eventoConLog;
    }

    public void CheckSeILinkVanno2(EventoConLog eventoConLog, int volteCheCiRiprova, bool laPrimaVoltaControllaDaCapo,
        int waitOgniVoltaCheCiRiprova = 10)
    {
        for (var j = 0; j < volteCheCiRiprova; j++)
        {
            var b = j != 0 || !laPrimaVoltaControllaDaCapo;
            for (var i = 0; i < _l.Count; i++) _l[i].CheckSeIlLinkVa(b, eventoConLog);
            Task.Delay(waitOgniVoltaCheCiRiprova).Wait();
        }
    }

    public void AggiustaNomiDoppi()
    {
        for (var i = 0; i < _l.Count; i++) _l[i].AggiustaNomeDoppio();
    }

    public void AddAndMerge(Gruppo g, int groupId)
    {
        _l[groupId].Merge(g);
    }

    public int? FindInRamSQL(long id)
    {
        ;

        var ids = id.ToString();

        for (var i = 0; i < _l.Count; i++)
        {
            var g = _l[i];
            if (g.PermanentId == ids) return i;
        }

        return null;
    }

    public void RicreaID()
    {
        for (var i = 0; i < _l.Count; i++) _l[i].RicreaId();
    }

    public void Fix_link_IDCorsi_se_ce_uno_che_ha_il_link_con_id_corso_uguale()
    {
        for (var i = 0; i < _l.Count; i++)
        for (var j = 0; j < _l.Count; j++)
            if (
                i != j &&
                !IsNullOrEmpty(_l[i].IDCorsoPolimi) &&
                !IsNullOrEmpty(_l[j].IDCorsoPolimi) &&
                _l[i].IDCorsoPolimi == _l[j].IDCorsoPolimi &&
                IsNullOrEmpty(_l[i].IdLink) &&
                !IsNullOrEmpty(_l[j].IdLink)
            )
            {
                _l[i].IdLink = _l[j].IdLink;
                _l[i].Platform = _l[j].Platform;

                _l[i].RicreaId();
            }
    }

    public void FixPianoStudi()
    {
        for (var i = 0; i < _l.Count; i++)
        {
            if (IsNullOrEmpty(_l[i].PianoDiStudi))
            {
                ;
            }
            else
            {
                _l[i].PianoDiStudi = _l[i].PianoDiStudi.Trim();

                if (_l[i].PianoDiStudi.EndsWith(":"))
                    _l[i].PianoDiStudi = _l[i].PianoDiStudi.Substring(0, _l[i].PianoDiStudi.Length - 1);
            }

            _l[i].RicreaId();
        }
    }

    public class CoordinatesBasedComparer : IComparer<Gruppo>
    {
        public int Compare(Gruppo a, Gruppo b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            var i1 = CompareOrdinal(a.Year, b.Year);
            if (i1 != 0)
                return i1;

            i1 = CompareOrdinal(a.Classe, b.Classe);
            if (i1 != 0)
                return i1;

            i1 = CompareOrdinal(a.Platform, b.Platform);
            if (i1 != 0)
                return i1;

            i1 = CompareOrdinal2(a.Office, b.Office);
            if (i1 != 0)
                return i1;

            i1 = CompareOrdinal(a.Degree, b.Degree);
            if (i1 != 0)
                return i1;

            i1 = CompareOrdinal(a.IdLink, b.IdLink);
            if (i1 != 0)
                return i1;

            i1 = CompareOrdinal(a.PermanentId, b.PermanentId);
            if (i1 != 0)
                return i1;

            i1 = CompareOrdinal2(a.CCS, b.CCS);
            if (i1 != 0)
                return i1;

            i1 = CompareOrdinal(a.PianoDiStudi, b.PianoDiStudi);
            if (i1 != 0)
                return i1;

            i1 = CompareInt(a.AnnoCorsoStudio, b.AnnoCorsoStudio);
            if (i1 != 0)
                return i1;

            return 0;
        }

        private static int CompareInt(int? annoCorsoStudio1, int? annoCorsoStudio2)
        {
            if (annoCorsoStudio1 == null && annoCorsoStudio2 == null)
                return 0;

            if (annoCorsoStudio1 == null)
                return -1;

            if (annoCorsoStudio2 == null)
                return +1;

            if (annoCorsoStudio1 == annoCorsoStudio2)
                return 0;

            if (annoCorsoStudio1 > annoCorsoStudio2)
                return +1;

            return -1;
        }
    }
}