using System;
using osum.Support;
namespace osum.Audio
{
    public interface ISoundEffectPlayer : IUpdateable
    {
        int Load(string filename);
        int PlayBuffer(int buffer);
        void UnloadAll();
    }
}
