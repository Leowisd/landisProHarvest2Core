﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Landis.Extension.Landispro.Harvest
{
    class ManagementArea
    {
        public int itsId;
        public int itsTotalSites;
        public int itsActiveSites;
        public int itsUpdateFlag;
        public List<int> itsStandList = new List<int>();

        public ManagementArea()
        {
            itsId = 0;
            itsTotalSites = 0;
            itsActiveSites = 0;
            itsUpdateFlag = 1;
        }

        public void setUpdateFlag()
        {
            itsUpdateFlag = 1;
        }

        public int getId()
        {
            return itsId;
        }

        public int inManagementArea(int r, int c)
        {
            uint tempMid = 0;
            uint sid;

            sid = (uint)BoundedPocketStandHarvester.standMap.getvalue32out((uint) r, (uint) c);
            if (sid > 0)
                tempMid = BoundedPocketStandHarvester.pstands[(int)sid].getManagementAreaId();
            int result;
            if (BoundedPocketStandHarvester.managementAreaMap.inMap((uint) r, (uint) c) && tempMid == itsId)
                result = 1;
            else result = 0;

            return result;
        }

        public int inManagementArea(Ldpoint p)
        {
            return inManagementArea(p.y, p.x);
        }

        public int numberOfSites()
        {
            return itsTotalSites;
        }

        public int numberofActiveSites()
        {
            return itsActiveSites;
        }

        public int numberOfStands()
        {
            return itsStandList.Count();
        }

        public void update()
        {
            if (itsUpdateFlag == 1)
                itsUpdateFlag = 0;
        }

        public void addStand(uint id)
        {
            if (!itsStandList.Contains((int)id))
                itsStandList.Add((int)id);
        }
    }
}
