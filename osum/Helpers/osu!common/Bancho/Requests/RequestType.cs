namespace osu_common.Tencho.Requests
{
    public enum RequestType
    {
        /// <summary>
        /// osu! wishes to inform tencho about its current state.
        /// </summary>
        Osu_SendUserStatus,
        /// <summary>
        /// osu! sends a chat message to tencho.
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
        /// Tencho replies to a login request.
        /// </summary>
        Tencho_LoginReply,
        /// <summary>
        /// Tencho warns osu! of an error.
        /// </summary>
        Tencho_CommandError,
        /// <summary>
        /// Tencho is proxying an irc message to osu!.
        /// </summary>
        Tencho_SendMessage,
        /// <summary>
        /// Tencho is requesting a ping from osu!.
        /// </summary>
        Tencho_Ping,
        /// <summary>
        /// Tencho is informing osu! of an IRC username change.
        /// </summary>
        Tencho_HandleIrcChangeUsername,
        /// <summary>
        /// Tencho is informing osu! of an IRC user quitting.
        /// </summary>
        Tencho_HandleIrcQuit,
        /// <summary>
        /// Tencho is informing osu! of a stat update for another user.
        /// </summary>
        Tencho_HandleOsuUpdate,
        /// <summary>
        /// Tencho is informing osu! that an osu! user quit.
        /// </summary>
        Tencho_HandleOsuQuit,
        /// <summary>
        /// Tells the host that a spectator has joined.
        /// </summary>
        Tencho_SpectatorJoined,
        /// <summary>
        /// Tells the host that a spectator has left.
        /// </summary>
        Tencho_SpectatorLeft,
        /// <summary>
        /// Tencho is sending spectator frames (as a bundle) to spectators.
        /// </summary>
        Tencho_SpectateFrames,
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
        /// Tencho is telling osu! to check for new versions.
        /// </summary>
        Tencho_VersionUpdate,
        /// <summary>
        /// osu! is sending an error report to be forwarded to peppy.
        /// </summary>
        Osu_ErrorReport,
        /// <summary>
        /// osu! is informing Tencho that it is unable to spectate the current host.
        /// </summary>
        Osu_CantSpectate,
        /// <summary>
        /// Tencho is informing the host that a spectator can't tune in.
        /// </summary>
        Tencho_SpectatorCantSpectate,
        /// <summary>
        /// Tencho forces osu!'s chat window to surface.
        /// </summary>
        Tencho_GetAttention,
        /// <summary>
        /// Tencho wants osu! to display an announcement popup.
        /// </summary>
        Tencho_Announce,
        /// <summary>
        /// Tencho is forwarding a private message from another osu!/IRC client.
        /// </summary>
        Osu_SendIrcMessagePrivate,
        /// <summary>
        /// Tencho is sending an update for a particular match's details.
        /// </summary>
        Tencho_MatchStateChange,
        /// <summary>
        /// Tencho is sending a new match entry.
        /// </summary>
        Tencho_MatchNew,
        /// <summary>
        /// Tencho is sending notification that a match has been disbanded.
        /// </summary>
        Tencho_MatchDisband,
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
        /// Tencho informs osu! that a player has joined the lobby.
        /// </summary>
        Tencho_LobbyJoin,
        /// <summary>
        /// Tencho informs osu! that a player has left the lobby.
        /// </summary>
        Tencho_LobbyPart,
        Tencho_MatchJoinSuccess,
        Tencho_MatchJoinFail,
        Osu_MatchChangeSlot,
        Osu_MatchReady,
        Osu_MatchLock,
        Osu_MatchChangeSettings,
        Tencho_FellowSpectatorJoined,
        Tencho_FellowSpectatorLeft,
        Osu_MatchStart, AllPlayersLoaded,
        Tencho_MatchStart,
        Osu_MatchScoreUpdate,
        Tencho_MatchScoreUpdate,
        Osu_MatchComplete,
        Tencho_MatchTransferHost,
        Osu_MatchChangeMods,
        Osu_MatchLoadComplete,
        Tencho_MatchAllPlayersLoaded,
        Osu_MatchNoBeatmap,
        Osu_MatchNotReady,
        Osu_MatchFailed,
        Tencho_MatchPlayerFailed,
        Tencho_MatchComplete,
        Osu_MatchHasBeatmap,
        Osu_MatchSkipRequest,
        Tencho_MatchSkip,
        Tencho_Unauthorised,
        Osu_ChannelJoin,
        Tencho_ChannelJoinSuccess,
        Tencho_ChannelAvailable,
        Tencho_ChannelRevoked,
        Tencho_ChannelAvailableAutojoin,
        Osu_BeatmapInfoRequest,
        Tencho_BeatmapInfoReply,
        Osu_MatchTransferHost,
        Tencho_LoginPermissions,
        Tencho_FriendsList,
        Osu_FriendAdd,
        Osu_FriendRemove,
        Tencho_ProtocolNegotiation,
        Tencho_TitleUpdate,
        Osu_MatchChangeTeam,
        Osu_ChannelLeave,
        Osu_ReceiveUpdates,
        Tencho_Monitor,
        Tencho_MatchPlayerSkipped,
        Osu_SetIrcAwayMessage,
        Tencho_UserPresence,
        Irc_Only,
        Osu_UserStatsRequest,
        Tencho_Restart,
        Osu_Invite,
        Tencho_Invite,
        Tencho_ChannelListingComplete,
        Osu_MatchChangePassword,
        Tencho_MatchChangePassword,
        Stream_Authenticate,
        Tencho_Authenticated,
        Stream_RequestMatch,
        Tencho_MatchFound,
        Stream_RequestStateChange,
        Stream_RequestSong,
        Tencho_MatchPlayerDataChange,
        Stream_InputUpdate
    }
}