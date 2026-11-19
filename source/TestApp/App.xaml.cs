using System.Windows;
#if DEVFLOW
using LeXtudio.DevFlow.Agent.Core;
using LeXtudio.DevFlow.Agent.Wpf;
using Microsoft.Maui.DevFlow.Agent.Core;
#endif

namespace TestApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#if DEVFLOW
        private WpfAgentService _devFlowService;
#endif

        public App()
        {
            //Dispatcher.Thread.CurrentUICulture = new System.Globalization.CultureInfo("ru");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
#if DEVFLOW
            System.AppDomain.CurrentDomain.FirstChanceException += (_, fc) =>
            {
                if (fc.Exception is System.InvalidOperationException ioe && ioe.Message.StartsWith("BuildWindowCore failed", System.StringComparison.Ordinal))
                {
                    var found = new System.Collections.Generic.List<string>();
                    void Walk(System.Windows.DependencyObject d, int depth)
                    {
                        if (d == null || depth > 60) return;
                        if (d is System.Windows.Interop.HwndHost h && h.GetType().Name == "WindowsFormsHost")
                        {
                            var t = h.GetType();
                            var asm = t.Assembly;
                            string info = $"{t.FullName} asm={asm.Location} size={(System.IO.File.Exists(asm.Location) ? new System.IO.FileInfo(asm.Location).Length : -1)} ver={asm.GetName().Version} handle=0x{h.Handle.ToInt64():X}";
                            try
                            {
                                var hc = t.GetProperty("HostContainerInternal", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)?.GetValue(h);
                                if (hc == null) info += " HostContainerInternal=null";
                                else
                                {
                                    var ct = hc.GetType();
                                    var hp = ct.GetProperty("Handle")?.GetValue(hc);
                                    var created = ct.GetProperty("IsHandleCreated")?.GetValue(hc);
                                    info += $" container={ct.FullName} containerAsm={ct.Assembly.GetName().Name} containerHandle={hp} IsHandleCreated={created}";
                                }
                            }
                            catch (System.Exception ex) { info += " reflect-failed=" + ex.GetType().Name + ":" + ex.Message; }
                            found.Add(info);
                        }
                        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(d); i++)
                            Walk(System.Windows.Media.VisualTreeHelper.GetChild(d, i), depth + 1);
                    }
                    foreach (System.Windows.Window w in Windows) Walk(w, 0);
                    System.Console.Error.WriteLine("[WFH-TRACE] " + (found.Count == 0 ? "(no WindowsFormsHost in the tree)" : string.Join(" | ", found)));
                }
            };
#endif
#if DEVFLOW
            _devFlowService = this.AddWpfDevFlowAgent(new AgentOptions
            {
                Port = GetAgentPort()
            });
#endif
        }

#if DEVFLOW
        private static int GetAgentPort()
        {
            var portValue = System.Environment.GetEnvironmentVariable("DEVFLOW_AGENT_PORT");
            if (int.TryParse(portValue, out var parsedPort) && parsedPort > 0)
            {
                return parsedPort;
            }

            return DevFlowAgentPortResolver.GetPortFromAssemblyMetadata() ?? AgentOptions.DefaultPort;
        }
#endif
    }
}
