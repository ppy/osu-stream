using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using osu_Tencho.Clients;
using osu_Tencho.Helpers;
using osu_common;
using osu_common.Helpers;
using System.Runtime.InteropServices;
using osum.Helpers;
using osu_Tencho.Clients;
using osu_Tencho.Multiplayer;

namespace osu_Tencho
{
    internal class Tencho
    {
        //Port settings
        internal static int PortTencho;
        private static int PortIrc;

        //Client ping timeouts.
        internal const int PING_TIMEOUT_OSU = 48000;
        internal const int PING_TIMEOUT_IRC = 128000;

        internal static int PING_INTERVAL_OSU = 4000;
        internal static int PING_INTERVAL_IRC = 64000;

        internal static Random Random = new Random();

        /// <summary>
        /// Internal protocol version number.
        /// Used to handle cases where clients should support more than one version.
        /// </summary>
        internal const int PROTOCOL_VERSION = 11;

        /// <summary>
        /// Public-visible version
        /// </summary>
        internal const string VERSION = "osu!Tencho";
        internal const string Hostname = "cho.ppy.sh";
        internal static string WebsiteDomain;

        internal static bool AllowMultiplayer = true;

        internal static bool AllowIrcConnections = true;
        internal static bool EnableIPBlackList = false;

        /// <summary>
        /// Can IRC users change their nicknames using /NICK?
        /// </summary>
        internal static bool AllowNickChanges = true;

        /// <summary>
        /// External component to lookup IP-Location pairs.
        /// </summary>
        internal static LookupService GeoIpLookup;

        /// <summary>
        /// Fuck threading, seriously.
        /// </summary>
        internal static TimeSpan LockTimeout = new TimeSpan(0, 0, 0, 10, 0);

        internal static string Topic = VERSION + " - IRC mode";

        private static TcpListener OsuListener;

        internal static pConfigManager Config;
        internal static int OsuMinimumVersion;

        internal static Scheduler Scheduler = new Scheduler();

        [DllImport("winmm.dll")]
        internal static extern uint timeBeginPeriod(uint period);

        [DllImport("winmm.dll")]
        internal static extern uint timeEndPeriod(uint period);

        internal static void Main()
        {
            try
            {
                //Increases Thread.Sleep() precision to ~1ms.
                timeBeginPeriod(1);

                //Set the commons protocol which will be used when receiving/sending bPackets.
                //OsuCommon.ProtocolVersion = PROTOCOL_VERSION;

                ThreadPool.SetMaxThreads(200, 200);

                timer.Start();

                WatchForConfigChanges();

                ReloadConfig();

                InitialiseServices();

                //Make some workers to begin with, so we don't starve ourselves.
                for (int i = 0; i < MaxWorkers; i++)
                    AddWorker();

                Bacon.Monitor();

                StartListening();

                StartBroadcasting();

                MainLoop(); //this blocks and loops forever.
            }
            catch (Exception e)
            {
                File.AppendAllText("crash-error.txt", e + "\n\n\n");
            }
            finally
            {
                timeEndPeriod(1);
            }
        }

        private static void StartBroadcasting()
        {
            Broadcaster.Initialize();
        }

        private static void WatchForConfigChanges()
        {
            fsw = new FileSystemWatcher(Environment.CurrentDirectory);
            fsw.EnableRaisingEvents = true;
            fsw.Changed += fsw_Changed;
        }

        static long lastConfigReload;
        static void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name == "osu!Tencho.cfg" && CurrentTime - lastConfigReload > 1000)
            {
                lastConfigReload = CurrentTime;
                ReloadConfig();
            }
        }

        static int connectionCountIrc;
        static int connectionCountOsu;

        /// <summary>
        /// Keeps the main thread occupied while it isn't actually doing anything.
        /// </summary>
        private static void MainLoop()
        {
            long elapsedMilliseconds = 0;

            Process us = Process.GetCurrentProcess();

            try
            {
                while (true)
                {
                    CurrentTime = timer.ElapsedMilliseconds;

                    if (elapsedMilliseconds % 10000 == 0)
                    {
                        float ram = us.WorkingSet64 / 1048576f;

                        int cps = 0;
                        foreach (TenchoWorker w in Workers.ToArray())
                            cps += (int)w.ProcessRate;

                        string statusString = string.Format("{0:yyyy/MM/dd hh:mm:ss} [i{1} o{2} m{3}] [w{4} cps{5} cl{6}]", DateTime.Now, 0, UserManager.CountProcessing, 0, Workers.Count, Workers[0].ProcessRate, NetClient.TotalClientCount);

                        Bacon.SetStatus(statusString);
                    }

                    if (elapsedMilliseconds % 1000 == 0)
                        CheckWorkerCount();

                    Scheduler.Update();

                    Thread.Sleep(50);
                    elapsedMilliseconds += 50;
                }
            }
            catch (Exception e)
            {
                Bacon.WriteSystem("Logging thread crashed on " + e);
            }
        }

        internal static string[] MessageOfTheDay;

        private static void InitialiseServices()
        {
            //MessageOfTheDay = File.ReadAllLines("Tencho.MOTD");
            //GeoIpLookup = new LookupService("GeoLiteCity.dat", LookupService.GEOIP_MEMORY_CACHE);

            Lobby.Initialize();
        }

        private static void StartListening()
        {
            OsuListener = new TcpListener(IPAddress.Any, PortTencho);

            while (true)
            {
                try
                {
                    OsuListener.Start();
                    OsuListener.BeginAcceptTcpClient(ConnectOsu, OsuListener);
                    Bacon.WriteSystem("Listening on " + PortTencho + " for osu! clients");
                }
                catch
                {
                    Bacon.WriteLine("Failed to bind listeners osu! listener to port " + PortTencho + "!");
                    Thread.Sleep(1000);
                    continue;
                }

                break;
            }
        }

        private static Stopwatch timer = new Stopwatch();

        /// <summary>
        /// Milliseconds elapsed since tencho initialisation.
        /// Has a resolution of 50ms.
        /// </summary>
        internal static long CurrentTime;
        internal static long CurrentTimeAccurate { get { return timer.ElapsedMilliseconds; } }
        private static int MaxWorkers;
        internal static int ClientUpdatesPerSecond;
        internal static int WorkerClientsPerSecond;
        internal static bool ReportClientCounts;

        const int MAX_CLIENTS = 8192;
        public static BufferStack Buffers = new BufferStack(NetClient.MAX_BUFFER_SIZE, MAX_CLIENTS * 2);

        internal static long CurrentTimestamp
        {
            get { return (DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks) / 10000000; }
        }

        #region Workers

        internal static List<TenchoWorker> Workers = new List<TenchoWorker>();
        private static long lastCommission;
        private static FileSystemWatcher fsw;

        private static void CheckWorkerCount()
        {
            int clientsPerWorker = UserManager.CountProcessing / Workers.Count;

            int availableCount = 0;
            for (int i = 0; i < Workers.Count; i++)
            {
                TenchoWorker w = Workers[i];

                if (!w.IsBusy)
                    availableCount++;
            }

            //Each available worker processes approximately 1,0000 clients per second.
            //We are aiming for a maximum 40ms delay to send a client any updates.
            //Therefore we need at least 1 worker thread per 400 clients.
            int minimumCount = Math.Max(1, (int)((float)UserManager.CountProcessing / WorkerClientsPerSecond * ClientUpdatesPerSecond));

            const int max_create_per_round = 5;
            int created = 0;

            while (availableCount < minimumCount && Workers.Count < MaxWorkers && created++ < max_create_per_round)
            {
                AddWorker();
                availableCount++;
            }

            const int timeUntilInitialDecommission = 10000;
            const int timeUntilConsecutiveDecomissions = 3000;

            if (availableCount > minimumCount && CurrentTime - lastCommission > timeUntilInitialDecommission)
            {
                DecommissionWorker(Workers[0]);
                lastCommission = CurrentTime - (timeUntilInitialDecommission - timeUntilConsecutiveDecomissions);
            }
        }

        internal static void DecommissionWorker(TenchoWorker w)
        {
            w.Decommission();
            Bacon.WriteSystem("Decommissioned worker [" + Workers.Count + "]");
            Workers.Remove(w);

            for (int i = 0; i < Workers.Count; i++)
                Workers[i].Id = i;

        }

        private static void AddWorker()
        {
            lastCommission = CurrentTime;
            Workers.Add(TenchoWorker.Create());
            Bacon.WriteSystem("Spawned worker [" + Workers.Count + "]");
        }

        #endregion

        #region ClientConnections

        private static void ConnectOsu(IAsyncResult ar)
        {
            connectionCountOsu++;

            TcpClient client = null;

            try
            {
                client = OsuListener.EndAcceptTcpClient(ar);
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, false);
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
            }
            catch { }

            if (client != null)
            {
                RunThread(wi =>
                {
                    try
                    {
                        Client c = new ClientStream(client);
                        c.InitializeClient();
                    }
                    catch (Exception e)
                    {
                        if (client != null)
                        {
                            try
                            {
                                client.Close();
                            }
                            catch { }
                        }
                    }
                });
            }

            while (true)
            {
                try
                {
                    OsuListener.BeginAcceptTcpClient(ConnectOsu, OsuListener);
                    break;
                }
                catch (Exception e)
                {
                    Bacon.WriteSystem("FATAL: " + e);
                    Thread.Sleep(500);
                }
            }
        }

        #endregion

        internal static void RunThread(WaitCallback work)
        {
            ThreadPool.UnsafeQueueUserWorkItem(work, null);
        }

        internal static void Exit()
        {
            fsw.EnableRaisingEvents = false;
            Config.SaveConfig();
            Environment.Exit(0);
        }

        internal static void ReloadConfig()
        {
            fsw.EnableRaisingEvents = false;

            Bacon.WriteSystem("Reloading configuration");

            try
            {

                if (Config == null)
                    Config = new pConfigManager("osu!Tencho.cfg");
                else
                    Config.LoadConfig();

                PortTencho = Config.GetValue("PortTencho", 16384);

                OsuMinimumVersion = Config.GetValue("OsuMinVer", 0);
                MaxWorkers = Config.GetValue("MaxWorkers", 1);
                ClientUpdatesPerSecond = Config.GetValue("ClientUpdatesPerSecond", 3);
                WorkerClientsPerSecond = Config.GetValue("WorkerClientsPerSecond", 20000);
                ReportClientCounts = Config.GetValue("ReportConnectionCounts", true);
                Bacon.LoggingEnabled = Config.GetValue("Logging", true);
                WebsiteDomain = Config.GetValue("WebsiteDomain","http://osu.ppy.sh");


                Config.SaveConfig();
            }
            catch
            {
                Bacon.WriteSystem("Reloading configuration failed!");
            }

            fsw.EnableRaisingEvents = true;
        }
    }
}