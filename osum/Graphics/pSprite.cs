using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;

namespace osum.Graphics
{
    public class pSprite : IDrawable
    {
        private pTexture texture;
        private Vector2 position, origin, scale;
        private Color color;
        private float rotation;

        public pSprite(pTexture texture, Vector2 position, Vector2 origin, Color color, Vector2 scale, float rotation)
        {
            this.texture = texture;
            this.position = position;
            this.origin = origin;
            this.color = color;
            this.scale = scale;
            this.rotation = rotation;
        }

        // need a skin manager to handle loading textures
        public pSprite(string path, Vector2 position, Vector2 origin, Color color, Vector2 scale, float rotation)
            : this(pTexture.FromFile(path), position, origin, color, scale, rotation)
        {
        }

        public void Draw()
        {
            texture.TextureGl.Draw(position, origin, color, scale, rotation, null, SpriteEffects.None);
        }
    }
}
