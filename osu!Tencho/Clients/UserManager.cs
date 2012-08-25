using System;
using System.Collections.Generic;

using osu_Tencho.Helpers;
using osu_common.Tencho.Objects;
using osu_common.Tencho.Requests;
using osu_common.Libraries.NetLib;
using System.Threading;
using osu_common.Tencho;

namespace osu_Tencho.Clients
{
    /// <summary>
    /// Handles all user lists with full thread locking.
    /// </summary>
    internal static partial class UserManager
    {
        private static readonly Dictionary<string, Client> clientsByName = new Dictionary<string, Client>(StringComparer.CurrentCultureIgnoreCase);
        private static readonly Dictionary<int, Client> clientsByUserId = new Dictionary<int, Client>();

        private static readonly List<Client> clientsProcessing = new List<Client>();
        private static readonly List<Client> authenticatedClients = new List<Client>();

        public static List<Client> Clients
        {
            get
            {
                List<Client> copy;
                lock (LockUserLists)
                    copy = new List<Client>(authenticatedClients);
                return copy;
            }
        }

        private static readonly object LockUserLists = new object();

        internal static int CountProcessing
        {
            get { return clientsProcessing.Count; }
        }

        internal static Client FindUser(string username, bool verbose = false)
        {
            Client c;

            string search = username.Replace(' ', '_');

            //always prioritise osu! client matches if verbose searching
            if (verbose && search.IndexOf("|osu") < 0)
                if (clientsByName.TryGetValue(search + "|osu", out c))
                    return c;

            if (clientsByName.TryGetValue(search, out c))
                return c;

            return null;
        }

        internal static Client FindUserById(int userId)
        {
            Client c;
            if (clientsByUserId.TryGetValue(userId, out c))
                return c;

            return null;
        }

        internal static void ForEach(Action<Client> action)
        {
            Clients.ForEach(action);
        }

        internal static bool RenameUser(Client client, string newUsername)
        {
            bool wasClient = false;
            bool newNameFree;

            lock (LockRegistration)
                lock (LockUserLists) //checked
                {
                    newNameFree = FindUser(newUsername) == null;
                    if (newNameFree)
                    {
                        if (authenticatedClients.Contains(client))
                        {
                            wasClient = true;
                            clientsByName.Remove(client.UsernameUnderscored);
                            clientsByName[newUsername.Replace(' ', '_')] = client;

                            if (client.UserId >= 0)
                            {
                                clientsByUserId.Remove(client.UserId);
                                clientsByUserId[client.UserId] = client as Client;
                            }
                        }

                        if (wasClient)
                            ForEach(c => c.HandleChangeUsername(client, newUsername));

                        client.Username = newUsername;
                        client.CreatePresence();
                    }
                }
            return newNameFree;
        }

        static object LockRegistration = new object();

        internal static bool RegisterClient(Client client, bool authenticated = false)
        {
            if (!authenticated)
            {
                lock (LockUserLists)
                    clientsProcessing.Add(client);
            }
            else
            {
                lock (LockRegistration)
                {
                    Client existing = FindUser(client.Username);
                    if (existing != null)
                        existing.KillInstant("replaced");

                    lock (LockUserLists)
                    {
                        authenticatedClients.Add(client);
                        clientsByName[client.UsernameUnderscored] = client;
                        clientsByUserId[client.UserId] = client;
                    }

                    client.CreatePresence();

                    foreach (Client other in authenticatedClients)
                    {
                        if (other == client) continue;

                        //if (clientOsu != null) clientOsu.HandlePresence(other);

                        //make other osu! clients aware of us.
                        //ClientOsu otherOsu = other as ClientOsu;
                        //if (otherOsu != null) otherOsu.HandlePresence(client);
                    }

                    //if (client.IsPrivileged)
                    //    client.JoinOtherChannel(ChannelManager.CHANNEL_PRIVILEGED);

                    //if (clientOsu != null)
                    //{
                    //    foreach (Channel c in ChannelManager.GetChannelList())
                    //        if (c.Advertise) clientOsu.HandleChannelAvailable(c, false);
                    //    clientOsu.HandleChannelListingFinished();
                    //}
                }
            }

            return true;
        }

        internal static bool UnregisterClient(Client client, string reason)
        {
            lock (LockRegistration)
            {
                bool wasClient;

                lock (LockUserLists) //checked
                {
                    clientsProcessing.Remove(client);

                    wasClient = authenticatedClients.Remove(client);

                    if (wasClient)
                    {
                        clientsByName.Remove(client.UsernameUnderscored);
                        clientsByUserId.Remove(client.UserId);
                    }
                }

                if (wasClient)
                {
                    //RequestType rt = client is ClientIrc ? RequestType.Tencho_HandleIrcQuit : RequestType.Tencho_HandleOsuQuit;

                    //Request adminRequest = new Request(rt, new bUserQuit(client, reason, true), true);
                    //Request standardRequest = new Request(rt, new bUserQuit(client, reason, false), true);

                    //ForEach(c => c.SendRequest(c.IsAdmin ? adminRequest : standardRequest));
                }

                return wasClient;
            }
        }

        internal static NetClient GetClientForProcessing(TenchoWorker worker)
        {
            try
            {
                int count = CountProcessing;
                if (count == 0) return null;

                int range = (int)Math.Ceiling((float)count / Tencho.Workers.Count);
                int start = range * worker.Id;

                int index = Math.Min(count - 1, start + worker.LastProcessedIndex);

                if (index < start) return null;

                NetClient c = clientsProcessing[index] as NetClient;

                worker.LastProcessedIndex = (worker.LastProcessedIndex + 1) % range;

                return c;
            }
            catch { return null; }
        }
    }
}