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
using System.Drawing;
using osum.GameModes.Play;

namespace osum.Network
{
    partial class Client : GameComponent
    {
        TouchBurster burster = new TouchBurster(false, new Color4(102, 125, 205, 100));

        pText overlayIcon;
        private void InitializeOverlay()
        {
            overlayIcon = new pText(string.Empty, 10, Vector2.Zero, 1, true, Color4.White);
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

        List<PointF> lastFrameBursts = new List<PointF>();

        public override void Update()
        {
            if (GameBase.Match != null)
            {
                if (Multiplay.Instance != null)
                {
                    int otherPlayers = GameBase.Match.Players.Count - 1;

                    if (otherPlayers > 0)
                        GameBase.ShowLoadingOverlayWithText("Found " + otherPlayers + " other player" + (otherPlayers > 1 ? "s" : ""));
                    else
                        GameBase.ShowLoadingOverlayWithText("Waiting for other players...");
                }
                
                overlayIcon.Text = GameBase.Match.State.ToString() + " (" + GameBase.Match.Players.Count + ")";

                List<PointF> bursts = new List<PointF>();

                foreach (bPlayerData d in GameBase.Match.Players)
                    if (d.Username != GameBase.ClientId)
                    {
                        foreach (TrackingPoint p in d.Input)
                        {
                            bursts.Add(p.Location);

                            if (p.WindowDelta == Vector2.Zero && !lastFrameBursts.Contains(p.Location))
                                burster.InputManager_OnDown(null, p);
                            else
                                burster.InputManager_OnMove(null, p);
                        }
                    }

                lastFrameBursts = bursts;
            }
            else if (GameBase.Client.Connected)
                overlayIcon.Text = "Connected";
            else
                overlayIcon.Text = "Disconnected";


            burster.Update();
            base.Update();
        }
    }
}
