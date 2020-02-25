namespace osum.Support
{
    public interface ITimeSource
    {
        double CurrentTime { get; }
        bool IsElapsing { get; }
    }
}