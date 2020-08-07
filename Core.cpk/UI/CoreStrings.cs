namespace AtomicTorch.CBND.CoreMod.UI
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class CoreStrings
    {
        // this tooltip is displayed when hovering over a structure to build
        public const string Action_Build = "Build";

        // this tooltip is displayed when hovering over a structure that could be relocated
        public const string Action_Relocate = "Relocate";

        // this tooltip is displayed when hovering over a structure to repair
        public const string Action_Repair = "Repair";

        public const string Ammo_CompatibleWeapons = "Compatible weapons:";

        public const string AutosaveNotification_Completed = "Save completed!";

        public const string AutosaveNotification_Content1 =
            "The server will perform a regular world save.";

        public const string AutosaveNotification_Content2 =
            "A short lag is expected.";

        public const string AutosaveNotification_DelayRemains_Format = "Save will run in: {0}";

        public const string AutosaveNotification_Saving = "Saving...";

        public const string AutosaveNotification_Title = "Server snapshot";

        public const string BrokenObjectLandClaimTooltip_TitleDestroyCountdown = "Destroyed in";

        public const string Button_Accept = "Accept";

        public const string Button_Add = "Add";

        public const string Button_Apply = "Apply";

        public const string Button_Back = "Back";

        public const string Button_BrowseRecipes = "Browse recipes";

        public const string Button_Cancel = "Cancel";

        public const string Button_Close = "Close";

        public const string Button_Configure = "Configure";

        public const string Button_Continue = "Continue";

        public const string Button_Deny = "Deny";

        // button to drain the liquid (such as the barrel content)
        public const string Button_Drain = "Drain";

        // dialog to confirm that the player is agree to dispose the liquid content (in barrel, cistern, etc)
        public const string Button_Drain_DialogConfirmation =
            @"Are you sure you want to drain the contents?
              [br]You will not be able to recover it.";

        public const string Button_Drink = "Drink";

        // For example, the server operator could press "Edit" button to open the editor for the server welcome message.
        public const string Button_Edit = "Edit";

        public const string Button_Help = "Help";

        // button to select/display the next entry
        public const string Button_Next = "Next";

        public const string Button_OK = "OK";

        // button to select/display the previous entry
        public const string Button_Previous = "Previous";

        public const string Button_Quit = "Quit";

        public const string Button_Reset = "Reset";

        public const string Button_Retry = "Retry";

        public const string Button_Save = "Save";

        public const string Button_Upgrade = "Upgrade";

        // visible in HUD bar title
        public const string CharacterStatName_Food = "Food";

        // visible in HUD bar title
        public const string CharacterStatName_Health = "Health";

        // visible in HUD bar title
        public const string CharacterStatName_Stamina = "Stamina";

        // visible in HUD bar title
        public const string CharacterStatName_Water = "Water";

        // to block (mute) chat messages from the selected player
        public const string Chat_MessageMenu_Block = "Block";

        // copy name of the selected player
        public const string Chat_MessageMenu_CopyName = "Copy name";

        public const string Chat_MessageMenu_InviteToParty = "Invite to party";

        // mention the selected player
        public const string Chat_MessageMenu_Mention = "Mention";

        // clicking on this menu entry will open a private chat with the selected player
        public const string Chat_MessageMenu_PrivateMessage = "Private message";

        // to report (for moderation) a chat message
        public const string Chat_MessageMenu_Report = "Report";

        // to unblock (un-mute) chat messages from the selected player
        public const string Chat_MessageMenu_Unblock = "Unblock";

        public const string Chat_PressKeyToOpen_Format = "Press [{0}] to start using chat";

        public const string ChatDisclaimer_Button_DisableChat = "Keep the chat hidden";

        public const string ChatDisclaimer_Button_EnableChat = "Enable chat";

        public const string ChatDisclaimer_Checkbox = "I understand and accept";

        public const string ChatDisclaimer_NotModerated_Community =
            "[b]Please note:[/b] You're playing on a community server, so the chat moderation is the responsibility of the server owner. Usually chats are NOT moderated, and you might be exposed to less-than-pleasant messages or even insults by other players. Only enable the chat if you are willing to accept these possibilities.";

        public const string ChatDisclaimer_NotModerated_Official =
            "[b]Please note:[/b] The chat is NOT moderated and you might be exposed to less-than-pleasant messages or even insults by other players. Only enable the chat if you are willing to accept these possibilities.";

        public const string ChatDisclaimer_Title_Community =
            "Would you like to enable in-game chat on this community server?";

        public const string ChatDisclaimer_Title_Official =
            "Would you like to enable in-game chat on this server?";

        public const string ChatDisclaimer_UseBlock =
            @"If you encounter any inappropriate messages—please click on the message and select ""Report"" or ""Block.""";

        public const string Checkbox_DoNotAskAgain = "Do not ask me again";

        public const string ClanTag_Current = "Clan tag:";

        public const string ClanTag_Exists =
            @"This clan tag is already taken by another team.
              [br]Please select another one.";

        public const string ClanTag_FormatWithName = "[{0}] {1}";

        public const string ClanTag_Invalid = "This clan tag is invalid.";

        public const string ClanTag_Requirements =
            @"Clan tag requirements:
              [*] only Latin letters and digits
              [*] starts with a letter
              [*] four character max";

        public const string ClanTag_Select = "Please select a clan tag";

        public const string ConstructionMetaInfo_Decoration =
            "This structure is purely decorative.";

        public const string ConstructionMetaInfo_ElectricityPowerConsumer =
            "This structure requires electricity to operate.";

        public const string ConstructionMetaInfo_ElectricityPowerProducer =
            "This structure produces electricity.";

        public const string ConstructionMetaInfo_ElectricityPowerStorage = "This structure stores electricity.";

        // an info message for a structure that cannot be relocated in the base once it's built
        public const string ConstructionMetaInfo_Unmovable =
            "This structure is unmovable once built.";

        // used like in "copy-paste"
        public const string Copy = "Copy";

        // Note: during translation, it might be a good idea to keep it as is (MAX) as the space is very limited (4 chars max).
        public const string CraftingCountSelector_Max = "MAX";

        // Note: during translation, it might be a good idea to keep it as is (MIN) as the space is very limited (4 chars max).
        public const string CraftingCountSelector_Min = "MIN";

        // Title for the active crafting queue control. Should not be much wider than the English version!
        public const string CraftingQueue_Title = "CRAFTING";

        public const string CraftingQueueItemControl_ShortcutsDescription =
            @"[b]LMB click[/b]
              [br]Make this recipe first in the queue.
              [br]
              [br][b]RMB click[/b]
              [br]Cancel this recipe.";

        // button to start crafting of the selected recipe
        public const string CraftingRecipeDetails_ButtonStartCraft = "CRAFT";

        public const string CraftingRecipesList_NoRecipesKnown =
            @"You don't know any recipes for this crafting station.
              [br][br][br]
              You can unlock more recipes in the technologies menu.";

        public const string CraftingRecipesList_TitleRecipesUnlocked = "Recipes unlocked";

        public const string CrateIconControl_NoIcon = "No icon";

        public const string CrateIconControl_SelectIcon = "Click here with an item to put an icon on the crate.";

        public const string DamageType_Chemical = "Chemical";

        public const string DamageType_Cold = "Cold";

        public const string DamageType_Explosion = "Explosion";

        public const string DamageType_Heat = "Heat";

        public const string DamageType_Impact = "Impact";

        public const string DamageType_Kinetic = "Kinetic";

        public const string DamageType_Psi = "Psi";

        public const string DamageType_Radiation = "Radiation";

        public const string Defense_Format_Chemical = "Chemical protection: {0}";

        public const string Defense_Format_Cold = "Cold protection: {0}";

        public const string Defense_Format_Explosion = "Explosion protection: {0}";

        public const string Defense_Format_Heat = "Heat protection: {0}";

        public const string Defense_Format_Impact = "Impact protection: {0}";

        public const string Defense_Format_Kinetic = "Kinetic protection: {0}";

        public const string Defense_Format_Psi = "Psi protection: {0}";

        public const string Defense_Format_Radiation = "Radiation protection: {0}";

        public const string Demo_Button_BuyGameOnSteam = "Buy CryoFall on Steam";

        public const string Demo_CanPurchase =
            "You can purchase CryoFall to receive the complete version of the game. It's a one-time purchase, and you will be able to continue playing where you left off with your existing character (if you have played recently). You will also be able to play on the community and modded servers, or even host your own server.";

        public const string Demo_Expired =
            "Your demo time has expired. Thanks for trying out the game—we hope you've enjoyed it!";

        public const string Demo_OnlyOfficialServers =
            @"With this demo version you can play on any of the official game servers.
              [br]Please purchase the full version to play on any other community or private game server or host your own servers.";

        public const string Demo_TimeRemaining =
            "The demo version will expire in:";

        public const string Demo_Title = "CryoFall Demo Version";

        public const string Demo_Welcome =
            "Welcome to the demo version of CryoFall. You can play the game with no restrictions (you have complete access to all game features on official game servers) for the duration of this demo version.";

        // This message displayed over an oil seep or any other recently spawned resource deposit.
        // It cannot be claimed instantly.
        // It's displayed like: Available for claim in: 29 minutes 59 seconds.
        public const string DepositAvailableForClaiming_Message = "Available to claim in:";

        public const string DepositCapacityStats_Depleted = "This deposit has been depleted.";

        public const string DepositCapacityStats_Message = "This deposit will be depleted in:";

        public const string DepositCapacityStats_Title = "Deposit capacity";

        public const string DepositCapacityStats_TitleInfinite = "Deep deposit (low yield, infinite)";

        public const string DepositCapacityStats_TitleLimited = "Surface deposit (high yield)";

        public const string Dialog_AreYouSureWantToUse_Format =
            "Are you sure want to use {0}?";

        // Max number of simultaneously controlled drones by a remote control device.
        public const string DroneControl_MaxSimultaneouslyControlledDrones =
            "Simultaneously controlled drones";

        public const string Duration_Instant = "Instant";

        public const string Duration_Quick = "Quick";

        public const string Duration_Slow = "Slow";

        public const string Duration_VeryQuick = "Very quick";

        public const string Duration_VerySlow = "Very slow";

        public const string EffectActionDescription_AddsEffects = "Adds effects:";

        public const string EffectActionDescription_RemovesEffects = "Removes effects:";

        public const string EnergyUnitAbbreviation = "EU";

        public const string EnergyUnitPerSecondAbbreviation = "EU/s";

        public const string EventDamage_FailureMessage_Format =
            "You need to deal at least {0}% total damage to be eligible for a reward in this event. Good luck next time!";

        public const string EventDamage_FailureTitle_Format =
            "Not enough damage";

        public const string EventDamage_SuccessMessage_Format =
            "You dealt {0}% total damage! Your share of the loot is {1}/{2}.";

        public const string Extras_DiscordTooltip = "Official CryoFall Discord server (English only)";

        public const string Extras_FeaturesTooltip = "CryoFall features slideshow";

        public const string Extras_PatchnotesTooltip = "All CryoFall updates patchnotes (English only)";

        public const string Extras_RoadmapTooltip = "CryoFall development roadmap (English only)";

        public const string FarmPlantTooltip_NotWatered = "Not watered";

        public const string FarmPlantTooltip_PossibleHarvestsCount = "Possible harvests";

        public const string FarmPlantTooltip_TitleHarvestInCountdown = "Harvest in";

        public const string FarmPlantTooltip_TitleHarvestInCountdown_Ready = "Ready for harvest";

        public const string FarmPlantTooltip_TitleSpoiled = "Rotten away";

        public const string FarmPlantTooltip_TitleSpoiledInCountdown = "Spoiled in";

        public const string FarmPlantTooltip_WateredForDuration = "Watered for";

        public const string Field_Email = "Email";

        public const string Field_Password = "Password";

        public const string Gender_Female = "Female";

        public const string Gender_Male = "Male";

        public const string HUDButtonsBar_DismountVehicle = "Dismount vehicle";

        public const string HUDButtonsBar_MenuTitle_Completionist = "Completionist";

        public const string HUDButtonsBar_MenuTitle_Construction = "Construction";

        public const string HUDButtonsBar_MenuTitle_Crafting = "Crafting";

        public const string HUDButtonsBar_MenuTitle_Equipment = "Equipment";

        public const string HUDButtonsBar_MenuTitle_Map = "Map";

        public const string HUDButtonsBar_MenuTitle_Politics = "Politics";

        public const string HUDButtonsBar_MenuTitle_Quests = "Quests";

        public const string HUDButtonsBar_MenuTitle_Skills = "Skills";

        // this is a menu with players list and party management
        public const string HUDButtonsBar_MenuTitle_Social = "Social";

        public const string HUDButtonsBar_MenuTitle_Technologies = "Technologies";

        public const string HUDEnergyChargeIndicator_TotalCapacity = "Total capacity";

        public const string HUDEnergyChargeIndicator_TotalCharge = "Total charge";

        public const string HUDQuestTrackingEntry_QuestTitleFormat = "Quest: {0}";

        public const string ImplantSlotOnStation_Biomaterial = "Biomaterial";

        public const string ImplantSlotOnStation_Button_Uninstall = "Uninstall";

        public const string ImplantSlotOnStation_ImplantInstalled = "Implant installed";

        public const string ImplantSlotOnStation_InstallImplant = "Install implant";

        public const string ImplantSlotOnStation_NoImplantInstalled = "No implant installed";

        public const string ImplantSlotOnStation_PlaceAnImplantToInstall = "Place an implant to install";

        // used as, for example: "Implant requires: 5 biomaterial"
        public const string ImplantSlotOnStation_Requirement = "Requires";

        public const string Item_SpoiledIn_Format = "Spoiled in: {0}";

        // as part of "Spoiled in: never"
        public const string Item_SpoiledIn_Never = "never";

        public const string ItemsContainer_Button_DisplayShortcuts = "Shortcuts";

        public const string ItemsContainer_Button_MatchDown = "Match Down";

        public const string ItemsContainer_Button_MatchUp = "Match Up";

        public const string ItemsContainer_Button_TakeAll = "Take All";

        public const string ItemsContainer_CapacityExceededTakeOnly =
            "Capacity exceeded. All items are take-out only.";

        public const string ItemsContainer_Title = "Items";

        public const string ItemsContainer_TitleInput = "Input";

        public const string ItemsContainer_TitleOutput = "Output";

        public const string LandClaimPlacementDisplayHelper_LabelBuffer = "Buffer";

        public const string LandClaimPlacementDisplayHelper_LabelTier_Format = "Tier {0}";

        public const string LearningPoints = "Learning points";

        public const string LearningPointsAbbreviation = "LP";

        public const string LinkSteamAccountForm_Title =
            "Please enter your [url=atomictorch.com]AtomicTorch.com[/url] account details to link your Steam account.";

        // used in public servers list and other places
        public const string ListFilters = "Filters";

        public const string LoadingSplashScreen_Title = "LOADING";

        public const string LoginAtomicTorchForm_Button_Login = "Login";

        public const string LoginAtomicTorchForm_KeepMeLoggedIn = "Keep me logged in";

        public const string LoginAtomicTorchForm_Title =
            "Login with [url=atomictorch.com/Account/Register]AtomicTorch.com[/url] account";

        public const string LootHacking_Progress = "Hacking progress";

        public const string MainMenu_TabCredits = "CREDITS";

        public const string MainMenu_TabCurrentGame = "CURRENT GAME";

        public const string MainMenu_TabExtras = "EXTRAS";

        public const string MainMenu_TabHome = "HOME";

        public const string MainMenu_TabOptions = "OPTIONS";

        public const string MainMenu_TabQuit = "QUIT";

        public const string MainMenu_TabServers = "SERVERS";

        public const string MainMenu_TabUpdates = "UPDATES HISTORY";

        public const string MainMenuOverlay_Button_LinkAccount = "Link account";

        public const string MainMenuOverlay_Button_Logout = "Logout";

        public const string MainMenuOverlay_YouAreLoggedInAs = "You are logged in as";

        public const string MasterServer = "Master Server";

        public const string MenuCurrentGame_Button_Disconnect = "Disconnect";

        public const string MenuCurrentGame_Button_ShowWelcomeMessage = "Show welcome message";

        public const string MenuCurrentGame_NotConnected = "Not connected.";

        public const string MenuHome_Forums = "Forums";

        public const string MenuHome_News = "News";

        public const string MenuHome_ReadMore = "Read more...";

        public const string MenuHome_RecentServers = "Recent servers";

        public const string MenuHome_ServerHistoryEmpty =
            @"Server history is empty.
              [br][br]You can use this section to quickly join
              [br]a server you've played recently.";

        // this is a small block with links on our social pages
        public const string MenuHome_Social = "Social";

        public const string MenuLogin_SteamError_Message =
            "Please quit the game, restart the Steam client and launch the game again.";

        public const string MenuLogin_SteamError_Title = "Steam Client Error";

        public const string MenuOptions_DialogUnappliedChanges_Message = "Do you want to apply the changes?";

        public const string MenuOptions_DialogUnappliedChanges_Title = "You didn't apply the changes";

        public const string MenuQuit_QuestionAreYouSure = "Are you sure you want to quit to desktop?";

        public const string MenuServers_AwaitingServerList = "Awaiting server list...";

        public const string MenuServers_Button_AddCustomServer = "Add Custom Server";

        public const string MenuServers_Button_ClearList = "Clear List";

        public const string MenuServers_Button_EditAddress = "Edit Address";

        public const string MenuServers_Button_JoinServer = "Join Server";

        public const string MenuServers_Button_RefreshAll = "Refresh All";

        public const string MenuServers_Button_RefreshServer = "Refresh Server";

        public const string MenuServers_Button_RemoveServer = "Remove Server";

        public const string MenuServers_Checkbox_ShowEmpty = "Show empty";

        public const string MenuServers_Checkbox_ShowIncompatible = "Show incompatible";

        public const string MenuServers_ClickToCopy = "Click to copy it!";

        public const string MenuServers_ColumnInfo = "Server info";

        public const string MenuServers_ListTitleCommunity = "Community";

        public const string MenuServers_ListTitleCustom = "Custom";

        public const string MenuServers_ListTitleFavorite = "Favorite";

        public const string MenuServers_ListTitleFeatured = "Featured";

        public const string MenuServers_ListTitleModded = "Modded";

        public const string MenuServers_ListTitleRecent = "Recent";

        public const string MenuServers_NoServersInThisList = "No servers in this list";

        public const string MenuServers_PublicGUID = "Public GUID";

        // displayed as: "Servers: 15"
        public const string MenuServers_ServersCount = "Servers";

        public const string MenuServers_ServerTag_Community_Description =
            "This server is hosted by a community member. The developers have no control over this server and don't guarantee that you will have a good experience there. The server might have customized rates making the game much easier or harder. The server owner is responsible for its moderation and support.";

        public const string MenuServers_ServerTag_Community_Title = "COMMUNITY";

        public const string MenuServers_ServerTag_Featured_Description =
            "Featured servers usually have a reputation for providing a high-quality experience, similar to official servers. However, these servers are still hosted by the community.";

        public const string MenuServers_ServerTag_Featured_Title = "FEATURED";

        public const string MenuServers_ServerTag_Modded_Description =
            "This server has third-party modifications. You may see significant changes from the original game. Please also remember that modded servers may contain bugs introduced by those modifications. Please use at your own risk.";

        public const string MenuServers_ServerTag_Modded_Title = "MODDED";

        public const string MenuServers_ServerTag_Official_Description =
            "This is one of the official CryoFall servers hosted by the developers.";

        public const string MenuServers_ServerTag_Official_Title = "OFFICIAL";

        public const string MenuServers_ServerTag_PvE_Description =
            @"This is a ""player versus environment"" server designed for peaceful exploration and development.
              You are protected from other players, but not from wildlife and environmental dangers!
              Joining a PvE server is a good choice if this is your first time playing CryoFall.";

        public const string MenuServers_ServerTag_PvE_Title = "PvE";

        public const string MenuServers_ServerTag_PvP_Description =
            @"This is a ""player versus player"" server that focuses on combat, map control and raiding.
              PvP servers have no restrictions—you can kill and be killed here.
              Be prepared to fight!";

        public const string MenuServers_ServerTag_PvP_Title = "PvP";

        public const string Meteorite_CooldownMessage_TimeRemainsFormat = "{0} until cooled down sufficiently.";

        public const string Meteorite_CooldownMessage_TooHotForMining = "The meteorite is still too hot for mining.";

        public const string ModsDisabledWindow_Description =
            @"The game was unable to launch with the mods, so they were disabled.
              [br]This is because at least one of the mods is outdated (incompatible with the current version of the game) or simply broken.
              [br]The game is unable to determine whether it's a particular mod causing an issue or if all of them are incompatible. You can check this by enabling the mods one by one.
              [br]If you want to continue using these mods, please ensure you're using their latest versions.
              [br]
              [br]Disabled mods list:
              [br]{0}";

        public const string ModsDisabledWindow_Title = "CryoFall has disabled the mods";

        public const string Network_Jitter = "Jitter";

        public const string Network_Jitter_SeverityRed =
            @"Looks like your Internet connection to the server is experiencing severe jitter.
              [br]As a consequence, you might experience character movement issues (such as frequent rubber banding), or your hits might miss frequently due to substantial discrepancy with the server.";

        public const string Network_Jitter_SeverityYellow =
            @"Looks like your Internet connection to the server is experiencing substantial jitter.
              [br]As a consequence, you might experience slight movement issues (such as occasional rubber banding), or your hits might miss occasionally due to substantial discrepancy with the server.";

        public const string Network_Perfect =
            @"Looks like your network connection is fine.
              [br]Enjoy the game!";

        public const string Network_PingAverage = "Ping (average)";

        public const string Network_PingAverage_SeverityRed =
            @"Looks like your Internet connection to the server is experiencing severe latency (very large ping).
              [br]As a consequence, some of your actions in the game (which require server response to take effect) will take much longer to execute and process.
              [br]E.g., your hits might often miss due to the substantial discrepancy with the server.
              [br]You might also experience frequent rubber banding or instant position correction.";

        public const string Network_PingAverage_SeverityYellow =
            @"Looks like your Internet connection to the server is experiencing significant latency (large ping).
              [br]As a consequence, some of your actions in the game (which require server response to take effect) will take longer to execute and process.
              [br]E.g. your hits might miss sometimes due to the substantial discrepancy with the server.
              [br]You might also experience some rubber banding or instant position correction.";

        public const string Network_PingFluctuationRange = "Ping fluctuation range";

        public const string Network_PingFluctuationRange_SeverityRed =
            @"Looks like your Internet connection to the server is experiencing severe latency fluctuations.
              [br]Some of the network packets are received with unusually high random lag, so the game has to increase buffering values, which effectively means playing with higher latency (higher ping).";

        public const string Network_PingFluctuationRange_SeverityYellow =
            @"Looks like your Internet connection to the server is experiencing substantial latency fluctuations.
              [br]Some of the network packets are received with unusually high random lag, so the game has to increase buffering values, which effectively means playing with higher latency (higher ping).";

        public const string Network_PingGame = "Ping (game)";

        public const string Network_Suggestions =
            @"[b]Suggestions:[/b]
              [*] For the best experience, it's recommended to play on a server in [b]your region[/b] (if available).
              [*] Consider [b]closing[/b] other applications (especially web browsers, torrent clients, and downloaders).
              [*] Consider switching to a [b]cable connection[/b] if your PC is currently connected via Wi-Fi or mobile connection (wireless connections are often the reason for significant jitter and latency fluctuations).";

        // displayed over an offline player character nickname
        public const string NicknameOfflinePlayer = "Offline Zzz...";

        public const string No = "No";

        public const string Notification_ActionForbidden = "Forbidden";

        public const string Notification_CannotPlaceThere_Title = "Cannot place there";

        public const string Notification_ErrorCannotInteract
            = "Cannot interact";

        public const string Notification_ErrorObjectUsedByAnotherPlayer
            = "This object is being used by another player.";

        public const string Notification_ObstaclesOnTheWay = "Obstacles in the way!";

        public const string Notification_TooFar = "Too far!";

        public const string ObjectAccessModeEditor_CurrentAccessMode = "Current access mode";

        public const string ObjectAccessModeEditor_TitleAccessModeSelection = "Access mode";

        public const string ObjectOwnersList_AddPartyMembers_Button = "Add party members";

        public const string ObjectOwnersList_AddPartyMembers_Dialog =
            "Are you sure you want to include all the party members into this list?";

        public const string ObjectOwnersList_AddPlayer = "Add player";

        public const string ObjectOwnersList_Empty = "No owners";

        public const string ObjectOwnersList_OnlyOwnerCanAdd =
            "Only the owner can add and remove access.";

        public const string ObjectOwnersList_RemoveOwner = "Remove owner";

        public const string ObjectOwnersList_Title = "Owners";

        // It's the same as ObjectOwnersList_Title but has ":" char in the end
        public const string ObjectOwnersList_Title2 = "Owners:";

        public const string ObjectTradingStationDisplay_ModeBuying = "BUYING";

        public const string ObjectTradingStationDisplay_ModeSelling = "FOR SALE";

        public const string OptionsInput_ContainerMenuCategory = "Container menu";

        public const string PartyManagement_Button_CreateParty = "Create party";

        public const string PartyManagement_Button_Invite = "Invite";

        public const string PartyManagement_Button_Leave = "Leave party";

        public const string PartyManagement_CurrentPartySize = "Party size";

        public const string PartyManagement_HowToInvite =
            "Enter player name below to invite them or right click any player name in the chat.";

        public const string PartyManagement_InviteMember = "Invite member";

        public const string PartyManagement_NoParty =
            @"You don't have a party yet.
              [br]
              [*]You can join a party by receiving and accepting an invitation.
              [*]You can start your own party and invite other players.";

        public const string PartyManagement_Title = "Party";

        public const string Performance__VRAM_SeverityYellow =
            @"Your GPU is having difficulty fitting all required textures into the VRAM, which may result in random stuttering of the rendered image.
              [br]Consider reducing the [b]Texture Quality[/b] option in the [b]Video Options[/b] menu.";

        public const string Performance_FPS_SeverityRed =
            "Looks like your machine is having difficulty processing the game fast enough, which is resulting in a significantly lowered framerate.";

        public const string Performance_FPS_SeverityYellow =
            "Looks like your machine is having difficulty processing the game fast enough, which is resulting in a lowered framerate.";

        public const string Performance_FramerateAverage = "Framerate (average)";

        public const string Performance_Perfect =
            @"Looks like the game's performance is fine.
              [br]Enjoy the game!";

        public const string Performance_Suggestions =
            @"[b]Suggestions:[/b]
              [br]First of all, please make sure your hardware meets the minimum system requirements.
              [br]Then, to improve performance you could try:
              [*] Updating your video card drivers.
              [*] Closing other applications while the game is running (especially web browsers, as they consume a huge amount of system resources even when minimized).
              [*] And if all else fails—reducing the graphics quality in Video Options.";

        public const string Performance_VRAM_SeverityRed =
            @"The VRAM capacity of your GPU is not enough to store all the required textures, so it has to copy them from RAM every frame. As a consequence, you may experience significant stuttering and a reduced framerate.
              [br]Please reduce the [b]Texture Quality[/b] option in the [b]Video Options[/b] menu.";

        public const string Performance_VRAMUsage = "VRAM usage";

        public const string PleaseWait = "Please wait...";

        public const string PowerConsumerState_PowerOff_Title =
            "This power consumer is [b]not connected[/b] to the power grid.";

        public const string PowerConsumerState_PowerOn_Description_Format =
            "This structure will keep working until the power level drops below the shutdown threshold ([b]{0}%[/b]).";

        public const string PowerConsumerState_PowerOn_Title =
            "This power consumer is [b]connected[/b] to the power grid.";

        public const string PowerConsumerState_PowerOnIdle_BelowShutdownThreshold_Description_Format =
            @"The power level in the power grid is below the shutdown threshold ([b]{0}%[/b]).
              [br]This structure will activate automatically when the power level rises above the [b]startup[/b] threshold ([b]{1}%[/b]).";

        public const string PowerConsumerState_PowerOnIdle_BelowStartupThreshold_Description_Format =
            @"The power level in the power grid is below the startup threshold ([b]{0}%[/b]).
              [br]This structure will activate automatically when the power level rises above it.";

        public const string PowerConsumerState_PowerOnIdle_Title = "This power consumer is [b]idle[/b].";

        public const string PowerGrid_NotConnectedMessage = "Not connected to the power grid.";

        public const string PowerGridState_ActiveFormat = "(active {0})";

        // not used anymore
        public const string PowerGridState_Button_RestorePower = "Restore power";

        public const string PowerGridState_Capacity = "Capacity:";

        public const string PowerGridState_Consumers = "Consumers:";

        public const string PowerGridState_CurrentConsumption = "Current consumption:";

        public const string PowerGridState_CurrentGeneration = "Current generation:";

        public const string PowerGridState_DepletedIn = "Depleted in:";

        public const string PowerGridState_DepletedIn_DurationNever = "Never";

        public const string PowerGridState_EfficiencyDescription
            = "Power grid efficiency depends on the size of the base.";

        public const string PowerGridState_EfficiencyFormat = "(efficiency {0}%)";

        public const string PowerGridState_EnergyInformation = "Energy information";

        public const string PowerGridState_Generators = "Generators:";

        public const string PowerGridState_GridSize = "Grid size:";

        // not used anymore
        public const string PowerGridState_GridStatus = "Grid status:";

        // not used anymore
        public const string PowerGridState_MaxDefenseConsumption = "Max defense consumption:";

        public const string PowerGridState_MaxGeneration = "Max generation:";

        public const string PowerGridState_MaxTotalConsumption = "Max total consumption:";

        public const string PowerGridState_PowerGridInformation = "Power grid information";

        // not used anymore
        public const string PowerGridState_Status_Blackout = "Blackout";

        // not used anymore
        public const string PowerGridState_Status_Nominal = "Nominal";

        public const string PowerGridState_Storages = "Storages:";

        public const string PowerProducerState_PowerOff_Title =
            "This power generator is [b]not connected[/b] to the power grid.";

        public const string PowerProducerState_PowerOn_Description_Format =
            "This structure will keep generating power until the shutdown threshold ([b]{0}%[/b]) is reached.";

        public const string PowerProducerState_PowerOn_Title =
            "This power generator is [b]connected[/b] to the power grid.";

        public const string PowerProducerState_PowerOnIdle_AboveShutdownThreshold_Description_Format =
            @"The power level in the power grid is above the shutdown threshold ([b]{0}%[/b]).
              [br]This generator will activate automatically when the power level drops below the [b]startup[/b] ([b]{1}%[/b]) threshold.";

        public const string PowerProducerState_PowerOnIdle_AboveStartupThreshold_Description_Format =
            @"The power level in the power grid is above the startup threshold ([b]{0}%[/b]).
              [br]This generator will activate automatically when the power level drops below it.";

        public const string PowerProducerState_PowerOnIdle_Title = "This power generator is [b]idle[/b].";

        public const string PowerSwitchControl_Button_ConnectFromPowerGrid = "Connect to power grid";

        public const string PowerSwitchControl_Button_DisconnectFromPowerGrid = "Disconnect from power grid";

        public const string PowerSwitchControl_Title_PowerControl = "Power control";

        public const string PowerSwitchControl_Title_PowerGridLevel = "Power grid level";

        public const string PowerSwitchControl_Title_PowerLevelThresholds_Shutdown = "Shutdown:";

        public const string PowerSwitchControl_Title_PowerLevelThresholds_Startup = "Startup:";

        public const string PowerSwitchControl_Title_PowerLevelThresholds_Title = "Power level thresholds";

        public const string Protection_Name = "Protection:";

        public const string Quest_Button_ClaimReward = "Claim";

        public const string Quest_Label_Completed = "Completed";

        public const string Quest_Label_New = "NEW";

        public const string Quest_Label_Ready = "READY";

        public const string QuestionAreYouSure = "Are you sure?";

        public const string RebindKeyWindow_RebindingKey = "Rebinding key";

        public const string RecipesBrowserRecipeDetails_Button_CancelRecipeSelection = "CANCEL";

        public const string RecipesBrowserRecipeDetails_Button_SelectRecipe = "SELECT";

        // After clicking on this notification player will be navigated into the screenshots folder.
        public const string ScreenshotNotification_NotificationFormat =
            "Screenshot ready: {0} (click to open)";

        public const string SelectUsernameForm_Message =
            "Please select a username";

        public const string ServerAddress = "Address";

        public const string ServerPing = "Ping";

        public const string ServerPlayersCount = "Players";

        public const string ServerWipedDate = "Wiped";

        public const string ShieldProtection_ActionRestrictedBaseUnderShieldProtection =
            "You cannot perform this action on a base under shield protection.";

        public const string ShieldProtection_ActivationConfirmation_DelayDuration_Format =
            "Activating the shield will take {0} (activation delay).";

        public const string ShieldProtection_ActivationConfirmation_DelayDurationWithCooldown_Format =
            "Activating the shield will take {0} (activation delay plus the remaining cooldown).";

        public const string ShieldProtection_ActivationConfirmation_ProtectionDuration_Format =
            "With the current charge, the active shield can provide protection for up to {0}.";

        public const string ShieldProtection_ActivationDelay = "Activation delay:";

        public const string ShieldProtection_Button_ActivateShield = "Activate shield";

        public const string ShieldProtection_Button_DeactivateShield = "Deactivate shield";

        public const string ShieldProtection_Button_RechargeShield = "Recharge shield";

        public const string ShieldProtection_CancelledActivationDueToRaidBlock =
            "Shield activation cancelled for your base due to the raid block.";

        public const string ShieldProtection_CannotActivateDuringRaidBlock =
            "Cannot activate the shield during the raid block.";

        public const string ShieldProtection_CooldownDuration = "Cooldown:";

        public const string ShieldProtection_CooldownRemains_Format = "(cooldown {0})";

        public const string ShieldProtection_CurrentShieldCharge = "Current shield charge:";

        public const string ShieldProtection_DeactivationNotes_Format =
            "Deactivating an active or activating shield will start a cooldown ({0}). During the cooldown the shield cannot be activated again.";

        public const string ShieldProtection_Description_1 =
            "Shield protects your base while you are away or offline.";

        public const string ShieldProtection_Description_2 =
            "Under shield protection the base doors and walls are immune to any damage.";

        public const string ShieldProtection_Description_3 =
            "Lockdown: nobody can enter or leave the base under shield protection (all doors are blocked).";

        public const string ShieldProtection_Description_4 =
            "Before activating the shield it must be recharged with electricity.";

        public const string ShieldProtection_Description_5 =
            "Activating the shield takes some time.";

        public const string ShieldProtection_Description_6 =
            "Deactivated shields cannot be reactivated immediately.";

        public const string ShieldProtection_Description_7 =
            "Active shield restricts the use of toolbox and crowbar.";

        public const string ShieldProtection_Description_8 =
            "Shield cannot be recharged while it is active—it must be deactivated first.";

        public const string ShieldProtection_Description_9 =
            "If the base is attacked (raid block started) during the shield activation, shield activation will cancel and activation cooldown will start.";

        public const string ShieldProtection_Dialog_ConfirmActivation =
            "Are you sure you want to activate the shield?";

        public const string ShieldProtection_Dialog_ConfirmDeactivation =
            "Are you sure you want to deactivate the shield?";

        public const string ShieldProtection_DoorBlockedByShield =
            "Lockdown: this door is blocked by active shield protection.";

        public const string ShieldProtection_Error_CannotUpgradeLandClaimUnderShieldProtection =
            "This land claim is under an active shield protection.";

        public const string ShieldProtection_EstimatedDuration = "Estimated duration:";

        public const string ShieldProtection_MaxDuration = "Max duration:";

        public const string ShieldProtection_NotAvailableInsideAnotherBase =
            @"Offline raiding protection shield is not available for this land claim as it's completely inside another base that provides its own shield protection.";

        public const string ShieldProtection_NotificationBaseActivatingShield_Message_Format =
            "Shield will become active in {0} unless interrupted (such as canceled by the base owner or disrupted by raiders applying the raid block).";

        public const string ShieldProtection_NotificationBaseActivatingShield_Title =
            "This base is activating shield protection";

        public const string ShieldProtection_NotificationBaseUnderShield_Message_Format =
            "This base is under shield protection, which can continue for up to {0}.";

        public const string ShieldProtection_NotificationBaseUnderShield_MessageOwner =
            "[u]You can deactivate the shield now (click here).[/u]";

        public const string ShieldProtection_NotificationBaseUnderShield_Title =
            "This base is under shield protection";

        public const string ShieldProtection_NotificationProtected_Message
            = "This base is under shield protection. No damage can be inflicted to any of the walls or doors. Cannot activate the raid block.";

        public const string ShieldProtection_RechargeTo = "Recharge to:";

        public const string ShieldProtection_ShieldStatus = "Shield status:";

        public const string ShieldProtection_Status_Activating = "Activating";

        public const string ShieldProtection_Status_Active = "Active";

        public const string ShieldProtection_Status_Inactive = "Inactive";

        public const string ShieldProtection_UpgradeToUnlock =
            "Offline raiding protection shield is available for higher tiers of the land claim building. Upgrade this claim as soon as possible to unlock this feature.";

        public const string Skill = "Skill";

        public const string StatusEffect_TitleIntensity = "Intensity";

        public const string SteamAccountLinkingBenefitsDescription_Message =
            @"[b]You can:[/b]
              [*] Download and run a Steam-free copy of CryoFall and still have full access to your game with the same characters and servers.
              [*] Get access to AtomicTorch's mailing list with notifications about upcoming features, new version launches and other goodies.
              [*] Participate in private polls available to AtomicTorch users.
              [br]
              [br][b]Coming soon:[/b]
              [*] Rent official cloud servers for CryoFall for you and your friends with the click of a button.";

        public const string SteamAccountLinkingBenefitsDescription_Title =
            "Why is linking to an AtomicTorch account recommended?";

        public const string SteamAccountRegisterOrLinkForm_CheckboxAcceptLicenseAgreement =
            "I accept the [url=atomictorch.com/Pages/Terms-of-Service]Terms of Service[/url], including the [url=atomictorch.com/Pages/Privacy-Policy]Privacy Policy[/url]";

        public const string SteamAccountRegisterOrLinkForm_ConfirmEmail = "Confirm Email";

        public const string SteamAccountRegisterOrLinkForm_Title =
            @"AtomicTorch.com account registration
              [br](for your Steam ID).";

        public const string SupporterPack_Badge = "Supporter badge";

        public const string SupporterPack_Description
            = @"Supporter Pack is a collection of bonus goodies for those who wish to go the extra mile to support the developers of CryoFall. As a sign of appreciation, the game will display a special supporter's badge in the game chat.
  [br][br]The pack doesn't provide any advantage in the game and is purely optional. It includes the game's complete soundtrack, concept art, high resolution game art, chat badge, etc. More items will be added to the Supporter Pack during the Early Access period.
  [br][br]Thank you from the bottom of our hearts for your support!";

        // Displayed like "Vehicles (Tier III)"
        public const string TechGroup_NameWithTier_Format = "{0} ({1})";

        public const string TechGroupTooltip_LearningPointsFormat = "Learning points: {0}";

        public const string TechGroupTooltip_Requirements = "Requirements";

        public const string TechGroupTooltip_TechnologiesAvailableCount = "Technologies available";

        public const string TechGroupTooltip_TechnologiesUnlockedCount = "Technologies unlocked";

        public const string TechGroupTooltip_TimeGate_AvailableNow = "Available now (time-gate expired)";

        public const string TechGroupTooltip_TimeGate_Format = "Will be available in: [b]{0}[/b]";

        public const string TechGroupTooltip_TimeGate_Title = "Time-gate:";

        public const string TechNodeTooltip_Perks = "Perks:";

        public const string TechNodeTooltip_Recipes = "Recipes:";

        public const string TechNodeTooltip_Structures = "Structures:";

        public const string TechNodeTooltip_TechnologicalPrerequisite = "Technological prerequisite";

        public const string TechNodeTooltip_Vehicles = "Vehicles:";

        public const string Technology = "Technology";

        public const string TextBoxSearchPlaceholder = "Search...";

        // used in case when the game needs to display a text like: "(item requires something)"
        public const string TextInParenthesisFormat = "({0})";

        public const string Title = "Title";

        public const string TitleActionFailed = "Action failed";

        public const string TitleArea = "Area";

        public const string TitleAttention = "Attention!";

        public const string TitleConnecting = "Connecting...";

        public const string TitleDestructionTimeout = "Destruction timeout";

        public const string TitleDurability = "Durability";

        public const string TitleEnergyCharge = "Charge";

        public const string TitleFreshness = "Freshness";

        public const string TitleFuel = "Fuel";

        // a skill, tech or building level
        public const string TitleLevel = "Level";

        public const string TitleLiquidGasoline = "Gasoline";

        public const string TitleLiquidMineralOil = "Mineral oil";

        public const string TitleLiquidPetroleum = "Petroleum";

        public const string TitleLiquidWater = "Water";

        public const string TitleLoading = "Loading...";

        public const string TitleModeAlwaysOn = "Always on";

        public const string TitleModeAuto = "Auto";

        public const string TitleModeOff = "Off";

        public const string TitleModeOn = "On";

        public const string TitleNoUpgradesAvailable = "No upgrades available";

        public const string TitleOwner = "Owner";

        public const string TitlePowerGrid = "Power grid";

        public const string TitleSafeStorage = "Safe storage";

        public const string TitleShieldProtection = "S.H.I.E.L.D.";

        public const string TitleStructurePoints = "Structure points";

        public const string TradingStationResources_ThisLotIsDisabled = "This lot is disabled";

        public const string TradingTooltip_CoinsPenny = "Penny coins";

        public const string TradingTooltip_CoinsShiny = "Shiny coins";

        public const string UnregisteredSteamAccountWelcome_Button_ProceedWithoutCreatingAnAccount
            = "Proceed without creating an account";

        public const string UnregisteredSteamAccountWelcome_Title =
            "It seems you are launching CryoFall for the first time. Would you like to link it to your existing [url=atomictorch.com]AtomicTorch.com[/url] account?";

        public const string UnstuckInFormat = "Unstuck in: {0}";

        public const string Vehicle_Ammo = "Ammo";

        public const string Vehicle_Armor = "Armor";

        public const string Vehicle_EnergyLevel = "Energy level";

        public const string Vehicle_Enter = "Enter";

        public const string Vehicle_FuelCells = "Fuel cells";

        public const string Vehicle_Hotbar_ArmorValueFormat = "Armor {1:F0}/{2:F0} ({0}%)";

        public const string Vehicle_Hotbar_EnergyPercentFormat = "Energy {0}%";

        public const string Vehicle_Mech_ItemSlot_LeftArm = "Left arm";

        public const string Vehicle_Mech_ItemSlot_RightArm = "Right arm";

        public const string Vehicle_Mech_ItemSlot_Weapon = "Weapon";

        public const string Vehicle_Mech_ItemSlot_WeaponHardpoint_Format = "Weapon ({0} hardpoint)";

        public const string Vehicle_Mech_NotificationWeaponNeedsInstallationOnMech =
            "This weapon can be used only when installed on a mech.";

        public const string Vehicle_ReactorCores = "Reactor cores";

        public const string VehicleGarage_ButtonPutCurrentVehicle = "Put current vehicle in garage";

        public const string VehicleGarage_ButtonTakeVehicle = "Take vehicle";

        public const string VehicleGarage_TitleAccessibleVehicles = "You have access to the following vehicles:";

        public const string VehicleGarage_TitleNoAccessibleVehicles = "You don't have access to any vehicles yet.";

        public const string VehicleGarage_VehicleStatus_Docked = "Docked";

        public const string VehicleGarage_VehicleStatus_InGarage = "In garage";

        public const string VehicleGarage_VehicleStatus_InUse = "Currently in use";

        public const string VehicleGarage_VehicleStatus_InWorld = "In the world";

        // build the selected vehicle
        public const string VehiclesAssemblyList_Button_Build = "Build";

        public const string VehiclesAssemblyList_NoSchematicsKnown =
            @"You don't have any schematics for this assembly bay.
              [br][br][br]
              You can unlock new schematics in the technologies menu.";

        public const string VehiclesAssemblyList_TitleSchematics = "Schematics";

        public const string VehiclesAssemblyList_TitleSchematicsUnlocked = "Schematics unlocked";

        // victory in an event such as boss fight or base defense
        public const string Victory = "Victory!";

        public const string Weapon_Accuracy = "Accuracy";

        public const string Weapon_AmmoCapacity = "Ammo capacity";

        public const string Weapon_ArmorPiercingCoefficient = "Armor piercing";

        // it's the same as "Fire rate" but for melee weapons only
        public const string Weapon_AttackRate = "Attack rate";

        public const string Weapon_CompatibleAmmo = "Compatible ammo:";

        public const string Weapon_CurrentAmmo = "Current ammo:";

        public const string Weapon_Damage = "Damage";

        public const string Weapon_DamageType = "Damage type";

        // please note that this is only for ranged weapons
        public const string Weapon_FireRate = "Fire rate";

        // for weapons that don't have a fire rate
        public const string Weapon_FireRate_SingleShot = "Single shot";

        public const string Weapon_Range = "Range";

        public const string Weapon_ReloadSpeed = "Reload speed";

        public const string Weapon_Spread = "Spread";

        public const string Weapon_StoppingPower = "Stopping power";

        public const string Weapon_Tooltip_ReferenceAmmo_Format =
            "Stats below are displayed for weapon with a reference ammo ({0}).";

        public const string Window_Tab_Main = "Main";

        public const string WindowAddOrEditServer_PleaseEnterServerAddress =
            @"Please enter server public GUID or host address.
              [br][br]
              Examples:
              [*]localhost
              [*]atomictorch.com:1234
              [*]FF11223344556677889900AABBCCDDEE";

        public const string WindowCharacterStyleCustomization_Button_Randomize = "Random";

        public const string WindowCompletionist_BestFishLength_Format = "Best length: {0} cm";

        public const string WindowCompletionist_BestFishWeight_Format = "Best weight: {0} kg";

        // Accept/claim/receive the reward.
        public const string WindowCompletionist_ButtonAccept = "Accept";

        public const string WindowCompletionist_Notification_FishNewRecord = "New record!";

        public const string WindowCompletionist_ProgressLabel = "Progress:";

        public const string WindowCompletionist_TabCreatures = "Creatures";

        public const string WindowCompletionist_TabFish = "Fish";

        public const string WindowCompletionist_TabFood = "Food";

        public const string WindowCompletionist_TabLoot = "Loot";

        public const string WindowCompletionist_UndiscoveredEntryTitle = "???";

        public const string WindowConstructionMenu_BlueprintRequires = "Blueprint requires";

        public const string WindowConstructionMenu_Button_Build = "Build";

        public const string WindowConstructionMenu_MessageToolboxRequired =
            @"Important! In order to [b]build[/b] or [b]repair[/b] anything, you need to have a [b]toolbox[/b]!
              [br]Equip a toolbox into your hotbar to be able to build.";

        public const string WindowConstructionMenu_PlaceBlueprint = "Place blueprint";

        public const string WindowConstructionMenu_TotalRequired = "Total required";

        public const string WindowCrateIconSelector_CanPickIcon =
            "You can pick an icon from any of the items placed in this container.";

        public const string WindowCrateIconSelector_NoItemsToSelectIcon =
            @"This container is empty.
              [br]There are no items to pick an icon from.";

        public const string WindowCrateIconSelector_SelectedIcon = "Selected icon";

        public const string WindowDataLog_Content = "Content";

        public const string WindowDataLog_From = "From";

        public const string WindowDataLog_Location = "Location";

        public const string WindowDataLog_To = "To";

        public const string WindowGeneratorSolar_SolarPanelsTitle = "SOLAR PANELS";

        public const string WindowInventory_Button_Customize = "Customize";

        public const string WindowInventory_Button_Unstuck = "Unstuck!";

        public const string WindowLandClaim_AllowedItemsDescription =
            "You cannot use land claim to store resources and basic items.";

        public const string WindowLandClaim_DecayInfo = "Decay info";

        public const string WindowLandClaim_DecayInfo_ConfirmationCheckbox =
            "I'm aware of the decay system";

        public const string WindowLandClaim_SafeStorageIsShared =
            "The safe storage items are shared between all the land claim structures on this base.";

        public const string WindowLandClaim_UpgradingOnlyOwnerCanDo = "Only the owner can upgrade this structure.";

        public const string WindowLandClaim_UpgradingRequiresLearningTech =
            "Upgrading this structure requires learning relevant technology.";

        public const string WindowLandClaim_UpgradingTitleRequiredItems = "Required";

        public const string WindowManufacturer_BestMatchingRecipe = "Best matching recipe";

        public const string WindowManufacturer_Button_ApplyMatchingRecipe = "Apply matching recipe";

        public const string WindowManufacturer_CurrentRecipe = "Current recipe";

        public const string WindowManufacturer_ErrorInputItemsDontMatchAnyRecipe =
            "Input items don't match any recipe!";

        public const string WindowManufacturer_ErrorInputItemsDontMatchCurrentRecipe =
            "Input items don't match the current recipe!";

        public const string WindowManufacturer_ErrorNoAvailableSpaceInOutput =
            "No available space in the output!";

        public const string WindowManufacturer_ErrorNotEnoughInputItemsForCurrentRecipe =
            "Not enough input items for current recipe!";

        public const string WindowManufacturer_FuelRequired = "Fuel required!";

        public const string WindowManufacturer_TitleOutputEmpty = "Empty";

        public const string WindowObjectLight_TitleLightMode = "Light mode";

        public const string WindowPolitics_RaidingRestriction_CurrentRaidWindow =
            "Raiding window is now open ({0} left).";

        // displayed as "Raiding any land claims is only possible during the raid window from 7 PM to 10 PM (3h in total)."
        public const string WindowPolitics_RaidingRestriction_DescriptionFormat =
            @"Raiding any land claims is only possible during the raid window from {0} to {1} ({2} in total).
              [br]Structures that are outside of land claims are not protected by this system and can be attacked or destroyed at any time.";

        public const string WindowPolitics_RaidingRestriction_Disabled =
            "This server has offline raiding protection disabled.";

        // displayed as "Next raid window is in 20h 45m 10s"
        public const string WindowPolitics_RaidingRestriction_NextRaidWindowTimeout =
            "Next raid window is in {0}.";

        public const string WindowPolitics_RaidingRestriction_Title =
            "Raiding protection";

        public const string WindowPowerStorage_NotConnectedToPowerGrid =
            "This power storage is not connected to any power grid.";

        public const string WindowQuests_CompletionReward = "Completion reward";

        public const string WindowQuests_CurrentQuests = "Current quests";

        public const string WindowQuests_Hints = "Hints";

        public const string WindowQuests_MessageAllAvailableQuestsCompleted =
            "You have completed all available quests.";

        public const string WindowQuests_MessageNoCompletedText = "You haven't completed any quests yet.";

        public const string WindowQuests_TitleCompletedQuestsCount = "Completed quests";

        public const string WindowQuests_TitleUnlockedQuestsCount = "Unlocked quests";

        public const string WindowRechargingStation_Title = "RECHARGE";

        public const string WindowRespawn_Button_RespawnAtYourBed = "Respawn at your bed";

        public const string WindowRespawn_Button_RespawnInTheWorld = "Respawn in the world";

        public const string WindowRespawn_Button_RespawnNearYourBed = "Respawn near your bed";

        public const string WindowRespawn_DamageSources = "Recent damage sources:";

        public const string WindowRespawn_Message =
            "Looks like you've been forcibly parted from this cruel world...and you lost all your stuff! Do you want revenge?";

        public const string WindowRespawn_MessageDespawned =
            "You've left your character outside of your land claim area for too long and it has been removed from the world. You can respawn again as normal. You did not lose your items.";

        public const string WindowRespawn_MessagePvE =
            "Looks like you've been forcibly parted from this cruel world... Do you want revenge?";

        public const string WindowRespawn_MessageWithNewbieProtection =
            "Looks like you've been killed by another player. But thankfully you still have your newbie protection, so you didn't lose any of your items. Be careful—next time this might be for real!";

        public const string WindowRespawn_TooltipCooldownExplanation =
            @"You've respawned in the bed recently.
              [br]Please wait for the [b]cooldown[/b] timer.";

        public const string WindowSignPicture_Title = "Select picture";

        public const string WindowSkills_CurrentLevelFormat = "Level {0}";

        public const string WindowSkills_CurrentLevelFormat2 = "Current level: {0}";

        public const string WindowSkills_MaxLevelReached = "Max level reached!";

        public const string WindowSkills_NoDiscoveredSkills =
            @"You haven't discovered any skills yet.
              [br][br]
              Skills are automatically unlocked when you accumulate
              [br]enough experience from a particular activity.
              [br]Try something new and see what happens!";

        public const string WindowSkills_ProgressTowardsTheNextLevel = "Progress toward the next level of skill";

        public const string WindowSkills_TitleSkillsDiscoveredCount = "Skills discovered";

        public const string WindowsObjectVehicle_RepairTextFormat =
            "To repair {0}% of armor you need:";

        public const string WindowsObjectVehicle_TabCargo_Title = "Cargo";

        public const string WindowsObjectVehicle_TabConstructVehicle = "Construction";

        public const string WindowsObjectVehicle_TabGarage = "Garage";

        public const string WindowsObjectVehicle_TabVehicleTitle = "Vehicle";

        public const string WindowSocial_OnlineNow = "Online now";

        public const string WindowSocial_ServerStatistics = "Server statistics";

        public const string WindowSocial_TitleOnlinePlayersList = "Players online";

        public const string WindowSocial_TotalPlayers = "Total players";

        public const string WindowSprinker_WaterNow = "Water now";

        public const string WindowSprinkler_NextWateringIn_Format = "Next watering attempt in: [b]{0}[/b]";

        public const string WindowSteamAccountLinking_Button_CreateNewAtomicTorchAccount =
            "Create new AtomicTorch account";

        public const string WindowSteamAccountLinking_Button_LinkToMyExistingAtomicTorch =
            "Link to my existing AtomicTorch account";

        public const string WindowSteamAccountLinking_TitleBenefitsExplanation_WhenSteamAccountExist
            = "You don't have to do it—you can simply continue using your existing Steam account to play, if you prefer. However, there are certain benefits in linking to an AtomicTorch.com account.";

        public const string WindowSteamAccountLinking_TitleBenefitsExplanation_WhenSteamAccountNotExist =
            "You absolutely don't have to do it and can simply use your existing Steam account to play. However, there are certain benefits in doing so.";

        public const string WindowSteamAccountLinking_YouCanLinkYourSteam =
            "You can link your Steam account with your AtomicTorch.com account or create a new one.";

        public const string WindowTechnologies_Info_AdditionalTechnologies =
            @"These additional technologies allow you to further specialize in different areas.
              [br]For higher tiers you are NOT meant to learn them all. Choose wisely.";

        public const string WindowTechnologies_Info_PrimaryTechnologies =
            "Primary technologies are recommended for all players to gain access to a basic technological foundation in CryoFall.";

        public const string WindowTechnologies_TechGroupLocked = "Locked";

        public const string WindowTechnologies_TechGroupPreview = "Preview";

        public const string WindowTechnologies_TechGroupResearch = "Research";

        public const string WindowTechnologies_TechGroupResearchCost = "Research cost";

        public const string WindowTechnologies_TechGroupUnlock = "Unlock";

        public const string WindowTechnologies_TechTreeChanged =
            @"It appears that technology trees have been changed.
              [br]This could be due to a game update or third-party changes (in the case of a community or modded server).
              [br]Your researched technologies were reset and all learning points (LP) spent were refunded, so you can redistribute the points again.
              [br]Enjoy the game!";

        public const string WindowTechnologies_TitlePrimaryTechnologies = "Primary technologies";

        public const string WindowTechnologies_TitleSpecializedTechnologies = "Specialized technologies";

        public const string WindowTechnologies_TooltipExplanationHowToGainLearningPoints =
            @"You can gain LP (learning points) by doing
              [br]any meaningful activity in the game.
              [br]The LP gain is directly proportional to
              [br]your character skill experience gain.";

        public const string WindowTinkerTable_Button_Repair = "Repair";

        public const string WindowTradingStationAdmin_LotConfig = "Lot config";

        public const string WindowTradingStationAdmin_LotsConfiguration = "Lots configuration";

        public const string WindowTradingStationAdmin_RadioButton_GroupTitle = "This trading station will be";

        public const string WindowTradingStationAdmin_RadioButton_ModeBuyingItems = "Buying items";

        public const string WindowTradingStationAdmin_RadioButton_ModeSellingItems = "Selling items";

        public const string WindowTradingStationAdmin_Stock = "Stock";

        public const string WindowTradingStationLotEditor_ListTitleAllItems = "ALL ITEMS";

        public const string WindowTradingStationLotEditor_ListTitleExistingItems = "EXISTING ITEMS";

        public const string WindowTradingStationLotEditor_Price = "Price";

        public const string WindowTradingStationLotEditor_Quantity = "Quantity";

        public const string WindowTradingStationLotEditor_SelectedLotItem = "Selected lot item";

        public const string WindowTradingStationUser_Button_Buy = "Buy";

        public const string WindowTradingStationUser_Button_Sell = "Sell";

        public const string WindowTradingStationUser_BuyingMode_Message
            = "This trading station is buying the following items:";

        public const string WindowTradingStationUser_BuyingMode_Title = "Buying";

        public const string WindowTradingStationUser_SellingMode_Message
            = "This trading station is selling the following items:";

        public const string WindowTradingStationUser_SellingMode_Title = "For sale";

        public const string WindowTrashCan_Title = "You can place any items here to get rid of them permanently.";

        // the game doesn't support custom pluralization so please translate this like a short version: "{0}d ago"
        public const string WipedDate_DaysAgo_Format = "{0} days ago";

        // the game doesn't support custom pluralization so please translate this like a short version: "{0}h ago"
        public const string WipedDate_HoursAgo_Format = "{0} hours ago";

        // the game server was just wiped (less than 2 hours ago)
        public const string WipedDate_JustWiped = "Just wiped";

        // the game server has been wiped yesterday (more than 24 hours but less than 2 days ago)
        public const string WipedDate_Yesterday = "Yesterday";

        public const string WorldMap_MapExplored = "Map explored";

        public const string WorldMap_PointedPosition = "Pointed position";

        public const string WorldMap_YourPosition = "Your position";

        public const string WorldMapMarkCurrentBed_Tooltip =
            @"[b]Bed[/b]
              [br]You can respawn at this location after death.";

        public const string WorldMapMarkDroppedItems_Tooltip =
            @"[b]Dropped items[/b]
              [br]Your dropped items are in this location.";

        public const string WorldMapMarkLandClaim_Tooltip =
            @"[b]Land claim[/b]
              [br]This is the land you have access to.";

        public const string WorldMapMarkLandClaim_Tooltip_LastUsedVehicle =
            "[b]Last used vehicle[/b]";

        public const string WorldMapMarkLandClaim_Tooltip_Owner =
            @"[b]Land claim[/b]
              [br]This is your land claim.";

        public const string WorldMapMarkRaid_Tooltip =
            @"[b]Base being raided[/b]
              [br]Your base is under attack!";

        public const string WorldMapMarkResourceLithium_Infinite_Tooltip =
            @"[b]Infinite geothermal spring[/b]
              [br]Public lithium source.";

        public const string WorldMapMarkResourceLithium_Tooltip =
            @"[b]Geothermal spring[/b]
              [br]Lithium deposit.";

        public const string WorldMapMarkResourceOil_Infinite_Tooltip =
            @"[b]Infinite oil seep[/b]
              [br]Public raw petroleum oil source.";

        public const string WorldMapMarkResourceOil_Tooltip =
            @"[b]Oil seep[/b]
              [br]Raw petroleum oil deposit.";

        public const string WorldMapMarkTradingTerminal_OtherPlayer_Tooltip =
            @"[b]Trading station[/b]
              [br]Another player owns this trading station.";

        public const string WorldMapMarkTradingTerminal_Own_Tooltip =
            @"[b]Trading station (yours)[/b]
              [br]You own this trading station.";

        public const string WorldObjectClaim_Description = "This object is claimed by another player.";

        public const string WorldObjectClaim_Description2 = "You cannot interact with it until the claim expires.";

        public const string WorldObjectClaim_ExpiresIn_Format = "The claim will expire in: {0}";

        public const string WorldObjectClaim_Title = "Object claimed";

        public const string Yes = "Yes";
    }
}