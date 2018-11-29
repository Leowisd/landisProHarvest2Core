﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Landis.Extension.Landispro.Harvest
{
    class HarvestEventQueue
    {
        int legendLabeNo;
        string[] legendLabe;
        List<HarvestEvent> itsEvents = new List<HarvestEvent>();

        public HarvestEventQueue()
        {
            legendLabeNo = 0;
        }

        public int Read(StreamReader inFile)
        {
            int id;
            int harvestEventMode = 0;
            HarvestEvent theEvent;
            int seqId = 0;

            string inString;
            string[] sarray;
            while (inFile.Peek() >= 0)
            {
                if ((inString = inFile.ReadLine()) == null)
                    throw new Exception("Error reading harvest event identifier from harvest section.");
                sarray = inString.Split('#');
                id = int.Parse(sarray[0]);

                switch (id)
                {
                    /*
                     * Accroding to the input data file HarvestparameterDebug.dat from Jacob, which used all possible functions, these
                     * methods may not be used in current version any longer.
                     */
                    //case HarvestEvent.EVENT_ONE_PASS_STAND_FILLING_REGIME: //may not used
                    //    theEvent = new OnePassStandFillingRegime();
                    //    harvestEventMode = harvestEventMode | (int)(float)Math.Pow(2.0f, (float)HarvestEvent.EVENT_ONE_PASS_STAND_FILLING_REGIME);
                    //    break;
                    //case HarvestEvent.EVENT_PERIODIC_STAND_FILLING_REGIME: // may not used
                    //    theEvent = new PeriodicStandFillingRegime();
                    //    harvestEventMode = harvestEventMode | (int)(float)Math.Pow(2.0f, (float)HarvestEvent.EVENT_PERIODIC_STAND_FILLING_REGIME);
                    //    break;
                    //case HarvestEvent.EVENT_TWO_PASS_STAND_FILLING_REGIME:  // may not used
                    //    theEvent = new TwoPassStandFillingRegime();
                    //    harvestEventMode = harvestEventMode | (int)(float)Math.Pow(2.0f, (float)HarvestEvent.EVENT_TWO_PASS_STAND_FILLING_REGIME);
                    //    break;
                    //case HarvestEvent.EVENT_ONE_PASS_STAND_SPREADING_REGIME:  // may not used
                    //    theEvent = new OnePassStandSpreadingRegime();
                    //    harvestEventMode = harvestEventMode | (int)(float)Math.Pow(2.0f, (float)HarvestEvent.EVENT_ONE_PASS_STAND_SPREADING_REGIME);
                    //    break;
                    //case HarvestEvent.EVENT_TWO_PASS_STAND_SPREADING_REGIME:  // may not used
                    //    theEvent = new TwoPassStandSpreadingRegime();
                    //    harvestEventMode = harvestEventMode | (int)(float)Math.Pow(2.0f, (float)HarvestEvent.EVENT_TWO_PASS_STAND_SPREADING_REGIME);
                    //    break;
                    //case HarvestEvent.EVENT_GROUP_SELECTION_REGIME:  // may not used
                    //    theEvent = new GroupSelectionRegime();
                    //    harvestEventMode = harvestEventMode | (int)(float)Math.Pow(2.0f, (float)HarvestEvent.EVENT_GROUP_SELECTION_REGIME);
                    //    break;
                    //case HarvestEvent.EVENT_PERIODIC_TWO_PASS_STAND_FILLING_REGIME:  // may not used
                    //    theEvent = new PeriodicTwoPassStandFillingRegime();
                    //    harvestEventMode = harvestEventMode | (int)(float)Math.Pow(2.0f, (float)HarvestEvent.EVENT_PERIODIC_TWO_PASS_STAND_FILLING_REGIME);
                    //    break;
                    //case HarvestEvent.EVENT_REPEATING_TWO_PASS_STAND_FILLING_REGIME:  // may not used
                    //    theEvent = new RepeatingTwoPassStandFillingRegime();
                    //    harvestEventMode = harvestEventMode | (int)(float)Math.Pow(2.0f, (float)HarvestEvent.EVENT_REPEATING_TWO_PASS_STAND_FILLING_REGIME);
                    //    break;
                    case HarvestEvent.EVENT_Volume_BA_THINING: //1
                        theEvent = new HarvestVolumeFittingRegime();
                        harvestEventMode = harvestEventMode | (int)(float)Math.Pow(2.0f, (float)HarvestEvent.EVENT_Volume_BA_THINING);
                        break;
                    case HarvestEvent.EVENT_GROUP_SELECTION_REGIME_70: //2
                        theEvent = new GroupSelectionRegime70();
                        harvestEventMode = harvestEventMode | (int)(float)Math.Pow(2.0f, (float)HarvestEvent.EVENT_GROUP_SELECTION_REGIME_70);
                        break;
                    case HarvestEvent.EVENT_STAND_STOCKING_HARVEST: //3
                        theEvent = new StockingCuttingRegime();
                        harvestEventMode = harvestEventMode | (int)(float)Math.Pow(2.0f, (float)HarvestEvent.EVENT_STAND_STOCKING_HARVEST);
                        break;

                    default:
                        throw new Exception("Error reading harvest event number.");
                }
                seqId++;
                theEvent.SetSequentialId(seqId);
                theEvent.Read(inFile); //HarvestRegime::read
                AddEvent(theEvent);
            }

            int j;
            legendLabe = new string[seqId + 1];
            if (legendLabe == null)
                throw new Exception("memory for LegendLabe not enough [seqId+1]");
            for (j = 0; j < seqId + 1; j++)
            {
                legendLabe[j] = "";
                if (legendLabe[j] == null)
                    throw new Exception("memory for LegendLabe not enough char [101]");
            }

            return harvestEventMode;
        }

        public void AddEvent(HarvestEvent someEvent)
        {
            itsEvents.Add(someEvent);
        }

        public void ProcessEvent(int itr)
        {
            HarvestEvent p;
            legendLabeNo = 0;
             
            foreach (HarvestEvent item in itsEvents)
            {
                p = item;
                legendLabe[p.GetSequentialId()] = p.GetLabel();
                legendLabeNo++;
                if (p.Conditions() == 1) //HarvestRegime::conditions()
                {
                    BoundedPocketStandHarvester.currentHarvestEventId++;

                    if (p.IsA() == HarvestEvent.EVENT_Volume_BA_THINING) //HarvestRegime::isA()
                    {
                        p.Harvest(); //HarvestRegime::harvest()
                    }
                    else if (p.IsA() == HarvestEvent.EVENT_GROUP_SELECTION_REGIME_70)
                    {
                        p.Harvest();
                    }
                    else if (p.IsA() == HarvestEvent.EVENT_STAND_STOCKING_HARVEST)
                    {
                        p.Harvest();
                    }
                }
            }
        }
    }
}
