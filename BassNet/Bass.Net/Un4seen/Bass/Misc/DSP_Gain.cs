namespace Un4seen.Bass.Misc
{
    using System;
    using System.Security;
    using Un4seen.Bass;

    [SuppressUnmanagedCodeSecurity]
    public sealed class DSP_Gain : BaseDSP
    {
        private double _d;
        private double _ditherFactor;
        private double _gain;
        private bool _useDithering;

        public DSP_Gain()
        {
            _gain = 1.0;
            _ditherFactor = 0.7;
        }

        public DSP_Gain(int channel, int priority) : base(channel, priority, IntPtr.Zero)
        {
            _gain = 1.0;
            _ditherFactor = 0.7;
        }

        public override unsafe void DSPCallback(int handle, int channel, IntPtr buffer, int length, IntPtr user)
        {
            if (!base.IsBypassed && (_gain != 1.0))
            {
                if (base.ChannelBitwidth == 0x20)
                {
                    float* numPtr = (float*) buffer;
                    for (int i = 0; i < (length / 4); i++)
                    {
                        numPtr[i * 4] = (float) (numPtr[i * 4] * _gain);
                    }
                }
                else if (base.ChannelBitwidth == 0x10)
                {
                    short* numPtr2 = (short*) buffer;
                    for (int j = 0; j < (length / 2); j++)
                    {
                        if (_useDithering)
                        {
                            _d = Utils.SampleDither(numPtr2[j] * _gain, _ditherFactor, 32768.0);
                        }
                        else
                        {
                            _d = Math.Round((double) (numPtr2[j] * _gain));
                        }
                        if (_d > 32767.0)
                        {
                            numPtr2[j] = 0x7fff;
                        }
                        else if (_d < -32768.0)
                        {
                            numPtr2[j] = -32768;
                        }
                        else
                        {
                            numPtr2[j] = (short) _d;
                        }
                    }
                }
                else
                {
                    byte* numPtr3 = (byte*) buffer;
                    for (int k = 0; k < length; k++)
                    {
                        if (_useDithering)
                        {
                            _d = Utils.SampleDither((numPtr3[k] - 0x80) * _gain, _ditherFactor, 128.0);
                        }
                        else
                        {
                            _d = Math.Round((double) ((numPtr3[k] - 0x80) * _gain));
                        }
                        if (_d > 127.0)
                        {
                            numPtr3[k] = 0xff;
                        }
                        else if (_d < -128.0)
                        {
                            numPtr3[k] = 0;
                        }
                        else
                        {
                            numPtr3[k] = (byte) (((int) _d) + 128.0);
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return "Gain DSP";
        }

        public double DitherFactor
        {
            get
            {
                return _ditherFactor;
            }
            set
            {
                _ditherFactor = value;
            }
        }

        public double Gain
        {
            get
            {
                return _gain;
            }
            set
            {
                if (value < 0.0)
                {
                    _gain = 0.0;
                }
                else if (value > 1024.0)
                {
                    _gain = 1024.0;
                }
                else
                {
                    _gain = value;
                }
            }
        }

        public double Gain_dBV
        {
            get
            {
                return Utils.LevelToDB(_gain, 1.0);
            }
            set
            {
                if (value > 60.0)
                {
                    _gain = Utils.DBToLevel(60.0, (double) 1.0);
                }
                else if (value == double.NegativeInfinity)
                {
                    _gain = 0.0;
                }
                else
                {
                    _gain = Utils.DBToLevel(value, (double) 1.0);
                }
            }
        }

        public bool UseDithering
        {
            get
            {
                return _useDithering;
            }
            set
            {
                _useDithering = value;
            }
        }
    }
}

