using System.Windows;
using LeXtudio.DevFlow.Agent.Core;
using LeXtudio.DevFlow.Agent.Wpf;
using AgentOptions = Microsoft.Maui.DevFlow.Agent.Core.AgentOptions;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private WpfAgentService _devFlowService;

        public App()
        {
            //Dispatcher.Thread.CurrentUICulture = new System.Globalization.CultureInfo("ru");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _devFlowService = this.AddWpfDevFlowAgent(new AgentOptions
            {
                Port = GetAgentPort()
            });
        }

        private static int GetAgentPort()
        {
            var portValue = System.Environment.GetEnvironmentVariable("DEVFLOW_AGENT_PORT");
            if (int.TryParse(portValue, out var parsedPort) && parsedPort > 0)
            {
                return parsedPort;
            }

            return DevFlowAgentPortResolver.GetPortFromAssemblyMetadata() ?? AgentOptions.DefaultPort;
        }
    }
}
