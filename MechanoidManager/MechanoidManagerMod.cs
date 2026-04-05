using RimWorld;
using UnityEngine;
using Verse;

namespace MechanoidManager
{
    public class MechanoidManagerMod : Mod
    {
        public static MechanoidManagerSettings Settings { get; private set; }

        public MechanoidManagerMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<MechanoidManagerSettings>();
            Settings.Clamp();
        }

        public override string SettingsCategory()
        {
            return "Mechanoid Manager";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.Clamp();

            var listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.CheckboxLabeled(
                "Disable mechanoid threats",
                ref Settings.disableMechanoids,
                "Blocks mechanoid raids and mech clusters.");

            listing.GapLine();
            listing.Label("Mechanoid threat frequency: " + (int)(Settings.mechanoidFrequency * 100f) + "%");
            Settings.mechanoidFrequency = listing.Slider(Settings.mechanoidFrequency, 0f, 2f);

            listing.GapLine();
            var easierBefore = Settings.easierMechanoids;
            listing.CheckboxLabeled(
                "Easier hostile mechs",
                ref Settings.easierMechanoids,
                "Non-player mech pawns are pushed down to about 22 HP. Mech turrets and mech structures are pushed down to about 45 HP while enabled.");

            if (Settings.easierMechanoids && !easierBefore && Current.Game != null)
            {
                var changed = MechanoidManagerRuntime.ApplyEasierMechanoidState();
                if (changed > 0)
                {
                    Messages.Message("Mechanoid Manager applied easier HP to " + changed + " live mech targets.", MessageTypeDefOf.TaskCompletion, false);
                }
            }

            listing.GapLine();

            if (Current.Game == null)
            {
                listing.Label("Load a save to use live mech actions.");
            }
            else
            {
                if (listing.ButtonText("Remove live hostile mechs now"))
                {
                    var removed = MechanoidManagerRuntime.RemoveLiveMechs();
                    Messages.Message("Mechanoid Manager removed " + removed + " live hostile mech targets.", MessageTypeDefOf.TaskCompletion, false);
                }

                if (listing.ButtonText("Apply easier mech HP now"))
                {
                    var changed = MechanoidManagerRuntime.ApplyEasierMechanoidState();
                    Messages.Message("Mechanoid Manager updated " + changed + " live hostile mech targets.", MessageTypeDefOf.TaskCompletion, false);
                }
            }

            listing.Gap(12f);
            if (listing.ButtonText("Reset to defaults"))
            {
                Settings.Reset();
            }

            listing.End();
        }

        public override void WriteSettings()
        {
            Settings.Clamp();
            base.WriteSettings();
        }
    }
}
