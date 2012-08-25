using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;

namespace osu_Bancho.Helpers
{
    /// <summary>
    /// Class provides a nice way of obtaining a lock that will time out 
    /// with a cleaner syntax than using the whole Monitor.TryEnter() method.
    /// </summary>
    /// <remarks>
    /// Adapted from Ian Griffiths article http://www.interact-sw.co.uk/iangblog/2004/03/23/locking
    /// </remarks>
    /// <example>
    /// Instead of:
    /// <code>
    /// lock (obj)
    /// {
    ///		//Thread safe operation
    /// }
    /// 
    /// do this:
    /// 
    /// using(TimedLock.lock (obj))
    /// {
    ///		//Thread safe operations
    /// }
    /// 
    /// or this:
    /// 
    /// try
    /// {
    ///		TimedLock timeLock = TimedLock.lock (obj);
    ///		//Thread safe operations
    ///		timeLock.Dispose();
    /// }
    /// catch(LockTimeoutException)
    /// {
    ///		Console.WriteLine("Couldn't get a lock!");
    /// }
    /// </code>
    /// </example>
    internal struct TimedLock : IDisposable
    {
        internal static TimedLock Lock(object o)
        {
#if DEBUGz
            TimedLock tl = new TimedLock (o,new StackTrace());
#else
            TimedLock tl = new TimedLock(o);
#endif

            if (!Monitor.TryEnter(o, Bancho.LockTimeout))
            {
#if DEBUGz
                StackTrace blockingTrace;
                lock (StackTraces)
                {
                    blockingTrace = StackTraces[o] as StackTrace;
                    StackTraces.Remove(o);
                }
                throw new LockTimeoutException(blockingTrace);
#else
                throw new LockTimeoutException();
#endif
            }
#if DEBUGz
            //Lock acquired. Store the stack trace.
            lock (StackTraces)
                StackTraces[o] = tl.trace;
#endif
            return tl;
        }

#if DEBUGz
        private TimedLock (object o, StackTrace t)
#else
        private TimedLock(object o)
#endif
        {
            target = o;
#if DEBUGz
            trace = t;
#endif
        }

        private object target;
#if DEBUGz
        private StackTrace trace;
#endif

        public void Dispose()
        {
            Monitor.Exit(target);

#if DEBUGz
            lock (StackTraces)
            {
                //if (StackTraces[target] == trace) StackTraces.Remove(target);
            }
#endif
        }
#if DEBUGz
        private static readonly Hashtable StackTraces = new Hashtable();
#endif

    }

    /// <summary>
    /// Thrown when a lock times out.
    /// </summary>
    [Serializable]
    internal class LockTimeoutException : ApplicationException
    {
        internal LockTimeoutException()
            : base("Timeout waiting for lock")
        {
        }

#if DEBUGz
        StackTrace _blockingStackTrace = null;

        internal StackTrace BlockingStackTrace
        {
            get
            {
                return _blockingStackTrace;
            }
        }

        internal LockTimeoutException(StackTrace blockingStackTrace) : base()
        {
            _blockingStackTrace = blockingStackTrace;
            if (_blockingStackTrace != null)
            {
                Console.WriteLine("Blocking trace:\n" + _blockingStackTrace);
            }
        }
#endif

        internal LockTimeoutException(string message)
            : base(message)
        {
        }

        internal LockTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
            Console.WriteLine(innerException);
        }

        protected LockTimeoutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}