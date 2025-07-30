using System;
using System.Drawing;
using System.Windows.Forms;

namespace Live_Rate_Application
{
    partial class Login
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            this.uname = new System.Windows.Forms.Label();
            this.password = new System.Windows.Forms.Label();
            this.unameTextBox = new System.Windows.Forms.TextBox();
            this.passwordtextBox = new System.Windows.Forms.TextBox();
            this.loginbutton = new System.Windows.Forms.Button();
            this.eyePictureBox = new System.Windows.Forms.PictureBox();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.formPanel = new System.Windows.Forms.Panel();
            this.exitLabelButton = new System.Windows.Forms.Label();
            this.saveCredential = new System.Windows.Forms.CheckBox();
            this.titleLabel = new System.Windows.Forms.Label();
            this.unameUnderline = new System.Windows.Forms.Panel();
            this.passwordUnderline = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.eyePictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.formPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // uname
            // 
            this.uname.AutoSize = true;
            this.uname.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.uname.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.uname.Location = new System.Drawing.Point(125, 188);
            this.uname.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.uname.Name = "uname";
            this.uname.Size = new System.Drawing.Size(87, 23);
            this.uname.TabIndex = 0;
            this.uname.Text = "Username";
            // 
            // password
            // 
            this.password.AutoSize = true;
            this.password.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.password.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.password.Location = new System.Drawing.Point(125, 275);
            this.password.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.password.Name = "password";
            this.password.Size = new System.Drawing.Size(80, 23);
            this.password.TabIndex = 1;
            this.password.Text = "Password";
            // 
            // unameTextBox
            // 
            this.unameTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.unameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.unameTextBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.unameTextBox.Location = new System.Drawing.Point(125, 219);
            this.unameTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.unameTextBox.Name = "unameTextBox";
            this.unameTextBox.Size = new System.Drawing.Size(375, 23);
            this.unameTextBox.TabIndex = 1;
            this.unameTextBox.Enter += new System.EventHandler(this.TextBox_Enter);
            this.unameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.UnameTextBox_KeyDown);
            this.unameTextBox.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // passwordtextBox
            // 
            this.passwordtextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.passwordtextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.passwordtextBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.passwordtextBox.Location = new System.Drawing.Point(125, 306);
            this.passwordtextBox.Margin = new System.Windows.Forms.Padding(4);
            this.passwordtextBox.Name = "passwordtextBox";
            this.passwordtextBox.PasswordChar = '•';
            this.passwordtextBox.Size = new System.Drawing.Size(338, 23);
            this.passwordtextBox.TabIndex = 2;
            this.passwordtextBox.Enter += new System.EventHandler(this.TextBox_Enter);
            this.passwordtextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PasswordtextBox_KeyDown);
            this.passwordtextBox.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // loginbutton
            // 
            this.loginbutton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.loginbutton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.loginbutton.FlatAppearance.BorderSize = 0;
            this.loginbutton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(80)))), ((int)(((byte)(175)))));
            this.loginbutton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(195)))));
            this.loginbutton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.loginbutton.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.loginbutton.ForeColor = System.Drawing.Color.White;
            this.loginbutton.Location = new System.Drawing.Point(125, 439);
            this.loginbutton.Margin = new System.Windows.Forms.Padding(4);
            this.loginbutton.Name = "loginbutton";
            this.loginbutton.Size = new System.Drawing.Size(375, 50);
            this.loginbutton.TabIndex = 4;
            this.loginbutton.Text = "LOGIN";
            this.loginbutton.UseVisualStyleBackColor = false;
            this.loginbutton.Click += new System.EventHandler(this.Login_Click);
            this.loginbutton.MouseEnter += new System.EventHandler(this.Button_MouseEnter);
            this.loginbutton.MouseLeave += new System.EventHandler(this.Button_MouseLeave);
            // 
            // eyePictureBox
            // 
            this.eyePictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.eyePictureBox.Image = ((System.Drawing.Image)(resources.GetObject("eyePictureBox.Image")));
            this.eyePictureBox.Location = new System.Drawing.Point(462, 306);
            this.eyePictureBox.Margin = new System.Windows.Forms.Padding(4);
            this.eyePictureBox.Name = "eyePictureBox";
            this.eyePictureBox.Size = new System.Drawing.Size(30, 22);
            this.eyePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.eyePictureBox.TabIndex = 4;
            this.eyePictureBox.TabStop = false;
            this.eyePictureBox.Click += new System.EventHandler(this.EyePictureBox_Click);
            // 
            // errorProvider
            // 
            this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider.ContainerControl = this;
            // 
            // formPanel
            // 
            this.formPanel.BackColor = System.Drawing.Color.White;
            this.formPanel.Controls.Add(this.exitLabelButton);
            this.formPanel.Controls.Add(this.saveCredential);
            this.formPanel.Controls.Add(this.titleLabel);
            this.formPanel.Controls.Add(this.uname);
            this.formPanel.Controls.Add(this.password);
            this.formPanel.Controls.Add(this.unameTextBox);
            this.formPanel.Controls.Add(this.passwordtextBox);
            this.formPanel.Controls.Add(this.loginbutton);
            this.formPanel.Controls.Add(this.eyePictureBox);
            this.formPanel.Controls.Add(this.unameUnderline);
            this.formPanel.Controls.Add(this.passwordUnderline);
            this.formPanel.Location = new System.Drawing.Point(62, 62);
            this.formPanel.Margin = new System.Windows.Forms.Padding(4);
            this.formPanel.Name = "formPanel";
            this.formPanel.Size = new System.Drawing.Size(625, 625);
            this.formPanel.TabIndex = 0;
            // 
            // exitLabelButton
            // 
            this.exitLabelButton.AutoSize = true;
            this.exitLabelButton.BackColor = System.Drawing.Color.Red;
            this.exitLabelButton.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.exitLabelButton.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.exitLabelButton.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exitLabelButton.ForeColor = System.Drawing.Color.White;
            this.exitLabelButton.Location = new System.Drawing.Point(556, 0);
            this.exitLabelButton.Name = "exitLabelButton";
            this.exitLabelButton.Size = new System.Drawing.Size(68, 40);
            this.exitLabelButton.TabIndex = 1;
            this.exitLabelButton.Text = "  X  ";
            this.exitLabelButton.Click += new System.EventHandler(this.exitLabelButton_Click);
            // 
            // saveCredential
            // 
            this.saveCredential.AutoSize = true;
            this.saveCredential.Location = new System.Drawing.Point(125, 374);
            this.saveCredential.Name = "saveCredential";
            this.saveCredential.Size = new System.Drawing.Size(129, 24);
            this.saveCredential.TabIndex = 3;
            this.saveCredential.Text = "Remember Me";
            this.saveCredential.UseVisualStyleBackColor = true;
            // 
            // titleLabel
            // 
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 18F, System.Drawing.FontStyle.Bold);
            this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.titleLabel.Location = new System.Drawing.Point(4, 66);
            this.titleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(617, 50);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "Welcome";
            this.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // unameUnderline
            // 
            this.unameUnderline.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.unameUnderline.Location = new System.Drawing.Point(125, 244);
            this.unameUnderline.Margin = new System.Windows.Forms.Padding(4);
            this.unameUnderline.Name = "unameUnderline";
            this.unameUnderline.Size = new System.Drawing.Size(375, 1);
            this.unameUnderline.TabIndex = 5;
            // 
            // passwordUnderline
            // 
            this.passwordUnderline.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.passwordUnderline.Location = new System.Drawing.Point(125, 331);
            this.passwordUnderline.Margin = new System.Windows.Forms.Padding(4);
            this.passwordUnderline.Name = "passwordUnderline";
            this.passwordUnderline.Size = new System.Drawing.Size(375, 1);
            this.passwordUnderline.TabIndex = 6;
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(750, 750);
            this.Controls.Add(this.formPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Login";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Login_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.eyePictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.formPanel.ResumeLayout(false);
            this.formPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Label uname;
        private Label password;
        private TextBox unameTextBox;
        private TextBox passwordtextBox;
        private Button loginbutton;
        private PictureBox eyePictureBox;
        private ErrorProvider errorProvider;
        private Panel formPanel;
        private Label titleLabel;
        private CheckBox saveCredential;
        private Panel unameUnderline;
        private Panel passwordUnderline;
        private Label exitLabelButton;
    }
}