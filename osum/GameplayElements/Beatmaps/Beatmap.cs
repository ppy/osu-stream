//  Beatmap.cs
//  Author: Dean Herbert <pe@ppy.sh>
//  Copyright (c) 2010 2010 Dean Herbert
using System;
using System.IO;
namespace osum.GameplayElements.Beatmaps
{
    public class Beatmap
    {
        public string ContainerFilename;

        public string BeatmapFilename { get { return ContainerFilename + "/beatmap.osu"; } }
        public string StoryboardFilename { get { return ""; } }

        public Beatmap(string containerFilename)
        {
            ContainerFilename = containerFilename;
        }


        public Stream GetFileStream(string filename)
        {
            return File.OpenRead(filename);
        }

    }
}

