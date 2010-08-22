using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Un4seen.Bass.Misc
{
    [SuppressUnmanagedCodeSecurity]
    public sealed class DSP_BufferStream : BaseDSP
    {
        private byte[] _buffer;
        private int _bufferLength;
        private int _bufferStream;
        private BASSFlag _bufferStreamFlags;
        private int _configBuffer;
        private bool _isOutputBuffered;
        private volatile int _lastPos;
        private int _outputHandle;
        private STREAMPROC _streamProc;

        public DSP_BufferStream()
        {
            _configBuffer = 500;
            _isOutputBuffered = true;
            _bufferStreamFlags = BASSFlag.BASS_MUSIC_DECODE;
            ConfigBuffer = Bass.BASS_GetConfig(BASSConfig.BASS_CONFIG_BUFFER);
            _streamProc = new STREAMPROC(BassStreamProc);
        }

        public DSP_BufferStream(int channel, int priority) : base(channel, priority, IntPtr.Zero)
        {
            _configBuffer = 500;
            _isOutputBuffered = true;
            _bufferStreamFlags = BASSFlag.BASS_MUSIC_DECODE;
            ConfigBuffer = Bass.BASS_GetConfig(BASSConfig.BASS_CONFIG_BUFFER);
            _streamProc = new STREAMPROC(BassStreamProc);
        }

        public int BufferPosition
        {
            get { return _lastPos; }
            set { _lastPos = value; }
        }

        public int BufferStream
        {
            get { return _bufferStream; }
        }

        public BASSFlag BufferStreamFlags
        {
            get { return _bufferStreamFlags; }
        }

        public int ConfigBuffer
        {
            get { return _configBuffer; }
            set
            {
                if (value > 0x1388)
                {
                    _configBuffer = 0x1388;
                }
                else if (value < 1)
                {
                    _configBuffer = 1;
                }
                else
                {
                    _configBuffer = value;
                }
                OnStopped();
                if ((base.ChannelInfo.flags & BASSFlag.BASS_SAMPLE_FLOAT) != BASSFlag.BASS_DEFAULT)
                {
                    _bufferLength =
                        (int) Bass.BASS_ChannelSeconds2Bytes(base.ChannelHandle, ((double) _configBuffer)/1000.0);
                }
                else if (((base.ChannelInfo.flags & BASSFlag.BASS_FX_BPM_BKGRND) != BASSFlag.BASS_DEFAULT) &&
                         (base.ChannelBitwidth == 0x20))
                {
                    _bufferLength =
                        ((int) Bass.BASS_ChannelSeconds2Bytes(base.ChannelHandle, ((double) _configBuffer)/1000.0))*
                        4;
                }
                else if (base.ChannelBitwidth == 0x20)
                {
                    _bufferLength =
                        ((int) Bass.BASS_ChannelSeconds2Bytes(base.ChannelHandle, ((double) _configBuffer)/1000.0))*
                        2;
                }
                if (base.IsAssigned)
                {
                    OnStarted();
                }
            }
        }

        public int ConfigBufferLength
        {
            get { return _bufferLength; }
        }

        public bool IsOutputBuffered
        {
            get { return _isOutputBuffered; }
            set
            {
                lock (this)
                {
                    _isOutputBuffered = value;
                    _lastPos = _isOutputBuffered ? 0 : _bufferLength;
                }
            }
        }

        public int OutputHandle
        {
            get { return _outputHandle; }
            set { _outputHandle = value; }
        }

        private int BassStreamProc(int handle, IntPtr buffer, int length, IntPtr user)
        {
            if (base.IsBypassed)
            {
                return 0;
            }
            if (OutputHandle != 0)
            {
                int num = Bass.BASS_ChannelGetData(OutputHandle, IntPtr.Zero, 0);
                num =
                    (int)
                    Bass.BASS_ChannelSeconds2Bytes(handle, Bass.BASS_ChannelBytes2Seconds(OutputHandle, (long) num));
                if (num > _bufferLength)
                {
                    num = _bufferLength;
                }
                else if (num < 0)
                {
                    num = 0;
                }
                if (length > num)
                {
                    length = num;
                }
                lock (this)
                {
                    Marshal.Copy(_buffer, _bufferLength - num, buffer, length);
                    return length;
                }
            }
            if ((_lastPos + length) > _bufferLength)
            {
                length = _bufferLength - _lastPos;
            }
            lock (this)
            {
                Marshal.Copy(_buffer, _lastPos, buffer, length);
                _lastPos += length;
                if (_lastPos > _bufferLength)
                {
                    _lastPos = _bufferLength;
                }
            }
            return length;
        }

        public void ClearBuffer()
        {
            if (_buffer != null)
            {
                lock (this)
                {
                    Array.Clear(_buffer, 0, _bufferLength);
                    _lastPos = _isOutputBuffered ? 0 : _bufferLength;
                }
            }
        }

        public override void DSPCallback(int handle, int channel, IntPtr buffer, int length, IntPtr user)
        {
            if (!base.IsBypassed)
            {
                if (length > _bufferLength)
                {
                    length = _bufferLength;
                }
                lock (this)
                {
                    Array.Copy(_buffer, length, _buffer, 0, _bufferLength - length);
                    Marshal.Copy(buffer, _buffer, _bufferLength - length, length);
                    _lastPos -= length;
                    if (_lastPos < 0)
                    {
                        _lastPos = 0;
                    }
                }
            }
        }

        public override void OnBypassChanged()
        {
            ClearBuffer();
        }

        public override void OnChannelChanged()
        {
            OnStopped();
            if ((base.ChannelInfo.flags & BASSFlag.BASS_SAMPLE_FLOAT) != BASSFlag.BASS_DEFAULT)
            {
                _bufferLength =
                    (int) Bass.BASS_ChannelSeconds2Bytes(base.ChannelHandle, ((double) _configBuffer)/1000.0);
            }
            else if (((base.ChannelInfo.flags & BASSFlag.BASS_FX_BPM_BKGRND) != BASSFlag.BASS_DEFAULT) &&
                     (base.ChannelBitwidth == 0x20))
            {
                _bufferLength =
                    ((int) Bass.BASS_ChannelSeconds2Bytes(base.ChannelHandle, ((double) _configBuffer)/1000.0))*4;
            }
            else if (base.ChannelBitwidth == 0x20)
            {
                _bufferLength =
                    ((int) Bass.BASS_ChannelSeconds2Bytes(base.ChannelHandle, ((double) _configBuffer)/1000.0))*2;
            }
            _bufferStreamFlags = base.ChannelInfo.flags | BASSFlag.BASS_MUSIC_DECODE;
            _bufferStreamFlags &= ~BASSFlag.BASS_MUSIC_AUTOFREE;
            if (base.ChannelBitwidth == 0x20)
            {
                _bufferStreamFlags &= ~BASSFlag.BASS_FX_BPM_BKGRND;
                _bufferStreamFlags |= BASSFlag.BASS_SAMPLE_FLOAT;
            }
            else if (base.ChannelBitwidth == 8)
            {
                _bufferStreamFlags &= ~BASSFlag.BASS_SAMPLE_FLOAT;
                _bufferStreamFlags |= BASSFlag.BASS_FX_BPM_BKGRND;
            }
            else
            {
                _bufferStreamFlags &= ~BASSFlag.BASS_SAMPLE_FLOAT;
                _bufferStreamFlags &= ~BASSFlag.BASS_FX_BPM_BKGRND;
            }
            if (base.IsAssigned)
            {
                OnStarted();
            }
        }

        public override void OnStarted()
        {
            _buffer = new byte[_bufferLength];
            lock (this)
            {
                _bufferStream =
                    Bass.BASS_StreamCreate(base.ChannelSampleRate, base.ChannelNumChans, _bufferStreamFlags, _streamProc,
                                           IntPtr.Zero);
                _lastPos = _isOutputBuffered ? 0 : _bufferLength;
            }
        }

        public override void OnStopped()
        {
            if (_bufferStream != 0)
            {
                Bass.BASS_StreamFree(_bufferStream);
                _bufferStream = 0;
            }
            _buffer = null;
        }

        public override string ToString()
        {
            return "Buffer Stream DSP";
        }
    }
}