﻿using System.Collections.Generic;

using Verse;
using RimWorld;
using CaveworldFlora;

namespace CaveBiome
{
    public class GenStep_CavePlants : GenStep_Plants
    {
        public const float plantMinGrowth = 0.07f;
        public const float plantMaxGrowth = 1.0f;

        public override void Generate(Map map, GenStepParams parms)
		{
            if (map.Biome != Util_CaveBiome.CaveBiomeDef)
            {
                // Use standard base function.
                base.Generate(map, parms);
                return;
            }
            
            // Enabled it to avoid error while checking new plant can be spawned nearby in same room.
            map.regionAndRoomUpdater.Enabled = true;
            var wildCavePlants = new List<ThingDef_ClusterPlant>();
            var wildCavePlantsWeighted = new Dictionary<ThingDef_ClusterPlant, float>();
            foreach (ThingDef def in map.Biome.AllWildPlants)
            {
                if (def is ThingDef_ClusterPlant cavePlantDef)
                {
                    wildCavePlants.Add(cavePlantDef);
                    wildCavePlantsWeighted.Add(cavePlantDef, map.Biome.CommonalityOfPlant(cavePlantDef) / cavePlantDef.clusterSizeRange.Average);
                }
            }

            var spawnTriesNumber = 10000;
            var totalSuccessfulSpawns = 0;
            var failedSpawns = 0;
            var totalFailedSpawns = 0;
            for (var tryIndex = 0; tryIndex < spawnTriesNumber; tryIndex++)
            {
                ThingDef_ClusterPlant cavePlantDef = wildCavePlants.RandomElementByWeight((ThingDef_ClusterPlant def) => wildCavePlantsWeighted[def]);

                var newDesiredClusterSize = cavePlantDef.clusterSizeRange.RandomInRange;
                IntVec3 spawnCell = IntVec3.Invalid;
                GenClusterPlantReproduction.TryGetRandomClusterSpawnCell(cavePlantDef, newDesiredClusterSize, false, map, out spawnCell); // Ignore temperature condition.
                if (spawnCell.IsValid)
                {
                    totalSuccessfulSpawns++;
                    failedSpawns = 0;
                    ClusterPlant newPlant = Cluster.SpawnNewClusterAt(map, spawnCell, cavePlantDef, newDesiredClusterSize);
                    newPlant.Growth = Rand.Range(ClusterPlant.minGrowthToReproduce, plantMaxGrowth);

                    var clusterIsMature = Rand.Value < 0.7f;
                    GrowCluster(newPlant, clusterIsMature);
                }
                else
                {
                    failedSpawns++;
                    totalFailedSpawns++;
                    if (failedSpawns >= 50)
                    {
                        break;
                    }
                }
            }
        }

        public static void GrowCluster(ClusterPlant plant, bool clusterIsMature)
        {
            Cluster cluster = plant.cluster;
            int seedPlantsNumber;
            if (clusterIsMature)
            {
                seedPlantsNumber = cluster.desiredSize - 1; // The first plant is already spawned.
            }
            else
            {
                seedPlantsNumber = (int)(cluster.desiredSize * Rand.Range(0.25f, 0.75f));
            }
            if (seedPlantsNumber == 0)
            {
                return;
            }
            for (var seedPlantIndex = 0; seedPlantIndex < seedPlantsNumber; seedPlantIndex++)
            {
                ClusterPlant seedPlant = GenClusterPlantReproduction.TryGrowCluster(cluster, false); // Ignore temperature condition.
                if (seedPlant != null)
                {
                    seedPlant.Growth = Rand.Range(plantMinGrowth, plantMaxGrowth);
                }
            }
            if (clusterIsMature
                && cluster.plantDef.symbiosisPlantDefEvolution != null)
            {
                ClusterPlant symbiosisPlant = GenClusterPlantReproduction.TrySpawnNewSymbiosisCluster(cluster);
                if (symbiosisPlant != null)
                {
                    symbiosisPlant.Growth = Rand.Range(plantMinGrowth, plantMaxGrowth);
                    GrowCluster(symbiosisPlant, clusterIsMature);
                }
            }
        }
    }
}
