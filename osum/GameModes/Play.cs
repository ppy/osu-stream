//  Play.cs
//  Author: Dean Herbert <pe@ppy.sh>
//  Copyright (c) 2010 2010 Dean Herbert
using System;
using osum.GameplayElements;
using osum.GameplayElements.Beatmaps;
using osum.Helpers;
//using osu.Graphics.Renderers;
using osu.Graphics.Primitives;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using System.Drawing;
using osum.Audio;
using osu.Graphics.Renderers;

namespace osum.GameModes

{
    public class Play : GameMode
    {
        HitObjectManager hitObjectManager;
        internal SliderTrackRenderer sliderTest;

        public Play() : base()
        {
        }

        void InputManager_OnDown(InputSource source, TrackingPoint point)
        {
            //check with the hitObjectManager for a relevant hitObject...
            HitObject found = hitObjectManager.FindObjectAt(point);

            if (found != null)
                found.Hit();
        }

        internal override void Initialize()
        {
            Beatmap beatmap = new Beatmap("Beatmaps/bcl/");

            InputManager.OnDown += new InputHandler(InputManager_OnDown);

            hitObjectManager = new HitObjectManager(beatmap);
            hitObjectManager.LoadFile();

            AudioEngine.Music.Load("Beatmaps/bcl/babycruisingedit.mp3");
            AudioEngine.Music.Play();

            sliderTest = new SliderTrackRenderer();
        }

        public override void Dispose()
        {
            InputManager.OnDown -= new InputHandler(InputManager_OnDown);

            hitObjectManager.Dispose();

            base.Dispose();
        }

        public override void Draw()
        {
            hitObjectManager.Draw();

            List<Line> list = new List<Line>();
            list.Add(new Line(new Vector2(20,20),new Vector2(400,400)));

            //sliderTest.Draw(list, 80, Color4.White, Color4.Black, null, new Rectangle(0,0,640,480));

            base.Draw();
        }

        public override void Update()
        {
            hitObjectManager.Update();

            base.Update();
        }
        
        
    }
}

