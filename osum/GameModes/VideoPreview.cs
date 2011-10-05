using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;
using OpenTK.Graphics;
using osum.Helpers;
using osu_common.Libraries.Osz2;
using osum.GameplayElements.Beatmaps;
using osum.Graphics;
using osum.Audio;
using osum.Support;
using osu_common.Libraries.NetLib;
using System.IO;
using osum.Graphics.Drawables;
using osum.GameModes.SongSelect;
using osum.Resources;

namespace osum.GameModes
{
    class VideoPreview : GameMode
    {
        private MenuBackground mb;
        private pSprite osuLogo;
        private pSprite osuLogoGloss;

        SpriteManager songInfoSpriteManager = new SpriteManager();

        public static string DownloadLink;

        public override void Initialize()
        {
            mb = new MenuBackground();
            mb.DrawDepth = 0.1f;
            mb.Position = new Vector2(440, -230);
            mb.ScaleScalar = 0.5f;

            osuLogo = new pSprite(TextureManager.Load(OsuTexture.menu_osu), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, 0), 0.9f, true, Color4.White);
            osuLogo.ScaleScalar = 0.7f;
            mb.Add(osuLogo);

            //gloss
            osuLogoGloss = new pSprite(TextureManager.Load(OsuTexture.menu_gloss), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, 0), 0.91f, true, Color4.White);
            osuLogoGloss.Additive = true;
            osuLogoGloss.ScaleScalar = 0.7f;
            mb.Add(osuLogoGloss);

            if (DownloadLink != null)
            {
                downloadRequest = new DataNetRequest(DownloadLink);
                downloadRequest.onUpdate += dnr_onUpdate;
                downloadRequest.onFinish += dnr_onFinish;

                NetManager.AddRequest(downloadRequest);
            }
            else
                ShowMetadata();

            GameBase.ShowLoadingOverlay = true;

            loadingBackground = new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSize.Width + 1, 0), true, 0, new Color4(0, 97, 115, 180));
            loadingBackground.Additive = true;
            loadingBackground.Field = FieldTypes.StandardSnapBottomLeft;
            loadingBackground.Origin = OriginTypes.BottomLeft;

            spriteManager.Add(loadingBackground);

            spriteManager.Add(new BackButton(delegate { Director.ChangeMode(OsuMode.Store); }, false));
        }

        void dnr_onFinish(byte[] data, Exception e)
        {
            GameBase.Scheduler.Add(delegate {
                if (data == null || e != null)
                {
                    Director.ChangeMode(OsuMode.Store);
                    return;
                }

                downloadProgress = 1;

                bool success = true;

                try
                {
                    Player.Beatmap = new Beatmap() { Package = new MapPackage(new MemoryStream(downloadRequest.data)) };
                }
                catch
                {
                    success = false;
                }

                GameBase.ShowLoadingOverlay = false;

                loadingBackground.FadeOut(1000);
                songInfoSpriteManager.FadeInFromZero(400);

                if (success)
                {
                    ShowMetadata();

                    Player.Autoplay = true;
                    Director.ChangeMode(OsuMode.Play, new FadeTransition(5000, 500));
                }
                else
                {
                    GameBase.Notify(LocalisationManager.GetString(OsuString.InternetFailed), delegate { Director.ChangeMode(OsuMode.Store); });
                }
            });
        }

        private DataNetRequest downloadRequest;
        private pRectangle loadingBackground;

        float downloadProgress;
        void dnr_onUpdate(object sender, long current, long total)
        {
            downloadProgress = (float)current / total;
        }

        public override void Dispose()
        {
            if (downloadRequest != null)
                downloadRequest.Abort();

            DownloadLink = null;
            base.Dispose();

            if (Director.PendingOsuMode != OsuMode.Play)
            {
                //if we are exiting out early.
                if (Player.Beatmap != null)
                {
                    Player.Beatmap.Dispose();
                    Player.Beatmap = null;
                }
            }
        }

        private void ShowMetadata()
        {
            //song info

            songInfoSpriteManager.Position = new Vector2(20, 0);

            Beatmap beatmap = Player.Beatmap;

            //256x172
            float aspectAdjust = GameBase.BaseSize.Width / (256 * GameBase.SpriteToBaseRatio);

            pSprite thumbSprite = new pSpriteDynamic()
            {
                LoadDelegate = delegate
                {
                    pTexture thumb = null;
                    byte[] bytes = beatmap.GetFileBytes("thumb-256.jpg");
                    if (bytes != null)
                        thumb = pTexture.FromBytes(bytes);
                    return thumb;
                },
                DrawDepth = 0.49f,
                Field = FieldTypes.StandardSnapCentre,
                Origin = OriginTypes.Centre,
                ScaleScalar = aspectAdjust,
                Alpha = 0.3f
            };
            spriteManager.Add(thumbSprite);

            float vPos = 5;

            string unicodeTitle = beatmap.Package.GetMetadata(MapMetaType.TitleUnicode);
            string normalTitle = beatmap.Title;

            pText title = new pText(normalTitle, 40, new Vector2(0, vPos), 1, true, Color4.White)
            {
                Field = FieldTypes.Standard,
                Origin = OriginTypes.TopLeft,
                TextShadow = true
            };
            songInfoSpriteManager.Add(title);

            vPos += 40;

            string unicodeArtist = beatmap.Package.GetMetadata(MapMetaType.ArtistUnicode);

            pText artist = new pText("by " + beatmap.Package.GetMetadata(MapMetaType.ArtistFullName), 30, new Vector2(0, vPos), 1, true, Color4.LightYellow)
            {
                Field = FieldTypes.Standard,
                Origin = OriginTypes.TopLeft,
                TextShadow = true
            };

            songInfoSpriteManager.Add(artist);

            vPos += 50;

            string artistTwitter = beatmap.Package.GetMetadata(MapMetaType.ArtistTwitter);
            string artistWeb = beatmap.Package.GetMetadata(MapMetaType.ArtistUrl);

            if (artistWeb != null)
            {
                pText info = new pText("web: " + artistWeb, 26, new Vector2(0, vPos), 1, true, Color4.SkyBlue)
                {
                    Field = FieldTypes.Standard,
                    Origin = OriginTypes.TopLeft
                };

                info.OnClick += delegate
                {
                    GameBase.Instance.OpenUrl(artistWeb);
                };
                songInfoSpriteManager.Add(info);
                vPos += 30;
            }

            if (artistTwitter != null)
            {
                pText info = new pText("twitter: " + artistTwitter, 26, new Vector2(0, vPos), 1, true, Color4.SkyBlue)
                {
                    Field = FieldTypes.Standard,
                    Origin = OriginTypes.TopLeft
                };

                info.OnClick += delegate
                {
                    GameBase.Instance.OpenUrl(artistTwitter.Replace(@"@", @"http://twitter.com/"));
                };
                songInfoSpriteManager.Add(info);
                vPos += 40;
            }

            string unicodeSource = beatmap.Package.GetMetadata(MapMetaType.SourceUnicode);
            string normalSource = beatmap.Package.GetMetadata(MapMetaType.Source);

            if (normalSource != null && normalSource != "Original")
            {
                pText source = new pText("As seen in " + normalSource, 26, new Vector2(0, vPos), 1, true, Color4.LightYellow)
                {
                    Field = FieldTypes.Standard,
                    Origin = OriginTypes.TopLeft,
                    TextShadow = true
                };
                songInfoSpriteManager.Add(source);
            }

            pText mapper = new pText("Level design by " + beatmap.Creator, 26, new Vector2(30, 0), 1, true, Color4.White)
            {
                Field = FieldTypes.StandardSnapBottomRight,
                Origin = OriginTypes.BottomRight
            };
            songInfoSpriteManager.Add(mapper);
        }

        public override bool Draw()
        {
            base.Draw();
            mb.Draw();
            songInfoSpriteManager.Draw();
            return true;
        }

        public override void Update()
        {
            songInfoSpriteManager.Update();

            base.Update();
            mb.Update();

            loadingBackground.Scale.Y = loadingBackground.Scale.Y * 0.9f + 0.1f * (GameBase.BaseSize.Height * downloadProgress);
        }
    }
}
