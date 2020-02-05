using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentScheduler;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Native;

namespace BusChecker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
            RefreshForm();
        }
        private void RefreshForm()
        {
            textBox1.Text = GetCurrentPCIeLanes().ToString();
            label2.Text = DateTime.Now.ToString("MM/dd hh:mm");
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
            }
        }
        private int GetCurrentPCIeLanes()
        {
            return GPUApi.GetCurrentPCIEDownStreamWidth(PhysicalGPU.GetPhysicalGPUs()[0].Handle);
        }
        public void RefreshData()
        {
            if (GetCurrentPCIeLanes() < 16)
            {
                MessageBox.Show("Current PCIe Lanes being used: " + GetCurrentPCIeLanes(), "Attention", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void menItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        private void InitContextMenu()
        {
            var contextMen = new ContextMenu();
            var menItem = new MenuItem()
            {
                Index = 0,
                Text = "E&xit"
            };
            menItem.Click += new EventHandler(menItem_Click);

            contextMen.MenuItems.Add(menItem);

            notifyIcon1.ContextMenu = contextMen;
        }
        private void InitRecurringJobs()
        {
            var reg = new Registry();

            reg.Schedule(() => RefreshData()).ToRunNow().AndEvery(1).Hours();

            JobManager.Initialize(reg);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            InitContextMenu();

            InitRecurringJobs();

            RefreshForm();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
            notifyIcon1.Visible = true;
        }
    }
}
