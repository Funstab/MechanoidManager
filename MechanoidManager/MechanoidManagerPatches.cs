using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace MechanoidManager
{
    [StaticConstructorOnStartup]
    public static class MechanoidManagerPatches
    {
        static MechanoidManagerPatches()
        {
            var harmony = new Harmony("com.funstab.mechanoidmanager");
            harmony.PatchAll();
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

    [HarmonyPatch(typeof(Pawn), "PreApplyDamage")]
    public static class Pawn_PreApplyDamage_Patch
    {
        static void Prefix(Pawn __instance, ref DamageInfo dinfo)
        {
            var settings = LoadedModManager.GetMod<MechanoidManagerMod>().GetSettings<MechanoidManagerSettings>();

            if (settings.easierMechanoids && __instance.RaceProps.IsMechanoid)
            {
                dinfo.SetAmount(dinfo.Amount - 2); // Reduce damage by 2
            }
        }
    }

    [HarmonyPatch(typeof(ThingDef), "ResolveReferences")]
    public static class ThingDef_ResolveReferences_Patch
    {
        static void Postfix(ThingDef __instance)
        {
            var settings = LoadedModManager.GetMod<MechanoidManagerMod>().GetSettings<MechanoidManagerSettings>();

            if (settings.easierMechanoids && __instance.race != null && __instance.race.IsMechanoid)
            {
                var health = __instance.statBases.FirstOrDefault(s => s.stat == StatDefOf.MaxHitPoints);
                if (health != null)
                {
                    health.value -= 2; // Reduce health
                }

                var sharpArmor = __instance.statBases.FirstOrDefault(s => s.stat == StatDefOf.ArmorRating_Sharp);
                if (sharpArmor != null)
                {
                    sharpArmor.value -= 2; // Reduce sharp armor
                }

                var bluntArmor = __instance.statBases.FirstOrDefault(s => s.stat == StatDefOf.ArmorRating_Blunt);
                if (bluntArmor != null)
                {
                    bluntArmor.value -= 2; // Reduce blunt armor
                }

                var meleeDamage = __instance.statBases.FirstOrDefault(s => s.stat == StatDefOf.MeleeWeapon_DamageMultiplier);
                if (meleeDamage != null)
                {
                    meleeDamage.value -= 2; // Reduce melee damage
                }

                var meleeCooldown = __instance.statBases.FirstOrDefault(s => s.stat == StatDefOf.MeleeWeapon_CooldownMultiplier);
                if (meleeCooldown != null)
                {
                    meleeCooldown.value += 2; // Increase cooldown
                }

                var flammability = __instance.statBases.FirstOrDefault(s => s.stat == StatDefOf.Flammability);
                if (flammability != null)
                {
                    flammability.value = 1.0f; // Make mechanoids flammable
                }
                else
                {
                    __instance.statBases.Add(new StatModifier { stat = StatDefOf.Flammability, value = 1.0f });
                }

                if (__instance.building != null && __instance.building.turretGunDef != null)
                {
                    var range = __instance.building.turretGunDef.statBases.FirstOrDefault(s => s.stat == StatDefOf.RangedWeapon_Cooldown);
                    if (range != null)
                    {
                        range.value -= 2; // Reduce turret range
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Building), "SpawnSetup")]
    public static class Building_SpawnSetup_Patch
    {
        static void Postfix(Building __instance, Map map)
        {
            var settings = LoadedModManager.GetMod<MechanoidManagerMod>().GetSettings<MechanoidManagerSettings>();

            if (settings.easierMechanoids && __instance.Faction != null && __instance.Faction.def.defName == "Mechanoid")
            {
                var shield = __instance.def.statBases.FirstOrDefault(s => s.stat == StatDefOf.RangedWeapon_Cooldown);
                if (shield != null)
                {
                    shield.value -= 2; // Reduce shield range
                }
            }
        }
    }
}
