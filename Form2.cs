using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Caffeine4RDP
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterParent;
            // Get the assembly version
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            richTextBox1.Text = $"Version {version.Major}.{version.Minor}.{version.Build}.{version.Revision} (64 Bit)";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
