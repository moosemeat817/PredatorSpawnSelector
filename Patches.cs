using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.AI;
using UnityEngine;
using System.Collections;

namespace PredatorSpawnSelector;

internal static class Patches
{
    [HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.Deserialize))]
    internal class OnlyTimberwolvesDeserialize
    {
        private static void Postfix(SpawnRegion __instance)
        {
            if (__instance.m_AiSubTypeSpawned == AiSubType.Wolf)
            {
                WolfManager.AdjustToRegionSetting(__instance);
            }
            else if (__instance.m_AiSubTypeSpawned == AiSubType.Bear)
            {
                BearManager.AdjustBearToRegionSetting(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.Start))]
    internal class OnlyTimberwolvesStart
    {
        private static void Postfix(SpawnRegion __instance)
        {
            if (__instance.m_AiSubTypeSpawned == AiSubType.Wolf)
            {
                WolfManager.AdjustToRegionSetting(__instance);
            }
            else if (__instance.m_AiSubTypeSpawned == AiSubType.Bear)
            {
                BearManager.AdjustBearToRegionSetting(__instance);
            }
        }
    }

    // Patch to handle challenge bear critical hits when BaseAi starts
    [HarmonyPatch(typeof(BaseAi), "Start")]
    internal class BaseAiStart
    {
        private static void Postfix(BaseAi __instance)
        {
            // Check if this is a challenge bear
            AiBearChallengeHunted challengeBearComponent = __instance.GetComponent<AiBearChallengeHunted>();
            if (challengeBearComponent != null)
            {
                // Set m_IgnoreCriticalHits to false to allow critical hits on challenge bears
                __instance.m_IgnoreCriticalHits = false;
                MelonLoader.MelonLogger.Msg("Challenge bear spawned - enabled critical hits (m_IgnoreCriticalHits = false)");
            }
        }
    }

    // Patch for scene initialization to handle cougars, bears, and wolves - using Awake for initial setup
    [HarmonyPatch(typeof(GameManager), "Awake")]
    internal class GameManagerAwake
    {
        private static void Postfix()
        {
            // Handle cougar territories when scene loads
            CougarManager.HandleCougarTerritoriesForCurrentScene();

            // Handle bear spawn regions when scene loads
            BearManager.HandleBearSpawnRegionsForCurrentScene();

            // Handle wolf spawn regions when scene loads
            WolfManager.HandleWolfSpawnRegionsForCurrentScene();
        }
    }

    // Additional patch for scene start to handle cougars, bears, and wolves with better timing
    [HarmonyPatch(typeof(GameManager), "Start")]
    internal class GameManagerStart
    {
        private static void Postfix()
        {
            // Handle cougar territories, bear spawn regions, and wolf spawn regions again after scene is fully started
            // This ensures all GameObjects are properly loaded before we try to disable them
            MelonLoader.MelonCoroutines.Start(DelayedAnimalHandling());
        }

        private static IEnumerator DelayedAnimalHandling()
        {
            // Wait a frame to ensure all objects are fully loaded
            yield return null;
            CougarManager.HandleCougarTerritoriesForCurrentScene();
            BearManager.HandleBearSpawnRegionsForCurrentScene();
            WolfManager.HandleWolfSpawnRegionsForCurrentScene();
        }
    }

    // Utility method for safe GameObject disabling (used by cougar patches)
    internal static void SafeDisable(string objectPath, string territoryName)
    {
        GameObject go = GameObject.Find(objectPath);
        if (go != null)
        {
            go.SetActive(false);
            MelonLoader.MelonLogger.Msg($"Successfully disabled {territoryName} at {objectPath}");
        }
        else
        {
            MelonLoader.MelonLogger.Warning($"Could not find {territoryName} at {objectPath}");
        }
    }
}