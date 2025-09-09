using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AgentSimulation.Core;
using AgentSimulation.Scenarios;
using AgentSimulation.Agents;

namespace agent_simulation_ui;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Simulation? _currentSimulation;
    private DispatcherTimer _uiUpdateTimer = null!;
    private DispatcherTimer _simulationStepTimer = null!;
    private StringWriter _simulationOutputCapture = null!;
    private TextWriter _originalConsoleOut = null!;
    private bool _isPaused = false;

    public MainWindow()
    {
        InitializeComponent();
        InitializeUI();
        SetupUIUpdateTimer();
    }

    private void InitializeUI()
    {
        // Initialize scenario selection
        LoadScenarios();
        
        // Setup team size slider event
        TeamSizeSlider.ValueChanged += (s, e) =>
        {
            TeamSizeLabel.Text = $"{(int)TeamSizeSlider.Value} Agents";
        };

        // Setup simulation control buttons
        PauseButton.Click += (s, e) => PauseSimulation();
        ResumeButton.Click += (s, e) => ResumeSimulation();
        StopButton.Click += (s, e) => StopSimulation();

        // Initially disable simulation controls
        PauseButton.IsEnabled = false;
        ResumeButton.IsEnabled = false;
        StopButton.IsEnabled = false;

        StatusBarText.Text = "Select a scenario and configure your team to begin";
    }

    private void LoadScenarios()
    {
        try
        {
            var scenarios = ScenarioLibrary.GetAllScenarios();
            ScenarioComboBox.ItemsSource = scenarios;
            ScenarioComboBox.DisplayMemberPath = "Name";
            ScenarioComboBox.SelectedValuePath = "Name";
            
            if (scenarios.Any())
            {
                ScenarioComboBox.SelectedIndex = 0;
                UpdateScenarioDescription();
            }

            ScenarioComboBox.SelectionChanged += (s, e) => UpdateScenarioDescription();
        }
        catch (Exception ex)
        {
            ShowError($"Failed to load scenarios: {ex.Message}");
        }
    }

    private void UpdateScenarioDescription()
    {
        if (ScenarioComboBox.SelectedItem is ScenarioDefinition scenario)
        {
            ScenarioDescription.Text = scenario.Description ?? "No description available.";
        }
    }

    private void SetupUIUpdateTimer()
    {
        _uiUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500) // Update UI every 500ms
        };
        _uiUpdateTimer.Tick += UpdateSimulationUI;

        _simulationStepTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(2000) // Execute one step every 2 seconds
        };
        _simulationStepTimer.Tick += ExecuteSimulationStep;
    }

    private void StartSimulationButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ScenarioComboBox.SelectedItem is not ScenarioDefinition selectedScenario)
            {
                ShowError("Please select a scenario first.");
                return;
            }

            StatusBarText.Text = "Creating team and starting simulation...";
            
            // Show tips if requested
            if (ShowTipsCheckBox.IsChecked == true)
            {
                var result = MessageBox.Show(
                    "💡 GAMEPLAY TIPS:\n\n" +
                    "🏥 Higher life support = better task efficiency bonuses\n" +
                    "🛡️ Completing defense tasks reduces damage from attacks\n" +
                    "🔧 Maintenance tasks reduce life support decay\n" +
                    "📊 Check colony stats to see your current capabilities\n" +
                    "⚡ Choose effort levels wisely - higher effort = more progress\n" +
                    "🎯 Focus on tasks that complement your team's strengths\n" +
                    "🤝 Work together - some tasks benefit from previous completions\n\n" +
                    "Ready to start?",
                    "Gameplay Tips",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);
                
                if (result == MessageBoxResult.No)
                {
                    StatusBarText.Text = "Simulation cancelled by user";
                    return;
                }
            }

            // Create team with specified size
            var teamSize = (int)TeamSizeSlider.Value;
            var team = CreateTeam(teamSize);

            // Setup console output capture
            SetupConsoleCapture();

            // Create and start simulation
            _currentSimulation = new Simulation(selectedScenario, team, "http://localhost:8080");
            _currentSimulation.Start();
            
            // Switch to simulation tab
            SimulationTab.IsEnabled = true;
            SimulationTab.IsSelected = true;
            
            // Enable simulation controls
            StartSimulationButton.IsEnabled = false;
            PauseButton.IsEnabled = true;
            StopButton.IsEnabled = true;
            ResumeButton.IsEnabled = false;
            
            // Start timers
            _uiUpdateTimer.Start();
            _simulationStepTimer.Start();
            _isPaused = false;
            
            StatusBarText.Text = "Simulation running...";
        }
        catch (Exception ex)
        {
            ShowError($"Failed to start simulation: {ex.Message}");
            StatusBarText.Text = "Failed to start simulation";
        }
    }

    private List<Agent> CreateTeam(int teamSize)
    {
        var team = new List<Agent>();
        
        // Add one human agent
        team.Add(new HumanAgent("Player"));
        
        // Fill rest with LLM agents
        for (int i = 1; i < teamSize; i++)
        {
            team.Add(new LLMAgent($"AI-Agent-{i}", $"Helpful AI assistant #{i}"));
        }
        
        return team;
    }

    private void SetupConsoleCapture()
    {
        _originalConsoleOut = Console.Out;
        _simulationOutputCapture = new StringWriter();
        Console.SetOut(_simulationOutputCapture);
    }

    private void RestoreConsoleOutput()
    {
        if (_originalConsoleOut != null)
        {
            Console.SetOut(_originalConsoleOut);
        }
    }

    private void ExecuteSimulationStep(object? sender, EventArgs e)
    {
        if (_currentSimulation == null || _isPaused) return;

        try
        {
            // Execute one simulation step
            bool continueRunning = _currentSimulation.ExecuteStep();
            
            if (!continueRunning || _currentSimulation.IsCompleted)
            {
                // Simulation has ended
                StatusBarText.Text = _currentSimulation.Scenario.IsSuccessful ? 
                    "Simulation completed successfully!" : 
                    "Simulation ended";
                StopSimulation();
            }
        }
        catch (Exception ex)
        {
            ShowError($"Simulation step error: {ex.Message}");
            StopSimulation();
        }
    }

    private void UpdateSimulationUI(object? sender, EventArgs e)
    {
        if (_currentSimulation == null) return;

        try
        {
            // Update simulation output
            var output = _simulationOutputCapture?.ToString() ?? "";
            if (!string.IsNullOrEmpty(output))
            {
                // Limit output size to prevent memory issues
                if (output.Length > 50000) // Limit to ~50KB
                {
                    var lines = output.Split('\n');
                    if (lines.Length > 200) // Keep last 200 lines
                    {
                        output = string.Join('\n', lines.Skip(lines.Length - 200));
                        
                        // Clear the capture and restart with limited output
                        _simulationOutputCapture?.GetStringBuilder().Clear();
                        _simulationOutputCapture?.Write(output);
                    }
                }
                
                SimulationOutput.Text = output;
                
                // Auto-scroll to bottom
                if (SimulationOutput.Parent is ScrollViewer scrollViewer)
                {
                    scrollViewer.ScrollToEnd();
                }
            }

            // Update status indicators
            UpdateStatusDisplay();
        }
        catch (Exception ex)
        {
            // Handle any UI update errors silently to avoid crashing
            System.Diagnostics.Debug.WriteLine($"UI Update Error: {ex.Message}");
        }
    }

    private void UpdateStatusDisplay()
    {
        if (_currentSimulation == null) return;
        
        // Get real values from the simulation
        var lifeSupport = _currentSimulation.Scenario.LifeSupport;
        var maxLifeSupport = 100; // Assuming max is 100
        var lifeSupportPercent = Math.Max(0, Math.Min(100, (lifeSupport * 100) / maxLifeSupport));
        
        LifeSupportText.Text = $"{lifeSupportPercent:F0}%";
        
        // Calculate defense based on completed defense tasks
        var defenseTasks = _currentSimulation.Scenario.Tasks.Where(t => t.Type.Contains("Defense")).ToList();
        var completedDefense = defenseTasks.Count(t => t.IsCompleted);
        var defensePercent = defenseTasks.Count > 0 ? (completedDefense * 100) / defenseTasks.Count : 0;
        DefenseText.Text = $"{defensePercent}%";
        
        // Show current step and remaining steps
        var remaining = _currentSimulation.MaxSteps - _currentSimulation.CurrentStep;
        TimeText.Text = $"Step {_currentSimulation.CurrentStep}/{_currentSimulation.MaxSteps}";
        
        StatusText.Text = _currentSimulation.IsRunning ? "Running" : 
                         _currentSimulation.IsCompleted ? "Completed" : "Stopped";
        
        // Update colors based on values
        UpdateStatusColors(lifeSupportPercent, defensePercent);
    }

    private void UpdateStatusColors(double lifeSupport, double defense)
    {
        // Update life support color
        if (lifeSupport >= 70)
            LifeSupportText.Foreground = new SolidColorBrush(Colors.LimeGreen);
        else if (lifeSupport >= 40)
            LifeSupportText.Foreground = new SolidColorBrush(Colors.Yellow);
        else
            LifeSupportText.Foreground = new SolidColorBrush(Colors.Red);
            
        // Update defense color
        if (defense >= 70)
            DefenseText.Foreground = new SolidColorBrush(Colors.LimeGreen);
        else if (defense >= 40)
            DefenseText.Foreground = new SolidColorBrush(Colors.Yellow);
        else
            DefenseText.Foreground = new SolidColorBrush(Colors.Red);
    }

    private void PauseSimulation()
    {
        _isPaused = true;
        _simulationStepTimer.Stop();
        PauseButton.IsEnabled = false;
        ResumeButton.IsEnabled = true;
        StatusBarText.Text = "Simulation paused";
    }

    private void ResumeSimulation()
    {
        _isPaused = false;
        _simulationStepTimer.Start();
        PauseButton.IsEnabled = true;
        ResumeButton.IsEnabled = false;
        StatusBarText.Text = "Simulation resumed";
    }

    private void StopSimulation()
    {
        // Stop timers
        _uiUpdateTimer?.Stop();
        _simulationStepTimer?.Stop();
        
        // Stop simulation
        _currentSimulation?.Stop();
        _currentSimulation = null;
        
        RestoreConsoleOutput();
        
        // Reset UI state
        StartSimulationButton.IsEnabled = true;
        PauseButton.IsEnabled = false;
        ResumeButton.IsEnabled = false;
        StopButton.IsEnabled = false;
        _isPaused = false;
        
        StatusBarText.Text = "Simulation stopped";
    }

    private void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    protected override void OnClosed(EventArgs e)
    {
        StopSimulation();
        base.OnClosed(e);
    }
}