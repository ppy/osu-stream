namespace osu_Tencho
{
    internal enum IrcCommands
    {
        /// <summary>
        /// RFC 2812 Internet Relay Chat: Client Protocol 
        /// </summary>
        RPL_WELCOME = 001,
        /// <summary>
        /// "Welcome to the Internet Relay Network <nick>!<user>@<host>" 
        /// </summary>
        RPL_YOURHOST = 002,
        /// <summary>
        /// "Your host is <servername>, running version <ver>" 
        /// </summary>
        RPL_CREATED = 003,
        /// <summary>
        /// "This server was created <date>" 
        /// </summary>
        RPL_MYINFO = 004,
        /// <summary>
        /// "<servername> <version> <available user modes> <available channel modes>" 
        /// </summary>
        RPL_ISUPPORT = 005,
        /// <summary>
        /// "005 nick PREFIX=(ov)@+ CHANTYPES=#& :are supported by this server" 
        /// </summary>
        RPL_USERHOST = 302,
        /// <summary>
        /// ":*1<reply> *( " " <reply> )" 
        /// </summary>
        RPL_ISON = 303,
        /// <summary>
        /// ":*1<nick> *( " " <nick> )" 
        /// </summary>
        RPL_AWAY = 301,
        /// <summary>
        /// "<nick> :<away message>" 
        /// </summary>
        RPL_UNAWAY = 305,
        /// <summary>
        /// ":You are no longer marked as being away" 
        /// </summary>
        RPL_NOWAWAY = 306,
        /// <summary>
        /// ":You have been marked as being away" 
        /// </summary>
        RPL_WHOISUSER = 311,
        /// <summary>
        /// "<nick> <user> <host> * :<real name>" 
        /// </summary>
        RPL_WHOISSERVER = 312,
        /// <summary>
        /// "<nick> <server> :<server info>" 
        /// </summary>
        RPL_WHOISOPERATOR = 313,
        /// <summary>
        /// "<nick> :is an IRC operator" 
        /// </summary>
        RPL_WHOISIDLE = 317,
        /// <summary>
        /// "<nick> <integer> :seconds idle" 
        /// </summary>
        RPL_ENDOFWHOIS = 318,
        /// <summary>
        /// "<nick> :End of WHOIS list" 
        /// </summary>
        RPL_WHOISCHANNELS = 319,
        /// <summary>
        /// "<nick> :*( ( "@" / "+" ) <channel> " " )" 
        /// </summary>
        RPL_WHOWASUSER = 314,
        /// <summary>
        /// "<nick> <user> <host> * :<real name>" 
        /// </summary>
        RPL_ENDOFWHOWAS = 369,
        /// <summary>
        /// "<nick> :End of WHOWAS" 
        /// </summary>
        RPL_LISTSTART = 321,
        /// <summary>
        /// Obsolete. Not used. 
        /// </summary>
        RPL_LIST = 322,
        /// <summary>
        /// "<channel> <# visible> :<topic>" 
        /// </summary>
        RPL_LISTEND = 323,
        /// <summary>
        /// ":End of LIST" 
        /// </summary>
        RPL_UNIQOPIS = 325,
        /// <summary>
        /// "<channel> <nickname>" 
        /// </summary>
        RPL_CHANNELMODEIS = 324,
        /// <summary>
        /// "<channel> <mode> <mode params>" 
        /// </summary>
        RPL_CREATIONTIME = 329,
        /// <summary>
        /// When the channel/server/something was created.
        /// </summary>
        RPL_NOTOPIC = 331,
        /// <summary>
        /// "<channel> :No topic is set" 
        /// </summary>
        RPL_TOPIC = 332,
        /// <summary>
        /// "<channel> :<topic>" 
        /// </summary>
        RPL_TOPIC_INFO = 333,
        /// <summary>
        /// <channel> <set by> <unixtime> 
        /// </summary>
        RPL_INVITING = 341,
        /// <summary>
        /// "<channel> <nick>" 
        /// </summary>
        RPL_SUMMONING = 342,
        /// <summary>
        /// "<user> :Summoning user to IRC" 
        /// </summary>
        RPL_INVITELIST = 346,
        /// <summary>
        /// "<channel> <invitemask>" 
        /// </summary>
        RPL_ENDOFINVITELIST = 347,
        /// <summary>
        /// "<channel> :End of channel invite list" 
        /// </summary>
        RPL_EXCEPTLIST = 348,
        /// <summary>
        /// "<channel> <exceptionmask>" 
        /// </summary>
        RPL_ENDOFEXCEPTLIST = 349,
        /// <summary>
        /// "<channel> :End of channel exception list" 
        /// </summary>
        RPL_VERSION = 351,
        /// <summary>
        /// "<version>.<debuglevel> <server> :<comments>" 
        /// </summary>
        RPL_WHOREPLY = 352,
        RPL_ENDOFWHO = 315,
        /// <summary>
        /// "<channel> <user> <host> <server> <nick> ( "H" / "G" > ["*"] [ ( "@" / "+" ) ] :<hopcount> <real name>" 
        /// </summary>
        /// <summary>
        /// "<name> :End of WHO list" 
        /// </summary>
        RPL_NAMEREPLY = 353,
        /// <summary>
        /// "( "=" / "*" / "@" ) <channel> :[ "@" / "+" ] <nick> *( " " [ "@" / "+" ] <nick> ) 
        /// </summary>
        RPL_ENDOFNAMES = 366,
        /// <summary>
        /// "<channel> :End of NAMES list" 
        /// </summary>
        RPL_LINKS = 364,
        /// <summary>
        /// "<mask> <server> :<hopcount> <server info>" 
        /// </summary>
        RPL_ENDOFLINKS = 365,
        /// <summary>
        /// "<mask> :End of LINKS list" 
        /// </summary>
        RPL_BANLIST = 367,
        /// <summary>
        /// "<channel> <banmask>" 
        /// </summary>
        RPL_ENDOFBANLIST = 368,
        /// <summary>
        /// "<channel> :End of channel ban list" 
        /// </summary>
        RPL_INFO = 371,
        /// <summary>
        /// ":<string>" 
        /// </summary>
        RPL_ENDOFINFO = 374,
        /// <summary>
        /// ":End of INFO list" 
        /// </summary>
        RPL_MOTDSTART = 375,
        /// <summary>
        /// ":- <server> Message of the day - " 
        /// </summary>
        RPL_MOTD = 372,
        /// <summary>
        /// ":- <text>" 
        /// </summary>
        RPL_ENDOFMOTD = 376,
        /// <summary>
        /// ":End of MOTD command" 
        /// </summary>
        RPL_YOUREOPER = 381,
        /// <summary>
        /// ":You are now an IRC operator" 
        /// </summary>
        RPL_REHASHING = 382,
        /// <summary>
        /// "<config file> :Rehashing" 
        /// </summary>
        RPL_YOURESERVICE = 383,
        /// <summary>
        /// "You are service <servicename>" 
        /// </summary>
        RPL_TIME = 391,
        /// <summary>
        /// "<server> :<string showing server’s local time>" 
        /// </summary>
        RPL_USERSSTART = 392,
        /// <summary>
        /// ":UserID Terminal Host" 
        /// </summary>
        RPL_USERS = 393,
        /// <summary>
        /// ":<username> <ttyline> <hostname>" 
        /// </summary>
        RPL_ENDOFUSERS = 394,
        /// <summary>
        /// ":End of users" 
        /// </summary>
        RPL_NOUSERS = 395,
        /// <summary>
        /// ":Nobody logged in" 
        /// </summary>
        RPL_TRACELINK = 200,
        RPL_TRACECONNECTING = 201,
        /// <summary>
        /// "Link <version & debug level> <destination> <next server> V<protocol version> <link uptime in seconds> <backstream sendq> <upstream sendq>" 
        /// </summary>
        /// <summary>
        /// "Try. <class> <server>" 
        /// </summary>
        RPL_TRACEHANDSHAKE = 202,
        /// <summary>
        /// "H.S. <class> <server>" 
        /// </summary>
        RPL_TRACEUNKNOWN = 203,
        /// <summary>
        /// "???? <class> [<client IP address in dot form>]" 
        /// </summary>
        RPL_TRACEOPERATOR = 204,
        /// <summary>
        /// "Oper <class> <nick>" 
        /// </summary>
        RPL_TRACEUSER = 205,
        /// <summary>
        /// "User <class> <nick>" 
        /// </summary>
        RPL_TRACESERVER = 206,
        RPL_TRACESERVICE = 207,
        /// <summary>
        /// "Serv <class> <int>S <int>C <server> <nick!user|*!*>@<host|server> V<protocol version>" 
        /// </summary>
        /// <summary>
        /// "Service <class> <name> <type> <active type>" 
        /// </summary>
        RPL_TRACENEWTYPE = 208,
        /// <summary>
        /// "<newtype> 0 <client name>" 
        /// </summary>
        RPL_TRACECLASS = 209,
        /// <summary>
        /// "Class <class> <count>" 
        /// </summary>
        RPL_TRACERECONNECT = 210,
        /// <summary>
        /// Unused. 
        /// </summary>
        RPL_TRACELOG = 261,
        /// <summary>
        /// "File <logfile> <debug level>" 
        /// </summary>
        RPL_TRACEEND = 262,
        /// <summary>
        /// "<server name> <version & debug level> :End of TRACE" 
        /// </summary>
        RPL_LOCALUSERS = 265,
        /// <summary>
        /// ":Current local users: 3 Max: 4" 
        /// </summary>
        RPL_GLOBALUSERS = 266,
        /// <summary>
        /// ":Current global users: 3 Max: 4" 
        /// </summary>
        RPL_STATSCONN = 250,
        /// <summary>
        /// "::Highest connection count: 4 (4 clients) (251 since server was (re)started)" 
        /// </summary>
        RPL_STATSLINKINFO = 211,
        RPL_STATSCOMMANDS = 212,
        /// <summary>
        /// "<linkname> <sendq> <sent messages> <sent Kbytes> <received messages> <received Kbytes> <time open>" 
        /// </summary>
        /// <summary>
        /// "<command> <count> <byte count> <remote count>" 
        /// </summary>
        RPL_ENDOFSTATS = 219,
        /// <summary>
        /// "<stats letter> :End of STATS report" 
        /// </summary>
        RPL_STATSUPTIME = 242,
        /// <summary>
        /// ":Server Up %d days %d:%02d:%02d" 
        /// </summary>
        RPL_STATSOLINE = 243,
        /// <summary>
        /// "O <hostmask> * <name>" 
        /// </summary>
        RPL_UMODEIS = 221,
        /// <summary>
        /// "<user mode string>" 
        /// </summary>
        RPL_SERVLIST = 234,
        /// <summary>
        /// "<name> <server> <mask> <type> <hopcount> <info>" 
        /// </summary>
        RPL_SERVLISTEND = 235,
        /// <summary>
        /// "<mask> <type> :End of service listing" 
        /// </summary>
        RPL_LUSERCLIENT = 251,
        /// <summary>
        /// ":There are <integer> users and <integer> services on <integer> servers" 
        /// </summary>
        RPL_LUSEROP = 252,
        /// <summary>
        /// "<integer> :operator(s) online" 
        /// </summary>
        RPL_LUSERUNKNOWN = 253,
        /// <summary>
        /// "<integer> :unknown connection(s)" 
        /// </summary>
        RPL_LUSERCHANNELS = 254,
        /// <summary>
        /// "<integer> :channels formed" 
        /// </summary>
        RPL_LUSERME = 255,
        /// <summary>
        /// ":I have <integer> clients and <integer> servers" 
        /// </summary>
        RPL_ADMINME = 256,
        /// <summary>
        /// ":<admin info>" 
        /// </summary>
        RPL_ADMINEMAIL = 259,
        /// <summary>
        /// ":<admin info>" 
        /// </summary>
        RPL_TRYAGAIN = 263,
        /// <summary>
        /// "<command> :Please wait a while and try again." 
        /// </summary>
        ERR_NOSUCHNICK = 401,
        /// <summary>
        /// "<nickname> :No such nick/channel" 
        /// </summary>
        ERR_NOSUCHSERVER = 402,
        /// <summary>
        /// "<server name> :No such server" 
        /// </summary>
        ERR_NOSUCHCHANNEL = 403,
        /// <summary>
        /// "<channel name> :No such channel" 
        /// </summary>
        ERR_CANNOTSENDTOCHAN = 404,
        /// <summary>
        /// "<channel name> :Cannot send to channel" 
        /// </summary>
        ERR_TOOMANYCHANNELS = 405,
        /// <summary>
        /// "<channel name> :You have joined too many channels" 
        /// </summary>
        ERR_WASNOSUCHNICK = 406,
        /// <summary>
        /// "<nickname> :There was no such nickname" 
        /// </summary>
        ERR_TOOMANYTARGETS = 407,
        /// <summary>
        /// "<target> :<error code> recipients. <abort message>" 
        /// </summary>
        ERR_NOSUCHSERVICE = 408,
        /// <summary>
        /// "<service name> :No such service" 
        /// </summary>
        ERR_NOORIGIN = 409,
        /// <summary>
        /// ":No origin specified" 
        /// </summary>
        ERR_NORECIPIENT = 411,
        /// <summary>
        /// ":No recipient given (<command>)" 
        /// </summary>
        ERR_NOTEXTTOSEND = 412,
        /// <summary>
        /// ":No text to send" 
        /// </summary>
        ERR_NOTOPLEVEL = 413,
        /// <summary>
        /// "<mask> :No toplevel domain specified" 
        /// </summary>
        ERR_WILDTOPLEVEL = 414,
        /// <summary>
        /// "<mask> :Wildcard in toplevel domain" 
        /// </summary>
        ERR_BADMASK = 415,
        /// <summary>
        /// "<mask> :Bad Server/host mask" 
        /// </summary>
        ERR_UNKNOWNCOMMAND = 421,
        /// <summary>
        /// "<command> :Unknown command" 
        /// </summary>
        ERR_NOMOTD = 422,
        /// <summary>
        /// ":MOTD File is missing" 
        /// </summary>
        ERR_NOADMININFO = 423,
        /// <summary>
        /// "<server> :No administrative info available" 
        /// </summary>
        ERR_FILEERROR = 424,
        /// <summary>
        /// ":File error doing <file op> on <file>" 
        /// </summary>
        ERR_NONICKNAMEGIVEN = 431,
        /// <summary>
        /// ":No nickname given" 
        /// </summary>
        ERR_ERRONEUSNICKNAME = 432,
        /// <summary>
        /// "<nick> :Erroneous nickname" 
        /// </summary>
        ERR_NICKNAMEINUSE = 433,
        /// <summary>
        /// "<nick> :Nickname is already in use" 
        /// </summary>
        ERR_NICKCOLLISION = 436,
        /// <summary>
        /// "<nick> :Nickname collision KILL from <user>@<host>" 
        /// </summary>
        ERR_UNAVAILRESOURCE = 437,
        /// <summary>
        /// "<nick/channel> :Nick/channel is temporarily unavailable" 
        /// </summary>
        ERR_USERNOTINCHANNEL = 441,
        /// <summary>
        /// "<nick> <channel> :They aren’t on that channel" 
        /// </summary>
        ERR_NOTONCHANNEL = 442,
        /// <summary>
        /// "<channel> :You‘re not on that channel" 
        /// </summary>
        ERR_USERONCHANNEL = 443,
        /// <summary>
        /// "<user> <channel> :is already on channel" 
        /// </summary>
        ERR_NOLOGIN = 444,
        /// <summary>
        /// "<user> :User not logged in" 
        /// </summary>
        ERR_SUMMONDISABLED = 445,
        /// <summary>
        /// ":SUMMON has been disabled" 
        /// </summary>
        ERR_USERSDISABLED = 446,
        /// <summary>
        /// ":USERS has been disabled" 
        /// </summary>
        ERR_NOTREGISTERED = 451,
        /// <summary>
        /// ":You have not registered" 
        /// </summary>
        ERR_NEEDMOREPARAMS = 461,
        /// <summary>
        /// "<command> :Not enough parameters" 
        /// </summary>
        ERR_ALREADYREGISTRED = 462,
        /// <summary>
        /// ":Unauthorized command (already registered)" 
        /// </summary>
        ERR_NOPERMFORHOST = 463,
        /// <summary>
        /// ":Your host isn’t among the privileged" 
        /// </summary>
        ERR_PASSWDMISMATCH = 464,
        /// <summary>
        /// ":Password incorrect" 
        /// </summary>
        ERR_YOUREBANNEDCREEP = 465,
        /// <summary>
        /// ":You are banned from this server" 
        /// </summary>
        ERR_YOUWILLBEBANNED = 466,
        /// <summary>
        /// Sent by a server to a user to inform that access to the 
        /// </summary>
        ERR_KEYSET = 467,
        /// <summary>
        /// "<channel> :Channel key already set" 
        /// </summary>
        ERR_CHANNELISFULL = 471,
        /// <summary>
        /// "<channel> :Cannot join channel (+l)" 
        /// </summary>
        ERR_UNKNOWNMODE = 472,
        /// <summary>
        /// "<char> :is unknown mode char to me for <channel>" 
        /// </summary>
        ERR_INVITEONLYCHAN = 473,
        /// <summary>
        /// "<channel> :Cannot join channel (+i)" 
        /// </summary>
        ERR_BANNEDFROMCHAN = 474,
        /// <summary>
        /// "<channel> :Cannot join channel (+b)" 
        /// </summary>
        ERR_BADCHANNELKEY = 475,
        /// <summary>
        /// "<channel> :Cannot join channel (+k)" 
        /// </summary>
        ERR_BADCHANMASK = 476,
        /// <summary>
        /// "<channel> :Bad Channel Mask" 
        /// </summary>
        ERR_NOCHANMODES = 477,
        /// <summary>
        /// "<channel> :Channel doesn’t support modes" 
        /// </summary>
        ERR_BANLISTFULL = 478,
        /// <summary>
        /// "<channel> <char> :Channel list is full" 
        /// </summary>
        ERR_NOPRIVILEGES = 481,
        /// <summary>
        /// ":Permission Denied- You‘re not an IRC operator" 
        /// </summary>
        ERR_CHANOPRIVSNEEDED = 482,
        /// <summary>
        /// "<channel> :You‘re not channel operator" 
        /// </summary>
        ERR_CANTKILLSERVER = 483,
        /// <summary>
        /// ":You can’t kill a server!" 
        /// </summary>
        ERR_RESTRICTED = 484,
        /// <summary>
        /// ":Your connection is restricted!" 
        /// </summary>
        ERR_UNIQOPPRIVSNEEDED = 485,
        /// <summary>
        /// ":You‘re not the original channel operator" 
        /// </summary>
        ERR_NOOPERHOST = 491,
        /// <summary>
        /// ":No O-lines for your host" 
        /// </summary>
        ERR_UMODEUNKNOWNFLAG = 501,
        /// <summary>
        /// ":Unknown MODE flag" 
        /// </summary>
        ERR_USERSDONTMATCH = 502,
        /// <summary>
        /// ":Cannot change mode for other users"
        /// </summary>
        RPL_SERVICEINFO = 231
    }
}