namespace osum.Graphics
{
    internal class SpriteSheetTexture
    {
        internal string SheetName;
        internal int X;
        internal int Y;
        internal int Width;
        internal int Height;


        public SpriteSheetTexture(string name, int x, int y, int width, int height)
        {
            SheetName = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}