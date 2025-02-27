// DisplayName = Spawn CtF

//--- GAME RULES BEGIN ---
//Prevent enemy from capturing your flag
//Score one point for grabbing the enemy's flag
//To capture, your flag must be at its stand
//Score 100 points each time enemy flag is captured
//--- GAME RULES END ---

//--------------------------------------------------------------------------------
//  <> Spawn CTF <>
//
//  Version: 1.1.25026
//  Date: August 09, 2003
//  By: ZOD
//  http://www.planettribes.com/syrinx/
//  Note: For Classic mod only!
//
//--------------------------------------------------------------------------------

//exec the AI scripts
exec("scripts/aiSCtF.cs");
//exec the prefs
exec("prefs/SctfPrefs.cs");

// Setup the default bans
setArmorDefaults("Light");
$Sctf::Armor = "Light";

//-- tracking  ---
function SCtFGame::initGameVars(%game)
{
   %game.SCORE_PER_SUICIDE    = 0;
   %game.SCORE_PER_TEAMKILL   = -10;
   %game.SCORE_PER_DEATH      = 0;
   %game.SCORE_PER_TK_DESTROY = -10;

   %game.SCORE_PER_KILL            = 10;
   %game.SCORE_PER_PLYR_FLAG_CAP   = 30;
   %game.SCORE_PER_PLYR_FLAG_TOUCH = 20;
   %game.SCORE_PER_TEAM_FLAG_CAP   = 100;
   %game.SCORE_PER_TEAM_FLAG_TOUCH = 1;
   %game.SCORE_PER_ESCORT_ASSIST   = 5;
   %game.SCORE_PER_HEADSHOT        = 1;
   %game.SCORE_PER_REARSHOT        = 1;
   %game.SCORE_PER_MIDAIR          = 1;

   %game.SCORE_PER_TURRET_KILL      = 10;
   %game.SCORE_PER_TURRET_KILL_AUTO = 5;
   %game.SCORE_PER_FLAG_DEFEND      = 5;
   %game.SCORE_PER_CARRIER_KILL     = 5;
   %game.SCORE_PER_FLAG_RETURN      = 10;
   %game.SCORE_PER_STALEMATE_RETURN = 15;
   %game.SCORE_PER_GEN_DEFEND       = 5;

   %game.SCORE_PER_DESTROY_GEN         = 10;
   %game.SCORE_PER_DESTROY_SENSOR      = 4;
   %game.SCORE_PER_DESTROY_TURRET      = 5;
   %game.SCORE_PER_DESTROY_ISTATION    = 2;
   %game.SCORE_PER_DESTROY_VSTATION    = 5;
   %game.SCORE_PER_DESTROY_MPBTSTATION = 3;
   %game.SCORE_PER_DESTROY_SOLAR       = 5;
   %game.SCORE_PER_DESTROY_SENTRY      = 4;
   %game.SCORE_PER_DESTROY_DEP_SENSOR = 1;
   %game.SCORE_PER_DESTROY_DEP_INV    = 2;
   %game.SCORE_PER_DESTROY_DEP_TUR    = 3;

   %game.SCORE_PER_DESTROY_SHRIKE    = 5;
   %game.SCORE_PER_DESTROY_BOMBER    = 8;
   %game.SCORE_PER_DESTROY_TRANSPORT = 5;
   %game.SCORE_PER_DESTROY_WILDCAT   = 5;
   %game.SCORE_PER_DESTROY_TANK      = 8;
   %game.SCORE_PER_DESTROY_MPB       = 12;
   %game.SCORE_PER_PASSENGER         = 2;

   %game.SCORE_PER_REPAIR_GEN         = 8;
   %game.SCORE_PER_REPAIR_SENSOR      = 1;
   %game.SCORE_PER_REPAIR_TURRET      = 4;
   %game.SCORE_PER_REPAIR_ISTATION    = 2;
   %game.SCORE_PER_REPAIR_VSTATION    = 4;
   %game.SCORE_PER_REPAIR_MPBTSTATION = 3;
   %game.SCORE_PER_REPAIR_SOLAR       = 4;
   %game.SCORE_PER_REPAIR_SENTRY      = 2;
   %game.SCORE_PER_REPAIR_DEP_SEN     = 1;
   %game.SCORE_PER_REPAIR_DEP_TUR     = 3;
   %game.SCORE_PER_REPAIR_DEP_INV     = 2;

   %game.FLAG_RETURN_DELAY = 45 * 1000;

   %game.TIME_CONSIDERED_FLAGCARRIER_THREAT = 3 * 1000;
   %game.RADIUS_GEN_DEFENSE = 20;
   %game.RADIUS_FLAG_DEFENSE = 20;

   %game.TOUCH_DELAY_MS = 20000;

   %game.fadeTimeMS = 2000;

   %game.notifyMineDist = 7.5;

   %game.stalemate = false;
   %game.stalemateObjsVisible = false;
   %game.stalemateTimeMS = 60000;
   %game.stalemateFreqMS = 15000;
   %game.stalemateDurationMS = 6000;
}

package SCtFGame
{
   function ShapeBase::cleanNonType(%this, %type)
   {
      if(%type $= SCtF)
      {
         for(%h = 0; (%typeList = getWord(%this.missionTypesList, %h)) !$= ""; %h++)
            if(%typeList $= CTF)
               return;
      }
      Parent::cleanNonType(%this, %type);
   }

   function ShapeBaseData::onDestroyed(%data, %obj, %prevstate)
   {
      %scorer = %obj.lastDamagedBy;
      if(!isObject(%scorer))
         return;

      if((%scorer.getType() & $TypeMasks::GameBaseObjectType) && %scorer.getDataBlock().catagory $= "Vehicles")
      {
         %name = %scorer.getDatablock().getName();
         if(%name $= "BomberFlyer" || %name $= "AssaultVehicle")
            %gunnerNode = 1;
         else
            %gunnerNode = 0;

         if(%scorer.getMountNodeObject(%gunnerNode))
         {
            %destroyer = %scorer.getMountNodeObject(%gunnerNode).client;
            %scorer = %destroyer;
            %damagingTeam = %scorer.team;
         }
      }
      else if(%scorer.getClassName() $= "Turret")
      {
         if(%scorer.getControllingClient())
         {
            // manned turret
            %destroyer = %scorer.getControllingClient();
            %scorer = %destroyer;
            %damagingTeam = %scorer.team;
         }
         else
            %scorer = %scorer.owner; // unmanned turret
      }
      if(!%damagingTeam)
         %damagingTeam = %scorer.team;

      if(%damagingTeam != %obj.team)
      {
         if(!%obj.soiledByEnemyRepair)
            Game.awardScoreStaticShapeDestroy(%scorer, %obj);
      }
      else
      {
         if(!%obj.getDataBlock().deployedObject)
            Game.awardScoreTkDestroy(%scorer, %obj);

         return;
      }
   }

   function ShapeBaseData::onDisabled(%data, %obj)
   {
      %obj.wasDisabled = true;
      Parent::onDisabled(%data, %obj);
   }

   function RepairGunImage::onRepair(%this, %obj, %slot)
   {
      Parent::onRepair(%this, %obj, %slot);
      %target = %obj.repairing;
      if(%target && %target.team != %obj.team)
         %target.soiledByEnemyRepair = true;
   }

   function Flag::objectiveInit(%data, %flag)
   {
      if (!%flag.isTeamSkinned)
      {
         %pos = %flag.getTransform();
         %group = %flag.getGroup();
      }
      %flag.originalPosition = %flag.getTransform();
      $flagPos[%flag.team] = %flag.originalPosition;
      %flag.isHome = true;
      %flag.carrier = "";
      %flag.grabber = "";
      setTargetSkin(%flag.getTarget(), CTFGame::getTeamSkin(CTFGame, %flag.team));
      setTargetSensorGroup(%flag.getTarget(), %flag.team);
      setTargetAlwaysVisMask(%flag.getTarget(), 0x7);
      setTargetRenderMask(%flag.getTarget(), getTargetRenderMask(%flag.getTarget()) | 0x2);
      %flag.scopeWhenSensorVisible(true);
      $flagStatus[%flag.team] = "<At Base>";

      //Point the flag and stand at each other
      %group = %flag.getGroup();
      %count = %group.getCount();
      %flag.stand = "";
      for(%i = 0; %i < %count; %i++)
      {
         %this = %group.getObject(%i);
         if(%this.getClassName() !$= "InteriorInstance" && %this.getClassName() !$= "SimGroup" && %this.getClassName() !$= "TSStatic")
         {
            if(%this.getDataBlock().getName() $= "ExteriorFlagStand")
            {
               %flag.stand = %this;
               %this.flag = %flag;
            }
         }
      }
      // set the nametag on the target
      setTargetName(%flag.getTarget(), CTFGame::getTeamName(CTFGame, %flag.team));

      // create a marker on this guy
      %flag.waypoint = new MissionMarker() {
         position = %flag.getTransform();
         dataBlock = "FlagMarker";
      };
      MissionCleanup.add(%flag.waypoint);

      // create a target for this (there is no MissionMarker::onAdd script call)
      %target = createTarget(%flag.waypoint, CTFGame::getTeamName( CTFGame, %flag.team), "", "", 'Base', %flag.team, 0);
      setTargetAlwaysVisMask(%target, 0xffffffff);

      //store the flag in an array
      $TeamFlag[%flag.team] = %flag;
      %flag.static = true;

      %flag.trigger = new Trigger()
      {
         dataBlock = flagTrigger;
         polyhedron = "-0.6 0.6 0.1 1.2 0.0 0.0 0.0 -1.2 0.0 0.0 0.0 2.5";
         position = %flag.position;
         rotation = %flag.rotation;
      };
      MissionCleanup.add(%flag.trigger);
      %flag.trigger.flag = %flag;
   }

   function Flag::onEnterLiquid(%data, %obj, %coverage, %type)
   {
      if(%type > 3)  // 1-3 are water, 4+ is lava and quicksand(?)
      {
          //error("flag("@%obj@") is in liquid type" SPC %type);
          // Changed slightly so this can be cancelled if it leaves the
          // lava before its supposed to be returned - Ilys
          %obj.lavaEnterThread = Game.schedule(3000, "flagReturn", %obj);
      }
   }

   function Flag::onLeaveLiquid(%data, %obj, %type)
   {
      // Added to stop the flag retrun if it slides out of the lava  - Ilys
      if(isEventPending(%obj.lavaEnterThread))
         cancel(%obj.lavaEnterThread);
   }

   function ProjectileData::onCollision(%data, %projectile, %targetObject, %modifier, %position, %normal)
   {
      if(!isObject(%targetObject) && !isObject(%projectile.sourceObject))
         return;
      if(!(%targetObject.getType() & ($TypeMasks::StaticTSObjectType | $TypeMasks::InteriorObjectType |
                                      $TypeMasks::TerrainObjectType | $TypeMasks::WaterObjectType)))
      {
         if(%projectile.sourceObject.team !$= %targetObject.team)
         {
            if(%targetObject.getDataBlock().getClassName() $= "PlayerData" && %data.getName() $= "DiscProjectile")
            {
	         %mask = $TypeMasks::StaticShapeObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType;
	         %start = %targetObject.getWorldBoxCenter();
               %distance = mFloor(VectorDist(%start, %projectile.initialPosition));
	         %end = getWord(%start, 0) SPC getWord(%start, 1) SPC getWord(%start, 2) - 15;
	         %grounded = ContainerRayCast(%start, %end, %mask, 0);
               if(!%grounded)
               {
                  %projectile.sourceObject.client.scoreMidAir++;
                  messageClient(%projectile.sourceObject.client, 'MsgMidAir', '\c0You received a %1 point bonus for a successful mid air shot.', Game.SCORE_PER_MIDAIR, %data.radiusDamageType, %distance);
                  messageTeamExcept(%projectile.sourceObject.client, 'MsgMidAir', '\c5%1 hit a mid air shot.', %projectile.sourceObject.client.name, %data.radiusDamageType, %distance);
                  Game.recalcScore(%projectile.sourceObject.client);
               }
            }
         }
         Parent::onCollision(%data, %projectile, %targetObject, %modifier, %position, %normal);
      }
   }
};

/////////////////////////////////////////////////////////////////////////////////////////
// Mission Functions ////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

function SCtFGame::missionLoadDone(%game)
{
   //default version sets up teams - must be called first...
   DefaultGame::missionLoadDone(%game);

   for(%i = 1; %i < (%game.numTeams + 1); %i++)
      $teamScore[%i] = 0;

   // remove
   MissionGroup.clearFlagWaypoints();

   //reset some globals, just in case...
   $dontScoreTimer[1] = false;
   $dontScoreTimer[2] = false;

   echo( "starting camp thread..." );
   %game.campThread_1 = schedule( 1000, 0, "checkVehicleCamping", 1 );
   %game.campThread_2 = schedule( 1000, 0, "checkVehicleCamping", 2 );
}

function SCtFGame::clientMissionDropReady(%game, %client)
{
   %class = "CTFGame"; // Fake out clients objective hud into thinking this is CTF
   messageClient(%client, 'MsgClientReady',"", %class);
   %game.resetScore(%client);
   for(%i = 1; %i <= %game.numTeams; %i++)
   {
      $Teams[%i].score = 0;
      messageClient(%client, 'MsgCTFAddTeam', "", %i, %game.getTeamName(%i), $flagStatus[%i], $TeamScore[%i]);
   }
   messageClient(%client, 'MsgMissionDropInfo', '\c0You are in mission %1 (%2).', $MissionDisplayName, $MissionTypeDisplayName, $ServerName );
   DefaultGame::clientMissionDropReady(%game, %client);
}

function SCtFGame::assignClientTeam(%game, %client, %respawn)
{
   DefaultGame::assignClientTeam(%game, %client, %respawn);
   // if player's team is not on top of objective hud, switch lines
   messageClient(%client, 'MsgCheckTeamLines', "", %client.team);
}

function SCtFGame::equip(%game, %player)
{
   for(%i = 0; %i < $InventoryHudCount; %i++)
      %player.client.setInventoryHudItem($InventoryHudData[%i, itemDataName], 0, 1);

   %player.client.clearBackpackIcon();
   if(!%player.client.isAIControlled())
   {
      %player.setArmor($Sctf::Armor);
      buyDeployableFavorites(%player.client);
      %player.setEnergyLevel(%player.getDataBlock().maxEnergy);
      %player.selectWeaponSlot( 0 );
   }
   else
   {
      %player.setInventory(EnergyPack, 1);
      %player.setInventory(Chaingun, 1);
      %player.setInventory(ChaingunAmmo, %player.getDataBlock().max[ChaingunAmmo]);
      %player.setInventory(Disc, 1);
      %player.setInventory(DiscAmmo, %player.getDataBlock().max[DiscAmmo]);
      %player.setInventory(GrenadeLauncher, 1);
      %player.setInventory(GrenadeLauncherAmmo, %player.getDataBlock().max[GrenadeLauncherAmmo]);
      %player.setInventory(Grenade, %player.getDataBlock().max[Grenade]);
      %player.setInventory(Mine, %player.getDataBlock().max[Mine]);
      %player.setInventory(Beacon, %player.getDataBlock().max[Beacon]);
      %player.setInventory(RepairKit, 1);
      %player.setInventory(TargetingLaser, 1);
      %player.use("Disc");
   }
   %player.weaponCount = 3;
}

function SCtFGame::timeLimitReached(%game)
{
   logEcho("game over (timelimit)");
   %game.gameOver();
   cycleMissions();
}

function SCtFGame::scoreLimitReached(%game)
{
   logEcho("game over (scorelimit)");
   %game.gameOver();
   cycleMissions();
}

function SCtFGame::gameOver(%game)
{
   // z0dd - ZOD, 5/27/03. Kill the anti-turtle schedule
   if(%game.turtleSchedule !$= "")
   {
      cancel(%game.turtleSchedule);
      %game.turtleSchedule = "";
   }

   // z0dd - ZOD, 9/28/02. Hack for flag collision bug.
   for(%f = 1; %f <= %game.numTeams; %f++)
   {
      cancel($TeamFlag[%f].searchSchedule);
      cancel($TeamFlag[%f].lavaEnterThread); // Kill Ilys lava schedule - ZOD
   }

   // -------------------------------------------
   // z0dd - ZOD, 9/28/02. Cancel camp schedules.
   if( Game.campThread_1 !$= "" )
      cancel(Game.campThread_1);

   if( Game.campThread_2 !$= "" )
      cancel(Game.campThread_2);

   //call the default
   DefaultGame::gameOver(%game);

   //send the winner message
   %winner = "";
   if ($teamScore[1] > $teamScore[2])
      %winner = %game.getTeamName(1);
   else if ($teamScore[2] > $teamScore[1])
      %winner = %game.getTeamName(2);

   switch$(%winner)
   {
	   case 'Storm':
		  messageAll('MsgGameOver', "Match has ended.~wvoice/announcer/ann.stowins.wav");
	   case 'Inferno':
		  messageAll('MsgGameOver', "Match has ended.~wvoice/announcer/ann.infwins.wav");
	   case 'Starwolf':
		  messageAll('MsgGameOver', "Match has ended.~wvoice/announcer/ann.swwin.wav");
	   case 'Blood Eagle':
		  messageAll('MsgGameOver', "Match has ended.~wvoice/announcer/ann.bewin.wav");
	   case 'Diamond Sword':
		  messageAll('MsgGameOver', "Match has ended.~wvoice/announcer/ann.dswin.wav");
	   case 'Phoenix':
		  messageAll('MsgGameOver', "Match has ended.~wvoice/announcer/ann.pxwin.wav");
	   default:
		  messageAll('MsgGameOver', "Match has ended.~wvoice/announcer/ann.gameover.wav");
   }

   messageAll('MsgClearObjHud', "");
   for(%i = 0; %i < ClientGroup.getCount(); %i ++)
   {
      %client = ClientGroup.getObject(%i);
      %game.resetScore(%client);
   }
   for(%j = 1; %j <= %game.numTeams; %j++)
      $TeamScore[%j] = 0;
}

/////////////////////////////////////////////////////////////////////////////////////////
// Flag Functions ///////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

function SCtFGame::playerTouchFlag(%game, %player, %flag)
{
   %client = %player.client;
   if ((%flag.carrier $= "") && (%player.getState() !$= "Dead"))
   {
      // z0dd - ZOD, 5/07/04. Cancel the lava return.
      if(isEventPending(%obj.lavaEnterThread))
         cancel(%obj.lavaEnterThread);

      //flag isn't held and has been touched by a live player
      if (%client.team == %flag.team)
         %game.playerTouchOwnFlag(%player, %flag);
      else
         %game.playerTouchEnemyFlag(%player, %flag);
   }
   // toggle visibility of the flag
   setTargetRenderMask(%flag.waypoint.getTarget(), %flag.isHome ? 0 : 1);
}

function SCtFGame::playerTouchOwnFlag(%game, %player, %flag)
{
   if(%flag.isHome)
   {
      if (%player.holdingFlag !$= "")
         %game.flagCap(%player);
   }
   else
      %game.flagReturn(%flag, %player);

   //call the AI function
   %game.AIplayerTouchOwnFlag(%player, %flag);
}

function SCtFGame::playerTouchEnemyFlag(%game, %player, %flag)
{
   // ---------------------------------------------------------------
   // z0dd, ZOD - 9/27/02. Player must wait to grab after throwing it
   if((%player.flagTossWait !$= "") && %player.flagTossWait)
      return false;

   cancel(%flag.searchSchedule); // z0dd - ZOD, 9/28/02. Hack for flag collision bug.  SquirrelOfDeath, 10/02/02: Moved from PlayerTouchFlag

   cancel(%game.updateFlagThread[%flag]); // z0dd - ZOD, 8/4/02. Cancel this flag's thread to KineticPoet's flag updater
   %game.flagHeldTime[%flag] = getSimTime(); // z0dd - ZOD, 8/15/02. Store time player grabbed flag.

   %client = %player.client;
   %player.holdingFlag = %flag;  //%player has this flag
   %flag.carrier = %player;  //this %flag is carried by %player

   // attach the camera to the flag.carrier
   if($Observers)
   {
	   for(%i = 0; %i < ClientGroup.getCount(); %i++)
	   {
			%cl = ClientGroup.getObject(%i);
			if(%cl.team <= 0 && %cl.observingFlag && %cl.flagObsTeam == %flag.team)
				observeFlag(%cl, %player.client, 2, %flag.team);
	   }
   }

   %player.mountImage(FlagImage, $FlagSlot, true, %game.getTeamSkin(%flag.team));
   %game.playerGotFlagTarget(%player);

   //only cancel the return timer if the player is in bounds...
   if(!%client.outOfBounds)
   {
      cancel($FlagReturnTimer[%flag]);
      $FlagReturnTimer[%flag] = "";
   }

   %player.mountImage(FlagImage, $FlagSlot, true, %game.getTeamSkin(%flag.team));

   %game.playerGotFlagTarget(%player);
   //only cancel the return timer if the player is in bounds...
   if (!%client.outOfBounds)
   {
      cancel($FlagReturnTimer[%flag]);
      $FlagReturnTimer[%flag] = "";
   }

   //if this flag was "at home", see if both flags have now been taken
   if (%flag.isHome)
   {
      // tiebreaker score
      game.awardScoreFlagTouch( %client, %flag );

      %startStalemate = false;
      if ($TeamFlag[1] == %flag)
         %startStalemate = !$TeamFlag[2].isHome;
      else
         %startStalemate = !$TeamFlag[1].isHome;

      if (%startStalemate)
         %game.stalemateSchedule = %game.schedule(%game.stalemateTimeMS, beginStalemate);

   }
   %flag.hide(true);
   %flag.startFade(0, 0, false);
   %flag.isHome = false;
   %flag.setVelocity("0 0 0");
   if(%flag.stand)
      %flag.stand.getDataBlock().onFlagTaken(%flag.stand);//animate, if exterior stand

   $flagStatus[%flag.team] = %client.nameBase;
   %teamName = %game.getTeamName(%flag.team);

   if(%grabspeed)
   {
	  messageTeamExcept(%client, 'MsgCTFFlagTaken', '\c2Teammate %1 took the %2 flag. (Speed: %5Kph)~wfx/misc/flag_snatch.wav', %client.name, %teamName, %flag.team, %client.nameBase, %grabspeed);
	  messageTeam(%flag.team, 'MsgCTFFlagTaken', '\c2Your flag has been taken by %1! (Speed: %5Kph)~wfx/misc/flag_taken.wav',%client.name, 0, %flag.team, %client.nameBase, %grabspeed);
	  messageTeam(0, 'MsgCTFFlagTaken', '\c2%1 took the %2 flag. (Speed: %5Kph)~wfx/misc/flag_snatch.wav', %client.name, %teamName, %flag.team, %client.nameBase, %grabspeed);
	  messageClient(%client, 'MsgCTFFlagTaken', '\c2You took the %2 flag. (Speed: %5Kph)~wfx/misc/flag_snatch.wav', %client.name, %teamName, %flag.team, %client.nameBase, %grabspeed);

      if(%grabspeed > 300)
		messageAll('', "~wfx/Bonuses/high-level4-blazing.wav");
   }
   else
   {
      messageTeamExcept(%client, 'MsgCTFFlagTaken', '\c2Teammate %1 took the %2 flag.~wfx/misc/flag_snatch.wav', %client.name, %teamName, %flag.team, %client.nameBase);
      messageTeam(%flag.team, 'MsgCTFFlagTaken', '\c2Your flag has been taken by %1!~wfx/misc/flag_taken.wav',%client.name, 0, %flag.team, %client.nameBase);
      messageTeam(0, 'MsgCTFFlagTaken', '\c2%1 took the %2 flag.~wfx/misc/flag_snatch.wav', %client.name, %teamName, %flag.team, %client.nameBase);
      messageClient(%client, 'MsgCTFFlagTaken', '\c2You took the %2 flag.~wfx/misc/flag_snatch.wav', %client.name, %teamName, %flag.team, %client.nameBase);
   }
   logEcho(%client.nameBase@" (pl "@%player@"/cl "@%client@") took team "@%flag.team@" flag ("@%grabspeed@")"); // MP: 6/15/2011 added grabspeed.

   //call the AI function
   %game.AIplayerTouchEnemyFlag(%player, %flag);

   //if the player is out of bounds, then in 3 seconds, it should be thrown back towards the in bounds area...
   if (%client.outOfBounds)
      %game.schedule(3000, "boundaryLoseFlag", %player);
}

function SCtFGame::playerGotFlagTarget(%game, %player)
{
   %player.scopeWhenSensorVisible(true);
   %target = %player.getTarget();
   setTargetRenderMask(%target, getTargetRenderMask(%target) | 0x2);
   if(%game.stalemateObjsVisible)
      setTargetAlwaysVisMask(%target, 0x7);
}

function SCtFGame::playerLostFlagTarget(%game, %player)
{
   %player.scopeWhenSensorVisible(false);
   %target = %player.getTarget();
   setTargetRenderMask(%target, getTargetRenderMask(%target) & ~0x2);
   // clear his always vis target mask
   setTargetAlwaysVisMask(%target, (1 << getTargetSensorGroup(%target)));
}

//----------------------------------------------------------------------------------------
// z0dd - ZOD, 8/4/02: KineticPoet's flag updater code
function SCtFGame::updateFlagTransform(%game, %flag)
{
   %flag.setTransform(%flag.getTransform());
   %game.updateFlagThread[%flag] = %game.schedule(256, "updateFlagTransform", %flag);
}

function SCtFGame::playerDroppedFlag(%game, %player)
{
   %client = %player.client;
   %flag = %player.holdingFlag;
   %game.updateFlagTransform(%flag); // z0dd - ZOD, 8/4/02, Call to KineticPoet's flag updater
   %held = %game.formatTime(getSimTime() - %game.flagHeldTime[%flag], false); // z0dd - ZOD, 8/15/02. How long did player hold flag?

   %game.playerLostFlagTarget(%player);

   %player.holdingFlag = ""; //player isn't holding a flag anymore
   %flag.carrier = "";  //flag isn't held anymore
   $flagStatus[%flag.team] = "<In the Field>";

   // attach the camera to the flag
   if($Observers)
   {
	   for(%i = 0; %i < ClientGroup.getCount(); %i++)
	   {
			%cl = ClientGroup.getObject(%i);
			if(%cl.team <= 0 && %cl.observingFlag && %cl.flagObsTeam == %flag.team)
				observeFlag(%cl, $TeamFlag[%flag.team], 1, %flag.team);
	   }
   }

   %player.unMountImage($FlagSlot);
   %flag.hide(false); //Does the throwItem function handle this?

   %teamName = %game.getTeamName(%flag.team);
   messageTeamExcept(%client, 'MsgCTFFlagDropped', '\c2Teammate %1 dropped the %2 flag. (Held: %4)~wfx/misc/flag_drop.wav', %client.name, %teamName, %flag.team, %held); // z0dd - ZOD, 8/15/02. How long flag was held
   messageTeam(%flag.team, 'MsgCTFFlagDropped', '\c2Your flag has been dropped by %1! (Held: %4)~wfx/misc/flag_drop.wav', %client.name, 0, %flag.team, %held); // z0dd - ZOD, 8/15/02. How long flag was held
   messageTeam(0, 'MsgCTFFlagDropped', '\c2%1 dropped the %2 flag. (Held: %4)~wfx/misc/flag_drop.wav', %client.name, %teamName, %flag.team, %held); // z0dd - ZOD, 8/15/02. How long flag was held
   if(!%player.client.outOfBounds)
      messageClient(%client, 'MsgCTFFlagDropped', '\c2You dropped the %2 flag. (Held: %4)~wfx/misc/flag_drop.wav', %client.name, %teamName, %flag.team, %held); // z0dd - ZOD, 8/15/02. How long flag was held
                                                                                                                                                                // Yogi, 8/18/02. 3rd param changed 0 -> %client.name
   logEcho(%client.nameBase@" (pl "@%player@"/cl "@%client@") dropped team "@%flag.team@" flag"@" (Held: "@%held@")");

   //don't duplicate the schedule if there's already one in progress...
   if ($FlagReturnTimer[%flag] <= 0)
      $FlagReturnTimer[%flag] = %game.schedule(%game.FLAG_RETURN_DELAY - %game.fadeTimeMS, "flagReturnFade", %flag);

   //call the AI function
   %game.AIplayerDroppedFlag(%player, %flag);
}

function SCtFGame::flagCap(%game, %player)
{
   %client = %player.client;
   %flag = %player.holdingFlag;
   %flag.carrier = "";

   // when a player cap the flag, attach to flag again
   if($Observers)
   {
	   for(%i = 0; %i < ClientGroup.getCount(); %i++)
	   {
			%cl = ClientGroup.getObject(%i);
			if(%cl.team <= 0 && %cl.observingFlag && %cl.flagObsTeam == %flag.team)
				observeFlag(%cl, $TeamFlag[%flag.team], 1, %flag.team);
	   }
   }

   %held = %game.formatTime(getSimTime() - %game.flagHeldTime[%flag], true); // z0dd - ZOD, 8/15/02. How long did player hold flag?

   %game.playerLostFlagTarget(%player);
   //award points to player and team
   %teamName = %game.getTeamName(%flag.team);
   messageTeamExcept(%client, 'MsgCTFFlagCapped', '\c2%1 captured the %2 flag! (Held: %5)~wfx/misc/flag_capture.wav', %client.name, %teamName, %flag.team, %client.team, %held);
   messageTeam(%flag.team, 'MsgCTFFlagCapped', '\c2Your flag was captured by %1. (Held: %5)~wfx/misc/flag_lost.wav', %client.name, 0, %flag.team, %client.team, %held);
   messageTeam(0, 'MsgCTFFlagCapped', '\c2%1 captured the %2 flag! (Held: %5)~wfx/misc/flag_capture.wav', %client.name, %teamName, %flag.team, %client.team, %held);
   messageClient(%client, 'MsgCTFFlagCapped', '\c2You captured the %2 flag! (Held: %5)~wfx/misc/flag_capture.wav', %client.name, %teamName, %flag.team, %client.team, %held); // Yogi, 8/18/02.  3rd param changed 0 -> %client.name

   logEcho(%client.nameBase@" (pl "@%player@"/cl "@%client@") capped team "@%client.team@" flag"@" (Held: "@%held@")"); // MP: 6/14/2011 The team is wrong, but the stats program auto-corrects it, so just leave as is
   %player.holdingFlag = ""; //no longer holding it.
   %player.unMountImage($FlagSlot);
   %game.awardScoreFlagCap(%client, %flag);
   %game.flagReset(%flag);

   //call the AI function
   %game.AIflagCap(%player, %flag);

   //Determine score status
   %caplimit = MissionGroup.CTF_scoreLimit;
   %otherteam = ( %client.team == 1 ) ? 2 : 1;
   //Find out caps from score
   %clientteamcaps = mFloor($TeamScore[%client.team] / %game.SCORE_PER_TEAM_FLAG_CAP);
   %otherteamcaps = mFloor($TeamScore[%otherteam] / %game.SCORE_PER_TEAM_FLAG_CAP);

   //Determine Gamepoint
   if(%clientteamcaps >= (%caplimit - 1))
   {
	   if(%clientteamcaps == %otherteamcaps)
		  %scorestatus = "tied";
	   else
		  %scorestatus = "gamepoint";
   }
   else
	   %scorestatus = "normal";

   //if this cap didn't end the game, play the announcer...
   if($missionRunning)
   {
	  switch$(%scorestatus)
	  {
		  case normal:
			  //classic uses only storm/inferno
			  if(%game.getTeamName(%client.team) $= 'Inferno')
				 messageAll("", '~wvoice/announcer/ann.infscores.wav');
			  else if(%game.getTeamName(%client.team) $= 'Storm')
				 messageAll("", '~wvoice/announcer/ann.stoscores.wav');
		  case tied:
		      //Announce tied message
			  messageAll("", '~wvoice/announcer/ann.closegame_03.wav');
		  case gamepoint:
			  //Announce gamepoint
			  messageAll("", '~wvoice/announcer/ann.gamepoint_imminent.wav');
	  }
   }
}

function SCtFGame::flagReturnFade(%game, %flag)
{
   $FlagReturnTimer[%flag] = %game.schedule(%game.fadeTimeMS, "flagReturn", %flag);
   %flag.startFade(%game.fadeTimeMS, 0, true);
}

function SCtFGame::flagReturn(%game, %flag, %player)
{
   cancel($FlagReturnTimer[%flag]);
   $FlagReturnTimer[%flag] = "";

   if(%flag.team == 1)
      %otherTeam = 2;
   else
      %otherTeam = 1;
   %teamName = %game.getTeamName(%flag.team);

   // when the flag return, attach to flag again
   if($Observers)
   {
	   for(%i = 0; %i < ClientGroup.getCount(); %i++)
	   {
			%cl = ClientGroup.getObject(%i);
			if(%cl.team <= 0 && %cl.observingFlag && %cl.flagObsTeam == %flag.team)
				observeFlag(%cl, $TeamFlag[%flag.team], 1, %flag.team);
	   }
   }

   if (%player !$= "")
   {
      //a player returned it
      %client = %player.client;
      messageTeamExcept(%client, 'MsgCTFFlagReturned', '\c2Teammate %1 returned your flag to base.~wfx/misc/flag_return.wav', %client.name, 0, %flag.team);
      messageTeam(%otherTeam, 'MsgCTFFlagReturned', '\c2Enemy %1 returned the %2 flag.~wfx/misc/flag_return.wav', %client.name, %teamName, %flag.team);
      messageTeam(0, 'MsgCTFFlagReturned', '\c2%1 returned the %2 flag.~wfx/misc/flag_return.wav', %client.name, %teamName, %flag.team);
      messageClient(%client, 'MsgCTFFlagReturned', '\c2You returned your flag.~wfx/misc/flag_return.wav', %client.name, %teamName, %flag.team); // Yogi, 8/18/02. 3rd param changed 0 -> %client.name
      logEcho(%client.nameBase@" (pl "@%player@"/cl "@%client@") returned team "@%flag.team@" flag");

      // find out what type of return it is
      // stalemate return?
      // ---------------------------------------------------
      // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
      if(%game.stalemate)
      {
        //error("Stalemate return!!!");
        %game.awardScoreStalemateReturn(%player.client);
      }
      // regular return
      else
      {
        %enemyFlagDist = vectorDist($flagPos[%flag.team], $flagPos[%otherTeam]);
        %dist = vectorDist(%flag.position, %flag.originalPosition);

        %rawRatio = %dist/%enemyFlagDist;
        %ratio = %rawRatio < 1 ? %rawRatio : 1;
        %percentage = mFloor( (%ratio) * 10 ) * 10;
        %game.awardScoreFlagReturn(%player.client, %percentage);
      }
   }
   else
   {
      //returned due to timer
      messageTeam(%otherTeam, 'MsgCTFFlagReturned', '\c2The %2 flag was returned to base.~wfx/misc/flag_return.wav', 0, %teamName, %flag.team);  //because it was dropped for too long
      messageTeam(%flag.team, 'MsgCTFFlagReturned', '\c2Your flag was returned.~wfx/misc/flag_return.wav', 0, 0, %flag.team);
      messageTeam(0, 'MsgCTFFlagReturned', '\c2The %2 flag was returned to base.~wfx/misc/flag_return.wav', 0, %teamName, %flag.team);
      logEcho("team "@%flag.team@" flag returned (timeout)");
   }
   %game.flagReset(%flag);
}

function SCtFGame::showStalemateTargets(%game)
{
   cancel(%game.stalemateSchedule);

   //show the targets
   for (%i = 1; %i <= 2; %i++)
   {
      %flag = $TeamFlag[%i];

      //find the object to scope/waypoint....
      //render the target hud icon for slot 1 (a centermass flag)
      //if we just set him as always sensor vis, it'll work fine.
      if (isObject(%flag.carrier))
         setTargetAlwaysVisMask(%flag.carrier.getTarget(), 0x7);
   }
   //schedule the targets to hide
   %game.stalemateObjsVisible = true;
   %game.stalemateSchedule = %game.schedule(%game.stalemateDurationMS, hideStalemateTargets);
}

function SCtFGame::hideStalemateTargets(%game)
{
   cancel(%game.stalemateSchedule);

   //hide the targets
   for (%i = 1; %i <= 2; %i++)
   {
      %flag = $TeamFlag[%i];
      if (isObject(%flag.carrier))
      {
         %target = %flag.carrier.getTarget();
         setTargetAlwaysVisMask(%target, (1 << getTargetSensorGroup(%target)));
      }
   }
   //schedule the targets to show again
   %game.stalemateObjsVisible = false;
   %game.stalemateSchedule = %game.schedule(%game.stalemateFreqMS, showStalemateTargets);
}

function SCtFGame::beginStalemate(%game)
{
   %game.stalemate = true;
   %game.showStalemateTargets();

   // z0dd - ZOD, 5/27/03. Added anti-turtling, return flags after x minutes
   if(!$Host::TournamentMode)
   {
      messageAll( 'MsgStalemate', "\c3Anti turtle initialized. Flags will be returned to bases in " @ $Host::ClassicAntiTurtleTime @ " minutes.");
      %game.turtleSchedule = %game.schedule($Host::ClassicAntiTurtleTime * 60000, "antiTurtle");
	   error(formatTimeString("HH:nn:ss") SPC "Anti-Turtle thread beginning now - ID:" SPC %game.turtleSchedule);
   }
}

function SCtFGame::endStalemate(%game)
{
   %game.stalemate = false;
   %game.hideStalemateTargets();
   cancel(%game.stalemateSchedule);
   cancel(%game.turtleSchedule);
}

// z0dd - ZOD, 5/27/03. Anti-turtle function
function CTFGame::antiTurtle(%game)
{
   if(isEventPending(%game.turtleSchedule))
	  cancel(%game.turtleSchedule);

   if(%game.turtleSchedule > 0)
      %game.turtleSchedule = 0;

   if(isEventPending(%game.stalemateSchedule))
	  cancel(%game.stalemateSchedule);

   if(%game.stalemateSchedule > 0)
      %game.stalemateSchedule = 0;

   for (%i = 1; %i <= Game.numTeams; %i++)
      Game.flagReturn($TeamFlag[%i]);

   messageAll( "", "\c3Both flags returned to bases to break stalemate.~wfx/misc/flag_return.wav");
   error(formatTimeString("HH:nn:ss") SPC "Anti-Turtle thread ended");
}

function SCtFGame::flagReset(%game, %flag)
{
   cancel(%game.updateFlagThread[%flag]); // z0dd - ZOD, 8/4/02. Cancel this flag's thread to KineticPoet's flag updater

   //any time a flag is reset, kill the stalemate schedule
   %game.endStalemate(%game);

   //make sure if there's a player carrying it (probably one out of bounds...), it is stripped first
   if (isObject(%flag.carrier))
   {
      //hide the target hud icon for slot 2 (a centermass flag - visible only as part of a teams sensor network)
      %game.playerLostFlagTarget(%flag.carrier);
      %flag.carrier.holdingFlag = ""; //no longer holding it.
      %flag.carrier.unMountImage($FlagSlot);
   }
   //fades, restore default position, home, velocity, general status, etc.
   %flag.setVelocity("0 0 0");
   %flag.setTransform(%flag.originalPosition);
   %flag.isHome = true;
   %flag.carrier = "";
   %flag.grabber = "";
   $flagStatus[%flag.team] = "<At Base>";
   %flag.hide(false);
   if(%flag.stand)
      %flag.stand.getDataBlock().onFlagReturn(%flag.stand);//animate, if exterior stand

   //fade the flag in...
   %flag.startFade(%game.fadeTimeMS, 0, false);

   // dont render base target
   setTargetRenderMask(%flag.waypoint.getTarget(), 0);

   //call the AI function
   %game.AIflagReset(%flag);

   // --------------------------------------------------------
   // z0dd - ZOD, 5/26/02. Don't let flag hover over defenders
   %flag.static = true;

   // --------------------------------------------------------------------------
   // z0dd - ZOD, 9/28/02. Hack for flag collision bug.
   if(%flag.searchSchedule !$="")
   {
      cancel(%flag.searchSchedule);
   }
   // --------------------------------------------------------------------------
}

function SCtFGame::notifyMineDeployed(%game, %mine)
{
   //see if the mine is within 5 meters of the flag stand...
   %mineTeam = %mine.sourceObject.team;
   %homeFlag = $TeamFlag[%mineTeam];
   if (isObject(%homeFlag))
   {
      %dist = VectorDist(%homeFlag.originalPosition, %mine.position);
      if (%dist <= %game.notifyMineDist)
      {
         messageTeam(%mineTeam, 'MsgCTFFlagMined', "The flag has been mined.~wvoice/announcer/flag_minedFem.wav" );
      }
   }
}

function SCtFGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc)
{
   if(%clVictim.headshot && %damageType == $DamageType::Laser && %clVictim.team != %clAttacker.team)
   {
      %clAttacker.scoreHeadshot++;
      if (%game.SCORE_PER_HEADSHOT != 0)
      {
         messageClient(%clAttacker, 'msgHeadshot', '\c0You received a %1 point bonus for a successful headshot.', %game.SCORE_PER_HEADSHOT);
         messageTeamExcept(%clAttacker, 'msgHeadshot', '\c5%1 hit a sniper rifle headshot.', %clAttacker.name);
      }
      %game.recalcScore(%clAttacker);
   }
   if(%clVictim.rearshot && %damageType == $DamageType::ShockLance && %clVictim.team != %clAttacker.team)
   {
      %clAttacker.scoreRearshot++;
      if (%game.SCORE_PER_REARSHOT != 0)
      {
         messageClient(%clAttacker, 'msgRearshot', '\c0You received a %1 point bonus for a successful rearshot.', %game.SCORE_PER_REARSHOT);
         messageTeamExcept(%clAttacker, 'msgRearshot', '\c5%1 hit a shocklance rearshot.', %clAttacker.name);
      }
      %game.recalcScore(%clAttacker);
   }
   //the DefaultGame will set some vars
   DefaultGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc);

   //if victim is carrying a flag and is not on the attackers team, mark the attacker as a threat for x seconds(for scoring purposes)
   if ((%clVictim.holdingFlag !$= "") && (%clVictim.team != %clAttacker.team))
   {
      %clAttacker.dmgdFlagCarrier = true;
      cancel(%clAttacker.threatTimer);  //restart timer
      %clAttacker.threatTimer = schedule(%game.TIME_CONSIDERED_FLAGCARRIER_THREAT, %clAttacker.dmgdFlagCarrier = false);
   }
}

////////////////////////////////////////////////////////////////////////////////////////

function SCtFGame::recalcScore(%game, %cl)
{
   %killValue = %cl.kills * %game.SCORE_PER_KILL;
   %deathValue = %cl.deaths * %game.SCORE_PER_DEATH;

   if (%killValue - %deathValue == 0)
      %killPoints = 0;
   else
      %killPoints = (%killValue * %killValue) / (%killValue - %deathValue);

   %cl.offenseScore = %killPoints +
                   %cl.suicides            * %game.SCORE_PER_SUICIDE +
                   %cl.escortAssists       * %game.SCORE_PER_ESCORT_ASSIST +
                   %cl.teamKills           * %game.SCORE_PER_TEAMKILL +
                   %cl.tkDestroys          * %game.SCORE_PER_TK_DESTROY +
                   %cl.scoreHeadshot       * %game.SCORE_PER_HEADSHOT +
                   %cl.scoreRearshot       * %game.SCORE_PER_REARSHOT +
                   %cl.scoreMidAir         * %game.SCORE_PER_MIDAIR +
                   %cl.flagCaps            * %game.SCORE_PER_PLYR_FLAG_CAP +
                   %cl.flagGrabs           * %game.SCORE_PER_PLYR_FLAG_TOUCH +
                   %cl.genDestroys         * %game.SCORE_PER_DESTROY_GEN +
                   %cl.sensorDestroys      * %game.SCORE_PER_DESTROY_SENSOR +
                   %cl.turretDestroys      * %game.SCORE_PER_DESTROY_TURRET +
                   %cl.iStationDestroys    * %game.SCORE_PER_DESTROY_ISTATION +
                   %cl.vstationDestroys    * %game.SCORE_PER_DESTROY_VSTATION +
                   %cl.mpbtstationDestroys * %game.SCORE_PER_DESTROY_MPBTSTATION +
                   %cl.solarDestroys       * %game.SCORE_PER_DESTROY_SOLAR +
                   %cl.sentryDestroys      * %game.SCORE_PER_DESTROY_SENTRY +
                   %cl.depSensorDestroys   * %game.SCORE_PER_DESTROY_DEP_SENSOR +
                   %cl.depTurretDestroys   * %game.SCORE_PER_DESTROY_DEP_TUR +
                   %cl.depStationDestroys  * %game.SCORE_PER_DESTROY_DEP_INV +
                   %cl.vehicleScore + %cl.vehicleBonus;

   %cl.defenseScore = %cl.genDefends      * %game.SCORE_PER_GEN_DEFEND +
                   %cl.flagDefends        * %game.SCORE_PER_FLAG_DEFEND +
                   %cl.carrierKills       * %game.SCORE_PER_CARRIER_KILL +
                   %cl.escortAssists      * %game.SCORE_PER_ESCORT_ASSIST +
                   %cl.turretKills        * %game.SCORE_PER_TURRET_KILL_AUTO +
                   %cl.mannedturretKills  * %game.SCORE_PER_TURRET_KILL +
                   %cl.genRepairs         * %game.SCORE_PER_REPAIR_GEN +
                   %cl.SensorRepairs      * %game.SCORE_PER_REPAIR_SENSOR +
                   %cl.TurretRepairs      * %game.SCORE_PER_REPAIR_TURRET +
                   %cl.StationRepairs     * %game.SCORE_PER_REPAIR_ISTATION +
                   %cl.VStationRepairs    * %game.SCORE_PER_REPAIR_VSTATION +
                   %cl.mpbtstationRepairs * %game.SCORE_PER_REPAIR_MPBTSTATION +
                   %cl.solarRepairs       * %game.SCORE_PER_REPAIR_SOLAR +
                   %cl.sentryRepairs      * %game.SCORE_PER_REPAIR_SENTRY +
                   %cl.depSensorRepairs   * %game.SCORE_PER_REPAIR_DEP_SEN +
                   %cl.depInvRepairs      * %game.SCORE_PER_REPAIR_DEP_INV +
                   %cl.depTurretRepairs   * %game.SCORE_PER_REPAIR_DEP_TUR +
                   %cl.returnPts;

   %cl.score = mFloor(%cl.offenseScore + %cl.defenseScore);
   %game.recalcTeamRanks(%cl);
}

function SCtFGame::updateKillScores(%game, %clVictim, %clKiller, %damageType, %implement)
{
   // is this a vehicle kill rather than a player kill
   // console error message suppression
   if( isObject( %implement ) )
   {
      if( %implement.getDataBlock().getName() $= "AssaultPlasmaTurret" ||  %implement.getDataBlock().getName() $= "BomberTurret" ) // gunner
           %clKiller = %implement.vehicleMounted.getMountNodeObject(1).client;
      else if(%implement.getDataBlock().catagory $= "Vehicles") // pilot
           %clKiller = %implement.getMountNodeObject(0).client;
   }

   if(%game.testTurretKill(%implement))   //check for turretkill before awarded a non client points for a kill
      %game.awardScoreTurretKill(%clVictim, %implement);
   else if (%game.testKill(%clVictim, %clKiller)) //verify victim was an enemy
   {
      %value = %game.awardScoreKill(%clKiller);
      %game.shareScore(%clKiller, %value);
      %game.awardScoreDeath(%clVictim);

      if (%game.testGenDefend(%clVictim, %clKiller))
         %game.awardScoreGenDefend(%clKiller);

      if(%game.testCarrierKill(%clVictim, %clKiller))
         %game.awardScoreCarrierKill(%clKiller);
      else
      {
         if (%game.testFlagDefend(%clVictim, %clKiller))
            %game.awardScoreFlagDefend(%clKiller);
      }
      if (%game.testEscortAssist(%clVictim, %clKiller))
         %game.awardScoreEscortAssist(%clKiller);
   }
   else
   {
      if (%game.testSuicide(%clVictim, %clKiller, %damageType))  //otherwise test for suicide
      {
         %game.awardScoreSuicide(%clVictim);
      }
      else
      {
         if (%game.testTeamKill(%clVictim, %clKiller, %damageType)) //otherwise test for a teamkill
            %game.awardScoreTeamKill(%clVictim, %clKiller);
      }
   }
}

function SCtFGame::testFlagDefend(%game, %victimID, %killerID)
{
   InitContainerRadiusSearch(%victimID.plyrPointOfDeath, %game.RADIUS_FLAG_DEFENSE, $TypeMasks::ItemObjectType);
   %objID = containerSearchNext();
   while(%objID != 0)
   {
     %objType = %objID.getDataBlock().getName();
     if ((%objType $= "Flag") && (%objID.team == %killerID.team))
          return true;  //found the(a) killer's flag near the victim's point of death
     else
        %objID = containerSearchNext();
   }
   return false; //didn't find a qualifying flag within required radius of victims point of death
}

function SCtFGame::testGenDefend(%game, %victimID, %killerID)
{
   InitContainerRadiusSearch(%victimID.plyrPointOfDeath, %game.RADIUS_GEN_DEFENSE, $TypeMasks::StaticShapeObjectType);
   %objID = containerSearchNext();
   while(%objID != 0)
   {
      %objType = %objID.getDataBlock().ClassName;
     if ((%objType $= "generator") && (%objID.team == %killerID.team))
        return true;  //found a killer's generator within required radius of victim's death
     else
        %objID = containerSearchNext();
   }
   return false;  //didn't find a qualifying gen within required radius of victim's point of death
}

function SCtFGame::testCarrierKill(%game, %victimID, %killerID)
{
   %flag = %victimID.plyrDiedHoldingFlag;
   return ((%flag !$= "") && (%flag.team == %killerID.team));
}

function SCtFGame::testEscortAssist(%game, %victimID, %killerID)
{
   return (%victimID.dmgdFlagCarrier);
}

function SCtFGame::testValidRepair(%game, %obj)
{
    if(!%obj.wasDisabled)
    {
        //error(%obj SPC "was never disabled");
        return false;
    }
    else if(%obj.lastDamagedByTeam == %obj.team)
    {
        //error(%obj SPC "was last damaged by a friendly");
        return false;
    }
    else if(%obj.team != %obj.repairedBy.team)
    {
        //error(%obj SPC "was repaired by an enemy");
        return false;
    }
    else
    {
        if(%obj.soiledByEnemyRepair)
            %obj.soiledByEnemyRepair = false;
        return true;
    }
}

function SCtFGame::awardScoreFlagCap(%game, %cl, %flag)
{
    %cl.flagCaps++;
    $TeamScore[%cl.team] += %game.SCORE_PER_TEAM_FLAG_CAP;
    messageAll('MsgTeamScoreIs', "", %cl.team, $TeamScore[%cl.team]);

    //%flag.grabber.flagGrabs++; //moved to awardScoreFlagTouch

    if (%game.SCORE_PER_TEAM_FLAG_CAP > 0)
    {
       %plural = (%game.SCORE_PER_PLYR_FLAG_CAP != 1 ? 's' : "");
       %plural2 = (%game.SCORE_PER_PLYR_FLAG_TOUCH != 1 ? 's' : "");

       if(%cl == %flag.grabber)
       {
          messageClient(%cl, 'msgCTFFriendCap', '\c0You receive %1 point%2 for stealing and capturing the enemy flag!', %game.SCORE_PER_PLYR_FLAG_CAP+%game.SCORE_PER_PLYR_FLAG_TOUCH, %plural);
          messageTeam(%flag.team, 'msgCTFEnemyCap', '\c0Enemy %1 received %2 point%3 for capturing your flag!', %cl.name, %game.SCORE_PER_PLYR_FLAG_CAP+%game.SCORE_PER_PLYR_FLAG_TOUCH, %plural);
          //messageTeamExcept(%cl, 'msgCTFFriendCap', '\c0Teammate %1 receives %2 point%3 for capturing the enemy flag!', %cl.name, %game.SCORE_PER_PLYR_FLAG_CAP+%game.SCORE_PER_PLYR_FLAG_TOUCH, %plural);  // z0dd - ZOD, 8/15/02. Message is pointless
       }
       else
       {
          if(isObject(%flag.grabber))  // is the grabber still here?
          {
             messageClient(%cl, 'msgCTFFriendCap', '\c0You receive %1 point%2 for capturing the enemy flag!  %3 gets %4 point%5 for the steal assist.', %game.SCORE_PER_PLYR_FLAG_CAP, %plural, %flag.grabber.name, %game.SCORE_PER_PLYR_FLAG_TOUCH, %plural2);
             messageClient(%flag.grabber, 'msgCTFFriendCap', '\c0You receive %1 point%2 for stealing a flag that was subsequently capped by %3.', %game.SCORE_PER_PLYR_FLAG_TOUCH, %plural2, %cl.name);
          }
          else
             messageClient(%cl, 'msgCTFFriendCap', '\c0You receive %1 point%2 for capturing the enemy flag!', %game.SCORE_PER_PLYR_FLAG_CAP, %plural);
       }
    }
    %game.recalcScore(%cl);
    if(isObject(%flag.grabber))
        %game.recalcScore(%flag.grabber);

    %game.checkScoreLimit(%cl.team);
}


function SCtFGame::awardScoreFlagTouch(%game, %cl, %flag)
{

    %flag.grabber = %cl;
    %flag.grabber.flagGrabs++; //moved from awardScoreFlagCap to correctly count flaggrabs
    %team = %cl.team;
	if( $DontScoreTimer[%team] )
		return;

   $dontScoreTimer[%team] = true;
   //tinman - needed to remove all game calls to "eval" for the PURE server...
   %game.schedule(%game.TOUCH_DELAY_MS, resetDontScoreTimer, %team);
   //schedule(%game.TOUCH_DELAY_MS, 0, eval, "$dontScoreTimer["@%team@"] = false;");
   $TeamScore[%team] += %game.SCORE_PER_TEAM_FLAG_TOUCH;
   messageAll('MsgTeamScoreIs', "", %team, $TeamScore[%team]);

   if (%game.SCORE_PER_TEAM_FLAG_TOUCH > 0)
   {
      %plural = (%game.SCORE_PER_TEAM_FLAG_TOUCH != 1 ? 's' : "");
      messageTeam(%team, 'msgCTFFriendFlagTouch', '\c0Your team receives %1 point%2 for grabbing the enemy flag!', %game.SCORE_PER_TEAM_FLAG_TOUCH, %plural);
      messageTeam(%flag.team, 'msgCTFEnemyFlagTouch', '\c0Enemy %1 receives %2 point%3 for grabbing your flag!', %cl.name, %game.SCORE_PER_TEAM_FLAG_TOUCH, %plural);
   }
   %game.recalcScore(%cl);
   %game.checkScoreLimit(%team);
}

function SCtFGame::resetDontScoreTimer(%game, %team)
{
   $dontScoreTimer[%team] = false;
}

function SCtFGame::checkScoreLimit(%game, %team)
{
   %scoreLimit = MissionGroup.CTF_scoreLimit * %game.SCORE_PER_TEAM_FLAG_CAP;
   // default of 5 if scoreLimit not defined
   if(%scoreLimit $= "")
      %scoreLimit = 5 * %game.SCORE_PER_TEAM_FLAG_CAP;
   if($TeamScore[%team] >= %scoreLimit)
      %game.scoreLimitReached();
}

function SCtFGame::awardScoreFlagReturn(%game, %cl, %perc)
{
   if (%game.SCORE_PER_FLAG_RETURN != 0)
   {
      %pts = mfloor( %game.SCORE_PER_FLAG_RETURN * (%perc/100) );
      if(%perc  == 100)
         messageClient(%cl, 'scoreFlaRetMsg', 'Flag return - exceeded capping distance - %1 point bonus.', %pts, %perc);
      else if(%perc  == 0)
         messageClient(%cl, 'scoreFlaRetMsg', 'You gently place the flag back on the stand.', %pts, %perc);
      else
         messageClient(%cl, 'scoreFlaRetMsg', '\c0Flag return from %2%% of capping distance - %1 point bonus.', %pts, %perc);
      %cl.returnPts += %pts;
   }
   %game.recalcScore(%cl);
   return %game.SCORE_PER_FLAG_RETURN;
}

function SCtFGame::awardScoreStalemateReturn(%game, %cl)
{
   if (%game.SCORE_PER_STALEMATE_RETURN != 0)
   {
      messageClient(%cl, 'scoreStaleRetMsg', '\c0You received a %1 point bonus for a stalemate-breaking, flag return.', %game.SCORE_PER_STALEMATE_RETURN);
      %cl.returnPts += %game.SCORE_PER_STALEMATE_RETURN;
   }
   %game.recalcScore(%cl);
    return %game.SCORE_PER_STALEMATE_RETURN;
}

function SCtFGame::awardScoreGenDefend(%game, %killerID)
{
   %killerID.genDefends++;
   if (%game.SCORE_PER_GEN_DEFEND != 0)
   {
      messageClient(%killerID, 'msgGenDef', '\c0You received a %1 point bonus for defending a generator.', %game.SCORE_PER_GEN_DEFEND);
      messageTeamExcept(%killerID, 'msgGenDef', '\c2%1 defended our generator from an attack.', %killerID.name); // z0dd - ZOD, 8/15/02. Tell team
   }
   %game.recalcScore(%cl);
    return %game.SCORE_PER_GEN_DEFEND;
}

function SCtFGame::awardScoreCarrierKill(%game, %killerID)
{
   %killerID.carrierKills++;
   if (%game.SCORE_PER_CARRIER_KILL != 0)
   {
      messageClient(%killerID, 'msgCarKill', '\c0You received a %1 point bonus for stopping the enemy flag carrier!', %game.SCORE_PER_CARRIER_KILL);
      messageTeamExcept(%killerID, 'msgCarKill', '\c2%1 stopped the enemy flag carrier.', %killerID.name); // z0dd - ZOD, 8/15/02. Tell team
   }
   %game.recalcScore(%killerID);
    return %game.SCORE_PER_CARRIER_KILL;
}

function SCtFGame::awardScoreFlagDefend(%game, %killerID)
{
   %killerID.flagDefends++;
   if (%game.SCORE_PER_FLAG_DEFEND != 0)
   {
      messageClient(%killerID, 'msgFlagDef', '\c0You received a %1 point bonus for defending your flag!', %game.SCORE_PER_FLAG_DEFEND);
      messageTeamExcept(%killerID, 'msgFlagDef', '\c2%1 defended our flag.', %killerID.name); // z0dd - ZOD, 8/15/02. Tell team
   }
   %game.recalcScore(%killerID);
    return %game.SCORE_PER_FLAG_DEFEND;
}

function SCtFGame::awardScoreEscortAssist(%game, %killerID)
{
   %killerID.escortAssists++;
   if (%game.SCORE_PER_ESCORT_ASSIST != 0)
   {
      messageClient(%killerID, 'msgEscAsst', '\c0You received a %1 point bonus for protecting the flag carrier!', %game.SCORE_PER_ESCORT_ASSIST);
      messageTeamExcept(%killerID, 'msgEscAsst', '\c2%1 protected our flag carrier.', %killerID.name); // z0dd - ZOD, 8/15/02. Tell team
   }
   %game.recalcScore(%killerID);
    return %game.SCORE_PER_ESCORT_ASSIST;
}

function SCtFGame::resetScore(%game, %client)
{
   %client.offenseScore = 0;
   %client.kills = 0;
   %client.scoreMidAir = 0;
   %client.deaths = 0;
   %client.suicides = 0;
   %client.escortAssists = 0;
   %client.teamKills = 0;
   %client.tkDestroys = 0; // z0dd - ZOD, 10/03/02. Penalty for tking equiptment.
   %client.flagCaps = 0;
   %client.flagGrabs = 0;
   %client.genDestroys = 0;
   %client.sensorDestroys = 0;
   %client.turretDestroys = 0;
   %client.iStationDestroys = 0;
   %client.vstationDestroys = 0;
   %client.mpbtstationDestroys = 0;
   %client.solarDestroys = 0;
   %client.sentryDestroys = 0;
   %client.depSensorDestroys = 0;
   %client.depTurretDestroys = 0;
   %client.depStationDestroys = 0;
   %client.vehicleScore = 0;
   %client.vehicleBonus = 0;

   %client.flagDefends = 0;
   %client.defenseScore = 0;
   %client.genDefends = 0;
   %client.carrierKills = 0;
   %client.escortAssists = 0;
   %client.mannedTurretKills = 0;
   %client.flagReturns = 0;
   %client.genRepairs = 0;
   %client.SensorRepairs = 0;
   %client.TurretRepairs = 0;
   %client.StationRepairs = 0;
   %client.VStationRepairs = 0;
   %client.mpbtstationRepairs = 0;
   %client.solarRepairs = 0;
   %client.sentryRepairs = 0;
   %client.depSensorRepairs = 0;
   %client.depInvRepairs = 0;
   %client.depTurretRepairs = 0;
   %client.returnPts = 0;
   %client.score = 0;
}

/////////////////////////////////////////////////////////////////////////////////////////
// Asset Destruction scoring ////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

function SCtFGame::awardScoreTkDestroy(%game, %cl, %obj)
{
   %cl.tkDestroys++;
   messageTeamExcept(%cl, 'msgTkDes', '\c5Teammate %1 destroyed your team\'s %3 objective!', %cl.name, %game.cleanWord(%obj.getDataBlock().targetTypeTag));
   messageClient(%cl, 'msgTkDes', '\c0You have been penalized %1 points for destroying your teams equiptment.', %game.SCORE_PER_TK_DESTROY);
   %game.recalcScore(%cl);
   %game.shareScore(%cl, %game.SCORE_PER_TK_DESTROY);
}

function SCtFGame::awardScoreStaticShapeDestroy(%game, %cl, %obj)
{
   %dataName = %obj.getDataBlock().getName();
   switch$ ( %dataName )
   {
      case "GeneratorLarge":
         %cl.genDestroys++;
         %value = %game.SCORE_PER_DESTROY_GEN;
         %msgType = 'msgGenDes';
         %tMsg = '\c5%1 destroyed an enemy %2 Generator!';
         %clMsg = '\c0You received a %1 point bonus for destroying an enemy generator.';

      case "SolarPanel":
         %cl.solarDestroys++;
         %value = %game.SCORE_PER_DESTROY_SOLAR;
         %msgType = 'msgSolarDes';
         %tMsg = '\c5%1 destroyed an enemy %2 Solar Panel!';
         %clMsg = '\c0You received a %1 point bonus for destroying an enemy solar panel.';

      case "SensorLargePulse" or "SensorMediumPulse":
         %cl.sensorDestroys++;
         %value = %game.SCORE_PER_DESTROY_SENSOR;
         %msgType = 'msgSensorDes';
         %tMsg = '\c5%1 destroyed an enemy %2 Sensor!';
         %clMsg = '\c0You received a %1 point bonus for destroying an enemy pulse sensor.';

      case "TurretBaseLarge":
         %cl.turretDestroys++;
         %value = %game.SCORE_PER_DESTROY_TURRET;
         %msgType = 'msgTurretDes';
         %tMsg = '\c5%1 destroyed an enemy %2 Turret!';
         %clMsg = '\c0You received a %1 point bonus for destroying an enemy base turret.';

      case "StationInventory":
         %cl.IStationDestroys++;
         %value = %game.SCORE_PER_DESTROY_ISTATION;
         %msgType = 'msgInvDes';
         %tMsg = '\c5%1 destroyed an enemy %2 Inventory Station!';
         %clMsg = '\c0You received a %1 point bonus for destroying an enemy inventory station.';

      case "StationAmmo":
         %cl.aStationDestroys++;
         %value = %game.SCORE_PER_DESTROY_ASTATION;
         %msgType = 'msgAmmoDes';
         %tMsg = '\c5%1 destroyed an enemy %2 Ammo Station!';
         %clMsg = '\c0You received a %1 point bonus for destroying an enemy ammo station.';

      case "StationVehicle":
         %cl.VStationDestroys++;
         %value = %game.SCORE_PER_DESTROY_VSTATION;
         %msgType = 'msgVSDes';
         %tMsg = '\c5%1 destroyed an enemy Vehicle Station!';
         %clMsg = '\c0You received a %1 point bonus for destroying an enemy vehicle station.';

      case "SentryTurret":
         %cl.sentryDestroys++;
         %value = %game.SCORE_PER_DESTROY_SENTRY;
         %msgType = 'msgSentryDes';
         %tMsg = '\c5%1 destroyed an enemy %2 Sentry Turret!';
         %clMsg = '\c0You received a %1 point bonus for destroying an enemy sentry turret.';

      case "DeployedMotionSensor" or "DeployedPulseSensor":
         %cl.depSensorDestroys++;
         %value = %game.SCORE_PER_DESTROY_DEP_SENSOR;
         %msgType = 'msgDepSensorDes';
         %tMsg = '\c5%1 destroyed an enemy Deployed Sensor!';
         %clMsg = '\c0You received a %1 point bonus for destroying an enemy deployed sensor.';

      case "TurretDeployedWallIndoor" or "TurretDeployedFloorIndoor" or "TurretDeployedCeilingIndoor":
         %cl.depTurretDestroys++;
         %value = %game.SCORE_PER_DESTROY_DEP_TUR;
         %msgType = 'msgDepTurDes';
         %tMsg = '\c5%1 destroyed an enemy Deployed Spider Clamp Turret!';
         %clMsg = '\c0You received a %1 point bonus for destroying an enemy deployed spider clamp turret.';

      case "TurretDeployedOutdoor":
         %cl.depTurretDestroys++;
         %value = %game.SCORE_PER_DESTROY_DEP_TUR;
         %msgType = 'msgDepTurDes';
         %tMsg = '\c5%1 destroyed an enemy Deployed Landspike Turret!';
         %clMsg = '\c0You received a %1 point bonus for destroying an enemy deployed landspike turret.';

      case "DeployedStationInventory":
         %cl.depStationDestroys++;
         %value = %game.SCORE_PER_DESTROY_DEP_INV;
         %msgType = 'msgDepInvDes';
         %tMsg = '\c5%1 destroyed an enemy Deployed Station!';
         %clMsg = '\c0You received a %1 point bonus for destroying an enemy deployed station.';

      case "MPBTeleporter":
         %cl.TStationDestroys++;
         %value = %game.SCORE_PER_DESTROY_TSTATION;
         %msgType = 'msgMPBTeleDes';
         %tMsg = '\c5%1 destroyed an enemy MPB Teleport Station!';
         %clMsg = '\c0You received a %1 point bonus for destroying an enemy MPB teleport station.';

      default:
         return;
   }
   if(isObject(%cl))
   {
	   teamDestroyMessage(%cl, 'MsgDestroyed', %tMsg, %cl.name, %obj.nameTag);
	   messageClient(%cl, %msgType, %clMsg, %value, %dataName);
	   %game.recalcScore(%cl);
	   %game.shareScore(%scorer, %value);
   }
   else //when the asset attacker is unknown
	  teamDestroyMessage(%cl, 'MsgDestroyed', %tMsg, "A teammate", %obj.nameTag);
}

/////////////////////////////////////////////////////////////////////////////////////////
// Repair Scoring Functions /////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

function SCtFGame::testValidRepair(%game, %obj)
{
   if(!%obj.wasDisabled)
      return false;
   else if(%obj.lastDamagedByTeam == %obj.team)
      return false;
   else if(%obj.team != %obj.repairedBy.team)
      return false;
   else
   {
      if(%obj.soiledByEnemyRepair)
         %obj.soiledByEnemyRepair = false;

      return true;
   }
}

function SCtFGame::objectRepaired(%game, %obj, %objName)
{
   %game.staticShapeOnRepaired(%obj, %objName);
   %obj.wasDisabled = false;
}

function SCtFGame::staticShapeOnRepaired(%game, %obj, %objName)
{
   if (%game.testValidRepair(%obj))
   {
      %repairman = %obj.repairedBy;
      %dataName = %obj.getDataBlock().getName();
      switch$ (%dataName)
      {
         case "GeneratorLarge":
            %repairman.genRepairs++;
            %score = %game.SCORE_PER_REPAIR_GEN;
            %tMsgType = 'msgGenRepaired';
            %msgType = 'msgGenRep';
            %tMsg = '\c0%1 repaired the %2 Generator!';
            %clMsg = '\c0You received a %1 point bonus for repairing a generator.';

         case "SolarPanel":
            %repairman.solarRepairs++;
            %score = %game.SCORE_PER_REPAIR_SOLAR;
            %tMsgType = 'msgsolarRepaired';
            %msgType = 'msgsolarRep';
            %tMsg = '\c0%1 repaired the %2 Solar Panel!';
            %clMsg = '\c0You received a %1 point bonus for repairing a solar panel.';

         case "SensorLargePulse" or "SensorMediumPulse":
            %repairman.sensorRepairs++;
            %score = %game.SCORE_PER_REPAIR_SENSOR;
            %tMsgType = 'msgSensorRepaired';
            %msgType = 'msgSensorRep';
            %tMsg = '\c0%1 repaired the %2 Pulse Sensor!';
            %clMsg = '\c0You received a %1 point bonus for repairing a pulse sensor.';

         case "StationInventory" or "StationAmmo":
            %repairman.stationRepairs++;
            %score = %game.SCORE_PER_REPAIR_ISTATION;
            %tMsgType = 'msgStationRepaired';
            %msgType = 'msgIStationRep';
            %tMsg = '\c0%1 repaired the %2 Station!';
            %clMsg = '\c0You received a %1 point bonus for repairing a station.';

         case "StationVehicle":
            %repairman.VStationRepairs++;
            %score = %game.SCORE_PER_REPAIR_VSTATION;
            %tMsgType = 'msgvstationRepaired';
            %msgType = 'msgVStationRep';
            %tMsg = '\c0%1 repaired the Vehicle Station!';
            %clMsg = '\c0You received a %1 point bonus for repairing a vehicle station.';

         case "TurretBaseLarge":
            %repairman.TurretRepairs++;
            %score = %game.SCORE_PER_REPAIR_TURRET;
            %tMsgType = 'msgTurretRepaired';
            %msgType = 'msgTurretRep';
            %tMsg = '\c0%1 repaired the %2 Turret!';
            %clMsg = '\c0You received a %1 point bonus for repairing a base turret.';

         case "SentryTurret":
            %repairman.sentryRepairs++;
            %score = %game.SCORE_PER_REPAIR_SENTRY;
            %tMsgType = 'msgsentryTurretRepaired';
            %msgType = 'msgSentryRep';
            %tMsg = '\c0%1 repaired the %2 Sentry Turret!';
            %clMsg = '\c0You received a %1 point bonus for repairing a sentry turret.';

         case "DeployedMotionSensor" or "DeployedPulseSensor":
            %repairman.depSensorRepairs++;
            %tMsgType = 'msgDepSensorRepaired';
            %msgType = 'msgDepSensorRep';
            %score = %game.SCORE_PER_REPAIR_DEP_SENSOR;
            %tMsg = '\c0%1 repaired a Deployed Sensor!';
            %clMsg = '\c0You received a %1 point bonus for repairing a deployed sensor.';

         case "TurretDeployedWallIndoor" or "TurretDeployedFloorIndoor" or "TurretDeployedCeilingIndoor":
            %repairman.depTurretRepairs++;
            %score = %game.SCORE_PER_REPAIR_DEP_TUR;
            %tMsgType = 'msgDepTurretRepaired';
            %msgType = 'msgDepTurretRep';
            %tMsg = '\c0%1 repaired a Spider Clamp Turret!';
            %clMsg = '\c0You received a %1 point bonus for repairing a deployable spider clamp turret.';

         case "TurretDeployedOutdoor":
            %repairman.depTurretRepairs++;
            %score = %game.SCORE_PER_REPAIR_DEP_TUR;
            %tMsgType = 'msgDepTurretRepaired';
            %msgType = 'msgDepTurretRep';
            %tMsg = '\c0%1 repaired a Landspike Turret!';
            %clMsg = '\c0You received a %1 point bonus for repairing a deployable landspike turret.';

         case "DeployedStationInventory":
            %repairman.depInvRepairs++;
            %score = %game.SCORE_PER_REPAIR_DEP_INV;
            %tMsgType = 'msgDepInvRepaired';
            %msgType = 'msgDepInvRep';
            %tMsg = '\c0%1 repaired a Deployable Station!';
            %clMsg = '\c0You received a %1 point bonus for repairing a deployed station.';

         case "MPBTeleporter":
            %repairman.TStationRepairs++;
            %score = %game.SCORE_PER_REPAIR_TSTATION;
            %tMsgType = 'msgMPBTeleRepaired';
            %msgType = 'msgMPBTeleRep';
            %tMsg = '\c0%1 repaired the MPB Teleporter Station!';
            %clMsg = '\c0You received a %1 point bonus for repairing a mpb teleporter station.';

         default:
            return;
      }
      teamRepairMessage(%repairman, %tMsgType, %tMsg, %repairman.name, %obj.nameTag);
      messageClient(%repairman, %msgType, %clMsg, %score, %dataName);
      %game.recalcScore(%repairman);
   }
}

function SCtFGame::enterMissionArea(%game, %playerData, %player)
{
   if(%player.getState() $= "Dead")
      return;

   %player.client.outOfBounds = false;
   messageClient(%player.client, 'EnterMissionArea', '\c1You are back in the mission area.');
   logEcho(%player.client.nameBase@" (pl "@%player@"/cl "@%player.client@") entered mission area");

   //the instant a player leaves the mission boundary, the flag is dropped, and the return is scheduled...
   if (%player.holdingFlag > 0)
   {
      cancel($FlagReturnTimer[%player.holdingFlag]);
      $FlagReturnTimer[%player.holdingFlag] = "";
   }
}

function SCtFGame::leaveMissionArea(%game, %playerData, %player)
{
   if(%player.getState() $= "Dead")
      return;

   // maybe we'll do this just in case
   %player.client.outOfBounds = true;
   // if the player is holding a flag, strip it and throw it back into the mission area
   // otherwise, just print a message
   if(%player.holdingFlag > 0)
      %game.boundaryLoseFlag(%player);
   else
      messageClient(%player.client, 'MsgLeaveMissionArea', '\c1You have left the mission area.~wfx/misc/warning_beep.wav');

   logEcho(%player.client.nameBase@" (pl "@%player@"/cl "@%player.client@") left mission area");
}

function SCtFGame::boundaryLoseFlag(%game, %player)
{
   // this is called when a player goes out of the mission area while holding
   // the enemy flag. - make sure the player is still out of bounds
   if (!%player.client.outOfBounds || !isObject(%player.holdingFlag))
      return;

   // ------------------------------------------------------------------------------
   // z0dd - ZOD - SquirrelOfDeath, 9/27/02. Delay on grabbing flag after tossing it
   %player.flagTossWait = true;
   %player.schedule(1000, resetFlagTossWait);
   // ------------------------------------------------------------------------------

   %client = %player.client;
   %flag = %player.holdingFlag;
   %flag.setVelocity("0 0 0");
   %flag.setTransform(%player.getWorldBoxCenter());
   %flag.setCollisionTimeout(%player);

   %held = %game.formatTime(getSimTime() - %game.flagHeldTime[%flag], false); // z0dd - ZOD, 8/15/02. How long did player hold flag?

   %game.playerDroppedFlag(%player);

   // now for the tricky part -- throwing the flag back into the mission area
   // let's try throwing it back towards its "home"
   %home = %flag.originalPosition;
   %vecx =  firstWord(%home) - firstWord(%player.getWorldBoxCenter());
   %vecy = getWord(%home, 1) - getWord(%player.getWorldBoxCenter(), 1);
   %vecz = getWord(%home, 2) - getWord(%player.getWorldBoxCenter(), 2);
   %vec = %vecx SPC %vecy SPC %vecz;

   // normalize the vector, scale it, and add an extra "upwards" component
   %vecNorm = VectorNormalize(%vec);
   %vec = VectorScale(%vecNorm, 1500);
   %vec = vectorAdd(%vec, "0 0 500");

   // z0dd - ZOD, 6/09/02. Remove anti-hover so flag can be thrown properly
   %flag.static = false;

   // z0dd - ZOD, 10/02/02. Hack for flag collision bug.
   %flag.searchSchedule = %game.schedule(10, "startFlagCollisionSearch", %flag);

   // apply the impulse to the flag object
   %flag.applyImpulse(%player.getWorldBoxCenter(), %vec);

   //don't forget to send the message
   //messageClient(%player.client, 'MsgCTFFlagDropped', '\c1You have left the mission area and lost the flag.~wfx/misc/flag_drop.wav', 0, 0, %player.holdingFlag.team);

   // z0dd - ZOD 3/30/02. Above message was sending the wrong varible to objective hud.
   messageClient(%player.client, 'MsgCTFFlagDropped', '\c1You have left the mission area and lost the flag. (Held: %4)~wfx/misc/flag_drop.wav', %client.name, 0, %flag.team, %held); // Yogi, 8/18/02. 3rd param changed 0 -> %client.name
   logEcho(%player.client.nameBase@" (pl "@%player@"/cl "@%player.client@") lost flag (out of bounds)"@" (Held: "@%held@")");
}

function SCtFGame::dropFlag(%game, %player)
{
   if(%player.holdingFlag > 0)
   {
      if (!%player.client.outOfBounds)
         %player.throwObject(%player.holdingFlag);
      else
         %game.boundaryLoseFlag(%player);
   }
}

function SCtFGame::applyConcussion(%game, %player)
{
   %game.dropFlag( %player );
}

function SCtFGame::vehicleDestroyed(%game, %vehicle, %destroyer)
{
    //vehicle name
    %data = %vehicle.getDataBlock();
    //%vehicleType = getTaggedString(%data.targetNameTag) SPC getTaggedString(%data.targetTypeTag);
    %vehicleType = getTaggedString(%data.targetTypeTag);
    if(%vehicleType !$= "MPB")
        %vehicleType = strlwr(%vehicleType);

    %enemyTeam = ( %destroyer.team == 1 ) ? 2 : 1;

    %scorer = 0;
    %multiplier = 1;

    %passengers = 0;
    for(%i = 0; %i < %data.numMountPoints; %i++)
        if(%vehicle.getMountNodeObject(%i))
            %passengers++;

    //what destroyed this vehicle
    if(%destroyer.client)
    {
        //it was a player, or his mine, satchel, whatever...
        %destroyer = %destroyer.client;
        %scorer = %destroyer;

        // determine if the object used was a mine
        if(%vehicle.lastDamageType == $DamageType::Mine)
            %multiplier = 2;
    }
    else if(%destroyer.getClassName() $= "Turret")
    {
        if(%destroyer.getControllingClient())
        {
            //manned turret
            %destroyer = %destroyer.getControllingClient();
            %scorer = %destroyer;
        }
        else
        {
            %destroyerName = "A turret";
            %multiplier = 0;
        }
    }
    else if(%destroyer.getDataBlock().catagory $= "Vehicles")
    {
        // Vehicle vs vehicle kill!
        if(%name $= "BomberFlyer" || %name $= "AssaultVehicle")
            %gunnerNode = 1;
        else
            %gunnerNode = 0;

        if(%destroyer.getMountNodeObject(%gunnerNode))
        {
            %destroyer = %destroyer.getMountNodeObject(%gunnerNode).client;
            %scorer = %destroyer;
        }
        %multiplier = 3;
    }
    else  // Is there anything else we care about?
        return;


    if(%destroyerName $= "")
        %destroyerName = %destroyer.name;

    if(%vehicle.team == %destroyer.team) // team kill
    {
        %pref = (%vehicleType $= "Assault Tank") ? "an" : "a";
        messageAll( 'msgVehicleTeamDestroy', '\c0%1 TEAMKILLED %3 %2!', %destroyerName, %vehicleType, %pref);
    }

    else // legit kill
    {
        //messageTeamExcept(%destroyer, 'msgVehicleDestroy', '\c0%1 destroyed an enemy %2.', %destroyerName, %vehicleType); // z0dd - ZOD, 8/20/02. not needed with new messenger on line below
        teamDestroyMessage(%destroyer, 'msgVehDestroyed', '\c5%1 destroyed an enemy %2!', %destroyerName, %vehicleType); // z0dd - ZOD, 8/20/02. Send teammates a destroy message
        messageTeam(%enemyTeam, 'msgVehicleDestroy', '\c0%1 destroyed your team\'s %2.', %destroyerName, %vehicleType);
        //messageClient(%destroyer, 'msgVehicleDestroy', '\c0You destroyed an enemy %1.', %vehicleType);

        if(%scorer)
        {
            %value = %game.awardScoreVehicleDestroyed(%scorer, %vehicleType, %multiplier, %passengers);
            %game.shareScore(%value);
        }
    }
}

function SCtFGame::awardScoreVehicleDestroyed(%game, %client, %vehicleType, %mult, %passengers)
{
    // z0dd - ZOD, 9/29/02. Removed T2 demo code from here

    if(%vehicleType $= "Grav Cycle")
        %base = %game.SCORE_PER_DESTROY_WILDCAT;
    else if(%vehicleType $= "Assault Tank")
        %base = %game.SCORE_PER_DESTROY_TANK;
    else if(%vehicleType $= "MPB")
        %base = %game.SCORE_PER_DESTROY_MPB;
    else if(%vehicleType $= "Turbograv")
        %base = %game.SCORE_PER_DESTROY_SHRIKE;
    else if(%vehicleType $= "Bomber")
        %base = %game.SCORE_PER_DESTROY_BOMBER;
    else if(%vehicleType $= "Heavy Transport")
        %base = %game.SCORE_PER_DESTROY_TRANSPORT;

    %total = ( %base * %mult ) + ( %passengers * %game.SCORE_PER_PASSENGER );

    %client.vehicleScore += %total;

    messageClient(%client, 'msgVehicleScore', '\c0You received a %1 point bonus for destroying an enemy %2.', %total, %vehicleType);
    %game.recalcScore(%client);
    return %total;
}

function SCtFGame::shareScore(%game, %client, %amount)
{
    // z0dd - ZOD, 9/29/02. Removed T2 demo code from here

    //error("share score of"SPC %amount SPC "from client:" SPC %client);
    // all of the player in the bomber and tank share the points
    // gained from any of the others
    %vehicle = %client.vehicleMounted;
    if(!%vehicle)
       return 0;

    %vehicleType = getTaggedString(%vehicle.getDataBlock().targetTypeTag);
    if(%vehicleType $= "Bomber" || %vehicleType $= "Assault Tank")
    {
        for(%i = 0; %i < %vehicle.getDataBlock().numMountPoints; %i++)
        {
            %occupant = %vehicle.getMountNodeObject(%i);
            if(%occupant)
            {
                %occCl = %occupant.client;
                if(%occCl != %client && %occCl.team == %client.team)
                {
                    // the vehicle has a valid teammate at this node
                    // share the score with them
                    %occCl.vehicleBonus += %amount;
                    %game.recalcScore(%occCl);
                }
            }
        }
    }
}

function SCtFGame::awardScoreTurretKill(%game, %victimID, %implement)
{
    if ((%killer = %implement.getControllingClient()) != 0) //award whoever might be controlling the turret
    {
        if (%killer == %victimID)
            %game.awardScoreSuicide(%victimID);
        else if (%killer.team == %victimID.team) //player controlling a turret killed a teammate
        {
            %killer.teamKills++;
            %game.awardScoreTurretTeamKill(%victimID, %killer);
            %game.awardScoreDeath(%victimID);
        }
        else
        {
            %killer.mannedturretKills++;
            %game.recalcScore(%killer);
            %game.awardScoreDeath(%victimID);
        }
    }
    else if ((%killer = %implement.owner) != 0) //if it isn't controlled, award score to whoever deployed it
    {
        if (%killer.team == %victimID.team)
        {
            %game.awardScoreDeath(%victimID);
        }
        else
        {
            %killer.turretKills++;
            %game.recalcScore(%killer);
            %game.awardScoreDeath(%victimID);
        }
    }
    //default is, no one was controlling it, no one owned it.  No score given.
}

function SCtFGame::testKill(%game, %victimID, %killerID)
{
   return ((%killerID !=0) && (%victimID.team != %killerID.team));
}

function SCtFGame::awardScoreKill(%game, %killerID)
{
   %killerID.kills++;
   %game.recalcScore(%killerID);
   return %game.SCORE_PER_KILL;
}

function checkVehicleCamping( %team )
{
   %position = $flagPos[%team];
   %radius = 5;
   InitContainerRadiusSearch(%position, %radius, $TypeMasks::VehicleObjectType );

   while ((%vehicle = containerSearchNext()) != 0)
   {
      %dist = containerSearchCurrRadDamageDist();

      if (%dist > %radius)
         continue;
      else
      {
         //if( %vehicle.team == %team )
            applyVehicleCampDamage( %vehicle );
      }
   }

   if( %team == 1 )
      Game.campThread_1 = schedule( 1000, 0, "checkVehicleCamping", 1 );
   else
      Game.campThread_2 = schedule( 1000, 0, "checkVehicleCamping", 2 );
}

function applyVehicleCampDamage( %vehicle )
{
   if( !isObject( %vehicle ) )
      return;

   if( %vehicle.getDamageState() $= "Destroyed" )
      return;

   %client = %vehicle.getMountNodeObject(0).client; // grab the pilot

   messageClient( %client, 'serverMessage', "Can't park vehicles in flag zones!" );
   %vehicle.getDataBlock().damageObject( %vehicle, 0, "0 0 0", 0.5, 0);
}

// z0dd - ZOD, 10/02/02. Hack for flag collision bug.
function SCtFGame::startFlagCollisionSearch(%game, %flag)
{
   %flag.searchSchedule = %game.schedule(10, "startFlagCollisionSearch", %flag); // SquirrelOfDeath, 10/02/02. Moved from after the while loop
   %pos = %flag.getWorldBoxCenter();
   InitContainerRadiusSearch( %pos, 1.0, $TypeMasks::VehicleObjectType | $TypeMasks::CorpseObjectType | $TypeMasks::PlayerObjectType );
   while((%found = containerSearchNext()) != 0)
   {
      %flag.getDataBlock().onCollision(%flag, %found);
      // SquirrelOfDeath, 10/02/02. Removed break to catch all players possibly intersecting with flag
   }
}

/////////////////////////////////////////////////////////////////////////////////////////
// VOTING ///////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

// function SCTFGame::sendGameVoteMenu(%game, %client, %key)
// {
//    DefaultGame::sendGameVoteMenu(%game, %client, %key);
//    if ( %game.scheduleVote $= "" )
//    {
//       if(%client.isAdmin)
//          messageClient( %client, 'MsgVoteItem', "", %key, 'VoteAntiTurtleTime', 'change the anti turtle time to', 'Change Anti-Turtle time' );
//       else
//         messageClient( %client, 'MsgVoteItem', "", %key, 'VoteAntiTurtleTime', 'change the anti turtle time to', 'Vote Anti-Turtle time' );
//    }
// }

// function SCTFGame::evalVote(%game, %typeName, %admin, %arg1, %arg2, %arg3, %arg4)
// {
//    DefaultGame::evalVote(%game, %typeName, %admin, %arg1, %arg2, %arg3, %arg4);
//    switch$ (%typeName)
//    {
//       case "voteAntiTurtleTime":
//          %game.voteAntiTurtleTime(%admin, %arg1, %arg2, %arg3, %arg4);
//    }
// }

// function SCTFGame::sendAntiTurtleTimeList( %game, %client, %key )
// {
//    messageClient( %client, 'MsgVoteItem', "", %key, 6, "", '6 minutes' );
//    messageClient( %client, 'MsgVoteItem', "", %key, 8, "", '8 minutes' );
//    messageClient( %client, 'MsgVoteItem', "", %key, 10, "", '10 minutes' );
//    messageClient( %client, 'MsgVoteItem', "", %key, 12, "", '12 minutes' );
//    messageClient( %client, 'MsgVoteItem', "", %key, 14, "", '14 minutes' );
//    messageClient( %client, 'MsgVoteItem', "", %key, 16, "", '16 minutes' );
//    messageClient( %client, 'MsgVoteItem', "", %key, 18, "", '18 minutes' );
//    messageClient( %client, 'MsgVoteItem', "", %key, 200, "", 'Disable Anti Turtle' );
// }

function SCtFGame::sendGameVoteMenu(%game, %client, %key)
{
   DefaultGame::sendGameVoteMenu(%game, %client, %key);
   if ( %game.scheduleVote $= "" )
   {
      //echo(%client.ForceVote);
      if(%client.ForceVote $= "skip_confirm+")
         return;

      if(!%isAdmin || (%isAdmin && %client.ForceVote))
      {
         messageClient( %client, 'MsgVoteItem', "", %key, 'VoteArmorClass', 'change the armor class to', 'Vote to change the Armor class' );
         //messageClient( %client, 'MsgVoteItem', "", %key, 'VoteAntiTurtleTime', 'change the anti turtle time to', 'Vote Anti-Turtle time' );
      }
      else
      {
         messageClient( %client, 'MsgVoteItem', "", %key, 'VoteArmorClass', 'change the armor class to', 'Change the Armor class' );
         //messageClient( %client, 'MsgVoteItem', "", %key, 'VoteAntiTurtleTime', 'change the anti turtle time to', 'Change Anti-Turtle time' );
      }
   }
}

// function SCtFGame::sendAntiTurtleTimeList( %game, %client, %key )
// {
//    messageClient( %client, 'MsgVoteItem', "", %key, 6, "", '6 minutes' );
//    messageClient( %client, 'MsgVoteItem', "", %key, 8, "", '8 minutes' );
//    messageClient( %client, 'MsgVoteItem', "", %key, 10, "", '10 minutes' );
//    messageClient( %client, 'MsgVoteItem', "", %key, 12, "", '12 minutes' );
//    messageClient( %client, 'MsgVoteItem', "", %key, 14, "", '14 minutes' );
//    messageClient( %client, 'MsgVoteItem', "", %key, 16, "", '16 minutes' );
//    messageClient( %client, 'MsgVoteItem', "", %key, 18, "", '18 minutes' );
//    messageClient( %client, 'MsgVoteItem', "", %key, 200, "", 'Disable Anti Turtle' );
// }

function SCtFGame::sendArmorClassList(%game, %client, %key)
{
   messageClient( %client, 'MsgVoteItem', "", %key, "Light", "", 'Light Class' );
   messageClient( %client, 'MsgVoteItem', "", %key, "Medium", "", 'Medium Class' );
   messageClient( %client, 'MsgVoteItem', "", %key, "Heavy", "", 'Heavy Class' );
}

function serverCmdGetArmorClassList( %client, %key )
{
   if ( isObject( Game ) )
      Game.sendArmorClassList( %client, %key );
}

function SCtFGame::evalVote(%game, %typeName, %admin, %arg1, %arg2, %arg3, %arg4)
{
   DefaultGame::evalVote(%game, %typeName, %admin, %arg1, %arg2, %arg3, %arg4);
   switch$ (%typeName)
   {
      case "voteAntiTurtleTime":
         %game.voteAntiTurtleTime(%admin, %arg1, %arg2, %arg3, %arg4);

      case "VoteArmorClass":
         %game.VoteArmorClass(%admin, %arg1, %arg2, %arg3, %arg4);
   }
}

// function SCtFGame::voteAntiTurtleTime(%game, %admin, %newLimit)
// {
//    if( %newLimit == 200 )
//       %display = "disabled";
//    else
//       %display = %newLimit;

//    %cause = "";
//    if ( %admin )
//    {
//       messageAll('MsgAdminForce', '\c3%1\c2 set the anti-turtle time to %2.~wfx/misc/diagnostic_on.wav', $AdminCl.name, %display);
//       $Host::ClassicAntiTurtleTime = %newLimit;
//       %cause = "(admin)";
//    }
//    else
//    {
//       %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
//       if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
//       {
//          messageAll('MsgVotePassed', '\c2The anti-turtle time is set to %1.', %display);
//          $Host::ClassicAntiTurtleTime = %newLimit;
//          %cause = "(vote)";
//       }
//       else
//          messageAll('MsgVoteFailed', '\c2The vote to change the anti-turtle time did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
//    }
//    if(%cause !$= "")
//       logEcho($AdminCl.name @ ": anti-turtle time set to "@%display SPC %cause, 1);
// }

function SCtFGame::VoteArmorClass(%game, %admin, %newLimit)
{
   %cause = "";
   if ( %admin )
   {
      messageAll('MsgAdminForce', '\c3%1\c2 set the armor class to %2.~wfx/misc/diagnostic_on.wav', $AdminCl.name, %newLimit);
      $Sctf::Armor = %newLimit;
      %cause = "(admin)";
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount)) > ($Host::VotePasspercent / 100))
      {
         messageAll('MsgVotePassed', '\c2The armor class was set to %1.', %newLimit);
         $Sctf::Armor = %newLimit;
         %cause = "(vote)";
      }
      else
         messageAll('MsgVoteFailed', '\c2The vote to change the armor class did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount) * 100));
   }
   switch$ ( %newLimit )
   {
      case "Light":
         setArmorDefaults(%newLimit);

      case "Medium":
         setArmorDefaults(%newLimit);

      case "Heavy":
         setArmorDefaults(%newLimit);
   }
   if(%cause !$= "")
      logEcho($AdminCl.name @ ": armor class set to "@%display SPC %cause, 1);
}

function serverCmdArmorDefaults(%client, %armor)
{
   if(%client.isAdmin)
   {
      Game.VoteArmorClass(true, %armor);
   }
}
