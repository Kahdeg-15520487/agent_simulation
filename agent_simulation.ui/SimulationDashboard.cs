using AgentSimulation.Core;
using AgentSimulation.Scenarios;
using AgentSimulation.Events;
using System.Text;

namespace AgentSimulation.UI
{
    public partial class SimulationDashboard : Form
    {
        private Simulation? simulation;
        private CancellationTokenSource? cancellationTokenSource;
        private System.Windows.Forms.Timer? stepTimer;
        private bool isStepInProgress = false;

        // UI Controls
        private Button btnStart;
        private Button btnStop;
        private Button btnPause;
        private Button btnStep;
        private ProgressBar progressBarSimulation;
        private ProgressBar progressBarLifeSupport;
        private ListBox listBoxLog;
        private ListView listViewTasks;
        private Label lblSimulationStatus;
        private Label lblStepInfo;
        private Label lblLifeSupportInfo;
        private Panel pnlUserInput;
        private Label lblUserPrompt;
        private ListBox listBoxUserOptions;
        private Button btnSubmitUserInput;
        private CheckBox chkAutoStep;

        public SimulationDashboard()
        {
            InitializeComponent();
            SetupEventHandlers();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form setup
            this.Text = "Agent Simulation Dashboard";
            this.Size = new Size(1200, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Control buttons
            btnStart = new Button { Text = "Start Simulation", Location = new Point(10, 10), Size = new Size(120, 30) };
            btnStop = new Button { Text = "Stop", Location = new Point(140, 10), Size = new Size(80, 30), Enabled = false };
            btnPause = new Button { Text = "Pause", Location = new Point(230, 10), Size = new Size(80, 30), Enabled = false };
            btnStep = new Button { Text = "Step", Location = new Point(320, 10), Size = new Size(80, 30), Enabled = false };
            chkAutoStep = new CheckBox { Text = "Auto Step (1s)", Location = new Point(410, 15), Size = new Size(120, 20) };

            // Status labels
            lblSimulationStatus = new Label { Text = "Ready", Location = new Point(10, 50), Size = new Size(200, 20), ForeColor = Color.Blue };
            lblStepInfo = new Label { Text = "Step: 0/50", Location = new Point(220, 50), Size = new Size(100, 20) };
            lblLifeSupportInfo = new Label { Text = "Life Support: 100", Location = new Point(330, 50), Size = new Size(150, 20) };

            // Progress bars
            progressBarSimulation = new ProgressBar { Location = new Point(10, 80), Size = new Size(300, 20) };
            progressBarLifeSupport = new ProgressBar { Location = new Point(320, 80), Size = new Size(200, 20), Maximum = 100 };

            // Tasks list view
            listViewTasks = new ListView
            {
                Location = new Point(10, 110),
                Size = new Size(500, 200),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            listViewTasks.Columns.Add("Task", 150);
            listViewTasks.Columns.Add("Progress", 80);
            listViewTasks.Columns.Add("Required", 80);
            listViewTasks.Columns.Add("Status", 80);
            listViewTasks.Columns.Add("Type", 100);

            // Log list box
            listBoxLog = new ListBox
            {
                Location = new Point(520, 110),
                Size = new Size(660, 500),
                ScrollAlwaysVisible = true,
                HorizontalScrollbar = true
            };

            // User input panel (initially hidden)
            pnlUserInput = new Panel
            {
                Location = new Point(10, 320),
                Size = new Size(500, 190),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                BackColor = Color.LightYellow
            };

            lblUserPrompt = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(480, 60),
                Text = "User input required:",
                AutoSize = false,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            listBoxUserOptions = new ListBox
            {
                Location = new Point(10, 75),
                Size = new Size(480, 80)
            };

            btnSubmitUserInput = new Button
            {
                Text = "Submit",
                Location = new Point(410, 160),
                Size = new Size(80, 25),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            pnlUserInput.Controls.Add(lblUserPrompt);
            pnlUserInput.Controls.Add(listBoxUserOptions);
            pnlUserInput.Controls.Add(btnSubmitUserInput);

            // Add all controls to form
            this.Controls.AddRange(new Control[] {
                btnStart, btnStop, btnPause, btnStep, chkAutoStep,
                lblSimulationStatus, lblStepInfo, lblLifeSupportInfo,
                progressBarSimulation, progressBarLifeSupport,
                listViewTasks, listBoxLog, pnlUserInput
            });

            this.ResumeLayout();
        }

        private void SetupEventHandlers()
        {
            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
            btnPause.Click += BtnPause_Click;
            btnStep.Click += BtnStep_Click;
            btnSubmitUserInput.Click += BtnSubmitUserInput_Click;
            chkAutoStep.CheckedChanged += ChkAutoStep_CheckedChanged;

            // Setup timer for auto-stepping
            stepTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            stepTimer.Tick += StepTimer_Tick;
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            if (simulation != null && simulation.IsRunning) return;

            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            var agents = new List<AgentSimulation.Agents.Agent>()
            {
                new AgentSimulation.Agents.Agent("Alice", "Brave"),
                new AgentSimulation.Agents.LLMAgent("Bob", "Cautious", endpoint: "http://localhost:8080"),
                new AgentSimulation.Agents.HumanAgent("Charlie")
            };
            simulation = new Simulation(
                ScenarioLibrary.GetCrashedSpaceshipScenario(), 
                agents,
                new ThreadSafeTextBoxWriter(listBoxLog), 
                "http://localhost:8080"
            );

            // Set the agents on the simulation
            simulation.Agents = agents;

            SubscribeToSimulationEvents();

            btnStart.Enabled = false;
            btnStop.Enabled = true;
            btnPause.Enabled = true;
            btnStep.Enabled = true;

            await Task.Run(() => simulation.Start(), cancellationTokenSource.Token);
            UpdateUI();
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            simulation?.Stop();
            stepTimer?.Stop();
            cancellationTokenSource?.Cancel();
            
            isStepInProgress = false;
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            btnPause.Enabled = false;
            btnStep.Enabled = false;
            chkAutoStep.Checked = false;
        }

        private void BtnPause_Click(object sender, EventArgs e)
        {
            if (simulation?.IsPaused == true)
            {
                simulation.Resume();
                btnPause.Text = "Pause";
                if (chkAutoStep.Checked) stepTimer?.Start();
            }
            else
            {
                simulation?.Pause();
                btnPause.Text = "Resume";
                stepTimer?.Stop();
            }
        }

        private async void BtnStep_Click(object sender, EventArgs e)
        {
            if (simulation != null && cancellationTokenSource != null && !cancellationTokenSource.Token.IsCancellationRequested && !isStepInProgress)
            {
                isStepInProgress = true;
                btnStep.Enabled = false;
                
                try
                {
                    await Task.Run(() => simulation.ExecuteStep(), cancellationTokenSource.Token);
                }
                finally
                {
                    isStepInProgress = false;
                    btnStep.Enabled = simulation.IsRunning && !simulation.IsCompleted;
                }
            }
        }

        private void ChkAutoStep_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoStep.Checked && simulation?.IsRunning == true && simulation?.IsPaused == false)
            {
                stepTimer?.Start();
            }
            else
            {
                stepTimer?.Stop();
            }
        }

        private async void StepTimer_Tick(object? sender, EventArgs e)
        {
            if (simulation?.IsRunning == true && simulation?.IsPaused == false && !isStepInProgress)
            {
                isStepInProgress = true;
                btnStep.Enabled = false;
                
                try
                {
                    await Task.Run(() => simulation.ExecuteStep());
                }
                finally
                {
                    isStepInProgress = false;
                    btnStep.Enabled = simulation.IsRunning && !simulation.IsCompleted;
                }
            }
        }

        private UserInputRequestedEventArgs? currentUserInputRequest;

        private void BtnSubmitUserInput_Click(object sender, EventArgs e)
        {
            if (currentUserInputRequest != null && listBoxUserOptions.SelectedIndex >= 0)
            {
                var selectedOption = currentUserInputRequest.Options[listBoxUserOptions.SelectedIndex];
                
                // Log the user's choice
                AddLogMessage($"🎮 {currentUserInputRequest.RequestingAgent.Name} chose: {selectedOption}");
                
                // Send the result (0-based index)
                currentUserInputRequest.ResponseTask.SetResult(listBoxUserOptions.SelectedIndex);
                
                // Hide panel and resume simulation
                pnlUserInput.Visible = false;
                currentUserInputRequest = null;
                
                // Resume simulation if it was running
                if (simulation?.IsRunning == true && simulation.IsPaused)
                {
                    simulation.Resume();
                }
            }
        }

        private void SubscribeToSimulationEvents()
        {
            if (simulation == null) return;

            simulation.StepStarted += OnStepStarted;
            simulation.StepCompleted += OnStepCompleted;
            simulation.TaskProgressUpdated += OnTaskProgressUpdated;
            simulation.TaskCompleted += OnTaskCompleted;
            simulation.AgentPerformedAction += OnAgentPerformedAction;
            simulation.SimulationStarted += OnSimulationStarted;
            simulation.SimulationPaused += OnSimulationPaused;
            simulation.SimulationResumed += OnSimulationResumed;
            simulation.SimulationCompleted += OnSimulationCompleted;
            simulation.SimulationStopped += OnSimulationStopped;
            simulation.UserInputRequested += OnUserInputRequested;
            simulation.LogMessageGenerated += OnLogMessageGenerated;
        }

        private void OnStepStarted(object sender, SimulationStepEventArgs e)
        {
            this.Invoke(() =>
            {
                lblStepInfo.Text = $"Step: {e.StepNumber}/{e.TotalSteps}";
                progressBarSimulation.Maximum = e.TotalSteps;
                progressBarSimulation.Value = e.StepNumber;
            });
        }

        private void OnStepCompleted(object sender, SimulationStepEventArgs e)
        {
            this.Invoke(() =>
            {
                UpdateTasksList();
                UpdateLifeSupportDisplay();
            });
        }

        private void OnTaskProgressUpdated(object sender, TaskStatusEventArgs e)
        {
            this.Invoke(() =>
            {
                UpdateTasksList();
                if (e.JustCompleted)
                {
                    AddLogMessage($"✅ Task Completed: {e.Task.Name}");
                }
            });
        }

        private void OnTaskCompleted(object sender, TaskCompletedEventArgs e)
        {
            this.Invoke(() =>
            {
                var message = $"🎉 {e.Task.Name} completed!";
                if (e.CompletedBy != null)
                {
                    message += $" (by {e.CompletedBy.Name})";
                }
                AddLogMessage(message);
            });
        }

        private void OnAgentPerformedAction(object sender, AgentActionEventArgs e)
        {
            this.Invoke(() =>
            {
                var message = $"🤖 {e.Agent.Name}: {e.Action}";
                if (e.TargetTask != null)
                {
                    message += $" (Task: {e.TargetTask.Name})";
                }
                AddLogMessage(message);
            });
        }

        private void OnSimulationStarted(object sender, SimulationStateEventArgs e)
        {
            this.Invoke(() =>
            {
                lblSimulationStatus.Text = "Running";
                lblSimulationStatus.ForeColor = Color.Green;
                UpdateTasksList();
                UpdateLifeSupportDisplay();
            });
        }

        private void OnSimulationPaused(object sender, SimulationStateEventArgs e)
        {
            this.Invoke(() =>
            {
                lblSimulationStatus.Text = "Paused";
                lblSimulationStatus.ForeColor = Color.Orange;
            });
        }

        private void OnSimulationResumed(object sender, SimulationStateEventArgs e)
        {
            this.Invoke(() =>
            {
                lblSimulationStatus.Text = "Running";
                lblSimulationStatus.ForeColor = Color.Green;
            });
        }

        private void OnSimulationCompleted(object sender, SimulationStateEventArgs e)
        {
            this.Invoke(() =>
            {
                lblSimulationStatus.Text = e.IsRunning ? "Completed (Success)" : "Completed (Failed)";
                lblSimulationStatus.ForeColor = e.IsRunning ? Color.Blue : Color.Red;
                
                isStepInProgress = false;
                btnStart.Enabled = true;
                btnStop.Enabled = false;
                btnPause.Enabled = false;
                btnStep.Enabled = false;
                chkAutoStep.Checked = false;
                stepTimer?.Stop();
                
                MessageBox.Show(e.StatusMessage ?? "Simulation completed", "Simulation Result", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private void OnSimulationStopped(object sender, SimulationStateEventArgs e)
        {
            this.Invoke(() =>
            {
                lblSimulationStatus.Text = "Stopped";
                lblSimulationStatus.ForeColor = Color.Red;
                isStepInProgress = false;
            });
        }

        private void OnUserInputRequested(object sender, UserInputRequestedEventArgs e)
        {
            this.Invoke(() =>
            {
                currentUserInputRequest = e;
                
                // Format the prompt with agent name
                lblUserPrompt.Text = $"🎮 {e.RequestingAgent.Name}'s Turn\n{e.Prompt}";
                
                // Clear and populate options
                listBoxUserOptions.Items.Clear();
                for (int i = 0; i < e.Options.Count; i++)
                {
                    listBoxUserOptions.Items.Add($"{i + 1}. {e.Options[i]}");
                }
                
                // Auto-select first option
                if (listBoxUserOptions.Items.Count > 0)
                {
                    listBoxUserOptions.SelectedIndex = 0;
                }
                
                // Show the input panel and pause simulation
                simulation?.Pause();
                pnlUserInput.Visible = true;
                pnlUserInput.BringToFront();
                
                // Also add to log
                AddLogMessage($"🎮 Waiting for {e.RequestingAgent.Name} to make a decision...");
            });
        }

        private void OnLogMessageGenerated(object sender, SimulationLogEventArgs e)
        {
            this.Invoke(() =>
            {
                var prefix = e.Level switch
                {
                    LogLevel.Error => "❌ ",
                    LogLevel.Warning => "⚠️ ",
                    LogLevel.Info => "ℹ️ ",
                    LogLevel.Debug => "🔍 ",
                    _ => ""
                };
                AddLogMessage($"{prefix}{e.Message}");
            });
        }

        private void UpdateTasksList()
        {
            if (simulation == null) return;

            var taskStatuses = simulation.GetTaskStatuses();
            
            listViewTasks.Items.Clear();
            foreach (var task in taskStatuses)
            {
                var item = new ListViewItem(task.Name);
                item.SubItems.Add(task.Progress.ToString());
                item.SubItems.Add(task.RequiredProgress.ToString());
                item.SubItems.Add(task.IsCompleted ? "✅ Done" : $"{task.ProgressPercentage:F0}%");
                item.SubItems.Add(task.Type.ToString());
                
                if (task.IsCompleted)
                {
                    item.BackColor = Color.LightGreen;
                }
                else if (task.ProgressPercentage > 50)
                {
                    item.BackColor = Color.LightYellow;
                }
                
                listViewTasks.Items.Add(item);
            }
        }

        private void UpdateLifeSupportDisplay()
        {
            if (simulation == null) return;

            var status = simulation.GetSimulationStatus();
            lblLifeSupportInfo.Text = $"Life Support: {status.LifeSupport} (-{status.LifeSupportDecay}/step)";
            progressBarLifeSupport.Value = Math.Max(0, Math.Min(100, status.LifeSupport));
            
            if (status.LifeSupport < 20)
            {
                lblLifeSupportInfo.ForeColor = Color.Red;
                progressBarLifeSupport.ForeColor = Color.Red;
            }
            else if (status.LifeSupport < 50)
            {
                lblLifeSupportInfo.ForeColor = Color.Orange;
                progressBarLifeSupport.ForeColor = Color.Orange;
            }
            else
            {
                lblLifeSupportInfo.ForeColor = Color.Green;
                progressBarLifeSupport.ForeColor = Color.Green;
            }
        }

        private void UpdateUI()
        {
            UpdateTasksList();
            UpdateLifeSupportDisplay();
        }

        private void AddLogMessage(string message)
        {
            listBoxLog.Items.Add(message);
            AutoScrollLogToBottom();
        }

        private void AutoScrollLogToBottom()
        {
            if (listBoxLog.Items.Count > 0)
            {
                listBoxLog.TopIndex = listBoxLog.Items.Count - 1;
                listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            stepTimer?.Stop();
            stepTimer?.Dispose();
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
