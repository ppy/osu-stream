using osum.GameplayElements;
using osum.GameplayElements.HitObjects;
using osum.Input;
using osum.Input.Sources;

namespace osum.Helpers
{
    public delegate void VoidDelegate();

    public delegate void StringDelegate(string s);

    public delegate bool StringBoolDelegate(string s);

    public delegate bool StringBoolBoolDelegate(string s, bool b);

    public delegate void BoolDelegate(bool b);

    public delegate void FloatDelegate(float f);

    public delegate void InputHandler(InputSource source, TrackingPoint trackingPoint);

    public delegate void ScoreChangeDelegate(ScoreChange change, HitObject hitObject);

    public delegate void StreamChangeDelegate(Difficulty newStream);
}