using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Landis.Extension.Landispro.Harvest
{
    class BoundedPocketStandHarvester: StandHarvest
    {
        public static int currentDecade;
        public static Landis.Extension.Succession.Landispro.speciesattrs pspeciesAttrs;
        public static Landis.Extension.Succession.Landispro.sites pCoresites;
        public static int giRow;
        public static int giCol;
        public static Landis.Extension.Succession.Landispro.pdp m_pPDP;
        public static int numberOfSpecies;
        public static int iParamstandAdjacencyFlag;
        public static int iParamharvestDecadeSpan;
        public static double fParamharvestThreshold;
        public static Landis.Extension.Succession.Landispro.map16 visitationMap = new Landis.Extension.Succession.Landispro.map16();
        public static Landis.Extension.Succession.Landispro.map16 standMap = new Landis.Extension.Succession.Landispro.map16();
        public static Landis.Extension.Succession.Landispro.map16 managementAreaMap = new Landis.Extension.Succession.Landispro.map16();
        public static Stands pstands;
        public static ManagementAreas managementAreas = new ManagementAreas();
        public static HarvestEventQueue harvestEvents = new HarvestEventQueue();
        public static StreamWriter harvestOutputFile1;
        public static string harvestOutputFile1_name = null;
        public static StreamWriter harvestOutputFile2;
        public static string harvestOutputFile2_name = null;
        public static ushort currentHarvestEventId;
        public static HARVESTSites pHarvestsites;

        public static int harvestWriteReportFirstTime = 1;

        private int itsTargetCut;
        private Ldpoint itsStartPoint;
        private List<Ldpoint> itsNeighborList = new List<Ldpoint>();

        public BoundedPocketStandHarvester(int targetCut, Ldpoint startPoint, SiteHarvester siteHarvester, HarvestPath path)
        {
            int standId = (int)standMap.getvalue32out((uint)startPoint.y, (uint)startPoint.x); //changed By Qia on Nov 4 2008      
            setStand(pstands[standId]);
            setSiteHarvester(siteHarvester);
            setPath(path);
            itsTargetCut = targetCut;
            itsStartPoint = startPoint;
        }

        public int EVENT_GROUP_SELECTION_REGIME_70_clear_cut(uint i, uint j)
        {
            int k;
            int m;
            int sitecut = 0;
            double TmpBasalAreaS = 0;
            Landis.Extension.Succession.Landispro.landunit l;
            l = pCoresites.locateLanduPt(i, j);
            for (k = 1; k <= pCoresites.SpecNum; k++)
            {
                if (pCoresites.flag_cut_GROUP_CUT[k - 1] == 1)
                {
                    for (m = 1; m <= pCoresites[i, j].specAtt(k).Longevity / pCoresites.SuccessionTimeStep; m++)
                    {
                        if (pCoresites[i, j].SpecieIndex(k).getTreeNum(m, k) > 0)
                        {
                            sitecut = 1;
                            TmpBasalAreaS = pCoresites.GetGrowthRates(k, m, l.LtID) * pCoresites.GetGrowthRates(k, m, l.LtID) / 4 * 3.1415926 * pCoresites[i, j].SpecieIndex(k).getTreeNum(m, k) / 10000.00;
                            if (pCoresites[i, j].specAtt(k).MinSproutAge <= m * pCoresites.SuccessionTimeStep && pCoresites[i, j].specAtt(k).MaxSproutAge >= m * pCoresites.SuccessionTimeStep)
                            {
                                pCoresites[i, j].SpecieIndex(k).TreesFromVeg += (int)pCoresites[i, j].SpecieIndex(k).getTreeNum(m, k);
                            }
                            pCoresites[i, j].SpecieIndex(k).setTreeNum(m, k, 0);
                            pHarvestsites.AddMoreValueHarvestBA_spec((int)i, (int)j, k - 1, TmpBasalAreaS);
                            pHarvestsites.AddMoreValueHarvestBA((int)i, (int)j, TmpBasalAreaS);
                        }
                    }
                }
            }
            if (sitecut >= 1)
            {
                pHarvestsites[(int)i, (int)j].harvestType = (short)getSiteHarvester().getHarvestType();
                pHarvestsites[(int)i, (int)j].lastHarvest = (short)currentDecade;
                for (k = 1; k <= pCoresites.SpecNum; k++)
                {
                    if (pCoresites.flag_plant_GROUP_CUT[k - 1] == 1)
                    {
                        uint tree_left = pCoresites[i, j].SpecieIndex(k).getTreeNum(1, k);
                        pCoresites[i, j].SpecieIndex(k).setTreeNum(1, k, pCoresites.num_TreePlant_GROUP_CUT[k - 1] + (int)tree_left);
                    }
                }
            }
            return sitecut;
        }


        public int harvest_EVENT_GROUP_SELECTION_REGIME_70(Ldpoint pt)
        {
            //int i, k;
            int k = 0;
            int[] r = new int[4];
            int siteCut = 0;
            int sumCut = 0;
            int c = 0;
            visitationMap[(uint)itsStartPoint.y, (uint)itsStartPoint.x] = currentHarvestEventId;
            itsNeighborList.Add(itsStartPoint);

            while (sumCut < itsTargetCut && itsNeighborList.Count > 0)
            {
                c = itsNeighborList.Count;
                pt = itsNeighborList[0];
                itsNeighborList.RemoveAt(0);

                if (GlobalFunctions.canBeHarvested(pt))
                {
                    siteCut = EVENT_GROUP_SELECTION_REGIME_70_clear_cut((uint)pt.y, (uint)pt.x);
                    sumCut += siteCut;

                    if (siteCut > 0)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            r[i] = i;
                        }
                        for (int i = 4; i > 0; i--)
                        {
                            k = (int)(i * Landis.Extension.Succession.Landispro.system1.frand());

                            //orignal no control over k, potential out of bound err 
                            //<Add By Qia on Nov 1 2012>
                            if (k < 0)
                            {
                                k = 0;
                            }
                            if (k > 3)
                            {
                                k = 3;
                            }
                            //</Add By Qia on Nov 1 2012>
                            switch (r[k])
                            {
                                case 0:
                                    addSiteNeighbor(pt.y, pt.x - 1);
                                    break;
                                case 1:
                                    addSiteNeighbor(pt.y, pt.x + 1);
                                    break;
                                case 2:
                                    addSiteNeighbor(pt.y - 1, pt.x);
                                    break;
                                case 3:
                                    addSiteNeighbor(pt.y + 1, pt.x);
                                    break;
                            }
                            r[k] = r[i - 1];
                            if (k < 0 || k > 3 || (i - 1) < 0 || (i - 1) > 3)
                            {
                                Console.Write("group selection index error\n");
                            }
                        }
                    }
                }
            }
            return sumCut;
        }


        public override int Harvest()
        {

            Ldpoint pt = new Ldpoint();
            int i = 0;
            int k = 0;
            int[] r = new int[4];
            int siteCut = 0;
            int sumCut = 0;
            int c = 0;
            visitationMap[(uint)itsStartPoint.y, (uint)itsStartPoint.x] = currentHarvestEventId;
            itsNeighborList.Add(itsStartPoint);
            while (sumCut < itsTargetCut && itsNeighborList.Count > 0)
            {
                c = itsNeighborList.Count;
                pt = itsNeighborList[0];
                itsNeighborList.RemoveAt(0);

                if (GlobalFunctions.canBeHarvested(pt))
                {
                    siteCut = harvest_EVENT_GROUP_SELECTION_REGIME_70(pt);
                    sumCut += siteCut;
                    if (siteCut > 0)
                    {
                        for (i = 0; i < 4; i++)
                        {
                            r[i] = i;
                        }
                        for (i = 4; i > 0; i--)
                        {
                            k = (int)(i * Landis.Extension.Succession.Landispro.system1.frand());
                            //orignal no control over k, potential out of bound err 
                            //<Add By Qia on Nov 1 2012>
                            if (k < 0)
                            {
                                k = 0;
                            }
                            if (k > 3)
                            {
                                k = 3;
                            }
                            switch (r[k])
                            {
                                case 0:
                                    addSiteNeighbor(pt.y, pt.x - 1);
                                    break;

                                case 1:
                                    addSiteNeighbor(pt.y, pt.x + 1);
                                    break;

                                case 2:
                                    addSiteNeighbor(pt.y - 1, pt.x);
                                    break;

                                case 3:
                                    addSiteNeighbor(pt.y + 1, pt.x);
                                    break;

                            }
                            r[k] = r[i - 1];
                            if (k < 0 || k > 3 || (i - 1) < 0 || (i - 1) > 3)
                            {

                                Console.Write("group selection index error\n");
                            }
                        }
                    }
                }
                return sumCut;
            }

            return sumCut;
        }

        public void addSiteNeighbor(int r, int c)
        {
            Debug.Assert(currentHarvestEventId > 0);
            Debug.Assert(currentHarvestEventId < 65535);
            bool first_val = getStand().inStand(r, c);
            if (first_val && visitationMap[(uint)r, (uint)c] != currentHarvestEventId)
            {
                Ldpoint p = new Ldpoint(c, r);
                itsNeighborList.Add(p);
                visitationMap[(uint)r, (uint)c] = currentHarvestEventId;
            }
        }

    }
}
