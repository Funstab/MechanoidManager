using UnityEngine;
using Verse;

namespace MechanoidManager
{
    public class MechanoidManagerMod : Mod
    {
        MechanoidManagerSettings settings;

        public MechanoidManagerMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<MechanoidManagerSettings>();
        }

        public override string SettingsCategory() => "Mechanoid Manager";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Disable Mechanoids", ref settings.disableMechanoids, "Completely disable mechanoids from appearing or raiding.");
            listingStandard.Label($"Mechanoid Raid Frequency: {settings.mechanoidFrequency * 100}%");
            settings.mechanoidFrequency = listingStandard.Slider(settings.mechanoidFrequency, 0.1f, 2.0f);
            listingStandard.End();
            settings.Write();
        }
    }
}
