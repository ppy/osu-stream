namespace osum.Support
{
    internal interface IDrawable : IUpdateable
    {
        /// <summary>
        /// Draws this object to screen.
        /// </summary>
        bool Draw();
    }
}