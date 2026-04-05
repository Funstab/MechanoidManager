using Verse;

namespace MechanoidManager
{
    public class MechanoidManagerSettings : ModSettings
    {
        public bool disableMechanoids;
        public float mechanoidFrequency = 1f;
        public bool easierMechanoids;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref disableMechanoids, "disableMechanoids", false);
            Scribe_Values.Look(ref mechanoidFrequency, "mechanoidFrequency", 1f);
            Scribe_Values.Look(ref easierMechanoids, "easierMechanoids", false);
            Clamp();
        }

        public void Clamp()
        {
            if (mechanoidFrequency < 0f)
            {
                mechanoidFrequency = 0f;
            }
            else if (mechanoidFrequency > 2f)
            {
                mechanoidFrequency = 2f;
            }
        }

        public void Reset()
        {
            disableMechanoids = false;
            mechanoidFrequency = 1f;
            easierMechanoids = false;
        }
    }
}
