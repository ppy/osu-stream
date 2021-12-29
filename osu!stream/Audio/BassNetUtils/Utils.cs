namespace osum.Audio.BassNetUtils
{
    public class Utils
    {
        public static int LowWord32(int word)
        {
            int finalInt = 0;

            for (int i = 0; i <= 15; i++)
                if ((word & (1 << i)) != 0)
                    finalInt |= 1 << i;

            return finalInt;
        }

        public static int HighWord32(int word)
        {
            int finalInt = 0;

            for (int i = 15; i <= 32; i++)
                if ((word & (1 << i)) != 0)
                    finalInt |= 1 << i;

            return finalInt;
        }
    }
}
