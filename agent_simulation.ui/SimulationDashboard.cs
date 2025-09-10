using AgentSimulation.Agents;
using AgentSimulation.Core;
using AgentSimulation.Scenarios;
using AgentSimulation.Events;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        private Button btnStep;
        private ProgressBar progressBarSimulation;
        private ProgressBar progressBarLifeSupport;
        private RichTextBox richTextBoxLog;
        private ListView listViewTasks;
        private ListView listViewAgents; // New agent status display
        private Label lblSimulationStatus;
        private Label lblStepInfo;
        private Label lblLifeSupportInfo;
        private Panel pnlUserInput;
        private Label lblUserPrompt;
        private ListBox listBoxUserOptions;
        private Button btnSubmitUserInput;
        private CheckBox chkAutoStep;
        private ListView listViewEffects;

        public SimulationDashboard()
        {
            InitializeComponent();
            SetupEventHandlers();
            
            // Enable keyboard event handling
            this.KeyPreview = true;
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
            btnStep = new Button { Text = "Step", Location = new Point(230, 10), Size = new Size(80, 30), Enabled = false };
            chkAutoStep = new CheckBox { Text = "Auto Step (1s)", Location = new Point(320, 15), Size = new Size(120, 20) };

            // Status labels
            lblSimulationStatus = new Label { Text = "Ready", Location = new Point(10, 50), Size = new Size(200, 20), ForeColor = Color.Blue };
            lblStepInfo = new Label { Text = "Step: 0/50", Location = new Point(220, 50), Size = new Size(100, 20) };
            lblLifeSupportInfo = new Label { Text = "Life Support: 100", Location = new Point(330, 50), Size = new Size(150, 20) };

            // Progress bars
            progressBarSimulation = new ProgressBar { Location = new Point(10, 80), Size = new Size(280, 20) };
            progressBarLifeSupport = new ProgressBar { Location = new Point(300, 80), Size = new Size(200, 20), Maximum = 100 };

            // Tasks list view
            listViewTasks = new ListView
            {
                Location = new Point(10, 110),
                Size = new Size(500, 150),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            listViewTasks.Columns.Add("Task", 150);
            listViewTasks.Columns.Add("Progress", 80);
            listViewTasks.Columns.Add("Required", 80);
            listViewTasks.Columns.Add("Status", 80);
            listViewTasks.Columns.Add("Type", 100);

            // Effects list view
            listViewEffects = new ListView
            {
                Location = new Point(10, 270),
                Size = new Size(500, 120),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            listViewEffects.Columns.Add("Effect", 200);
            listViewEffects.Columns.Add("Type", 60);
            listViewEffects.Columns.Add("Target", 120);
            listViewEffects.Columns.Add("Duration", 80);

            // Agent status list view
            listViewAgents = new ListView
            {
                Location = new Point(520, 10),
                Size = new Size(660, 90),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            listViewAgents.Columns.Add("Agent", 120);
            listViewAgents.Columns.Add("Stamina", 70);
            listViewAgents.Columns.Add("Food", 70);
            listViewAgents.Columns.Add("Rest", 70);
            listViewAgents.Columns.Add("Status", 120);
            listViewAgents.Columns.Add("Activity", 150);

            // Log rich text box for colorful output
            richTextBoxLog = new RichTextBox
            {
                Location = new Point(520, 110),
                Size = new Size(660, 500),
                ScrollBars = RichTextBoxScrollBars.Vertical,
                ReadOnly = true,
                BackColor = Color.Black,
                Font = new Font("Consolas", 9),
                WordWrap = true
            };

            // User input panel (initially hidden)
            pnlUserInput = new Panel
            {
                Location = new Point(10, 400),
                Size = new Size(500, 200),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                BackColor = Color.LightYellow
            };

            lblUserPrompt = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(480, 40),
                Text = "User input required:",
                AutoSize = false,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            listBoxUserOptions = new ListBox
            {
                Location = new Point(10, 55),
                Size = new Size(480, 100)
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
                btnStart, btnStop, btnStep, chkAutoStep,
                lblSimulationStatus, lblStepInfo, lblLifeSupportInfo,
                progressBarSimulation, progressBarLifeSupport,
                listViewTasks, listViewEffects, listViewAgents, richTextBoxLog, pnlUserInput
            });

            this.ResumeLayout();
        }

        private void SetupEventHandlers()
        {
            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
            btnStep.Click += BtnStep_Click;
            btnSubmitUserInput.Click += BtnSubmitUserInput_Click;
            chkAutoStep.CheckedChanged += ChkAutoStep_CheckedChanged;

            // Add keyboard event handler for shortcuts
            this.KeyDown += SimulationDashboard_KeyDown;

            // Setup timer for auto-stepping
            stepTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            stepTimer.Tick += StepTimer_Tick;
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            if (simulation != null && simulation.IsRunning) return;

            // Show the simulation setup dialog
            using (var setupDialog = new SimulationSetupDialog())
            {
                if (setupDialog.ShowDialog(this) != DialogResult.OK || !setupDialog.StartSimulation)
                {
                    return; // User cancelled or didn't start simulation
                }

                // Get the selected scenario and team from the dialog
                var selectedScenario = setupDialog.SelectedScenario;
                var teamAgents = setupDialog.TeamAgents;

                if (selectedScenario == null || teamAgents.Count == 0)
                {
                    MessageBox.Show("Invalid scenario or team configuration.", "Setup Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                cancellationTokenSource?.Cancel();
                cancellationTokenSource = new CancellationTokenSource();

                // Create simulation with selected scenario and team
                var logWriter = new ThreadSafeRichTextBoxWriter(richTextBoxLog, AddLogMessage);
                simulation = new Simulation(
                    selectedScenario, 
                    teamAgents,
                    logWriter
                );

                // Set the agents on the simulation
                simulation.Agents = teamAgents;

                SubscribeToSimulationEvents();

                btnStart.Enabled = false;
                btnStop.Enabled = true;
                btnStep.Enabled = true;

                await Task.Run(() => simulation.Start(), cancellationTokenSource.Token);
                UpdateUI();
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            simulation?.Stop();
            stepTimer?.Stop();
            cancellationTokenSource?.Cancel();
            
            isStepInProgress = false;
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            btnStep.Enabled = false;
            chkAutoStep.Checked = false;
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

        private void SimulationDashboard_KeyDown(object sender, KeyEventArgs e)
        {
            // Only handle keyboard shortcuts when user input panel is visible
            if (!pnlUserInput.Visible || currentUserInputRequest == null)
                return;

            // Handle number keys 1-9 for option selection
            if (e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D9)
            {
                int optionIndex = (int)(e.KeyCode - Keys.D1); // Convert to 0-based index
                
                // Check if the option index is valid
                if (optionIndex < listBoxUserOptions.Items.Count)
                {
                    listBoxUserOptions.SelectedIndex = optionIndex;
                    AddLogMessage($"🎮 Selected option {optionIndex + 1}: {currentUserInputRequest.Options[optionIndex]}");
                }
            }
            // Handle numpad keys 1-9 as well
            else if (e.KeyCode >= Keys.NumPad1 && e.KeyCode <= Keys.NumPad9)
            {
                int optionIndex = (int)(e.KeyCode - Keys.NumPad1); // Convert to 0-based index
                
                // Check if the option index is valid
                if (optionIndex < listBoxUserOptions.Items.Count)
                {
                    listBoxUserOptions.SelectedIndex = optionIndex;
                    AddLogMessage($"🎮 Selected option {optionIndex + 1}: {currentUserInputRequest.Options[optionIndex]}");
                }
            }
            // Handle Enter key to submit the selected option
            else if (e.KeyCode == Keys.Enter && listBoxUserOptions.SelectedIndex >= 0)
            {
                // Simulate clicking the submit button
                BtnSubmitUserInput_Click(btnSubmitUserInput, EventArgs.Empty);
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
                UpdateAgentsList();
                UpdateLifeSupportDisplay();
                UpdateEffectsList();
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
                // Determine success based on the status message
                bool isSuccess = e.StatusMessage != null && 
                               (e.StatusMessage.Contains("successfully") || e.StatusMessage.Contains("✅"));
                
                lblSimulationStatus.Text = isSuccess ? "Completed (Success)" : "Completed (Failed)";
                lblSimulationStatus.ForeColor = isSuccess ? Color.Green : Color.Red;
                
                isStepInProgress = false;
                btnStart.Enabled = true;
                btnStop.Enabled = false;
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
                
                // Format the prompt with agent name and keyboard shortcuts
                lblUserPrompt.Text = $"🎮 {e.RequestingAgent.Name}'s Turn\n{e.Prompt}\n\n⌨️ Shortcuts: Press 1-{e.Options.Count} to select, Enter to submit";
                
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
            // this.Invoke(() =>
            // {
            //     var prefix = e.Level switch
            //     {
            //         LogLevel.Error => "❌ ",
            //         LogLevel.Warning => "⚠️ ",
            //         LogLevel.Info => "ℹ️ ",
            //         LogLevel.Debug => "🔍 ",
            //         _ => ""
            //     };
            //     AddLogMessage($"{prefix}{e.Message}");
            // });
        }

        private void UpdateTasksList()
        {
            if (simulation == null) return;

            var taskStatuses = simulation.GetTaskStatuses();
            
            listViewTasks.Items.Clear();
            foreach (var task in taskStatuses)
            {
                var importantFlag = task.IsImportant ? "⭐ " : "";
                var item = new ListViewItem($"{importantFlag}{task.Name}");
                item.SubItems.Add(task.Progress.ToString());
                item.SubItems.Add(task.RequiredProgress.ToString());
                item.SubItems.Add(task.IsCompleted ? "✅ Done" : $"{task.ProgressPercentage:F0}%");
                item.SubItems.Add(task.Type.ToString());
                
                if (task.IsCompleted)
                {
                    item.BackColor = Color.LightGreen;
                }
                else if (task.IsImportant)
                {
                    item.BackColor = Color.LightCoral; // Highlight important tasks
                }
                else if (task.ProgressPercentage > 50)
                {
                    item.BackColor = Color.LightYellow;
                }
                
                listViewTasks.Items.Add(item);
            }
        }

        private void UpdateAgentsList()
        {
            if (simulation == null) return;

            listViewAgents.Items.Clear();

            foreach (var agent in simulation.Agents)
            {
                var item = new ListViewItem(agent.Name);
                item.SubItems.Add($"{agent.Stamina}%");
                item.SubItems.Add($"{agent.Food}%");
                item.SubItems.Add($"{agent.Rest}%");
                
                // Status indicator
                string status = "Healthy";
                if (agent.IsResting)
                    status = "Resting";
                else if (agent.IsEating)
                    status = "Eating";
                else if (agent.Stamina <= Agent.EXHAUSTED_THRESHOLD)
                    status = "Exhausted";
                else if (agent.Food <= Agent.HUNGRY_THRESHOLD)
                    status = "Hungry";
                else if (agent.Rest <= Agent.TIRED_THRESHOLD)
                    status = "Tired";
                    
                item.SubItems.Add(status);
                item.SubItems.Add(agent.CurrentThought ?? "");
                
                // Color coding based on status
                if (agent.IsResting || agent.IsEating)
                    item.BackColor = Color.LightBlue;
                else if (status == "Exhausted")
                    item.BackColor = Color.LightCoral;
                else if (status == "Hungry" || status == "Tired")
                    item.BackColor = Color.LightYellow;
                else
                    item.BackColor = Color.LightGreen;
                
                listViewAgents.Items.Add(item);
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

        private void UpdateEffectsList()
        {
            if (simulation == null) return;

            var effects = simulation.Scenario.EffectManager.GetEffectDisplayStrings();
            
            listViewEffects.Items.Clear();
            foreach (var effectStr in effects)
            {
                // Parse the effect display string to extract components
                var parts = effectStr.Split(' ', 4); // Split on first 4 spaces to get icon, name, target, and rest
                if (parts.Length >= 4)
                {
                    var typeIcon = parts[0]; // ↗️ or ↘️
                    var effectType = typeIcon == "↗️" ? "Buff" : "Debuff";
                    var effectName = parts[1];
                    
                    // Extract target and duration from the remaining parts
                    var remaining = string.Join(" ", parts.Skip(2));
                    var targetMatch = remaining.IndexOf('(');
                    var durationMatch = remaining.LastIndexOf('-');
                    
                    var target = targetMatch > 0 ? remaining.Substring(targetMatch + 1, remaining.IndexOf(')') - targetMatch - 1) : "Unknown";
                    var duration = durationMatch > 0 ? remaining.Substring(durationMatch + 1).Trim() : "Unknown";

                    var item = new ListViewItem($"{typeIcon} {effectName}");
                    item.SubItems.Add(effectType);
                    item.SubItems.Add(target);
                    item.SubItems.Add(duration);
                    
                    // Color code by type
                    if (effectType == "Buff")
                    {
                        item.BackColor = Color.LightGreen;
                    }
                    else
                    {
                        item.BackColor = Color.LightCoral;
                    }
                    
                    listViewEffects.Items.Add(item);
                }
            }
        }

        private void UpdateUI()
        {
            UpdateTasksList();
            UpdateAgentsList();
            UpdateLifeSupportDisplay();
            UpdateEffectsList();
        }

        private void AddLogMessage(string message)
        {
            AddLogMessage(message, GetMessageColor(message));
        }

        private void AddLogMessage(string message, Color color)
        {
            if (richTextBoxLog.InvokeRequired)
            {
                richTextBoxLog.Invoke(new Action(() => AppendColoredText(message, color)));
            }
            else
            {
                AppendColoredText(message, color);
            }
        }

        private Color GetMessageColor(string message)
        {
            // Determine color based on message content and prefixes
            if (message.Contains("�") || message.Contains("EVENT:") || message.Contains("⚡ EVENT"))
                return Color.Orange;
            if (message.Contains("�🔍") || message.Contains("💭") || message.Contains("thinks:") || message.Contains("Analyzing"))
                return Color.Cyan;
            if (message.Contains("🤖") || message.Contains("⚡") || message.Contains("🔧") || message.Contains("performs:") || message.Contains("works on"))
                return Color.Yellow;
            if (message.Contains("✅") || message.Contains("🎉") || message.Contains("completed") || message.Contains("Mission accomplished"))
                return Color.LightGreen;
            if (message.Contains("❌") || message.Contains("💀") || message.Contains("failed") || message.Contains("Mission failed"))
                return Color.Red;
            if (message.Contains("⚠️") || message.Contains("warning") || message.Contains("Life Support:") || message.Contains("critical"))
                return Color.Orange;
            if (message.Contains("🎮") || message.Contains("Human") || message.Contains("chose:") || message.Contains("Waiting for"))
                return Color.Magenta;
            if (message.Contains("ℹ️") || message.Contains("Step") || message.Contains("===") || message.Contains("SIMULATION"))
                return Color.LightBlue;
            
            return Color.White; // Default color
        }

        private void AppendColoredText(string text, Color color)
        {
            richTextBoxLog.SelectionStart = richTextBoxLog.TextLength;
            richTextBoxLog.SelectionLength = 0;
            richTextBoxLog.SelectionColor = color;
            richTextBoxLog.AppendText(text + Environment.NewLine);
            richTextBoxLog.SelectionColor = Color.White; // Reset to default
            
            // Auto-scroll to bottom
            richTextBoxLog.SelectionStart = richTextBoxLog.Text.Length;
            richTextBoxLog.ScrollToCaret();
        }

        private void AutoScrollLogToBottom()
        {
            // This method is now handled in AppendColoredText
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
