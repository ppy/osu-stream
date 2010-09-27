//  Beatmap.cs
//  Author: Dean Herbert <pe@ppy.sh>
//  Copyright (c) 2010 2010 Dean Herbert
using System;
using System.IO;
using System.Collections.Generic;
using osum.GameplayElements.Beatmaps;
using osu_common.Libraries.Osz2;
namespace osum.GameplayElements.Beatmaps
{
    public partial class Beatmap
    {
        public string ContainerFilename;

        public byte DifficultyOverall;
        public byte DifficultyCircleSize;
        public byte DifficultyHpDrainRate;
        public int StackLeniency = 1;

        public string BeatmapFilename { get { return ContainerFilename + "/beatmap.osu"; } }
        public string StoryboardFilename { get { return ""; } }

        private MapPackage package;
        public MapPackage Package
        {
            get
            {
                if (package == null) package = new MapPackage(ContainerFilename);

                return package;
            }

        }
        public string AudioFilename;

        public Beatmap(string containerFilename)
        {
            ContainerFilename = containerFilename;
        }

        public Stream GetFileStream(string filename)
        {
            return Package.GetFile(filename);
        }

        internal byte[] GetFileBytes(string filename)
        {
            byte[] data = null;

            using (Stream stream = GetFileStream(filename))
            {
                if (stream != null)
                {
                    data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                    stream.Close();
                }

            }

            return data;
        }
    }
}

