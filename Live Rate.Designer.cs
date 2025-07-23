using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.MsForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Live_Rate_Application
{
    partial class Live_Rate
    {
        private System.ComponentModel.IContainer components = null;
        private ToolStripControlHost editMarketWatchHost; // Added this declaration

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Live_Rate));
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.toolsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.marketWatchMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newMarketWatchMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openCTRLOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Tools = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportToXSLXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.headerPanel = new System.Windows.Forms.Panel();
            this.titleLabel = new System.Windows.Forms.Label();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.panelStatusStrip = new System.Windows.Forms.StatusStrip();
            this.editMarketWatchButton = new System.Windows.Forms.Button();
            this.editMarketWatchHost = new System.Windows.Forms.ToolStripControlHost(this.editMarketWatchButton);
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.mainMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.Tools.SuspendLayout();
            this.headerPanel.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.panelStatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.BackColor = System.Drawing.Color.WhiteSmoke;
            this.mainMenu.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.mainMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsMenuItem,
            this.editMarketWatchHost,
            this.marketWatchMenuItem});
            this.mainMenu.Location = new System.Drawing.Point(0, 30);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.Size = new System.Drawing.Size(1536, 31);
            this.mainMenu.TabIndex = 0;
            this.mainMenu.Padding = new Padding(0); // Remove default padding
            this.mainMenu.GripMargin = new Padding(0); // Remove grip margin
            this.mainMenu.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            // 
            // editMarketWatchHost
            // 
            this.editMarketWatchHost.Alignment = ToolStripItemAlignment.Right;
            this.editMarketWatchHost.Margin = new Padding(5, 0, 10, 0);
            // 
            // editMarketWatchButton
            // 
            this.editMarketWatchButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.editMarketWatchButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.editMarketWatchButton.FlatAppearance.BorderSize = 0;
            this.editMarketWatchButton.ForeColor = System.Drawing.Color.Black;
            this.editMarketWatchButton.BackColor = System.Drawing.Color.Transparent;
            this.editMarketWatchButton.Location = new System.Drawing.Point(1400, 3);
            this.editMarketWatchButton.Name = "editMarketWatchButton";
            this.editMarketWatchButton.Size = new System.Drawing.Size(133, 24);
            this.editMarketWatchButton.TabIndex = 1;
            this.editMarketWatchButton.Text = "Edit";
            this.editMarketWatchButton.Visible = false;
            this.editMarketWatchButton.Click += new System.EventHandler(this.EditMarketWatchButton_Click);
            // 
            // toolsMenuItem
            // 
            this.toolsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToolStripMenuItem,
            this.disconnectToolStripMenuItem});
            this.toolsMenuItem.Name = "toolsMenuItem";
            this.toolsMenuItem.Size = new System.Drawing.Size(62, 27);
            this.toolsMenuItem.Text = "Tools";
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(255, 28);
            this.connectToolStripMenuItem.Text = "Connect     (CTRL+C)";
            this.connectToolStripMenuItem.ToolTipText = "Connect with Live Data Rate";
            this.connectToolStripMenuItem.Click += new System.EventHandler(this.ConnectToolStripMenuItem_Click);
            // 
            // disconnectToolStripMenuItem
            // 
            this.disconnectToolStripMenuItem.Enabled = false;
            this.disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
            this.disconnectToolStripMenuItem.Size = new System.Drawing.Size(255, 28);
            this.disconnectToolStripMenuItem.Text = "Disconnect (CTRL+D)";
            this.disconnectToolStripMenuItem.ToolTipText = "Disconnect From Server and stop data Update";
            this.disconnectToolStripMenuItem.Click += new System.EventHandler(this.DisconnectToolStripMenuItem_Click);
            // 
            // marketWatchMenuItem
            // 
            this.marketWatchMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newMarketWatchMenuItem,
            this.openCTRLOToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.marketWatchMenuItem.Name = "marketWatchMenuItem";
            this.marketWatchMenuItem.Size = new System.Drawing.Size(130, 27);
            this.marketWatchMenuItem.Text = "Market Watch";
            // 
            // newMarketWatchMenuItem
            // 
            this.newMarketWatchMenuItem.Name = "newMarketWatchMenuItem";
            this.newMarketWatchMenuItem.Size = new System.Drawing.Size(231, 28);
            this.newMarketWatchMenuItem.Text = "New      (CTRL+N)";
            this.newMarketWatchMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.newMarketWatchMenuItem.ToolTipText = "Click to open new Marketwatch";
            this.newMarketWatchMenuItem.Click += new System.EventHandler(this.NewMarketWatchMenuItem_Click);
            // 
            // openCTRLOToolStripMenuItem
            // 
            this.openCTRLOToolStripMenuItem.Name = "openCTRLOToolStripMenuItem";
            this.openCTRLOToolStripMenuItem.Size = new System.Drawing.Size(231, 28);
            this.openCTRLOToolStripMenuItem.Text = "View";
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(231, 28);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
            // 
            // dataGridView1
            // 
            // Disable user editing features
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.ReadOnly = true;

            // Visual layout
            this.dataGridView1.BackgroundColor = Color.White;
            this.dataGridView1.BorderStyle = BorderStyle.None;
            this.dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Raised;
            this.dataGridView1.GridColor = Color.Gainsboro;
            this.dataGridView1.Dock = DockStyle.Fill;
            this.dataGridView1.EnableHeadersVisualStyles = false;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 36;
            this.dataGridView1.SelectionMode = DataGridViewSelectionMode.CellSelect;
            this.dataGridView1.MultiSelect = false;  // Ensure MultiSelect is disabled
            this.dataGridView1.ClearSelection();


            // Column header style
            DataGridViewCellStyle columnHeaderStyle = new DataGridViewCellStyle();
            columnHeaderStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            columnHeaderStyle.BackColor = Color.FromArgb(33, 150, 243); // Material Blue
            columnHeaderStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            columnHeaderStyle.ForeColor = Color.White;
            columnHeaderStyle.SelectionBackColor = Color.FromArgb(30, 136, 229); // Hover effect
            columnHeaderStyle.SelectionForeColor = Color.White;
            columnHeaderStyle.WrapMode = DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = columnHeaderStyle;
            this.dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView1.ColumnHeadersHeight = 40;

            // Default cell style
            DataGridViewCellStyle cellStyle = new DataGridViewCellStyle();
            cellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            cellStyle.BackColor = Color.White;
            cellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            cellStyle.ForeColor = Color.Black;
            cellStyle.WrapMode = DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = cellStyle;

            // Alternating row colors
            this.dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            this.toolTip.SetToolTip(this.dataGridView1, "Right-click for more options");
            this.dataGridView1.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGridView1_CellMouseDown);
            this.dataGridView1.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1_CellMouseEnter);
            this.dataGridView1.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1_CellMouseLeave);
            this.dataGridView1.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DataGridView1_DataError);
            // 
            // Tools
            // 
            this.Tools.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.Tools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportToXSLXToolStripMenuItem,
            this.refreshToolStripMenuItem});
            this.Tools.Name = "Tools";
            this.Tools.Size = new System.Drawing.Size(178, 52);
            // 
            // exportToXSLXToolStripMenuItem
            // 
            this.exportToXSLXToolStripMenuItem.Name = "exportToXSLXToolStripMenuItem";
            this.exportToXSLXToolStripMenuItem.Size = new System.Drawing.Size(177, 24);
            this.exportToXSLXToolStripMenuItem.Text = "Export to Excel";
            this.exportToXSLXToolStripMenuItem.Click += new System.EventHandler(this.ExportToXSLXToolStripMenuItem_Click);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(177, 24);
            this.refreshToolStripMenuItem.Text = "Refresh Data";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.RefreshToolStripMenuItem_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 20);
            // 
            // headerPanel
            // 
            this.headerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.headerPanel.Controls.Add(this.titleLabel);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Location = new System.Drawing.Point(0, 0);
            this.headerPanel.Margin = new System.Windows.Forms.Padding(4);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(1536, 30);
            this.headerPanel.TabIndex = 3;
            // 
            // titleLabel
            // 
            this.titleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.ForeColor = System.Drawing.Color.White;
            this.titleLabel.Location = new System.Drawing.Point(0, 0);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(1536, 30);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "DEFAULT";
            this.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this.panelStatusStrip);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 821);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(1536, 26);
            this.bottomPanel.TabIndex = 4;
            // 
            // panelStatusStrip
            // 
            this.panelStatusStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelStatusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.panelStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.panelStatusStrip.Location = new System.Drawing.Point(0, 0);
            this.panelStatusStrip.Name = "panelStatusStrip";
            this.panelStatusStrip.Size = new System.Drawing.Size(1536, 26);
            this.panelStatusStrip.TabIndex = 0;
            // 
            // Live_Rate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1536, 847);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.mainMenu);
            this.Controls.Add(this.headerPanel);
            this.Controls.Add(this.bottomPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenu;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Live_Rate";
            this.Text = "Live Rates";
            this.Load += new System.EventHandler(this.Live_Rate_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Live_Rate_KeyDown);
            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.Tools.ResumeLayout(false);
            this.headerPanel.ResumeLayout(false);
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.panelStatusStrip.ResumeLayout(false);
            this.panelStatusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip mainMenu;
        private System.Windows.Forms.ToolStripMenuItem toolsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disconnectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem marketWatchMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newMarketWatchMenuItem;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ContextMenuStrip Tools;
        private System.Windows.Forms.ToolStripMenuItem exportToXSLXToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private Panel headerPanel;
        private System.Windows.Forms.Label titleLabel;
        private ToolTip toolTip;
        private ToolStripMenuItem openCTRLOToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private Button editMarketWatchButton;
        private StatusStrip panelStatusStrip;
        private Panel bottomPanel;
        //private ToolStripControlHost editMarketWatchHost;
    }
}