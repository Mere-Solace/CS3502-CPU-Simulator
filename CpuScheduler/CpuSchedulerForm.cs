using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace CpuScheduler
{
    /// <summary>
    /// Main form for demonstrating CPU scheduling algorithms.
    /// </summary>
    public partial class CpuSchedulerForm : Form
    {
        private DataTable processTable;
        private Random random = new Random();
        private bool isDarkMode = true; // Default to dark mode

        // STUDENTS: Configure these limits based on your algorithm performance requirements
        private const int MIN_PROCESS_COUNT = 1;
        private const int MAX_PROCESS_COUNT = 100;
        private const int DEFAULT_PROCESS_COUNT = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="CpuSchedulerForm"/> class.
        /// </summary>
        public CpuSchedulerForm()
        {
            InitializeComponent();
            InitializeProcessTable();
        }

        /// <summary>
        /// Handles welcome page navigation.
        /// </summary>
        private void WelcomeButton_Click(object sender, EventArgs e)
        {
            ShowPanel(welcomePanel);
            sidePanel.Height = btnWelcome.Height;
            sidePanel.Top = btnWelcome.Top;
        }

        /// <summary>
        /// Handles results navigation.
        /// </summary>
        private void DashBoardButton_Click(object sender, EventArgs e)
        {
            ShowPanel(resultsPanel);
            sidePanel.Height = btnDashBoard.Height;
            sidePanel.Top = btnDashBoard.Top;
        }

        /// <summary>
        /// Navigates to the scheduler panel.
        /// </summary>
        private void CpuSchedulerButton_Click(object sender, EventArgs e)
        {
            ShowPanel(schedulerPanel);
            sidePanel.Height = btnCpuScheduler.Height;
            sidePanel.Top = btnCpuScheduler.Top;
        }

        /// <summary>
        /// Handles About page navigation.
        /// </summary>
        private void AboutButton_Click(object sender, EventArgs e)
        {
            ShowPanel(aboutPanel);
            sidePanel.Height = btnAbout.Height;
            sidePanel.Top = btnAbout.Top;
        }

        /// <summary>
        /// Toggles between dark and light mode themes.
        /// </summary>
        private void DarkModeToggle_Click(object sender, EventArgs e)
        {
            isDarkMode = !isDarkMode;
            ApplyTheme();
        }

        /// <summary>
        /// Shows the specified panel and hides all others.
        /// </summary>
        private void ShowPanel(Panel panelToShow)
        {
            welcomePanel.Visible = false;
            schedulerPanel.Visible = false;
            resultsPanel.Visible = false;
            aboutPanel.Visible = false;
            panelToShow.Visible = true;
            panelToShow.BringToFront();
        }

        /// <summary>
        /// Initializes the Welcome panel with introduction and navigation guide.
        /// </summary>
        private void InitializeWelcomeContent()
        {
            welcomeTextBox.Text = @"Welcome to CPU Scheduler Simulator

This educational tool helps CS 3502 students learn and experiment with CPU scheduling algorithms used in operating systems.

GETTING STARTED

Navigate using the sidebar buttons on the left:

🏠 WELCOME
This introduction page explaining the simulator and navigation.

⚙️ SCHEDULER
The main interface where you can:
• Enter the number of processes to simulate
• Choose from 4 scheduling algorithms:
  - FCFS (First Come, First Serve)
  - SJF (Shortest Job First)
  - Priority Scheduling
  - Round Robin
• Run simulations and see immediate feedback

📊 RESULTS
View detailed results from your last algorithm execution:
• Process execution details
• Algorithm-specific information
• Summary statistics
Results persist until you run a new simulation.

📚 ABOUT
Learn about the algorithms:
• How each algorithm works
• When to use each algorithm
• Learning objectives and concepts
• Algorithm characteristics and trade-offs

🔄 RESTART APPLICATION
Reset the simulator to its initial state.

HOW TO USE
1. Click 'Scheduler' to start
2. Enter number of processes (try 3-5 for learning)
3. Click an algorithm button to run simulation
4. View results in the 'Results' section
5. Learn more in the 'About' section
6. Experiment with different algorithms and process counts

Ready to start? Click 'Scheduler' to begin your CPU scheduling exploration!";
        }

        /// <summary>
        /// Initializes the About tab with educational content about CPU scheduling algorithms.
        /// </summary>
        private void InitializeAboutContent()
        {
            aboutTextBox.Text = @"CPU Scheduling Algorithms

This simulator demonstrates four fundamental CPU scheduling algorithms used in operating systems:

FIRST COME, FIRST SERVE (FCFS)
• Non-preemptive algorithm
• Processes are executed in the order they arrive
• Simple to implement but can lead to convoy effect
• Good for batch systems with long processes

SHORTEST JOB FIRST (SJF)
• Non-preemptive algorithm  
• Selects process with shortest burst time first
• Optimal for minimizing average waiting time
• Requires knowledge of process execution times

PRIORITY SCHEDULING
• Can be preemptive or non-preemptive
• Each process has a priority number
• CPU allocated to highest priority process
• May cause starvation of low-priority processes

ROUND ROBIN (RR)
• Preemptive algorithm using time quantum
• Each process gets equal CPU time slices
• Good for time-sharing systems
• Performance depends on quantum size

Learning Objectives:
• Understand how different algorithms handle process scheduling
• Compare algorithm performance and characteristics  
• Explore trade-offs between fairness and efficiency
• Learn when to use each algorithm type

Instructions:
1. Use the Scheduler tab to run algorithms
2. View execution results in the Results tab
3. Experiment with different process counts
4. Compare algorithm behaviors and outcomes";
        }

        /// <summary>
        /// STUDENTS: Helper method to get process data from the DataGrid
        /// Use this in your custom algorithm implementations instead of prompting users
        /// Returns: List of process data (ID, Burst Time, Priority, Arrival Time)
        /// </summary>
        public List<ProcessData> GetProcessDataFromGrid()
        {
            var processList = new List<ProcessData>();
            foreach (DataRow row in processTable.Rows)
            {
                processList.Add(new ProcessData
                {
                    ProcessID = row["Process ID"].ToString(),
                    BurstTime = Convert.ToInt32(row["Burst Time"]),
                    Priority = Convert.ToInt32(row["Priority"]),
                    ArrivalTime = Convert.ToInt32(row["Arrival Time"])
                });
            }
            return processList;
        }

        /// <summary>
        /// STUDENTS: Validates process count input with configurable limits
        /// Returns true if valid, false otherwise
        /// </summary>
        private bool IsValidProcessCount(string input, out int processCount)
        {
            if (int.TryParse(input, out processCount))
            {
                return processCount >= MIN_PROCESS_COUNT && processCount <= MAX_PROCESS_COUNT;
            }
            processCount = 0;
            return false;
        }

        /// <summary>
        /// STUDENTS: Displays scheduling results in a formatted table
        /// Use this method to show your algorithm results consistently
        /// </summary>
        private void DisplaySchedulingResults(List<SchedulingResult> results, string algorithmName)
        {
            listView1.Clear();
            listView1.View = View.Details;
            listView1.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            // Set up columns for detailed results
            listView1.Columns.Add("Process ID", 100, HorizontalAlignment.Left);
            listView1.Columns.Add("Arrival", 215, HorizontalAlignment.Left);
            listView1.Columns.Add("Burst", 175, HorizontalAlignment.Left);
            listView1.Columns.Add("Start", 240, HorizontalAlignment.Left);
            listView1.Columns.Add("Finish", 175, HorizontalAlignment.Left);
            listView1.Columns.Add("Waiting", 175, HorizontalAlignment.Left);
            listView1.Columns.Add("Turnaround", 300, HorizontalAlignment.Left);

            double AverageProcessesServedPerUnitTime = 0.0;
            double TimeIntervalSize = 0.0;
            // Add process results
            foreach (var result in results)
            {
                if (result.StartTime == -1) // skip overview entry
                {
                    AverageProcessesServedPerUnitTime = ((SchedulingOverview)result).AverageProcessesServedPerUnitTime;
                    TimeIntervalSize = ((SchedulingOverview)result).NumTimesSwitched;
                    continue;
                }
                var item = new ListViewItem(result.ProcessID);
                item.SubItems.Add(result.ArrivalTime.ToString());
                item.SubItems.Add(result.BurstTime.ToString());
                item.SubItems.Add(result.StartTime.ToString());
                item.SubItems.Add(result.FinishTime.ToString());
                item.SubItems.Add(result.WaitingTime.ToString());
                item.SubItems.Add(result.TurnaroundTime.ToString());
                listView1.Items.Add(item);
            }

            // Add summary statistics
            var avgWaiting = results.Average(r => r.WaitingTime);
            var avgTurnaround = results.Average(r => r.TurnaroundTime);
            var cpuUtilization = PerformanceMetrics.CalculateCPUUtilization(results, results.Max(r => r.FinishTime) - results.Min(r => r.ArrivalTime));
            var throughput = PerformanceMetrics.CalculateThroughput(results, results.Max(r => r.FinishTime) - results.Min(r => r.ArrivalTime));
            var avgResponse = PerformanceMetrics.CalculateAverageResponseTime(results);

            var summaryItem = new ListViewItem("SUMMARY");
            summaryItem.SubItems.Add(algorithmName);
            summaryItem.SubItems.Add($"{results.Count - 1} processes");
            summaryItem.SubItems.Add($"Avg Wait: {avgWaiting:F1}");
            summaryItem.SubItems.Add($"Avg Turn: {avgTurnaround:F1}");
            summaryItem.SubItems.Add($"CPU Utilization: {cpuUtilization:F1}%");
            summaryItem.SubItems.Add($"Throughput: {throughput:F4} procs/sec");
            listView1.Items.Add(summaryItem);

            var summaryItemExt = new ListViewItem("");
            summaryItemExt.SubItems.Add($"Avg Response Time: {avgResponse:F1}");
            summaryItemExt.SubItems.Add($"Context Switches: {results.Sum(r => r.NumTimesSwitched)}");
            summaryItemExt.SubItems.Add($"Distint Procs Served: {AverageProcessesServedPerUnitTime:F4} per >");
            summaryItemExt.SubItems.Add($"Sqrt of Total Time: {TimeIntervalSize} units");
            listView1.Items.Add(summaryItemExt);
        }

        /// <summary>
        /// Initializes the process data table structure.
        /// </summary>
        private void InitializeProcessTable()
        {
            processTable = new DataTable();
            processTable.Columns.Add("Process ID", typeof(string));
            processTable.Columns.Add("Burst Time", typeof(int));
            processTable.Columns.Add("Priority", typeof(int));
            processTable.Columns.Add("Arrival Time", typeof(int));

            processDataGrid.DataSource = processTable;
            processDataGrid.AllowUserToAddRows = false;
            processDataGrid.AllowUserToDeleteRows = false;

            // Set column widths and configure for larger datasets
            if (processDataGrid.Columns.Count > 0)
            {
                processDataGrid.Columns[0].Width = 100; // Process ID
                processDataGrid.Columns[1].Width = 100; // Burst Time
                processDataGrid.Columns[2].Width = 100; // Priority  
                processDataGrid.Columns[3].Width = 100; // Arrival Time

                // STUDENTS: Performance optimizations for larger datasets
                processDataGrid.VirtualMode = false; // Set to true if using 500+ processes
                processDataGrid.RowHeadersVisible = false; // Save space
                processDataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None; // Faster rendering

                // Automatically size rows to fit content so users can change row height by content
                processDataGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                // Allow cell content to wrap if it becomes long (helps automatic row height)
                processDataGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
                // Set a reasonable default row template height; AutoSizeRowsMode will override when needed
                processDataGrid.RowTemplate.Height = 22;
            }
        }

        /// <summary>
        /// Handles the Set Process Count button click.
        /// </summary>
        private void SetProcessCount_Click(object sender, EventArgs e)
        {
            // STUDENTS: Process count validation using helper method
            // Adjust MIN/MAX_PROCESS_COUNT constants above for your requirements
            if (IsValidProcessCount(txtProcess.Text, out int processCount))
            {
                // STUDENTS: Performance warning for large datasets
                if (processCount > 50)
                {
                    var result = MessageBox.Show(
                        $"You are creating {processCount} processes. This may impact performance.\n\nContinue?",
                        "Large Dataset Warning",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.No)
                    {
                        txtProcess.Focus();
                        return;
                    }
                }

                processTable.Clear();

                for (int i = 0; i < processCount; i++)
                {

                    // *** NOTE FOR LATER COME AND CHANGE LATER ***

                    DataRow row = processTable.NewRow();
                    row["Process ID"] = $"P{i + 1}";
                    row["Burst Time"] = random.Next(1, 11); // Default 1-10
                    row["Priority"] = i + 1; // Default priority
                    row["Arrival Time"] = 0; // Default arrival time
                    processTable.Rows.Add(row);
                }

                // Reset combo box selection
                cmbLoadExample.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show($"Please enter a valid number of processes ({MIN_PROCESS_COUNT}-{MAX_PROCESS_COUNT})",
                    "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProcess.Focus();
            }
        }

        /// <summary>
        /// Generates random data for the process table.
        /// </summary>
        private void GenerateRandom_Click(object sender, EventArgs e)
        {
            foreach (DataRow row in processTable.Rows)
            {
                row["Burst Time"] = random.Next(1, 21);
                row["Priority"] = random.Next(1, processTable.Rows.Count + 1);
                row["Arrival Time"] = random.Next(0, 10);
            }
        }

        /// <summary>
        /// Clears all process data and resets to default state.
        /// </summary>
        private void ClearAll_Click(object sender, EventArgs e)
        {
            processTable.Clear();
            txtProcess.Text = DEFAULT_PROCESS_COUNT.ToString();
            cmbLoadExample.SelectedIndex = 0;
            txtProcess.Focus();
        }

        /// <summary>
        /// Loads example process scenarios.
        /// </summary>
        private void LoadExample_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbLoadExample.SelectedIndex <= 0 || processTable.Rows.Count == 0)
                return;

            switch (cmbLoadExample.SelectedIndex)
            {
                case 1: // Short Processes
                    foreach (DataRow row in processTable.Rows)
                    {
                        row["Burst Time"] = random.Next(1, 6);
                        row["Priority"] = random.Next(1, 5);
                        row["Arrival Time"] = 0;
                    }
                    break;

                case 2: // Mixed Load
                    foreach (DataRow row in processTable.Rows)
                    {
                        row["Burst Time"] = random.Next(1, 21);
                        row["Priority"] = random.Next(1, 10);
                        row["Arrival Time"] = random.Next(0, 5);
                    }
                    break;

                case 3: // Heavy Load
                    foreach (DataRow row in processTable.Rows)
                    {
                        row["Burst Time"] = random.Next(10, 31);
                        row["Priority"] = random.Next(1, 5);
                        row["Arrival Time"] = random.Next(0, 10);
                    }
                    break;

                case 4: // Priority Demo
                    int priority = processTable.Rows.Count;
                    foreach (DataRow row in processTable.Rows)
                    {
                        row["Burst Time"] = random.Next(5, 15);
                        row["Priority"] = priority--;
                        row["Arrival Time"] = 0;
                    }
                    break;
            }

            cmbLoadExample.SelectedIndex = 0; // Reset dropdown
        }

        /// <summary>
        /// STUDENTS: Saves DataGrid data to CSV file for external editing or backup
        /// This allows you to prepare process data in Excel/CSV editors
        /// </summary>
        private void SaveData_Click(object sender, EventArgs e)
        {
            if (processTable.Rows.Count == 0)
            {
                MessageBox.Show("No process data to save. Please set process count first.",
                    "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveDialog.DefaultExt = "csv";
                saveDialog.FileName = "ProcessData.csv";
                saveDialog.Title = "Save Process Data";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var writer = new System.IO.StreamWriter(saveDialog.FileName))
                        {
                            // Write header
                            writer.WriteLine("Process ID,Burst Time,Priority,Arrival Time");

                            // Write data rows
                            foreach (DataRow row in processTable.Rows)
                            {
                                writer.WriteLine($"{row["Process ID"]},{row["Burst Time"]},{row["Priority"]},{row["Arrival Time"]}");
                            }
                        }

                        MessageBox.Show($"Process data saved successfully to:\n{saveDialog.FileName}",
                            "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving file: {ex.Message}",
                            "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// STUDENTS: Loads process data from CSV file for testing custom datasets
        /// This allows you to prepare complex test scenarios in Excel/CSV editors
        /// </summary>
        private void LoadData_Click(object sender, EventArgs e)
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                openDialog.DefaultExt = "csv";
                openDialog.Title = "Load Process Data from CSV";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var loadedData = new List<ProcessData>();
                        using (var reader = new System.IO.StreamReader(openDialog.FileName))
                        {
                            // Skip header line
                            var headerLine = reader.ReadLine();
                            if (headerLine == null)
                            {
                                MessageBox.Show("The CSV file is empty.", "Load Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            string line;
                            int lineNumber = 1;
                            while ((line = reader.ReadLine()) != null)
                            {
                                lineNumber++;
                                var parts = line.Split(',');

                                if (parts.Length != 4)
                                {
                                    MessageBox.Show($"Invalid format on line {lineNumber}. Expected format: ProcessID,BurstTime,Priority,ArrivalTime",
                                        "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                try
                                {
                                    loadedData.Add(new ProcessData
                                    {
                                        ProcessID = parts[0].Trim(),
                                        BurstTime = int.Parse(parts[1].Trim()),
                                        Priority = int.Parse(parts[2].Trim()),
                                        ArrivalTime = int.Parse(parts[3].Trim())
                                    });
                                }
                                catch (FormatException)
                                {
                                    MessageBox.Show($"Invalid number format on line {lineNumber}.",
                                        "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                        }

                        if (loadedData.Count == 0)
                        {
                            MessageBox.Show("No process data found in the CSV file.", "Load Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (loadedData.Count > MAX_PROCESS_COUNT)
                        {
                            MessageBox.Show($"CSV contains {loadedData.Count} processes, but maximum allowed is {MAX_PROCESS_COUNT}. Loading first {MAX_PROCESS_COUNT} processes.",
                                "Process Count Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            loadedData = loadedData.Take(MAX_PROCESS_COUNT).ToList();
                        }

                        // Clear existing data and load from CSV
                        processTable.Clear();
                        foreach (var process in loadedData)
                        {
                            DataRow row = processTable.NewRow();
                            row["Process ID"] = process.ProcessID;
                            row["Burst Time"] = process.BurstTime;
                            row["Priority"] = process.Priority;
                            row["Arrival Time"] = process.ArrivalTime;
                            processTable.Rows.Add(row);
                        }

                        // Update UI to reflect loaded data
                        txtProcess.Text = loadedData.Count.ToString();
                        cmbLoadExample.SelectedIndex = 0;

                        MessageBox.Show($"Successfully loaded {loadedData.Count} processes from:\n{openDialog.FileName}",
                            "Load Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading file: {ex.Message}",
                            "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        /// <summary>
        /// Executes the First-Come, First-Served algorithm using DataGrid data.
        /// STUDENTS: This demonstrates how to use GetProcessDataFromGrid() instead of prompts
        /// Use this pattern for your custom algorithm implementations
        /// </summary>
        private void FirstComeFirstServeButton_Click(object sender, EventArgs e)
        {
            var processData = GetProcessDataFromGrid();
            if (processData.Count > 0)
            {
                // STUDENTS: Example implementation using DataGrid data
                var results = CPUAlgorithms.RunFCFSAlgorithm(processData);

                // Update Results tab with detailed scheduling results
                DisplaySchedulingResults(results, "FCFS - First Come First Serve");

                // Switch to Results panel and update sidebar
                ShowPanel(resultsPanel);
                sidePanel.Height = btnDashBoard.Height;
                sidePanel.Top = btnDashBoard.Top;
            }
            else
            {
                MessageBox.Show("Please set process count and ensure the data grid has process data.",
                    "No Process Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProcess.Focus();
            }
        }

        /// <summary>
        /// Executes the Shortest Job First algorithm using DataGrid data.
        /// STUDENTS: Updated to use GetProcessDataFromGrid() instead of prompts
        /// Use this pattern for your custom algorithm implementations
        /// </summary>
        private void ShortestJobFirstButton_Click(object sender, EventArgs e)
        {
            var processData = GetProcessDataFromGrid();
            if (processData.Count > 0)
            {
                // STUDENTS: Updated implementation using DataGrid data
                var results = CPUAlgorithms.RunSJFAlgorithm(processData);

                // Update Results tab with detailed scheduling results
                DisplaySchedulingResults(results, "SJF - Shortest Job First");

                // Switch to Results panel and update sidebar
                ShowPanel(resultsPanel);
                sidePanel.Height = btnDashBoard.Height;
                sidePanel.Top = btnDashBoard.Top;
            }
            else
            {
                MessageBox.Show("Please set process count and ensure the data grid has process data.",
                    "No Process Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProcess.Focus();
            }
        }

        /// <summary>
        /// Executes the Priority algorithm using DataGrid data.
        /// STUDENTS: Updated to use GetProcessDataFromGrid() instead of prompts
        /// Higher priority numbers = higher priority (1=lowest, higher numbers=higher priority)
        /// </summary>
        private void PriorityButton_Click(object sender, EventArgs e)
        {
            var processData = GetProcessDataFromGrid();
            if (processData.Count > 0)
            {
                // STUDENTS: Updated implementation using DataGrid data
                var results = CPUAlgorithms.RunPriorityAlgorithm(processData);

                // Update Results tab with detailed scheduling results
                DisplaySchedulingResults(results, "Priority Scheduling (Higher # = Higher Priority)");

                // Switch to Results panel and update sidebar
                ShowPanel(resultsPanel);
                sidePanel.Height = btnDashBoard.Height;
                sidePanel.Top = btnDashBoard.Top;
            }
            else
            {
                MessageBox.Show("Please set process count and ensure the data grid has process data.",
                    "No Process Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProcess.Focus();
            }
        }

        /// <summary>
        /// Occurs when the process count text changes.
        /// </summary>
        private void ProcessTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Restarts the application.
        /// </summary>
        private void RestartApp_Click(object sender, EventArgs e)
        {
            Hide();
            CpuSchedulerForm cpuScheduler = new CpuSchedulerForm();
            cpuScheduler.ShowDialog();
        }



        /// <summary>
        /// STUDENTS: Applies rounded corners to a button for modern UI appearance
        /// Call this method for any custom buttons you add to maintain consistency
        /// </summary>
        private void ApplyRoundedCorners(Button button, int radius = 15)
        {
            GraphicsPath path = new GraphicsPath();
            Rectangle rect = new Rectangle(0, 0, button.Width - 1, button.Height - 1);

            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();

            button.Region = new Region(path);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
        }

        /// <summary>
        /// Handles form load logic.
        /// </summary>
        private void CpuSchedulerForm_Load(object sender, EventArgs e)
        {
            // Set default to Welcome panel
            sidePanel.Height = btnWelcome.Height;
            sidePanel.Top = btnWelcome.Top;
            listView1.View = View.Details;
            listView1.GridLines = true;

            // Initialize Results panel with placeholder message
            listView1.Clear();
            listView1.Columns.Add("Information", 500, HorizontalAlignment.Left);
            var welcomeItem = new ListViewItem("No results yet");
            welcomeItem.SubItems.Add("Run a scheduling algorithm to see results here");
            listView1.Items.Add(welcomeItem);

            // Initialize Welcome and About content
            InitializeWelcomeContent();
            InitializeAboutContent();

            // Load default process data for immediate use
            LoadDefaultProcessData();

            // Apply rounded corners to all buttons for modern UI
            ApplyRoundedCorners(btnSetProcessCount);
            ApplyRoundedCorners(btnGenerateRandom);
            ApplyRoundedCorners(btnClearAll);
            ApplyRoundedCorners(btnSaveData);
            ApplyRoundedCorners(btnLoadData);
            ApplyRoundedCorners(btnFCFS);
            ApplyRoundedCorners(btnSJF);
            ApplyRoundedCorners(btnPriority);
            ApplyRoundedCorners(btnRoundRobin);
            ApplyRoundedCorners(btnMLFQ);
            ApplyRoundedCorners(btnCFS);
            ApplyRoundedCorners(btnDarkModeToggle);

            // Apply default dark theme
            ApplyTheme();

            // Show Welcome panel by default
            ShowPanel(welcomePanel);
        }

        /// <summary>
        /// STUDENTS: Loads default process data when the application starts
        /// This provides immediate usability without requiring manual setup
        /// </summary>
        private void LoadDefaultProcessData()
        {
            // Populate the underlying DataTable with 5 default processes for immediate testing.
            // Do not index into the DataGridView.Rows collection directly. When the DataGridView is
            // data-bound to a DataTable, add/remove rows via the DataTable so the grid updates
            // and automatic row sizing works reliably.
            processTable.Clear();

            int[] burstTimes = new int[] { 6, 8, 7, 3, 4 };
            int[] arrivalTimes = new int[] { 0, 2, 4, 6, 8 };

            for (int i = 0; i < 5; i++)
            {
                DataRow row = processTable.NewRow();
                row["Process ID"] = $"P{i + 1}";
                row["Burst Time"] = burstTimes[i];
                row["Priority"] = i + 1;
                row["Arrival Time"] = arrivalTimes[i];
                processTable.Rows.Add(row);
            }

            // Ask the grid to recalculate row heights based on content
            processDataGrid.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells);

            // Set the process count text to match
            txtProcess.Text = "5";

            // Set combo box to default selection
            cmbLoadExample.SelectedIndex = 0;
        }

        /// <summary>
        /// STUDENTS: Applies dark or light theme to all UI elements
        /// This provides a modern interface that's easier on the eyes
        /// </summary>
        private void ApplyTheme()
        {
            if (isDarkMode)
            {
                ApplyDarkTheme();
                btnDarkModeToggle.Text = "☀️ Light Mode";
            }
            else
            {
                ApplyLightTheme();
                btnDarkModeToggle.Text = "🌙 Dark Mode";
            }
        }

        /// <summary>
        /// STUDENTS: Applies dark theme colors to all UI components
        /// </summary>
        private void ApplyDarkTheme()
        {
            // Main form background
            this.BackColor = Color.FromArgb(45, 45, 48);

            // Sidebar panel
            panel1.BackColor = Color.FromArgb(37, 37, 38);
            sidePanel.BackColor = Color.FromArgb(0, 122, 204); // Blue accent

            // All sidebar buttons
            ApplyDarkThemeToButton(btnWelcome);
            ApplyDarkThemeToButton(btnCpuScheduler);
            ApplyDarkThemeToButton(btnDashBoard);
            ApplyDarkThemeToButton(btnAbout);
            ApplyDarkThemeToButton(btnDarkModeToggle);

            // Restart label
            restartApp.BackColor = Color.FromArgb(37, 37, 38);
            restartApp.ForeColor = Color.FromArgb(241, 241, 241);

            // Copyright label
            label1.ForeColor = Color.FromArgb(153, 153, 153);

            // Content panels
            contentPanel.BackColor = Color.FromArgb(30, 30, 30);
            welcomePanel.BackColor = Color.FromArgb(30, 30, 30);
            schedulerPanel.BackColor = Color.FromArgb(30, 30, 30);
            resultsPanel.BackColor = Color.FromArgb(30, 30, 30);
            aboutPanel.BackColor = Color.FromArgb(30, 30, 30);

            // Text boxes
            welcomeTextBox.BackColor = Color.FromArgb(37, 37, 38);
            welcomeTextBox.ForeColor = Color.FromArgb(241, 241, 241);
            aboutTextBox.BackColor = Color.FromArgb(37, 37, 38);
            aboutTextBox.ForeColor = Color.FromArgb(241, 241, 241);

            // Process input controls
            labelProcess.ForeColor = Color.FromArgb(241, 241, 241);
            txtProcess.BackColor = Color.FromArgb(51, 51, 55);
            txtProcess.ForeColor = Color.FromArgb(241, 241, 241);

            // Data grid
            processDataGrid.BackgroundColor = Color.FromArgb(37, 37, 38);
            processDataGrid.DefaultCellStyle.BackColor = Color.FromArgb(51, 51, 55);
            processDataGrid.DefaultCellStyle.ForeColor = Color.FromArgb(241, 241, 241);
            processDataGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
            processDataGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(241, 241, 241);
            processDataGrid.GridColor = Color.FromArgb(62, 62, 66);

            // Combo box
            cmbLoadExample.BackColor = Color.FromArgb(51, 51, 55);
            cmbLoadExample.ForeColor = Color.FromArgb(241, 241, 241);

            // ListView (Results)
            listView1.BackColor = Color.FromArgb(37, 37, 38);
            listView1.ForeColor = Color.FromArgb(241, 241, 241);

            // All scheduler buttons with dark theme colors
            ApplyDarkThemeToSchedulerButton(btnSetProcessCount);
            ApplyDarkThemeToSchedulerButton(btnGenerateRandom);
            ApplyDarkThemeToSchedulerButton(btnClearAll);
            ApplyDarkThemeToSchedulerButton(btnSaveData);
            ApplyDarkThemeToSchedulerButton(btnLoadData);
            ApplyDarkThemeToSchedulerButton(btnFCFS);
            ApplyDarkThemeToSchedulerButton(btnSJF);
            ApplyDarkThemeToSchedulerButton(btnMLFQ);
            ApplyDarkThemeToSchedulerButton(btnCFS);
            ApplyDarkThemeToSchedulerButton(btnPriority);
            ApplyDarkThemeToSchedulerButton(btnRoundRobin);
        }

        /// <summary>
        /// STUDENTS: Applies light theme colors to all UI components
        /// </summary>
        private void ApplyLightTheme()
        {
            // Main form background
            this.BackColor = SystemColors.Control;

            // Sidebar panel
            panel1.BackColor = SystemColors.InactiveBorder;
            sidePanel.BackColor = Color.SeaGreen;

            // All sidebar buttons
            ApplyLightThemeToButton(btnWelcome);
            ApplyLightThemeToButton(btnCpuScheduler);
            ApplyLightThemeToButton(btnDashBoard);
            ApplyLightThemeToButton(btnAbout);
            ApplyLightThemeToButton(btnDarkModeToggle);

            // Restart label
            restartApp.BackColor = SystemColors.InactiveBorder;
            restartApp.ForeColor = Color.DarkBlue;

            // Copyright label
            label1.ForeColor = SystemColors.ControlText;

            // Content panels
            contentPanel.BackColor = SystemColors.Control;
            welcomePanel.BackColor = SystemColors.Control;
            schedulerPanel.BackColor = SystemColors.Control;
            resultsPanel.BackColor = SystemColors.Control;
            aboutPanel.BackColor = SystemColors.Control;

            // Text boxes
            welcomeTextBox.BackColor = SystemColors.Window;
            welcomeTextBox.ForeColor = SystemColors.WindowText;
            aboutTextBox.BackColor = SystemColors.Window;
            aboutTextBox.ForeColor = SystemColors.WindowText;

            // Process input controls
            labelProcess.ForeColor = SystemColors.ControlText;
            txtProcess.BackColor = SystemColors.Window;
            txtProcess.ForeColor = SystemColors.WindowText;

            // Data grid
            processDataGrid.BackgroundColor = SystemColors.Window;
            processDataGrid.DefaultCellStyle.BackColor = SystemColors.Window;
            processDataGrid.DefaultCellStyle.ForeColor = SystemColors.WindowText;
            processDataGrid.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
            processDataGrid.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;
            processDataGrid.GridColor = SystemColors.ControlDark;

            // Combo box
            cmbLoadExample.BackColor = SystemColors.Window;
            cmbLoadExample.ForeColor = SystemColors.WindowText;

            // ListView (Results)
            listView1.BackColor = SystemColors.Window;
            listView1.ForeColor = SystemColors.WindowText;

            // All scheduler buttons with original light colors
            ApplyLightThemeToSchedulerButton(btnSetProcessCount);
            ApplyLightThemeToSchedulerButton(btnGenerateRandom);
            ApplyLightThemeToSchedulerButton(btnClearAll);
            ApplyLightThemeToSchedulerButton(btnSaveData);
            ApplyLightThemeToSchedulerButton(btnLoadData);

            // Algorithm buttons with their original colors
            btnFCFS.BackColor = Color.Beige;
            btnSJF.BackColor = Color.AntiqueWhite;
            btnPriority.BackColor = Color.Bisque;
            btnRoundRobin.BackColor = Color.PapayaWhip;
            btnMLFQ.BackColor = Color.LimeGreen;
            btnCFS.BackColor = Color.AliceBlue;


            // Reset text color for algorithm buttons
            btnFCFS.ForeColor = SystemColors.ControlText;
            btnSJF.ForeColor = SystemColors.ControlText;
            btnPriority.ForeColor = SystemColors.ControlText;
            btnRoundRobin.ForeColor = SystemColors.ControlText;
            btnMLFQ.ForeColor = SystemColors.ControlText;
            btnCFS.ForeColor = SystemColors.ControlText;
        }

        /// <summary>
        /// STUDENTS: Helper method to apply dark theme to sidebar buttons
        /// </summary>
        private void ApplyDarkThemeToButton(Button button)
        {
            button.BackColor = Color.FromArgb(37, 37, 38);
            button.ForeColor = Color.FromArgb(241, 241, 241);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(62, 62, 66);
        }

        /// <summary>
        /// STUDENTS: Helper method to apply light theme to sidebar buttons
        /// </summary>
        private void ApplyLightThemeToButton(Button button)
        {
            button.BackColor = SystemColors.InactiveBorder;
            button.ForeColor = SystemColors.ControlText;
            button.FlatAppearance.MouseOverBackColor = SystemColors.ButtonHighlight;
        }

        /// <summary>
        /// STUDENTS: Helper method to apply dark theme to scheduler buttons
        /// </summary>
        private void ApplyDarkThemeToSchedulerButton(Button button)
        {
            button.BackColor = Color.FromArgb(51, 51, 55);
            button.ForeColor = Color.FromArgb(241, 241, 241);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 122, 204);
        }

        /// <summary>
        /// STUDENTS: Helper method to apply light theme to scheduler buttons
        /// </summary>
        private void ApplyLightThemeToSchedulerButton(Button button)
        {
            button.BackColor = SystemColors.ButtonFace;
            button.ForeColor = SystemColors.ControlText;
            button.FlatAppearance.MouseOverBackColor = Color.PaleGreen;
        }

        /// <summary>
        /// Executes the Round Robin algorithm using DataGrid data.
        /// STUDENTS: Updated to use GetProcessDataFromGrid() instead of prompts
        /// Each process gets a time quantum (default 4) before switching to next process
        /// </summary>
        private void RoundRobinButton_Click(object sender, EventArgs e)
        {
            var processData = GetProcessDataFromGrid();
            if (processData.Count > 0)
            {
                // Prompt for quantum time - this is algorithm-specific parameter
                string quantumInput = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter quantum time for Round Robin scheduling:",
                    "Quantum Time",
                    "4");

                if (int.TryParse(quantumInput, out int quantumTime) && quantumTime > 0)
                {
                    // STUDENTS: Updated implementation using DataGrid data
                    var results = CPUAlgorithms.RunRoundRobinAlgorithm(processData, quantumTime);

                    // Update Results tab with detailed scheduling results
                    DisplaySchedulingResults(results, $"Round Robin (Quantum = {quantumTime})");

                    // Switch to Results panel and update sidebar
                    ShowPanel(resultsPanel);
                    sidePanel.Height = btnDashBoard.Height;
                    sidePanel.Top = btnDashBoard.Top;
                }
                else
                {
                    MessageBox.Show("Please enter a valid quantum time (positive integer).",
                        "Invalid Quantum Time", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please set process count and ensure the data grid has process data.",
                    "No Process Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProcess.Focus();
            }
        }

        private void MultilevelFeedbackQueueButton_Click(object sender, EventArgs e)
        {
            var processData = GetProcessDataFromGrid();
            if (processData.Count > 0)
            {
                var results = CPUAlgorithms.RunMLFQAlgorithm(processData);

                DisplaySchedulingResults(results, "Multilevel Feedback Queue");

                ShowPanel(resultsPanel);
                sidePanel.Height = btnDashBoard.Height;
                sidePanel.Top = btnDashBoard.Top;
            }
            else
            {
                MessageBox.Show("Please set process count and ensure the data grid has process data.",
                    "No Process Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProcess.Focus();
            }
        }

        private void CompletelyFairSchedulerButton_Click(object sender, EventArgs e)
        {
            var processData = GetProcessDataFromGrid();
            if (processData.Count > 0)
            {
                var results = CPUAlgorithms.RunCFSAlgorithm(processData);

                DisplaySchedulingResults(results, "Completely Fair Scheduler - (Real-world Linux Scheduler)");

                ShowPanel(resultsPanel);
                sidePanel.Height = btnDashBoard.Height;
                sidePanel.Top = btnDashBoard.Top;
            }
        }

        private async void SaveAllResultsButton_Click(object sender, EventArgs e)
        {
            btnCompareAll.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                await Task.Run(() =>
                {
                    RunAllAlgorithmsAndSave();
                });

                MessageBox.Show("All process data saved successfully!", "Export Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnCompareAll.Enabled = true;
            }
        }

        private void RunAllAlgorithmsAndSave()
        {
            var allResults = new List<(string AlgorithmName, List<SchedulingResult> Results)>();

            for (int i = 0; i < 6; i++)
            {
                // run algorithm, collect results — DO NOT call DisplaySchedulingResults here!
                var data = GetProcessDataFromGrid();
                List<SchedulingResult> results = i switch
                {
                    0 => CPUAlgorithms.RunFCFSAlgorithm(data),
                    1 => CPUAlgorithms.RunSJFAlgorithm(data),
                    2 => CPUAlgorithms.RunPriorityAlgorithm(data),
                    3 => CPUAlgorithms.RunRoundRobinAlgorithm(data, 4),
                    4 => CPUAlgorithms.RunMLFQAlgorithm(data),
                    5 => CPUAlgorithms.RunCFSAlgorithm(data),
                    _ => null
                };
                string name = new[] {
                    "FCFS", "SJF", "Priority", "Round Robin", "MLFQ", "CFS"
                }[i];

                allResults.Add((name, results));
            }

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fileName = "AlgoData.csv";
            string path = Path.Combine(desktopPath, fileName);

            // Increment the filename if it already exists
            int count = 1;
            while (File.Exists(path))
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                string extension = Path.GetExtension(fileName);
                string newFileName = $"{fileNameWithoutExt}({count}){extension}";
                path = Path.Combine(desktopPath, newFileName);
                count++;
            }

            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("Algo Name,Avg Waiting,Avg Turnaround,Avg Response,CPU Util (%),Throughput,Context Switches,Distinct Procs Served,Interval Size");

                foreach (var (name, results) in allResults)
                {
                    if (results == null || results.Count == 0) continue;

                    int totalTime = Math.Max(1, results.Max(r => r.FinishTime) - results.Min(r => r.ArrivalTime));
                    var avgWaiting = results.Average(r => r.WaitingTime);
                    var avgTurnaround = results.Average(r => r.TurnaroundTime);
                    var avgResponse = PerformanceMetrics.CalculateAverageResponseTime(results);
                    var cpuUtil = PerformanceMetrics.CalculateCPUUtilization(results, totalTime);
                    var throughput = PerformanceMetrics.CalculateThroughput(results, totalTime);
                    var overview = results.OfType<SchedulingOverview>().FirstOrDefault();

                    writer.WriteLine($"{name},{avgWaiting:F1},{avgTurnaround:F1},{avgResponse:F1},{cpuUtil:F1},{throughput:F4},{results.Sum(r => r.NumTimesSwitched)},{overview?.AverageProcessesServedPerUnitTime:F4},{overview?.NumTimesSwitched}");
                }
            }
        }
    }

    
    /// <summary>
    /// STUDENTS: Custom button class with rounded edges for modern UI appearance
    /// You can use this for your custom algorithm buttons to maintain visual consistency
    /// </summary>
    public class RoundedButton : Button
    {
        private int borderRadius = 10;
        private Color borderColor = Color.FromArgb(200, 200, 200);

        public int BorderRadius
        {
            get { return borderRadius; }
            set { borderRadius = value; Invalidate(); }
        }

        public Color BorderColor
        {
            get { return borderColor; }
            set { borderColor = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Create rounded rectangle path
            GraphicsPath path = new GraphicsPath();
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            path.AddArc(rect.X, rect.Y, borderRadius, borderRadius, 180, 90);
            path.AddArc(rect.X + rect.Width - borderRadius, rect.Y, borderRadius, borderRadius, 270, 90);
            path.AddArc(rect.X + rect.Width - borderRadius, rect.Y + rect.Height - borderRadius, borderRadius, borderRadius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - borderRadius, borderRadius, borderRadius, 90, 90);
            path.CloseAllFigures();

            // Set button region to rounded shape
            Region = new Region(path);

            // Fill background
            using (SolidBrush brush = new SolidBrush(BackColor))
            {
                g.FillPath(brush, path);
            }

            // Draw border
            using (Pen pen = new Pen(borderColor, 1))
            {
                g.DrawPath(pen, path);
            }

            // Draw text
            TextRenderer.DrawText(g, Text, Font, ClientRectangle, ForeColor, 
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            path.Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}