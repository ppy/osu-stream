using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osu_Tencho.Clients;

namespace osu_Tencho.Multiplayer
{
    internal static class Lobby
    {
        static List<ServerMatch> Matches = new List<ServerMatch>();

        internal static void Initialize()
        {
        }

        internal static ServerMatch CreateMatch()
        {
            ServerMatch match = new ServerMatch(getNextMatchId());
            Matches.Add(match);
            return match;
        }

        internal static bool EndMatch(ServerMatch match)
        {
            bool removed = Matches.Remove(match);

            return removed;
        }

        static int currentMatchId;
        private static int getNextMatchId()
        {
            return currentMatchId++;
        }

        internal static ServerMatch Matchmake(NetClient client)
        {
            ServerMatch foundMatch = Matches.Find(m => m.AcceptingPlayers);
            if (foundMatch == null) //didn't find an available match, so create a new one
                foundMatch = CreateMatch();

             if (foundMatch.Join(client))
                return foundMatch;

            return null;
        }
    }
}
