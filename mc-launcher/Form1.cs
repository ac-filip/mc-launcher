using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Auth.Microsoft;
using CmlLib.Core.ProcessBuilder;
using CmlLib.Core.Version;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace mc_launcher
{
    public partial class Form1 : Form
    {
        // ======================== MOVABLE WINDOW \/\/\/\/\/ ============================================= //
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        // ======================== ROUNDED CORNERS  ============================================= //
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
        // ======================== ===============  ============================================= //


        int ramVal = 8000;
        string playerName = "";
        public Form1()
        {
            InitializeComponent();
            this.MouseDown += Form1_MouseDown;
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 15, 15));
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            this.tabControl1.SelectedIndex = 0;
            InitLauncher();
            await LoadVersions();

            try
            {
                session = await LoginMicrosoft();
                labelStatus.Text = $"{session.Username}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            comboBox1.SelectedIndex = 0;

        }



        private async void button1_Click(object sender, EventArgs e)
        {
            if (session == null)
            {
                MessageBox.Show("Login first!");
                return;
            }

            string version = comboBox1.SelectedItem.ToString();

            await Launch(version, session);

            //launch dialogue

            launching f2 = new launching();
            f2.ShowDialog();
        }


        private void label1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        ///==========================================================================================================================

        private async Task<MSession> LoginMicrosoft()
        {
            var loginHandler = JELoginHandlerBuilder.BuildDefault();

            // Opens browser for Microsoft login
            var session = await loginHandler.Authenticate();

            return session;
        }
        MinecraftLauncher launcher;

        private void InitLauncher()
        {
            var path = new MinecraftPath(); // default .minecraft
            launcher = new MinecraftLauncher(path);


        }
        private async Task LoadVersions()
        {
            var versions = await launcher.GetAllVersionsAsync();

            comboBox1.Items.Clear();

            foreach (var v in versions)
            {
                comboBox1.Items.Add(v.Name);
            }
        }

        private async Task Launch(string version, MSession session)
        {
            // download game if needed
            await launcher.InstallAsync(version);

            var option = new MLaunchOption
            {
                Session = session,
                MaximumRamMb = ramVal
            };

            var process = await launcher.BuildProcessAsync(version, option);
            process.Start();
        }

        MSession session;

        private async void buttonLogin_Click(object sender, EventArgs e)
        {
            try
            {
                session = await LoginMicrosoft();
                labelStatus.Text = $"Logged in as {session.Username}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/ac-filip");

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {
            this.tabControl1.SelectedIndex = 1;
        }

        private void label3_Click(object sender, EventArgs e)
        {
            this.tabControl1.SelectedIndex = 0;
        }

        private void label7_Click(object sender, EventArgs e)
        {
            this.tabControl1.SelectedIndex = 2;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
           ramVal = (int)numericUpDown1.Value;
        }

        private async void buttonLaunchOffline_Click(object sender, EventArgs e)
        {
            if (textBoxPlayerName.Text == "")
            {
                MessageBox.Show("Error: Player name cannot be blank");
            }
            else { playerName = textBoxPlayerName.Text; }

            string version = comboBox1.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(version))
            {
                MessageBox.Show("Select a version first!");
                return;
            }

            try
            {
                // Create offline session (non-premium)
                var offlineSession = MSession.GetOfflineSession(playerName);

                // Install version if needed
                await launcher.InstallAsync(version);

                var option = new MLaunchOption
                {
                    Session = offlineSession,
                    MaximumRamMb = ramVal
                };

                var process = await launcher.BuildProcessAsync(version, option);
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        
        private void labelMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void label9_Click(object sender, EventArgs e)
        {
            timerMenu.Enabled = true;
        }
        int MenuState = 0;
        private void timerMenu_Tick(object sender, EventArgs e)
        {
            if (MenuState == 0)
            {
                panelMenu.Width += 10;
                textBoxPlayerName.Left += 10;
                label5.Left += 10;
                label8.Left += 10;
                webBrowser1.Left += 10;

                if (panelMenu.Width >= 140)
                {
                    timerMenu.Enabled = false;
                    MenuState = 1;
                }
            }
            else if (MenuState == 1)
            {
                panelMenu.Width -= 10;
                textBoxPlayerName.Left -= 10;
                label5.Left -= 10;
                label8.Left -= 10;
                webBrowser1.Left -= 10;

                if (panelMenu.Width <= 50)
                {
                    timerMenu.Enabled = false;
                    MenuState = 0;
                }
            }

        }

        private void iconMenu_Click(object sender, EventArgs e)
        {
            timerMenu.Enabled = true;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            this.tabControl1.SelectedIndex = 0;
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            this.tabControl1.SelectedIndex = 2;
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            this.tabControl1.SelectedIndex = 1;
        }
    }
}
