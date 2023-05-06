using System;
using OpenTK;
using OpenTK.Graphics;
using osum.Audio;
using osum.GameModes.Play;
using osum.GameModes.SongSelect;
using osum.GameplayElements;
using osum.Graphics;
using osum.Graphics.Sprites;
using osum.Helpers;
using osum.Input;
using osum.Input.Sources;
using osum.Support;

namespace osum.GameModes.Options
{
    public class Credits : Player
    {
        private readonly string[] creditsRoll =
        {
            "OsuTexture.menu_logo",
            "Created by Dean \"peppy\" Herbert",
            "*Graphics",
            "Koko Ban - Concept artwork, Interface mockups, colours!",
            "LuigiHann - Gameplay element contributions, rank letters",
            "peppy - Interface, gameplay elements, animation and flow",
            "XiaoUnlimited - Spinner circle",
            "*Implementation",
            "peppy - Most stuff?",
            "Intermezzo - Android platform deployment, package file format, encryption",
            "mm201 - Slider perfection, gameplay mechanic tweaking, hit sample augments",
            "Echo49 - package file format, engine upgrades, general tweaking",
            "Tim Oliver - help with new iOS device compatibility fixes",
            "*Game Audio",
            "nekodex - osu!stream theme music, results screen sfx",
            "Natteke - credits screen mix",
            "XiaoUnlimited - countdown voice",
            "*Artist Relations",
            "peppy",
            "dvorak",
            "jericho2442",
            "*Level Design",
            "Garven",
            "Gens",
            "jericho2442",
            "Krisom",
            "Larto",
            "mm201",
            "ouranhshc",
            "peppy",
            "RandomJibberish",
            "Sushi",
            "*Localisation",
            "chonicle, statementreply - Chinese (Simplified)",
            "Alace, qinche - Chinese (Traditional)",
            "Navy - Danish",
            "Elysion, Dagonpater - French",
            "Nharox, Larto - German",
            "Card N'FoRcE, Inamaru - Italian",
            "ItsZeanKun - Indonesian",
            "SiRiRu, dvorak & co. - Japanese",
            "PJMS, wetchan & co. - Korean",
            "Natsuuki, Hector980 - Spanish",
            "Xgor - Swedish",
            "Kharl, RuRu - Thai",
            "*Thanks to",
            "Nuudles - Developing the cydia osu! release which is still standing strong",
            "Testers - Special thanks to Cyclone, Doddler, dvorak, Guy-kun, James, mattyu007, nekodex, PJMS, Saphier, tobebuta and my mum (i'm serious)",
            "#bat - For support and help on various occasions"
        };

        private readonly int beatLength = 800;

        private pDrawable lastText;

        private static int height_extra = 200 + GameBase.SuperWidePadding;

        public override void Initialize()
        {
            spriteManager.CheckSpritesAreOnScreenBeforeRendering = true;

            Difficulty = Difficulty.None;
            Beatmap = null;

            Clock.ResetManual();

            InitializeBgm();

            base.Initialize();

            playfieldBackground.Colour = new Color4(33, 81, 138, 255);

            s_ButtonBack = new BackButton(delegate { Director.ChangeMode(OsuMode.Options); }, false);
            topMostSpriteManager.Add(s_ButtonBack);

            int time = Clock.ModeTime;

            const int speed = 17000;
            const int spacing = 1800;
            const int header_spacing = 2100;
            const int image_spacing = 2500;

            int len = creditsRoll.Length;
            for (int i = 0; i < len; i++)
            {
                string drawString = creditsRoll[i];

                if (drawString.Length == 0)
                    continue;

                bool isHeader = false;
                if (drawString[0] == '*')
                {
                    drawString = drawString.Substring(1);
                    isHeader = true;
                }

                pDrawable text;

                if (isHeader)
                {
                    text = new pText(drawString, 30, Vector2.Zero, SpriteManager.drawOrderFwdPrio(i), true, new Color4(255, 168, 0, 255))
                    {
                        Field = FieldTypes.StandardSnapTopCentre,
                        Origin = OriginTypes.Centre,
                        Clocking = ClockTypes.Manual,
                        RemoveOldTransformations = false,
                        Alpha = 1
                    };

                    if (i > 0)
                        time += header_spacing - spacing;

                    text.Transform(new TransformationV(new Vector2(text.Position.X, GameBase.BaseSizeFixedWidth.Y + height_extra), new Vector2(0, -100), time, time + speed));
                    time += header_spacing;
                }
                else if (drawString.StartsWith("OsuTexture."))
                {
                    text = new pSprite(TextureManager.Load((OsuTexture)Enum.Parse(typeof(OsuTexture), drawString.Replace("OsuTexture.", ""))), Vector2.Zero)
                    {
                        Field = FieldTypes.StandardSnapTopCentre,
                        Origin = OriginTypes.Centre,
                        Clocking = ClockTypes.Manual,
                        RemoveOldTransformations = false,
                        Alpha = 1
                    };

                    if (i > 0)
                        time += image_spacing - spacing;

                    text.Transform(new TransformationV(new Vector2(text.Position.X, GameBase.BaseSizeFixedWidth.Y + height_extra), new Vector2(0, -100), time, time + speed));
                    time += image_spacing;
                }
                else
                {
                    string[] split = drawString.Split('-');

                    if (split.Length == 1)
                    {
                        text = new pText(drawString, 26, Vector2.Zero, SpriteManager.drawOrderFwdPrio(i), true, Color4.White)
                        {
                            Field = FieldTypes.StandardSnapTopCentre,
                            Origin = OriginTypes.Centre,
                            Clocking = ClockTypes.Manual,
                            RemoveOldTransformations = false,
                            TextShadow = true,
                            Alpha = 1
                        };
                    }
                    else
                    {
                        text = new pText(split[0].Trim(), 22, new Vector2(-10, 0), SpriteManager.drawOrderFwdPrio(i), true, i % 2 == 0 ? new Color4(187, 230, 255, 255) : new Color4(255, 187, 253, 255))
                        {
                            Field = FieldTypes.StandardSnapTopCentre,
                            Origin = OriginTypes.CentreRight,
                            Clocking = ClockTypes.Manual,
                            RemoveOldTransformations = false,
                            Alpha = 1
                        };

                        pText text2 = new pText(split[1].Trim(), 16, new Vector2(10, 0), new Vector2(300, 0), SpriteManager.drawOrderFwdPrio(i), true, i % 2 == 0 ? new Color4(200, 200, 200, 255) : new Color4(240, 240, 240, 255), false)
                        {
                            Field = FieldTypes.StandardSnapTopCentre,
                            Origin = OriginTypes.CentreLeft,
                            Clocking = ClockTypes.Manual,
                            RemoveOldTransformations = false,
                            Alpha = 1
                        };

                        text2.Transform(new TransformationV(new Vector2(text2.Position.X, GameBase.BaseSizeFixedWidth.Y + height_extra), new Vector2(text2.Position.X, -100), time, time + speed));
                        spriteManager.Add(text2);
                    }

                    text.Transform(new TransformationV(new Vector2(text.Position.X, GameBase.BaseSizeFixedWidth.Y + height_extra), new Vector2(text.Position.X, -100), time, time + speed));
                    time += spacing;
                }

                spriteManager.Add(text);
                lastText = text;
            }

            InputManager.OnMove += InputManager_OnMove;
        }

        public override void Dispose()
        {
            InputManager.OnMove -= InputManager_OnMove;
            base.Dispose();
        }

        private float incrementalSpeed = 1;
        private BackButton s_ButtonBack;

        private void InputManager_OnMove(InputSource source, TrackingPoint trackingPoint)
        {
            if (!InputManager.IsPressed || InputManager.PrimaryTrackingPoint == null || InputManager.PrimaryTrackingPoint.HoveringObject is BackButton)
                return;

            incrementalSpeed = (-trackingPoint.WindowDelta.Y * 2) * 0.5f + incrementalSpeed * 0.5f;

            Clock.IncrementManual(incrementalSpeed);
            if (incrementalSpeed < 0) spriteManager.ResetFirstTransformations();
        }

        protected override void initializeUIElements()
        {
        }

        /// <summary>
        /// Initializes the song select BGM and starts playing. Static for now so it can be triggered from anywhere.
        /// </summary>
        internal void InitializeBgm()
        {
            //Start playing song select BGM.
#if iOS
            AudioEngine.Music.Load("Skins/Default/credits.m4a", true);
#else
            AudioEngine.Music.Load("Skins/Default/credits.mp3", true);
#endif
            AudioEngine.Music.Play();
        }

        public override bool Draw()
        {
            base.Draw();
            return true;
        }

        private int lastBeat;
        private int lastBeatNoLoop;

        public override void Update()
        {
            int currentBeat = (int)((Clock.AudioTime) / (beatLength / 4f)) % 16;
            int currentBeatNoLoop = (int)((Clock.AudioTime) / (beatLength / 4f));

            if (lastText.Position.Y < 0 && !Director.IsTransitioning)
            {
                Director.ChangeMode(OsuMode.Options, new FadeTransition(3000, FadeTransition.DEFAULT_FADE_IN));
            }

            if (currentBeat != lastBeat)
            {
                switch (currentBeat)
                {
                    case 0:
                        spriteManager.ScaleScalar = 1.04f;
                        spriteManager.ScaleTo(1, 200, EasingTypes.In);
                        break;
                    case 2:
                    case 10:
                        spriteManager.Rotation = 0.015f;
                        spriteManager.RotateTo(0, 300);
                        break;
                    case 6:
                    case 14:
                        spriteManager.Rotation = -0.015f;
                        spriteManager.RotateTo(0, 300);
                        break;
                    case 7:
                        spriteManager.ScaleScalar = 1.01f;
                        spriteManager.ScaleTo(1, 200, EasingTypes.In);
                        break;
                    case 8:
                        spriteManager.ScaleScalar = 1.04f;
                        spriteManager.ScaleTo(1, 200, EasingTypes.In);
                        break;
                }

                lastBeat = currentBeat;
            }

            if (currentBeatNoLoop != lastBeatNoLoop)
            {
                switch (currentBeatNoLoop)
                {
                    case 160:
                    case 168:
                        playfieldBackground.FlashColour(Color4.White, 500);
                        break;
                }

                lastBeatNoLoop = currentBeatNoLoop;
            }


            if (!InputManager.IsPressed)
            {
                incrementalSpeed = 0.2f + incrementalSpeed * 0.8f;
                Clock.IncrementManual(incrementalSpeed);
                if (incrementalSpeed < 0) spriteManager.ResetFirstTransformations();
            }

            playfieldBackground.Velocity = 0.4f * incrementalSpeed;
            base.Update();
        }

        protected override void UpdateStream()
        {
        }
    }
}
