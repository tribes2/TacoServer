//Fixes for collision tunneling and other issues, note only tested in classic
//Script By:DarkTiger
//v3.4   switch over to using SAT/OBB for hitbox detection accuracy, also heavy optimizations becuase of this change
//v3.3 - fixed ceiling deadstoping,fixed wall and ceiling tunneling $antiObjTunnel
//V3.2 - script refactor, removed flag sim in favor of just an offset on toss

$ftEnable = 1;//disables all
$limitFlagCalls = 1; // prevents frame perfect events witch can cause bad outcomes
$antiCeiling = 1; // note this is auto enabled with $boxStuckFix as it needs to check for this
$antiObjTunnel = 0;//prevents terrain and interior tunneling more thigns can be added see first part of DefaultGame::flagColTest
$antiFlagImpluse = 1000;//time out period to prevent explosions from effecting flags on drop/toss

$boxStuckFix = 1;// enables flag offset, spawns the flag outside of the player to keep it from getting stuck
$flagOffset = 1;// how far to offset the flag  1m seems like it works 90% of the time

// adds initial update to setVelocity and setTransform to updates its parameters across clients
//enable $flagResetTime with setting it to 5000 if you disable a mempatch

//expermental flag static fix
//memPatch("60456c","11000018");//transform
memPatch("6040ff","01"); //setVelocity
$flagResetTime = 0;// 1000-5000 if you want this feature enabled, resets flag to stand in case of desync should not be needed

//best to leave these values alone unless you understand what the code is doing
$flagSimTime = 64;//note a higher the time, the larger the sweep scans will be
$flagCheckRadius = 50;
$playerSizeBox = "1.2 1.2 2.3";
$flagBoxSize = "0.796666 0.139717 2.46029";

//0 = old AABB method uses fixed box size makes the player bit narrow
//1 = new OBB method uses perfect box intersection
//2 = AABB method but uses boundbox can make the player larger then it is given there direction
//3 = AABB fixed sizes  uses  $playerSizeBox and $flagBoxSize to do the box checking
$boxCollision = 1;// off is the old AABB method aka the old method
$flagColBypass = 0;//bypass all other collision methods other then this one

package flagFix{
   //because the fade is 2000ms we can compensate the delayed reset not to mess with client timers
   function Flag::onCollision(%data,%obj,%col){
      if(!$flagColBypass){
         parent::onCollision(%data,%obj,%col);
      }
   }

   function ShapeBase::throwObject(%this,%obj){
      parent::throwObject(%this,%obj);
      %data = %obj.getDatablock();
      if($ftEnable && %data.getName() $= "Flag"){
         %tpos = %obj.getTransform();
         %fpos = getWords(%tpos,0, 2);
         %obj.dtLastPos = %fpos;
         %vel = %obj.getVelocity();
         %posOffset = VectorAdd(%fpos, VectorScale(VectorNormalize(%vel), 2));

         if($antiCeiling){//flag height 2.46029
            //0.1 offset any fp errors with the flag position being at ground level, 2.4 offset flag height offset + some extra
            %upRay = containerRayCast(vectorAdd(%fpos,"0 0 0.1"), vectorAdd(%fpos,"0 0 2.5"), $TypeMasks::InteriorObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::ForceFieldObjectType, %obj);
            if(%upRay){
               %obj.setTransform(vectorSub(%this.getPosition(),"0 0" SPC 0.3) SPC getWords(%this.getTransform(),3,6));
               %obj.setVelocity(getWords(%vel,0,1) SPC 0);
            }
         }
         if($boxStuckFix && !%upRay){
            %wallMask = $TypeMasks::TerrainObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::StaticObjectType | $TypeMasks::ForceFieldObjectType | $TypeMasks::VehicleObjectType;
            %wallRay = containerRayCast(%fpos, %posOffset, %wallMask, %obj);

            %fwoPos = VectorAdd(%fpos, VectorScale(VectorNormalize(%vel), $flagOffset));

            %upRay = containerRayCast(vectorAdd(%fwoPos,"0 0 0.1"), vectorAdd(%fwoPos,"0 0 2.5"), $TypeMasks::InteriorObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::ForceFieldObjectType, %obj);

            if(!%wallRay){// make sure we dont end up in a wall
              if(!%upRay){
                  %obj.setTransform(%fwoPos SPC getWords(%this.getTransform(),3,6));
                  %obj.dtLastPos = %fwoPos;
               }
            }
         }
      }
   }

   function CTFGame::startMatch(%game){
      parent::startMatch(%game);
      if(!isEventPending(Game.flagLoop)){
         $TeamFlag[1].resetTime = 0;
         $TeamFlag[2].resetTime = 0;
         %game.atHomeFlagLoop();
      }
   }

   function SCtFGame::startMatch(%game){
      parent::startMatch(%game);
      if(!isEventPending(Game.flagLoop)){
         $TeamFlag[1].resetTime = 0;
         $TeamFlag[2].resetTime = 0;
         %game.atHomeFlagLoop();
      }
   }

   function PracticeCTFGame::startMatch(%game){
      parent::startMatch(%game);
       if(!isEventPending(Game.flagLoop)){
         $TeamFlag[1].resetTime = 0;
         $TeamFlag[2].resetTime = 0;
         %game.atHomeFlagLoop();
      }
   }

   //prevents frame perfect flag touches witch can cause bad stuff to happen in ctf
   function CTFGame::playerTouchFlag(%game, %player, %flag){
      if(%flag.lastFlagCallms > 0 && $limitFlagCalls){
         %timeDif = getSimTime() - %flag.lastFlagCallms;
         if(%timeDif < 32){
            return;
         }
      }
      %flag.lastFlagCallms = getSimTime();
      parent::playerTouchFlag(%game, %player, %flag);
   }

   function SCtFGame::playerTouchFlag(%game, %player, %flag){
      if(%flag.lastFlagCallms > 0 && $limitFlagCalls){
         %timeDif = getSimTime() - %flag.lastFlagCallms;
         if(%timeDif < 32){
            return;
         }
      }
      %flag.lastFlagCallms = getSimTime();
      parent::playerTouchFlag(%game, %player, %flag);
   }

   function PracticeCTFGame::playerTouchFlag(%game, %player, %flag){
       if(%flag.lastFlagCallms > 0 && $limitFlagCalls){
         %timeDif = getSimTime() - %flag.lastFlagCallms;
         if(%timeDif < 32){
            return;
         }
      }
      %flag.lastFlagCallms = getSimTime();
      parent::playerTouchFlag(%game, %player, %flag);
   }

   function CTFGame::playerDroppedFlag(%game, %player){
      %flag = %player.holdingFlag;
      %flag.lastDropTime = getSimTime();
      parent::playerDroppedFlag(%game, %player);
   }

   function SCtFGame::playerDroppedFlag(%game, %player){
      %flag = %player.holdingFlag;
      %flag.lastDropTime = getSimTime();
      parent::playerDroppedFlag(%game, %player);
   }

   function PracticeCTFGame::playerDroppedFlag(%game, %player){
      %flag = %player.holdingFlag;
      %flag.lastDropTime = getSimTime();
      parent::playerDroppedFlag(%game, %player);
   }

   function Flag::shouldApplyImpulse(%data, %obj){
      %val = parent::shouldApplyImpulse(%data, %obj);
      if(%val && $antiFlagImpluse > 0 && %obj.lastDropTime > 0){
         %time = getSimTime() - %obj.lastDropTime;
         if(%time < $antiFlagImpluse){
             %val = 0;
         }
      }
      return %val;
   }
};
activatePackage(flagFix);

function FlagCollision(%data,%obj,%col){
   if($flagColBypass){
      if (%col.getDataBlock().className $= Armor)
      {
         if (%col.isMounted())
            return;

         // z0dd - ZOD, 6/13/02. Touch the flag and your invincibility and cloaking goes away.
         if(%col.station $= "" && %col.isCloaked())
         {
            if( %col.respawnCloakThread !$= "" )
            {
               Cancel(%col.respawnCloakThread);
               %col.setCloaked( false );
               %col.respawnCloakThread = "";
            }
         }
         if( %col.client > 0 )
         {
            %col.setInvincibleMode(0, 0.00);
            %col.setInvincible( false );
         }

         // a player hit the flag
         Game.playerTouchFlag(%col, %obj);
      }
   }
   else{
      %data.onCollision(%flag, %player);
   }
}

function vectorMul(%a,%b){
   %x = getWords(%a,0) * getWords(%b,0);
   %y = getWords(%a,1) * getWords(%b,1);
   %z = getWords(%a,2) * getWords(%b,2);
 return %x SPC %y SPC %z;
}

function generateBoxData(){

   %playerSize = $playerSizeBox; //"1.2 1.2 2.3";
   %halfSize = vectorMul(%playerSize, "0.5 0.5 0");
   $plrBoxMin = %minA = VectorSub("0 0 0", %halfSize);
   $plrBoxMax = %maxA = getWords(%halfSize,0,1) SPC getWord(%playerSize,2);
   $plrBox = %minA SPC %maxA;
   %vSubA = vectorSub(%maxA, %minA);

   %flagSize = $flagBoxSize;
   %halfSize = vectorMul(%flagSize, "0.5 0.5 0");
   $flagBoxMin = %minB = VectorSub("0 0 -0.1", %halfSize);
   $flagBoxMax = %maxB = getWords(%halfSize,0,1) SPC getWord(%flagSize,2);
   $flagBox = %minB SPC %maxB;
   %vSubB = vectorSub(%maxB, %minB);

   %box[0] = "0 0 0";
   %box[1] = "1 0 0";
   %box[2] = "0 1 0";
   %box[3] = "1 1 0";
   %box[4] = "0 0 1";
   %box[5] = "1 0 1";
   %box[6] = "0 1 1";
   %box[7] = "1 1 1";

   for(%i = 0; %i < 8; %i++){
      $playerBoxData[%i] = vectorAdd(%minA, vectorMul(%vSubA, %box[%i]));
      $flagBoxData[%i] =   vectorAdd(%minB, vectorMul(%vSubB, %box[%i]));
   }

}generateBoxData();

function vectorLerp(%point1, %point2, %t) {
	return vectorAdd(%point1, vectorScale(vectorSub(%point2, %point1), %t));
}

function boxIntersectAABB(%plr, %flg, %lerpPos){
   if($boxCollision == 2){
      %a = %plr.getWorldBox();
      %b = %flg.getWorldBox();
   }
   else if($boxCollision == 3){
      %fpos = %flg.getPosition();

      %a = vectorAdd($plrBoxMin, %lerpPos) SPC vectorAdd($plrBoxMax, %lerpPos);
      %b = vectorAdd($flagBoxMin, %fpos) SPC vectorAdd($flagBoxMax, %fpos);
   }
   else{
      %plrMinMax = %plr.getObjectBox();
      %a = vectorAdd(getWords(%plrMinMax,0,2), %lerpPos) SPC vectorAdd(getWords(%plrMinMax,3,5), %lerpPos);
      %b = %flg.getWorldBox();
   }

   return (getWord(%a, 0) <= getWord(%b, 3) && getWord(%a, 3) >= getWord(%b, 0)) &&
         (getWord(%a, 1) <= getWord(%b, 4) && getWord(%a, 4) >= getWord(%b, 1)) &&
         (getWord(%a, 2)<= getWord(%b, 5) && getWord(%a, 5) >= getWord(%b, 2));
}

function DefaultGame::flagColTest(%game, %flag, %rsTeam, %ext){
////////////////////////////////////////////////////////////////////////////////
//obj tunneling check
   %flagPos = %flag.getPosition();
   if(!%flag.isHome && $antiObjTunnel){
      %fOffset =vectorAdd(%flagPos,"0 0 0.1");
      %dist = vectorDist(%flag.dtLastPos, %fOffset);
      if(%dist > 2.5){//2.5 is the rough flag height
         %wallMask = $TypeMasks::TerrainObjectType | $TypeMasks::InteriorObjectType;
         %terRay = containerRayCast(%flag.dtLastPos, %fOffset, %wallMask, %flag);
         if(%terRay){
            %flag.setTransform(%flag.dtLastPos SPC getWords(%flag.getTransform(), 3, 6));
         }
         else{
            %flag.dtLastPos = %fOffset;
         }
      }
   }
////////////////////////////////////////////////////////////////////////////////
//flag collision check
   InitContainerRadiusSearch( %flagPos, $flagCheckRadius, $TypeMasks::PlayerObjectType);
   while((%player = containerSearchNext()) != 0){
      %playerPos = %player.getPosition();
      if((%rsTeam && %flag.team != %player.team) || !%rsTeam || %player == %ext){
         %flagDist = vectorDist(%flagPos, %playerPos);
         if(%player.lastSim > 0 && (%player.getState() !$= "Dead")){// only check at speed
            //%fdot = vectorDot(vectorNormalize(%player.getVelocity()),vectorNormalize(VectorSub(%flagPos, %playerPos)));
            // %tickDist = vectorLen(%player.getVelocity()) * ($flagSimTime/1000);
            %sweepCount = mFloor(vectorDist(%playerPos, %player.oldPos) + 1.5);
            if((getSimTime() - %player.lastSim) <= 128 && %flagDist-2 <  %sweepCount){//make sure are last position is valid
               //error(%player SPC %flagDist SPC %sweepCount);
               for(%i = 0; %i < %sweepCount; %i++){// sweep behind us to see if  we should have hit something
                  %lerpPos = vectorLerp(%playerPos, %player.oldPos, %i/(%sweepCount-1));//back sweep
                  if($boxCollision == 1){
                     if(boxIntersect(%player, %flag, %lerpPos)){
                        //error("flag hit");
                        FlagCollision(%flag.getDataBlock(),%flag, %player);
                        break;
                     }
                  }
                  else{
                     if(boxIntersectAABB(%player, %flag, %lerpPos)){
                        //error("flag hit");
                        FlagCollision(%flag.getDataBlock(),%flag, %player);
                        break;
                     }
                  }
               }
            }
         }
      }
      %player.oldPos = %playerPos;
      %player.lastSim = getSimTime();
   }
   //error("scan count" SPC %scanCount SPC %scanPlrCount);
}

function DefaultGame::atHomeFlagLoop(%game){
   if(isObject($TeamFlag[1]) && isObject($TeamFlag[2])){
      if($flagResetTime > 0){
         if(%game.flagResetTime > $flagResetTime){
            if($TeamFlag[1].isHome){
               $TeamFlag[1].setTransform($TeamFlag[1].originalPosition);
            }
            if($TeamFlag[2].isHome){
               $TeamFlag[2].setTransform($TeamFlag[2].originalPosition);
            }
              %game.flagResetTime = 0;
         }
         %game.flagResetTime += $flagSimTime;
      }

      if($TeamFlag[1].isHome && $TeamFlag[2].isHome){//11
         %game.flagColTest($TeamFlag[1],1,0);// only look at the other team
         %game.flagColTest($TeamFlag[2],1,0);// only look at the other team
      }
      else if(!$TeamFlag[1].isHome && $TeamFlag[2].isHome){//01
         if(isObject($TeamFlag[1].carrier)){// flag has been dropped
            %game.flagColTest($TeamFlag[2],1, $TeamFlag[1].carrier); //scan for other team expect for are carrier
         }
         else{
            %game.flagColTest($TeamFlag[1],0,0);// scan for everyone can touch it
            %game.flagColTest($TeamFlag[2],1,0);// team 2 flag is still at home so  only scan for the other team
         }
      }
      else if($TeamFlag[1].isHome && !$TeamFlag[2].isHome){//10
         if(isObject($TeamFlag[2].carrier)){// flag has been dropped
            %game.flagColTest($TeamFlag[1],1, $TeamFlag[2].carrier); //scan for other team expect for are carrier
         }
         else{
            %game.flagColTest($TeamFlag[1],1,0);// team 1 flag is still at home so  only scan for the other team
            %game.flagColTest($TeamFlag[2],0,0);// scan for everyone can touch it
         }
      }
      else if(!$TeamFlag[1].isHome && !$TeamFlag[2].isHome){//00
         if(!isObject($TeamFlag[1].carrier)){// flag has been dropped
            %game.flagColTest($TeamFlag[1],0,0);// scan for everyone can touch it
         }
         if(!isObject($TeamFlag[2].carrier)){// flag has been dropped
            %game.flagColTest($TeamFlag[2],0,0);// scan for everyone can touch it
         }
      }
   }
   if(isObject(Game) && $ftEnable){
      %speed = ($HostGamePlayerCount - $HostGameBotCount > 0) ? $flagSimTime : 30000;
      %game.flagLoop = %game.schedule(%speed, "atHomeFlagLoop");
   }
}

function boxIntersect(%objA, %objB, %scanPos) {
    // Retrieve the 8 corners of the box for both objects
    %matrixA = %objA.getTransform();
    if(getWordCount(%scanPos)){// need to check a postion other then  default
      %matrixA  = %scanPos SPC getWords(%matrixA,3,6);
    }
    %matrixB = %objB.getTransform();


    %cornerA[0] = MatrixMulPoint(%matrixA, "-0.6 -0.6 0");
    %cornerA[1] = MatrixMulPoint(%matrixA, "0.6 -0.6 0");
    %cornerA[2] = MatrixMulPoint(%matrixA, "-0.6 0.6 0");
    %cornerA[3] = MatrixMulPoint(%matrixA, "0.6 0.6 0");

    %cornerA[4] = MatrixMulPoint(%matrixA, "-0.6 -0.6 2.3");
    %cornerA[5] = MatrixMulPoint(%matrixA, "0.6 -0.6 2.3");
    %cornerA[6] = MatrixMulPoint(%matrixA, "-0.6 0.6 2.3");
    %cornerA[7] = MatrixMulPoint(%matrixA, "0.6 0.6 2.3");


    %cornerB[0] = MatrixMulPoint(%matrixB, "-0.398333 -0.0698583 -0.1");
    %cornerB[1] = MatrixMulPoint(%matrixB, "0.398333 -0.0698583 -0.1");
    %cornerB[2] = MatrixMulPoint(%matrixB, "-0.398333 0.0698587 -0.1");
    %cornerB[3] = MatrixMulPoint(%matrixB, "0.398333 0.0698587 -0.1");

    %cornerB[4] = MatrixMulPoint(%matrixB, "-0.398333 -0.0698583 2.46029");
    %cornerB[5] = MatrixMulPoint(%matrixB, "0.398333 -0.0698583 2.46029");
    %cornerB[6] = MatrixMulPoint(%matrixB, "-0.398333 0.0698587 2.46029");
    %cornerB[7] = MatrixMulPoint(%matrixB, "0.398333 0.0698587 2.46029");

    // Define the axes to test (these are the edges of both boxes)
    %ax[0] = vectorNormalize(vectorSub(%cornerA[1], %cornerA[0]));//X cross forward and up
    %ax[1] = vectorNormalize(vectorSub(%cornerA[2], %cornerA[0]));//Y forward vector
    %ax[2] = "0 0 1";

    %ax[3] = vectorNormalize(vectorSub(%cornerB[1], %cornerB[0]));//X cross forward and up
    %ax[4] = vectorNormalize(vectorSub(%cornerB[2], %cornerB[0])); //Y forward vector
    %ax[5] = "0 0 1";


    // For each axis
    for (%i = 0; %i < 6; %i++) {
        %axis = %ax[%i];

        // Project each corner of box A onto the axis
        %minProjA = vectorDot(%cornerA[0], %axis);
        %maxProjA = %minProjA;

        for (%j = 1; %j < 8; %j++) {
            %projA = vectorDot(%cornerA[%j], %axis);

            %minProjA = (%projA < %minProjA) ? %projA : %minProjA;
            %maxProjA = (%projA > %maxProjA) ? %projA : %maxProjA;
        }

        // Project each corner of box B onto the axis
        %minProjB = vectorDot(%cornerB[0], %axis);
        %maxProjB = %minProjB;

        for (%j = 1; %j < 8; %j++) {
            %projB = vectorDot(%cornerB[%j], %axis);
            %minProjB = (%projB < %minProjB) ? %projB : %minProjB;
            %maxProjB = (%projB > %maxProjB) ? %projB : %maxProjB;
        }

        // Check for overlap
        if (%maxProjA < %minProjB || %maxProjB < %minProjA) {
            return false; // No overlap on this axis, boxes do not intersect
        }
    }

    return true; // Overlap on all axes, boxes intersect
}

function colFlagTest(){
   %conObj = LocalClientConnection.player;  //LocalClientConnection.getControlObject();
   %flag1 = boxIntersect(%conObj,$TeamFlag[1]);
   %flag2 = boxIntersect(%conObj,$TeamFlag[2]);

   %flag1AABB = boxIntersectAABB(%conObj, $TeamFlag[1], %conObj.getPosition());
   %flag2AABB = boxIntersectAABB(%conObj, $TeamFlag[2], %conObj.getPosition());

   error("test" SPC "Flag 1" SPC %flag1 SPC "Flag 2" SPC %flag2);
   error("test" SPC "Flag 1A" SPC %flag1AABB SPC "Flag 2A" SPC %flag2AABB);
   bottomprint(%conObj.client, "FlagS" SPC %flag1 SPC "FlagI" SPC %flag2 SPC "FlagS AABB" SPC %flag1AABB SPC "FlagI AABB" SPC %flag2AABB, 5, 1);
}

function testFlagSpeed(%speed){
  %player = LocalClientConnection.player;
  %fvec = %player.getForwardVector();
  %vel = vectorScale(%fvec,%speed);
  %player.setVelocity(%vel);
}