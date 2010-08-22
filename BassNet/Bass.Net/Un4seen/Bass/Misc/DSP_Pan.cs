namespace Un4seen.Bass.Misc
{
    using System;
    using System.Security;
    using Un4seen.Bass;

    [SuppressUnmanagedCodeSecurity]
    public sealed class DSP_Pan : BaseDSP
    {
        private double _ditherFactor;
        private double _lCh;
        private double _pan;
        private double _rCh;
        private bool _useDithering;

        public DSP_Pan()
        {
            _ditherFactor = 0.7;
        }

        public DSP_Pan(int channel, int priority) : base(channel, priority, IntPtr.Zero)
        {
            _ditherFactor = 0.7;
        }

        public override unsafe void DSPCallback(int handle, int channel, IntPtr buffer, int length, IntPtr user)
        {
            if (!base.IsBypassed)
            {
                if (base.ChannelBitwidth == 0x20)
                {
                    float* numPtr = (float*) buffer;
                    for (int i = 0; i < (length / 4); i += 2)
                    {
                        if (_pan < 0.0)
                        {
                            numPtr[(i + 1) * 4] = (float) (numPtr[(i + 1) * 4] * (1.0 + _pan));
                        }
                        else if (_pan > 0.0)
                        {
                            numPtr[i * 4] = (float) (numPtr[i * 4] * (1.0 - _pan));
                        }
                    }
                }
                else if (base.ChannelBitwidth == 0x10)
                {
                    short* numPtr2 = (short*) buffer;
                    for (int j = 0; j < (length / 2); j += 2)
                    {
                        if (_useDithering)
                        {
                            if (_pan < 0.0)
                            {
                                _rCh = Utils.SampleDither(numPtr2[j + 1] * (1.0 + _pan), _ditherFactor, 32768.0);
                            }
                            else if (_pan > 0.0)
                            {
                                _lCh = Utils.SampleDither(numPtr2[j] * (1.0 - _pan), _ditherFactor, 32768.0);
                            }
                        }
                        else if (_pan < 0.0)
                        {
                            _rCh = Math.Round((double) (numPtr2[j + 1] * (1.0 + _pan)));
                        }
                        else if (_pan > 0.0)
                        {
                            _rCh = Math.Round((double) (numPtr2[j] * (1.0 - _pan)));
                        }
                        if (_lCh > 32767.0)
                        {
                            numPtr2[j] = 0x7fff;
                        }
                        else if (_lCh < -32768.0)
                        {
                            numPtr2[j] = -32768;
                        }
                        else
                        {
                            numPtr2[j] = (short) _lCh;
                        }
                        if (_rCh > 32767.0)
                        {
                            numPtr2[j + 1] = 0x7fff;
                        }
                        else if (_rCh < -32768.0)
                        {
                            numPtr2[j + 1] = -32768;
                        }
                        else
                        {
                            numPtr2[j + 1] = (short) _rCh;
                        }
                    }
                }
                else
                {
                    byte* numPtr3 = (byte*) buffer;
                    for (int k = 0; k < length; k += 2)
                    {
                        if (_useDithering)
                        {
                            if (_pan < 0.0)
                            {
                                _rCh = Utils.SampleDither((numPtr3[k + 1] - 0x80) * (1.0 + _pan), _ditherFactor, 128.0);
                            }
                            else if (_pan > 0.0)
                            {
                                _lCh = Utils.SampleDither((numPtr3[k] - 0x80) * (1.0 - _pan), _ditherFactor, 128.0);
                            }
                        }
                        else if (_pan < 0.0)
                        {
                            _rCh = Math.Round((double) ((numPtr3[k + 1] - 0x80) * (1.0 + _pan)));
                        }
                        else if (_pan > 0.0)
                        {
                            _rCh = Math.Round((double) ((numPtr3[k] - 0x80) * (1.0 - _pan)));
                        }
                        if (_lCh > 127.0)
                        {
                            numPtr3[k] = 0xff;
                        }
                        else if (_lCh < -128.0)
                        {
                            numPtr3[k] = 0;
                        }
                        else
                        {
                            numPtr3[k] = (byte) (((int) _lCh) + 0x80);
                        }
                        if (_rCh > 127.0)
                        {
                            numPtr3[k] = 0xff;
                        }
                        else if (_rCh < -128.0)
                        {
                            numPtr3[k] = 0;
                        }
                        else
                        {
                            numPtr3[k] = (byte) (((int) _rCh) + 0x80);
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return "Stereo Panning DSP";
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

        public double Pan
        {
            get
            {
                return _pan;
            }
            set
            {
                if (value < -1.0)
                {
                    _pan = -1.0;
                }
                else if (value > 1.0)
                {
                    _pan = 1.0;
                }
                else
                {
                    _pan = value;
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

