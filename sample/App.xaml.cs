using LeXtudio.DevFlow.Agent.Core;
using LeXtudio.DevFlow.Agent.WPF;
using Microsoft.Maui.DevFlow.Agent.Core;

namespace AvalonDock.Sample;

public partial class App : System.Windows.Application
{
    private WpfAgentService? _agent;

    public MainWindow? SampleMainWindow => MainWindow as MainWindow;
    public global::AvalonDock.DockingManager? DockManager => SampleMainWindow?.GetDockManager();

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        _agent = this.AddWpfDevFlowAgent(new AgentOptions
        {
            Port = GetAgentPort()
        });
    }

    private static int GetAgentPort()
    {
        var portValue = Environment.GetEnvironmentVariable("DEVFLOW_AGENT_PORT");
        if (int.TryParse(portValue, out var parsedPort) && parsedPort > 0)
        {
            return parsedPort;
        }

        return DevFlowAgentPortResolver.GetPortFromAssemblyMetadata() ?? 5500;
    }
}
