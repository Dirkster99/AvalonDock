namespace AvalonDock.WinFormsTestApp
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using Xceed.Wpf.AvalonDock;
    using Xceed.Wpf.AvalonDock.Layout;
    using Xceed.Wpf.AvalonDock.Layout.Serialization;
    using Xceed.Wpf.AvalonDock.Themes;

    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        DockingManager _dockingManager = new DockingManager();
        protected override void OnLoad(EventArgs e)
        {
            _dockingManager.Theme = new Vs2013LightTheme();
            XmlLayoutSerializer serializer = new XmlLayoutSerializer(_dockingManager);

            serializer.LayoutSerializationCallback += (s, args) =>
                {
                    switch (args.Model.ContentId)
                    {
                        case "toolWindow1":
                            args.Content = new System.Windows.Controls.TextBlock() { Text = args.Model.ContentId };
                            break;
                        default:
                            args.Content = new System.Windows.Controls.TextBox() { Text = args.Model.ContentId };
                            break;
                    }

                };

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "WinFormsTestApp.DefaultLayout.xml";

            string result = string.Empty;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }

            serializer.Deserialize(new System.IO.StringReader(result));

            LayoutDocument doc = new LayoutDocument() { Title = "test" };
            _dockingManager.Layout.Descendents().OfType<LayoutDocumentPane>().First().Children.Add(doc);

            dockingManagerHost.Child = _dockingManager;

            base.OnLoad(e);
        }

        private void menuItemVS2010_Click(object sender, EventArgs e)
        {
            _dockingManager.Theme = new Xceed.Wpf.AvalonDock.Themes.VS2010Theme();
            this.SetChecked(menuItemVS2010);
        }

        private void menuItemMetro_Click(object sender, EventArgs e)
        {
            _dockingManager.Theme = new Xceed.Wpf.AvalonDock.Themes.MetroTheme();
            this.SetChecked(menuItemMetro);
        }

        private void menuItemGeneric_Click(object sender, EventArgs e)
        {
            _dockingManager.Theme = new Xceed.Wpf.AvalonDock.Themes.GenericTheme();
            this.SetChecked(menuItemGeneric);
        }

        private void menuItemExpressionDark_Click(object sender, EventArgs e)
        {
            _dockingManager.Theme = new Xceed.Wpf.AvalonDock.Themes.ExpressionDarkTheme();
            this.SetChecked(menuItemExpressionDark);
        }

        private void menuItemExpressionLight_Click(object sender, EventArgs e)
        {
            _dockingManager.Theme = new Xceed.Wpf.AvalonDock.Themes.ExpressionLightTheme();
            this.SetChecked(menuItemExpressionLight);
        }

        private void vS2013ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _dockingManager.Theme = new Xceed.Wpf.AvalonDock.Themes.Vs2013LightTheme();
            this.SetChecked(menuItemVs2013);
        }

        private void menuItemAero_Vs2013Dark(object sender, EventArgs e)
        {
            _dockingManager.Theme = new Xceed.Wpf.AvalonDock.Themes.Vs2013DarkTheme();
            this.SetChecked(menuItemVS2013Dark);
        }

        private void SetChecked(ToolStripMenuItem toCheck)
        {
            menuItemVS2013Dark.Checked = false;
            menuItemGeneric.Checked = false;
            menuItemVS2010.Checked = false;
            menuItemExpressionDark.Checked = false;
            menuItemMetro.Checked = false;

            toCheck.Checked = true;
        }

        private void menuItemAero_Click(object sender, EventArgs e)
        {
            _dockingManager.Theme = new Xceed.Wpf.AvalonDock.Themes.AeroTheme();
            this.SetChecked(menuItemAero);
        }
    }
}
