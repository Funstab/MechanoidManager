using Verse;

namespace MechanoidManager
{
    public class MechanoidManagerSettings : ModSettings
    {
        public bool disableMechanoids = false;
        public float mechanoidFrequency = 1.0f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref disableMechanoids, "disableMechanoids", false);
            Scribe_Values.Look(ref mechanoidFrequency, "mechanoidFrequency", 1.0f);
        }
    }
}
