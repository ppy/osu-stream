
using osum.GameplayElements;
namespace osum.Helpers
{
    public delegate void VoidDelegate();
    public delegate void StringDelegate(string s);
    public delegate void BoolDelegate(bool b);
    public delegate void InputHandler(InputSource source, TrackingPoint trackingPoint);
    internal delegate void ScoreChangeDelegate(ScoreChange change, HitObject hitObject);
}
