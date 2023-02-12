namespace JsonPolimi_Core_nf.Tipi;

public class CheckGruppo
{
    public enum E
    {
        VECCHIA_RICERCA,
        NUOVA_RICERCA,
        TUTTO,
        RICERCA_SITO_V3
    }

    //value
    public E n;

    public CheckGruppo(E a)
    {
        n = a;
    }
}