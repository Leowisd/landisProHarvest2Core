// This file is part of the Harvest Management library for LANDIS-II.

using Landis.SpatialModeling;

namespace Landis.Library.HarvestManagement
{
    /// <summary>
    /// A ranking requirement which marks a stand for harvest if it has met the time since last wind
    /// </summary>
    public class TimeSinceLastWind
        : IRequirement
    {
        private ushort timeSinceWind;

        //---------------------------------------------------------------------

        public TimeSinceLastWind(ushort windtime)
        {
            timeSinceWind = windtime;
        }

        //---------------------------------------------------------------------

        bool IRequirement.MetBy(Stand stand)
        {
            double avgtime = 0;
            int num = 0;

            if (SiteVars.TimeOfLastWind != null)
            {
                foreach (ActiveSite site in stand)
                {
                    avgtime = avgtime + SiteVars.TimeOfLastWind[(Site)site];
                    num = num + 1;
                }
            }

            avgtime = avgtime / num;

            if (num == 0)
            {
                return false;
            }

            return avgtime >= timeSinceWind;
        }
    }
}
