using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Ionic.Zip;

namespace JsonPolimi_Core_nf.Utils;

public class ODS_Reader
{
    public static List<List<string>>? Read()
    {
        var o = new OpenFileDialog();
        var r = o.ShowDialog();
        return r != DialogResult.OK ? null : Read2(o.FileName);
    }

    public static List<List<string>>? Read2(string? fileName)
    {
        var e = GetContentXml(fileName);
        if (e == null)
            return null;

        var stream2 = new MemoryStream();
        e.Extract(stream2);

        var r2 = Encoding.ASCII.GetString(stream2.ToArray());

        var doc = new XmlDocument();
        doc.LoadXml(r2);

        return GetTableFromXml(doc);
    }

    private static List<List<string>>? GetTableFromXml(XmlNode doc)
    {
        var x1 = GetTableFromXml1(doc);
        if (x1 == null)
            return null;

        var x2 = GetTableFromXml2(x1);
        if (x2 == null)
            return null;

        var x3 = GetTableFromXml3(x2);
        if (x3 == null)
            return null;

        var x4 = GetTableFromXml4(x3);
        if (x4 == null)
            return null;

        var r = new List<List<string>>();

        foreach (var x5 in x4.ChildNodes)
            if (x5 is XmlElement { Name: "table:table-row" } x6)
            {
                var r1 = GetRowFromXml(x6);
                if (r1 != null)
                    r.Add(r1);
            }

        return r;
    }

    private static List<string> GetRowFromXml(XmlNode x6)
    {
        var r = new List<string>();

        foreach (var c1 in x6.ChildNodes)
            if (c1 is XmlElement c2)
            {
                if (c2.OuterXml.Contains("number-columns-repeated"))
                {
                    var c3 = c2.OuterXml.Split('"');
                    var i2 = DetectRepeteadColumn(c3) ?? 1; //debug here

                    var c4 = Convert.ToInt32(c3[i2]);
                    for (var i = 0; i < c4; i++) r.Add(c2.InnerText);
                }
                else
                {
                    r.Add(c2.InnerText);
                }
            }

        return r;
    }

    private static int? DetectRepeteadColumn(IReadOnlyList<string> c3)
    {
        var i = DetectRepeteadColumn2(c3);

        return i + 1;
    }

    private static int? DetectRepeteadColumn2(IReadOnlyList<string> c3)
    {
        for (var i = 0; i < c3.Count; i++)
        {
            var s = c3[i];
            if (s.Contains("number-columns-repeated"))
                return i;
        }

        return null;
    }

    private static XmlElement? GetTableFromXml4(XmlNode x1)
    {
        foreach (var x2 in x1.ChildNodes)
            if (x2 is XmlElement { Name: "table:table" } x3)
                return x3;

        return null;
    }

    private static XmlElement? GetTableFromXml3(XmlNode doc)
    {
        foreach (var c1 in doc.ChildNodes)
            if (c1 is XmlElement x2)
                return x2;

        return null;
    }

    private static XmlElement? GetTableFromXml2(XmlNode x1)
    {
        foreach (var x2 in x1.ChildNodes)
            if (x2 is XmlElement { Name: "office:body" } x3)
                return x3;

        return null;
    }

    private static XmlElement? GetTableFromXml1(XmlNode doc)
    {
        foreach (var c1 in doc.ChildNodes)
            if (c1 is XmlElement x2)
                return x2;

        return null;
    }

    private static ZipEntry? GetContentXml(string? fileName)
    {
        var options = new ReadOptions { Encoding = Encoding.UTF8 };
        using var zip = ZipFile.Read(fileName, options);
        return zip.FirstOrDefault(e => e.FileName == "content.xml");
    }
}