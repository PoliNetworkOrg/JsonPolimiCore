using JsonPolimi_Core_nf.Enums;

namespace JsonPolimi_Core_nf.Tipi;

public class SomiglianzaClasse
{
    public readonly Gruppo? a2;
    public Gruppo? a1;
    public SomiglianzaEnum somiglianzaEnum;

    public SomiglianzaClasse(SomiglianzaEnum dIVERSI, Gruppo? a1 = null, Gruppo? a2 = null)
    {
        somiglianzaEnum = dIVERSI;
        this.a1 = a1;
        this.a2 = a2;
    }
}