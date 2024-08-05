using Verse;

namespace MechanoidManager
{
    public class MechanoidManagerSettings : ModSettings
    {
        public bool disableMechanoids = false;
        public float mechanoidFrequency = 1.0f; // 1.0f means 100% (normal frequency)
        public bool easierMechanoids = false; // New setting for easier mechanoids

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref disableMechanoids, "disableMechanoids", false);
            Scribe_Values.Look(ref mechanoidFrequency, "mechanoidFrequency", 1.0f);
            Scribe_Values.Look(ref easierMechanoids, "easierMechanoids", false); // Expose new setting
        }
    }
}
