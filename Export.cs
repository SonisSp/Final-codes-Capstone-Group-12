using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Octokit;
using Octokit.Reactive;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

public class Export
{
    // This code is made with the help of Group 28
    public static void ToCsv<T>(
        IEnumerable<T> data,
        string path,
        params (string Header, Func<T, object?> Selector)[] cols)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        using (var sw = File.CreateText(path)) // no-arg, cross-version safe
        {
            // header
            sw.WriteLine(string.Join(",", cols.Select(c => Csv(c.Header))));

            // rows
            foreach (var item in data)
            {
                var fields = cols.Select(c => Csv(c.Selector(item)?.ToString() ?? string.Empty));
                sw.WriteLine(string.Join(",", fields));
            }
        }
    }

    public static string Csv(string s)
    {
        // RFC-4180 style quoting
        if (s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }
}