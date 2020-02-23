using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.GData.Client;
using Google.GData.Spreadsheets;

namespace LocalisationUpdater
{
    static class Program
    {
        static SpreadsheetsService ss = new SpreadsheetsService("stream-localisation");

        static void Main(string[] args)
        {
            Environment.CurrentDirectory = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("osum", StringComparison.Ordinal)) + @"\osum\osum\Localisation\";
            ss.setUserCredentials("stream@ppy.sh", "osumisawsum");

            SpreadsheetFeed feed = ss.Query(new SpreadsheetQuery());

            SpreadsheetEntry sheet = null;

            //get spreadsheet
            foreach (var entry in feed.Entries.OfType<SpreadsheetEntry>())
            {
                if (entry.Title.Text == "osu!stream localisation")
                {
                    sheet = entry;
                    break;
                }
            }

            if (sheet == null)
            {
                Console.WriteLine("failed to get spreadsheet");
                Console.ReadLine();
            }

            //get worksheet
            AtomLink link = sheet.Links.FindService(GDataSpreadsheetsNameTable.WorksheetRel, null);
            WorksheetQuery query = new WorksheetQuery(link.HRef.ToString());
            WorksheetFeed wfeed = ss.Query(query);


            WorksheetEntry worksheet = wfeed.Entries[0] as WorksheetEntry;

            ProcessWorksheet(worksheet);
        }

        static List<LanguageCodeWriter> writers = new List<LanguageCodeWriter>();

        private static void ProcessWorksheet(WorksheetEntry worksheet)
        {
            AtomLink cellFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.CellRel, null);

            CellQuery query = new CellQuery(cellFeedLink.HRef.ToString());
            CellFeed feed = ss.Query(query);

            Console.WriteLine("Processing cells...");

            string currentKey = null;
            foreach (var cell in feed.Entries.OfType<CellEntry>())
            {
                switch (cell.Column)
                {
                    case 1:
                        switch (cell.Value)
                        {
                            case "--OSUSTREAM START--":
                                break;
                            case "--OSUSTREAM END--":
                                Console.WriteLine("Found end!");
                                return;
                            default:
                                Console.WriteLine("Processing key: " + cell.Value);
                                currentKey = cell.Value;
                                break;
                        }

                        break;
                    default:
                        if (cell.Row == 2)
                        {
                            //langauge codes
                            writers.Add(new LanguageCodeWriter(cell.Value));
                            continue;
                        }

                        if (writers.Count > 0)
                        {
                            if (cell.Value == null) continue;

                            string content = cell.Value.Trim(' ', '\n', '\r', '\t');

                            if (content.IndexOf(@"\\", StringComparison.Ordinal) >= 0)
                            {
                                Console.WriteLine("key {0} translation {1} failed with excess escape characters", currentKey, content);
                                Console.ReadLine();
                            }

                            try
                            {
                                int argCount = content.Count(c => c == '{');
                                object[] args = new object[argCount];
                                for (int i = 0; i < argCount; i++)
                                    args[i] = "test";

                                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                                string.Format(content, args);
                            }
                            catch
                            {
                                Console.WriteLine("key {0} translation {1} failed with string format", currentKey, content);
                                Console.ReadLine();
                            }

                            writers[(int)cell.Column - 2].Add(currentKey, content);
                        }

                        break;
                }
            }
        }
    }

    public class LanguageCodeWriter
    {
        private string Code;
        readonly string Filename;

        public LanguageCodeWriter(string code)
        {
            Code = code;
            Filename = code + ".txt";
            File.WriteAllText(Filename, "");
        }

        public void Add(string key, string val)
        {
            File.AppendAllText(Filename, key + "=" + val + "\n");
        }
    }
}