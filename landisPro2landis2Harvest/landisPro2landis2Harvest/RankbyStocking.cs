﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Landis.Extension.Landispro.Harvest
{
    class RankbyStocking: StandRankingAlgorithm
    {
        public RankbyStocking(int someManagementAreaId, int someRotationAge) : base(someManagementAreaId, someRotationAge)
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
                SortKeyArrayDouble[i] = computeStandStocking(stand);
            }
            descendingSort_doubleArray(theStandArray, SortKeyArrayDouble, theLength);
            assign(theStandArray, theLength, ref theRankedList);
            SortKeyArrayDouble = null;
        }

        public double computeStandStocking(Stand stand)
        {
            Ldpoint p = new Ldpoint();
            Landis.Extension.Succession.Landispro.site site;
            double count = 0;
            int m;
            int k;
            double num_trees = 0; //N
            double Diameters = 0; //D
            double Diameters_square = 0; //D^2
            double x = BoundedPocketStandHarvester.pCoresites.stocking_x_value;
            double y = BoundedPocketStandHarvester.pCoresites.stocking_y_value;
            double z = BoundedPocketStandHarvester.pCoresites.stocking_z_value;
            Landis.Extension.Succession.Landispro.landunit l;
            for (StandIterator it = new StandIterator(stand); it.moreSites(); it.gotoNextSite())
            {
                p = it.getCurrentSite();
                l = BoundedPocketStandHarvester.pCoresites.locateLanduPt((uint)p.y, (uint)p.x);
                site = BoundedPocketStandHarvester.pCoresites[(uint)p.y, (uint)p.x];
                count += 1;
                for (k = 1; k <= BoundedPocketStandHarvester.pCoresites.SpecNum; k++)
                {
                    for (m = 1; m <= site.specAtt(k).Longevity / BoundedPocketStandHarvester.pCoresites.SuccessionTimeStep; m++)
                    {
                        num_trees += site.SpecieIndex(k).getTreeNum(m, k);
                        Diameters += BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.LtID) * site.SpecieIndex(k).getTreeNum(m, k) / 2.54;
                        Diameters_square += BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.LtID) * BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.LtID) * site.SpecieIndex(k).getTreeNum(m, k) / 2.54 / 2.54;
                    }
                }
            }
            return (x * num_trees + y * Diameters + z * Diameters_square) / (BoundedPocketStandHarvester.pCoresites.CellSize * BoundedPocketStandHarvester.pCoresites.CellSize / 4046.86) / stand.numberOfActiveSites();
        }

    }
}
