﻿using System.Collections.Generic;
using System.Linq;

using Verse;
using RimWorld;

namespace CaveBiome
{
    public abstract class IncidentWorker_ShipPartCrashInCave : IncidentWorker_CrashedShipPart
    {
        private const float ShipPointsFactor = 0.9f;

        protected virtual int CountToSpawn => 1;
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            var map = (Map)parms.target;
            return map.listerThings.ThingsOfDef(def.mechClusterBuilding).Count <= 0;
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map)parms.target;
            var num = 0;
            var countToSpawn = CountToSpawn;
            IntVec3 vec = IntVec3.Invalid;
            for (var i = 0; i < countToSpawn; i++)
            {
                IntVec3 spawnCell = IntVec3.Invalid;
                if (map.Biome == Util_CaveBiome.CaveBiomeDef)
                {
                    TryFindShipCrashSite(map, out spawnCell);
                }
                else
                {
                    bool validator(IntVec3 c)
                    {
                        if (c.Fogged(map))
                        {
                            return false;
                        }
                        foreach (IntVec3 current in GenAdj.CellsOccupiedBy(c, Rot4.North, def.mechClusterBuilding.size))
                        {
                            if (!current.Standable(map))
                            {
                                var result = false;
                                return result;
                            }
                            if (map.roofGrid.Roofed(current))
                            {
                                var result = false;
                                return result;
                            }
                        }
                        return map.reachability.CanReachColony(c);
                    }
                    if (!CellFinderLoose.TryFindRandomNotEdgeCellWith(14, validator, map, out spawnCell))
                    {
                        break;
                    }
                }
                if (spawnCell.IsValid == false)
                {
                    break;
                }
                GenExplosion.DoExplosion(spawnCell, map, 3f, DamageDefOf.Flame, null, -1, -1,  null, null, null, null, null, 0f, 1, false, null, 0f, 1);
                var building_CrashedShipPart = (Building)GenSpawn.Spawn(def.mechClusterBuilding, spawnCell, map);
                building_CrashedShipPart.SetFaction(Faction.OfMechanoids, null);
                var points = parms.points * ShipPointsFactor;
                var list2 = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
                {
                    groupKind = PawnGroupKindDefOf.Combat,
                    tile = map.Tile,
                    faction = Faction.OfMechanoids,
                    points = points
                }, true).ToList<Pawn>();
                num++;
                vec = spawnCell;
            }
            if (num > 0)
            {
                Find.CameraDriver.shaker.DoShake(1f);
                Find.LetterStack.ReceiveLetter(def.letterLabel, def.letterText, def.letterDef, new TargetInfo(vec, map, false), null);
            }
            return num > 0;
        }

        public void TryFindShipCrashSite(Map map, out IntVec3 spawnCell)
        {
            spawnCell = IntVec3.Invalid;
            List<Thing> caveWellsList = map.listerThings.ThingsOfDef(Util_CaveBiome.CaveWellDef);
            foreach (Thing caveWell in caveWellsList.InRandomOrder())
            {
                if (IsValidPositionForShipCrashSite(map, caveWell.Position))
                {
                    spawnCell = caveWell.Position;
                    return;
                }
            }
        }

        public bool IsValidPositionForShipCrashSite(Map map, IntVec3 position)
        {
            if ((position.InBounds(map) == false)
                || position.Fogged(map))
            {
                return false;
            }
			foreach (IntVec3 checkedPosition in GenAdj.CellsOccupiedBy(position, Rot4.North, def.mechClusterBuilding.size))
			{
				if ((checkedPosition.Standable(map) == false)
                    || checkedPosition.Roofed(map)
                    || map.reachability.CanReachColony(checkedPosition) == false)
				{
					return false;
				}
			}
            return true;
        }
    }
}
