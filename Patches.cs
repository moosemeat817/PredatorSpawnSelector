using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.AI;
using UnityEngine;
using System.Collections;

namespace PredatorSpawnSelector;

internal static class Patches
{
    private const float INITIAL_DELAY = 0.5f;  // Initial delay after scene start (seconds)
    private const float RECHECK_DELAY = 1.0f;  // Delay between rechecks to catch re-enabled regions (seconds)
    private const int MAX_RECHECKS = 5;        // Maximum number of times to recheck and re-disable

    [HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.Deserialize))]
    internal class OnlyTimberwolvesDeserialize
    {
        private static void Postfix(SpawnRegion __instance)
        {
            if (__instance.m_AiSubTypeSpawned == AiSubType.Wolf)
            {
                MelonLoader.MelonLogger.Msg($"[Deserialize] Wolf SpawnRegion found: {__instance.gameObject.name}");
                WolfManager.AdjustToRegionSetting(__instance);
            }
            else if (__instance.m_AiSubTypeSpawned == AiSubType.Bear)
            {
                MelonLoader.MelonLogger.Msg($"[Deserialize] Bear SpawnRegion found: {__instance.gameObject.name}");
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
                MelonLoader.MelonLogger.Msg($"[SpawnRegion.Start] Wolf SpawnRegion started: {__instance.gameObject.name}");
                WolfManager.AdjustToRegionSetting(__instance);
            }
            else if (__instance.m_AiSubTypeSpawned == AiSubType.Bear)
            {
                MelonLoader.MelonLogger.Msg($"[SpawnRegion.Start] Bear SpawnRegion started: {__instance.gameObject.name}");
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
                MelonLoader.MelonLogger.Msg($"[BaseAi.Start] Challenge bear spawned - enabled critical hits: {__instance.gameObject.name}");
            }
        }
    }

    // Patch for scene initialization to handle cougars, bears, and wolves - using Awake for initial setup
    [HarmonyPatch(typeof(GameManager), "Awake")]
    internal class GameManagerAwake
    {
        private static void Postfix()
        {
            MelonLoader.MelonLogger.Msg("[GameManager.Awake] Scene awake - starting animal handling");

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
            MelonLoader.MelonLogger.Msg("[GameManager.Start] Scene started - scheduling delayed animal handling");

            // Handle cougar territories, bear spawn regions, and wolf spawn regions with delays
            // This ensures all GameObjects are properly loaded before we try to disable them
            MelonLoader.MelonCoroutines.Start(DelayedAnimalHandling());
        }

        private static IEnumerator DelayedAnimalHandling()
        {
            // Initial delay to allow all objects to fully load
            MelonLoader.MelonLogger.Msg($"[DelayedAnimalHandling] Waiting {INITIAL_DELAY} seconds for initial scene load...");
            yield return new WaitForSeconds(INITIAL_DELAY);

            MelonLoader.MelonLogger.Msg("[DelayedAnimalHandling] Performing initial animal handling");
            CougarManager.HandleCougarTerritoriesForCurrentScene();
            BearManager.HandleBearSpawnRegionsForCurrentScene();
            WolfManager.HandleWolfSpawnRegionsForCurrentScene();

            // Rechecks to catch any regions that might have been re-enabled by randomizers
            for (int i = 0; i < MAX_RECHECKS; i++)
            {
                MelonLoader.MelonLogger.Msg($"[DelayedAnimalHandling] Waiting {RECHECK_DELAY} seconds before recheck {i + 1}/{MAX_RECHECKS}...");
                yield return new WaitForSeconds(RECHECK_DELAY);

                MelonLoader.MelonLogger.Msg($"[DelayedAnimalHandling] Performing recheck {i + 1}/{MAX_RECHECKS}");
                CougarManager.HandleCougarTerritoriesForCurrentScene();
                BearManager.HandleBearSpawnRegionsForCurrentScene();
                WolfManager.HandleWolfSpawnRegionsForCurrentScene();
            }

            MelonLoader.MelonLogger.Msg("[DelayedAnimalHandling] Completed all animal handling checks");
        }
    }

    // Utility method for safe GameObject disabling (used by cougar patches)
    internal static void SafeDisable(string objectPath, string territoryName)
    {
        GameObject go = GameObject.Find(objectPath);
        if (go != null)
        {
            bool wasActive = go.activeSelf;
            go.SetActive(false);
            MelonLoader.MelonLogger.Msg($"[SafeDisable] Successfully disabled {territoryName}");
            MelonLoader.MelonLogger.Msg($"  Path: {objectPath}");
            MelonLoader.MelonLogger.Msg($"  Was Active: {wasActive} | Now Active: {go.activeSelf}");
        }
        else
        {
            MelonLoader.MelonLogger.Warning($"[SafeDisable] Could not find {territoryName} at path: {objectPath}");
        }
    }
}