using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.MsForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Live_Rate_Application
{
    partial class Live_Rate
    {
        private System.ComponentModel.IContainer components = null;

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Live_Rate));
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.toolsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMarketWatchHost = new System.Windows.Forms.ToolStripMenuItem();
            this.marketWatchMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newMarketWatchMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openCTRLOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Tools = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportToXSLXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addEditSymbolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addEditColumnsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.headerPanel = new System.Windows.Forms.Panel();
            this.titleLabel = new System.Windows.Forms.Label();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.panelStatusStrip = new System.Windows.Forms.StatusStrip();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.fontSizeComboBox = new System.Windows.Forms.ComboBox();
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
            this.mainMenu.GripMargin = new System.Windows.Forms.Padding(0);
            this.mainMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsMenuItem,
            this.saveMarketWatchHost,
            this.marketWatchMenuItem});
            this.mainMenu.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.mainMenu.Location = new System.Drawing.Point(0, 30);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.Padding = new System.Windows.Forms.Padding(0);
            this.mainMenu.Size = new System.Drawing.Size(1536, 27);
            this.mainMenu.TabIndex = 0;
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
            // saveMarketWatchHost
            // 
            this.saveMarketWatchHost.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.saveMarketWatchHost.BackColor = System.Drawing.Color.Transparent;
            this.saveMarketWatchHost.ForeColor = System.Drawing.Color.Black;
            this.saveMarketWatchHost.Margin = new System.Windows.Forms.Padding(5, 0, 10, 0);
            this.saveMarketWatchHost.Name = "saveMarketWatchHost";
            this.saveMarketWatchHost.Size = new System.Drawing.Size(165, 27);
            this.saveMarketWatchHost.Text = "Save MarketWatch";
            this.saveMarketWatchHost.Visible = false;
            this.saveMarketWatchHost.Click += new System.EventHandler(this.saveMarketWatchHost_Click);
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
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.White;
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(136)))), ((int)(((byte)(229)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView1.ColumnHeadersHeight = 40;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 10F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.EnableHeadersVisualStyles = false;
            this.dataGridView1.GridColor = System.Drawing.Color.Gainsboro;
            this.dataGridView1.Location = new System.Drawing.Point(0, 57);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 36;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView1.Size = new System.Drawing.Size(1536, 764);
            this.dataGridView1.TabIndex = 1;
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
            this.refreshToolStripMenuItem,
            this.addEditSymbolsToolStripMenuItem,
            this.addEditColumnsToolStripMenuItem});
            this.Tools.Name = "Tools";
            this.Tools.Size = new System.Drawing.Size(200, 100);
            // 
            // exportToXSLXToolStripMenuItem
            // 
            this.exportToXSLXToolStripMenuItem.Name = "exportToXSLXToolStripMenuItem";
            this.exportToXSLXToolStripMenuItem.Size = new System.Drawing.Size(199, 24);
            this.exportToXSLXToolStripMenuItem.Text = "Export to Excel";
            this.exportToXSLXToolStripMenuItem.Click += new System.EventHandler(this.ExportToXSLXToolStripMenuItem_Click);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(199, 24);
            this.refreshToolStripMenuItem.Text = "Refresh Data";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.RefreshToolStripMenuItem_Click);
            // 
            // addEditSymbolsToolStripMenuItem
            // 
            this.addEditSymbolsToolStripMenuItem.Enabled = false;
            this.addEditSymbolsToolStripMenuItem.Name = "addEditSymbolsToolStripMenuItem";
            this.addEditSymbolsToolStripMenuItem.Size = new System.Drawing.Size(199, 24);
            this.addEditSymbolsToolStripMenuItem.Text = "Add/Edit Symbols";
            this.addEditSymbolsToolStripMenuItem.Click += new System.EventHandler(this.addEditSymbolsToolStripMenuItem_Click);
            // 
            // addEditColumnsToolStripMenuItem
            // 
            this.addEditColumnsToolStripMenuItem.Name = "addEditColumnsToolStripMenuItem";
            this.addEditColumnsToolStripMenuItem.Size = new System.Drawing.Size(199, 24);
            this.addEditColumnsToolStripMenuItem.Text = "Add/Edit Columns";
            this.addEditColumnsToolStripMenuItem.Click += new System.EventHandler(this.addEditColumnsToolStripMenuItem_Click);
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
            this.bottomPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
            this.panelStatusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 13, 0);
            this.panelStatusStrip.Size = new System.Drawing.Size(1536, 26);
            this.panelStatusStrip.TabIndex = 0;
            // 
            // fontSizeComboBox
            // 
            this.fontSizeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fontSizeComboBox.FormattingEnabled = true;
            this.fontSizeComboBox.Items.AddRange(new object[] {
            "10",
            "12",
            "14",
            "16",
            "18",
            "20",
            "22",
            "24",
            "26",
            "28",
            "30"});
            this.fontSizeComboBox.Location = new System.Drawing.Point(1306, 32);
            this.fontSizeComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.fontSizeComboBox.Name = "fontSizeComboBox";
            this.fontSizeComboBox.Size = new System.Drawing.Size(160, 24);
            this.fontSizeComboBox.TabIndex = 5;
            this.fontSizeComboBox.Text = "Font Size";
            this.fontSizeComboBox.SelectedIndexChanged += new System.EventHandler(this.fontSizeComboBox_SelectedIndexChanged);
            // 
            // Live_Rate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1536, 847);
            this.Controls.Add(this.fontSizeComboBox);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.mainMenu);
            this.Controls.Add(this.headerPanel);
            this.Controls.Add(this.bottomPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenu;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Live_Rate";
            this.Text = "Live Rates";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Live_Rate_FormClosed);
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
        public System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private Panel headerPanel;
        public System.Windows.Forms.Label titleLabel;
        private ToolTip toolTip;
        private ToolStripMenuItem openCTRLOToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private StatusStrip panelStatusStrip;
        private Panel bottomPanel;
        private ToolStripMenuItem addEditSymbolsToolStripMenuItem;
        private ToolStripMenuItem saveMarketWatchHost;
        private ComboBox fontSizeComboBox;
        private ToolStripMenuItem addEditColumnsToolStripMenuItem;
    }
}