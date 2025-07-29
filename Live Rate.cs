using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Live_Rate_Application.Helper;
using Live_Rate_Application.MarketWatch;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace Live_Rate_Application
{
    public partial class Live_Rate : Form
    {

        [DllImport("oleaut32.dll", PreserveSig = false)]
        static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);
        private Helper.Common CommonClass;
        // In Live_Rate.cs
        public bool IsConnected
        {
            get { return connectionViewMode == ConnectionViewMode.Connect; }
            set { connectionViewMode = value ? ConnectionViewMode.Connect : ConnectionViewMode.Disconnect; }
        }
        public SocketIO socket = null;
        public readonly string AppFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Live Rate");
        public string lastOpenMarketWatch = string.Empty;
        bool isLoadedSymbol = false;
        public List<string> selectedSymbols = new List<string>();
        private List<string> symbolMaster = new List<string>();
        private bool isSymbolMasterInitialized = false;
        public List<string> FileLists = new List<string>();
        public List<string> columnPreferences;
        public List<string> allColumns = new List<string>()
               {
                    "Symbol",
                    "Bid",
                    "Ask",
                    "High",
                    "Low",
                    "Open",
                    "Close",
                    "LTP",
                    "DateTime"
               };
        public string saveFileName;
        public bool isEdit = false;
        private Dictionary<string, decimal> previousAsks = new Dictionary<string, decimal>();
        private int symbolColumnFixedWidth = 0;
        public string token;
        public int fontSize = 12;
        public static Live_Rate CurrentInstance { get; private set; }
        // DataTable Variables
        public System.Data.DataTable marketDataTable = new System.Data.DataTable();
        private readonly object tableLock = new object();
        private System.Windows.Forms.Button saveButton = new System.Windows.Forms.Button();

        private Panel panelAddSymbols;
        private CheckedListBox checkedListSymbols;
        private System.Windows.Forms.Button btnConfirmAddSymbols;
        private System.Windows.Forms.Button btnCancelAddSymbols;
        private System.Windows.Forms.Button btnSelectAllSymbols;
        private Panel panelAddColumns;
        private CheckedListBox checkedListColumns;
        private System.Windows.Forms.Button btnSelectAllColumns;
        private System.Windows.Forms.Button btnConfirmAddColumns;
        private System.Windows.Forms.Button btnCancelAddColumns;
        public class Symbol
        {
            public int Id { get; set; }
            public int ClientId { get; set; }
            public string Identifier { get; set; }
            public string Contract { get; set; }
        }

        public class SymbolResponse
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public List<Symbol> Data { get; set; }
        }
        private List<Symbol> _symbols = new List<Symbol>();
        public List<string> identifiers;

        //Excel File Variables
        public Excel.Application excelApp;
        private bool _headersWritten = false;
        private Excel.Workbook workbook;
        private Excel.Worksheet worksheet;
        private readonly string excelFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Live Rate", "Live Rate.xlsx");
        public enum MarketWatchViewMode
        {
            Default,
            New
        }
        public MarketWatchViewMode marketWatchViewMode = MarketWatchViewMode.Default;
        public enum ConnectionViewMode
        {
            Connect,
            Disconnect
        }
        public ConnectionViewMode connectionViewMode = ConnectionViewMode.Connect;

        #region Form Method

        public Live_Rate()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;

            InitializeComponent();

            this.KeyPreview = true; // Allow form to detect key presses
            this.DoubleBuffered = true;

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);

        }

        private async void Live_Rate_Load(object sender, EventArgs e)
        {
            if (!IsInDesignMode())
            {
                Login login = Login.CurrentInstance;
                token = login?.token;
                var (currentWatch, currentColumns) = CredentialManager.GetCurrentMarketWatchWithColumns();
                lastOpenMarketWatch =  currentWatch;
                columnPreferences = currentColumns;

                await LoadSymbolsAsync();

                CommonClass = new Helper.Common(this);
                CommonClass.StartInternetMonitor();

                InitializeSocket();
                InitializeDataTable();
                this.WindowState = FormWindowState.Maximized;
                dataGridView1.Dock = DockStyle.Fill;
                dataGridView1.ContextMenuStrip = Tools;
                CurrentInstance = this;

                PositionFontSizeComboBox();
                MenuLoad();
                HandleLastOpenedMarketWatch();

            }
        }


        private bool IsInDesignMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
                   Debugger.IsAttached && Process.GetCurrentProcess().ProcessName == "devenv";
        }

        private void PositionFontSizeComboBox()
        {
            if (saveMarketWatchHost.Owner == null) return;

            // Get the ToolStrip that contains the button
            ToolStrip toolStrip = saveMarketWatchHost.Owner;

            // Get the screen position of the ToolStripButton
            System.Drawing.Point buttonScreenPoint = toolStrip.PointToScreen(saveMarketWatchHost.Bounds.Location);

            // Convert to form's client coordinates
            System.Drawing.Point buttonClientPoint = this.PointToClient(buttonScreenPoint);

            // Move ComboBox just right to Save button with spacing
            int spacing = 50;
            fontSizeComboBox.Location = new System.Drawing.Point(buttonClientPoint.X + saveMarketWatchHost.Width + spacing, buttonClientPoint.Y + 2);
            fontSizeComboBox.BringToFront();
        }


        private async Task LoadSymbolsAsync()
        {
            try
            {
                string apiUrl = $"http://35.176.5.121:1001/ClientAuth/getSymbols?Token={token}";

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();

                        var symbolResponse = JsonSerializer.Deserialize<SymbolResponse>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (symbolResponse != null && symbolResponse.IsSuccess)
                        {
                            _symbols = symbolResponse.Data;
                            identifiers = _symbols.Select(s => s.Identifier).ToList();
                        }
                    }
                    else
                    {
                        MessageBox.Show($"API failed: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading symbols: " + ex.Message);
            }
        }

        private void Live_Rate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close(); // Close the login form
                System.Windows.Forms.Application.Exit(); // Terminate the application
            }

            if (e.Control && e.KeyCode == Keys.N && marketWatchViewMode != MarketWatchViewMode.New)
            {
                NewMarketWatchMenuItem_Click(this, EventArgs.Empty);
                e.Handled = true;
            }

            if (e.Control && e.KeyCode == Keys.C && connectionViewMode != ConnectionViewMode.Connect)
            {
                ConnectToolStripMenuItem_Click(this, EventArgs.Empty);
                e.Handled = true;
            }

            if (e.Control && e.KeyCode == Keys.D && connectionViewMode != ConnectionViewMode.Disconnect)
            {
                DisconnectToolStripMenuItem_Click(this, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private async void LiveRate_FormClosed(object sender, FormClosedEventArgs e)
        {

            // Your existing cleanup code remains unchanged
            if (workbook != null)
            {
                try { workbook.Close(false); } catch { }
                Marshal.ReleaseComObject(workbook);
            }

            if (excelApp != null)
            {
                try
                {
                    if (excelApp.Workbooks.Count == 0)
                        excelApp.Quit();
                }
                catch { }
                Marshal.ReleaseComObject(excelApp);
            }
            try
            {
                if (socket != null)
                {
                    if (socket.Connected)
                    {
                        await socket.DisconnectAsync();
                    }
                    socket.Dispose();
                }
            }
            catch { }


            GC.Collect();
            GC.WaitForPendingFinalizers();
            System.Windows.Forms.Application.Exit();
        }

        private void RefreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Refresh logic here
            statusLabel.Text = "Ready";

        }

        private void NewMarketWatchMenuItem_Click(object sender, EventArgs e)
        {
            marketWatchViewMode = MarketWatchViewMode.New;

            // Disconnect socket and clear old grid
            socket.DisconnectAsync();
            dataGridView1.Visible = false;
            dataGridView1.Rows.Clear();

            // Remove old editable grid and Save button if they exist
            var existingGrid = this.Controls.Find("editableMarketWatchGridView", true).FirstOrDefault();
            existingGrid?.Dispose();

            if (saveButton != null && this.Controls.Contains(saveButton))
                this.Controls.Remove(saveButton);

            if (isEdit == false)
            {
                selectedSymbols.Clear();
                saveFileName = null;
                isLoadedSymbol = false;
            }

            // Create new editable grid
            var editableGrid = new EditableMarketWatchGrid();
            editableGrid.Name = "editableMarketWatchGridView";
            this.Controls.Add(editableGrid);
            editableGrid.BringToFront();
            editableGrid.Focus();
            editableGrid.isEditMarketWatch = true;

            if (editableGrid != null && editableGrid.selectedSymbols != null && isEdit)
            {
                if (saveFileName != null)
                    editableGrid.saveFileName = saveFileName;
            }

            editableGrid.fontSize = fontSize;

            // Update UI state
            toolsMenuItem.Enabled = false;
            newMarketWatchMenuItem.Enabled = false;
            saveMarketWatchHost.Visible = true;
            saveMarketWatchHost.Text = "Save MarketWatch";
            statusLabel.Text = "Connected...";

            if (isEdit)
            {
                titleLabel.Text = $"Edit {saveFileName.ToUpper()} MarketWatch";
            }
            if (!isEdit)
            {
                titleLabel.Text = "New MarketWatch";
            }


            saveFileName = null;

            foreach (ToolStripMenuItem item in openCTRLOToolStripMenuItem.DropDownItems)
            {
                item.Enabled = true;
            }
        }

        public async void ConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {

            try
            {
                UpdateUI(() =>
                {
                    statusLabel.Text = "Connecting...";
                    connectToolStripMenuItem.Enabled = false;
                    disconnectToolStripMenuItem.Enabled = false;

                });

                if (socket != null && !socket.Connected)
                {
                    await socket.ConnectAsync();
                }
                connectionViewMode = ConnectionViewMode.Connect;
            }
            catch (Exception ex)
            {
                UpdateUI(() =>
                {
                    statusLabel.Text = $"Connection failed: {ex.Message}";

                    connectToolStripMenuItem.Enabled = true;
                    disconnectToolStripMenuItem.Enabled = false;
                    MessageBox.Show($"Connection failed: {ex.Message}", "Error",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }

        public async void DisconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateUI(() =>
                {
                    statusLabel.Text = "Disconnecting...";
                    disconnectToolStripMenuItem.Enabled = false;

                });

                if (socket != null && socket.Connected)
                {
                    await socket.DisconnectAsync();
                }
                connectionViewMode = ConnectionViewMode.Disconnect;
            }
            catch (Exception ex)
            {
                UpdateUI(() =>
                {
                    statusLabel.Text = $"Disconnection failed: {ex.Message}";

                    disconnectToolStripMenuItem.Enabled = true;
                    MessageBox.Show($"Disconnection failed: {ex.Message}", "Error",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }

        public void DefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolsMenuItem.Enabled = true;
            isLoadedSymbol = false;
            LiveRateGrid();

            MenuLoad();
            titleLabel.Text = "DEFAULT";
            saveFileName = null;
            isEdit = false;
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileLists == null || FileLists.Count == 0)
            {
                MessageBox.Show("No Market Watch available to delete.", "Information",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var selectionForm = new Form())
            {
                selectionForm.Text = "Select Market Watch to Delete";
                selectionForm.Width = 600;
                selectionForm.Height = 500;
                selectionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                selectionForm.StartPosition = FormStartPosition.CenterParent;
                selectionForm.BackColor = System.Drawing.Color.White;
                selectionForm.Font = new System.Drawing.Font("Segoe UI", 9);
                selectionForm.Icon = SystemIcons.WinLogo;

                var headerPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 50,
                    BackColor = System.Drawing.Color.FromArgb(0, 120, 215)
                };

                var headerLabel = new System.Windows.Forms.Label
                {
                    Text = "Select Market Watch to Delete",
                    Dock = DockStyle.Fill,
                    ForeColor = System.Drawing.Color.White,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold),
                    Padding = new Padding(15, 0, 0, 0)
                };
                headerPanel.Controls.Add(headerLabel);

                // Search box for filtering
                var searchBox = new System.Windows.Forms.TextBox
                {
                    Dock = DockStyle.Top,
                    Height = 30,
                    Margin = new Padding(10, 10, 10, 5),
                    Font = new System.Drawing.Font("Segoe UI", 9),
                    Text = "Search Here..."

                };

                // Modern list view with checkboxes
                var listView = new ListView
                {
                    Dock = DockStyle.Fill,
                    CheckBoxes = true,
                    View = System.Windows.Forms.View.Details,
                    FullRowSelect = true,
                    GridLines = false,
                    MultiSelect = false,
                    BorderStyle = BorderStyle.None,
                    BackColor = SystemColors.Window
                };

                // Modern column headers
                listView.Columns.Add("Market Watch Name", 300);
                listView.Columns.Add("Path", 250);

                // Add files to list view
                foreach (string filePath in FileLists)
                {
                    if (filePath != saveFileName)
                    {
                        var item = new ListViewItem(Path.GetFileName(filePath));
                        item.SubItems.Add(filePath);
                        item.Tag = filePath; // Store full path in tag
                        listView.Items.Add(item);
                    }
                }

                if (listView.Items.Count == 0)
                {
                    MessageBox.Show("There is only one MarketWatch and that Open so can't Delete.", "Information",
                             MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Selection controls panel
                var controlsPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 50,
                    BackColor = System.Drawing.Color.FromArgb(240, 240, 240)
                };

                // Modern flat buttons
                var selectAllButton = new System.Windows.Forms.Button
                {
                    Text = "Select All",
                    FlatStyle = FlatStyle.Flat,
                    BackColor = System.Drawing.Color.White,
                    ForeColor = System.Drawing.Color.FromArgb(0, 120, 215),
                    Height = 30,
                    Width = 120,
                    Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                    Margin = new Padding(10, 10, 0, 10)
                };


                var deleteButton = new System.Windows.Forms.Button
                {
                    Text = "Delete Selected",
                    FlatStyle = FlatStyle.Flat,
                    BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                    ForeColor = System.Drawing.Color.White,
                    Height = 30,
                    Width = 120,
                    Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                    Margin = new Padding(0, 10, 90, 10)
                };

                var cancelButton = new System.Windows.Forms.Button
                {
                    Text = "Cancel",
                    FlatStyle = FlatStyle.Flat,
                    BackColor = System.Drawing.Color.White,
                    ForeColor = System.Drawing.Color.FromArgb(0, 120, 215),
                    Height = 30,
                    Width = 80,
                    Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                    Margin = new Padding(0, 10, 10, 10)
                };

                // Button event handlers
                selectAllButton.Click += (s, args) =>
                {
                    foreach (ListViewItem item in listView.Items)
                    {
                        item.Checked = true;
                    }
                };


                cancelButton.Click += (s, args) => selectionForm.DialogResult = DialogResult.Cancel;

                deleteButton.Click += (s, args) =>
                {
                    var selectedFiles = listView.CheckedItems.Cast<ListViewItem>()
                                             .Select(item => item.Tag.ToString())
                                             .ToList();

                    if (selectedFiles.Count == 0)
                    {
                        MessageBox.Show("Please select at least one Market Watch to delete.",
                                        "No Selection",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                        return;
                    }

                    // Modern confirmation dialog
                    var confirmResult = MessageBox.Show($"Are you sure you want to delete {selectedFiles.Count} Market Watch(s)?",
                                                     "Confirm Deletion",
                                                     MessageBoxButtons.YesNo,
                                                     MessageBoxIcon.Warning,
                                                     MessageBoxDefaultButton.Button2);

                    if (confirmResult == DialogResult.Yes)
                    {
                        int successCount = 0;
                        var failedDeletions = new List<string>();

                        foreach (string filePath in selectedFiles)
                        {
                            if (saveFileName == filePath)
                            {
                                MessageBox.Show("Can't Delete Open MarketWatch", "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            string fullpath = Path.Combine(AppFolder, $"{filePath}.slt");
                            try
                            {
                                File.Delete(fullpath);
                                successCount++;
                            }
                            catch (Exception ex)
                            {
                                failedDeletions.Add($"{Path.GetFileName(filePath)}: {ex.Message}");
                            }
                        }

                        // Modern result display
                        var resultMessage = new StringBuilder();
                        resultMessage.AppendLine($"Successfully deleted {successCount} Market Watch(s).");

                        if (failedDeletions.Count > 0)
                        {
                            resultMessage.AppendLine();
                            resultMessage.AppendLine("The following files couldn't be deleted:");
                            resultMessage.AppendLine(string.Join(Environment.NewLine, failedDeletions));
                        }

                        MessageBox.Show(resultMessage.ToString(),
                                      "Deletion Results",
                                      MessageBoxButtons.OK,
                                      failedDeletions.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                        if (successCount > 0)
                        {
                            selectionForm.DialogResult = DialogResult.OK;
                        }

                        MenuLoad();
                    }
                };

                // Search functionality
                searchBox.TextChanged += (s, args) =>
                {
                    listView.BeginUpdate();
                    listView.Items.Clear();

                    foreach (string filePath in FileLists.Where(f =>
                        Path.GetFileName(f).IndexOf(searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        var item = new ListViewItem(Path.GetFileName(filePath));
                        item.SubItems.Add(filePath);
                        item.Tag = filePath;
                        listView.Items.Add(item);
                    }

                    listView.EndUpdate();
                };

                // Add controls to panels
                controlsPanel.Controls.Add(selectAllButton);
                controlsPanel.Controls.Add(deleteButton);
                controlsPanel.Controls.Add(cancelButton);

                // Add controls to form
                selectionForm.Controls.Add(listView);
                selectionForm.Controls.Add(searchBox);
                selectionForm.Controls.Add(headerPanel);
                selectionForm.Controls.Add(controlsPanel);

                // Set form buttons
                selectionForm.AcceptButton = deleteButton;
                selectionForm.CancelButton = cancelButton;

                // Show dialog
                if (selectionForm.ShowDialog() == DialogResult.OK)
                {
                    DefaultToolStripMenuItem_Click(this, EventArgs.Empty);
                    saveFileName = null;
                }
            }
        }

        private void saveMarketWatchHost_Click(object sender, EventArgs e)
        {
            if (saveMarketWatchHost.Text == "Save MarketWatch")
            {

                EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;

                if (editableMarketWatchGrid != null && editableMarketWatchGrid.selectedSymbols != null)
                {
                    selectedSymbols = editableMarketWatchGrid.selectedSymbols;
                    editableMarketWatchGrid.SaveSymbols(selectedSymbols);
                }
                else
                {
                    MessageBox.Show("No active market watch grid found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region Datagrid View EventListener
        private void DataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                dataGridView1.ClearSelection();
                //dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected = true;
            }
        }

        private void DataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void DataGridView1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
            }
        }

        private void DataGridView1_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.RowIndex % 2 == 0)
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.White;
                else
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 248, 248);
            }
        }
        #endregion

        #region Helper
        private void UpdateUI(System.Action action)
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
            {
                try
                {
                    this.BeginInvoke(action);
                }
                catch (ObjectDisposedException) { /* Form is closing */ }
            }
            else
            {
                action();
            }
        }

        public void LiveRateGrid()
        {
            if (!isLoadedSymbol)
                marketWatchViewMode = MarketWatchViewMode.Default;

            socket.ConnectAsync();

            // Hide the DataGridView
            dataGridView1.Visible = true;
            dataGridView1.BringToFront();
            dataGridView1.Focus();
            newMarketWatchMenuItem.Enabled = true;
        }

        public void MenuLoad()
        {
            EditableMarketWatchGrid editableMarketWatchGrid = new EditableMarketWatchGrid();
            try
            {
                // Get all .slt files from the application folder
                List<string> fileNames = Directory.GetFiles(EditableMarketWatchGrid.AppFolder, "*.slt")
                                                 .Select(Path.GetFileNameWithoutExtension)
                                                 .ToList();

                FileLists = fileNames;

                // Clear existing menu items
                openCTRLOToolStripMenuItem.DropDownItems.Clear();
                // Add Default menu item with click handler
                ToolStripMenuItem defaultMenuItem = new ToolStripMenuItem("Default");
                defaultMenuItem.Click += (sender, e) =>
                {
                    var clickedItem = (ToolStripMenuItem)sender;
                    DefaultToolStripMenuItem_Click(sender, e);
                    addEditSymbolsToolStripMenuItem.Enabled = false;
                    SetActiveMenuItem(clickedItem);
                    saveMarketWatchHost.Visible = false;
                    lastOpenMarketWatch = "Default";
                };
                defaultMenuItem.Enabled = false;
                openCTRLOToolStripMenuItem.DropDownItems.Add(defaultMenuItem);

                // Add each file as a menu item with a click handler
                foreach (string fileName in fileNames)
                {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem(fileName);
                    menuItem.Click += (sender, e) =>
                    {
                        // Handle file selection here
                        string selectedFile = (sender as ToolStripMenuItem).Text;
                        saveFileName = selectedFile;
                        addEditSymbolsToolStripMenuItem.Enabled = true;
                        LoadSymbol(Path.Combine(selectedFile + ".slt"));
                        SetActiveMenuItem(menuItem);
                        titleLabel.Text = selectedFile.ToUpper();
                        isEdit = false;
                        saveMarketWatchHost.Visible = false;
                        lastOpenMarketWatch = selectedFile;

                    };
                    openCTRLOToolStripMenuItem.DropDownItems.Add(menuItem);
                }
            }
            catch (DirectoryNotFoundException)
            {
                // Clear existing menu items
                openCTRLOToolStripMenuItem.DropDownItems.Clear();
                // Add Default menu item with click handler
                ToolStripMenuItem defaultMenuItem = new ToolStripMenuItem("Default");
                defaultMenuItem.Click += (sender, e) =>
                {
                    var clickedItem = (ToolStripMenuItem)sender;
                    DefaultToolStripMenuItem_Click(sender, e);
                    MenuLoad();
                    addEditSymbolsToolStripMenuItem.Enabled = false;
                    saveFileName = null;
                    SetActiveMenuItem(clickedItem);
                    saveMarketWatchHost.Visible = false;
                    titleLabel.Text = "DEFAULT";
                    lastOpenMarketWatch = "Default";
                };
                defaultMenuItem.Enabled = false;
                openCTRLOToolStripMenuItem.DropDownItems.Add(defaultMenuItem);
            }
            catch (Exception)
            {

            }
        }

        private void SetActiveMenuItem(ToolStripMenuItem activeItem)
        {
            foreach (ToolStripMenuItem item in openCTRLOToolStripMenuItem.DropDownItems)
            {
                item.Enabled = (item != activeItem);
                if (item.Text == activeItem.Text)
                    item.Enabled = false;
                else
                    item.Enabled = true;
            }
        }

        public void LoadSymbol(string Filename)
        {

            try
            {
                //MessageBox.Show($"Showing {Filename} File...","File Open",MessageBoxButtons.OK,MessageBoxIcon.Information);
                selectedSymbols.Clear();
                Filename = Path.Combine(AppFolder, Filename);
                string cipherText = File.ReadAllText(Filename);
                string json = CryptoHelper.Decrypt(cipherText, EditableMarketWatchGrid.passphrase);
                var symbols = JsonSerializer.Deserialize<List<string>>(json);
                selectedSymbols.AddRange(symbols);
                isLoadedSymbol = true;
            }
            catch (Exception)
            {
                MessageBox.Show("File Was Never Save Or Moved Please Try Again!", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            LiveRateGrid();

            MenuLoad();

        }
        #endregion

        #region Excel Methods
        public void ExportExcelOnClick()
        {
            // Run Excel operations in a separate thread with unsafe queuing
            ThreadPool.QueueUserWorkItem(_ =>
            {
                // Ensure documents folder exists
                Directory.CreateDirectory(Path.GetDirectoryName(excelFilePath));

                try
                {
                    // If file doesn't exist, create it with headers
                    if (!File.Exists(excelFilePath))
                    {
                        Excel.Application tempApp = null;
                        Excel.Workbook tempWorkbook = null;
                        Excel.Worksheet tempWorksheet = null;

                        try
                        {
                            tempApp = new Excel.Application();
                            tempWorkbook = tempApp.Workbooks.Add();
                            tempWorksheet = (Excel.Worksheet)tempWorkbook.Sheets[1];
                            tempWorksheet.Name = "Sheet1";

                            // Write headers
                            for (int col = 0; col < marketDataTable.Columns.Count; col++)
                            {
                                tempWorksheet.Cells[1, col + 1] = marketDataTable.Columns[col].ColumnName;
                            }

                            // Save and close
                            tempWorkbook.SaveAs(excelFilePath);
                            tempWorkbook.Close(false);
                            tempApp.Quit();
                        }
                        finally
                        {
                            // Proper cleanup in reverse order
                            if (tempWorksheet != null) Marshal.ReleaseComObject(tempWorksheet);
                            if (tempWorkbook != null) Marshal.ReleaseComObject(tempWorkbook);
                            if (tempApp != null) Marshal.ReleaseComObject(tempApp);

                            // Force garbage collection
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            GC.Collect();
                        }

                        Helper.DesktopShortcut desktopShortcut = new Helper.DesktopShortcut();
                        ModifyRegistry();
                    }


                    // Open the file using Excel interop
                    excelApp = new Excel.Application
                    {
                        Visible = true,
                        DisplayAlerts = false, // Prevent Excel alerts from stealing focus
                        UserControl = true, // Set Excel to run in background
                        Interactive = true,
                        IgnoreRemoteRequests = true,
                    };

                    workbook = excelApp.Workbooks.Open(excelFilePath);
                    worksheet = (Excel.Worksheet)workbook.Sheets[1];

                    // Flush any data collected so far
                    RefreshExcelFromDataTable(marketDataTable);
                }
                catch (Exception ex)
                {
                    // Note: Need to marshal this back to UI thread if you want to show it in UI
                    Console.WriteLine("Excel export error: " + ex.Message);
                }
            }, null);
        }

        private void ExportToXSLXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportExcelOnClick();
        }

        private void RefreshExcelFromDataTable(System.Data.DataTable data) =>
            // Run Excel operations in a background thread to prevent UI freezing
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {

                bool fileopen = CommonClass.IsFileLocked(excelFilePath);
                if (fileopen && (workbook == null || worksheet == null))
                {
                    try
                    {
                        // Try to get running Excel instance
                        try
                        {
                            excelApp = GetRunningExcelInstance();
                            if (excelApp != null)
                            {
                                excelApp.UserControl = true; // Prevent Excel from taking focus
                                excelApp.DisplayAlerts = false; // Suppress Excel alerts
                                excelApp.IgnoreRemoteRequests = true; // Ignore Request of File Open/Write in same Instance
                                ((Excel.AppEvents_Event)excelApp).NewWorkbook += ExcelApp_NewWorkbook;
                            }
                        }
                        catch (COMException)
                        {
                            Console.WriteLine("Excel is not running.");
                            return;
                        }

                        if (excelApp == null)
                        {
                            Console.WriteLine("Excel is not running.");
                            return;
                        }

                        // Get the active workbook
                        workbook = excelApp.ActiveWorkbook;

                        if (workbook == null)
                        {
                            Console.WriteLine("No workbook is currently open.");
                            return;
                        }

                        // Get "Sheet1"
                        worksheet = workbook.Sheets["Sheet1"] as Excel.Worksheet;

                        if (worksheet == null)
                        {
                            Console.WriteLine("Sheet1 not found.");
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        workbook = null;
                        worksheet = null;
                        return;
                    }
                }

                if (data == null || workbook == null || worksheet == null)
                {
                    return;
                }

                if (workbook == null || worksheet == null || fileopen == false)
                {
                    CleanupExcelResources();
                    return;
                }

                try
                {
                    excelApp.IgnoreRemoteRequests = true;
                    List<(Excel.Range cell, System.Drawing.Color color)> symbolCellsToColor = new List<(Excel.Range, System.Drawing.Color)>();

                    // Validate workbook
                    string workbookName = workbook.FullName;
                    if (!workbookName.Contains("Live Rate.xlsx"))
                    {
                        CleanupExcelResources();
                        return;
                    }

                    int rowCount = data.Rows.Count;
                    int colCount = data.Columns.Count;

                    // 1. Read existing values BEFORE overwriting (for comparison)
                    object[,] oldValues = null;
                    if (rowCount > 0)
                    {
                        Excel.Range readRange = worksheet.Range[
                            worksheet.Cells[2, 1],
                            worksheet.Cells[1 + rowCount, colCount]  // Start at row 2, include rowCount rows
                        ];
                        oldValues = (object[,])readRange.Value2;
                        Marshal.ReleaseComObject(readRange);
                    }

                    // 2. Format headers and column A
                    Excel.Range columnA = worksheet.Range["A:A"];
                    columnA.Font.Bold = true;
                    Marshal.ReleaseComObject(columnA); // Release immediately


                    if (!_headersWritten)
                    {
                        // Write headers once
                        for (int col = 0; col < data.Columns.Count; col++)
                        {
                            worksheet.Cells[1, col + 1].Value2 = data.Columns[col].ColumnName;
                        }
                        _headersWritten = true;
                    }



                    // 3. Bulk write new data (if exists)
                    if (rowCount > 0)
                    {
                        // Prepare data array
                        object[,] dataArray = new object[rowCount, colCount];
                        for (int r = 0; r < rowCount; r++)
                        {
                            for (int c = 0; c < colCount; c++)
                            {
                                if (c == colCount - 1) // Last column (date-time)
                                {
                                    // Try to parse the value as DateTime
                                    if (DateTime.TryParse(data.Rows[r][c]?.ToString(), out DateTime dateValue))
                                    {
                                        dataArray[r, c] = dateValue; // Store as DateTime for Excel
                                    }
                                    else
                                    {
                                        dataArray[r, c] = data.Rows[r][c]; // Fallback to original value
                                        Console.WriteLine($"[⚠️ Warning]: Could not parse date-time in row {r + 1}, column {c + 1}: {data.Rows[r][c]}");
                                    }
                                }
                                else
                                {
                                    dataArray[r, c] = data.Rows[r][c]; // Other columns unchanged
                                }
                            }
                        }

                        // Write to worksheet in single operation
                        Excel.Range writeRange = worksheet.Range[
                            worksheet.Cells[2, 1],
                            worksheet.Cells[1 + rowCount, colCount]  // 2 + rowCount - 1 = 1 + rowCount
                        ];
                        writeRange.Value2 = dataArray;

                        // Apply date-time format to the last column
                        Excel.Range lastColumnRange = worksheet.Range[
                            worksheet.Cells[2, colCount],
                            worksheet.Cells[1 + rowCount, colCount]
                        ];
                        lastColumnRange.NumberFormat = "dd/mm/yyyy hh:mm:ss";

                        Marshal.ReleaseComObject(lastColumnRange); // Release immediately
                        Marshal.ReleaseComObject(writeRange); // Release immediately

                        // 4. Apply color formatting to changed values
                        // Store the target ranges for Red and Green colors
                        List<Excel.Range> redCells = new List<Excel.Range>();
                        List<Excel.Range> greenCells = new List<Excel.Range>();

                        for (int r = 0; r < rowCount; r++)
                        {
                            for (int c = 1; c < colCount - 1; c++)  // Skip first/last columns
                            {
                                // Get old/new values with bounds checking
                                object oldVal = (oldValues != null &&
                                                 (r + 1) < oldValues.GetLength(0) &&
                                                 (c + 1) < oldValues.GetLength(1))
                                    ? oldValues[r + 1, c + 1]  // Excel arrays are 1-based
                                    : null;

                                object newVal = data.Rows[r][c];

                                // Handle numeric comparisons
                                if (decimal.TryParse(oldVal?.ToString(), out decimal oldDecimal) &&
                                    decimal.TryParse(newVal?.ToString(), out decimal newDecimal))
                                {
                                    if (newDecimal > oldDecimal)
                                        greenCells.Add(worksheet.Cells[2 + r, c + 1]);
                                    else if (newDecimal < oldDecimal)
                                        redCells.Add(worksheet.Cells[2 + r, c + 1]);
                                }
                            }


                            // Symbol and Ask logic
                            string symbol = data.Rows[r][0]?.ToString() ?? "";
                            object askVal = data.Rows[r][2]; // Ask column = index 2

                            string arrow = "";
                            System.Drawing.Color arrowColor = System.Drawing.Color.Black;

                            if (askVal != DBNull.Value && decimal.TryParse(askVal.ToString(), out decimal currentAsk))
                            {
                                if (previousAsks.TryGetValue(symbol, out decimal prevAsk))
                                {
                                    if (currentAsk > prevAsk)
                                    {
                                        arrow = " ▲";
                                        arrowColor = System.Drawing.Color.Green;
                                    }
                                    else if (currentAsk < prevAsk)
                                    {
                                        arrow = " ▼";
                                        arrowColor = System.Drawing.Color.Red;
                                    }
                                }
                                previousAsks[symbol] = currentAsk;
                            }

                            // Update Symbol column (column A, index 0)
                            dataArray[r, 0] = symbol + arrow;

                            if (!string.IsNullOrEmpty(arrow))
                            {
                                Excel.Range symbolCell = worksheet.Cells[2 + r, 1]; // Row index in Excel starts at 2, Column A = 1
                                symbolCellsToColor.Add((symbolCell, arrowColor));
                            }
                        }

                        // Apply colors in batches
                        if (greenCells.Any())
                        {
                            foreach (var cell in greenCells)
                            {
                                cell.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Green);
                                Marshal.ReleaseComObject(cell); // Release each cell immediately after use
                            }
                        }

                        if (redCells.Any())
                        {
                            foreach (var cell in redCells)
                            {
                                cell.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
                                Marshal.ReleaseComObject(cell); // Release each cell immediately after use
                            }
                        }

                        // Clear the lists to avoid reusing released COM objects
                        greenCells.Clear();
                        redCells.Clear();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[❌ Excel write failed]: {ex.Message}");
                }

            });

        public void SymbolExportToExcel() =>
    System.Threading.ThreadPool.QueueUserWorkItem(_ =>
    {
        try
        {
            bool fileopen = CommonClass.IsFileLocked(excelFilePath);
            if (fileopen && (workbook == null || worksheet == null))
            {
                // Try to get running Excel instance
                try
                {
                    excelApp = GetRunningExcelInstance();
                    if (excelApp != null)
                    {
                        // Set all properties in one go
                        excelApp.UserControl = true; // Prevent Excel from taking focus
                        excelApp.DisplayAlerts = false; // Suppress Excel alerts
                        excelApp.IgnoreRemoteRequests = true; // Ignore Request of File Open/Write in same Instance
                        ((Excel.AppEvents_Event)excelApp).NewWorkbook += ExcelApp_NewWorkbook;
                    }
                }
                catch (COMException)
                {
                    Console.WriteLine("Excel is not running.");
                    return;
                }

                if (excelApp == null)
                {
                    Console.WriteLine("Excel is not running.");
                    return;
                }

                // Get the active workbook
                workbook = excelApp.ActiveWorkbook;

                if (workbook == null)
                {
                    Console.WriteLine("No workbook is currently open.");
                    return;
                }

                // Get "Sheet1"
                worksheet = workbook.Sheets["Sheet1"] as Excel.Worksheet;

                if (worksheet == null)
                {
                    Console.WriteLine("Sheet1 not found.");
                    return;
                }
            }

            if (marketDataTable == null || workbook == null || worksheet == null || dataGridView1 == null)
            {
                return;
            }

            if (workbook == null || worksheet == null || fileopen == false)
            {
                CleanupExcelResources();
                return;
            }


            // Prepare data in memory first
            int columnCount = dataGridView1.Columns.Count;
            int rowCount = dataGridView1.Rows.Count;
            if (dataGridView1.AllowUserToAddRows && rowCount > 0 &&
                dataGridView1.Rows[rowCount - 1].IsNewRow)
            {
                rowCount--;
            }

            // Write headers in one operation
            object[,] headers = new object[1, columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                headers[0, i] = dataGridView1.Columns[i].HeaderText;
            }

            Excel.Range headerRange = worksheet.Range[
                worksheet.Cells[1, 1],
                worksheet.Cells[1, columnCount]];
            headerRange.Value = headers;
            headerRange.Font.Bold = true;

            // Clear old data (except headers and preserved rows)
            Excel.Range usedRange = worksheet.UsedRange;
            if (usedRange != null && usedRange.Rows.Count > 1 + rowCount)
            {
                int firstRowToClear = 2 + rowCount;
                int lastRowInSheet = usedRange.Rows.Count;

                Excel.Range rowsToClear = worksheet.Range[
                    worksheet.Cells[firstRowToClear, 1],
                    worksheet.Cells[lastRowInSheet, usedRange.Columns.Count]];

                rowsToClear.Clear();
            }

            // Prepare data for bulk write
            object[,] data = new object[rowCount, columnCount];
            List<Excel.Range> coloredCells = new List<Excel.Range>();
            List<Excel.Range> rightAlignedCells = new List<Excel.Range>();
            List<Excel.Range> leftAlignedCells = new List<Excel.Range>();
            List<Excel.Range> numberCells = new List<Excel.Range>();

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    DataGridViewCell dgvCell = dataGridView1.Rows[i].Cells[j];
                    object value = dgvCell.Value;
                    data[i, j] = value;

                    // Track cells that need special formatting
                    Excel.Range cell = (Excel.Range)worksheet.Cells[i + 2, j + 1];

                    if (dgvCell.Style.ForeColor == System.Drawing.Color.Green)
                    {
                        coloredCells.Add(cell);
                        cell.Font.Color = Excel.XlRgbColor.rgbGreen;
                    }
                    else if (dgvCell.Style.ForeColor == System.Drawing.Color.Red)
                    {
                        coloredCells.Add(cell);
                        cell.Font.Color = Excel.XlRgbColor.rgbRed;
                    }
                    else
                    {
                        coloredCells.Add(cell);
                        cell.Font.Color = Excel.XlRgbColor.rgbBlack;
                    }

                    if (dgvCell.Style.Alignment == DataGridViewContentAlignment.MiddleRight)
                    {
                        rightAlignedCells.Add(cell);
                    }
                    else if (dgvCell.Style.Alignment == DataGridViewContentAlignment.MiddleLeft)
                    {
                        leftAlignedCells.Add(cell);
                    }

                    if (value != null && (value is double || value is decimal || value is int))
                    {
                        numberCells.Add(cell);
                    }
                }
            }

            // Bulk write data
            if (rowCount > 0)
            {
                Excel.Range dataRange = worksheet.Range[
                    worksheet.Cells[2, 1],
                    worksheet.Cells[rowCount + 1, columnCount]];
                dataRange.Value = data;
            }

            // Apply formatting in bulk where possible
            if (rightAlignedCells.Count > 0)
            {
                worksheet.Range[rightAlignedCells.ToArray()].HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            }
            if (leftAlignedCells.Count > 0)
            {
                worksheet.Range[leftAlignedCells.ToArray()].HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
            }
            if (numberCells.Count > 0)
            {
                worksheet.Range[numberCells.ToArray()].NumberFormat = "0.00";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exporting to Excel: {ex.Message}");
        }
    });

        private void ExcelApp_NewWorkbook(Excel.Workbook wb)
        {
            // Close the newly created workbook immediately
            wb.Close(false);  // false = don't save changes
            excelApp.StatusBar = "New workbook creation is disabled";
            Console.WriteLine("New workbook creation is disabled.");
        }

        private Excel.Application GetRunningExcelInstance()
        {

            // Get Workbook By Moniker
            dynamic tempWorkbook = Marshal.BindToMoniker(excelFilePath);
            Excel.Application excelAppTemp = tempWorkbook.Application;
            Console.WriteLine($"Found Excel instance with PID: {excelAppTemp.Hwnd}");
            if (excelAppTemp != null)
            {
                excelAppTemp.IgnoreRemoteRequests = true;
                ((Excel.AppEvents_Event)excelAppTemp).NewWorkbook += ExcelApp_NewWorkbook;
                //Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;
                return excelAppTemp;
            }

            // Get all running Excel processes
            var excelProcesses = System.Diagnostics.Process.GetProcessesByName("EXCEL");

            if (excelProcesses.Length == 0)
            {
                Console.WriteLine("No Excel instances are running.");
                return null;
            }
            foreach (var process in excelProcesses)
            {
                try
                {
                    // Get the Excel application object for this process
                    Guid clsid = new Guid("00024500-0000-0000-C000-000000000046");
                    GetActiveObject(ref clsid, IntPtr.Zero, out object obj);

                    if (obj is Excel.Application TempexcelApp)
                    {
                        Console.WriteLine($"Checking Excel instance with PID: {process.Id}");

                        // Check workbooks in this instance
                        foreach (Excel.Workbook workbook in TempexcelApp.Workbooks)
                        {
                            if (workbook.Name.Equals("Live Rate.xlsx", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"Found workbook in instance PID: {process.Id}");
                                TempexcelApp.IgnoreRemoteRequests = true;
                                ((Excel.AppEvents_Event)TempexcelApp).NewWorkbook += ExcelApp_NewWorkbook;
                                //Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;
                                return TempexcelApp; // Return the instance with the workbook
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accessing Excel instance: {ex.Message}");
                    continue;
                }
            }
            Console.WriteLine("No instance found with the workbook 'Live Rate.xlsx'");
            return null;
        }

        private void ModifyRegistry()
        {
            string keyPath = @"Software\Classes\Excel.Sheet.12\shell\Open\command";
            string value = "\"C:\\Program Files\\Microsoft Office\\Root\\Office16\\EXCEL.EXE\" /x \"%1\"";

            try
            {
                // Get current user identity
                string user = WindowsIdentity.GetCurrent().Name;

                // Create permission rule
                RegistrySecurity security = new RegistrySecurity();
                security.AddAccessRule(new RegistryAccessRule(
                    user,
                    RegistryRights.FullControl,
                    InheritanceFlags.None,
                    PropagationFlags.None,
                    AccessControlType.Allow
                ));

                // Create or open the key with custom security
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(
                    keyPath,
                    RegistryKeyPermissionCheck.ReadWriteSubTree,
                    security))
                {
                    key.SetValue("", value);
                    Console.WriteLine("Registry updated with permission.");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("You must run this program as administrator.");
            }
        }

        private void CleanupExcelResources()
        {
            try
            {
                // Release in reverse order of creation
                if (worksheet != null)
                {
                    Marshal.FinalReleaseComObject(worksheet);
                    worksheet = null;
                }

                if (workbook != null)
                {
                    Marshal.FinalReleaseComObject(workbook);
                    workbook = null;
                }


                if (excelApp != null)
                {
                    try
                    {
                        if (excelApp.Workbooks.Count == 0)
                            excelApp.Quit();
                    }
                    catch { }
                    Marshal.ReleaseComObject(excelApp);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Cleanup Error] {ex.Message}");
            }
            finally
            {

                if (excelApp != null)
                {
                    try
                    {
                        if (excelApp.Workbooks.Count == 0)
                            excelApp.Quit();
                    }
                    catch { }
                    Marshal.ReleaseComObject(excelApp);
                }

                foreach (var process in Process.GetProcessesByName("EXCEL"))
                {
                    try
                    {
                        // Only kill processes with no visible window (background processes)
                        if (string.IsNullOrEmpty(process.MainWindowTitle))
                        {
                            process.Kill();
                            process.WaitForExit(1000); // Wait up to 1 second
                        }
                    }
                    catch
                    {
                        // Ignore any errors (process already closed, access denied, etc.)
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }

            }
        }
        #endregion

        #region Socket
        private async void InitializeSocket()
        {


            socket = new SocketIO("https://excel.starlineapi.in:1008", new SocketIOOptions
            {
                Reconnection = true,
                ReconnectionAttempts = int.MaxValue,
                ReconnectionDelay = 1000,
                RandomizationFactor = 0.5,
                EIO = 4 // Use Engine.IO protocol version 4
            });


            socket.OnConnected += async (sender, e) =>
            {
                Console.WriteLine("✅ Connected to server");
                await socket.EmitAsync("client", "starline");

                UpdateUI(() =>
                {
                    IsConnected = true;
                    statusLabel.Text = "Connected to server";
                    connectToolStripMenuItem.Enabled = false;
                    disconnectToolStripMenuItem.Enabled = true;
                });
            };

            socket.OnDisconnected += (sender, e) =>
            {
                Console.WriteLine("❌ Disconnected from server");

                UpdateUI(() =>
                {
                    IsConnected = false;
                    statusLabel.Text = "Disconnected";

                    connectToolStripMenuItem.Enabled = true;
                    disconnectToolStripMenuItem.Enabled = false;
                });
            };

            socket.OnError += (sender, e) =>
            {
                Console.WriteLine($"⚠️ Socket error: {e}");

                UpdateUI(() =>
                {
                    statusLabel.Text = $"Error: {e}";

                    connectToolStripMenuItem.Enabled = true;
                    disconnectToolStripMenuItem.Enabled = false;
                });
            };


            try
            {

                socket.On("excelRate", response =>
                {
                    try
                    {
                        var json = response.GetValue().ToString();
                        var jsonArray = new JsonArray();
                        try
                        {
                            jsonArray = JsonNode.Parse(json)?.AsArray();

                        }
                        catch (Exception)
                        {
                            jsonArray = null;
                        }
                        if (jsonArray == null) return;


                        lock (tableLock)
                        {
                            if (marketDataTable == null) return; // safety check

                            marketDataTable.Clear();

                            foreach (var item in jsonArray)
                            {
                                var row = marketDataTable.NewRow();

                                row["Symbol"] = item["Symbol"]?.ToString();
                                row["Bid"] = item["Bid"]?.ToString();
                                row["Ask"] = item["Ask"]?.ToString();
                                row["High"] = item["High"]?.ToString();
                                row["Low"] = item["Low"]?.ToString();
                                row["Open"] = item["Open"]?.ToString();
                                row["Close"] = item["Close"]?.ToString();
                                row["LTP"] = item["LTP"]?.ToString();
                                row["DateTime"] = item["DateTime"]?.ToString();

                                marketDataTable.Rows.Add(row);
                            }

                            // ✅ Populate symbolMaster only once
                            if (!isSymbolMasterInitialized)
                            {
                                symbolMaster = marketDataTable.AsEnumerable()
                                                    .Select(r => r.Field<string>("Symbol"))
                                                    .Distinct()
                                                    .ToList();

                                isSymbolMasterInitialized = true;
                            }

                        }

                        //// Update UI safely
                        //UpdateGrid();

                        // Update UI safely
                        UpdateUI(() =>
                        {
                            UpdateGrid();
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("❌ Error processing data: " + ex.Message);
                    }
                });


            }
            catch (Exception)
            {
                InitializeSocket();
            }

            try
            {
                await socket.ConnectAsync();
                Console.ReadLine(); // Keep the app running
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Connection error: " + ex.Message);
            }
        }
        #endregion

        #region GridView

        private void UpdateGrid()
        {
            if (dataGridView1.InvokeRequired)
            {
                dataGridView1.BeginInvoke(new System.Action(UpdateGridInternal));
            }
            else
            {
                UpdateGridInternal();
            }
        }

        private void InitializeDataGridView()
        {

            // Clear existing columns if any
            dataGridView1.Columns.Clear();

            // Add columns from the list
            foreach (string columnName in allColumns)
            {
                DataGridViewColumn column = new DataGridViewTextBoxColumn
                {
                    Name = columnName,
                    HeaderText = columnName,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    ReadOnly = true
                };

               

                dataGridView1.Columns.Add(column);
            }

            // Enable double buffering for better performance
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.SetProperty,
                null, dataGridView1, new object[] { true });


        }

        private void UpdateGridInternal()
        {
            int FixedRowCount = 17;

            if (marketDataTable == null)
                return; // Or handle accordingly

            var rows = marketDataTable.Rows.Cast<DataRow>().ToList();

            bool symbolRowUpdate = false;

            // Clean up symbols not in selectedSymbols
            foreach (DataRow row in rows)
            {
                if (row == null || row.RowState == DataRowState.Deleted || row.RowState == DataRowState.Detached)
                    continue;

                if (isLoadedSymbol)
                {
                    string symbol = row[0]?.ToString();
                    if (!selectedSymbols.Contains(symbol))
                    {
                        row.Delete();
                        FixedRowCount--;
                        continue;
                    }
                }
            }

            if (dataGridView1.IsDisposed) return;

            dataGridView1.SuspendLayout();
            try
            {


                // Ensure columns exist and style them
                if (dataGridView1.Columns.Count == 0)
                {
                    InitializeDataGridView();

                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells; // or None

                }

                // Apply header and cell styles only once
                var headerFont = new System.Drawing.Font("Segoe UI", fontSize + 2, FontStyle.Bold);
                var cellFont = new System.Drawing.Font("Segoe UI", fontSize, FontStyle.Regular);
                var symbolFont = new System.Drawing.Font("Segoe UI", fontSize, FontStyle.Bold);

                dataGridView1.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = headerFont
                };

                dataGridView1.ColumnHeadersHeight = (int)Math.Ceiling((fontSize + 2) * 3.0);

                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    if (column.Index == 0)
                    {
                        column.DefaultCellStyle = new DataGridViewCellStyle
                        {
                            Alignment = DataGridViewContentAlignment.MiddleLeft,
                            Font = symbolFont
                        };
                    }
                    else
                    {
                        column.DefaultCellStyle = new DataGridViewCellStyle
                        {
                            Alignment = DataGridViewContentAlignment.MiddleRight,
                            Font = cellFont
                        };
                    }
                }

                // Maintain exact row count
                while (dataGridView1.Rows.Count < FixedRowCount)
                    dataGridView1.Rows.Add();
                while (dataGridView1.Rows.Count > FixedRowCount)
                    dataGridView1.Rows.RemoveAt(dataGridView1.Rows.Count - 1);

                int rowsToUpdate = Math.Min(FixedRowCount, marketDataTable.Rows.Count);

                for (int i = 0; i < rowsToUpdate; i++)
                {
                    for (int j = 0; j < dataGridView1.Columns.Count; j++)
                    {
                        if (j == 0)
                        {
                            string symbol = marketDataTable.Rows[i][j]?.ToString() ?? "";
                            int askColumnIndex = 2; // Update accordingly
                            object askValueObj = marketDataTable.Rows[i][askColumnIndex];

                            string arrow = "";
                            System.Drawing.Color arrowColor = System.Drawing.Color.Black;

                            if (askValueObj != DBNull.Value && decimal.TryParse(askValueObj.ToString(), out decimal currentAsk))
                            {
                                if (previousAsks.TryGetValue(symbol, out decimal previousAsk))
                                {
                                    if (currentAsk > previousAsk)
                                    {
                                        arrow = " ▲";
                                        arrowColor = System.Drawing.Color.Green;
                                    }
                                    else if (currentAsk < previousAsk)
                                    {
                                        arrow = " ▼";
                                        arrowColor = System.Drawing.Color.Red;
                                    }
                                }

                                previousAsks[symbol] = currentAsk;
                            }

                            var symbolCell = dataGridView1.Rows[i].Cells[j];
                            symbolCell.Value = symbol + arrow;
                            symbolCell.Style = new DataGridViewCellStyle
                            {
                                Alignment = DataGridViewContentAlignment.MiddleLeft,
                                ForeColor = arrowColor,
                                Font = symbolFont
                            };
                            continue;
                        }

                        // Skip last column for special handling (e.g. timestamp?)
                        if (j == dataGridView1.Columns.Count - 1)
                        {
                            dataGridView1.Rows[i].Cells[j].Value = marketDataTable.Rows[i][j]?.ToString();
                            dataGridView1.Rows[i].Cells[j].Style = new DataGridViewCellStyle
                            {
                                Alignment = DataGridViewContentAlignment.MiddleLeft,
                                ForeColor = System.Drawing.Color.Black,
                                Font = cellFont
                            };
                            continue;
                        }

                        object currentValueObj = dataGridView1.Rows[i].Cells[j].Value;
                        string currentValueStr = currentValueObj?.ToString() ?? string.Empty;
                        object value = marketDataTable.Rows[i][j];

                        if (value != DBNull.Value && double.TryParse(value.ToString(), out double number))
                        {
                            dataGridView1.Rows[i].Cells[j].Value = number.ToString("F2");
                        }
                        else
                        {
                            dataGridView1.Rows[i].Cells[j].Value = string.Empty;
                        }

                        var cellStyle = new DataGridViewCellStyle
                        {
                            Alignment = DataGridViewContentAlignment.MiddleRight,
                            Font = cellFont
                        };

                        if (decimal.TryParse(currentValueStr, out decimal currentDecimal) &&
                            decimal.TryParse(value?.ToString(), out decimal newDecimal))
                        {
                            if (newDecimal > currentDecimal)
                                cellStyle.ForeColor = System.Drawing.Color.Green;
                            else if (newDecimal < currentDecimal)
                                cellStyle.ForeColor = System.Drawing.Color.Red;
                            else
                                cellStyle.ForeColor = System.Drawing.Color.Black;
                        }

                        dataGridView1.Rows[i].Cells[j].Style = cellStyle;
                    }
                    dataGridView1.Rows[i].Height = (int)Math.Ceiling((fontSize) * 2.7);
                }

                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

                // Handle symbol column fixed width
                if (symbolColumnFixedWidth == 0 && rowsToUpdate > 0)
                {
                    int maxSymbolWidth = 0;
                    using (Graphics g = dataGridView1.CreateGraphics())
                    {
                        for (int i = 0; i < rowsToUpdate; i++)
                        {
                            var cell = dataGridView1.Rows[i].Cells[0];
                            var text = cell.Value?.ToString() ?? "";
                            System.Drawing.Size textSize = TextRenderer.MeasureText(text, symbolFont);
                            maxSymbolWidth = Math.Max(maxSymbolWidth, textSize.Width);
                        }
                        maxSymbolWidth += 20; // padding
                    }

                    symbolColumnFixedWidth = maxSymbolWidth;
                    if (dataGridView1.Columns.Count > 0)
                    {
                        dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                        dataGridView1.Columns[0].Width = symbolColumnFixedWidth;
                    }
                }
                else if (symbolColumnFixedWidth > 0 && dataGridView1.Columns.Count > 0)
                {
                    dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                    dataGridView1.Columns[0].Width = symbolColumnFixedWidth;
                }

                // Clear extra rows
                for (int i = rowsToUpdate; i < FixedRowCount; i++)
                {
                    for (int j = 0; j < dataGridView1.Columns.Count; j++)
                    {
                        if (dataGridView1.Rows[i].Cells[j].Value != null)
                        {
                            dataGridView1.Rows[i].Cells[j].Value = DBNull.Value;
                            dataGridView1.Rows[i].Cells[j].Style = dataGridView1.DefaultCellStyle;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (!symbolRowUpdate && !dataGridView1.ReadOnly && isLoadedSymbol)
                    symbolRowUpdate = true;

                dataGridView1.ResumeLayout();

                if (!isLoadedSymbol)
                    RefreshExcelFromDataTable(marketDataTable);
                else
                    SymbolExportToExcel();
            }
        }

        protected void InitializeDataTable()
        {
            if (marketDataTable == null)
                marketDataTable = new System.Data.DataTable();

            if (marketDataTable.Columns.Count == 0)
            {
                marketDataTable.Columns.Add("Symbol");
                marketDataTable.Columns.Add("Bid");
                marketDataTable.Columns.Add("Ask");
                marketDataTable.Columns.Add("High");
                marketDataTable.Columns.Add("Low");
                marketDataTable.Columns.Add("Open");
                marketDataTable.Columns.Add("Close");
                marketDataTable.Columns.Add("LTP");
                marketDataTable.Columns.Add("DateTime");

            }
        }

        #endregion
        private void addEditSymbolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Create panel if it hasn't been initialized yet
            if (panelAddSymbols == null)
            {
                // Initialize panel
                panelAddSymbols = new Panel
                {
                    Size = new System.Drawing.Size(500, 500),
                    BackColor = System.Drawing.Color.White,
                    BorderStyle = BorderStyle.None,
                    Visible = false,
                    Padding = new Padding(20),
                };

                panelAddSymbols.Paint += (s2, e2) =>
                {
                    ControlPaint.DrawBorder(e2.Graphics, panelAddSymbols.ClientRectangle,
                        System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid,
                        System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid,
                        System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid,
                        System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid);
                };

                panelAddSymbols.Location = new System.Drawing.Point(
                    (this.Width - panelAddSymbols.Width) / 2,
                    (this.Height - panelAddSymbols.Height) / 2
                );

                // Title label
                System.Windows.Forms.Label titleLabel = new System.Windows.Forms.Label
                {
                    Text = "🔄 Add / Edit Symbols",
                    Font = new System.Drawing.Font("Segoe UI Semibold", 16, FontStyle.Bold),
                    ForeColor = System.Drawing.Color.FromArgb(50, 50, 50),
                    Dock = DockStyle.Top,
                    Height = 50,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Padding = new Padding(0, 10, 0, 10)
                };

                // CheckedListBox
                checkedListSymbols = new CheckedListBox
                {
                    Height = 320,
                    Dock = DockStyle.Top,
                    Font = new System.Drawing.Font("Segoe UI", 10),
                    BorderStyle = BorderStyle.FixedSingle,
                    CheckOnClick = true,
                    BackColor = System.Drawing.Color.White
                };

                // Button container
                Panel buttonPanel = new Panel
                {
                    Height = 80,
                    Dock = DockStyle.Bottom,
                    Padding = new Padding(10),
                    BackColor = System.Drawing.Color.White
                };

                // Buttons
                btnSelectAllSymbols = new System.Windows.Forms.Button
                {
                    Text = "Select All",
                    Height = 40,
                    Width = 120,
                    BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                    ForeColor = System.Drawing.Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnSelectAllSymbols.FlatAppearance.BorderSize = 0;

                btnConfirmAddSymbols = new System.Windows.Forms.Button
                {
                    Text = "✔ Save",
                    Height = 40,
                    Width = 120,
                    BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                    ForeColor = System.Drawing.Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnConfirmAddSymbols.FlatAppearance.BorderSize = 0;

                btnCancelAddSymbols = new System.Windows.Forms.Button
                {
                    Text = "✖ Cancel",
                    Height = 40,
                    Width = 120,
                    BackColor = System.Drawing.Color.LightGray,
                    ForeColor = System.Drawing.Color.Black,
                    FlatStyle = FlatStyle.Flat,
                    Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnCancelAddSymbols.FlatAppearance.BorderSize = 0;

                // Layout
                btnSelectAllSymbols.Left = 30;
                btnConfirmAddSymbols.Left = 170;
                btnCancelAddSymbols.Left = 310;

                buttonPanel.Controls.Add(btnSelectAllSymbols);
                buttonPanel.Controls.Add(btnConfirmAddSymbols);
                buttonPanel.Controls.Add(btnCancelAddSymbols);

                panelAddSymbols.Controls.Add(checkedListSymbols);
                panelAddSymbols.Controls.Add(buttonPanel);
                panelAddSymbols.Controls.Add(titleLabel);

                this.Controls.Add(panelAddSymbols);

                this.Resize += (s3, e3) =>
                {
                    panelAddSymbols.Location = new System.Drawing.Point(
                        (this.Width - panelAddSymbols.Width) / 2,
                        (this.Height - panelAddSymbols.Height) / 2
                    );
                };

                // Hook up events

                btnSelectAllSymbols.Click += (s, e2) =>
                {
                    bool allChecked = true;
                    for (int i = 0; i < checkedListSymbols.Items.Count; i++)
                    {
                        if (!checkedListSymbols.GetItemChecked(i))
                        {
                            allChecked = false;
                            break;
                        }
                    }

                    bool check = !allChecked;
                    btnSelectAllSymbols.Text = check ? "Unselect All" : "Select All";

                    for (int i = 0; i < checkedListSymbols.Items.Count; i++)
                    {
                        checkedListSymbols.SetItemChecked(i, check);
                    }
                };

                btnConfirmAddSymbols.Click += (s, e2) =>
                {
                    var currentlyChecked = checkedListSymbols.CheckedItems.Cast<string>().ToList();
                    var previouslySelected = selectedSymbols;

                    var addedSymbols = currentlyChecked.Except(previouslySelected).ToList();
                    var removedSymbols = previouslySelected.Except(currentlyChecked).ToList();

                    if (!addedSymbols.Any() && !removedSymbols.Any())
                    {
                        MessageBox.Show("No changes made.");
                        return;
                    }

                    EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;

                    editableMarketWatchGrid.isGrid = false;
                    editableMarketWatchGrid.saveFileName = saveFileName;
                    selectedSymbols = currentlyChecked;
                    editableMarketWatchGrid.SaveSymbols(selectedSymbols);
                    UpdateGrid();

                    panelAddSymbols.Visible = false;
                };

                btnCancelAddSymbols.Click += (s, e2) =>
                {
                    panelAddSymbols.Visible = false;
                };
            }

            // Refresh items before showing
            checkedListSymbols.Items.Clear();

            // Add selected symbols first
            foreach (string symbol in symbolMaster)
            {
                if (selectedSymbols.Contains(symbol))
                {
                    checkedListSymbols.Items.Add(symbol, true);
                }
            }

            // Then unselected symbols
            foreach (string symbol in symbolMaster)
            {
                if (!selectedSymbols.Contains(symbol))
                {
                    checkedListSymbols.Items.Add(symbol, false);
                }
            }

            panelAddSymbols.Visible = true;
            panelAddSymbols.BringToFront();

        }

        private void fontSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            fontSize = Convert.ToInt32(fontSizeComboBox.SelectedItem.ToString());

            EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;
            if (editableMarketWatchGrid != null)
                editableMarketWatchGrid.fontSize = fontSize;
        }

        private void Live_Rate_FormClosed(object sender, FormClosedEventArgs e)
        {
            string lastMarketWatchName = saveFileName ?? "Default";

            // Correct way to call the static method
            CredentialManager.SaveMarketWatchWithColumns(lastMarketWatchName, columnPreferences);

            System.Windows.Forms.Application.Exit();
        }

        public void HandleLastOpenedMarketWatch()
        {
            if (string.IsNullOrEmpty(lastOpenMarketWatch))
                return;

            // Find and click the matching menu item
            foreach (ToolStripMenuItem item in openCTRLOToolStripMenuItem.DropDownItems)
            {
                if (item.Text == lastOpenMarketWatch)
                {
                    item.PerformClick();
                    break;
                }
            }
        }

        private void addEditColumnsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Create panel if it hasn't been initialized yet
            if (panelAddColumns == null)
            {
                // Initialize panel
                panelAddColumns = new Panel
                {
                    Size = new System.Drawing.Size(500, 500),
                    BackColor = System.Drawing.Color.White,
                    BorderStyle = BorderStyle.None,
                    Visible = false,
                    Padding = new Padding(20),
                };

                panelAddColumns.Paint += (s2, e2) =>
                {
                    ControlPaint.DrawBorder(e2.Graphics, panelAddColumns.ClientRectangle,
                        System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid,
                        System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid,
                        System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid,
                        System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid);
                };

                panelAddColumns.Location = new System.Drawing.Point(
                    (this.Width - panelAddColumns.Width) / 2,
                    (this.Height - panelAddColumns.Height) / 2
                );

                // Title label
                System.Windows.Forms.Label titleLabel = new System.Windows.Forms.Label
                {
                    Text = "📊 Add / Edit Columns",
                    Font = new System.Drawing.Font("Segoe UI Semibold", 16, FontStyle.Bold),
                    ForeColor = System.Drawing.Color.FromArgb(50, 50, 50),
                    Dock = DockStyle.Top,
                    Height = 50,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Padding = new Padding(0, 10, 0, 10)
                };

                // CheckedListBox
                checkedListColumns = new CheckedListBox
                {
                    Height = 320,
                    Dock = DockStyle.Top,
                    Font = new System.Drawing.Font("Segoe UI", 10),
                    BorderStyle = BorderStyle.FixedSingle,
                    CheckOnClick = true,
                    BackColor = System.Drawing.Color.White
                };

                // Button container
                Panel buttonPanel = new Panel
                {
                    Height = 80,
                    Dock = DockStyle.Bottom,
                    Padding = new Padding(10),
                    BackColor = System.Drawing.Color.White
                };

                // Buttons
                btnSelectAllColumns = new System.Windows.Forms.Button
                {
                    Text = "Select All",
                    Height = 40,
                    Width = 120,
                    BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                    ForeColor = System.Drawing.Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnSelectAllColumns.FlatAppearance.BorderSize = 0;

                btnConfirmAddColumns = new System.Windows.Forms.Button
                {
                    Text = "✔ Save",
                    Height = 40,
                    Width = 120,
                    BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                    ForeColor = System.Drawing.Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnConfirmAddColumns.FlatAppearance.BorderSize = 0;

                btnCancelAddColumns = new System.Windows.Forms.Button
                {
                    Text = "✖ Cancel",
                    Height = 40,
                    Width = 120,
                    BackColor = System.Drawing.Color.LightGray,
                    ForeColor = System.Drawing.Color.Black,
                    FlatStyle = FlatStyle.Flat,
                    Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnCancelAddColumns.FlatAppearance.BorderSize = 0;

                // Layout
                btnSelectAllColumns.Left = 30;
                btnConfirmAddColumns.Left = 170;
                btnCancelAddColumns.Left = 310;

                buttonPanel.Controls.Add(btnSelectAllColumns);
                buttonPanel.Controls.Add(btnConfirmAddColumns);
                buttonPanel.Controls.Add(btnCancelAddColumns);

                panelAddColumns.Controls.Add(checkedListColumns);
                panelAddColumns.Controls.Add(buttonPanel);
                panelAddColumns.Controls.Add(titleLabel);

                this.Controls.Add(panelAddColumns);

                this.Resize += (s3, e3) =>
                {
                    panelAddColumns.Location = new System.Drawing.Point(
                        (this.Width - panelAddColumns.Width) / 2,
                        (this.Height - panelAddColumns.Height) / 2
                    );
                };

                // Hook up events
                btnSelectAllColumns.Click += (s, e2) =>
                {
                    bool allChecked = true;
                    for (int i = 0; i < checkedListColumns.Items.Count; i++)
                    {
                        if (!checkedListColumns.GetItemChecked(i))
                        {
                            allChecked = false;
                            break;
                        }
                    }

                    bool check = !allChecked;
                    btnSelectAllColumns.Text = check ? "Unselect All" : "Select All";

                    for (int i = 0; i < checkedListColumns.Items.Count; i++)
                    {
                        checkedListColumns.SetItemChecked(i, check);
                    }
                };

                btnConfirmAddColumns.Click += (s, e2) =>
                {
                    var currentlyChecked = checkedListColumns.CheckedItems.Cast<string>().ToList();
                    var previouslySelected = columnPreferences.Count > 0 ? columnPreferences : allColumns;

                    if (!currentlyChecked.Any())
                    {
                        MessageBox.Show("Please select at least one column.");
                        return;
                    }

                    if (currentlyChecked.SequenceEqual(previouslySelected))
                    {
                        MessageBox.Show("No changes made.");
                        panelAddColumns.Visible = false;
                        return;
                    }

                    // Save the new column preferences
                    columnPreferences = currentlyChecked;

                    panelAddColumns.Visible = false;
                    MessageBox.Show("Columns updated successfully!");

                };

                btnCancelAddColumns.Click += (s, e2) =>
                {
                    panelAddColumns.Visible = false;
                };
            }

            // Refresh items before showing
            checkedListColumns.Items.Clear();

            // Get the columns to display (use allColumns if no preferences set)
            var columnsToShow = columnPreferences.Count > 0 ? columnPreferences : allColumns;

            // Add selected columns first (preserving order)
            foreach (string column in allColumns)
            {
                if (columnsToShow.Contains(column))
                {
                    checkedListColumns.Items.Add(column, true);
                }
            }

            // Then add unselected columns
            foreach (string column in allColumns)
            {
                if (!columnsToShow.Contains(column))
                {
                    checkedListColumns.Items.Add(column, false);
                }
            }

            // Update Select All button text
            btnSelectAllColumns.Text = checkedListColumns.CheckedItems.Count == checkedListColumns.Items.Count
                ? "Unselect All"
                : "Select All";

            panelAddColumns.Visible = true;
            panelAddColumns.BringToFront();
        }
    }
}
