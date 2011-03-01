using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BeatmapCombinator
{
    class BeatmapDifficulty
    {
        internal string VersionName;
        internal List<HitObjectLine> HitObjectLines = new List<HitObjectLine>();
        internal List<string> HeaderLines = new List<string>();
    }

    class HitObjectLine
    {
        internal string StringRepresentation;
        internal int Time;
    }

    class BeatmapCombinator
    {
        /// <summary>
        /// Combines many .osu files into one .osc
        /// </summary>
        /// <param name="args">Directory containing many .osu files</param>
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("No path specified!");
                return;
            }

            string[] osuFiles = Directory.GetFiles(args[0], "*.osu");

            if (osuFiles.Length < 1)
            {
                Console.WriteLine("No .osu files found!");
                return;
            }

            Console.WriteLine("Files found:");
            Console.WriteLine(string.Join("\n", osuFiles));

            string newFilename = osuFiles[0].Remove(osuFiles[0].LastIndexOf('[') - 1) + ".osc";

            List<BeatmapDifficulty> difficulties = new List<BeatmapDifficulty>();

            foreach (string f in osuFiles)
            {
                BeatmapDifficulty bd = new BeatmapDifficulty();
                difficulties.Add(bd);

                bool readingHitObjects = false;

                foreach (string line in File.ReadAllLines(f))
                {
                    if (line.StartsWith("Version:"))
                    {
                        bd.VersionName = line.Replace("Version:", "");
                        continue;
                    }

                    if (readingHitObjects)
                    {
                        if (line.Length < 1) continue;

                        bd.HitObjectLines.Add(new HitObjectLine() { StringRepresentation = line, Time = Int32.Parse(line.Split(',')[2]) });
                        continue;
                    }
                    else
                        bd.HeaderLines.Add(line);

                    switch (line)
                    {
                        case "[HitObjects]":
                            readingHitObjects = true;
                            break;
                    }
                }
            }

            using (StreamWriter output = new StreamWriter(newFilename))
            {
                //write headers first (use first difficulty as arbitrary source)
                foreach (string l in difficulties[0].HeaderLines)
                    output.WriteLine(l);

                //keep track of how many hitObject lines are remaining for each difficulty
                int[] linesRemaining = new int[difficulties.Count];
                for (int i = 0; i < difficulties.Count; i++)
                    linesRemaining[i] = difficulties[i].HitObjectLines.Count;

                int currentTime = 0;

                string currentLine = "";
                string currentLinePrefix = "";

                while (!linesRemaining.All(i => i == 0))
                {
                    int bestMatchDifficulty = -1;
                    HitObjectLine bestMatchLine = null;

                    for (int i = 0; i < difficulties.Count; i++)
                    {
                        if (linesRemaining[i] == 0)
                            continue;

                        int holOffset = difficulties[i].HitObjectLines.Count - linesRemaining[i];
                        
                        HitObjectLine line = difficulties[i].HitObjectLines[holOffset];

                        if (line.Time > currentTime && (bestMatchLine == null || line.Time < bestMatchLine.Time))
                        {
                            bestMatchDifficulty = i;
                            bestMatchLine = line;
                        }
                    }


                    if (currentLine == bestMatchLine.StringRepresentation)
                        //add this difficulty index to the start of the line if it is a dupe line
                        currentLinePrefix += "|" + bestMatchDifficulty;
                    else
                    {
                        if (currentLine.Length > 0) output.WriteLine(currentLinePrefix + "," + currentLine);
                        currentLine = bestMatchLine.StringRepresentation;
                        currentLinePrefix = bestMatchDifficulty.ToString();
                    }
                    
                    linesRemaining[bestMatchDifficulty]--;
                }

                //write the final line from buffer.
                output.WriteLine(currentLinePrefix + "," + currentLine);
            }
        }
    }
}
