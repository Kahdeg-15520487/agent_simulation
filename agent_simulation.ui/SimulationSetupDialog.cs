using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AgentSimulation.Agents;
using AgentSimulation.Core;
using AgentSimulation.Scenarios;

namespace AgentSimulation.UI
{
    public partial class SimulationSetupDialog : Form
    {
        private ComboBox cmbScenarios;
        private ComboBox cmbTeamPresets;
        private ListBox lstTeamComposition;
        private Button btnAddHuman;
        private Button btnAddAI;
        private Button btnRemoveAgent;
        private Button btnStart;
        private Button btnCancel;
        private Label lblScenario;
        private Label lblTeam;
        private Label lblTeamComposition;
        private TextBox txtAgentName;
        private ComboBox cmbPersonality;
        private TextBox txtLLMApiUrl;
        private Label lblLLMApiUrl;
        private GroupBox grpScenario;
        private GroupBox grpTeam;
        private RichTextBox rtbScenarioDescription;

        public ScenarioDefinition? SelectedScenario { get; private set; }
        public List<Agent> TeamAgents { get; private set; } = new();
        public string LLMApiUrl => string.IsNullOrWhiteSpace(txtLLMApiUrl.Text) ? "http://localhost:5000" : txtLLMApiUrl.Text;
        public bool StartSimulation { get; private set; } = false;

        public SimulationSetupDialog()
        {
            InitializeComponent();
            LoadScenarios();
            LoadTeamPresets();
            LoadPersonalities();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form setup
            this.Text = "Simulation Setup";
            this.Size = new Size(650, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Scenario selection group
            grpScenario = new GroupBox
            {
                Text = "Scenario Selection",
                Location = new Point(10, 10),
                Size = new Size(610, 180)
            };

            lblScenario = new Label
            {
                Text = "Choose Scenario:",
                Location = new Point(10, 25),
                Size = new Size(100, 20)
            };

            cmbScenarios = new ComboBox
            {
                Location = new Point(10, 50),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            rtbScenarioDescription = new RichTextBox
            {
                Location = new Point(220, 25),
                Size = new Size(380, 145),
                ReadOnly = true,
                BackColor = SystemColors.Control
            };

            grpScenario.Controls.AddRange(new Control[] { lblScenario, cmbScenarios, rtbScenarioDescription });

            // Team building group
            grpTeam = new GroupBox
            {
                Text = "Team Building",
                Location = new Point(10, 200),
                Size = new Size(610, 280)
            };

            lblTeam = new Label
            {
                Text = "Team Preset:",
                Location = new Point(10, 25),
                Size = new Size(100, 20)
            };

            cmbTeamPresets = new ComboBox
            {
                Location = new Point(10, 50),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            lblTeamComposition = new Label
            {
                Text = "Team Composition:",
                Location = new Point(10, 85),
                Size = new Size(120, 20)
            };

            lstTeamComposition = new ListBox
            {
                Location = new Point(10, 110),
                Size = new Size(300, 120)
            };

            // Agent addition controls
            txtAgentName = new TextBox
            {
                Location = new Point(320, 110),
                Size = new Size(120, 25),
                PlaceholderText = "Agent Name"
            };

            cmbPersonality = new ComboBox
            {
                Location = new Point(320, 145),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            lblLLMApiUrl = new Label
            {
                Text = "LLM API URL:",
                Location = new Point(320, 175),
                Size = new Size(80, 20)
            };

            txtLLMApiUrl = new TextBox
            {
                Location = new Point(320, 200),
                Size = new Size(210, 25),
                Text = "http://localhost:5000"
            };

            btnAddHuman = new Button
            {
                Text = "Add Human",
                Location = new Point(450, 110),
                Size = new Size(80, 25)
            };

            btnAddAI = new Button
            {
                Text = "Add AI",
                Location = new Point(450, 145),
                Size = new Size(80, 25)
            };

            btnRemoveAgent = new Button
            {
                Text = "Remove",
                Location = new Point(320, 230),
                Size = new Size(80, 25)
            };

            grpTeam.Controls.AddRange(new Control[] { 
                lblTeam, cmbTeamPresets, lblTeamComposition, lstTeamComposition,
                txtAgentName, cmbPersonality, lblLLMApiUrl, txtLLMApiUrl, 
                btnAddHuman, btnAddAI, btnRemoveAgent
            });

            // Dialog buttons
            btnStart = new Button
            {
                Text = "Start Simulation",
                Location = new Point(450, 490),
                Size = new Size(120, 30),
                DialogResult = DialogResult.OK
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(320, 490),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };

            // Add all controls to form
            this.Controls.AddRange(new Control[] { grpScenario, grpTeam, btnStart, btnCancel });

            // Set up event handlers
            SetupEventHandlers();

            this.ResumeLayout();
        }

        private void SetupEventHandlers()
        {
            cmbScenarios.SelectedIndexChanged += CmbScenarios_SelectedIndexChanged;
            cmbTeamPresets.SelectedIndexChanged += CmbTeamPresets_SelectedIndexChanged;
            btnAddHuman.Click += BtnAddHuman_Click;
            btnAddAI.Click += BtnAddAI_Click;
            btnRemoveAgent.Click += BtnRemoveAgent_Click;
            btnStart.Click += BtnStart_Click;
            lstTeamComposition.SelectedIndexChanged += LstTeamComposition_SelectedIndexChanged;
            txtLLMApiUrl.TextChanged += TxtLLMApiUrl_TextChanged;
        }

        private void LoadScenarios()
        {
            var scenarios = new Dictionary<string, ScenarioDefinition>
            {
                { "Crashed Spaceship", ScenarioLibrary.GetCrashedSpaceshipScenario() },
                { "Zombie Apocalypse", ScenarioLibrary.GetZombieApocalypseScenario() }
            };

            cmbScenarios.Items.Clear();
            foreach (var scenario in scenarios)
            {
                cmbScenarios.Items.Add(scenario.Key);
                cmbScenarios.Tag = scenarios; // Store scenarios for later retrieval
            }

            if (cmbScenarios.Items.Count > 0)
            {
                cmbScenarios.SelectedIndex = 0;
            }
        }

        private void LoadTeamPresets()
        {
            cmbTeamPresets.Items.Clear();
            cmbTeamPresets.Items.Add("Custom");

            var teamPresets = TeamPresets.GetAllPresets();
            foreach (var preset in teamPresets.Values)
            {
                cmbTeamPresets.Items.Add(preset.Name);
            }

            cmbTeamPresets.SelectedIndex = 0;
        }

        private void LoadPersonalities()
        {
            cmbPersonality.Items.Clear();
            cmbPersonality.Items.AddRange(new[] { 
                "Analytical", "Creative", "Cautious", "Aggressive", "Diplomatic", 
                "Technical", "Leader", "Support" 
            });
            cmbPersonality.SelectedIndex = 0;
        }

        private void CmbScenarios_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbScenarios.SelectedItem != null && cmbScenarios.Tag is Dictionary<string, ScenarioDefinition> scenarios)
            {
                string selectedScenario = cmbScenarios.SelectedItem.ToString();
                if (scenarios.TryGetValue(selectedScenario, out ScenarioDefinition scenario))
                {
                    SelectedScenario = scenario;
                    rtbScenarioDescription.Text = $"{scenario.Name}\n\n{scenario.Description}\n\n";
                    
                    if (!string.IsNullOrEmpty(scenario.WinCondition))
                        rtbScenarioDescription.Text += $"Win Condition: {scenario.WinCondition}\n";
                    
                    if (!string.IsNullOrEmpty(scenario.LoseCondition))
                        rtbScenarioDescription.Text += $"Lose Condition: {scenario.LoseCondition}\n";
                    
                    rtbScenarioDescription.Text += $"\nInitial Life Support: {scenario.InitialLifeSupport}\n";
                    rtbScenarioDescription.Text += $"Life Support Decay: {scenario.LifeSupportDecay}/step\n";
                    rtbScenarioDescription.Text += $"Duration: {scenario.HoursPerStep} hours per step";
                }
            }
        }

        private void CmbTeamPresets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTeamPresets.SelectedItem == null) return;

            string presetName = cmbTeamPresets.SelectedItem.ToString();
            TeamAgents.Clear();

            if (presetName == "Custom")
            {
                // Start with a basic human player for custom teams
                if (TeamAgents.Count == 0)
                {
                    TeamAgents.Add(new HumanAgent("Player"));
                }
            }
            else
            {
                // Find the matching preset and create agents
                var teamPresets = TeamPresets.GetAllPresets();
                var matchingPreset = teamPresets.Values.FirstOrDefault(p => p.Name == presetName);
                
                if (matchingPreset != null)
                {
                    foreach (var agentPreset in matchingPreset.Agents)
                    {
                        string llmApiUrl = string.IsNullOrWhiteSpace(txtLLMApiUrl.Text) ? "http://localhost:5000" : txtLLMApiUrl.Text;
                        
                        Agent agent = agentPreset.Type switch
                        {
                            AgentType.Human => new HumanAgent(agentPreset.Name),
                            AgentType.LLM => new LLMAgent(agentPreset.Name, agentPreset.Personality, endpoint: llmApiUrl),
                            AgentType.BasicAI => new Agent(agentPreset.Name, agentPreset.Personality),
                            _ => new Agent(agentPreset.Name, agentPreset.Personality)
                        };
                        TeamAgents.Add(agent);
                    }
                }
            }

            UpdateTeamCompositionDisplay();
        }

        private void BtnAddHuman_Click(object sender, EventArgs e)
        {
            string name = string.IsNullOrWhiteSpace(txtAgentName.Text) ? $"Human {TeamAgents.Count + 1}" : txtAgentName.Text;

            TeamAgents.Add(new HumanAgent(name));
            UpdateTeamCompositionDisplay();
            txtAgentName.Clear();
        }

        private void BtnAddAI_Click(object sender, EventArgs e)
        {
            string name = string.IsNullOrWhiteSpace(txtAgentName.Text) ? $"AI {TeamAgents.Count + 1}" : txtAgentName.Text;
            string personality = cmbPersonality.SelectedItem?.ToString() ?? "Analytical";
            string llmApiUrl = string.IsNullOrWhiteSpace(txtLLMApiUrl.Text) ? "http://localhost:5000" : txtLLMApiUrl.Text;

            TeamAgents.Add(new LLMAgent(name, personality, endpoint: llmApiUrl));
            UpdateTeamCompositionDisplay();
            txtAgentName.Clear();
        }

        private void BtnRemoveAgent_Click(object sender, EventArgs e)
        {
            if (lstTeamComposition.SelectedIndex >= 0 && lstTeamComposition.SelectedIndex < TeamAgents.Count)
            {
                TeamAgents.RemoveAt(lstTeamComposition.SelectedIndex);
                UpdateTeamCompositionDisplay();
            }
        }

        private void LstTeamComposition_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRemoveAgent.Enabled = lstTeamComposition.SelectedIndex >= 0;
        }

        private void UpdateTeamCompositionDisplay()
        {
            lstTeamComposition.Items.Clear();
            for (int i = 0; i < TeamAgents.Count; i++)
            {
                var agent = TeamAgents[i];
                string agentType = agent switch
                {
                    HumanAgent => "🎮 Human",
                    LLMAgent => "🧠 AI",
                    _ => "🤖 Bot"
                };
                lstTeamComposition.Items.Add($"{i + 1}. {agent.Name} - {agentType} ({agent.Personality})");
            }

            // Enable/disable buttons based on team composition
            btnStart.Enabled = TeamAgents.Count > 0 && SelectedScenario != null;
            btnRemoveAgent.Enabled = lstTeamComposition.SelectedIndex >= 0;
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (TeamAgents.Count == 0)
            {
                MessageBox.Show("Please add at least one agent to the team.", "No Team Members", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (SelectedScenario == null)
            {
                MessageBox.Show("Please select a scenario.", "No Scenario Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StartSimulation = true;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void TxtLLMApiUrl_TextChanged(object sender, EventArgs e)
        {
            // Update the endpoint URL for all existing LLM agents
            string newUrl = string.IsNullOrWhiteSpace(txtLLMApiUrl.Text) ? "http://localhost:5000" : txtLLMApiUrl.Text;
            
            foreach (var agent in TeamAgents.OfType<LLMAgent>())
            {
                agent.Endpoint = newUrl;
            }
        }
    }
}
