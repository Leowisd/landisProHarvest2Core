using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Landis.Core;
using Landis.Library.HarvestManagement;
using System.Diagnostics;
using Landis.SpatialModeling;


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
            Console.WriteLine("The datafile is:",dataFile);

            GlobalFunctions.HarvestPassInit(Landis.Extension.Succession.Landispro.PlugIn.gl_sites,
                                            Landis.Extension.Succession.Landispro.PlugIn.numSpecies,
                                            Landis.Extension.Succession.Landispro.PlugIn.gl_param.OutputDir, 
                                            dataFile,
                                            Landis.Extension.Succession.Landispro.PlugIn.pPDP);

            Console.WriteLine("Harvest Dll loaded in...");
            GlobalFunctions.HarvestPass(Landis.Extension.Succession.Landispro.PlugIn.gl_sites, Landis.Extension.Succession.Landispro.PlugIn.gl_spe_Attrs);
            Landis.Extension.Succession.Landispro.PlugIn.gl_sites.Harvest70outputdim();
            Console.WriteLine("Finish getting input");

        }

        public override void Initialize()
        {
            Landis.Extension.Succession.Landispro.PlugIn.gl_sites.stocking_x_value = Landis.Extension.Succession.Landispro.PlugIn.gl_param.Stocking_x_value;
            Landis.Extension.Succession.Landispro.PlugIn.gl_sites.stocking_y_value = Landis.Extension.Succession.Landispro.PlugIn.gl_param.Stocking_y_value;
            Landis.Extension.Succession.Landispro.PlugIn.gl_sites.stocking_z_value = Landis.Extension.Succession.Landispro.PlugIn.gl_param.Stocking_z_value;
            Landis.Extension.Succession.Landispro.PlugIn.gl_sites.SuccessionTimeStep = Landis.Extension.Succession.Landispro.PlugIn.gl_param.SuccessionTimestep;
            Landis.Extension.Succession.Landispro.PlugIn.gl_sites.TimeStepHarvest = Landis.Extension.Succession.Landispro.PlugIn.gl_param.TimeStepHarvest;

            Landis.Extension.Succession.Landispro.PlugIn.numSpecies = Landis.Extension.Succession.Landispro.PlugIn.gl_spe_Attrs.NumAttrs;

            Landis.Extension.Succession.Landispro.PlugIn.freq[5] = 1;
        }

        public override void Run()
        {
            int i = modelCore.TimeSinceStart;

            if (i % Landis.Extension.Succession.Landispro.PlugIn.gl_sites.TimeStepHarvest == 0)
            {
                Harvest.GlobalFunctions.HarvestPassCurrentDecade(i);
                for (int r = 1; r <= Landis.Extension.Succession.Landispro.PlugIn.snr; r++)
                {
                    for (int c = 1; c <= Landis.Extension.Succession.Landispro.PlugIn.snc; c++)
                    {
                        Harvest.GlobalFunctions.setUpdateFlags(r, c);
                    }
                }
            }
        }
    }
}
