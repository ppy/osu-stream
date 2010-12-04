namespace osu_common.Bancho.Requests
{
    public enum RequestType
    {
        /// <summary>
        /// osu! wishes to inform bancho about its current state.
        /// </summary>
        Osu_SendUserStatus,
        /// <summary>
        /// osu! sends a chat message to bancho.
        /// </summary>
        Osu_SendIrcMessage,
        /// <summary>
        /// osu! is closing.
        /// </summary>
        Osu_Exit,
        /// <summary>
        /// osu! wants to get new stats for the local player.
        /// </summary>
        Osu_RequestStatusUpdate,
        /// <summary>
        /// osu! replies to a ping request.
        /// </summary>
        Osu_Pong,
        /// <summary>
        /// Bancho replies to a login request.
        /// </summary>
        Bancho_LoginReply,
        /// <summary>
        /// Bancho warns osu! of an error.
        /// </summary>
        Bancho_CommandError,
        /// <summary>
        /// Bancho is proxying an irc message to osu!.
        /// </summary>
        Bancho_SendIrcMessage,
        /// <summary>
        /// Bancho is requesting a ping from osu!.
        /// </summary>
        Bancho_Ping,
        /// <summary>
        /// Bancho is informing osu! of an IRC username change.
        /// </summary>
        Bancho_HandleIrcChangeUsername,
        /// <summary>
        /// Bancho is informing osu! of an IRC user quitting.
        /// </summary>
        Bancho_HandleIrcQuit,
        /// <summary>
        /// Bancho is informing osu! of an IRC user joining.
        /// </summary>
        Bancho_HandleIrcJoin,
        /// <summary>
        /// Bancho is informing osu! of a stat update for another user.
        /// </summary>
        Bancho_HandleOsuUpdate,
        /// <summary>
        /// Bancho is informing osu! that an osu! user quit.
        /// </summary>
        Bancho_HandleOsuQuit,
        /// <summary>
        /// Tells the host that a spectator has joined.
        /// </summary>
        Bancho_SpectatorJoined,
        /// <summary>
        /// Tells the host that a spectator has left.
        /// </summary>
        Bancho_SpectatorLeft,
        /// <summary>
        /// Bancho is sending spectator frames (as a bundle) to spectators.
        /// </summary>
        Bancho_SpectateFrames,
        /// <summary>
        /// osu! client has requested to spectate someone.
        /// </summary>
        Osu_StartSpectating,
        /// <summary>
        /// osu! client wants to stop spectating altogether.
        /// </summary>
        Osu_StopSpectating,
        /// <summary>
        /// osu! is sending gameplay frames to be redistributed to spectators.
        /// </summary>
        Osu_SpectateFrames,
        /// <summary>
        /// Bancho is telling osu! to check for new versions.
        /// </summary>
        Bancho_VersionUpdate,
        /// <summary>
        /// osu! is sending an error report to be forwarded to peppy.
        /// </summary>
        Osu_ErrorReport,
        /// <summary>
        /// osu! is informing Bancho that it is unable to spectate the current host.
        /// </summary>
        Osu_CantSpectate,
        /// <summary>
        /// Bancho is informing the host that a spectator can't tune in.
        /// </summary>
        Bancho_SpectatorCantSpectate,
        /// <summary>
        /// Bancho forces osu!'s chat window to surface.
        /// </summary>
        Bancho_GetAttention,
        /// <summary>
        /// Bancho wants osu! to display an announcement popup.
        /// </summary>
        Bancho_Announce,
        /// <summary>
        /// Bancho is forwarding a private message from another osu!/IRC client.
        /// </summary>
        Osu_SendIrcMessagePrivate,
        /// <summary>
        /// Bancho is sending an update for a particular match's details.
        /// </summary>
        Bancho_MatchUpdate,
        /// <summary>
        /// Bancho is sending a new match entry.
        /// </summary>
        Bancho_MatchNew,
        /// <summary>
        /// Bancho is sending notification that a match has been disbanded.
        /// </summary>
        Bancho_MatchDisband,
        /// <summary>
        /// osu! has left the multiplayer lobby.
        /// </summary>
        Osu_LobbyPart,
        /// <summary>
        /// osu! has joined the multiplayer lobby.
        /// </summary>
        Osu_LobbyJoin,
        /// <summary>
        /// osu! has created a new multiplayer match.
        /// </summary>
        Osu_MatchCreate,
        /// <summary>
        /// osu! wants to join a multiplayer match.
        /// </summary>
        Osu_MatchJoin,
        /// <summary>
        /// osikeu! wants to leave the current match.
        /// </summary>
        Osu_MatchPart,
        /// <summary>
        /// Bancho informs osu! that a player has joined the lobby.
        /// </summary>
        Bancho_LobbyJoin,
        /// <summary>
        /// Bancho informs osu! that a player has left the lobby.
        /// </summary>
        Bancho_LobbyPart,
        Bancho_MatchJoinSuccess,
        Bancho_MatchJoinFail,
        Osu_MatchChangeSlot,
        Osu_MatchReady,
        Osu_MatchLock,
        Osu_MatchChangeSettings,
        Bancho_FellowSpectatorJoined,
        Bancho_FellowSpectatorLeft,
        Osu_MatchStart, AllPlayersLoaded,
        Bancho_MatchStart,
        Osu_MatchScoreUpdate,
        Bancho_MatchScoreUpdate,
        Osu_MatchComplete,
        Bancho_MatchTransferHost,
        Osu_MatchChangeMods,
        Osu_MatchLoadComplete,
        Bancho_MatchAllPlayersLoaded,
        Osu_MatchNoBeatmap,
        Osu_MatchNotReady,
        Osu_MatchFailed,
        Bancho_MatchPlayerFailed,
        Bancho_MatchComplete,
        Osu_MatchHasBeatmap,
        Osu_MatchSkipRequest,
        Bancho_MatchSkip,
        Bancho_Unauthorised,
        Osu_ChannelJoin,
        Bancho_ChannelJoinSuccess,
        Bancho_ChannelAvailable,
        Bancho_ChannelRevoked,
        Bancho_ChannelAvailableAutojoin,
        Osu_BeatmapInfoRequest,
        Bancho_BeatmapInfoReply,
        Osu_MatchTransferHost,
        Bancho_LoginPermissions,
        Bancho_FriendsList,
        Osu_FriendAdd,
        Osu_FriendRemove,
        Bancho_ProtocolNegotiation,
        Bancho_TitleUpdate,
        Osu_MatchChangeTeam,
        Osu_ChannelLeave,
        Osu_ReceiveUpdates,
        Bancho_Monitor,
        Bancho_MatchPlayerSkipped,
        Osu_SetIrcAwayMessage
    }
}