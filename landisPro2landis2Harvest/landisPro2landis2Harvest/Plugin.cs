using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Landis.Core;
using Landis.Library.HarvestManagement;
using System.Diagnostics;
using Landis.SpatialModeling;
using System.IO;
using Landis.Extension.Landispro;

namespace Landis.Extension.Landispro.Harvest
{
    public class PlugIn 
     : HarvestExtensionMain
    {
        public static readonly string ExtensionName = "Landis_pro Harvest";


        private static ICore modelCore;

        public PlugIn()
            : base(ExtensionName)
        {
        }

        public static ICore ModelCore
        {
            get
            {
                return modelCore;
            }
            private set
            {
                modelCore = value;
            }
        }

        public override void LoadParameters(string dataFile, ICore mCore)
        {
            modelCore = mCore;
            Console.WriteLine("The datafile is: " + dataFile);
            Console.WriteLine("Starting Parameters Loading...");
            Console.WriteLine();
            GlobalFunctions.HarvestPassInit(Landis.Extension.Succession.Landispro.PlugIn.gl_sites,
                                            Landis.Extension.Succession.Landispro.PlugIn.numSpecies,
                                            Landis.Extension.Succession.Landispro.PlugIn.gl_param.OutputDir, 
                                            dataFile,
                                            Landis.Extension.Succession.Landispro.PlugIn.pPDP);

            Console.WriteLine("Harvest Dll loaded in...");
            Console.WriteLine();

            GlobalFunctions.HarvestPass(Landis.Extension.Succession.Landispro.PlugIn.gl_sites, Landis.Extension.Succession.Landispro.PlugIn.gl_spe_Attrs);
            Landis.Extension.Succession.Landispro.PlugIn.gl_sites.Harvest70outputdim();

            Console.WriteLine("Finish getting input");
            Console.WriteLine();
        }

        public override void Initialize()
        {
            Console.WriteLine("Starting Initialization...");
            Console.WriteLine();

            Landis.Extension.Succession.Landispro.PlugIn.gl_sites.stocking_x_value = Landis.Extension.Succession.Landispro.PlugIn.gl_param.Stocking_x_value;
            Landis.Extension.Succession.Landispro.PlugIn.gl_sites.stocking_y_value = Landis.Extension.Succession.Landispro.PlugIn.gl_param.Stocking_y_value;
            Landis.Extension.Succession.Landispro.PlugIn.gl_sites.stocking_z_value = Landis.Extension.Succession.Landispro.PlugIn.gl_param.Stocking_z_value;
            Landis.Extension.Succession.Landispro.PlugIn.gl_sites.SuccessionTimeStep = Landis.Extension.Succession.Landispro.PlugIn.gl_param.SuccessionTimestep;
            Landis.Extension.Succession.Landispro.PlugIn.gl_sites.TimeStepHarvest = Landis.Extension.Succession.Landispro.PlugIn.gl_param.TimeStepHarvest;
            Timestep = Landis.Extension.Succession.Landispro.PlugIn.gl_sites.TimeStepHarvest;

            Landis.Extension.Succession.Landispro.PlugIn.numSpecies = Landis.Extension.Succession.Landispro.PlugIn.gl_spe_Attrs.NumAttrs;

            Landis.Extension.Succession.Landispro.PlugIn.freq[5] = 1;

            Console.WriteLine("Finish Initialization...");
            Console.WriteLine();
        }

        public override void Run()
        {
            Console.WriteLine("Starting Running Landis_pro Harvest...");
            Console.WriteLine();

            int i = modelCore.TimeSinceStart;

            if (i % Landis.Extension.Succession.Landispro.PlugIn.gl_sites.TimeStepHarvest == 0)
            {
                GlobalFunctions.HarvestPassCurrentDecade(i);
                for (int r = 1; r <= Landis.Extension.Succession.Landispro.PlugIn.snr; r++)
                {
                    for (int c = 1; c <= Landis.Extension.Succession.Landispro.PlugIn.snc; c++)
                    {
                        GlobalFunctions.setUpdateFlags(r, c);
                    }
                }
            }
            singularLandisIteration(i, Landis.Extension.Succession.Landispro.PlugIn.pPDP);

            Console.WriteLine("Finishing Running Landis_pro Harvest...");
            Console.WriteLine();
        }

        public void singularLandisIteration(int itr, Landis.Extension.Succession.Landispro.pdp ppdp)
        {
            DateTime ltime, ltimeTemp;
            TimeSpan ltimeDiff;

            string fptimeBU = Landis.Extension.Succession.Landispro.PlugIn.fpforTimeBU_name;
            using (StreamWriter fpforTimeBU = File.AppendText(Landis.Extension.Succession.Landispro.PlugIn.fpforTimeBU_name))
            {
                fpforTimeBU.WriteLine("\nProcessing succession at Year: {0}:", itr);

                if (itr % Landis.Extension.Succession.Landispro.PlugIn.gl_sites.TimeStepHarvest == 0)
                {
                    Console.WriteLine("Processing harvest events.\n");
                    ltime = DateTime.Now;
                    Harvest.GlobalFunctions.HarvestprocessEvents(itr / Landis.Extension.Succession.Landispro.PlugIn.gl_sites.SuccessionTimeStep);  //Global Function
                    putHarvestOutput(itr / Landis.Extension.Succession.Landispro.PlugIn.gl_sites.TimeStepHarvest, Landis.Extension.Succession.Landispro.PlugIn.wAdfGeoTransform); //output img files
                    ltimeTemp = DateTime.Now;
                    ltimeDiff = ltimeTemp - ltime;
                    fpforTimeBU.WriteLine("Processing harvest: " + ltimeDiff + " seconds");
                }
            }        
        }

        private static void putHarvestOutput(int itr, double[] wAdfGeoTransform)
        {
            Landis.Extension.Succession.Landispro.map8 m = new Landis.Extension.Succession.Landispro.map8(Landis.Extension.Succession.Landispro.PlugIn.gl_sites.Header);
            string str = null;
            string str1 = null;
            string str_htyp = null;
            string str_htyp1 = null;
            string str_dec = null;
            string str_dec1 = null;
            Harvest.GlobalFunctions.writeStandReport();

            if (itr * Landis.Extension.Succession.Landispro.PlugIn.freq[3] <= Landis.Extension.Succession.Landispro.PlugIn.gl_param.Num_Iteration)
            {
                str_htyp1 = string.Format("{0}/{1}/htyp{2:D}.img", Landis.Extension.Succession.Landispro.PlugIn.gl_param.OutputDir, "Harvest", itr * Landis.Extension.Succession.Landispro.PlugIn.freq[3] * Landis.Extension.Succession.Landispro.PlugIn.gl_sites.TimeStepHarvest); //*
                str_dec1 = string.Format("{0}/{1}/hdec{2:D}.img", Landis.Extension.Succession.Landispro.PlugIn.gl_param.OutputDir, "Harvest", itr * Landis.Extension.Succession.Landispro.PlugIn.freq[3] * Landis.Extension.Succession.Landispro.PlugIn.gl_sites.TimeStepHarvest); //*
                Harvest.GlobalFunctions.output_harvest_Dec_Type(itr, str_htyp, str_htyp1, str_dec, str_dec1, wAdfGeoTransform); //*
                for (int i = 0; i < Landis.Extension.Succession.Landispro.PlugIn.gl_sites.SpecNum; i++)
                {
                    str1 = string.Format("{0}/{1}/{2}_BACut{3:D}.img", Landis.Extension.Succession.Landispro.PlugIn.gl_param.OutputDir, "Harvest", Landis.Extension.Succession.Landispro.PlugIn.gl_spe_Attrs[i + 1].Name, itr * Landis.Extension.Succession.Landispro.PlugIn.freq[3] * Landis.Extension.Succession.Landispro.PlugIn.gl_sites.TimeStepHarvest); //*
                    Harvest.GlobalFunctions.PutOutput_harvestBACut_spec(str, str1, i, wAdfGeoTransform); //*
                }
                str1 = string.Format("{0}/{1}/BACut{2:D}.img", Landis.Extension.Succession.Landispro.PlugIn.gl_param.OutputDir, "Harvest", itr * Landis.Extension.Succession.Landispro.PlugIn.freq[3] * Landis.Extension.Succession.Landispro.PlugIn.gl_sites.TimeStepHarvest); //*
                Harvest.GlobalFunctions.PutOutput_harvestBACut(str, str1, wAdfGeoTransform);
            }
            if (Landis.Extension.Succession.Landispro.PlugIn.gl_param.Num_Iteration % Landis.Extension.Succession.Landispro.PlugIn.freq[3] != 0)
            {
                str_htyp1 = string.Format("{0}/{1}/htyp{2:D}.img", Landis.Extension.Succession.Landispro.PlugIn.gl_param.OutputDir, "Harvest", Landis.Extension.Succession.Landispro.PlugIn.gl_param.Num_Iteration * Landis.Extension.Succession.Landispro.PlugIn.gl_sites.TimeStepHarvest); //*
                str_dec1 = string.Format("{0}/{1}/hdec{2:D}.img", Landis.Extension.Succession.Landispro.PlugIn.gl_param.OutputDir, "Harvest", Landis.Extension.Succession.Landispro.PlugIn.gl_param.Num_Iteration * Landis.Extension.Succession.Landispro.PlugIn.gl_sites.TimeStepHarvest); //*
                Harvest.GlobalFunctions.output_harvest_Dec_Type(itr, str_htyp, str_htyp1, str_dec, str_dec1, wAdfGeoTransform); //*
                for (int i = 0; i < Landis.Extension.Succession.Landispro.PlugIn.gl_sites.SpecNum; i++)
                {
                    str1 = string.Format("{0}/{1}/{2}_BACut{3:D}.img", Landis.Extension.Succession.Landispro.PlugIn.gl_param.OutputDir, "Harvest", Landis.Extension.Succession.Landispro.PlugIn.gl_spe_Attrs[i + 1].Name, Landis.Extension.Succession.Landispro.PlugIn.gl_param.Num_Iteration * Landis.Extension.Succession.Landispro.PlugIn.gl_sites.TimeStepHarvest); //*
                    Harvest.GlobalFunctions.PutOutput_harvestBACut_spec(str, str1, i, wAdfGeoTransform); //*
                }
                str1 = string.Format("{0}/{1}/BACut{2:D}.img", Landis.Extension.Succession.Landispro.PlugIn.gl_param.OutputDir, "Harvest", Landis.Extension.Succession.Landispro.PlugIn.gl_param.Num_Iteration * Landis.Extension.Succession.Landispro.PlugIn.gl_sites.TimeStepHarvest); //*
                Harvest.GlobalFunctions.PutOutput_harvestBACut(str, str1, wAdfGeoTransform);
            }
        }
    }
}
