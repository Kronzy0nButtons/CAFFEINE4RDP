using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Caffeine4RDP
{
    public partial class Form1 : Form
    {

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // width of ellipse
            int nHeightEllipse // height of ellipse
        );

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        private const int INPUT_KEYBOARD = 1;
        private const int VK_NUMPAD0 = 0x60;
        private const int VK_NUMPAD9 = 0x69;

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private Dictionary<string, IntPtr> rdpWindows = new Dictionary<string, IntPtr>();


        public Form1()
        {
            InitializeComponent();
            LoadRdpWindows();

            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST)
                m.Result = (IntPtr)(HT_CAPTION);
        }

        private const int WM_NCHITTEST = 0x84;
        private const int HT_CLIENT = 0x1;
        private const int HT_CAPTION = 0x2;

        private void LoadRdpWindows()
        {
            if (timer1 != null)
            {
                // Stop the timer
                timer1.Stop();
                timer1.Dispose();
            }
            timer1 = null;
            button1.Text = "Start";

            rdpWindows.Clear();
            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    StringBuilder windowText = new StringBuilder(256);
                    GetWindowText(hWnd, windowText, windowText.Capacity);
                    string title = windowText.ToString();

                    if (title.Contains("Remote Desktop") || title.Contains(".local")) // Filter for RDP windows
                    {
                        rdpWindows[title] = hWnd;
                    }
                }
                return true;
            }, IntPtr.Zero);

            if (rdpWindows.Count == 0)
            {
                // No RDP windows found
                MessageBox.Show("No Remote Desktop windows found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBox1.Enabled = false; // Disable the dropdown
                comboBox1.DataSource = null; // Clear the DataSource
                button1.Enabled = false;
            }
            else
            {
                // RDP windows found, populate the dropdown
                comboBox1.DataSource = new BindingSource(rdpWindows, null);
                comboBox1.DisplayMember = "Key";
                comboBox1.ValueMember = "Value";
                comboBox1.Enabled = true; // Enable the dropdown
                button1.Enabled = true;
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (timer1 == null)
            {
                // Start the timer
                int interval = (int)numericUpDown1.Value * 1000;
                timer1 = new System.Windows.Forms.Timer { Interval = interval };
                timer1.Tick += timer1_Tick;
                timer1.Start();
                button1.Text = "Stop";
            }
            else
            {
                // Stop the timer
                timer1.Stop();
                timer1.Dispose();
                timer1 = null;
                button1.Text = "Start";
            }
        }

        private void timer1_Tick(object? sender, EventArgs e)
        {
            if (comboBox1.SelectedValue is IntPtr hWnd)
            {
                SendRandomInput(hWnd);
            }
        }

        private void SendRandomInput(IntPtr hWnd)
        {
            Random rand = new Random();
            int key = rand.Next(VK_NUMPAD0, VK_NUMPAD9);

            // Create a keyboard input structure
            Input[] inputs = new Input[]
            {
                new Input
                {
                    Type = INPUT_KEYBOARD,
                    Data = new InputUnion
                    {
                        ki = new KeyboardInput
                        {
                            wVk = (ushort)key,
                            dwFlags = 0 // Key down
                        }
                    }
                },
                new Input
                {
                    Type = INPUT_KEYBOARD,
                    Data = new InputUnion
                    {
                        ki = new KeyboardInput
                        {
                            wVk = (ushort)key,
                            dwFlags = 2 // Key up
                        }
                    }
                }
            };

            SetForegroundWindow(hWnd); // Bring window to the foreground
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadRdpWindows();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (timer1 != null)
            {
                // Stop the timer
                timer1.Stop();
                timer1.Dispose();
            }
            timer1 = null;
            button1.Text = "Start";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form2 aboutForm = new Form2();
            aboutForm.ShowDialog(this);
        }

        #region Structs and Enums
        [StructLayout(LayoutKind.Sequential)]
        private struct Input
        {
            public int Type;
            public InputUnion Data;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public MouseInput mi;
            [FieldOffset(0)]
            public KeyboardInput ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardInput
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MouseInput
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        #endregion
    }
}
