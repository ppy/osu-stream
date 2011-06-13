using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using osum.Support;
using osum.Graphics.Sprites;
using OpenTK;

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
using System.Drawing;
#endif


namespace osum.Graphics.Skins
{
    internal partial class SpriteSheetTexture
    {
        private string sheetName;
        internal string SheetName
        {
            get
            {
                return sheetName + "_" + GameBase.SpriteSheetResolution;
            }
            set
            {
                sheetName = value;
            }

        }

        internal int X;
        internal int Y;
        internal int Width;
        internal int Height;


        public SpriteSheetTexture(string name, int x, int y, int width, int height)
        {
            this.SheetName = name;
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
    }

    /// <summary>
    /// Handle the loading of textures from various sources.
    /// Caching, reuse, unloading and everything else.
    /// </summary>
    internal static partial class TextureManager
    {
        static Dictionary<OsuTexture, SpriteSheetTexture> textureLocations = new Dictionary<OsuTexture, SpriteSheetTexture>();

        public static void Initialize()
        {
            LoadSprites();

            GameBase.OnScreenLayoutChanged += delegate
            {
                DisposeDisposable();
            };
        }

        public static void Update()
        {
#if DEBUG
            DebugOverlay.AddLine("TextureManager: " + SpriteCache.Count + " cached " + DisposableTextures.Count + " dynamic");
#endif
        }


        public static void DisposeAll(bool force)
        {
            UnloadAll(force);

            SpriteCache.Clear();
            AnimationCache.Clear();
        }

        public static void UnloadAll(bool force)
        {
            foreach (pTexture p in SpriteCache.Values)
                p.UnloadTexture();

            if (force)
            {
                foreach (pTexture p in SpriteCachePermanent.Values)
                    p.UnloadTexture();
            }

            DisposeDisposable();
        }

        public static void DisposeDisposable()
        {
            foreach (pTexture p in DisposableTextures)
                p.Dispose();
            DisposableTextures.Clear();
            availableSurfaces = null;
        }

        public static void ReloadAll(bool forceUnload = true)
        {
            List<pTexture> cache = SpriteCache.Values.ToList();
            if (forceUnload) SpriteCache.Clear();

            foreach (pTexture p in cache)
            {
                if (forceUnload) p.TextureGl.Delete();
                p.ReloadIfPossible();
            }

            cache = SpriteCachePermanent.Values.ToList();
            if (forceUnload) SpriteCachePermanent.Clear();

            foreach (pTexture p in cache)
            {
                if (forceUnload) p.TextureGl.Delete();
                p.ReloadIfPossible();
            }

            PopulateSurfaces();

            GL.Clear(Constants.COLOR_BUFFER_BIT);
        }

        public static void RegisterDisposable(pTexture t)
        {
            if (t == null)
                throw new NullReferenceException("tried to add a null texture to DisposableTextures");
            DisposableTextures.Add(t);
        }

        internal static Dictionary<string, pTexture> SpriteCache = new Dictionary<string, pTexture>();
        internal static Dictionary<string, pTexture> SpriteCachePermanent = new Dictionary<string, pTexture>();
        internal static Dictionary<string, pTexture[]> AnimationCache = new Dictionary<string, pTexture[]>();
        internal static List<pTexture> DisposableTextures = new List<pTexture>();

        internal static pTexture Load(OsuTexture texture)
        {
            SpriteSheetTexture info;

            if (textureLocations.TryGetValue(texture, out info))
            {
                pTexture tex = Load(info.SheetName, info.SheetName.StartsWith("hit"));
                tex.OsuTextureInfo = texture; //set this so if we need to do a reload we will get the correct sheet.

                tex = tex.Clone(); //make a new instance because we may be using different coords.
                tex.X = info.X;
                tex.Y = info.Y;
                tex.Width = info.Width;
                tex.Height = info.Height;
                tex.OsuTextureInfo = texture;

                return tex;
            }
            else
            {
                //fallback to separate files (or don't!)
                return null;
            }
        }

        internal static pTexture Load(string name)
        {
            return Load(name, false);
        }

        internal static pTexture Load(string name, bool permanent)
        {
            pTexture texture;

            Dictionary<string, pTexture> destinationCache = permanent ? SpriteCachePermanent : SpriteCache;

            if (destinationCache.TryGetValue(name, out texture))
                return texture;


            string path = @"Skins/Default/" + name.Replace(".png", "") + (name.Contains('_') ? string.Empty : "_" + GameBase.SpriteSheetResolution);

            if (NativeAssetManager.Instance.FileExists(path))
            {
                texture = pTexture.FromFile(path);
                texture.Permanent = permanent;
                destinationCache.Add(name, texture);
                return texture;
            }

            return null;
        }

        internal static pTexture[] LoadAnimation(OsuTexture osuTexture, int count)
        {
            pTexture[] textures;

            string name = osuTexture.ToString();

            if (AnimationCache.TryGetValue(name, out textures))
                return textures;

            textures = new pTexture[count];

            for (int i = 0; i < count; i++)
                textures[i] = Load((OsuTexture)(osuTexture + i));

            AnimationCache.Add(name, textures);
            return textures;
        }

        internal static pTexture[] LoadAnimation(string name)
        {
            pTexture[] textures;
            pTexture texture;

            if (AnimationCache.TryGetValue(name, out textures))
                return textures;

            texture = Load(name + "-0");

            // if the texture is found, load all subsequent textures
            if (texture != null)
            {
                List<pTexture> list = new List<pTexture>();
                list.Add(texture);

                for (int i = 1; true; i++)
                {
                    texture = Load(name + "-" + i);

                    if (texture == null)
                    {
                        textures = list.ToArray();
                        AnimationCache.Add(name, textures);
                        return textures;
                    }

                    list.Add(texture);
                }
            }

            // if the texture can't be found, try without number
            texture = Load(name);

            if (texture != null)
            {
                textures = new[] { texture };
                AnimationCache.Add(name, textures);
                return textures;
            }

            return null;
        }

        static Queue<pTexture> availableSurfaces;


        static bool requireSurfaces;
        internal static bool RequireSurfaces
        {
            get
            {
                return requireSurfaces;
            }

            set
            {
                requireSurfaces = value;

                if (value)
                {
                    PopulateSurfaces();
                }
                else
                {
                    if (availableSurfaces != null)
                    {
                        while (availableSurfaces.Count > 0)
                            availableSurfaces.Dequeue().Dispose();
                        availableSurfaces = null;
                    }
                }
            }
        }

        internal static void PopulateSurfaces()
        {
            if (availableSurfaces == null && RequireSurfaces)
            {
                availableSurfaces = new Queue<pTexture>();

                int size = GameBase.NativeSize.Height;

                for (int i = 0; i < 4; i++)
                {
                    TextureGl gl = new TextureGl(size, size);
                    gl.SetData(IntPtr.Zero, 0, PixelFormat.Rgba);
                    pTexture t = new pTexture(gl, size, size);
                    t.BindFramebuffer();

#if iOS
                    //we need to draw once to screen on iOS in order to avoid lag the first frame they are drawn (some kind of internal optimisation?)
                    using (pSprite p = new pSprite(t, Vector2.Zero))
                        p.Draw();
#endif

                    RegisterDisposable(t);
                    availableSurfaces.Enqueue(t);
                }
            }
        }


        internal static pTexture RequireTexture(int width, int height)
        {
            PopulateSurfaces();

            if (availableSurfaces.Count == 0)
                return null;

            //todo: optimise FBO width/height. should only need two at max dimensions (or maybe even one)

            pTexture tex = availableSurfaces.Dequeue();
            tex.Width = width;
            tex.Height = height;

            return tex;
        }

        internal static void ReturnTexture(pTexture texture)
        {
            //todo: check if we should not nullify this and accept returned surfaces instead.
            if (availableSurfaces == null)
                return;

            if (!texture.IsDisposed && texture.TextureGl.Loaded)
                availableSurfaces.Enqueue(texture);
        }
    }

    internal enum TextureSource
    {
        Game,
        Skin,
        Beatmap
    }

    internal enum OsuTexture
    {
        None = 0,
        hit50,
        hit100,
        hit300,
        sliderfollowcircle,
        hit100k,
        hit300g,
        hit300k,
        hit0,
        sliderscorepoint,
        hitcircle,
        hitcircleoverlay,
        sliderarrow,
        holdcircle,
        followpoint,
        connectionline,
        default_0,
        default_1,
        default_2,
        default_3,
        default_4,
        default_5,
        default_6,
        default_7,
        default_8,
        default_9,
        default_comma,
        default_dot,
        default_percent,
        default_x,
        sliderb_0,
        sliderb_1,
        sliderb_2,
        sliderb_3,
        sliderb_4,
        sliderb_5,
        sliderb_6,
        sliderb_7,
        sliderb_8,
        sliderb_9,
        score_0,
        score_1,
        score_2,
        score_3,
        score_4,
        score_5,
        score_6,
        score_7,
        score_8,
        score_9,
        score_comma,
        score_dot,
        score_percent,
        score_x,
        playfield,
        songselect_header,
        stream_changing_down,
        stream_changing_up,
        stream_changing_arrow,
        songselect_footer,
        songselect_thumbnail,
        songselect_back_hexagon,
        songselect_back_arrow,
        songselect_tab_bar_background,
        songselect_mode_arrow,
        songselect_audio_preview,
        songselect_audio_pause,
        songselect_audio_play,
        songselect_mode_stream,
        songselect_mode_easy,
        songselect_mode_expert,
        songselect_tab_bar_play,
        songselect_tab_bar_rank,
        songselect_tab_bar_other,
        songselect_store_buy_background,
        scorebar_marker_hit,
        scorebar_marker,
        scorebar_colour,
        scorebar_background,
        spinner_background,
        spinner_circle,
        spinner_clear,
        spinner_spin,
        spinner_spm,
        menu_background,
        menu_osu,
        menu_circle,
        menu_stream,
        menu_gloss,
        menu_headphones,
        failed,
        play_menu_pull,
        play_menu_background,
        play_menu_restart,
        play_menu_quit,
        play_menu_continue,
        countdown_background,
        countdown_3,
        countdown_2,
        countdown_1,
        countdown_go,
        countdown_ready,
        mouse_burst,
        songselect_background,
        songselect_panel,
        songselect_panel_selected,
        menu_options,
        menu_item_background,
        menu_tutorial,
        menu_store,
        menu_play
    }
}
