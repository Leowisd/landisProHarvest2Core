// This file is part of the Harvest Management library for LANDIS-II.

namespace Landis.Library.HarvestManagement
{
    /// <summary>
    /// A requirement that a stand must meet in order to be eligible for
    /// ranking.
    /// </summary>
    public interface IRankingRequirement
    {
        /// <summary>
        /// Does a stand meet the requirement?
        /// </summary>
        bool MetBy(Stand stand);
    }
}
