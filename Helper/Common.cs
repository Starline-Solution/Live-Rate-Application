using SocketIOClient;
using System;
using System.Globalization;
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
        Live_Rate live_Rate = Live_Rate.CurrentInstance;

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
                Interval = 1000 // check every 1 seconds
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
            }
        }

        private void ResumeAppLogic()
        {
            if (live_Rate != null && live_Rate.socket.Disconnected == true)
            {
                live_Rate.socket.ConnectAsync();
                if (live_Rate.socket.Disconnected == true) 
                {
                    MessageBox.Show("Real time Data stop due to unexpected Network change!","Alert",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                }
            }
            
        }


        // Helper method for safe decimal conversion
        public decimal SafeConvertToDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value) ||
                value.Equals("NaN", StringComparison.OrdinalIgnoreCase))
            {
                return 0m;
            }

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            return 0m; // Default fallback value
        }


    }
}
