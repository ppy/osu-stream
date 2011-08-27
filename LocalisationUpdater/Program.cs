using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.GData.Spreadsheets;
using Google.GData.Client;
using System.IO;

namespace LocalisationUpdater
{
    class Program
    {
        static SpreadsheetsService ss = new SpreadsheetsService("stream-localisation");

        static void Main(string[] args)
        {
            Environment.CurrentDirectory = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("osum")) + @"\osum\osum\Localisation\";
            ss.setUserCredentials("stream@ppy.sh", "osumisawsum");

            SpreadsheetFeed feed = ss.Query(new SpreadsheetQuery());

            SpreadsheetEntry sheet = null;

            //get spreadsheet
            foreach (SpreadsheetEntry entry in feed.Entries)
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
            foreach (CellEntry cell in feed.Entries)
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
                            writers[(int)cell.Column - 2].Add(currentKey, cell.Value.Trim(' ', '\n','\r','\t'));

                        break;
                }
            }
        }
    }

    public class LanguageCodeWriter
    {
        string Code;
        string Filename;
        public LanguageCodeWriter(string code)
        {
            Code = code;
            Filename = code + ".txt";
            File.WriteAllText(Filename,"");
        }

        public void Add(string key, string val)
        {
            File.AppendAllText(Filename, key + "=" + val + "\n");
        }
    }
}
