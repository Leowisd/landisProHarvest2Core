using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;


namespace Landis.Extension.Succession.Landispro
{
    public class pdp
    {
        private struct bdapdp
        {
            public short[,] sTSLBDA; //tinesince last BDA
            public char[,] cBDASeverity;
            public char[] BDAType;
        }

        private uint iCols, iRows;
        
        //BGrowth
        
        //private int[,] iTotalLiveBiomass;
        //private int[,] iLiveBiomassBySpecies;
        //private int[,] iCoarseWoodyDebris;
        //private int[,] iDeadFineBiomass;

        //Succession
        private short[,] sTSLMortality;

        //Add by YYF 2018/11
        //Harvest
        public short[,] sTSLHarvest;
        public char[,] cHarvestEvent;

        public void addedto_sTSLMortality(uint i, uint j, short added_value)
        {
            sTSLMortality[i, j] += added_value;
        }



        public pdp() { }


        public pdp(int mode, uint col, uint row)
        {
            sTSLHarvest = null;
            cHarvestEvent = null;
            set_parameters(mode, col, row);
        }


        //public void set_parameters(int mode, uint col, uint row, int BDANo)
        public void set_parameters(int mode, uint col, uint row)
        {
            iCols   = col;
            iRows   = row;

            //Harvest
            if (sTSLHarvest == null)
                sTSLHarvest = new short[iRows, iCols];
            if (cHarvestEvent == null)
                cHarvestEvent = new char[iRows, iCols];

            //Succession
            uint array_row = iRows + 1;
            uint array_col = iCols + 1;

            sTSLMortality = new short[array_row, array_col];
            
            
        }




        ~pdp()
        {
            sTSLMortality 		  = null;
            sTSLHarvest = null;
            cHarvestEvent = null;
        }
    }
}
