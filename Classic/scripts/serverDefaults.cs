$Host::useCustomSkins = 1;

$Host::teamSkin[0] = "blank";
$Host::teamSkin[1] = "base";
$Host::teamSkin[2] = "baseb";
$Host::teamSkin[3] = "swolf";
$Host::teamSkin[4] = "dsword";
$Host::teamSkin[5] = "beagle";
$Host::teamSkin[6] = "cotp";

$Host::teamName[0] = "Unassigned";
$Host::teamName[1] = "Storm";
$Host::teamName[2] = "Inferno";
$Host::teamName[3] = "Starwolf";
$Host::teamName[4] = "Diamond Sword";
$Host::teamName[5] = "Blood Eagle";
$Host::teamName[6] = "Phoenix";

$Host::holoName[0] = "";
$Host::holoName[1] = "Storm";
$Host::holoName[2] = "Inferno";
$Host::holoName[3] = "Starwolf";
$Host::holoName[4] = "DSword";
$Host::holoName[5] = "BloodEagle";
$Host::holoName[6] = "Harbinger";

// -----------------------------------------
// z0dd - ZOD, 9/29/02. Removed T2 demo code
$Host::GameName = "Tribes 2 Test";
$Host::Info = "This is a Tribes 2 Classic server. More information in Discord. <a:playt2.com/discord>playt2.com/discord</a>";
$Host::Map = "VaubanLak";
$Host::MaxPlayers = 30;
$Host::MissionType = "LakRabbit";
// -----------------------------------------

// ------------------------------------------------
// z0dd - ZOD, 7/12/02. New admin feature variables
$Host::AdminPassword = "changethis";
$Host::ClassicSuperAdminPassword = "changeme";
$Host::ClassicAutoRestartServer = 0;                  // Automatically restart server, enable/disable
$Host::ClassicRestartTime = 12;                       // Time in hours to send quit to server
$Host::ClassicEchoChat = 0;                           // Print global chat to server console
$Host::ClassicTelnet = 0;                             // Enable/disable Telnet access to server
$Host::ClassicTelnetPort = 666;                       // Telnet port, must be open on host
$Host::ClassicTelnetPassword = "FullAccessPassword";  // Full access telnet password, can send commands to server
$Host::ClassicTelnetListenPass = "ListenOnyPassword"; // Read only telnet password, cannot send commands to server
$Host::ClassicLogEchoEnabled = 0;                     // Print special messages to server console
$Host::ClassicRandomMissions = 0;                     // Randomly load missions of the same type
$Host::ClassicMaxTelepads = 3;                        // How many special practice CTF pads each player gets
$Host::ClassicRandomizeTeams = 0;                     // Random team selection for players
$Host::ClassicFairTeams = 0;                          // Dissallow players from making teams uneven
$Host::ClassicAutoPWEnabled = 0;                      // Automatic join password setting of server after $Host::ClassicAutoPWPlayerCount is reached
$Host::ClassicAutoPWPassword = "changeit";            // $Host::Password changed to this if $Host::ClassicAutoPWEnabled is enabled
$Host::ClassicAutoPWPlayerCount = 30;                 // When server reaches this number of players, and $Host::ClassicAutoPWEnabled is enabled, join password set to $Host::ClassicAutoPWPassword
$Host::ClassicPacketRateToClient = 24;                // Packets per second sent to clients. Settings: modem 12, Cable 24, T1 32.
$Host::ClassicPacketSize = 400;                       // Size of packets sent to clients. Settings: modem 200, Cable 400, T1 450.
$Host::ClassicUseHighPerformanceCounter = 1;          // Setting to 0 will fix stuttering problem on dual processor servers.
$Host::ClassicLoadTR2Gametype = 0;                    // Option to not load Tr2 gametype
$Host::ClassicConnectLog = 1;                         // Logs all connections to prefs/*Connect.log
$Host::ClassicAntiTurtleTime = 5;                     // How many minutes after a stalemate in CTF are the flags returned
$Host::ClassicLimitArmors = 0;                        // Restrict armor types like turrets, larger team dictates amount avail
$Host::ClassicBadWordFilter = 0;                      // Replace potty mouths words with random garbage
$Host::ClassicAllowConsoleAccess = 0;                 // Allows super admins to use the servers console via Admin hud.
$Host::ClassicNoNullVoiceSpam = 0;                    // Allow or disallow NULL voice usage. 1 enabled NULL voice to be used.
$Host::ClassicBalancedBots = 0;                       // For every client join a bot is disconnected
$Host::ClassicCanKickBots = 0;                        // Allow/disallow vote kicking of bots
$Host::ClassicCycleMisTypes = 0;                      // Cycle to the next mission type every mission load
$Host::ClassicRandomMisTypes = 0;                     // Cycle to a random mission type every mission load
$Host::ClassicAdminLogPath = "prefs";                 // Path to save Admin log files
$Host::ClassicConnLogPath = "prefs";                  // Path to save Connection log files
$Host::ClassicLoadPlasmaTurretChanges = 0;            // Plasma turret does less damage and projectile si slower.
$Host::ClassicLoadHavocChanges = 0;                   // Havoc gets a built in sensor jammer with 20 meter radius.
$Host::ClassicLoadSniperChanges = 0;                  // Sniper Rifle uses ammo with 12 shots and energy.
$Host::ClassicLoadMissileChanges = 0;                 // Handheld missile launcher will not lock onto players and can no-lock fire.
$Host::ClassicLoadMortarChanges = 0;                  // Handheld mortar range limited to 450 meters.
$Host::ClassicLoadBlasterChanges = 0;                 // Blaster shoots 6 projectiles ala shotgun.
$Host::ClassicLoadPlayerChanges = 0;                  // Load up new gameplay changes allowing players to be shot while in vehicles.
$Host::ClassicLoadMineChanges = 0;                    // Enable/Disable mine disc.
$Host::ClassicLoadVRamChanges = 0;                    // Vehicles take damage when ramming players.

// ------------------------------------------------

$Host::AdminList = "";       // all players that will be automatically an admin upon joining server
$Host::SuperAdminList = "";  // all players that will be automatically a super admin upon joining server
$Host::BindAddress = "";     // set to an ip address if the server wants to specify which NIC/IP to use
$Host::Port = 28000;
$Host::Password = "";
$Host::PureServer = 0;
$Host::Dedicated = 0;
$Host::TimeLimit = 45;
$Host::BotCount = 0;
$Host::BotsEnabled = 0;
$Host::MinBotDifficulty = 0.5;
$Host::MaxBotDifficulty = 0.75;
$Host::NoSmurfs = 1;
$Host::VoteTime = 30;               // amount of time before votes are calculated
$Host::VotePassPercent = 60;        // percent needed to pass a vote
$Host::KickBanTime = 20;				// specified in minutes
$Host::BanTime = 43200;				// specified in minutes (43200 is one month)
$Host::PlayerRespawnTimeout = 60;   // time before a dead player is forced into observer mode
$Host::warmupTime = 20;
$Host::TournamentMode = 0;
$Host::allowAdminPlayerVotes = 0;
$Host::FloodProtectionEnabled = 1;
$Host::MaxMessageLen = 120;
$Host::VoteSpread = 20;
$Host::TeamDamageOn = 1;
$Host::Siege::Halftime = 20000;
$Host::CRCTextures = 0;
$Host::ShowIngamePlayerScores = 1;

// 0: .v12 (1.2 kbits/sec), 1: .v24 (2.4 kbits/sec), 2: .v29 (2.9kbits/sec)
// 3:  GSM (6.6 kbits/sec)
$Audio::maxEncodingLevel = 3;
$Audio::maxVoiceChannels = 2;

//Took out MapLimits

//Taco Addons
$Host::EmptyServerReset = 1;					//To control whether the server auto resets when empty
$Host::EmptyServerResetTime = 120;				//Time in Minutes to reset an empty server
$Host::EnableAutobalance = 1;					//Will autobalance when teams are uneven.
$Host::EnableMortarTurret = 0; 					//Enable or Disable Mortar Turret
$Host::EnableNetTourneyClient = 0; 				//Enable or Disable Tourney Net Client checking
$Host::EnableNoBaseRapeNotify = 1; 				//Get a base rape notification
$Host::EnableTeamBalanceNotify = 1; 			//Get a teambalance notification
$Host::EnableTurretPlayerCount = 10; 			//How many to enable turrets
$Host::EnableVoteSoundReminders = 3;			//If you want a sound chime during voting, number of times
$Host::AntiPackEnable = 1; 						//Enable or disable AntiCloak
$Host::AntiPackPlayerCount = 6; 				//How many to enable Cloak
$Host::PUGautoPassword = 0; 					//Auto enable a password in tournament mode
$Host::PUGPassword = "pickup"; 					//PUG password, Auto or enable/disable thru admin menu
$Host::PUGpasswordAlwaysOn = 0;					//If you want the pug password Always on
$Host::DMSLOnlyMode = 0;						//Shocklance Only Mode for Deathmatch
$Host::LCTFProMode = 0;							//Pro mode for LCTF
$Host::LoadingScreenUseDebrief = 1;				//Enable Debrief Style Loading screen; Gives you more lines and MOTD
$Host::LoadScreenColor1 = "05edad";				//Loading Screen color; First Column
$Host::LoadScreenColor2 = "29DEE7";				//Loading Screen color; Second Column
$Host::LoadScreenColor3 = "33CCCC";				//Loading Screen color; Accents
$Host::LoadScreenLine1 = "Join Discord:";											                    //Loading screen Line 1 Topic
$Host::LoadScreenLine1_Msg = "<a:playt2.com/discord>playt2.com/discord</a>";		                    //Loading Screen Line 1 Message
$Host::LoadScreenLine2 = "Game Modes:";												                    //Loading screen Line 2 Topic
$Host::LoadScreenLine2_Msg = "LakRabbit, Capture the Flag, DeathMatch, (Light Only) Capture the Flag";	//Loading Screen Line 2 Message
$Host::LoadScreenLine3 = "Get Mappacks:";										                       	//Loading screen Line 3 Topic
$Host::LoadScreenLine3_Msg = "<a:playt2.com/mappacks>playt2.com/mappacks</a>";		                    //Loading Screen Line 3 Message
$Host::LoadScreenLine4 = "Server Location:";									                      	//Loading screen Line 4 Topic
$Host::LoadScreenLine4_Msg = "Iowa";											                    	//Loading Screen Line 4 Message
$Host::LoadScreenLine5 = "Server Hosted:";									                    		//Loading screen Line 5 Topic (Debrief LoadScreen Only)
$Host::LoadScreenLine5_Msg = "Google Compute Engine";						                    		//Loading Screen Line 5 Message (Debrief LoadScreen Only)
$Host::LoadScreenLine6 = "Server Github:";									                    		//Loading screen Line 6 Topic (Debrief LoadScreen Only)
$Host::LoadScreenLine6_Msg = "https://github.com/ChocoTaco1/TacoServer";	                    		//Loading Screen Line 6 Message (Debrief LoadScreen Only)
$Host::LoadScreenMOTD1 = "Celebrating 20 Years of Tribes2!";						//MOTD or Events Line 1 Message (Debrief LoadScreen Only)
$Host::LoadScreenMOTD2 = "Come play Arena on Wednesday Nights!";					//MOTD or Events Line 2 Message (Debrief LoadScreen Only)
$Host::LoadScreenMOTD3 = "Lak crowd early evenings after work during the week.";	//MOTD or Events Line 3 Message (Debrief LoadScreen Only)
$Host::LoadScreenMOTD4 = "Big CTF games Fridays, Saturdays, and Sundays!";			//MOTD or Events Line 4 Message	(Debrief LoadScreen Only)
$Host::ClassicAdminLog = 1;
$Host::ClassicAdminLogPath = "logs/Admin/log.txt";
$Host::ClassicChatLog = 1;
$Host::ClassicChatLogPath = "logs/Chat/";
$Host::ClassicConnectLog = 1;
$Host::ClassicConnLogPath = "logs/Connect/log.txt";
$Host::ClassicMOTD = "<color:3cb4b4><font:Sui Generis:22>Discord PUB\n<color:3cb4b4><font:Univers:16>Server brought to you by Ravin and Choco\n<color:3cb4b4><font:Univers:16>Celebrating 20 Years of Tribes2!";
$Host::ClassicMOTDLines = 3;
$Host::ClassicMOTDTime = 6;
$Host::ClassicRotationCustom = 1;
$Host::ClassicRotationFile = "prefs/mapRotation.cs";
$Host::ClassicViralBanning = 1;														//Ban ip and other accounts if a banned player trys to rejoin
$Host::ClassicWhitelist = "prefs/whitelist.cs";
$Host::ClassicSuppressTraversalRootError = 1;										//Suppress console spam error on certain indoor maps
$Host::ClassicMaxVotes = 5;															//Max a player can vote per mission
$Host::ClassicVoteLog = 1;															//Log Votes
$Host::ClassicVoteLogPath = "logs/Vote/Vote.log";									//Vote Log location
$Host::ServerRules1 = "\c2if\c4(\c3%client.attitude \c4$= \c5\"fun\" \c4&& \c3%client.Llama_Grabs \c4< \c51\c4) {";
$Host::ServerRules2 = "    \c1keepTeamsBalanced\c4(\c2%client\c4); \c1dontSwitch2WinningTeam(\c2%client\c4); }";
$Host::ServerRules3 = "\c2else if\c4(\c3%client.attitude \c4$= \c5\"too_serious\"\c4) {";
$Host::ServerRules4 = "    \c1try2HaveFun\c4(\c2%client, %attitude\c4); \c1thisIsPubServer\c4(\c2%client, %attitude\c4); }";
$Host::AnimateWithTransitions = 1;													//????????????? T1 engine var may or may not help timescale ?????????????
$Host::AllowAdmin2Admin = 0;
$Host::AllowAdminBan = 0;
$Host::AllowAdminKick = 1;
$Host::AllowAdminPassVote = 1;
$Host::AllowAdminStopVote = 1;
$Host::AllowAdminVotes = 1;
$Host::AllowPlayerVoteChangeMission = 1;
$Host::AllowPlayerVoteSkipMission = 1;
$Host::AllowPlayerVoteTimeLimit = 1;
$Host::AllowPlayerVoteTournamentMode = 1;
$Host::AllowPlayerVoteTeamDamage = 1;
$Host::AllowPlayerVoteNextMission = 1;
$Host::NoBaseRapeEnabled = 1;														//Enable or Disable No Base Rape
$Host::NoBaseRapePlayerCount = 14;													//Min number players the turn off No Base Rape
$Host::AveragePings = 1;															//Show Average ping in F2 menu
$Host::GuidCheck = 1;																//Dont allow players to join without a Guid or Name
$Host::MinFlagRecordPlayerCount = 6;												//Min number players to allow flag record to be recorded
$Host::ItemRespawnTime = 30;														//Time it takes for items to respawn (repairpacks)
$Host::TKMax = 12;																	//Max tks before kick
$Host::TKWarn1 = 4;																	//Max tks before 1st warning
$Host::TKWarn2 = 8;																	//Max tks before 2nd warning
$Host::KickObserverStartOnJoin = 0;													//Start KickObserver Timer as soon as player joins server
$Host::KickObserverTimeout = 1200;													//How long player can stay in observer before being booted (1200 is 20 mins)
//$Host::ClassicBanlist = "prefs/banlist.cs";
$Host::VoteCooldown = 120;															//Time cooldown that dosnt allow a player to vote again after theyve initiated a vote (120 is 2 mins)
$Host::VoteDelayTime = 120;															//Delay the ability to vote (For everyone) at the beginning of the match (120 is 2 minutes)
$Host::ClassicTeamKillLog = 1;                                                      //Enable/Disable Teamkill Logging
$Host::ClassicTeamKillLogPath = "logs/TeamKills/teamkills.log";                     //TeamKill Log Path
$Host::ClassicDailyHardRestart = 0;                                                 //Enable/Disable Daily Hard Restart
$Host::ClassicDailyHardRestartTime = "10\t00\tam";                                  //Hard Restart Server Time Ex:10am
$Host::LoadScreenShowLogo = 0;                                                      //Special Logo on LoadScreen
$Host::LoadScreenShowLogoName = "dpub/DPUB_logo";                                   //Logo location Filename
$Host::MapChangeMSG = 0;															//Center Print between mapchanges
$Host::MapChangeMSGContent = "<color:3cb4b4><font:Sui Generis:22>Pickup Night\n<color:3cb4b4><font:Univers:16>Saturday, March 5th\n<color:3cb4b4><font:Univers:16>Join discord for details";
$Host::MultipleMapRotation = 0;														//Rotate thru multiple map rotation files 1,2,3, etc
$Host::MultipleMapRotationCount = 3;												//How many multiple map rotation files (mapRotation1.cs, mapRotation2.cs, mapRotation3.cs, etc)

//LakRabbit
$Host::LakRabbitUnlimitedDJ = 1;				//Unlimited disc-jumps if enabled
$Host::LakRabbitNoSplashDamage = 1;				//Splash Damage disabled or not
$Host::LakRabbitShowFlagIcon = 1;				//Show flag Icon in lak
$Host::LakRabbitShowFlagTask = 0;				//Does annoying sound every 30 seconds
