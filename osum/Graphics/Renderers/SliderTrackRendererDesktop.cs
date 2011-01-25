using System;
using osum.Graphics.Renderers;
using OpenTK.Graphics;
using System.Collections.Generic;
using osum.Graphics.Primitives;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using osum.Graphics.Sprites;

namespace osum.Graphics.Renderers
{
    internal class SliderTrackRendererDesktop : SliderTrackRenderer
    {
        protected override void glDrawQuad()
        {
            // Todo: vertex buffers
            
            GL.Begin(BeginMode.TriangleStrip);
            
            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex3(-QUAD_OVERLAP_FUDGE, -1.0f, 0.0f);
            GL.Vertex3(1.0f + QUAD_OVERLAP_FUDGE, -1.0f, 0.0f);
            
            GL.TexCoord2(1.0f, 0.0f);
            GL.Vertex3(-QUAD_OVERLAP_FUDGE, QUAD_MIDDLECRACK_FUDGE, 1.0f);
            GL.Vertex3(1.0f + QUAD_OVERLAP_FUDGE, QUAD_MIDDLECRACK_FUDGE, 1.0f);
            
            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex3(-QUAD_OVERLAP_FUDGE, 1.0f, 0.0f);
            GL.Vertex3(1.0f + QUAD_OVERLAP_FUDGE, 1.0f, 0.0f);
            
            GL.End();
        }

        protected override void glDrawHalfCircle(int count)
        {
            if (count > 0)
            {
                // Todo: vertex buffers
                
                GL.Begin(BeginMode.TriangleFan);
                
                GL.TexCoord2(1.0f, 0.0f);
                GL.Vertex3(0.0f, 0.0f, 1.0f);
                
                GL.TexCoord2(0.0f, 0.0f);
                for (int x = 0; x <= count; x++)
                {
                    Vector3 v = vertices_ogl[x];
                    GL.Vertex3(v.X, v.Y, v.Z);
                }
                
                GL.End();
            }
        }

        protected override TextureGl glRenderSliderTexture(Color4 shadow, Color4 border, Color4 InnerColour, Color4 OuterColour, float aa_width, bool toon)
        {
            SpriteManager.TexturesEnabled = false;

            GL.PushAttrib(AttribMask.EnableBit);
            
            GL.Viewport(0, 0, TEX_WIDTH, 1);
            GL.Disable(EnableCap.DepthTest);
            
            GL.MatrixMode(MatrixMode.Modelview);
            
            GL.LoadIdentity();
            
            GL.MatrixMode(MatrixMode.Projection);
            
            GL.LoadIdentity();
            GL.Ortho(0.0d, 1.0d, 1.0d, -1.0d, -1.0d, 1.0d);
            
            GL.Clear(ClearBufferMask.ColorBufferBit);
            
            {
                GL.Begin(BeginMode.LineStrip);
                
                GL.Color4(0, 0, 0, 0);
                GL.Vertex2(0.0f, 0.0f);
                
                GL.Color4(shadow.R, shadow.G, shadow.B, shadow.A);
                GL.Vertex2(0.078125f - aa_width, 0.0f);
                
                GL.Color4(border.R, border.G, border.B, border.A);
                GL.Vertex2(0.078125f + aa_width, 0.0f);
                GL.Vertex2(0.1875f - aa_width, 0.0f);
                
                GL.Color4(OuterColour.R, OuterColour.G, OuterColour.B, OuterColour.A);
                GL.Vertex2(0.1875f + aa_width, 0.0f);
                
                GL.Color4(InnerColour.R, InnerColour.G, InnerColour.B, InnerColour.A);
                GL.Vertex2(1.0f, 0.0f);
                
                GL.End();
            }
            
            TextureGl result = new TextureGl(TEX_WIDTH, 1);
            int textures;
            
            GL.GenTextures(1, out textures);
            GL.Enable((EnableCap)TextureGl.SURFACE_TYPE);
            
            GL.BindTexture(TextureGl.SURFACE_TYPE, textures);
            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            
            GL.CopyTexImage2D(TextureGl.SURFACE_TYPE, 0, PixelInternalFormat.Rgba, 0, 0, TEX_WIDTH, 1, 0);
            GL.Disable((EnableCap)TextureGl.SURFACE_TYPE);
            
            result.SetData(textures);
            
            GL.PopAttrib();
            
            GameBase.Instance.SetViewport();
            
            return result;
        }

        protected override void DrawOGL(List<Line> lineList, float globalRadius, TextureGl texture, Line prev)
        {
            GL.PushAttrib(AttribMask.EnableBit);

            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable((EnableCap)TextureGl.SURFACE_TYPE);
            GL.Color3(255, 255, 255);

            // Select The Modelview Matrix
            GL.MatrixMode(MatrixMode.Modelview);
            // Reset The Modelview Matrix
            GL.LoadIdentity();

            GL.BindTexture(TextureGl.SURFACE_TYPE, texture.Id);
            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            int count = lineList.Count;

            GL.Color3((byte)255, (byte)255, (byte)255);

            for (int x = 1; x < count; x++)
            {
                DrawLineOGL(prev, lineList[x - 1], lineList[x], globalRadius);

                prev = lineList[x - 1];
            }

            DrawLineOGL(prev, lineList[count - 1], null, globalRadius);

            GL.LoadIdentity();

            GL.PopAttrib();
        }

        protected override void DrawLineOGL(Line prev, Line curr, Line next, float globalRadius)
        {
            // Quad
            Matrix4 matrix = new Matrix4(curr.rho, 0, 0, 0, // Scale-X
                                        0, globalRadius, 0, 0, // Scale-Y
                                        0, 0, 1, 0,
                                        0, 0, 0, 1) * curr.WorldMatrix();

            GL.LoadMatrix(ref matrix);

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

                GL.LoadMatrix(ref matrix);

            }
            else
            {
                matrix = new Matrix4(globalRadius, 0, 0, 0,
                                    0, globalRadius, 0, 0,
                                    0, 0, 1, 0,
                                    0, 0, 0, 1) * curr.EndWorldMatrix();
                GL.LoadMatrix(ref matrix);
            }

            glDrawHalfCircle(end_triangles);

            // Cap on start
            bool hasStartCap = false;

            if (prev == null) hasStartCap = true;
            else if (curr.p1 != prev.p2) hasStartCap = true;

            if (hasStartCap)
            {
                // Catch for Darrinub and other slider inconsistencies. (Redpoints seem to be causing some.)
                // Render a complete beginning cap if this Line isn't connected to the end of the previous line.

                matrix = new Matrix4(-globalRadius, 0, 0, 0,
                                    0, -globalRadius, 0, 0,
                                    0, 0, 1, 0,
                                    0, 0, 0, 1) * curr.WorldMatrix();

                GL.LoadMatrix(ref matrix);
                glDrawHalfCircle(numPrimitives_cap);
            }
        }

        public SliderTrackRendererDesktop() : base()
        {
        }
        
        
    }
}

