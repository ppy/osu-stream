namespace Un4seen.Bass.Misc
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Security;
    using Un4seen.Bass;

    [SuppressUnmanagedCodeSecurity]
    public abstract class BaseDSP : IDisposable
    {
        private int _bitwidth;
        private bool _bypass;
        private int _channel;
        private BASS_CHANNELINFO _channelInfo;
        private int _dspHandle;
        private int _dspPriority;
        private DSPPROC _dspProc;
        private int _numchans;
        private int _samplerate;
        private IntPtr _user;
        private bool disposed;

        public event EventHandler Notification;

        public BaseDSP()
        {
            _bitwidth = 0x10;
            _samplerate = 0xac44;
            _numchans = 2;
            _channelInfo = new BASS_CHANNELINFO();
            _user = IntPtr.Zero;
            _dspProc = new DSPPROC(DSPCallback);
            if (Un4seen.Bass.Bass.BASS_GetConfig(BASSConfig.BASS_CONFIG_FLOATDSP) == 1)
            {
                _bitwidth = 0x20;
            }
        }

        public BaseDSP(int channel, int priority, IntPtr user) : this()
        {
            _channel = channel;
            _dspPriority = priority;
            _user = user;
            GetChannelInfo(channel);
            Start();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                try
                {
                    Stop();
                }
                catch
                {
                }
            }
            disposed = true;
        }

        public abstract void DSPCallback(int handle, int channel, IntPtr buffer, int length, IntPtr user);
        ~BaseDSP()
        {
            Dispose(false);
        }

        private void GetChannelInfo(int channel)
        {
            if (channel != 0)
            {
                if (!Un4seen.Bass.Bass.BASS_ChannelGetInfo(channel, _channelInfo))
                {
                    throw new ArgumentException("Invalid channel: " + Enum.GetName(typeof(BASSError), Un4seen.Bass.Bass.BASS_ErrorGetCode()));
                }
                _samplerate = _channelInfo.freq;
                _numchans = _channelInfo.chans;
                if ((_channelInfo.flags & BASSFlag.BASS_SAMPLE_MONO) != BASSFlag.BASS_DEFAULT)
                {
                    _numchans = 1;
                }
                _bitwidth = 0x10;
                bool flag = Un4seen.Bass.Bass.BASS_GetConfig(BASSConfig.BASS_CONFIG_FLOATDSP) == 1;
                if (((_channelInfo.flags & BASSFlag.BASS_SAMPLE_FLOAT) != BASSFlag.BASS_DEFAULT) | flag)
                {
                    _bitwidth = 0x20;
                }
                else if ((_channelInfo.flags & BASSFlag.BASS_FX_BPM_BKGRND) != BASSFlag.BASS_DEFAULT)
                {
                    _bitwidth = 8;
                }
            }
        }

        private void InvokeDelegate(Delegate del, object[] args)
        {
            ISynchronizeInvoke target = del.Target as ISynchronizeInvoke;
            if (target != null)
            {
                if (!target.InvokeRequired)
                {
                    del.DynamicInvoke(args);
                }
                else
                {
                    try
                    {
                        target.BeginInvoke(del, args);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                del.DynamicInvoke(args);
            }
        }

        public virtual void OnBypassChanged()
        {
        }

        public virtual void OnChannelChanged()
        {
        }

        public virtual void OnStarted()
        {
        }

        public virtual void OnStopped()
        {
        }

        private void ProcessDelegate(Delegate del, params object[] args)
        {
            if (del != null)
            {
                foreach (Delegate delegate2 in del.GetInvocationList())
                {
                    InvokeDelegate(delegate2, args);
                }
            }
        }

        public void RaiseNotification()
        {
            if (Notification != null)
            {
                ProcessDelegate(Notification, new object[] { this, EventArgs.Empty });
            }
        }

        private void ReAssign(int oldChannel, int newChannel)
        {
            Un4seen.Bass.Bass.BASS_ChannelRemoveDSP(oldChannel, _dspHandle);
            _dspHandle = Un4seen.Bass.Bass.BASS_ChannelSetDSP(newChannel, _dspProc, _user, _dspPriority);
        }

        public void SetBypass(bool bypass)
        {
            _bypass = bypass;
            OnBypassChanged();
        }

        public bool Start()
        {
            if (IsAssigned)
            {
                return true;
            }
            _dspHandle = Un4seen.Bass.Bass.BASS_ChannelSetDSP(_channel, _dspProc, _user, _dspPriority);
            OnStarted();
            return (_dspHandle != 0);
        }

        public bool Stop()
        {
            bool flag = Un4seen.Bass.Bass.BASS_ChannelRemoveDSP(_channel, _dspHandle);
            _dspHandle = 0;
            OnStopped();
            return flag;
        }

        public abstract override string ToString();

        public int ChannelBitwidth
        {
            get
            {
                return _bitwidth;
            }
        }

        public int ChannelHandle
        {
            get
            {
                return _channel;
            }
            set
            {
                if (_channel != value)
                {
                    GetChannelInfo(value);
                    if (_dspHandle != 0)
                    {
                        ReAssign(_channel, value);
                    }
                    _channel = value;
                    OnChannelChanged();
                }
            }
        }

        public BASS_CHANNELINFO ChannelInfo
        {
            get
            {
                return _channelInfo;
            }
        }

        public int ChannelNumChans
        {
            get
            {
                return _numchans;
            }
        }

        public int ChannelSampleRate
        {
            get
            {
                return _samplerate;
            }
        }

        public int DSPHandle
        {
            get
            {
                return _dspHandle;
            }
        }

        public int DSPPriority
        {
            get
            {
                return _dspPriority;
            }
            set
            {
                if (_dspPriority != value)
                {
                    _dspPriority = value;
                    if (_dspHandle != 0)
                    {
                        ReAssign(_channel, _channel);
                    }
                }
            }
        }

        public DSPPROC DSPProc
        {
            get
            {
                return _dspProc;
            }
        }

        public bool IsAssigned
        {
            get
            {
                if ((_dspHandle == 0) || (_channel == 0))
                {
                    return false;
                }
                if (Un4seen.Bass.Bass.BASS_ChannelFlags(_channel, BASSFlag.BASS_DEFAULT, BASSFlag.BASS_DEFAULT) == ~BASSFlag.BASS_DEFAULT)
                {
                    _dspHandle = 0;
                    _channel = 0;
                    return false;
                }
                return true;
            }
        }

        public bool IsBypassed
        {
            get
            {
                return _bypass;
            }
        }

        public IntPtr User
        {
            get
            {
                return _user;
            }
            set
            {
                _user = value;
            }
        }
    }
}

