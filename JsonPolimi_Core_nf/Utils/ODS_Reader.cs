using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Ionic.Zip;

namespace JsonPolimi_Core_nf.Utils;

public class ODS_Reader
{
    public static List<List<string>> Read()
    {
        var o = new OpenFileDialog();
        var r = o.ShowDialog();
        return r != DialogResult.OK ? null : Read2(o.FileName);
    }

    public static List<List<string>> Read2(string fileName)
    {
        var e = GetContentXML(fileName);
        if (e == null)
            return null;

        var stream2 = new MemoryStream();
        e.Extract(stream2);

        var r2 = Encoding.ASCII.GetString(stream2.ToArray());

        var doc = new XmlDocument();
        doc.LoadXml(r2);

        return GetTableFromXML(doc);
    }

    private static List<List<string>> GetTableFromXML(XmlDocument doc)
    {
        var x1 = GetTableFromXML1(doc);
        if (x1 == null)
            return null;

        var x2 = GetTableFromXML2(x1);
        if (x2 == null)
            return null;

        var x3 = GetTableFromXML3(x2);
        if (x3 == null)
            return null;

        var x4 = GetTableFromXML4(x3);
        if (x4 == null)
            return null;

        var r = new List<List<string>>();

        foreach (var x5 in x4.ChildNodes)
            if (x5 is XmlElement x6)
                if (x6.Name == "table:table-row")
                {
                    var r1 = GetRowFromXML(x6);
                    if (r1 != null)
                        r.Add(r1);
                }

        return r;
    }

    private static List<string> GetRowFromXML(XmlElement x6)
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

    private static int? DetectRepeteadColumn(string[] c3)
    {
        var i = DetectRepeteadColumn2(c3);

        return i + 1;
    }

    private static int? DetectRepeteadColumn2(string[] c3)
    {
        for (var i = 0; i < c3.Length; i++)
        {
            var s = c3[i];
            if (s.Contains("number-columns-repeated"))
                return i;
        }

        return null;
    }

    private static XmlElement GetTableFromXML4(XmlElement x1)
    {
        foreach (var x2 in x1.ChildNodes)
            if (x2 is XmlElement x3)
                if (x3.Name == "table:table")
                    return x3;

        return null;
    }

    private static XmlElement GetTableFromXML3(XmlElement doc)
    {
        foreach (var c1 in doc.ChildNodes)
            if (c1 is XmlElement x2)
                return x2;

        return null;
    }

    private static XmlElement GetTableFromXML2(XmlElement x1)
    {
        foreach (var x2 in x1.ChildNodes)
            if (x2 is XmlElement x3)
                if (x3.Name == "office:body")
                    return x3;

        return null;
    }

    private static XmlElement GetTableFromXML1(XmlDocument doc)
    {
        foreach (var c1 in doc.ChildNodes)
            if (c1 is XmlElement x2)
                return x2;

        return null;
    }

    private static ZipEntry GetContentXML(string fileName)
    {
        var options = new ReadOptions { Encoding = Encoding.UTF8 };
        using (var zip = ZipFile.Read(fileName, options))
        {
            foreach (var e in zip)
                if (e.FileName == "content.xml")
                    return e;
        }

        return null;
    }
}