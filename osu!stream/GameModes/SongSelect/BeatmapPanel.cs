using System;
using osum.Graphics.Sprites;
using osum.GameplayElements.Beatmaps;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;
using System.Text.RegularExpressions;
using osum.Graphics.Drawables;
using osum.Graphics.Renderers;
using osum.Graphics.Skins;
using osum.Audio;
using osum.Graphics;
using osum.GameplayElements;
using System.Collections.Generic;
namespace osum.GameModes.SongSelect
{
    internal class BeatmapPanel : pSpriteCollection
    {
        internal Beatmap Beatmap;

        internal pSprite s_BackingPlate;
        internal pSprite s_BackingPlate2;
        internal pText s_Text;
        internal pText s_TextArtist;
        internal pSprite s_Thumbnail;

        float base_depth = 0.4f;

        internal const int PANEL_HEIGHT = 60;
        public static Color4 BACKGROUND_COLOUR = new Color4(255, 255, 255, 240);
        private pSprite s_Star;
        private pSprite s_StarBg;

        internal BeatmapPanel(Beatmap beatmap, EventHandler action, int index)
        {
            base_depth += 0.0001f * index;

            s_BackingPlate = new pSprite(TextureManager.Load(OsuTexture.songselect_panel), Vector2.Zero)
            {
                DrawDepth = base_depth,
                Colour = new Color4(255, 255, 255, 170),
                Tag = this
            };

            Sprites.Add(s_BackingPlate);

            Beatmap = beatmap;

            if (action != null)
                s_BackingPlate.OnClick += action;

            s_BackingPlate.HandleClickOnUp = true;

            s_BackingPlate.OnHover += delegate
            {
                s_BackingPlate.FadeOut(100, 0.01f);
                s_BackingPlate2.FadeColour(BeatmapPanel.BACKGROUND_COLOUR, 80);
            };
            s_BackingPlate.OnHoverLost += delegate
            {
                s_BackingPlate.FadeIn(60);
                s_BackingPlate2.FadeColour(Color4.Transparent, 100);
            };

            s_Text = new pText(string.Empty, 26, Vector2.Zero, Vector2.Zero, 0.5f, true, Color4.White, false);
            s_Text.Bold = true;
            s_Text.Offset = new Vector2(100, 0);
            Sprites.Add(s_Text);

            s_TextArtist = new pText(string.Empty, 26, Vector2.Zero, Vector2.Zero, 0.51f, true, Color4.OrangeRed, false);
            s_TextArtist.Offset = new Vector2(100, 29);
            Sprites.Add(s_TextArtist);

            pTexture thumb = null;

            float starCount = 0;

            if (beatmap != null)
            {
                try
                {
                    s_Text.Text = beatmap.Title;
                    s_TextArtist.Text = (beatmap.DifficultyStars > 0 ? " [osu!stream] " : "[PC版] ") + beatmap.Artist;
                    starCount = beatmap.DifficultyStars / 2f;
                }
                catch
                {
                    //could fail due to corrupt package.
                }
            }
            else
            {
                thumb = TextureManager.Load(OsuTexture.songselect_thumb_dl);
            }

            if (thumb != null)
                s_Thumbnail = new pSprite(thumb, Vector2.Zero) { DrawDepth = base_depth + 0.02f };
            else
                s_Thumbnail = new pSpriteDynamic() { LoadDelegate = GetThumbnail, DrawDepth = 0.49f };

            s_Thumbnail.AlphaBlend = false;
            s_Thumbnail.Offset = new Vector2(8.5f, 3.8f);
            Sprites.Add(s_Thumbnail);

            s_BackingPlate2 = new pSprite(TextureManager.Load(OsuTexture.songselect_panel_selected), Vector2.Zero)
            {
                DrawDepth = base_depth + 0.01f,
                Colour = new Color4(255, 255, 255, 0)
            };

            Sprites.Add(s_BackingPlate2);

            if (beatmap != null)
            {
                if (starCount > 0)
                {
                    s_StarBg = new pSprite(TextureManager.Load(OsuTexture.difficulty_bar_bg), Vector2.Zero)
                    {
                        Origin = OriginTypes.BottomLeft,
                        Field = FieldTypes.StandardSnapRight,
                        DrawDepth = base_depth + 0.06f,
                        Offset = new Vector2(174, PANEL_HEIGHT)
                    };
                    Sprites.Add(s_StarBg);

                    s_Star = new pSprite(TextureManager.Load(OsuTexture.difficulty_bar_colour), Vector2.Zero)
                    {
                        Origin = OriginTypes.BottomLeft,
                        Field = FieldTypes.StandardSnapRight,
                        DrawDepth = base_depth + 0.07f,
                        Offset = new Vector2(174, PANEL_HEIGHT)
                    };

                    if (starCount == 0)
                        //always use zero-width for no stars (even though this should not ever happen) to avoid single-pixel glitching.
                        s_Star.DrawWidth = 0;
                    else if (starCount < 5)
                    {
                        const int border = 2;
                        s_Star.DrawWidth = (int)((s_Star.DrawWidth - border * 2) * starCount / 5f) + border;
                    }

                    Sprites.Add(s_Star);
                }

                foreach (DifficultyScoreInfo diffInfo in Beatmap.BeatmapInfo.DifficultyScores.Values)
                {
                    if (diffInfo.HighScore != null)
                    {
                        int offset = 0;
                        switch (diffInfo.difficulty)
                        {
                            case Difficulty.Easy:
                                offset = 90;
                                break;
                            case Difficulty.Normal:
                                offset = 50;
                                break;
                            case Difficulty.Expert:
                                offset = 10;
                                break;
                        }

                        pSprite rankingSprite = new pSprite(diffInfo.HighScore.RankingTextureTiny, Vector2.Zero)
                        {
                            Origin = OriginTypes.TopRight,
                            Field = FieldTypes.StandardSnapRight,
                            DrawDepth = base_depth + 0.08f,
                            Offset = new Vector2(offset, 2)
                        };
                        Sprites.Add(rankingSprite);
                        rankSprites.Add(rankingSprite);
                    }
                }
            }
        }

        List<pDrawable> rankSprites = new List<pDrawable>();
        public bool NewSection;

        private pTexture GetThumbnail()
        {
            pTexture thumb = null;
            byte[] bytes = Beatmap.GetFileBytes("thumb-128.jpg");
            if (bytes != null)
                thumb = pTexture.FromBytes(bytes);
            return thumb;
        }

        internal void HideRankings(bool instant)
        {
            foreach (pDrawable p in rankSprites)
                p.FadeOut(instant ? 0 : 300);
            if (s_Star != null)
            {
                s_StarBg.FadeOut(instant ? 0 : 300);
                s_Star.FadeOut(instant ? 0 : 300);
            }
        }

        internal void ShowRankings()
        {
            foreach (pDrawable p in rankSprites)
                p.FadeIn(200);
            if (s_Star != null)
            {
                s_StarBg.FadeIn(200);
                s_Star.FadeIn(200);
            }
        }
    }
}

