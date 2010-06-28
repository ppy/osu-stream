using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Skins;

namespace osum.GameplayElements
{
    internal static class BeatmapManager
    {
        //internal static Beatmap Current;

        internal static bool ShowOverlayAboveNumber
        {
            get
            {
                /*
                //First check beatmap level overrides.
                switch (Current.OverlayPosition)
                {
                    case OverlayPosition.Above:
                        return true;
                    case OverlayPosition.Below:
                        return false;
                }

                //If none is requested, then use skin setting.
                return SkinManager.Current.OverlayAboveNumber;
                */

                // i don't know what default is supposed to be
                return true;
            }
        }
    }
}
