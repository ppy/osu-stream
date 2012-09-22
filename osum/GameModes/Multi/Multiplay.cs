using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using osu_common.Tencho.Requests;
using osum.GameModes.SongSelect;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;
using osum.Helpers;

namespace osum.GameModes.Play
{
    class Multiplay : GameMode
    {
        private BackButton s_ButtonBack;
        private pButton startButton;

        internal static Multiplay Instance;

        public override void Dispose()
        {
            Instance = null;
            base.Dispose();
        }

        public override void Initialize()
        {
            Instance = this;

            pSprite background =
               new pSprite(TextureManager.Load(OsuTexture.songselect_background), FieldTypes.StandardSnapCentre, OriginTypes.Centre,
                           ClockTypes.Mode, Vector2.Zero, 0, true, new Color4(56, 56, 56, 255));
            background.AlphaBlend = false;
            spriteManager.Add(background);

            s_ButtonBack = new BackButton(delegate
            {
                Director.ChangeMode(OsuMode.MainMenu);
                GameBase.LeaveMatch();
            }, true);

            spriteManager.Add(s_ButtonBack);

            GameBase.Client.SendRequest(RequestType.Stream_RequestMatch, null);

            GameBase.ShowLoadingOverlayWithText("Waiting for other players...");

            startButton = new pButton("Start Game!", new Vector2(GameBase.BaseSizeFixedWidth.Width / 2, 300), new Vector2(280, 50), Color4.SkyBlue, delegate
            {
                Director.ChangeMode(OsuMode.SongSelect);
            });
            spriteManager.Add(startButton);
        }

        public override void Update()
        {
            startButton.Visible = GameBase.Match != null && GameBase.Match.Players.Count > 1;
            base.Update();
        }
    }
}
