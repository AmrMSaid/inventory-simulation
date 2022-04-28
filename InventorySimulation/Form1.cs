﻿using System;
using System.Windows.Forms;
using InventoryModels;
using InventoryTesting;

namespace InventorySimulation
{
    public partial class Form1 : Form
    {
        public static SimulationSystem simulationSystem = new SimulationSystem();

        public Form1()
        {
            InitializeComponent();
            simulationSystem = simulationSystem.BuildSimulationSystem();
            simulationSystem.BuildSimulationTable();
            dataGridView1.DataSource = simulationSystem.SimulationCases;
            string testingManager = TestingManager.Test(simulationSystem, Constants.FileNames.TestCase1);
            MessageBox.Show(testingManager);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
