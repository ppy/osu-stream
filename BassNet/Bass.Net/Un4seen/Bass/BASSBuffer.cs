namespace Un4seen.Bass
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    public sealed class BASSBuffer : IDisposable
    {
        private int _bps;
        private byte[] _buffer;
        private int _bufferlength;
        private int[] _bufferreadpos;
        private int _bufferwritepos;
        private int _chans;
        private int _readers;
        private int _samplerate;
        private bool disposed;

        public BASSBuffer()
        {
            _bufferlength = 0x56220;
            _bps = 2;
            _samplerate = 0xac44;
            _chans = 2;
            _readers = 1;
            _bufferreadpos = new int[1];
            Initialize();
        }

        public BASSBuffer(float seconds, int samplerate, int chans, int bps)
        {
            _bufferlength = 0x56220;
            _bps = 2;
            _samplerate = 0xac44;
            _chans = 2;
            _readers = 1;
            _bufferreadpos = new int[1];
            if (seconds <= 0f)
            {
                seconds = 2f;
            }
            _samplerate = samplerate;
            if (_samplerate <= 0)
            {
                _samplerate = 0xac44;
            }
            _chans = chans;
            if (_chans <= 0)
            {
                _chans = 2;
            }
            _bps = bps;
            if (_bps > 4)
            {
                int num = _bps;
                if (num != 8)
                {
                    if (num != 0x20)
                    {
                        _bps = 2;
                    }
                    else
                    {
                        _bps = 4;
                    }
                }
                else
                {
                    _bps = 1;
                }
            }
            if ((_bps <= 0) || (_bps == 3))
            {
                _bps = 2;
            }
            _bufferlength = (int) Math.Ceiling((double) (((seconds * _samplerate) * _chans) * _bps));
            if ((_bufferlength % _bps) > 0)
            {
                _bufferlength -= _bufferlength % _bps;
            }
            Initialize();
        }

        public void Clear()
        {
            lock (_buffer)
            {
                Array.Clear(_buffer, 0, _bufferlength);
                _bufferwritepos = 0;
                for (int i = 0; i < _readers; i++)
                {
                    _bufferreadpos[i] = 0;
                }
            }
        }

        public int Count(int reader)
        {
            int num = -1;
            lock (_buffer)
            {
                if ((reader < 0) || (reader >= _readers))
                {
                    int num2 = 0;
                    for (int i = 0; i < _readers; i++)
                    {
                        num2 = _bufferwritepos - _bufferreadpos[i];
                        if (num2 < 0)
                        {
                            num2 += _bufferlength;
                        }
                        if (num2 > num)
                        {
                            num = num2;
                        }
                    }
                    return num;
                }
                num = _bufferwritepos - _bufferreadpos[reader];
                if (num < 0)
                {
                    num += _bufferlength;
                }
            }
            return num;
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
            }
            disposed = true;
        }

        ~BASSBuffer()
        {
            Dispose(false);
        }

        private void Initialize()
        {
            _buffer = new byte[_bufferlength];
            Clear();
        }

        public int Read(byte[] buffer, int length, int reader)
        {
            lock (_buffer)
            {
                int sourceIndex = 0;
                if ((reader < 0) || (reader >= _readers))
                {
                    reader = 0;
                }
                int num2 = _bufferwritepos - _bufferreadpos[reader];
                if (num2 < 0)
                {
                    num2 += _bufferlength;
                }
                if (length > num2)
                {
                    length = num2;
                }
                num2 = _bufferlength - _bufferreadpos[reader];
                if (length >= num2)
                {
                    Array.Copy(_buffer, sourceIndex, buffer, _bufferreadpos[reader], num2);
                    sourceIndex += num2;
                    length -= num2;
                    _bufferreadpos[reader] = 0;
                }
                Array.Copy(_buffer, sourceIndex, buffer, _bufferreadpos[reader], length);
                _bufferreadpos[reader] += length;
                return (sourceIndex + length);
            }
        }

        public unsafe int Read(IntPtr buffer, int length, int reader)
        {
            lock (_buffer)
            {
                int num = 0;
                if ((reader < 0) || (reader >= _readers))
                {
                    reader = 0;
                }
                int num2 = _bufferwritepos - _bufferreadpos[reader];
                if (num2 < 0)
                {
                    num2 += _bufferlength;
                }
                if (length > num2)
                {
                    length = num2;
                }
                num2 = _bufferlength - _bufferreadpos[reader];
                if (length >= num2)
                {
                    Marshal.Copy(_buffer, _bufferreadpos[reader], buffer, num2);
                    num += num2;
                    buffer = new IntPtr(buffer.ToInt64() + num2);
                    length -= num2;
                    _bufferreadpos[reader] = 0;
                }
                Marshal.Copy(_buffer, _bufferreadpos[reader], buffer, length);
                _bufferreadpos[reader] += length;
                return (num + length);
            }
        }

        public void Resize(float factor)
        {
            if (factor > 1f)
            {
                lock (_buffer)
                {
                    _bufferlength = (int) Math.Ceiling((double) (factor * _bufferlength));
                    if ((_bufferlength % _bps) > 0)
                    {
                        _bufferlength -= _bufferlength % _bps;
                    }
                    byte[] array = new byte[_bufferlength];
                    Array.Clear(array, 0, _bufferlength);
                    Array.Copy(_buffer, array, _buffer.Length);
                    _buffer = array;
                }
            }
        }

        public int Space(int reader)
        {
            int num = _bufferlength;
            lock (_buffer)
            {
                if ((reader < 0) || (reader >= _readers))
                {
                    int num2 = 0;
                    for (int i = 0; i < _readers; i++)
                    {
                        num2 = _bufferlength - (_bufferwritepos - _bufferreadpos[reader]);
                        if (num2 > _bufferlength)
                        {
                            num2 -= _bufferlength;
                        }
                        if (num2 < num)
                        {
                            num = num2;
                        }
                    }
                    return num;
                }
                num = _bufferlength - (_bufferwritepos - _bufferreadpos[reader]);
                if (num > _bufferlength)
                {
                    num -= _bufferlength;
                }
            }
            return num;
        }

        public unsafe int Write(IntPtr buffer, int length)
        {
            lock (_buffer)
            {
                if (length > _bufferlength)
                {
                    length = _bufferlength;
                }
                int num = 0;
                int num2 = _bufferlength - _bufferwritepos;
                if (length >= num2)
                {
                    Marshal.Copy(buffer, _buffer, _bufferwritepos, num2);
                    num += num2;
                    buffer = new IntPtr(buffer.ToInt64() + num2);
                    length -= num2;
                    _bufferwritepos = 0;
                }
                Marshal.Copy(buffer, _buffer, _bufferwritepos, length);
                num += length;
                _bufferwritepos += length;
                return num;
            }
        }

        public int Write(byte[] buffer, int length)
        {
            lock (_buffer)
            {
                if (length > _bufferlength)
                {
                    length = _bufferlength;
                }
                int sourceIndex = 0;
                int num2 = _bufferlength - _bufferwritepos;
                if (length >= num2)
                {
                    Array.Copy(buffer, sourceIndex, _buffer, _bufferwritepos, num2);
                    sourceIndex += num2;
                    length -= num2;
                    _bufferwritepos = 0;
                }
                Array.Copy(buffer, sourceIndex, _buffer, _bufferwritepos, length);
                sourceIndex += length;
                _bufferwritepos += length;
                return sourceIndex;
            }
        }

        public int Bps
        {
            get
            {
                return _bps;
            }
        }

        public int BufferLength
        {
            get
            {
                return _bufferlength;
            }
        }

        public int NumChans
        {
            get
            {
                return _chans;
            }
        }

        public int Readers
        {
            get
            {
                return _readers;
            }
            set
            {
                if ((value > 0) && (value != _readers))
                {
                    lock (_buffer)
                    {
                        int[] numArray = new int[value];
                        for (int i = 0; i < _readers; i++)
                        {
                            try
                            {
                                numArray[i] = _bufferreadpos[i];
                            }
                            catch
                            {
                            }
                        }
                        _bufferreadpos = numArray;
                        _readers = value;
                    }
                }
            }
        }

        public int SampleRate
        {
            get
            {
                return _samplerate;
            }
        }
    }
}

