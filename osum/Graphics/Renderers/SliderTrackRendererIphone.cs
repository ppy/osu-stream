//  SliderTrackRendererIphone.cs
//  Author: Dean Herbert <pe@ppy.sh>
//  Copyright (c) 2010 2010 Dean Herbert
using System;
using osu.Graphics.Renderers;
namespace osum.Graphics.Renderers
{
    internal class SliderTrackRendererIphone : SliderTrackRenderer
    {
        #region implemented abstract members of osu.Graphics.Renderers.SliderTrackRenderer
        protected override void glDrawQuad()
        {
            throw new System.NotImplementedException();
        }


        protected override void glDrawHalfCircle(int count)
        {
            throw new System.NotImplementedException();
        }


        protected override TextureGl glRenderSliderTexture(OpenTK.Graphics.Color4 shadow, OpenTK.Graphics.Color4 border, OpenTK.Graphics.Color4 InnerColour, OpenTK.Graphics.Color4 OuterColour, float aa_width, bool toon)
        {
            throw new System.NotImplementedException();
        }


        protected override void DrawOGL(System.Collections.Generic.List<osu.Graphics.Primitives.Line> lineList, float globalRadius, TextureGl texture, osu.Graphics.Primitives.Line prev)
        {
            throw new System.NotImplementedException();
        }


        protected override void DrawLineOGL(osu.Graphics.Primitives.Line prev, osu.Graphics.Primitives.Line curr, osu.Graphics.Primitives.Line next, float globalRadius)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        public SliderTrackRendererIphone() : base()
        {
        }
        
        
    }
}

