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

namespace osum.GameModes.Store
{
    internal class PackPanel : pSpriteCollection
    {
        internal pDrawable s_BackingPlate;
        internal pText s_Text;
        internal pText s_Price;
        internal pSprite s_PriceBackground;
        internal pSprite s_Thumbnail;

        float base_depth = 0.6f;

        static Color4 colourNormal = new Color4(50, 50, 50, 255);
        static Color4 colourHover = new Color4(28, 139, 242, 255);
        static Color4 colourHover2 = new Color4(0, 77, 164, 255);

        internal const int PANEL_HEIGHT = 60;

        internal float Height = PANEL_HEIGHT + 4;

        public PackPanel(string packTitle, string price, EventHandler action)
        {
            //base_depth += 0.001f * index;

            s_BackingPlate = new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSizeFixedWidth.Width, PANEL_HEIGHT), true, base_depth, colourNormal);
            Sprites.Add(s_BackingPlate);


            s_BackingPlate.OnClick += delegate {
                if (isDownloading) return;

                s_BackingPlate.FadeColour(colourHover2, 80);
                s_BackingPlate.HandleInput = false;

                s_PriceBackground.FadeIn(100);
                s_Price.FadeColour(Color4.White, 100);
                s_PriceBackground.HandleInput = true;

                if (!isPreviewing)
                    songPreviewBacks[0].Click();
            };
            s_BackingPlate.OnClick += action;

            s_BackingPlate.HandleClickOnUp = true;

            s_BackingPlate.OnHover += delegate {
                if (isDownloading) return;
                s_BackingPlate.FadeColour(colourHover2, 80);
            };
            s_BackingPlate.OnHoverLost += delegate {
                if (isDownloading) return;
                s_BackingPlate.FadeColour(colourNormal, 80);
            };

            s_Text = new pText(packTitle, 32, Vector2.Zero, new Vector2(GameBase.BaseSizeFixedWidth.Width, PANEL_HEIGHT), base_depth + 0.01f, true, Color4.White, false);
            s_Text.Bold = true;
            s_Text.Offset = new Vector2(74, 14);
            if (s_Text.Texture != null)
                Sprites.Add(s_Text);

            s_PriceBackground = new pSprite(TextureManager.Load(OsuTexture.songselect_store_buy_background), FieldTypes.StandardSnapRight, OriginTypes.TopRight, ClockTypes.Mode, Vector2.Zero, base_depth + 0.01f, true, Color4.White);
            s_PriceBackground.Alpha = 0;
            s_PriceBackground.OnClick += OnPurchase;
            s_PriceBackground.Offset = new Vector2(1, 1);
            Sprites.Add(s_PriceBackground);

            s_Price = new pText(price, 52, Vector2.Zero, Vector2.Zero, base_depth + 0.02f, true, new Color4(255, 255, 255, 128), false);
            s_Price.TextAlignment = TextAlignment.Left;
            s_Price.Origin = OriginTypes.TopCentre;
            s_Price.Field = FieldTypes.StandardSnapRight;
            s_Price.Offset = new Vector2(80, 0);
            Sprites.Add(s_Price);

            s_Thumbnail = new pSprite(TextureManager.Load(OsuTexture.songselect_thumbnail), Vector2.Zero) { DrawDepth = base_depth + 0.02f };
            s_Thumbnail.Offset = new Vector2(2, 2);
            Sprites.Add(s_Thumbnail);
        }

        internal int BeatmapCount { get { return filenames.Count; } }

        int currentDownload = 0;
        bool isDownloading;

        void OnPurchase(object sender, EventArgs e)
        {
            if (isPreviewing)
                StoreMode.ResetAllPreviews(true);

            isDownloading = true;

            startNextDownload();

            s_PriceBackground.FadeOut(100);
            s_Price.FadeOut(100);

            s_BackingPlate.HandleInput = false;

            songPreviewButtons.ForEach(b => b.FadeOut(100));
            songPreviewBacks.ForEach(b =>
            {
                b.HandleInput = false;
                b.Scale.X = 0;
                b.Colour = Color4.DarkBlue;
            });

            StoreMode.PurchaseInitiated(this);
        }

        void startNextDownload()
        {
            Downloading = true;

            string filename = filenames[currentDownload];
            string path = SongSelectMode.BeatmapPath + "/" + filename;
            string downloadPath = "http://d.osu.ppy.sh/osum/" + s_Text.Text + "/" + filename;

            Console.WriteLine("Downloading " + downloadPath);

            FileNetRequest fnr = new FileNetRequest(path, downloadPath);
            fnr.onFinish += delegate
            {
                currentDownload++;
                if (currentDownload < filenames.Count)
                    startNextDownload();
                else
                {
                    Downloading = false;
                    GameBase.Scheduler.Add(delegate { StoreMode.DownloadComplete(this); });
                }

            };

            fnr.onUpdate += delegate(object sender, long current, long total) {
                songPreviewBacks[currentDownload].Scale.X = GameBase.BaseSize.Width * ((float)current/total);
            };

            NetManager.AddRequest(fnr);
        }

        List<pDrawable> songPreviewBacks = new List<pDrawable>();
        List<pSprite> songPreviewButtons = new List<pSprite>();
        List<string> filenames = new List<string>();

        bool isPreviewing;

        DataNetRequest previewRequest;

        internal void ResetPreviews()
        {
            if (previewRequest != null)
            {
                previewRequest.Abort();
                previewRequest = null;
            }

            if (isDownloading) return;

            isPreviewing = false;

            foreach (pDrawable p in songPreviewBacks)
            {
                p.FadeColour(Color4.Black, 200);
                p.TagNumeric = 0;
            }

            foreach (pSprite p in songPreviewButtons)
                p.Texture = TextureManager.Load(OsuTexture.songselect_audio_preview);

            s_BackingPlate.FadeColour(colourNormal, 200);
            s_BackingPlate.HandleInput = true;

            s_PriceBackground.FadeOut(100);
            s_Price.FadeColour(new Color4(255, 255, 255, 128),100);
            s_PriceBackground.HandleInput = false;
        }

        internal void Add(string filename)
        {
            pSprite preview = new pSprite(TextureManager.Load(OsuTexture.songselect_audio_preview), Vector2.Zero) { DrawDepth = base_depth + 0.02f, Origin = OriginTypes.Centre };
            preview.Offset = new Vector2(68, Height + 20);
            Sprites.Add(preview);
            songPreviewButtons.Add(preview);

            pRectangle back = new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSizeFixedWidth.Width, 40), true, base_depth, Color4.Black);
            back.HandleClickOnUp = true;

            back.OnHover += delegate { if (back.TagNumeric != 1) back.FadeColour(new Color4(40,40,40,255),200); };
            back.OnHoverLost += delegate { if (back.TagNumeric != 1) back.FadeColour(Color4.Black,200); };

            back.OnClick += delegate(object sender, EventArgs e) {

                bool isPausing = back.TagNumeric == 1;

                StoreMode.ResetAllPreviews(isPausing);

                if (isPausing) return;

                AudioEngine.Music.Stop(true);

                previewRequest = new DataNetRequest("http://d.osu.ppy.sh/osum/" + s_Text.Text + "/" + filename + ".mp3");
                previewRequest.onFinish += delegate(Byte[] data, Exception ex) {
                    GameBase.Scheduler.Add(delegate {
                        if (ex != null)
                        {
                            StoreMode.ResetAllPreviews(true);
                            GameBase.Notify("Failed to load song preview.\nPlease check your internet connection.");
                        }

                        StoreMode.PlayPreview(data);
                        preview.Transformations.Clear();
                        preview.Rotation = 0;
                        preview.Texture = TextureManager.Load(OsuTexture.songselect_audio_preview_pause);
                    });
                };
                NetManager.AddRequest(previewRequest);

                back.FadeColour(colourHover,0);
                back.Transform(new Transformation(TransformationType.VectorScale, new Vector2(back.Scale.X,0), back.Scale,Clock.ModeTime, Clock.ModeTime + 200, EasingTypes.In));
                back.TagNumeric = 1;

                preview.Texture = TextureManager.Load(OsuTexture.songselect_audio_preview_load);
                preview.Transform(new Transformation(TransformationType.Rotation, 0, 500, Clock.ModeTime, Clock.ModeTime + 100000));
                isPreviewing = true;

                StoreMode.EnsureVisible(s_BackingPlate);

                s_BackingPlate.Click(false);
            };

            songPreviewBacks.Add(back);

            back.Origin = OriginTypes.CentreLeft;
            back.Offset = new Vector2(0, Height + 20);
            Sprites.Add(back);

            filenames.Add(filename);

            Regex r = new Regex(@"(.*) - (.*) \((.*)\)");
            Match m = r.Match(Path.GetFileNameWithoutExtension(filename));

            pText artist = new pText(m.Groups[1].Value, 26, Vector2.Zero, Vector2.Zero, base_depth + 0.01f, true, Color4.SkyBlue, false);
            artist.Bold = true;
            artist.Offset = new Vector2(110, Height + 4);
            Sprites.Add(artist);

            pText title = new pText(m.Groups[2].Value, 26, Vector2.Zero, Vector2.Zero, base_depth + 0.01f, true, Color4.White, false);

            title.Offset = new Vector2(120 + artist.MeasureText().X / GameBase.BaseToNativeRatio, Height + 4);
            Sprites.Add(title);


            Height += 43;
        }

        public bool Downloading { get; private set; }
    }
}
