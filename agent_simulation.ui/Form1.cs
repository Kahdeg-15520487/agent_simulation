using AgentSimulation.Core;
using AgentSimulation.Scenarios;
using AgentSimulation.Events;
using System.Text;

namespace agent_simulation.ui
{
    public partial class Form1 : Form
    {
        private Simulation? simulation;
        private CancellationTokenSource? cancellationTokenSource;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // Clean up background task when form is closed
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            base.OnFormClosed(e);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // Cancel any existing simulation
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            var agents = new List<AgentSimulation.Agents.Agent>()
            {
                new AgentSimulation.Agents.Agent("Alice", "Brave"),
                new AgentSimulation.Agents.LLMAgent("Bob","Cautius",endpoint:"http://localhost:8080"),
                new AgentSimulation.Agents.HumanAgent("Charlie",()=>1)
            };

            this.simulation = new Simulation(ScenarioLibrary.GetCrashedSpaceshipScenario(), agents, new ThreadSafeTextBoxWriter(this.listBox1), "http://localhost:8080");

            // Subscribe to simulation events for basic logging
            SubscribeToSimulationEvents();

            // Start simulation on background thread
            await Task.Run(() => this.simulation.Start(), cancellationTokenSource.Token);
        }

        private void SubscribeToSimulationEvents()
        {
            if (simulation == null) return;

            // Subscribe to key events
            simulation.TaskCompleted += (sender, e) =>
            {
                this.Invoke(() =>
                {
                    this.Text = $"Agent Simulation - Task Completed: {e.Task.Name}";
                });
            };

            simulation.SimulationCompleted += (sender, e) =>
            {
                this.Invoke(() =>
                {
                    this.Text = "Agent Simulation - COMPLETED";
                    MessageBox.Show(e.StatusMessage ?? "Simulation completed!", "Result");
                });
            };

            simulation.StepCompleted += (sender, e) =>
            {
                this.Invoke(() =>
                {
                    this.Text = $"Agent Simulation - Step {e.StepNumber}/{e.TotalSteps} ({e.ProgressPercentage:F0}%)";
                });
            };
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (this.simulation != null && cancellationTokenSource != null && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                await Task.Run(() => this.simulation.ExecuteStep(), cancellationTokenSource.Token);
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            var dashboard = new SimulationDashboard();
            dashboard.Show();
        }
    }

    public class ThreadSafeTextBoxWriter : TextWriter
    {
        private ListBox listBox;
        public ThreadSafeTextBoxWriter(ListBox listBox)
        {
            this.listBox = listBox;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine()
        {
            if (listBox.InvokeRequired)
            {
                listBox.Invoke(new Action(() => {
                    listBox.Items.Add("");
                    AutoScrollToBottom();
                }));
            }
            else
            {
                listBox.Items.Add("");
                AutoScrollToBottom();
            }
        }

        public override void WriteLine(string? value)
        {
            if (listBox.InvokeRequired)
            {
                listBox.Invoke(new Action(() => {
                    listBox.Items.Add(value ?? "");
                    AutoScrollToBottom();
                }));
            }
            else
            {
                listBox.Items.Add(value ?? "");
                AutoScrollToBottom();
            }
        }

        private void AutoScrollToBottom()
        {
            if (listBox.Items.Count > 0)
            {
                listBox.TopIndex = listBox.Items.Count - 1;
            }
        }
    }
}
