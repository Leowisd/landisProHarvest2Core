﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Landis.Extension.Landispro.Harvest
{
    class Stand
    {
        public int itsId;
        public uint itsManagementAreaId;
        public int itsTotalSites;
        public int itsActiveSites;
        private int itsHarvestableSites;
        private int itsMeanAge;
        private int itsUpdateFlag;
        private int itsRecentHarvestFlag;
        private int itsRank;
        private int itsReserveFlag;
        public Ldpoint itsMinPoint;
        public Ldpoint itsMaxPoint;
        public List<int> itsNeighborList = new List<int>();

        public Stand()
        {
            itsId = 0;
            itsManagementAreaId = 0;
            itsTotalSites = 0;
            itsActiveSites = 0;
            itsHarvestableSites = 0;
            itsMeanAge = 0;
            itsUpdateFlag = 1;
            itsRecentHarvestFlag = 0;
            itsRank = 0;
            itsReserveFlag = 0;
            itsMinPoint = new Ldpoint();
            itsMaxPoint= new Ldpoint();
        }

        public void Copy(Stand s)
        {
            itsId = s.itsId;
            itsManagementAreaId = s.itsManagementAreaId;
            itsTotalSites = s.itsTotalSites;
            itsActiveSites = s.itsActiveSites;
            itsHarvestableSites = s.itsHarvestableSites;
            itsMeanAge = s.itsMeanAge;
            itsUpdateFlag = s.itsUpdateFlag;
            itsRecentHarvestFlag = s.itsRecentHarvestFlag;
            itsRank = s.itsRank;
            itsReserveFlag = s.itsReserveFlag;
            itsMinPoint.x = s.itsMinPoint.x;
            itsMinPoint.y = s.itsMinPoint.y;
            itsMaxPoint.x = s.itsMaxPoint.x;
            itsMaxPoint.y = s.itsMaxPoint.y;
            for (int i = 0; i<s.itsNeighborList.Count; i++)
            {
                itsNeighborList[i] = s.itsNeighborList[i];
            }
        }

        public uint getManagementAreaId()
        {
            return itsManagementAreaId;
        }

        public bool neighborsWereRecentlyHarvested()
        {
            int id;
            foreach (int it in itsNeighborList)
            {
                id = it;
                if (BoundedPocketStandHarvester.pstands[id].wasRecentlyHarvested() == 1)
                {
                    return true;
                }
            }
            return false;

        }

        public int wasRecentlyHarvested()
        {
            update();
            return itsRecentHarvestFlag;
        }

        public uint isNeighbor(int r, int c)
        {
            uint nid;
            if (GlobalFunctions.inBounds(r, c) == 1)
            {
                return 0;
            }
            if ((nid = (uint)BoundedPocketStandHarvester.standMap.getvalue32out((uint)r, (uint)c)) <= 0 || nid == itsId)
            {
                return 0;
            }
            else
            {
                return nid;
            }
        }
        public void addNeighbor(uint id)
        {
            if (!itsNeighborList.Contains((int)id))
            {
                itsNeighborList.Add((int)id);
            }
        }

        public Boolean canBeHarvested()
        {
            //cerr << "int Stand::canBeHarvested() " << endl;
            update();
            return itsHarvestableSites > 0;
            // Jacob return !isReserved() && (itsHarvestableSites > 0);
        }

        public Ldpoint getMinPoint()
        {
            return itsMinPoint;
        }

        public Ldpoint getMaxPoint()
        {
            return itsMaxPoint;
        }

        public int getId()
        {
            return itsId;
        }

        public int numberOfActiveSites()
        {

            return itsActiveSites;

        }

        public bool inStand(int r, int c)
        {

            uint sid = 0;
            uint tempMid = 0;

            if (BoundedPocketStandHarvester.standMap.inMap((uint)r, (uint)c) == false)
            {
                return false;
            }

            sid = (uint)BoundedPocketStandHarvester.standMap.getvalue32out((uint)r, (uint)c); //change by Qia on Nov 4 2008
            if (sid > 0)
            {
                tempMid = BoundedPocketStandHarvester.pstands[(int)sid].getManagementAreaId();
            }
            Boolean result = (BoundedPocketStandHarvester.standMap.inMap((uint)r, (uint)c) && BoundedPocketStandHarvester.standMap.getvalue32out((uint)r, (uint)c) == itsId && tempMid == itsManagementAreaId); //change by Qia on Nov 4 2008

            return result;

        }

        public Ldpoint getRandomPoint()
        {
            Ldpoint pt = new Ldpoint();
            do
            {
                pt.x = Landis.Extension.Succession.Landispro.system1.irand(itsMinPoint.x, itsMaxPoint.x);
                pt.y = Landis.Extension.Succession.Landispro.system1.irand(itsMinPoint.y, itsMaxPoint.y);

            } while (!inStand(pt.y, pt.x));
            return pt;
        }



        public void update()
        {
            //static int count_update = 0;
            //count_update++;

            Ldpoint pt = new Ldpoint();

            Landis.Extension.Succession.Landispro.site site;

            if (itsUpdateFlag == 1)
            {
                if (itsActiveSites == 0)
                {
                    itsMeanAge = 0;
                    itsHarvestableSites = 0;
                    itsRecentHarvestFlag = 0;
                }
                else
                {
                    //static int get_updatecount = 0;
                    //get_updatecount++;
                    int sum = 0;
                    int rcount = 0;
                    itsHarvestableSites = 0;
                    Ldpoint tmp_pt = this.getMinPoint();
                    Ldpoint tmp_ptmax = this.getMaxPoint();
                    int temp_id = this.getId();
                    for (StandIterator it = new StandIterator(this); it.moreSites(); it.gotoNextSite())
                    {
                        pt = it.getCurrentSite();                 
                        site = BoundedPocketStandHarvester.pCoresites[(uint)pt.y, (uint)pt.x];
                        if (BoundedPocketStandHarvester.pCoresites.locateLanduPt((uint)pt.y, (uint)pt.x).active()) //original landis4.0: site->landUnit->active()
                        {
                            BoundedPocketStandHarvester.pHarvestsites.BefStChg(pt.y, pt.x); //Add By Qia on Nov 10 2008
                            sum += BoundedPocketStandHarvester.pHarvestsites[pt.y, pt.x].getMaxAge(pt.y, pt.x);
                            BoundedPocketStandHarvester.pHarvestsites.AftStChg(pt.y, pt.x); //Add By Qia on Nov 10 2008                     
                            if (BoundedPocketStandHarvester.standMap.getvalue32out((uint)pt.y, (uint)pt.x) > 0 && BoundedPocketStandHarvester.pHarvestsites[pt.y, pt.x].canBeHarvested(pt.y, pt.x)) //change by Qia on Nov 4 2008
                            {

                                itsHarvestableSites++;
                            }                        
                            if (BoundedPocketStandHarvester.pHarvestsites[pt.y, pt.x].wasRecentlyHarvested())
                            {
                                rcount++;
                            }
                        }
                    }
                    itsMeanAge = sum / numberOfActiveSites();                  
                    if ((float)rcount / numberOfActiveSites() < BoundedPocketStandHarvester.fParamharvestThreshold)
                    {
                        itsRecentHarvestFlag = 0;
                    }
                    else
                    {
                        itsRecentHarvestFlag = 1;
                    }                
                }
                itsUpdateFlag = 0;
            }
        }

        public int getAge()
        {
            update();
            return itsMeanAge;
        }

        public void setRank(int rank)
        {
            itsRank = rank;
        }

        public void setUpdateFlag()
        {
            itsUpdateFlag = 1;
        }
        public void reserve()
        {

            itsReserveFlag = 1;
        }
    }
}
