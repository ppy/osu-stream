using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;
using osum.Graphics.Sprites;
using Color = OpenTK.Graphics.Color4;
#if IPHONE
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

using osum.Graphics;
using osum;
using System.Collections.Generic;
using osu.Helpers;
using osum.GameplayElements;
using osum.Graphics.Skins;
using osu.Graphics.Primitives;
using System.Drawing;


namespace osu.Graphics.Renderers
{
    /// <summary>
    /// Class to handle drawing of Greg's enhanced sliders.
    /// </summary>
    internal abstract class SliderTrackRenderer
    {
        protected const int MAXRES = 24; // A higher MAXRES produces rounder endcaps at the cost of more vertices
        protected const int TEX_WIDTH = 128; // Please keep power of two

        // Make the quad overhang just slightly to avoid 1px holes between a quad and a wedge from rounding errors.
        protected const float QUAD_OVERLAP_FUDGE = 3.0e-4f;

        // If the peak vertex of a quad is at exactly 0, we get a crack running down the center of horizontal linear sliders.
        // We shift the vertex slightly off to the side to avoid this.
        protected const float QUAD_MIDDLECRACK_FUDGE = 1.0e-4f;

        // Bias to the number of polygons to render in a given wedge. Also ... fixes ... holes.
        protected const float WEDGE_COUNT_FUDGE = 0.0f; // Seems this fudge is unneeded YIPEE

        protected int bytesPerVertex;
        protected int numIndices_quad;
        protected int numIndices_cap;
        protected int numPrimitives_quad;
        protected int numPrimitives_cap;
        protected int numVertices_quad;
        protected int numVertices_cap;

        protected TextureGl[] textures_ogl;

        protected TextureGl grey_ogl;

        protected TextureGl multi_ogl;

        protected bool toon;
        protected Color border_colour;

        protected Vector3[] vertices_ogl;

        protected bool am_initted_geom = false;
        protected bool am_initted_tex = false;

        /// <summary>
        /// Performs all advanced computation needed to draw sliders in a particular beatmap.
        /// </summary>
        /// <param name="device">Shared GraphicsDevice</param>
        /// <param name="content">Shared ContentManager</param>
        /// <param name="outer_colours">Array of colours for the outside of the track. There should be one element for each combo colour in the map.</param>
        /// <param name="inner_colours">Array of colours for the inside of the track. There should be one element for each combo colour in the map.</param>
        /// <param name="border_colour">Single colour for the track's border.</param>
        /// <param name="toon">If true, the track gradient is made of four solid colours instead of a smooth gradient.</param>
        /// <param name="compute_geometry">If true, meshes will be computed, as opposed to keeping the ones from before. Leave false if you know this isn't the first time a map is being loaded.</param>
        internal void Init(Color[] outer_colours, Color[] inner_colours, Color border_colour)
        {
            this.border_colour = border_colour;

            int iColours = inner_colours.Length;
            if (outer_colours.Length != iColours) throw new ArgumentException("Outer colours and inner colours must match!");

            {
                if (!am_initted_geom) // TODO: Vertex buffers
                {
                    numVertices_quad = 6;
                    numPrimitives_quad = 4;
                    numIndices_quad = 6;

                    numVertices_cap = MAXRES + 2;
                    numPrimitives_cap = MAXRES;
                    numIndices_cap = 3 * MAXRES;

                    glCalculateCapMesh();

                    am_initted_geom = true;
                }

                if (iColours == 0) // Temporary catch for i332-triggered hard crash
                {
                    textures_ogl = new TextureGl[1];
                    textures_ogl[0] = glRenderSliderTexture(new Color(255, 255, 255, 255), new Color(255, 255, 255, 64), new Color(0, 0, 0, 64));
                }
                else
                {
                    textures_ogl = new TextureGl[iColours];

                    for (int x = 0; x < iColours; x++)
                    {
                        textures_ogl[x] = glRenderSliderTexture(border_colour, inner_colours[x], outer_colours[x]);
                    }
                }

                Color grey1, grey2;
                ComputeSliderColour(Color.Gray, out grey1, out grey2);
                //grey_ogl = glRenderSliderTexture(border_colour, grey1, grey2);
                multi_ogl = textures_ogl[0]; // Should be unneeded if things go right.

                am_initted_tex = true;
            }
        }

        /// <summary>
        /// The cap mesh is a half cone.
        /// </summary>
        private void glCalculateCapMesh()
        {
            vertices_ogl = new Vector3[numVertices_cap - 1];

            float maxRes = (float)MAXRES;
            float step = MathHelper.Pi / maxRes;

            vertices_ogl[0] = new Vector3(0.0f, -1.0f, 0.0f);

            for (int z = 1; z < MAXRES; z++)
            {
                float angle = (float)z * step;
                vertices_ogl[z] = new Vector3((float)(Math.Sin(angle)), -(float)(Math.Cos(angle)), 0.0f);
            }

            vertices_ogl[MAXRES] = new Vector3(0.0f, 1.0f, 0.0f);
        }

        protected abstract void glDrawQuad();

        protected abstract void glDrawHalfCircle(int count);

        /// <summary>
        /// Render a gradient into a 256x1 texture.
        /// </summary>
        protected abstract TextureGl glRenderSliderTexture(Color shadow, Color border, Color InnerColour, Color OuterColour, float aa_width, bool toon);


        /// <summary>
        /// This overload computes the outer/inner colours and AA widths, and has a hardcoded shadow colour.
        /// </summary>
        private TextureGl glRenderSliderTexture(Color border, Color InnerColour, Color OuterColour)
        {
            // This formula is used to keep sliders smooth for a wide variety of screen resolutions and circle sizes.
            // In most cases, it's set to equal a nice ratio of screen pixels, but it's constrained to within two values.
            // When circles are very large, the hitcircle(overlay).png textures become a bit fuzzy, so sharp sliders would look unnatural.
            // When they are tiny, we get the opposite problem. I confine it at 1/16th of a circle-radius, which is almost as wide as its border.
            float aa_width = Math.Min(Math.Max(0.25f / (DifficultyManager.HitObjectRadius * GameBase.GamefieldRatio), 0.015625f), 0.0625f);

            Color shadow = new Color(0, 0, 0, 128);

            return glRenderSliderTexture(shadow, border, InnerColour, OuterColour, aa_width, toon);
        }

        /// <summary>
        /// Recomputes the textures used on Tag Multi custom colour sliders.
        /// </summary>
        internal void SetTagCustomColour(Color color)
        {
            Color inner, outer;
            ComputeSliderColour(color, out inner, out outer);

            multi_ogl = glRenderSliderTexture(border_colour, inner, outer);
        }

        /// <summary>
        /// Helper function to turn a single colour into a lighter and darker shade for use with the slider's gradient.
        /// </summary>
        /// <param name="colour">HitObject colour</param>
        /// <param name="InnerColour">Track center</param>
        /// <param name="OuterColour">Track edges</param>
        internal static void ComputeSliderColour(Color colour, out Color InnerColour, out Color OuterColour)
        {
            Color col = new Color(colour.R, colour.G, colour.B, 230/255f); // Weird opengl transparency issue
            InnerColour = ColourHelper.Lighten2(col, 0.5f);
            OuterColour = ColourHelper.Darken(col, 0.1f);
        }

        /// <summary>
        /// Draws a slider to the active device using a cached texture.
        /// </summary>
        /// <param name="lineList">List of lines to use</param>
        /// <param name="globalRadius">Width of the slider</param>
        /// <param name="ColourIndex">Current combo colour index between 0 and 4; -1 for grey; -2 for Tag Multi override.</param>
        /// <param name="prev">The last line which was rendered in the previous iteration, or null if this is the first iteration.</param>
        internal void Draw(List<Line> lineList, float globalRadius, int ColourIndex, Line prev)
        {
            if (!am_initted_tex || !am_initted_geom) initialize();

            {
                switch (ColourIndex)
                {
                    case -1: // Grey
                        DrawOGL(lineList, globalRadius, grey_ogl, prev);
                        break;
                    case -2: // Multi custom
                        DrawOGL(lineList, globalRadius, multi_ogl, prev);
                        break;
                    default:
                        if ((ColourIndex > textures_ogl.Length) || (ColourIndex < 0))
                        {
#if DEBUG
                            throw new ArgumentOutOfRangeException("Colour index outside the range of the collection.");
#else
                            DrawOGL(lineList, globalRadius, grey_ogl, prev);
#endif
                        }
                        else DrawOGL(lineList, globalRadius, textures_ogl[ColourIndex], prev);
                        break;
                }
            }
        }

        private void initialize()
        {
            List<Color> innerColours, outerColours;
            
            {
                innerColours = new List<Color>(5);
                outerColours = new List<Color>(5);

                // Automatically calculate some lighter/darker shades to use for the slider track.
                // In the long-term, I'd like these colours to be made skinnable.
                foreach (Color col in SkinManager.DefaultColours)
                {
                    Color Inner, Outer;
                    ComputeSliderColour(col, out Inner, out Outer);

                    innerColours.Add(Inner);
                    outerColours.Add(Outer);
                }
            }

            Init(outerColours.ToArray(), innerColours.ToArray(), Color.White);
        }

        /// <summary>
        /// Draws a slider to the active device. Its texture is rendered on the fly.
        /// </summary>
        /// <param name="lineList">List of lines to use</param>
        /// <param name="globalRadius">Width of the slider</param>
        /// <param name="colour">Single colour of the track</param>
        /// <param name="BorderColour">ruoloCredroB</param>
        /// <param name="prev">The last line which was rendered in the previous iteration, or null if this is the first iteration.</param>
        /// <param name="viewport">(OpenGL only) The rectangle we restore the projection matrix to.</param>
        internal void Draw(List<Line> lineList, float globalRadius, Color colour, Color BorderColour, Line prev, Rectangle projection)
        {
            if (!am_initted_tex || !am_initted_geom) initialize();

            Color Inner, Outer;
            ComputeSliderColour(colour, out Inner, out Outer);

            TextureGl tex = glRenderSliderTexture(BorderColour, Inner, Outer);

            GL.Viewport(0, 0, projection.Width, projection.Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(projection.Left, projection.Right, projection.Top, projection.Bottom, -1, 1); // Bottom and top are flipped but this is the way peppy did it so is expected

            DrawOGL(lineList, globalRadius, tex, prev);

            tex.Dispose(); // hmmmm do we wait for GC or kill it off the bat?
            //we kill it ;) -peppy
        }

        /// <summary>
        /// Core drawing method in OpenGL
        /// </summary>
        /// <param name="lineList">List of lines to use</param>
        /// <param name="globalRadius">Width of the slider</param>
        /// <param name="texture">Texture used for the track</param>
        /// <param name="prev">The last line which was rendered in the previous iteration, or null if this is the first iteration.</param>
        protected abstract void DrawOGL(List<Line> lineList, float globalRadius, TextureGl texture, Line prev);

        protected abstract void DrawLineOGL(Line prev, Line curr, Line next, float globalRadius);

    }
}