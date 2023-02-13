using System.Collections.Generic;
using JsonPolimi_Core_nf.Enums;

namespace JsonPolimi_Core_nf.Tipi;

public class InfoParteDiGruppo
{
    public ImmagineGruppo? immagine;
    public Lingua? lingua;
    public LinkGruppo? link;
    public List<InfoParteDiGruppo>? sottopezzi;
    public string? testo_selvaggio;

    public InfoParteDiGruppo(string? testo_selvaggio)
    {
        this.testo_selvaggio = testo_selvaggio;
    }

    public InfoParteDiGruppo(LinkGruppo? link)
    {
        this.link = link;
    }

    public InfoParteDiGruppo(ImmagineGruppo? immagine)
    {
        this.immagine = immagine;
    }

    public InfoParteDiGruppo(List<InfoParteDiGruppo>? sottopezzi)
    {
        this.sottopezzi = sottopezzi;
    }

    public InfoParteDiGruppo(Lingua lingua)
    {
        this.lingua = lingua;
    }
}