//  Play.cs
//  Author: Dean Herbert <pe@ppy.sh>
//  Copyright (c) 2010 2010 Dean Herbert
using System;
using osum.GameplayElements;
using osum.GameplayElements.Beatmaps;

namespace osum.GameModes

{
    public class Play : GameMode
    {
        HitObjectManager hitObjectManager;

        public Play() : base()
        {
        }

        internal override void Initialize()
        {
            Beatmap beatmap = new Beatmap("Beatmaps/bcl/");

            hitObjectManager = new HitObjectManager(beatmap);
            hitObjectManager.LoadFile();

            GameBase.Instance.backgroundAudioPlayer.Load("Beatmaps/bcl/babycruisingedit.mp3");
            GameBase.Instance.backgroundAudioPlayer.Play();

            foreach (HitObject h in hitObjectManager.hitObjects)
                spriteManager.Add(h);
        }
        
        
    }
}

