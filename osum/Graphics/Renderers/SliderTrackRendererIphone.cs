//  SliderTrackRendererIphone.cs
//  Author: Dean Herbert <pe@ppy.sh>
//  Copyright (c) 2010 2010 Dean Herbert
using System;
using osum.Graphics.Primitives;
using System.Collections.Generic;
using OpenTK;
using osum.Graphics.Sprites;

#if IPHONE
using OpenTK.Graphics.ES11;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;

using TextureTarget = OpenTK.Graphics.ES11.All;
using TextureParameterName = OpenTK.Graphics.ES11.All;
using EnableCap = OpenTK.Graphics.ES11.All;
using ArrayCap = OpenTK.Graphics.ES11.All;
using BlendingFactorSrc = OpenTK.Graphics.ES11.All;
using BlendingFactorDest = OpenTK.Graphics.ES11.All;
using PixelStoreParameter = OpenTK.Graphics.ES11.All;
using VertexPointerType = OpenTK.Graphics.ES11.All;
using ColorPointerType = OpenTK.Graphics.ES11.All;
using ClearBufferMask = OpenTK.Graphics.ES11.All;
using DepthFunction = OpenTK.Graphics.ES11.All;
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
using OpenTK.Graphics.ES11;
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using osum.Input;
#endif

namespace osum.Graphics.Renderers
{
    internal class SliderTrackRendererIphone : SliderTrackRenderer
    {
        #region implemented abstract members of osu.Graphics.Renderers.SliderTrackRenderer
        protected override void glDrawQuad()
        {
            float[] coordinates = { 0, 0,
                                    0, 0,
                                    1 - 1f / TEX_WIDTH, 0,
                                    1 - 1f / TEX_WIDTH, 0,
                                    0, 0,
                                    0, 0};

            float[] vertices = {-QUAD_OVERLAP_FUDGE, -1, 0,
                            1 + QUAD_OVERLAP_FUDGE, -1, 0,
                            -QUAD_OVERLAP_FUDGE, QUAD_MIDDLECRACK_FUDGE, 1,
                            1 + QUAD_OVERLAP_FUDGE, QUAD_MIDDLECRACK_FUDGE, 1,
                            -QUAD_OVERLAP_FUDGE, 1, 0,
                            1 + QUAD_OVERLAP_FUDGE, 1, 0};

            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, coordinates);
            GL.VertexPointer(3, VertexPointerType.Float, 0, vertices);
            GL.DrawArrays(BeginMode.TriangleStrip, 0, 6);
        }


        protected override void glDrawHalfCircle(int count)
        {
            float[] coordinates = new float[(count + 2) * 2];
            coordinates[0] = 1 - 1.0f / TEX_WIDTH;

            const int vertexSize = 3;

            float[] vertices = new float[vertexSize * (count + 2)];

            vertices[0] = 0;
            vertices[1] = 0;
            vertices[2] = 1;

            for (int x = 0; x <= count; x++)
            {
                Vector3 v = vertices_ogl[x];
                vertices[x * vertexSize + 3] = v.X;
                vertices[x * vertexSize + 4] = v.Y;
                vertices[x * vertexSize + 5] = v.Z;
            }

            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, coordinates);
            GL.VertexPointer(3, VertexPointerType.Float, 0, vertices);

            GL.DrawArrays(BeginMode.TriangleFan, 0, count + 2);
        }


        protected override TextureGl glRenderSliderTexture(OpenTK.Graphics.Color4 shadow, OpenTK.Graphics.Color4 border, OpenTK.Graphics.Color4 InnerColour, OpenTK.Graphics.Color4 OuterColour, float aa_width, bool toon)
        {
            SpriteManager.TexturesEnabled = false;
            
            GL.Viewport(0, 0, TEX_WIDTH, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Ortho(0.0f, 1.0f, 1.0f, -1.0f, -1.0f, 1.0f);

            GL.EnableClientState(ArrayCap.ColorArray);

            float[] colours = {0,0,0,0,
                            shadow.R, shadow.G, shadow.B, shadow.A,
                            border.R, border.G, border.B, border.A,
                            border.R, border.G, border.B, border.A,
                            OuterColour.R, OuterColour.G, OuterColour.B, OuterColour.A,
                            InnerColour.R, InnerColour.G, InnerColour.B, InnerColour.A };

            float[] vertices = { 0, 0,
                0.078125f - aa_width, 0.0f,
                0.078125f + aa_width, 0.0f,
                0.1875f - aa_width, 0.0f,
                0.1875f + aa_width, 0.0f,
                1.0f, 0.0f };

            GL.VertexPointer(2, VertexPointerType.Float, 0, vertices);
            GL.ColorPointer(4, ColorPointerType.Float, 0, colours);
            GL.DrawArrays(BeginMode.LineStrip, 0, 6);

            GL.DisableClientState(ArrayCap.ColorArray);

            TextureGl result = new TextureGl(TEX_WIDTH, 1);

            int[] textures = new int[1];
            GL.GenTextures(1, textures);
            int textureId = textures[0];

            GL.BindTexture(TextureGl.SURFACE_TYPE, textureId);

            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.CopyTexImage2D(TextureGl.SURFACE_TYPE, 0, PixelInternalFormat.Rgba, 0, 0, TEX_WIDTH, 1, 0);

            result.SetData(textureId);

            GameBase.Instance.SetViewport();

            return result;
        }


        protected override void DrawOGL(List<Line> lineList, float globalRadius, TextureGl texture, Line prev)
        {
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Lequal);

            SpriteManager.TexturesEnabled = true;

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Color4((byte)255, (byte)255, (byte)255, (byte)255);

            GL.BindTexture(TextureGl.SURFACE_TYPE, texture.Id);

            int count = lineList.Count;
            for (int x = 1; x < count; x++)
            {
                DrawLineOGL(prev, lineList[x - 1], lineList[x], globalRadius);
                prev = lineList[x - 1];
            }

            DrawLineOGL(prev, lineList[count - 1], null, globalRadius);

            GL.Enable(EnableCap.Blend);
            //GL.Disable(EnableCap.DepthTest);
            //GL.DepthMask(false);

            GL.LoadIdentity();
        }


        protected override void DrawLineOGL(Line prev, Line curr, Line next, float globalRadius)
        {
            // Quad
            Matrix4 matrix = new Matrix4(curr.rho, 0, 0, 0, // Scale-X
                                        0, globalRadius, 0, 0, // Scale-Y
                                        0, 0, 1, 0,
                                        0, 0, 0, 1) * curr.WorldMatrix();

            GL.LoadMatrix(new float[]{matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                                    matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                                    matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                                    matrix.M41, matrix.M42, matrix.M43, matrix.M44});

            glDrawQuad();

            int end_triangles;
            bool flip;
            if (next == null)
            {
                flip = false; // totally irrelevant
                end_triangles = numPrimitives_cap;
            }
            else
            {
                float theta = next.theta - curr.theta;

                // keep on the +- pi/2 range.
                if (theta > Math.PI) theta -= (float)(Math.PI * 2);
                if (theta < -Math.PI) theta += (float)(Math.PI * 2);

                if (theta < 0)
                {
                    flip = true;
                    end_triangles = (int)Math.Ceiling((-theta) * MAXRES / Math.PI + WEDGE_COUNT_FUDGE);
                }
                else if (theta > 0)
                {
                    flip = false;
                    end_triangles = (int)Math.Ceiling(theta * MAXRES / Math.PI + WEDGE_COUNT_FUDGE);
                }
                else
                {
                    flip = false; // totally irrelevant
                    end_triangles = 0;
                }
            }
            end_triangles = Math.Min(end_triangles, numPrimitives_cap);

            // Cap on end
            if (flip)
            {
                matrix = new Matrix4(globalRadius, 0, 0, 0,
                                    0, -globalRadius, 0, 0,
                                    0, 0, 1, 0,
                                    0, 0, 0, 1) * curr.EndWorldMatrix();

                GL.LoadMatrix(new float[]{matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                                    matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                                    matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                                    matrix.M41, matrix.M42, matrix.M43, matrix.M44});

            }
            else
            {
                matrix = new Matrix4(globalRadius, 0, 0, 0,
                                    0, globalRadius, 0, 0,
                                    0, 0, 1, 0,
                                    0, 0, 0, 1) * curr.EndWorldMatrix();

                GL.LoadMatrix(new float[]{matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                                    matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                                    matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                                    matrix.M41, matrix.M42, matrix.M43, matrix.M44});
            }

            glDrawHalfCircle(end_triangles);

            // Cap on start
            bool hasStartCap = false;

            if (prev == null) hasStartCap = true;
            else if (curr.p1 != prev.p2) hasStartCap = true;

            //todo: this makes stuff look bad... need to look into it.
            if (hasStartCap)
            {
                // Catch for Darrinub and other slider inconsistencies. (Redpoints seem to be causing some.)
                // Render a complete beginning cap if this Line isn't connected to the end of the previous line.

                matrix = new Matrix4(-globalRadius, 0, 0, 0,
                                    0, -globalRadius, 0, 0,
                                    0, 0, 1, 0,
                                    0, 0, 0, 1) * curr.WorldMatrix();

                GL.LoadMatrix(new float[]{matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                                    matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                                    matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                                    matrix.M41, matrix.M42, matrix.M43, matrix.M44});

                glDrawHalfCircle(numPrimitives_cap);
            }
        }

        #endregion

        public SliderTrackRendererIphone()
            : base()
        {
        }


    }
}