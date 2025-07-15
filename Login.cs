using DocumentFormat.OpenXml.Office2010.Word.DrawingShape;
using Live_Rate_Application.Helper;
using System;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace Live_Rate_Application
{
    public partial class Login : Form
    {
        private readonly Helper.Common CommonClass;
        private bool passwordVisible = false; // Track password visibility state

        public Login()
        {
            InitializeComponent();
            this.KeyPreview = true; // Allow form to detect key presses
            this.FormClosed += Login_FormClosed;
            this.StartPosition = FormStartPosition.CenterScreen;

            CommonClass = new Common(this);

            // Initialize eye button
            InitializeEyeButton();
            // Initialize Save Credentials
            LoadSavedCredentials();
        }

        private void InitializeEyeButton()
        {
            // Set initial image
            try
            {
                eyePictureBox.Image = Properties.Resources.eye_open;
            }
            catch
            {
                // Fallback if image not found
                eyePictureBox.BackColor = Color.White;
                eyePictureBox.Paint += (s, e) => {
                    e.Graphics.DrawString("👁",
                        new Font("Segoe UI Emoji", 12),
                        Brushes.Black, 0, 0);
                };
            }

            eyePictureBox.Click += (s, e) => TogglePasswordVisibility();
            eyePictureBox.BringToFront();
        }

        private void LoadSavedCredentials()
        {
            // Explicitly declare the tuple types
            (string username, string password) = CredentialManager.LoadCredentials();

            if (username != null)
            {
                unameTextBox.Text = username;
                saveCredential.Checked = true;
                passwordtextBox.Text = password;
                loginbutton.Focus();
            }
        }

        private void TogglePasswordVisibility()
        {
            passwordVisible = !passwordVisible;
            passwordtextBox.PasswordChar = passwordVisible ? '\0' : '•';

            try
            {
                eyePictureBox.Image = passwordVisible
                    ? Properties.Resources.eye_close
                    : Properties.Resources.eye_open;
            }
            catch
            {
                // Fallback if images not available
                eyePictureBox.Invalidate(); // Forces repaint of our drawn eye
            }
        }

        private async void Login_Click(object sender, EventArgs e)
        {
            if (CommonClass.InternetAvilable())
            {

                string uname = unameTextBox.Text.Trim();
                string password = passwordtextBox.Text.Trim();

                //uname = "admin124";
                //password = "Ab123456";

                if (string.IsNullOrEmpty(uname) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Please enter both username and password.",
                                           "Authentication Failed",
                                           MessageBoxButtons.OK,
                                           MessageBoxIcon.Error);
                    return;
                }

                var loginData = new
                {
                    Username = uname,
                    password
                };

                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        string apiUrl = "http://18.133.220.200:9202/api/userlogin";
                        string jsonData = JsonSerializer.Serialize(loginData);
                        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseContent = await response.Content.ReadAsStringAsync();

                            using (JsonDocument doc = JsonDocument.Parse(responseContent))
                            {
                                var root = doc.RootElement;

                                string token = root.GetProperty("token").GetString();
                                var user = root.GetProperty("user");
                                string username = user.GetProperty("username").GetString();
                                bool active = user.GetProperty("active").GetBoolean();

                                if (active)
                                {
                                    Live_Rate live_Rate = new Live_Rate();
                                    live_Rate.Show();
                                    SaveCredential();
                                    this.Hide();
                                }
                                else
                                {
                                    MessageBox.Show("Subscription Expired.",
                                           "Authorization Failed",
                                           MessageBoxButtons.OK,
                                           MessageBoxIcon.Error);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid username or password.",
                                           "Authentication Failed",
                                           MessageBoxButtons.OK,
                                           MessageBoxIcon.Exclamation);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Login failed: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Check your internet connection and try again.",
                          "No Internet",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Warning);
            }
        }

        private void Login_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit(); // Closes all forms and ends the application
        }

        private void UnameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + Backspace → Clear all text
            if (e.Control && e.KeyCode == Keys.Back)
            {
                unameTextBox.Text = "";
                e.SuppressKeyPress = true;
            }

            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Optional: prevent ding sound
                loginbutton.PerformClick(); // Triggers the Click event
            }
        }

        private void PasswordtextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + Backspace → Clear all text
            if (e.Control && e.KeyCode == Keys.Back)
            {
                passwordtextBox.Text = "";
                e.SuppressKeyPress = true;
            }

            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Optional: prevent ding sound
                loginbutton.PerformClick(); // Triggers the Click event
            }
        }

        private void Login_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close(); // Close the login form
                Application.Exit(); // Terminate the application
            }
        }

        // Add these event handlers for better UX:

        private void TextBox_Enter(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;
            if (textBox.Tag is Panel underline)
            {
                underline.BackColor = Color.FromArgb(0, 120, 215);
                underline.Height = 2;
            }
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;
            if (textBox.Tag is Panel underline)
            {
                underline.BackColor = Color.FromArgb(200, 200, 200);
                underline.Height = 1;
            }
        }

        private void Button_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void Button_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        private void EyePictureBox_Click(object sender, EventArgs e)
        {
            if (this.passwordtextBox.PasswordChar == '•')
            {
                this.passwordtextBox.PasswordChar = '\0';
                this.eyePictureBox.Image = Properties.Resources.eye_open;
            }
            else
            {
                this.passwordtextBox.PasswordChar = '•';
                this.eyePictureBox.Image = Properties.Resources.eye_close;
            }
        }

        private void SaveCredential() 
        {
            if (!saveCredential.Checked)
            {
                CredentialManager.DeleteCredentials();
                return;
            }
            CredentialManager credentialManager = new CredentialManager(unameTextBox.Text, passwordtextBox.Text, saveCredential.Checked ? true : false);
        }

    }
}
