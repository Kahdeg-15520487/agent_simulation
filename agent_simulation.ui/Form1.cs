using AgentSimulation.Core;
using AgentSimulation.Scenarios;
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

            // Start simulation on background thread
            await Task.Run(() => this.simulation.Start(), cancellationTokenSource.Token);
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
                listBox.Invoke(new Action(() => listBox.Items.Add("")));
            }
            else
            {
                listBox.Items.Add("");
            }
        }

        public override void WriteLine(string? value)
        {
            if (listBox.InvokeRequired)
            {
                listBox.Invoke(new Action(() => listBox.Items.Add(value ?? "")));
            }
            else
            {
                listBox.Items.Add(value ?? "");
            }
        }
    }
}
