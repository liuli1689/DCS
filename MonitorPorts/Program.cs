using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MonitorPorts
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            //HardwareInfo hardwareInfo = new HardwareInfo();
            //string cpuID = hardwareInfo.GetCpuID();
            //if (cpuID == "BFEBFBFF000406F1")
            //{
            //    Application.Run(new MainForm());
            //}
            //else
            //{
            //    MessageBox.Show("非本机，不能使用！");
            //    Application.Exit();
            //}
        }
    }
}
