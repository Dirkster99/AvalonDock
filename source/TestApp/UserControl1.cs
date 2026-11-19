using System;
using System.Windows.Forms;

namespace TestApp
{
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
            HandleCreated += OnControlHandleCreated;
        }

        // HandleCreated rather than an OnLoad override: LibreWinForms, which backs
        // System.Windows.Forms on the portable backend, has neither OnLoad nor a Load event, and this
        // control is compiled for every platform. HandleCreated is also the more accurate trigger -
        // it is exactly when the child handles this reads become valid.
        private void OnControlHandleCreated(object sender, EventArgs e)
        {
            label1.Text = textBox1.Handle.ToString();
            label2.Text = textBox2.Handle.ToString();
        }

		private void label1_Click(object sender, EventArgs e)
		{

		}

		// An Alt+letter mnemonic on this button (its text starts with "&M") runs this handler through
		// ProcessMnemonic. It writes a marker the interop cases read back from label2, because "the button
		// fired" is not observable any other way from outside the hosted control tree.
		private void mnemonicButton_Click(object sender, EventArgs e)
		{
			label2.Text = "mnemonic: clicked";
		}
	}
}
