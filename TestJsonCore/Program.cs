// See https://aka.ms/new-console-template for more information

using JsonPolimi_Core_nf.Tipi;

Console.WriteLine("Hello, World!");


;
//var result = JsonPolimi_Core_nf.Tipi.Gruppo.CheckSeIlLinkVa2_Telegram("https://t.me/joinchat/aY_y5vie7GU2MDNk", default, null);
;


;


ListaGruppo x = new()
{
    { new Gruppo { IdLink = "aY_y5vie7GU2MDNk", Platform = "TG" }, true }
};
x.CheckSeILinkVanno2(null, 10, true);
;