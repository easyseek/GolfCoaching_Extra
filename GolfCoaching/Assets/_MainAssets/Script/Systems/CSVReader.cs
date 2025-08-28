using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

public class CSVReader
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };

    public static string m_FilePath;

    public static List<Dictionary<string, object>> ReadCSV(string file)
    {
        var list = new List<Dictionary<string, object>>();

        string filePath = string.Empty;
        string source = string.Empty;

        //filePath = $"{Application.dataPath}/{file}.csv";

        // C
        if (!Path.IsPathRooted(file) && file.StartsWith("DataBase"))
        {
            string cDrivePath = Path.Combine(@"C:\", file + ".csv");
            if (File.Exists(cDrivePath))
            {
                filePath = cDrivePath;
            }
        }

        // Assets
        if (string.IsNullOrEmpty(filePath))
        {
            string assetPath = Path.Combine(Application.dataPath, file + ".csv");
            if (File.Exists(assetPath))
            {
                filePath = assetPath;
            }
        }

        // StreamingAssets
        if (string.IsNullOrEmpty(filePath))
        {
            string streamingPath = Path.Combine(Application.streamingAssetsPath, file + ".csv");
            if (File.Exists(streamingPath))
            {
                filePath = streamingPath;
            }
        }

        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogWarning($"CSV ������ ã�� �� �����ϴ�");
            return list;
        }

        using (StreamReader sr = new StreamReader(filePath, System.Text.Encoding.Default))
        {
            source = sr.ReadToEnd();
        }

        var lines = Regex.Split(source, LINE_SPLIT_RE);

        if(lines.Length <= 1)
        {
            Debug.LogWarning("CSV ������ ������ �ùٸ��� �ʽ��ϴ�.");
            return list;
        }

        int headerIndex = 0;

        while (headerIndex < lines.Length && lines[headerIndex].TrimStart().StartsWith(";"))
        {
            headerIndex++;
        }

        if (headerIndex >= lines.Length)
        {
            return list;
        }

        var header = Regex.Split(lines[headerIndex], SPLIT_RE);

        for (int i = 0; i < header.Length; i++)
        {
            header[i] = header[i].TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS);
        }

        for (int i = headerIndex + 1; i < lines.Length; i++)
        {
            var line = lines[i];

            if (line.TrimStart().StartsWith(";"))
                continue;

            var values = Regex.Split(line, SPLIT_RE);
            if (values.Length == 0 || string.IsNullOrEmpty(values[0].Trim()))
                continue;

            if (values[0].TrimStart().StartsWith(";"))
                continue;

            var entry = new Dictionary<string, object>();
            int columnCount = Math.Min(header.Length, values.Length);

            for (int j = 0; j < columnCount; j++)
            {
                if (j > 0 && values[j].TrimStart().StartsWith(";"))
                    continue;

                string value = values[j]
                    .TrimStart(TRIM_CHARS)
                    .TrimEnd(TRIM_CHARS)
                    .Replace("\\n", "##NEWLINE##")
                    .Replace("\\", "")
                    .Replace("##NEWLINE##", "\\n");

                object finalValue = value;
                if (int.TryParse(value, out int iv))
                    finalValue = iv;
                else if (float.TryParse(value, out float fv))
                    finalValue = fv;

                entry[header[j]] = finalValue;
            }

            if (entry.Count > 0)
                list.Add(entry);
        }

        return list;
    }
}
