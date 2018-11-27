// This file is part of the Harvest Management library for LANDIS-II.

using Landis.SpatialModeling;

namespace Landis.Library.HarvestManagement
{
    /// <summary>
    /// A stand ranking method based on the time since last fire and wind disturbances
    /// </summary>
    public class TimeSinceDisturbanceRank
        : StandRankingMethod
    {
        //---------------------------------------------------------------------

        public TimeSinceDisturbanceRank()
        {
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the rank for a stand.
        /// </summary>
        protected override double ComputeRank(Stand stand, int i)
        {
            SiteVars.GetExternalVars();
            
            foreach(IRequirement req in Requirements)
            {
                //check for fire or wind requirements
                if(req is TimeSinceLastFire || req is TimeSinceLastWind)
                {
                    if(req.MetBy(stand))
                    {
                        return 1;
                    }
                }
            }
            return 0; //if we are still here, stand does not meet requirements
        }
    }
}
