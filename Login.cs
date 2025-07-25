using DocumentFormat.OpenXml.Office2010.Word.DrawingShape;
using Live_Rate_Application.Helper;
using System;
using System.Collections.Generic;
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
        public string token;
        public static Login CurrentInstance { get; private set; }
        public Login()
        {
            CurrentInstance = this; // Set the instance for later use
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
                    username = uname,
                    password
                };

                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        string apiUrl = "http://35.176.5.121:1001/ClientAuth/login";
                        string jsonData = JsonSerializer.Serialize(loginData);
                        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                        string responseContent = await response.Content.ReadAsStringAsync();

                        using (JsonDocument doc = JsonDocument.Parse(responseContent))
                        {
                            var root = doc.RootElement;

                            if (response.IsSuccessStatusCode)
                            {
                                bool isSuccess = root.GetProperty("isSuccess").GetBoolean();

                                if (isSuccess)
                                {
                                    token = root.GetProperty("token").GetString();

                                    // Decode JWT token
                                    var payload = DecodeJwtPayload(token);

                                    // Extract values from decoded payload
                                    string userName = payload.GetProperty("ClientName").GetString();
                                    bool isActive = payload.GetProperty("IsActive").GetString().ToLower() == "true";

                                    if (isActive)
                                    {
                                        Live_Rate live_Rate = new Live_Rate();
                                        live_Rate.Show();
                                        SaveCredential(); // Presumably saves token or login info
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
                                else
                                {
                                    string message = root.GetProperty("message").GetString();
                                    MessageBox.Show(message ?? "Login failed.",
                                        "Authentication Failed",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Exclamation);
                                }
                            }
                            else
                            {
                                string message = root.GetProperty("message").GetString();
                                MessageBox.Show(message ?? "Login failed.",
                                    "Authentication Failed",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Exclamation);
                            }
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

        // Helper method to decode JWT payload
        public static JsonElement DecodeJwtPayload(string jwt)
        {
            string payload = jwt.Split('.')[1];

            // Add padding if required
            int mod = payload.Length % 4;
            if (mod > 0)
                payload += new string('=', 4 - mod);

            byte[] bytes = Convert.FromBase64String(payload);
            string json = Encoding.UTF8.GetString(bytes);

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                return doc.RootElement.Clone();
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
