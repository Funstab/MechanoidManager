using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace MechanoidManager
{
    [StaticConstructorOnStartup]
    public static class MechanoidManagerPatches
    {
        static MechanoidManagerPatches()
        {
            var harmony = new Harmony("com.funstab.mechanoidmanager");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryExecuteWorker")]
    public static class IncidentWorker_RaidEnemy_TryExecuteWorker_Patch
    {
        static bool Prefix(ref bool __result, IncidentParms parms)
        {
            var settings = LoadedModManager.GetMod<MechanoidManagerMod>().GetSettings<MechanoidManagerSettings>();

            // Check if mechanoids are disabled
            if (settings.disableMechanoids && parms.faction != null && parms.faction.def.defName == "Mechanoid")
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_MechCluster), "TryExecuteWorker")]
    public static class IncidentWorker_MechCluster_TryExecuteWorker_Patch
    {
        static bool Prefix(ref bool __result, IncidentParms parms)
        {
            var settings = LoadedModManager.GetMod<MechanoidManagerMod>().GetSettings<MechanoidManagerSettings>();

            // Check if mechanoids are disabled
            if (settings.disableMechanoids)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(StorytellerComp), "IncidentChanceFinal")]
    public static class StorytellerComp_IncidentChanceFinal_Patch
    {
        static void Postfix(ref float __result, IIncidentTarget target, IncidentDef def)
        {
            var settings = LoadedModManager.GetMod<MechanoidManagerMod>().GetSettings<MechanoidManagerSettings>();

            if (def == IncidentDefOf.MechCluster)
            {
                __result *= settings.mechanoidFrequency;
            }
        }
    }
}
