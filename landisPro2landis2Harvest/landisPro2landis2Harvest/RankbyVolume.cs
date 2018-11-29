using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Landis.Extension.Landispro.Harvest
{
    class RankbyVolume: StandRankingAlgorithm
    {
        public RankbyVolume(int someManagementAreaId, int someRotationAge) : base(someManagementAreaId, someRotationAge)
        {
        }

        public override void read(StreamReader infile)
        {
        }

        public override void rankStands(ref List<int> theRankedList)
        {
            int i;
            int id;
            Stand stand;
            int theLength = 0;
            double[] SortKeyArrayDouble;
            IntArray theStandArray = new IntArray(itsManagementArea.numberOfStands());
            IntArray theAgeArray = new IntArray(itsManagementArea.numberOfStands());
            IntArray theSortKeyArray = new IntArray(itsManagementArea.numberOfStands());
            filter(ref theStandArray, ref theAgeArray, ref theLength);
            SortKeyArrayDouble = new double[theLength + 1];

            for (i = 1; i <= theLength; i++)
            {
                id = theStandArray[i];
                stand = BoundedPocketStandHarvester.pstands[id];
                SortKeyArrayDouble[i] = computeStandBA(stand);
            }
            descendingSort_doubleArray(theStandArray, SortKeyArrayDouble, theLength);
            assign(theStandArray, theLength, ref theRankedList);
            SortKeyArrayDouble = null;
        }

        public double computeStandBA(Stand stand)
        {
            Ldpoint p = new Ldpoint();
            Landis.Extension.Succession.Landispro.site site;
            double count = 0;
            int m;
            int k;
            double TmpBasalAreaS = 0;
            Landis.Extension.Succession.Landispro.landunit l;
            for (StandIterator it = new StandIterator(stand); it.moreSites(); it.gotoNextSite())
            {
                p = it.getCurrentSite();
                site = BoundedPocketStandHarvester.pCoresites[(uint)p.y, (uint)p.x];
                l = BoundedPocketStandHarvester.pCoresites.locateLanduPt((uint)p.y, (uint)p.x);
                count += 1;
                for (k = 1; k <= BoundedPocketStandHarvester.pCoresites.SpecNum; k++)
                {
                    for (m = 1; m <= site.specAtt(k).Longevity / BoundedPocketStandHarvester.pCoresites.SuccessionTimeStep; m++)
                    {
                        TmpBasalAreaS += BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.LtID) * BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.LtID) / 4 * 3.1415926 * site.SpecieIndex(k).getTreeNum(m, k) / 10000.00;
                    }
                }
            }
            if (count > 0)
            {
                TmpBasalAreaS = TmpBasalAreaS / count;
            }
            return TmpBasalAreaS;
        }

    }
}
