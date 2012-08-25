using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osu_Tencho.Clients;
using osu_common.Tencho;
using osum.Helpers.osu_common.Tencho.Objects;
using osu_common.Tencho.Requests;

namespace osu_Tencho.Multiplayer
{
    internal class ServerMatch : bMatch
    {
        Dictionary<NetClient, bPlayerData> ClientData = new Dictionary<NetClient, bPlayerData>();

        bool Active = true;

        public ServerMatch(int id)
        {
            MatchId = id;
        }

        public override MatchState State
        {
            get
            {
                return base.State;
            }
            set
            {
                base.State = value;
                SendUpdates(RequestType.Tencho_MatchStateChange);
            }
        }

        private void SendUpdates(RequestType updateType)
        {
            foreach (NetClient c in ClientData.Keys)
                c.SendRequest(updateType, this);
        }

        public bool AcceptingPlayers { get { return Active && (State == MatchState.Gathering || State == MatchState.Results); } }

        internal bool RequestState(ClientStream client, MatchState requestedState)
        {
            switch (requestedState)
            {
                case MatchState.SongSelect:
                    if (State != MatchState.Gathering && State != MatchState.Results)
                        return false;
                    State = requestedState;
                    return true;
                case MatchState.Preparing:
                    ClientData[client].State = PlayerState.DifficultySelected;
                    CheckReady();
                    return true;
                case MatchState.Playing:
                    ClientData[client].State = PlayerState.ReadyToPlay;
                    CheckReady();
                    return true;
                case MatchState.Results:
                    ClientData[client].State = PlayerState.FinishedPlaying;
                    CheckReady();
                    return true;
            }

            return false;
        }

        private void CheckReady()
        {
            switch (State)
            {
                case MatchState.DifficultySelect:
                    foreach (bPlayerData p in Players)
                        if (p.State != PlayerState.DifficultySelected)
                            return;

                    //we seem to be ready to start the match.
                    State = MatchState.Preparing;
                    break;
                case MatchState.Preparing:
                    foreach (bPlayerData p in Players)
                        if (p.State != PlayerState.ReadyToPlay)
                            return;
                    State = MatchState.Playing;
                    break;
                case MatchState.Playing:
                    foreach (bPlayerData p in Players)
                        if (p.State != PlayerState.FinishedPlaying)
                            return;
                    State = MatchState.Results;
                    break;
            }
        }

        internal bool Join(NetClient client)
        {
            if (ClientData.ContainsKey(client)) return false;

            bPlayerData data = new bPlayerData() { Username = client.Username, State = PlayerState.Waiting };
            Players.Add(data);

            SendUpdates(RequestType.Tencho_MatchPlayerDataChange);

            //add the client after sending updates.
            ClientData[client] = data;

            
            Console.WriteLine("We have a new client joining this match ({0})! Total players is now {1}", client.Username, ClientData.Count);

            return true;
        }

        internal bool Leave(NetClient client)
        {
            if (!ClientData.ContainsKey(client)) return false;

            Players.Remove(ClientData[client]);
            ClientData.Remove(client);

            SendUpdates(RequestType.Tencho_MatchPlayerDataChange);

            Console.WriteLine("Someone left the match. Only {0} players remain!", ClientData.Count);

            if (ClientData.Count == 0)
            {
                Console.WriteLine("No players left, so this match is going bye-bye");
                if (Lobby.EndMatch(this))
                    Active = false;
            }

            return true;
        }

        internal bool ChangeSong(ClientStream client, bBeatmap beatmap)
        {
            if (State != MatchState.SongSelect && State != MatchState.DifficultySelect)
                return false;

            Beatmap = beatmap;
            if (Beatmap.Filename == null)
            {
                State = MatchState.SongSelect;
            }
            else
                State = MatchState.DifficultySelect;

            return true;
        }

        internal void UpdatePlayer(ClientStream client, bPlayerData data)
        {
            bPlayerData localData;
            if (!ClientData.TryGetValue(client, out localData))
                return;
            localData.Input = data.Input;

            SendUpdates(RequestType.Tencho_MatchPlayerDataChange);
        }
    }
}
