﻿
using Verse;
using RimWorld;

namespace CaveBiome
{
    public static class Util_CaveBiome
    {
        // Crystal lamp.
        public static ThingDef CrystalLampDef
        {
            get
            {
                return ThingDef.Named("CrystalLamp");
            }
        }


        // Roof and cave well.
        public static ThingDef CaveRoofDef
        {
            get
            {
                return ThingDef.Named("CaveRoof");
            }
        }

        public static ThingDef CaveWellDef
        {
            get
            {
                return ThingDef.Named("CaveWell");
            }
        }
        
        // Weather and light.
        public static WeatherDef CaveCalmWeatherDef
        {
            get
            {
                return WeatherDef.Named("CaveCalm");
            }
        }

        public static GameConditionDef CaveEnvironmentGameConditionDef
        {
            get
            {
                return GameConditionDef.Named("CaveEnvironment");
            }
        }

        // Biome.
        public static BiomeDef CaveBiomeDef
        {
            get
            {
                return BiomeDef.Named("Cave");
            }
        }

        // Corpses generators.
        public static ThingDef AnimalCorpsesGeneratorDef
        {
            get
            {
                return ThingDef.Named("AnimalCorpsesGenerator");
            }
        }

        public static ThingDef VillagerCorpsesGeneratorDef
        {
            get
            {
                return ThingDef.Named("VillagerCorpsesGenerator");
            }
        }
    }
}
