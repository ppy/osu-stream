using System;
using System.IO;
using osu_common.Bancho;
using osu_common.Helpers;
using System.Collections.Generic;

namespace osu_common.Bancho.Objects
{
    public enum ReplayAction
    {
        Standard,
        NewSong,
        Skip,
        Completion,
        Fail
    }
    public class bReplayFrameBundle : bSerializable
    {
        public List<bReplayFrame> ReplayFrames;
        public bScoreFrame ScoreFrame;
        public ReplayAction Action;

        public bReplayFrameBundle(List<bReplayFrame> frames, ReplayAction action, bScoreFrame scoreFrame)
        {
            ReplayFrames = frames;
            Action = action;
            ScoreFrame = scoreFrame;
        }

        public bReplayFrameBundle(Stream s)
        {
            ReplayFrames = new List<bReplayFrame>();

            SerializationReader sr = new SerializationReader(s);

            
            int frameCount = sr.ReadUInt16();
            for (int i = 0; i < frameCount; i++)
                ReplayFrames.Add(new bReplayFrame(s));

            Action = (ReplayAction)sr.ReadByte();

            try
            {
                ScoreFrame = new bScoreFrame(s);
            }
            catch (Exception)
            { }
        }

        #region bSerializable Members

        public void ReadFromStream(SerializationReader sr)
        {
            throw new System.NotImplementedException();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write((ushort)ReplayFrames.Count);
            foreach(bReplayFrame f in ReplayFrames)
                f.WriteToStream(sw);

            sw.Write((byte)Action);
            
            ScoreFrame.WriteToStream(sw);
        }

        #endregion
    }
}