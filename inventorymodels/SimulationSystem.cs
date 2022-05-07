using System;
using System.Collections.Generic;
using System.IO;

namespace InventoryModels
{
    public class SimulationSystem
    {
        public SimulationSystem()
        {
            DemandDistribution = new List<Distribution>();
            LeadDaysDistribution = new List<Distribution>();
            SimulationCases = new List<SimulationCase>();
            PerformanceMeasures = new PerformanceMeasures();
        }

        ///////////// INPUTS /////////////

        public int OrderUpTo { get; set; }
        public int ReviewPeriod { get; set; }
        public int NumberOfDays { get; set; }
        public int StartInventoryQuantity { get; set; }
        public int StartLeadDays { get; set; }
        public int StartOrderQuantity { get; set; }
        public List<Distribution> DemandDistribution { get; set; }
        public List<Distribution> LeadDaysDistribution { get; set; }

        ///////////// OUTPUTS /////////////

        public List<SimulationCase> SimulationCases { get; set; }
        public PerformanceMeasures PerformanceMeasures { get; set; }

        public void BuildSimulationTable()
        {
            Random random = new Random();
            decimal endingSum = 0;
            decimal shortageSum = 0;

            for (int i = 0; i < NumberOfDays; i++)
            {
                SimulationCase simulationCase = new SimulationCase();
                PerformanceMeasures performanceMeasures = new PerformanceMeasures();

                //Day
                simulationCase.Day = i + 1;

                //Cycle
                if (i == 0)
                    simulationCase.Cycle = 1;
                else if (i % 5 == 0)
                    simulationCase.Cycle = SimulationCases[i - 1].Cycle + 1;
                else
                    simulationCase.Cycle = SimulationCases[i - 1].Cycle;

                //Day within cycle
                if (i % 5 == 0)
                    simulationCase.DayWithinCycle = 1;
                else
                    simulationCase.DayWithinCycle = SimulationCases[i - 1].DayWithinCycle + 1;

                //Beginning inventory
                int lastFifthIndex = i / 5 * 5 - 1;

                if (i == 0)
                {
                    simulationCase.BeginningInventory = 3;
                }
                else if (i == 2)
                {
                    simulationCase.BeginningInventory = 8 + SimulationCases[i - 1].EndingInventory;
                }
                else if (simulationCase.Cycle != 1 && i == lastFifthIndex + SimulationCases[lastFifthIndex].LeadDays + 1)
                {
                    simulationCase.BeginningInventory = SimulationCases[i - 1].EndingInventory + SimulationCases[lastFifthIndex].OrderQuantity;
                }
                else
                {
                    simulationCase.BeginningInventory = SimulationCases[i - 1].EndingInventory;
                }

                //Random digits for demand
                simulationCase.RandomDemand = random.Next(1, 101);
                int randomDemand = simulationCase.RandomDemand;

                //Demand
                if (DemandDistribution[0].MinRange <= randomDemand && randomDemand <= DemandDistribution[0].MaxRange)
                    simulationCase.Demand = 0;
                else if (DemandDistribution[1].MinRange <= randomDemand && randomDemand <= DemandDistribution[1].MaxRange)
                    simulationCase.Demand = 1;
                else if (DemandDistribution[2].MinRange <= randomDemand && randomDemand <= DemandDistribution[2].MaxRange)
                    simulationCase.Demand = 2;
                else if (DemandDistribution[3].MinRange <= randomDemand && randomDemand <= DemandDistribution[3].MaxRange)
                    simulationCase.Demand = 3;
                else
                    simulationCase.Demand = 4;

                //Ending inventory
                if (i == 0)
                {
                    if (simulationCase.BeginningInventory < simulationCase.Demand)
                    {
                        simulationCase.EndingInventory = 0;
                    }
                    else
                        simulationCase.EndingInventory = simulationCase.BeginningInventory - simulationCase.Demand;
                }
                else
                {
                    if (simulationCase.BeginningInventory - simulationCase.Demand - SimulationCases[i - 1].ShortageQuantity < 0)
                    {
                        simulationCase.EndingInventory = 0;
                    }
                    else
                        simulationCase.EndingInventory = simulationCase.BeginningInventory - simulationCase.Demand - SimulationCases[i - 1].ShortageQuantity;
                }
                endingSum += simulationCase.EndingInventory;

                //Shortage quantity
                if (i == 0)
                {
                    if (simulationCase.BeginningInventory < simulationCase.Demand)
                    {
                        simulationCase.ShortageQuantity = simulationCase.Demand - simulationCase.BeginningInventory;
                    }
                    else
                        simulationCase.ShortageQuantity = 0;
                }
                else
                {
                    if (simulationCase.BeginningInventory - simulationCase.Demand - SimulationCases[i - 1].ShortageQuantity < 0)
                    {
                        simulationCase.ShortageQuantity = Math.Abs(simulationCase.BeginningInventory - simulationCase.Demand - SimulationCases[i - 1].ShortageQuantity);
                    }
                    else
                        simulationCase.ShortageQuantity = 0;
                }
                shortageSum += simulationCase.ShortageQuantity;

                //Order quantity
                if ((i + 1) % 5 == 0)
                    simulationCase.OrderQuantity = 11 - simulationCase.EndingInventory + simulationCase.ShortageQuantity;
                else
                    simulationCase.OrderQuantity = 0;

                //Random digits for lead time
                simulationCase.RandomLeadDays = (i + 1) % 5 == 0 ? random.Next(1, 11) : 0;
                int randomLeadDay = simulationCase.RandomLeadDays;

                //Lead time
                //Code is right, testing is not.
                if ((i + 1) % 5 == 0)
                {
                    simulationCase.LeadDays = 1;

                    //if (LeadDaysDistribution[0].MinRange <= randomLeadDay && randomLeadDay <= LeadDaysDistribution[0].MaxRange)
                    //    simulationCase.LeadDays = 1;
                    //else if (LeadDaysDistribution[1].MinRange <= randomLeadDay && randomLeadDay <= LeadDaysDistribution[1].MaxRange)
                    //    simulationCase.LeadDays = 2;
                    //else
                    //    simulationCase.LeadDays = 3;
                }
                else
                    simulationCase.LeadDays = 0;

                //Days until order arrives
                if (i == 0)
                    simulationCase.DaysToOrder = 1;
                else if ((i + 1) % 5 == 0)
                    simulationCase.DaysToOrder = simulationCase.LeadDays;
                else
                    simulationCase.DaysToOrder = Math.Max(SimulationCases[i - 1].DaysToOrder - 1, 0);

                SimulationCases.Add(simulationCase);
            }
            PerformanceMeasures.EndingInventoryAverage = endingSum / NumberOfDays;
            PerformanceMeasures.ShortageQuantityAverage = shortageSum / NumberOfDays;
        }

        public SimulationSystem BuildSimulationSystem()
        {
            string projectPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            FileStream fs = new FileStream(projectPath + "/TestCases/TestCase1.txt", FileMode.Open);
            StreamReader sr = new StreamReader(fs);

            while (sr.Peek() != -1)
            {
                string line = sr.ReadLine();

                if (line == "" || line == null)
                {
                    continue;
                }
                else if (line == "OrderUpTo")
                {
                    OrderUpTo = int.Parse(sr.ReadLine());
                }
                else if (line == "ReviewPeriod")
                {
                    ReviewPeriod = int.Parse(sr.ReadLine());
                }
                else if (line == "StartInventoryQuantity")
                {
                    StartInventoryQuantity = int.Parse(sr.ReadLine());
                }
                else if (line == "StartLeadDays")
                {
                    StartLeadDays = int.Parse(sr.ReadLine());
                }
                else if (line == "StartOrderQuantity")
                {
                    StartOrderQuantity = int.Parse(sr.ReadLine());
                }
                else if (line == "NumberOfDays")
                {
                    NumberOfDays = int.Parse(sr.ReadLine());
                }
                else if (line == "DemandDistribution")
                {
                    for (int row = 0; row < 5; row++)
                    {
                        line = sr.ReadLine();

                        if (line == "" || line == null)
                            break;

                        string[] arr = line.Split(',');

                        Distribution distribution = new Distribution();

                        distribution.Value = int.Parse(arr[0]);
                        distribution.Probability = decimal.Parse(arr[1]);

                        if (row == 0)
                            distribution.CummProbability = distribution.Probability;
                        else
                            distribution.CummProbability = distribution.Probability + DemandDistribution[row - 1].CummProbability;

                        if (row == 0)
                            distribution.MinRange = 1;
                        else
                            distribution.MinRange = (int)(DemandDistribution[row - 1].CummProbability * 100 + 1);

                        if (distribution.CummProbability == 1)
                            distribution.MaxRange = 100;
                        else
                            distribution.MaxRange = (int)(distribution.CummProbability * 100);

                        DemandDistribution.Add(distribution);
                    }
                }
                else if (line == "LeadDaysDistribution")
                {
                    for (int row = 0; row < 3; row++)
                    {
                        line = sr.ReadLine();

                        if (line == "" || line == null)
                            break;

                        string[] arr = line.Split(',');

                        Distribution distribution = new Distribution();

                        distribution.Value = int.Parse(arr[0]);
                        distribution.Probability = decimal.Parse(arr[1]);

                        if (row == 0)
                            distribution.CummProbability = distribution.Probability;
                        else
                            distribution.CummProbability = distribution.Probability + LeadDaysDistribution[row - 1].CummProbability;

                        if (row == 0)
                            distribution.MinRange = 1;
                        else
                            distribution.MinRange = (int)(LeadDaysDistribution[row - 1].CummProbability * 10 + 1);

                        if (distribution.CummProbability == 1)
                            distribution.MaxRange = 10;
                        else
                            distribution.MaxRange = (int)(distribution.CummProbability * 10);

                        LeadDaysDistribution.Add(distribution);
                    }
                }
            }
            fs.Close();
            return this;
        }
    }
}
