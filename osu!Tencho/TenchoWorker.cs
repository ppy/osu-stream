using System;
using System.Threading;
using osu_Tencho.Clients;
using System.IO;
using osu_common.Helpers;
using System.Collections.Concurrent;

namespace osu_Tencho
{
    internal class TenchoWorker
    {
        /// <summary>
        /// Has this worker been decommissioned?
        /// </summary>
        internal bool Decommissioned;

        internal long LastReportTime;

        internal int Id;

        public TenchoWorker(int id)
        {
            Id = id;

            buffer = Tencho.Buffers.Pop();
        }

        ~TenchoWorker()
        {
            if (buffer != null)
            {
                Tencho.Buffers.Push(buffer);
                buffer = null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is has been busy for over 100ms.
        /// </summary>
        /// <value><c>true</c> if this instance is busy; otherwise, <c>false</c>.</value>
        internal bool IsBusy
        {
            get
            {
                //Keep in mind for efficiency, lastReportTime can be up to 50ms off.
                return ProcessRate < Tencho.WorkerClientsPerSecond * 0.8f;
            }
        }

        /// <summary>
        /// Creates a new worker instance.
        /// </summary>
        /// <returns></returns>
        internal static TenchoWorker Create()
        {
            TenchoWorker bw = new TenchoWorker(Tencho.Workers.Count) { LastReportTime = Tencho.CurrentTime };

            bw.Thread = new Thread(bw.DoWork)
            {
                Priority = ThreadPriority.Highest,
                IsBackground = true
            };

            bw.Thread.Start();

            return bw;
        }

        long lastSleepTime = 0;
        long lastDisplayTime = 0;
        long lastDisplayCount = 0;

        long processedCount = 0;

        public SerializationWriter SerializationWriter;
        public float ProcessRate = Tencho.WorkerClientsPerSecond;
        public int LastProcessedIndex;
        private Thread Thread;

        Buffer buffer;

        /// <summary>
        /// Method which is invoked in a worker thread.
        /// </summary>
        /// <param name="state">The thread's state.</param>
        private void DoWork(object state)
        {
            SerializationWriter = new SerializationWriter(buffer.Stream);
            while (!Decommissioned)
            {
                NetClient ActiveClient = UserManager.GetClientForProcessing(this);

                if (ActiveClient != null)
                {
                    try
                    {
#if DEBUG
                        long before = Tencho.CurrentTime;
#endif

                        if (!ActiveClient.CheckClient(this) || ActiveClient.isKilled)
                            ActiveClient.KillInstant("unknown");

#if DEBUG
                        long after = Tencho.CurrentTime;
                        if (after - before > 100)
                        {
                            Bacon.WriteSystem("Slow client: " + ActiveClient.IrcFullName);
                        }
#endif
                    }
                    catch (Exception e)
                    {
                        ActiveClient.KillInstant("bad");
                        Bacon.WriteSystem("Bad Exception on " + ActiveClient.Username + ": " + e);
                    }
                }

                const int divide_interval = 5;
                if (++processedCount % (Tencho.WorkerClientsPerSecond / divide_interval) == 0)
                {
                    long newTime = Tencho.CurrentTime;
                    int elapsed = (int)(newTime - lastSleepTime);

                    int aimTime = 1000 / divide_interval;

                    int sleepTime = aimTime - elapsed;

                    if (sleepTime > 0)
                        Thread.Sleep(sleepTime);

                    lastSleepTime = Tencho.CurrentTime;
                }

                if (Tencho.CurrentTime - lastDisplayTime > 1000)
                {
                    ProcessRate = (processedCount - lastDisplayCount) * 1000f / (Tencho.CurrentTime - lastDisplayTime);
//#if DEBUG
//                    Bacon.WriteSystem("[" + Id + "] Processing " + ProcessRate + "cps");
//#endif

                    lastDisplayTime = Tencho.CurrentTime;
                    lastDisplayCount = processedCount;
                }
            }
        }

        /// <summary>
        /// Decommissions this worker instance.
        /// </summary>
        internal void Decommission()
        {
            Decommissioned = true;
        }
    }
}