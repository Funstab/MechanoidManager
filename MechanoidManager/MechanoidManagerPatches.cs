using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
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

    public class MechanoidManagerGameComponent : GameComponent
    {
        private int nextRefreshTick;
        private bool pendingApply;

        public MechanoidManagerGameComponent(Game game)
        {
        }

        public override void GameComponentTick()
        {
            if (Current.Game == null || Find.TickManager == null)
            {
                return;
            }

            var settings = MechanoidManagerRuntime.Settings;
            if (settings == null)
            {
                return;
            }

            var currentTick = Find.TickManager.TicksGame;

            if (pendingApply)
            {
                pendingApply = false;
                nextRefreshTick = currentTick + 5;
            }

            if (!settings.easierMechanoids)
            {
                return;
            }

            if (currentTick < nextRefreshTick)
            {
                return;
            }

            nextRefreshTick = currentTick + 30;
            MechanoidManagerRuntime.ApplyEasierMechanoidStateNow();
        }

        public void RequestApply()
        {
            pendingApply = true;
        }
    }

    internal static class MechanoidManagerRuntime
    {
        internal const int MechStructureTargetHitPoints = 45;
        internal const float EasierMechDamageMultiplier = 4f;

        internal static MechanoidManagerSettings Settings => MechanoidManagerMod.Settings;

        private static MechanoidManagerGameComponent Component => Current.Game?.GetComponent<MechanoidManagerGameComponent>();

        internal static bool IsMechanoidFaction(Faction faction)
        {
            return faction != null && faction.def != null && faction.def.defName == "Mechanoid";
        }

        internal static bool IsHostileToPlayer(Faction faction)
        {
            return faction != null && Faction.OfPlayer != null && faction.HostileTo(Faction.OfPlayer);
        }

        internal static bool IsNonPlayerMechPawn(Pawn pawn)
        {
            if (pawn == null || pawn.Destroyed || pawn.Dead)
            {
                return false;
            }

            if (pawn.Faction == Faction.OfPlayer)
            {
                return false;
            }

            return pawn.RaceProps != null && pawn.RaceProps.IsMechanoid;
        }

        internal static bool IsHostileMechBuilding(Building building)
        {
            if (building == null || building.Destroyed)
            {
                return false;
            }

            if (building.Faction == Faction.OfPlayer)
            {
                return false;
            }

            if (IsMechanoidFaction(building.Faction) || IsHostileToPlayer(building.Faction))
            {
                return true;
            }

            var defName = building.def != null ? building.def.defName : null;
            if (string.IsNullOrEmpty(defName))
            {
                return false;
            }

            var lower = defName.ToLowerInvariant();
            return lower.Contains("mechcluster")
                || lower.Contains("mech")
                || lower.Contains("defoliator")
                || lower.Contains("psychicdrone")
                || lower.Contains("smokepop")
                || lower.Contains("assembler")
                || lower.Contains("turret");
        }

        internal static bool IsLikelyMechIncident(IncidentDef def)
        {
            if (def == null)
            {
                return false;
            }

            if (def == IncidentDefOf.MechCluster)
            {
                return true;
            }

            var name = def.defName;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return name.IndexOf("mech", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        internal static int RemoveLiveMechs()
        {
            if (Current.Game == null)
            {
                return 0;
            }

            var removed = 0;

            foreach (var map in Find.Maps)
            {
                var pawns = map.mapPawns.AllPawnsSpawned.ToList();
                foreach (var pawn in pawns)
                {
                    if (!IsNonPlayerMechPawn(pawn))
                    {
                        continue;
                    }

                    pawn.Destroy(DestroyMode.Vanish);
                    removed++;
                }

                var buildings = map.listerBuildings.allBuildingsColonist
                    .Concat(map.listerBuildings.allBuildingsNonColonist)
                    .Distinct()
                    .ToList();

                foreach (var building in buildings)
                {
                    if (!IsHostileMechBuilding(building))
                    {
                        continue;
                    }

                    building.Destroy(DestroyMode.Vanish);
                    removed++;
                }
            }

            return removed;
        }

        internal static int ApplyEasierMechanoidState()
        {
            if (Current.Game == null)
            {
                return 0;
            }

            var affected = CountLiveMechTargets();
            Component?.RequestApply();
            return affected;
        }

        internal static void ApplyEasierMechanoidStateNow()
        {
            if (Current.Game == null)
            {
                return;
            }

            foreach (var map in Find.Maps)
            {
                var pawns = map.mapPawns.AllPawnsSpawned.ToList();
                foreach (var pawn in pawns)
                {
                    if (!IsNonPlayerMechPawn(pawn))
                    {
                        continue;
                    }

                    WeakenMechPawnStep(pawn);
                }

                var buildings = map.listerBuildings.allBuildingsColonist
                    .Concat(map.listerBuildings.allBuildingsNonColonist)
                    .Distinct()
                    .ToList();

                foreach (var building in buildings)
                {
                    if (!IsHostileMechBuilding(building))
                    {
                        continue;
                    }

                    ClampThingHitPoints(building, MechStructureTargetHitPoints);
                }
            }
        }

        private static int CountLiveMechTargets()
        {
            var count = 0;

            foreach (var map in Find.Maps)
            {
                count += map.mapPawns.AllPawnsSpawned.Count(IsNonPlayerMechPawn);

                count += map.listerBuildings.allBuildingsColonist
                    .Concat(map.listerBuildings.allBuildingsNonColonist)
                    .Distinct()
                    .Count(IsHostileMechBuilding);
            }

            return count;
        }

        private static void WeakenMechPawnStep(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || pawn.Destroyed || !pawn.Spawned || pawn.health == null)
            {
                return;
            }

            float damage = 10f;

            if (pawn.kindDef != null)
            {
                if (pawn.kindDef.combatPower >= 400f)
                {
                    damage = 18f;
                }
                else if (pawn.kindDef.combatPower >= 200f)
                {
                    damage = 14f;
                }
            }

            try
            {
                pawn.TakeDamage(new DamageInfo(
                    DamageDefOf.Bullet,
                    damage,
                    999f,
                    -1f,
                    null,
                    null,
                    null,
                    DamageInfo.SourceCategory.ThingOrUnknown,
                    null));
            }
            catch
            {
            }
        }

        private static void ClampThingHitPoints(Thing thing, int targetHitPoints)
        {
            if (thing == null || thing.Destroyed || !thing.def.useHitPoints)
            {
                return;
            }

            var target = Math.Min(targetHitPoints, thing.MaxHitPoints);
            if (target < 1)
            {
                target = 1;
            }

            if (thing.HitPoints > target)
            {
                thing.HitPoints = target;
            }
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryExecuteWorker")]
    public static class IncidentWorker_RaidEnemy_TryExecuteWorker_Patch
    {
        static bool Prefix(ref bool __result, IncidentParms parms)
        {
            var settings = MechanoidManagerRuntime.Settings;
            if (settings == null)
            {
                return true;
            }

            if (settings.disableMechanoids && MechanoidManagerRuntime.IsMechanoidFaction(parms.faction))
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
        static bool Prefix(ref bool __result)
        {
            var settings = MechanoidManagerRuntime.Settings;
            if (settings == null)
            {
                return true;
            }

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
        static void Postfix(ref float __result, IncidentDef def)
        {
            var settings = MechanoidManagerRuntime.Settings;
            if (settings == null)
            {
                return;
            }

            if (MechanoidManagerRuntime.IsLikelyMechIncident(def))
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
            var settings = MechanoidManagerRuntime.Settings;
            if (settings == null || !settings.easierMechanoids)
            {
                return;
            }

            if (!MechanoidManagerRuntime.IsNonPlayerMechPawn(__instance))
            {
                return;
            }

            dinfo.SetAmount(dinfo.Amount * MechanoidManagerRuntime.EasierMechDamageMultiplier);
        }
    }

    [HarmonyPatch(typeof(Thing), "SpawnSetup")]
    public static class Thing_SpawnSetup_Patch
    {
        static void Postfix(Thing __instance)
        {
            var settings = MechanoidManagerRuntime.Settings;
            if (settings == null || !settings.easierMechanoids || Current.Game == null)
            {
                return;
            }

            if (__instance is Pawn pawn && MechanoidManagerRuntime.IsNonPlayerMechPawn(pawn))
            {
                Current.Game.GetComponent<MechanoidManagerGameComponent>()?.RequestApply();
                return;
            }

            if (__instance is Building building && MechanoidManagerRuntime.IsHostileMechBuilding(building))
            {
                Current.Game.GetComponent<MechanoidManagerGameComponent>()?.RequestApply();
            }
        }
    }
}
