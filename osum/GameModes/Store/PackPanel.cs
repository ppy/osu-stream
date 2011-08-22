using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using OpenTK.Graphics;
using osum.Graphics.Drawables;
using OpenTK;
using System.IO;
using System.Text.RegularExpressions;
using osum.Graphics.Renderers;
using osum.Graphics.Skins;
using osum.Helpers;
using osum.Audio;
using osu_common.Libraries.NetLib;
using osum.GameModes.SongSelect;
using osum.Resources;
using osum.GameplayElements;
using osum.GameplayElements.Beatmaps;

namespace osum.GameModes.Store
{
    public class PackPanel : pSpriteCollection
    {
        internal pDrawable s_BackingPlate;
        internal pDrawable s_BackingPlate2;
        internal pText s_Text;
        internal pText s_Price;
        internal pSprite s_PriceBackground;
        internal pSprite s_Thumbnail;

        float base_depth = 0.6f;

        static Color4 colourNormal = new Color4(50, 50, 50, 200);
        static Color4 colourHover = new Color4(28, 139, 242, 255);
        static Color4 colourHover2 = new Color4(0, 77, 164, 255);

        internal const int PANEL_HEIGHT = 60;

        internal float Height = PANEL_HEIGHT + 4;

        List<pDrawable> songPreviewBacks = new List<pDrawable>();
        List<pSprite> songPreviewButtons = new List<pSprite>();
        List<PackItem> packItems = new List<PackItem>();

        bool isPreviewing;
        DataNetRequest previewRequest;

        public string PackId;
        public bool IsFree;
        public bool Ready;
        public byte[] Receipt;

        public void SetPrice(string price, bool isFree = false)
        {
            if (s_LoadingPrice != null)
            {
                s_LoadingPrice.FadeOut(100);
                s_LoadingPrice.AlwaysDraw = false;
            }

            Ready = true;
            s_Price.Text = price;
            IsFree = isFree;
        }

        public PackPanel(string packTitle, string packId, bool free)
        {
            PackId = packId;
            IsFree = free;

            Ready = free;

            Sprites.Add(s_BackingPlate = new pSprite(TextureManager.Load(OsuTexture.songselect_panel), Vector2.Zero)
            {
                DrawDepth = base_depth,
                Colour = new Color4(255, 255, 255, 170),
                HandleClickOnUp = true
            });

            s_BackingPlate.OnClick += delegate
            {
                if (Downloading) return;

                s_BackingPlate.HandleInput = false;

                s_PriceBackground.FadeIn(100);
                s_PriceBackground.Transform(new TransformationF(TransformationType.Fade, 1, 0.6f, 100, 1500, EasingTypes.Out) { Looping = true, LoopDelay = 300 });
                s_PriceBackground.Transform(new TransformationF(TransformationType.Fade, 0.6f, 1, 1500, 1800, EasingTypes.In) { Looping = true, LoopDelay = 1400 });

                s_Price.FadeColour(Color4.White, 100);
                s_PriceBackground.HandleInput = true;

                s_BackingPlate2.FadeColour(Color4.White, 0);

                if (!isPreviewing)
                    songPreviewBacks[0].Click();
            };

            s_BackingPlate.OnHover += delegate
            {
                if (Downloading) return;

                s_BackingPlate.FadeOut(100, 0.01f);
                s_BackingPlate2.FadeColour(BeatmapPanel.BACKGROUND_COLOUR, 80);
            };

            s_BackingPlate.OnHoverLost += delegate
            {
                if (Downloading || isPreviewing) return;

                s_BackingPlate.FadeIn(60);
                s_BackingPlate2.FadeColour(Color4.Transparent, 100);

            };

            Sprites.Add(s_BackingPlate2 = new pSprite(TextureManager.Load(OsuTexture.songselect_panel_selected), Vector2.Zero)
            {
                DrawDepth = base_depth + 0.01f,
                Colour = new Color4(255, 255, 255, 0)
            });

            Sprites.Add(s_Text = new pText(packTitle, 32, Vector2.Zero, Vector2.Zero, base_depth + 0.02f, true, Color4.White, false)
            {
                Bold = true,
                Offset = new Vector2(100, 14)
            });

            Sprites.Add(s_PriceBackground = new pSprite(TextureManager.Load(OsuTexture.songselect_store_buy_background), FieldTypes.StandardSnapRight, OriginTypes.TopRight, ClockTypes.Mode, Vector2.Zero, base_depth + 0.02f, true, Color4.White)
            {
                Alpha = 0,
                Offset = new Vector2(1, 1)
            });
            s_PriceBackground.OnClick += OnPurchase;

            Sprites.Add(s_Price = new pText(free ? LocalisationManager.GetString(OsuString.Free) : null, 46, Vector2.Zero, Vector2.Zero, base_depth + 0.03f, true, new Color4(255, 255, 255, 128), false)
            {
                Origin = OriginTypes.TopCentre,
                Field = FieldTypes.StandardSnapRight,
                Offset = new Vector2(80, 0)
            });

            if (!free)
            {
                s_LoadingPrice = new pSprite(TextureManager.Load(OsuTexture.songselect_audio_preview), FieldTypes.StandardSnapRight, OriginTypes.Centre, ClockTypes.Mode, Vector2.Zero, base_depth + 0.04f, true, Color4.White)
                {
                    Offset = new Vector2(75,30),
                    ExactCoordinates = false
                };
                s_LoadingPrice.Transform(new TransformationF(TransformationType.Rotation, 0, MathHelper.Pi * 2, Clock.ModeTime, Clock.ModeTime + 2000) { Looping = true });
                Sprites.Add(s_LoadingPrice);
            }

            Sprites.Add(s_Thumbnail = new pSprite(TextureManager.Load(OsuTexture.songselect_thumbnail), Vector2.Zero)
            {
                DrawDepth = base_depth + 0.02f,
                Offset = new Vector2(2, 2)
            });
        }

        pSprite s_LoadingPrice;

        internal int BeatmapCount { get { return packItems.Count; } }

        int currentDownload = 0;
        
#if iOS
        const string PREFERRED_FORMAT = "m4a";
#else
        const string PREFERRED_FORMAT = "mp3";
#endif

        void OnPurchase(object sender, EventArgs e)
        {
            StoreMode.PurchaseInitiated(this);
        }

        public void Download()
        {
            Downloading = true;

            if (isPreviewing)
                StoreMode.ResetAllPreviews(true, true);

            startNextDownload();

            s_PriceBackground.Transformations.Clear();
            s_PriceBackground.FadeOut(100);
            s_Price.FadeOut(100);

            s_BackingPlate.HandleInput = false;

            songPreviewButtons.ForEach(b => b.FadeOut(100));
            songPreviewBacks.ForEach(b =>
            {
                b.HandleInput = false;
                b.Alpha = 0;
                b.Colour = Color4.OrangeRed;
            });
        }

        void startNextDownload()
        {
            Downloading = true;

            PackItem item = packItems[currentDownload];

            string path = SongSelectMode.BeatmapPath + "/" + item.Filename;

            string receipt64 = Receipt != null ? Convert.ToBase64String(Receipt) : "";

            string downloadPath = "http://www.osustream.com/dl/download2.php";
            string param = "filename=" + PackId + " - " + s_Text.Text + "/" + item.Filename + "&id=" + GameBase.Instance.DeviceIdentifier + "&recp=" + receipt64;
            if (item.UpdateChecksum != null)
                param += "&update=" + item.UpdateChecksum;
#if !DIST
            Console.WriteLine("Downloading " + downloadPath);
            Console.WriteLine("param " + param);
#endif

            FileNetRequest fnr = new FileNetRequest(path, downloadPath, "POST", param);
            fnr.onFinish += delegate
            {
                BeatmapDatabase.PopulateBeatmap(new Beatmap(path)); //record the new download in our local database.

                currentDownload++;
                if (currentDownload < packItems.Count)
                    startNextDownload();
                else
                {
                    Downloading = false;
                    GameBase.Scheduler.Add(delegate { StoreMode.DownloadComplete(this); });
                }

            };

            pDrawable back = songPreviewBacks[currentDownload];

            back.Transform(new TransformationF(TransformationType.Fade, 1, 0, Clock.ModeTime, Clock.ModeTime + 700) { Looping = true });

            fnr.onUpdate += delegate(object sender, long current, long total)
            {
                if (back.Alpha != 1)
                {
                    GameBase.Scheduler.Add(delegate
                    {
                        back.Transformations.Clear();
                        back.Alpha = 1;
                    }, true);
                }

                back.Scale.X = GameBase.BaseSize.Width * ((float)current / total);
            };

            NetManager.AddRequest(fnr);
        }

        internal void ResetPreviews(bool deselectPack)
        {
            if (previewRequest != null)
                previewRequest.Abort();

            isPreviewing = false;

            foreach (pSprite p in songPreviewButtons)
            {
                p.ExactCoordinates = true;
                p.Texture = TextureManager.Load(OsuTexture.songselect_audio_play);
                p.Transformations.Clear();
                p.Rotation = 0;
            }

            if (Downloading) return;

            foreach (pDrawable p in songPreviewBacks)
            {
                p.FadeColour(new Color4(40, 40, 40, 0), 200);
                p.TagNumeric = 0;
            }

            if (deselectPack)
            {
                s_BackingPlate.HandleInput = true;
                s_BackingPlate.FadeIn(100);
                s_BackingPlate2.FadeColour(Color4.Transparent, 100);

                s_PriceBackground.Transformations.Clear();
                s_PriceBackground.FadeOut(100);

                s_Price.FadeColour(new Color4(255, 255, 255, 128), 100);
                s_PriceBackground.HandleInput = false;
            }
        }

        internal void Add(PackItem item)
        {
            pSprite preview = new pSprite(TextureManager.Load(OsuTexture.songselect_audio_play), Vector2.Zero) { DrawDepth = base_depth + 0.02f, Origin = OriginTypes.Centre };
            preview.Offset = new Vector2(38, Height + 20);
            Sprites.Add(preview);
            songPreviewButtons.Add(preview);

            pRectangle back = new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSizeFixedWidth.Width, 40), true, base_depth, new Color4(40, 40, 40, 0));
            back.HandleClickOnUp = true;

            back.OnHover += delegate { if (back.TagNumeric != 1) back.FadeColour(new Color4(40, 40, 40, 255), 200); };
            back.OnHoverLost += delegate { if (back.TagNumeric != 1) back.FadeColour(new Color4(40, 40, 40, 0), 200); };

            back.OnClick += delegate(object sender, EventArgs e)
            {

                bool isPausing = back.TagNumeric == 1;

                StoreMode.ResetAllPreviews(isPausing, true);

                if (isPausing) return;

                AudioEngine.Music.Stop(true);

                AudioEngine.PlaySample(OsuSamples.MenuClick);

                if (previewRequest != null) previewRequest.Abort();

                string downloadPath = "http://www.osustream.com/dl/preview.php";
                string param = "filename=" + PackId + " - " + s_Text.Text + "/" + item.Filename + "&format=" + PREFERRED_FORMAT;
                previewRequest = new DataNetRequest(downloadPath,"POST",param);
                previewRequest.onFinish += delegate(Byte[] data, Exception ex)
                {
                    if (previewRequest.AbortRequested) return;

                    GameBase.Scheduler.Add(delegate
                    {
                        if (ex != null || data == null || data.Length < 10000)
                        {
                            StoreMode.ResetAllPreviews(true, false);
                            GameBase.Notify("Failed to load song preview.\nPlease check your internet connection.");
                            return;
                        }

                        preview.Transformations.Clear();
                        preview.Rotation = 0;

                        StoreMode.PlayPreview(data);
                        preview.ExactCoordinates = true;
                        preview.Texture = TextureManager.Load(OsuTexture.songselect_audio_pause);
                    });
                };
                NetManager.AddRequest(previewRequest);

                back.FadeColour(colourHover, 0, false);
                back.Transform(new TransformationV(new Vector2(back.Scale.X, 0), back.Scale, Clock.ModeTime, Clock.ModeTime + 200, EasingTypes.In) { Type = TransformationType.VectorScale });
                back.TagNumeric = 1;

                preview.Texture = TextureManager.Load(OsuTexture.songselect_audio_preview);
                preview.ExactCoordinates = false;
                preview.Transform(new TransformationF(TransformationType.Rotation, 0, MathHelper.Pi * 2, Clock.ModeTime, Clock.ModeTime + 1000) { Looping = true });
                isPreviewing = true;

                StoreMode.EnsureVisible(s_BackingPlate);

                s_BackingPlate.Click(false);
            };

            songPreviewBacks.Add(back);

            back.Origin = OriginTypes.CentreLeft;
            back.Offset = new Vector2(0, Height + 20);
            Sprites.Add(back);

            packItems.Add(item);

            string artistString;
            string titleString;

            if (item.Title == null)
            {
                Regex r = new Regex(@"(.*) - (.*) \((.*)\)");
                Match m = r.Match(Path.GetFileNameWithoutExtension(item.Filename));

                artistString = m.Groups[1].Value;
                titleString = m.Groups[2].Value;
            }
            else
            {
                artistString = item.Title.Substring(0, item.Title.IndexOf('-')).Trim();
                titleString = item.Title.Substring(item.Title.IndexOf('-') + 1).Trim();
            }

            pText artist = new pText(artistString, 26, Vector2.Zero, Vector2.Zero, base_depth + 0.01f, true, Color4.SkyBlue, false);
            artist.Bold = true;
            artist.Offset = new Vector2(80, Height + 4);
            Sprites.Add(artist);

            pText title = new pText(titleString, 26, Vector2.Zero, Vector2.Zero, base_depth + 0.01f, true, Color4.White, false);

            title.Offset = new Vector2(95 + artist.MeasureText().X / GameBase.BaseToNativeRatio, Height + 4);
            Sprites.Add(title);


            Height += 43;
        }

        public bool Downloading { get; private set; }
    }

    public class PackItem
    {
        public string Filename;
        public string UpdateChecksum;
        public string Title;

        public PackItem(string filename, string title, string updateChecksum = null)
        {
            Filename = filename;
            UpdateChecksum = updateChecksum;
            Title = title;

        }
    }
}
