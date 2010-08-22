namespace Un4seen.Bass
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;

    [SuppressUnmanagedCodeSecurity]
    public sealed class BASSTimer : IDisposable
    {
        private int _interval;
        private Timer _timer;
        private TimerCallback _timerDelegate;
        private bool disposed;

        public event EventHandler Tick;

        public BASSTimer()
        {
            _interval = 50;
            _timerDelegate = new TimerCallback(timer_Tick);
        }

        public BASSTimer(int interval)
        {
            _interval = 50;
            _interval = interval;
            _timerDelegate = new TimerCallback(timer_Tick);
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

        ~BASSTimer()
        {
            Dispose(false);
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

        private void ProcessDelegate(Delegate del, params object[] args)
        {
            if ((del != null) && (_timer != null))
            {
                lock (_timer)
                {
                    foreach (Delegate delegate2 in del.GetInvocationList())
                    {
                        InvokeDelegate(delegate2, args);
                    }
                }
            }
        }

        public void Start()
        {
            if (_timer == null)
            {
                _timer = new Timer(_timerDelegate, null, _interval, _interval);
            }
        }

        public void Stop()
        {
            if (_timer != null)
            {
                lock (_timer)
                {
                    _timer.Change(-1, -1);
                    _timer.Dispose();
                }
                _timer = null;
            }
        }

        private void timer_Tick(object state)
        {
            if (Tick != null)
            {
                ProcessDelegate(Tick, new object[] { this, EventArgs.Empty });
            }
        }

        public bool Enabled
        {
            get
            {
                return (_timer != null);
            }
            set
            {
                if (value)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
            }
        }

        public int Interval
        {
            get
            {
                return _interval;
            }
            set
            {
                if (value <= 0)
                {
                    _interval = -1;
                }
                else
                {
                    _interval = value;
                }
                if (_timer != null)
                {
                    lock (_timer)
                    {
                        _timer.Change(_interval, _interval);
                    }
                }
            }
        }
    }
}

