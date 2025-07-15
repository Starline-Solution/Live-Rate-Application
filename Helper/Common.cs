using SocketIOClient;
using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Live_Rate_Application.Helper
{
    public class Common
    {
        private System.Windows.Forms.Timer internetCheckTimer;
        private bool isInternetAvailable = true;
        private readonly Control uiContext; // store a reference to the UI thread control

        public Common(Control control)
        {
            uiContext = control;
        }

        public bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    stream.Close();
                }
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }

        public bool InternetAvilable()
        {
            try
            {
                // Quick check using NetworkInterface
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    // More thorough check by pinging a reliable server
                    using (var ping = new Ping())
                    {
                        var reply = ping.Send("8.8.8.8", 3000); // Google DNS
                        return reply.Status == IPStatus.Success;

                    }

                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void StartInternetMonitor()
        {
            internetCheckTimer = new System.Windows.Forms.Timer
            {
                Interval = 2000 // check every 2 seconds
            };
            internetCheckTimer.Tick += InternetCheckTimer_Tick;
            internetCheckTimer.Start();
        }

        private void InternetCheckTimer_Tick(object sender, EventArgs e)
        {
            bool currentlyAvailable = InternetAvilable();

            if (currentlyAvailable && !isInternetAvailable)
            {
                isInternetAvailable = true;
                ResumeAppLogic();
            }
            else if (!currentlyAvailable && isInternetAvailable)
            {
                isInternetAvailable = false;
                PauseAppLogic();
            }
        }

        private void PauseAppLogic()
        {
            internetCheckTimer.Stop();
            uiContext.Invoke((MethodInvoker)(() =>
            {
                MessageBox.Show("Internet connection lost. The app will pause until it's restored.",
                                "Internet Disconnected",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }));
            internetCheckTimer.Start();
        }

        private void ResumeAppLogic()
        {
            uiContext.Invoke((MethodInvoker)(() =>
            {
                MessageBox.Show("Internet connection restored. The app will now resume.",
                                "Internet Connected",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }));
        }



    }
}
