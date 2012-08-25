using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.GameModes;
using osum.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using osum.Audio;
using osum.Helpers.osu_common.Tencho.Objects;
using osum.Graphics;

namespace osum.Network
{
    partial class Client : GameComponent
    {
        TouchBurster burster = new TouchBurster(false, Color4.Pink);

        pText overlayIcon;
        private void InitializeOverlay()
        {
            overlayIcon = new pText(string.Empty, 14, Vector2.Zero, 1, true, Color4.White);
            spriteManager.Add(overlayIcon);

            OnConnect += Client_OnConnect;
            OnDisconnect += Client_OnDisconnect;
        }

        public override bool Draw()
        {
            burster.Draw();
            return base.Draw();
        }

        void Client_OnDisconnect()
        {
            overlayIcon.Text = "Disconnected";
            AudioEngine.PlaySample(OsuSamples.stream_down);
        }

        void Client_OnConnect()
        {
            overlayIcon.Text = "Connected";
            AudioEngine.PlaySample(OsuSamples.stream_up);
        }

        public override void Update()
        {
            if (GameBase.Match != null)
            {
                overlayIcon.Text = GameBase.Match.State.ToString() + " (" + GameBase.Match.Players.Count + ")";
                foreach (bPlayerData d in GameBase.Match.Players)
                    //if (d.Username != GameBase.ClientId)
                    {
                        foreach (TrackingPoint p in d.Input)
                            burster.Burst(p.BasePosition);
                    }
            }

            burster.Update();
            base.Update();
        }
    }
}
