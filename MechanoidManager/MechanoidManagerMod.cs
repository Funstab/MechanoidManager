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
            listingStandard.Label($"Mechanoid Raid Frequency: {(int)(settings.mechanoidFrequency * 100)}%");
            settings.mechanoidFrequency = listingStandard.Slider(settings.mechanoidFrequency, 0f, 2.0f);
            listingStandard.CheckboxLabeled("Easier Mechanoids", ref settings.easierMechanoids, "Make mechanoids easier by reducing their stats and making them flammable.");
            listingStandard.End();
            settings.Write();
        }
    }
}
