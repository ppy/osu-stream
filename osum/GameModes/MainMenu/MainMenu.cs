using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.GameModes;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;
using System.Drawing;
using osum.Audio;
using osum.Support;
using osum.Graphics;
using System.IO;
using osum.Graphics.Drawables;
using osum.UI;
using osum.Resources;
#if iOS
using OpenTK.Graphics.ES11;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;

using TextureTarget = OpenTK.Graphics.ES11.All;
using TextureParameterName = OpenTK.Graphics.ES11.All;
using EnableCap = OpenTK.Graphics.ES11.All;
using BlendingFactorSrc = OpenTK.Graphics.ES11.All;
using BlendingFactorDest = OpenTK.Graphics.ES11.All;
using PixelStoreParameter = OpenTK.Graphics.ES11.All;
using VertexPointerType = OpenTK.Graphics.ES11.All;
using ColorPointerType = OpenTK.Graphics.ES11.All;
using ClearBufferMask = OpenTK.Graphics.ES11.All;
using TexCoordPointerType = OpenTK.Graphics.ES11.All;
using BeginMode = OpenTK.Graphics.ES11.All;
using MatrixMode = OpenTK.Graphics.ES11.All;
using PixelInternalFormat = OpenTK.Graphics.ES11.All;
using PixelFormat = OpenTK.Graphics.ES11.All;
using PixelType = OpenTK.Graphics.ES11.All;
using ShaderType = OpenTK.Graphics.ES11.All;
using VertexAttribPointerType = OpenTK.Graphics.ES11.All;
using ProgramParameter = OpenTK.Graphics.ES11.All;
using ShaderParameter = OpenTK.Graphics.ES11.All;
using ErrorCode = OpenTK.Graphics.ES11.All;
using TextureEnvParameter = OpenTK.Graphics.ES11.All;
using TextureEnvTarget =  OpenTK.Graphics.ES11.All;
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using osum.Input;
#endif

namespace osum.GameModes
{
    class MainMenu : GameMode
    {
        pSprite osuLogo;
        pSprite osuLogoGloss;

        List<pSprite> explosions = new List<pSprite>();

        internal SpriteManager spriteManagerBehind = new SpriteManager();

        MenuState State = MenuState.Logo;

        static bool firstDisplay = true;

        public override void Initialize()
        {
            int initial_display = firstDisplay ? 2950 : 0;

            //spriteManagerBehind.Add(menuBackground);

            menuBackgroundNew = new MenuBackground();
            menuBackgroundNew.Clocking = ClockTypes.Mode;

            const int logo_stuff_v_offset = -20;

            Transformation logoBounce = new TransformationBounce(initial_display, initial_display + 2000, 0.625f, 0.4f, 2);

            osuLogo = new pSprite(TextureManager.Load(OsuTexture.menu_osu), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, logo_stuff_v_offset), 0.9f, true, Color4.White);
            osuLogo.Transform(logoBounce);
            osuLogo.OnClick += osuLogo_OnClick;
            menuBackgroundNew.Add(osuLogo);

            //gloss
            osuLogoGloss = new pSprite(TextureManager.Load(OsuTexture.menu_gloss), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, logo_stuff_v_offset), 0.91f, true, Color4.White);
            osuLogoGloss.Additive = true;
            menuBackgroundNew.Add(osuLogoGloss);

            Transformation explosionFade = new TransformationF(TransformationType.Fade, 0, 1, initial_display + 500, initial_display + 700);

            pSprite explosion = new pSprite(TextureManager.Load(OsuTexture.menu_circle), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(-90 * 0.625f, -90 * 0.625f + logo_stuff_v_offset), 0.8f, true, new Color4(112, 58, 144, 255));
            explosion.ScaleScalar = sizeForExplosion(0);
            explosion.Transform(explosionFade);
            explosions.Add(explosion);
            menuBackgroundNew.Add(explosion);

            explosion = new pSprite(TextureManager.Load(OsuTexture.menu_circle), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(170 * 0.625f, 10 * 0.625f + logo_stuff_v_offset), 0.8f, true, new Color4(242, 25, 138, 255));
            explosion.ScaleScalar = sizeForExplosion(1);
            explosion.Transform(explosionFade);
            explosions.Add(explosion);
            menuBackgroundNew.Add(explosion);

            explosion = new pSprite(TextureManager.Load(OsuTexture.menu_circle), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(-130 * 0.625f, 88 * 0.625f + logo_stuff_v_offset), 0.8f, true, new Color4(254, 148, 4, 255));
            explosion.ScaleScalar = sizeForExplosion(2);
            explosion.Transform(explosionFade);
            explosions.Add(explosion);
            menuBackgroundNew.Add(explosion);

            Transformation fadeIn = new TransformationF(TransformationType.Fade, 0, 1, initial_display, initial_display);
            spriteManager.Sprites.ForEach(s => s.Transform(fadeIn));

            stream = new pSprite(TextureManager.Load(OsuTexture.menu_stream), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, 180), 0.95f, true, Color4.White);
            stream.Transform(new TransformationF(TransformationType.Fade, 0, 1, initial_display + 900, initial_display + 1300));
            spriteManager.Add(stream);

            additiveStream = stream.Clone();
            additiveStream.Additive = true;
            additiveStream.DrawDepth = 0.96f;
            additiveStream.Transform(new TransformationF(TransformationType.Fade, 0, 1, initial_display + 1000, initial_display + 1200) { Looping = true, LoopDelay = 5000 });
            additiveStream.Transform(new TransformationF(TransformationType.Fade, 1, 0, initial_display + 1200, initial_display + 2000) { Looping = true, LoopDelay = 4400 });
            spriteManager.Add(additiveStream);

            osuLogoSmall = new pSprite(TextureManager.Load(OsuTexture.menu_logo), FieldTypes.Standard, OriginTypes.TopLeft, ClockTypes.Mode, new Vector2(5, 5), 0.9f, true, Color4.White);
            osuLogoSmall.OnClick += delegate
            {
                if (State == MenuState.Select)
                    Director.ChangeMode(OsuMode.MainMenu);
            };
            osuLogoSmall.Alpha = 0;
            spriteManager.Add(osuLogoSmall);

            NewsButton = new NewsButton();
            spriteManager.Add(NewsButton);
            NewsButton.OnClick += new EventHandler(newsButton_OnClick);
            if (GameBase.Config.GetValue<int>("NewsLastRead", 0) == 0)
                NewsButton.HasNews = true;
            NewsButton.Alpha = 0;

            menuBackgroundNew.Transform(fadeIn);

            osuLogo.Transform(fadeIn);

            InitializeBgm();

            menuBackgroundNew.Transform(new TransformationBounce(initial_display, initial_display + 2000, menuBackgroundNew.ScaleScalar, 0.8f, 2));

            if (firstDisplay)
            {
                pDrawable whiteLayer = pSprite.FullscreenWhitePixel;
                whiteLayer.Alpha = 0;
                whiteLayer.Clocking = ClockTypes.Mode;
                //whiteLayer.Additive = true;
                spriteManager.Add(whiteLayer);

                whiteLayer.Transform(new TransformationF(TransformationType.Fade, 0, 0.125f, 800, initial_display - 200));
                whiteLayer.Transform(new TransformationF(TransformationType.Fade, 0.125f, 1f, initial_display - 200, initial_display));
                whiteLayer.Transform(new TransformationF(TransformationType.Fade, 1, 0, initial_display, initial_display + 1200, EasingTypes.In));

                pSprite headphones = new pSprite(TextureManager.Load(OsuTexture.menu_headphones), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, 0), 0.9f, false, Color4.White);
                headphones.Additive = true;
                headphones.Transform(new TransformationF(TransformationType.Fade, 0, 1, 50, 200));
                headphones.Transform(new TransformationF(TransformationType.Fade, 1, 1, 1000, initial_display));
                spriteManager.Add(headphones);

                GameBase.Scheduler.Add(delegate
                {
                    AudioEngine.PlaySample(OsuSamples.MainMenu_Intro);
                    GameBase.Scheduler.Add(delegate { AudioEngine.Music.Play(); }, 2950);
                }, true);

                if (GameBase.Config.GetValue<bool>("firstrun", true))
                {
                    Notification notification = new Notification(
                        LocalisationManager.GetString(OsuString.FirstRunWelcome),
                        LocalisationManager.GetString(OsuString.FirstRunTutorial),
                        NotificationStyle.YesNo,
                        delegate(bool answer)
                        {
                            if (answer)
                            {
                                AudioEngine.PlaySample(OsuSamples.MenuHit);
                                Director.ChangeMode(OsuMode.Tutorial);
                            }
                            GameBase.Config.SetValue<bool>("firstrun", false);
                            GameBase.Config.SaveConfig();
                        });

                    GameBase.Scheduler.Add(delegate
                    {
                        GameBase.Notify(notification);
                    }, initial_display + 1500);
                }

            }
            else
            {
                if (Director.LastOsuMode == OsuMode.Tutorial)
                    AudioEngine.Music.SeekTo(0);
                AudioEngine.Music.Play();
            }

            string username = GameBase.Config.GetValue<string>("username", null);
            if (username != null)
            {
                bool hasAuth = GameBase.HasAuth;
                pText usernameText = new pText(username + (hasAuth ? string.Empty : " (guest)"), 20, new Vector2(hasAuth ? 35 : 2, 0), 1, true, Color4.White);
                usernameText.TextShadow = true;
                spriteManager.Add(usernameText);

                if (hasAuth)
                {
                    pSpriteWeb avatar = new pSpriteWeb(@"http://api.twitter.com/1/users/profile_image/" + username);
                    spriteManager.Add(avatar);
                }
            }

            firstDisplay = false;
        }

        void newsButton_OnClick(object sender, EventArgs e)
        {
            int time = UnixTimestamp.FromDateTime(DateTime.Now);

            GameBase.Instance.ShowWebView(@"http://osustream.com/p/news", "News");
            GameBase.Config.SetValue<int>("NewsLastRead", time);

            NewsButton.HasNews = false;
        }

        void osuLogo_OnClick(object sender, EventArgs e)
        {
            State = MenuState.Select;

            osuLogo.HandleInput = false;
            osuLogo.Transformations.Clear();

            AudioEngine.PlaySample(OsuSamples.MenuHit);

            menuBackgroundNew.BeAwesome();

            Transformation fadeIn = new TransformationF(TransformationType.Fade, 0, 0.98f, Clock.ModeTime + 1300, Clock.ModeTime + 1700);
            osuLogoSmall.Transform(fadeIn);

            Transformation move = new TransformationV(new Vector2(0, 50), Vector2.Zero, Clock.ModeTime + 500, Clock.ModeTime + 1000, EasingTypes.In);
            fadeIn = new TransformationF(TransformationType.Fade, 0, 0.98f, Clock.ModeTime + 500, Clock.ModeTime + 1000);
            NewsButton.Transform(fadeIn);
            NewsButton.Transform(move);

            osuLogo.Transformations.Clear();
            osuLogo.Transform(new TransformationF(TransformationType.Scale, osuLogo.ScaleScalar, osuLogo.ScaleScalar * 2.4f, Clock.ModeTime, Clock.ModeTime + 1300, EasingTypes.InDouble));
            osuLogo.Transform(new TransformationF(TransformationType.Rotation, osuLogo.Rotation, 0.35f, Clock.ModeTime, Clock.ModeTime + 1000, EasingTypes.In));

            osuLogoGloss.Transformations.Clear();
            osuLogoGloss.FadeOut(200);
            osuLogoGloss.Transform(new TransformationF(TransformationType.Scale, osuLogoGloss.ScaleScalar, osuLogoGloss.ScaleScalar * 2.4f, Clock.ModeTime, Clock.ModeTime + 1300, EasingTypes.InDouble));
            stream.FadeOut(150);
            additiveStream.FadeOut(150);

            osuLogo.FadeOut(800);

            explosions.ForEach(s => { s.Transformations.Clear(); s.FadeOut(100); });
        }



        /// <summary>
        /// Initializes the song select BGM and starts playing. Static for now so it can be triggered from anywhere.
        /// </summary>
        internal static bool InitializeBgm()
        {
            //Start playing song select BGM.
#if iOS
            bool didLoad = AudioEngine.Music.Load("Skins/Default/mainmenu.m4a", true);
#else
            bool didLoad = AudioEngine.Music.Load("Skins/Default/mainmenu.mp3", true);
#endif

            return didLoad;
        }

        public override void Dispose()
        {
            //we will never use these textures again (the "intro" sheet) so get rid of them for good.
            TextureManager.Dispose(OsuTexture.menu_headphones);

            menuBackgroundNew.Dispose();
            spriteManagerBehind.Dispose();
            base.Dispose();
        }

        double elapsedRotation;
        private pSprite menuOptions;
        private pSprite stream;

        int lastBgmBeat = 0;
        float between_beats = 375 / 4f;
        int offset = 0;
        const int bar = 8;
        private pDrawable additiveStream;
        private MenuBackground menuBackgroundNew;
        private pSprite osuLogoSmall;
        public NewsButton NewsButton;

        public override void Update()
        {
            osuLogoGloss.Rotation = -menuBackgroundNew.Rotation;

            if (AudioEngine.Music.IsElapsing)
            {
                elapsedRotation += Clock.ElapsedMilliseconds;
                osuLogo.Rotation += (float)(Math.Cos((elapsedRotation) / 1000f) * 0.0001 * Clock.ElapsedMilliseconds);

                TransformationF tr = menuBackgroundNew.Transformations.Find(t => t.Type == TransformationType.Rotation) as TransformationF;

                float rCh = -(float)(Math.Cos((elapsedRotation + 500) / 3000f) * 0.00002 * Clock.ElapsedMilliseconds);
                if (tr != null)
                    tr.EndFloat += rCh;
                else
                    menuBackgroundNew.Rotation += rCh;

                tr = menuBackgroundNew.Transformations.Find(t => t.Type == TransformationType.Scale) as TransformationF;

                float sCh = (float)(Math.Cos((elapsedRotation + 500) / 4000f) * 0.00002 * Clock.ElapsedMilliseconds);
                if (tr != null)
                    tr.EndFloat += sCh;
                else
                    menuBackgroundNew.ScaleScalar += sCh;
            }

            if (State != MenuState.Select)
                updateBeat();

            base.Update();
            spriteManagerBehind.Update();
            menuBackgroundNew.Update();

            osuLogoGloss.ScaleScalar = osuLogo.ScaleScalar;
        }

        private void explode(int beat)
        {
            pDrawable explosion = explosions[beat];

            if (explosion.Alpha == 0 && !menuBackgroundNew.IsAwesome && osuLogo.ScaleScalar >= 0.6f)
            {
                explosion.ScaleScalar *= 1.3f;
                explosion.FadeIn(100);
            }

            if (!menuBackgroundNew.IsAwesome)
            {
                float adjust = beat == 0 ? 0.95f : (beat == 1 ? 1.05f : 1);
                if (osuLogo.Transformations.Count != 0 && osuLogo.Transformations[0] is TransformationBounce)
                    ((TransformationBounce)osuLogo.Transformations[0]).EndFloat *= adjust;
                else
                {
                    osuLogo.ScaleScalar *= adjust;
                    osuLogo.ScaleTo(0.625f, 500, EasingTypes.In);
                }
            }

            explosion.FlashColour(ColourHelper.Lighten2(explosion.Colour, 0.4f), 350);
            explosion.ScaleScalar *= 1.2f;
            explosion.ScaleTo(sizeForExplosion(beat), 400, EasingTypes.In);
        }

        private float sizeForExplosion(int beat)
        {
            return 0.7f - beat * 0.05f;
        }

        public override bool Draw()
        {
            spriteManagerBehind.Draw();
            menuBackgroundNew.Draw();

            base.Draw();



            //if (!Director.IsTransitioning)
            //	osuLogo.ScaleScalar = 1 + AudioEngine.Music.CurrentVolume/100;

            return true;
        }

        private void updateBeat()
        {
            int newBeat = (int)((Clock.AudioTime - offset) / between_beats);
            if (lastBgmBeat != newBeat)
            {
                switch (newBeat)
                {
                    case 0:
                    case 10:
                    case 16:
                    case 24:
                    case 26:
                    case 32:
                    case 42:
                    case 48:
                    case 56:
                    case 58:
                    case 64:
                    case 74:
                    case 80:
                    case 88:
                    case 90:
                    case 96:
                    case 106:
                    case 112:
                    case 120:
                    case 126:
                    case 128:
                    case 138:
                    case 144:
                    case 152:
                    case 154:
                    case 160:
                    case 170:
                    case 176:
                    case 184:
                    case 186:
                    case 192:
                    case 202:
                    case 208:
                    case 216:
                    case 218:
                    case 224:
                    case 234:
                    case 240:
                    case 248:
                    case 249:
                    case 250:
                    case 254:
                    case 256:
                    case 266:
                    case 272:
                    case 280:
                    case 282:
                    case 288:
                    case 298:
                    case 304:
                    case 312:
                    case 314:
                    case 320:
                    case 330:
                    case 336:
                    case 344:
                    case 346:
                    case 352:
                    case 362:
                    case 368:
                    case 376:
                    case 378:
                    case 382:
                    case 384:
                    case 394:
                    case 400:
                    case 408:
                    case 410:
                    case 416:
                    case 426:
                    case 432:
                    case 440:
                    case 442:
                    case 448:
                    case 458:
                    case 464:
                    case 472:
                    case 474:
                    case 480:
                    case 484:
                    case 488:
                    case 492:
                    case 496:
                    case 498:
                    case 500:
                    case 502:
                    case 504:
                    case 505:
                    case 506:
                    case 507:
                    case 508:
                    case 509:
                    case 510:
                    case 511:
                    case 608:
                    case 612:
                    case 616:
                    case 620:
                    case 624:
                    case 626:
                    case 628:
                    case 630:
                    case 632:
                    case 633:
                    case 634:
                    case 635:
                    case 636:
                    case 637:
                    case 638:
                    case 639:
                    case 640:
                    case 650:
                    case 656:
                    case 664:
                    case 666:
                    case 672:
                    case 682:
                    case 688:
                    case 696:
                    case 698:
                    case 704:
                    case 714:
                    case 720:
                    case 728:
                    case 730:
                    case 736:
                    case 746:
                    case 752:
                    case 760:
                    case 762:
                    case 766:
                    case 768:
                    case 778:
                    case 784:
                    case 792:
                    case 794:
                    case 800:
                    case 810:
                    case 816:
                    case 824:
                    case 826:
                    case 832:
                    case 842:
                    case 848:
                    case 856:
                    case 858:
                    case 864:
                    case 874:
                    case 880:
                    case 888:
                    case 890:
                    case 894:
                    case 896:
                    case 906:
                    case 912:
                    case 920:
                    case 922:
                    case 928:
                    case 938:
                    case 944:
                    case 952:
                    case 954:
                    case 960:
                    case 970:
                    case 976:
                    case 984:
                    case 986:
                    case 992:
                    case 1002:
                    case 1008:
                    case 1016:
                    case 1018:
                    case 1022:
                        explode(0);
                        break;
                }

                switch (newBeat)
                {
                    case 4:
                    case 12:
                    case 20:
                    case 28:
                    case 36:
                    case 44:
                    case 52:
                    case 60:
                    case 68:
                    case 76:
                    case 84:
                    case 92:
                    case 100:
                    case 108:
                    case 116:
                    case 123:
                    case 125:
                    case 132:
                    case 140:
                    case 148:
                    case 156:
                    case 164:
                    case 172:
                    case 180:
                    case 188:
                    case 196:
                    case 204:
                    case 212:
                    case 220:
                    case 228:
                    case 236:
                    case 244:
                    case 249:
                    case 252:
                    case 253:
                    case 260:
                    case 268:
                    case 276:
                    case 284:
                    case 292:
                    case 300:
                    case 308:
                    case 316:
                    case 324:
                    case 332:
                    case 340:
                    case 348:
                    case 356:
                    case 364:
                    case 372:
                    case 379:
                    case 381:
                    case 388:
                    case 396:
                    case 404:
                    case 412:
                    case 420:
                    case 428:
                    case 436:
                    case 444:
                    case 452:
                    case 460:
                    case 468:
                    case 476:
                    case 480:
                    case 484:
                    case 488:
                    case 492:
                    case 496:
                    case 498:
                    case 500:
                    case 502:
                    case 504:
                    case 505:
                    case 506:
                    case 507:
                    case 508:
                    case 509:
                    case 510:
                    case 511:
                    case 608:
                    case 612:
                    case 616:
                    case 620:
                    case 624:
                    case 626:
                    case 628:
                    case 630:
                    case 632:
                    case 633:
                    case 634:
                    case 635:
                    case 636:
                    case 637:
                    case 638:
                    case 639:
                    case 644:
                    case 652:
                    case 660:
                    case 668:
                    case 676:
                    case 684:
                    case 692:
                    case 700:
                    case 708:
                    case 716:
                    case 724:
                    case 732:
                    case 740:
                    case 748:
                    case 756:
                    case 763:
                    case 765:
                    case 772:
                    case 780:
                    case 788:
                    case 796:
                    case 804:
                    case 812:
                    case 820:
                    case 828:
                    case 836:
                    case 844:
                    case 852:
                    case 860:
                    case 868:
                    case 876:
                    case 884:
                    case 892:
                    case 900:
                    case 908:
                    case 916:
                    case 924:
                    case 932:
                    case 940:
                    case 948:
                    case 956:
                    case 964:
                    case 972:
                    case 980:
                    case 988:
                    case 996:
                    case 1004:
                    case 1012:
                    case 1017:
                    case 1020:
                    case 1021:
                        explode(1);
                        break;
                }

                switch (newBeat)
                {
                    case 130:
                    case 134:
                    case 138:
                    case 142:
                    case 146:
                    case 150:
                    case 154:
                    case 158:
                    case 162:
                    case 166:
                    case 170:
                    case 174:
                    case 178:
                    case 182:
                    case 186:
                    case 190:
                    case 194:
                    case 198:
                    case 202:
                    case 206:
                    case 210:
                    case 214:
                    case 218:
                    case 222:
                    case 226:
                    case 230:
                    case 234:
                    case 238:
                    case 258:
                    case 262:
                    case 266:
                    case 270:
                    case 274:
                    case 278:
                    case 282:
                    case 286:
                    case 290:
                    case 294:
                    case 298:
                    case 302:
                    case 306:
                    case 310:
                    case 314:
                    case 318:
                    case 322:
                    case 326:
                    case 330:
                    case 334:
                    case 338:
                    case 342:
                    case 346:
                    case 350:
                    case 354:
                    case 358:
                    case 362:
                    case 366:
                    case 374:
                    case 378:
                    case 386:
                    case 390:
                    case 394:
                    case 398:
                    case 402:
                    case 406:
                    case 410:
                    case 414:
                    case 418:
                    case 422:
                    case 426:
                    case 430:
                    case 434:
                    case 438:
                    case 442:
                    case 446:
                    case 450:
                    case 454:
                    case 458:
                    case 462:
                    case 466:
                    case 470:
                    case 474:
                    case 478:
                    case 482:
                    case 486:
                    case 490:
                    case 494:
                    case 642:
                    case 646:
                    case 650:
                    case 654:
                    case 658:
                    case 662:
                    case 666:
                    case 670:
                    case 674:
                    case 678:
                    case 682:
                    case 686:
                    case 690:
                    case 694:
                    case 698:
                    case 702:
                    case 706:
                    case 710:
                    case 714:
                    case 718:
                    case 722:
                    case 726:
                    case 730:
                    case 734:
                    case 738:
                    case 742:
                    case 746:
                    case 750:
                    case 758:
                    case 762:
                    case 770:
                    case 774:
                    case 778:
                    case 782:
                    case 786:
                    case 790:
                    case 794:
                    case 798:
                    case 802:
                    case 806:
                    case 810:
                    case 814:
                    case 818:
                    case 822:
                    case 826:
                    case 830:
                    case 834:
                    case 838:
                    case 842:
                    case 846:
                    case 850:
                    case 854:
                    case 858:
                    case 862:
                    case 866:
                    case 870:
                    case 874:
                    case 878:
                    case 886:
                    case 890:
                        explode(2);
                        break;
                }

                lastBgmBeat = newBeat;
            }
        }
    }

    enum MenuState
    {
        Logo,
        Select
    }
}