using DocumentFormat.OpenXml.Wordprocessing;
using Live_Rate_Application.Helper;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;

namespace Live_Rate_Application.MarketWatch
{
    public class EditableMarketWatchGrid : DataGridView
    {
        private readonly DataTable marketWatchDatatable = new DataTable();
        private SocketIO socket = null;
        private List<string> symbolMaster = new List<string>();
        private bool isSymbolMasterInitialized = false;
        public List<string> selectedSymbols = new List<string>();
        public int fontSize = 12; // Default font size
        private readonly Helper.Common CommonClass;
        public static EditableMarketWatchGrid CurrentInstance { get; private set; }
        public bool isEditMarketWatch = false;
        private DataGridView editableMarketWatchGridView;
        public static readonly string AppFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Live Rate");
        public static readonly string SymbolListFile = Path.Combine(AppFolder, "symbols.slt");
        public static readonly string passphrase = "v@d{4NME4sOSywXF";
        public string saveFileName;
        public bool isDelete = false;
        public List<string> columnPreferences = new List<string>();
        public List<string> columnPreferencesDefault = new List<string>();
        private ContextMenuStrip rightClickMenu;
        private Panel panelAddSymbols;
        private CheckedListBox checkedListSymbols;
        private Button btnConfirmAddSymbols;
        private Button btnCancelAddSymbols;
        private Button btnSelectAllSymbols;  // declare this with other buttons
        public bool isGrid = true; // Flag to check if this is a grid or not
        private Panel panelAddColumns;
        private CheckedListBox checkedListColumns;
        private System.Windows.Forms.Button btnSelectAllColumns;
        private System.Windows.Forms.Button btnConfirmAddColumns;
        private System.Windows.Forms.Button btnCancelAddColumns;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (socket != null)
                {
                    socket.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        public EditableMarketWatchGrid()
        {

            CommonClass = new Helper.Common(this);
            CurrentInstance = this;
            InitializeDataTable();
            InitializeGrid();
            InitializeAddSymbolPanel();
            InitializeSocket();
            // InitializeSaveButton();
            this.KeyDown += EditableMarketWatchGrid_KeyDown;
            InitializeToolTip();
        }

        public void InitializeToolTip() 
        {
            rightClickMenu = new ContextMenuStrip();

            var addItem = new ToolStripMenuItem("Add/Edit Symbol");
            var addColumn = new ToolStripMenuItem("Add/Edit Column");
            addItem.Click += AddSymbol_Click;
            addColumn.Click += AddColumn_Click;

            rightClickMenu.Items.Add(addItem);
            rightClickMenu.Items.Add(addColumn);

            this.CellMouseClick += EditableMarketWatchGrid_CellMouseClick;
        }

        private void EditableMarketWatchGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                SaveSymbols(selectedSymbols);
            }
        }

        private void InitializeDataTable()
        {
            marketWatchDatatable.Columns.Add("Symbol", typeof(string));
            marketWatchDatatable.Columns.Add("Bid", typeof(decimal));
            marketWatchDatatable.Columns.Add("Ask", typeof(decimal));
            marketWatchDatatable.Columns.Add("High", typeof(decimal));
            marketWatchDatatable.Columns.Add("Low", typeof(decimal));
            marketWatchDatatable.Columns.Add("Open", typeof(decimal));
            marketWatchDatatable.Columns.Add("Close", typeof(decimal));
            marketWatchDatatable.Columns.Add("LTP", typeof(decimal));
            marketWatchDatatable.Columns.Add("DateTime", typeof(DateTime));
        }

        private void InitializeGrid()
        {
            Live_Rate defaultGridInstance = Live_Rate.CurrentInstance;
            this.Name = "editableMarketWatchGridView";
            this.Dock = DockStyle.Fill;
            this.ReadOnly = false;
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            this.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            this.Font = new System.Drawing.Font("Segoe UI", fontSize);
            this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.ColumnHeadersHeight = 40;
            this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.AllowUserToResizeRows = false;
            this.ScrollBars = ScrollBars.Both;
            this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.RowTemplate.Height = 30; // or any height you want
            this.ApplyColumnStyles();
            DataGridViewCellStyle columnHeaderStyle = new DataGridViewCellStyle();
            columnHeaderStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            columnHeaderStyle.Font = new System.Drawing.Font("Segoe UI", fontSize + 2, FontStyle.Bold);
            this.ColumnHeadersDefaultCellStyle = columnHeaderStyle;
            this.CellValueChanged += EditableMarketWatchGrid_CellValueChanged;
            this.CurrentCellDirtyStateChanged += EditableMarketWatchGrid_CurrentCellDirtyStateChanged;
            //this.EditingControlShowing += DataGridView_EditingControlShowing;

            typeof(DataGridView).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.SetProperty,
                    null, this, new object[] { true });



            rightClickMenu = new ContextMenuStrip();

            var addItem = new ToolStripMenuItem("Add/Edit Symbol");
            var addColumn = new ToolStripMenuItem("Add/Edit Column");
            addItem.Click += AddSymbol_Click;
            addColumn.Click += AddColumn_Click;

            rightClickMenu.Items.Add(addItem);
            rightClickMenu.Items.Add(addColumn);

            this.CellMouseClick += EditableMarketWatchGrid_CellMouseClick;



            editableMarketWatchGridView = this;
        }

        private void InitializeAddSymbolPanel()
        {
            // Container panel (with padding and rounded look)
            panelAddSymbols = new Panel
            {
                Size = new Size(500, 500),
                BackColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.None,
                Visible = false,
                Padding = new Padding(20),
            };

            // Shadow effect (optional - mimic with a border or external lib if needed)
            panelAddSymbols.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, panelAddSymbols.ClientRectangle,
                    System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid,
                    System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid,
                    System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid,
                    System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid);
            };

            // Center panel
            panelAddSymbols.Location = new Point(
                (this.Width - panelAddSymbols.Width) / 2,
                (this.Height - panelAddSymbols.Height) / 2
            );

            // Select All button
            btnSelectAllSymbols = new Button
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
            btnSelectAllSymbols.Click += BtnSelectAllSymbols_Click;


            // Title label
            Label titleLabel = new Label
            {
                Text = "🔄 Add / Edit Symbols",
                Font = new System.Drawing.Font("Segoe UI Semibold", 16, FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(50, 50, 50),
                Dock = DockStyle.Top,
                Height = 80,
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

            // Button container (for spacing)
            Panel buttonPanel = new Panel
            {
                Height = 80,
                Dock = DockStyle.Bottom,
                Padding = new Padding(10),
                BackColor = System.Drawing.Color.White
            };

            btnConfirmAddSymbols = new Button
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
            btnConfirmAddSymbols.Click += btnConfirmAddSymbols_Click;

            btnCancelAddSymbols = new Button
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
            btnCancelAddSymbols.Click += btnCancelAddSymbols_Click;

            // Add buttons side by side
            // Position buttons side by side with spacing
            btnSelectAllSymbols.Left = 30;
            btnConfirmAddSymbols.Left = 170;  // adjusted to fit 3 buttons
            btnCancelAddSymbols.Left = 310;

            buttonPanel.Controls.Add(btnSelectAllSymbols);
            buttonPanel.Controls.Add(btnConfirmAddSymbols);
            buttonPanel.Controls.Add(btnCancelAddSymbols);

            // Add controls to panel
            panelAddSymbols.Controls.Add(checkedListSymbols);
            panelAddSymbols.Controls.Add(buttonPanel);
            panelAddSymbols.Controls.Add(titleLabel);

            // Add panel to the main control
            this.Controls.Add(panelAddSymbols);

            // Keep panel centered on resize
            this.Resize += (s, e) =>
            {
                panelAddSymbols.Location = new Point(
                    (this.Width - panelAddSymbols.Width) / 2,
                    (this.Height - panelAddSymbols.Height) / 2
                );
            };
        }

        private void BtnSelectAllSymbols_Click(object sender, EventArgs e)
        {
            bool allChecked = true;

            // Check if all items are already checked
            for (int i = 0; i < checkedListSymbols.Items.Count; i++)
            {
                if (!checkedListSymbols.GetItemChecked(i))
                {
                    allChecked = false;
                    break;
                }
            }

            // If all checked, uncheck all; else check all
            bool check = !allChecked;
            if (!check) 
                btnSelectAllSymbols.Text = "Select All"; // Change button text to "Select All"
            else 
                btnSelectAllSymbols.Text = "Unselect All"; // Change button text to "Unselect All"

            for (int i = 0; i < checkedListSymbols.Items.Count; i++)
                {
                    checkedListSymbols.SetItemChecked(i, check);
                }
        }

        private void EditableMarketWatchGrid_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.ClearSelection();
                if (e.RowIndex >= 0)
                    this.Rows[e.RowIndex].Selected = true;

                rightClickMenu.Show(Cursor.Position);
            }
        }

        private void AddSymbol_Click(object sender, EventArgs e)
        {
            ShowAddSymbolPanel();
        }

        private void AddColumn_Click(object sender, EventArgs e)
        {
            ShowAddColumnPanel();
        }

        private void ShowAddSymbolPanel()
        {
            checkedListSymbols.Items.Clear();

            // First: Add selected symbols (preserving their order in symbolMaster)
            foreach (string symbol in symbolMaster)
            {
                if (selectedSymbols.Contains(symbol))
                {
                    checkedListSymbols.Items.Add(symbol, true);
                }
            }

            // Then: Add unselected symbols
            foreach (string symbol in symbolMaster)
            {
                if (!selectedSymbols.Contains(symbol))
                {
                    checkedListSymbols.Items.Add(symbol, false);
                }
            }

            panelAddSymbols.Visible = true;
        }

        private void btnConfirmAddSymbols_Click(object sender, EventArgs e)
        {
            // Get the current checked (selected) symbols
            var currentlyChecked = checkedListSymbols.CheckedItems.Cast<string>().ToList();

            // Get previously saved symbols
            var previouslySelected = selectedSymbols;

            // Find newly added symbols
            var addedSymbols = currentlyChecked.Except(previouslySelected).ToList();

            // Find removed (now unchecked) symbols
            var removedSymbols = previouslySelected.Except(currentlyChecked).ToList();

            // No change? Show message and exit
            if (!addedSymbols.Any() && !removedSymbols.Any())
            {
                MessageBox.Show("No changes made.");
                return;
            }

            isGrid = false;

            // ✅ Update the selectedSymbols to match currently checked list
            selectedSymbols = currentlyChecked;

            // ✅ Save full updated list
            SaveSymbols(selectedSymbols);

            panelAddSymbols.Visible = false;

            // ✅ Refresh the grid
            UpdateGridBySymbol(selectedSymbols.Distinct().ToList());

        }

        private void btnCancelAddSymbols_Click(object sender, EventArgs e)
        {
            panelAddSymbols.Visible = false;
        }

        private void ShowAddColumnPanel()
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
                    var previouslySelected = columnPreferences.Count > 0 ? columnPreferences : columnPreferencesDefault;
                  

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

                    // Make sure Symbol column is always visible in the grid
                    if (!columnPreferences.Contains("Symbol"))
                    {
                        columnPreferences.Add("Symbol");
                    }

                    // Update DataTable column visibility
                    foreach (DataColumn column in marketWatchDatatable.Columns)
                    {
                        column.ColumnMapping = columnPreferences.Contains(column.ColumnName)
                            ? MappingType.Element
                            : MappingType.Hidden;
                    }

                    // Update grid column visibility
                    UpdateGridColumnVisibility();



                    panelAddColumns.Visible = false;
                    //MessageBox.Show("Columns updated successfully!");

                };

                btnCancelAddColumns.Click += (s, e2) =>
                {
                    panelAddColumns.Visible = false;
                };
            }

            // Refresh items before showing
            checkedListColumns.Items.Clear();


            // Get the columns to display (use allColumns if no preferences set)
            var columnsToShow = columnPreferences.Count > 0 ? columnPreferences : columnPreferencesDefault;

            // Add selected columns first (preserving order)
            foreach (string column in columnPreferencesDefault)
            {
                if (columnsToShow.Contains(column) && column != "Symbol")
                {
                    checkedListColumns.Items.Add(column, true);
                }
            }

            // Then add unselected columns
            foreach (string column in columnPreferencesDefault)
            {
                if (!columnsToShow.Contains(column) && column != "Symbol")
                {
                    checkedListColumns.Items.Add(column, false);
                }
            }

            // Update Select All button text
            btnSelectAllColumns.Text = checkedListColumns.CheckedItems.Count == checkedListColumns.Items.Count
                ? "Unselect All"
                : "Select All";



            // Make sure Symbol column is always visible in the grid
            if (!columnPreferences.Contains("Symbol"))
            {
                columnPreferences.Add("Symbol");
            }

            // Update DataTable column visibility to ensure Symbol is always visible
            foreach (DataColumn column in marketWatchDatatable.Columns)
            {
                if (column.ColumnName == "Symbol")
                {
                    column.ColumnMapping = MappingType.Element;
                }
                else
                {
                    column.ColumnMapping = columnPreferences.Contains(column.ColumnName)
                        ? MappingType.Element
                        : MappingType.Hidden;
                }
            }

            panelAddColumns.Visible = true;
            panelAddColumns.BringToFront();
        }

        private void UpdateGridColumnVisibility()
        {
            // Suspend layout for better performance
            this.SuspendLayout();

            try
            {
                foreach (DataGridViewColumn column in this.Columns)
                {
                    // Only hide/show columns that exist in our preferences list
                    if (columnPreferencesDefault.Contains(column.Name))
                    {
                        column.Visible = columnPreferences.Contains(column.Name);
                    }
                }
            }
            finally
            {
                this.ResumeLayout();
            }
        }

        private void ApplyColumnStyles()
        {
            foreach (DataGridViewColumn column in this.Columns)
            {
                if (column.ValueType == typeof(decimal))
                {
                    column.DefaultCellStyle.Format = "N2";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (column.ValueType == typeof(DateTime))
                {
                    column.DefaultCellStyle.FormatProvider = CultureInfo.InvariantCulture;
                    column.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
                }
            }
        }

        private async void InitializeSocket()
        {
            socket = new SocketIO("https://excel.starlineapi.in:1008", new SocketIOOptions
            {
                Reconnection = true,
                ReconnectionAttempts = int.MaxValue,
                ReconnectionDelay = 1000,
                RandomizationFactor = 0.5,
                EIO = 4
            });

            socket.OnConnected += async (sender, e) =>
            {
                Console.WriteLine("✅ Connected to server");
                await socket.EmitAsync("client", "starline");
                Live_Rate defaultGridInstance = Live_Rate.CurrentInstance;
                defaultGridInstance.statusLabel.Text = "CTRL + S (Save MarketWatch)";
                
            };

            socket.OnDisconnected += (sender, e) =>
            {
                Console.WriteLine("❌ Disconnected from server");
            };

            socket.OnError += (sender, e) =>
            {
                Console.WriteLine($"⚠️ Socket error: {e}");
            };

            socket.On("excelRate", response =>
            {
                try
                {
                    var json = response.GetValue().ToString();
                    var jsonArray = JsonNode.Parse(json)?.AsArray();
                    if (jsonArray == null) return;

                    this.Invoke((MethodInvoker)delegate
                    {
                        lock (marketWatchDatatable)
                        {
                            marketWatchDatatable.Clear();
                            foreach (var item in jsonArray)
                            {
                                var row = marketWatchDatatable.NewRow();
                                row["Symbol"] = item["Symbol"]?.ToString();
                                // Safe decimal conversion with NaN handling
                                row["Bid"] = CommonClass.SafeConvertToDecimal(item["Bid"]?.ToString());
                                row["Ask"] = CommonClass.SafeConvertToDecimal(item["Ask"]?.ToString());
                                row["High"] = CommonClass.SafeConvertToDecimal(item["High"]?.ToString());
                                row["Low"] = CommonClass.SafeConvertToDecimal(item["Low"]?.ToString());
                                row["Open"] = CommonClass.SafeConvertToDecimal(item["Open"]?.ToString());
                                row["Close"] = CommonClass.SafeConvertToDecimal(item["Close"]?.ToString());
                                row["LTP"] = CommonClass.SafeConvertToDecimal(item["LTP"]?.ToString());
                                row["DateTime"] = DateTime.ParseExact(
                                    item["DateTime"]?.ToString() ?? DateTime.Now.ToString(),
                                    "dd/MM/yyyy HH:mm:ss",  // Matches "15/07/2025 17:38:21"
                                    CultureInfo.InvariantCulture
                                );
                                marketWatchDatatable.Rows.Add(row);
                            }

                            foreach (DataColumn column in marketWatchDatatable.Columns)
                            {
                                // Set column visibility based on preferences
                                column.ColumnMapping = columnPreferences.Contains(column.ColumnName)
                                    ? MappingType.Element
                                    : MappingType.Hidden;
                            }

                            // ✅ Populate symbolMaster only once
                            if (!isSymbolMasterInitialized)
                            {
                                symbolMaster = marketWatchDatatable.AsEnumerable()
                                                    .Select(r => r.Field<string>("Symbol"))
                                                    .Distinct()
                                                    .ToList();

                                AddManualEditableRow();  // 🔥 create dropdown row here


                                isSymbolMasterInitialized = true;
                            }

                            UpdateGridWithLatestData();

                            if (symbolMaster != null && isEditMarketWatch == true)
                            {

                                Live_Rate defaultGridInstance = Live_Rate.CurrentInstance;

                                if (defaultGridInstance != null && defaultGridInstance.selectedSymbols != null)
                                {
                                    UpdateGridBySymbol(defaultGridInstance.selectedSymbols);
                                }

                                isEditMarketWatch = false;

                            }


                        }
                    });

                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Error processing data: " + ex.Message);
                }
            });

            try
            {
                await socket.ConnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Connection error: " + ex.Message);
            }
        }

        private void EditableMarketWatchGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (this.IsCurrentCellDirty)
            {
                this.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void EditableMarketWatchGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;


            var grid = sender as DataGridView;
            if (grid.Columns[e.ColumnIndex].Name == "Symbol")
            {
                var selectedValue = grid.Rows[e.RowIndex].Cells["Symbol"].Value?.ToString();

                if (!string.IsNullOrEmpty(selectedValue))
                {
                    // Add Symbol to List for Saving in Future.
                    if (!selectedSymbols.Contains(selectedValue))
                        selectedSymbols.Add(selectedValue);

                    // Try to find the symbol in the marketWatchDatatable
                    DataRow[] foundRows = marketWatchDatatable.Select($"Symbol = '{selectedValue}'");

                    if (foundRows.Length > 0)
                    {
                        DataRow row = foundRows[0];

                        // Dynamically update all visible columns from the data table
                        foreach (DataColumn column in marketWatchDatatable.Columns)
                        {
                            if (column.ColumnMapping != MappingType.Hidden &&
                                grid.Columns.Contains(column.ColumnName) &&
                                column.ColumnName != "Symbol") // Skip Symbol column as it's our key
                            {
                                grid.Rows[e.RowIndex].Cells[column.ColumnName].Value = row[column];
                            }
                        }
                    }

                    // Add a new row if this is the last row
                    if (e.RowIndex == grid.Rows.Count - 1)
                    {
                        int newRowIndex = grid.Rows.Add();
                        grid.Rows[newRowIndex].Cells["Symbol"] = new DataGridViewComboBoxCell
                        {
                            DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                            FlatStyle = FlatStyle.Flat,
                            DataSource = symbolMaster,
                            Value = null,
                        };
                        grid.Rows[newRowIndex].Cells["Symbol"].Style.Font = new System.Drawing.Font("Segoe UI", fontSize, FontStyle.Bold);
                    }

                    UpdateGridWithLatestData();
                }
            }
        }

        private void UpdateGridWithLatestData()
        {

            // Create a dictionary for faster lookup of market data
            var marketDataDict = new Dictionary<string, DataRow>();
            foreach (DataRow row in marketWatchDatatable.Rows)
            {
                var symbol = row["Symbol"].ToString();
                if (!marketDataDict.ContainsKey(symbol))
                {
                    marketDataDict.Add(symbol, row);
                }
            }

            // Process rows in bulk
            foreach (DataGridViewRow gridRow in editableMarketWatchGridView.Rows)
            {
                if (gridRow.IsNewRow) continue;

                var symbolCell = gridRow.Cells["Symbol"];
                if (symbolCell.Value == null) continue;

                string symbol = symbolCell.Value.ToString();
                if (!marketDataDict.TryGetValue(symbol, out DataRow dataRow)) continue;

                // Update all cells at once for this row
                UpdateRowCells(gridRow, dataRow);
            }
        }

        private void UpdateRowCells(DataGridViewRow gridRow, DataRow dataRow)
        {
            isDelete = false;


            // ✅ Adjust row height based on font size
            int rowHeight = (int)Math.Ceiling(fontSize * 2.8); // tweak multiplier as needed in all rows
            gridRow.Height = rowHeight;


            // Store previous values for comparison
            var previousValues = new Dictionary<string, decimal?>();
            foreach (DataGridViewCell cell in gridRow.Cells)
            {
                if (cell.Value == null || cell.OwningColumn.Name == "Symbol") continue;

                if (decimal.TryParse(cell.Value.ToString(), out decimal decimalValue))
                {
                    previousValues[cell.OwningColumn.Name] = decimalValue;
                }
            }

            DataGridViewCellStyle columnHeaderStyle = new DataGridViewCellStyle();
            columnHeaderStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            columnHeaderStyle.Font = new System.Drawing.Font("Segoe UI", fontSize + 2, FontStyle.Bold);
            this.ColumnHeadersDefaultCellStyle = columnHeaderStyle;
            this.ColumnHeadersHeight = (int)Math.Ceiling(fontSize * 3.0); // tweak multiplier as needed in Header

            // Create cell styles in advance
            var symbolStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                ForeColor = System.Drawing.Color.Black,
                Font = new System.Drawing.Font("Segoe UI", fontSize, FontStyle.Bold),

            };

            // Create cell styles in advance
            var defaultStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                ForeColor = System.Drawing.Color.Black,
                Font = new System.Drawing.Font("Segoe UI", fontSize, FontStyle.Regular),
            };

            var rightAlignStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleRight,
                ForeColor = System.Drawing.Color.Black,
                Font = new System.Drawing.Font("Segoe UI", fontSize, FontStyle.Regular),
            };



            // Update all cells in one pass
            foreach (DataGridViewCell cell in gridRow.Cells)
            {
                var columnName = cell.OwningColumn.Name;

                if (!columnPreferences.Contains(columnName)) continue;
                
                if (columnName == "Symbol")
                {
                    cell.Style = symbolStyle; // Keep Symbol column with default style
                    continue;
                }             

                object value = dataRow[columnName];
                if (value == DBNull.Value)
                {
                    cell.Value = string.Empty;
                    cell.Style = defaultStyle;
                    continue;
                }

                if (value is decimal || value is double || value is float || value is int)
                {
                    decimal newDecimal = Convert.ToDecimal(value);
                    cell.Value = newDecimal.ToString("N2");

                    // Clone the right-align style to avoid creating new instances
                    var style = (DataGridViewCellStyle)rightAlignStyle.Clone();

                    // Apply color coding if we have previous value
                    if (previousValues.TryGetValue(columnName, out decimal? previousValue) && previousValue.HasValue)
                    {
                        style.ForeColor = newDecimal > previousValue.Value ? System.Drawing.Color.Green :
                                         newDecimal < previousValue.Value ? System.Drawing.Color.Red : System.Drawing.Color.Black;
                    }

                    cell.Style = style;
                }
                else if (value is DateTime dateTimeValue)
                {
                    cell.Value = dateTimeValue;
                    var style = (DataGridViewCellStyle)defaultStyle.Clone();
                    style.FormatProvider = CultureInfo.InvariantCulture;
                    style.Format = "dd/MM/yyyy HH:mm:ss";
                    cell.Style = style;
                }
                else
                {
                    cell.Value = value.ToString();
                    cell.Style = defaultStyle;
                }
            }
        }

        public void SaveSymbols(List<string> SymbolList)
        {
            try
            {
                int symbolCount = SymbolList.Count;
                int rowCount = editableMarketWatchGridView.NewRowIndex >= 0
                    ? editableMarketWatchGridView.Rows.Count - 1
                    : editableMarketWatchGridView.Rows.Count;

                rowCount = rowCount - 1;

                if (symbolCount != rowCount && isGrid)
                {
                    // Clear the selectedSymbols list
                    SymbolList.Clear();

                    // Iterate through each row in the gridview
                    foreach (DataGridViewRow row in editableMarketWatchGridView.Rows)
                    {
                        // Skip if the row is the new row (if applicable)
                        if (!row.IsNewRow)
                        {
                            // Get the value from the "Symbol" column
                            var symbolValue = row.Cells["Symbol"].Value;

                            // Add to selectedSymbols if the value is not null
                            if (symbolValue != null)
                            {
                                SymbolList.Add(symbolValue.ToString());
                            }
                        }
                    }
                }

                if (SymbolList.Count == 0)
                {
                    MessageBox.Show("Please Select Atleast one Symbol for Save", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                if (saveFileName == null)
                {// Show save file dialog
                    using (var saveDialog = new SaveFileDialog())
                    {
                        saveDialog.InitialDirectory = AppFolder;  // Set default directory
                        saveDialog.Filter = "Symbol List Files (*.slt)|*.slt|All files (*.*)|*.*";
                        saveDialog.Title = "Save Symbol List";
                        saveDialog.DefaultExt = ".slt";
                        saveDialog.AddExtension = true;

                        if (!Directory.Exists(AppFolder))
                            Directory.CreateDirectory(AppFolder);

                        // If user selected a file
                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            string json = JsonSerializer.Serialize(SymbolList);
                            string encryptedJson = CryptoHelper.Encrypt(json, passphrase);

                            // Ensure directory exists (should already exist from AppFolder)
                            if (!Directory.Exists(AppFolder))
                                Directory.CreateDirectory(AppFolder);


                            // Save to the user-selected filename
                            File.WriteAllText(saveDialog.FileName, encryptedJson);

                            if (isGrid)
                            {
                                SymbolList.Clear();
                            }

                            saveFileName = Path.GetFileNameWithoutExtension(saveDialog.FileName);


                            MessageBox.Show($"{Path.GetFileNameWithoutExtension(saveDialog.FileName)} MarketWatch Save Successfully", "MarketWatch Save", MessageBoxButtons.OK);

                        }
                    }
                }
                else
                {
                    string json = JsonSerializer.Serialize(SymbolList);
                    string encryptedJson = CryptoHelper.Encrypt(json, passphrase);

                    // Ensure directory exists (should already exist from AppFolder)
                    if (!Directory.Exists(AppFolder))
                        Directory.CreateDirectory(AppFolder);

                    saveFileName = Path.Combine(AppFolder, $"{saveFileName}.slt");
                    // Save to the user-selected filename
                    File.WriteAllText(saveFileName, encryptedJson);

                    if (isGrid)
                    {
                        SymbolList.Clear();
                    }

                    MessageBox.Show($"{Path.GetFileNameWithoutExtension(saveFileName)} Marketwatch Update Successfully", "MarketWatch Save", MessageBoxButtons.OK);

                    saveFileName = Path.GetFileNameWithoutExtension(saveFileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Problem While Saving File: {ex.Message}", "Saving Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                selectedSymbols = SymbolList;
                Live_Rate live_Rate = Live_Rate.CurrentInstance;
                if (live_Rate == null)
                    live_Rate.LiveRateGrid();
                if (saveFileName != null)
                {
                    live_Rate.titleLabel.Text = $"{saveFileName}";
                }
                live_Rate.MenuLoad();
            }
        }

        private void AddManualEditableRow()
        {
            if (Columns != null)
                Columns.Clear();

            var columnsToAdd = marketWatchDatatable?.Columns
                  .Cast<DataColumn>()
                  .Where(col => col.ColumnMapping != MappingType.Hidden)
                  .ToList(); // ✅ This creates List<DataColumn>



            // Create searchable combo box column with enhanced features
            var symbolColumn = new DataGridViewComboBoxColumn
            {
                Name = "Symbol",
                HeaderText = "Symbol",
                DataSource = new BindingList<string>(),
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                FlatStyle = FlatStyle.Flat,
                Width = 200, // Increased width for better visibility
                AutoComplete = true,
            };

            // Add custom styling for better UX
            symbolColumn.CellTemplate.Style.BackColor = System.Drawing.Color.WhiteSmoke;
            symbolColumn.CellTemplate.Style.SelectionBackColor = System.Drawing.Color.LightSteelBlue;
            if(symbolColumn != null)
                symbolColumn.CellTemplate.Style.Font = new System.Drawing.Font("Segoe UI", fontSize, FontStyle.Bold);

            Columns.Add(symbolColumn);

            // Add all other visible columns from data table
            foreach (var dataColumn in columnsToAdd)
            {
                if (dataColumn.ColumnName == "Symbol") continue; // Skip as we already added it

                var gridColumn = new DataGridViewTextBoxColumn
                {
                    Name = dataColumn.ColumnName,
                    HeaderText = dataColumn.ColumnName,
                    ValueType = dataColumn.DataType,
                    ReadOnly = true // All columns except Symbol are read-only
                };

                // Apply default formatting based on data type
                if (dataColumn.DataType == typeof(decimal) ||
                    dataColumn.DataType == typeof(double) ||
                    dataColumn.DataType == typeof(float))
                {
                    gridColumn.DefaultCellStyle.Format = "N2";
                    gridColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (dataColumn.DataType == typeof(DateTime))
                {
                    gridColumn.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
                }

                Columns.Add(gridColumn);
            }

            // Add empty row manually
            int rowIndex = Rows.Add();


            // Create an enhanced combo box cell with search functionality
            var comboCell = new DataGridViewComboBoxCellEx
            {
                DataSource = new BindingList<string>(symbolMaster),
                Value = null,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                FlatStyle = FlatStyle.Flat,
            };

            Rows[rowIndex].Cells["Symbol"] = comboCell;
            Rows[rowIndex].Cells["Symbol"].Style.Font = new System.Drawing.Font("Segoe UI", fontSize, FontStyle.Bold);


            // After adding all columns
            foreach (DataGridViewColumn column in this.Columns)
            {
                // Make only Symbol column editable
                column.ReadOnly = column.Name != "Symbol";
                column.Visible = columnsToAdd.Any(dc => dc.ColumnName == column.Name);
            }
            Rows[rowIndex].Height = (int)Math.Ceiling(fontSize * 2.8); // Adjust row height based on font size
        }

        private void InitializeRowCells(DataGridViewRow row)
        {
            // Set default values for all cells except Symbol
            foreach (DataGridViewCell cell in row.Cells)
            {
                if (cell.OwningColumn.Name != "Symbol")
                {
                    cell.Value = DBNull.Value;
                }
            }
        }

        public void UpdateGridBySymbol(List<string> symbols)
        {
            selectedSymbols.Clear();
            selectedSymbols = symbols; // Filter valid symbols

            editableMarketWatchGridView.Rows.Clear();
            editableMarketWatchGridView.Columns.Clear();

            InitializeGrid();
            AddManualEditableRow();
            InitializeSocket();

            try
            {

                // Add new row and get its index
                int rowIndex = 0;
                // Add symbol rows (only for valid symbols)
                foreach (var symbol in selectedSymbols)
                {

                    // Get reference to the actual cell in the grid (not creating a new instance)
                    var cell = (DataGridViewComboBoxCell)editableMarketWatchGridView.Rows[rowIndex].Cells["Symbol"];

                    cell.DataSource = symbolMaster;
                    cell.Value = symbol;

                    // Initialize other cells to prevent null reference issues
                    InitializeRowCells(editableMarketWatchGridView.Rows[rowIndex]);

                    rowIndex++;
                }

                // Ensure column visibility is applied
                UpdateGridColumnVisibility();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating grid: {ex.Message}");
            }

            UpdateGridWithLatestData();
        }
    }

    // Custom ComboBoxCell class with enhanced search
    public class DataGridViewComboBoxCellEx : DataGridViewComboBoxCell
    {
        private string _searchText = string.Empty;
        private bool _isDroppedDown = false;
        private DateTime _lastKeyPressTime = DateTime.MinValue;
        private const int KeyPressThreshold = 300; // milliseconds
        private List<int> _matchedIndexes = new List<int>();
        private char _lastKeyChar;
        private int _matchIndex = 0;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == this.ColumnIndex && e.RowIndex == this.RowIndex)
            {
                _isDroppedDown = !_isDroppedDown;
                if (_isDroppedDown)
                {
                    this.DataGridView.BeginEdit(true);
                    var editingControl = this.DataGridView.EditingControl as DataGridViewComboBoxEditingControl;
                    if (editingControl != null)
                    {
                        // Reset state for this cell
                        ResetState();

                        editingControl.DroppedDown = true;
                        editingControl.KeyPress += EditingControl_KeyPress;
                        editingControl.TextUpdate += EditingControl_TextUpdate;
                        editingControl.KeyDown += EditingControl_KeyDown;
                        editingControl.SelectedIndexChanged += EditingControl_SelectedIndexChanged;
                        editingControl.Leave += EditingControl_Leave; // Add Leave event to clean up
                    }
                }
            }
            base.OnMouseClick(e);
        }

        private void EditingControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            var editingControl = sender as DataGridViewComboBoxEditingControl;
            if (editingControl.SelectedIndex >= 0)
            {
                // Update the cell value when an item is selected
                this.Value = editingControl.SelectedItem;
                _searchText = string.Empty;
            }
        }

        private void EditingControl_KeyDown(object sender, KeyEventArgs e)
        {
            var editingControl = sender as DataGridViewComboBoxEditingControl;

            if (e.KeyCode == Keys.Escape)
            {
                var result = MessageBox.Show("Do you want to Exit Application?", "Exit Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    _isDroppedDown = false;
                    editingControl.DroppedDown = false;
                    ResetState();
                    this.DataGridView.EndEdit();
                    _searchText = string.Empty; 
                }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (editingControl.Items.Count > 0)
                {
                    // Select the first item when Enter is pressed
                    this.Value = editingControl.Items[0];
                    _searchText = string.Empty;
                    ResetState();
                    editingControl.DroppedDown = false;
                    this.DataGridView.EndEdit();
                }
            }
        }

        private void EditingControl_Leave(object sender, EventArgs e)
        {
            // Clean up event handlers and reset state when leaving the control
            var editingControl = sender as DataGridViewComboBoxEditingControl;
            if (editingControl != null)
            {
                editingControl.KeyPress -= EditingControl_KeyPress;
                editingControl.TextUpdate -= EditingControl_TextUpdate;
                editingControl.KeyDown -= EditingControl_KeyDown;
                editingControl.SelectedIndexChanged -= EditingControl_SelectedIndexChanged;
                editingControl.Leave -= EditingControl_Leave;
            }
            ResetState();
            _isDroppedDown = false;
            this.DataGridView.EndEdit();
        }

        private void EditingControl_TextUpdate(object sender, EventArgs e)
        {
            var editingControl = sender as DataGridViewComboBoxEditingControl;
            FilterItems(editingControl);
        }

        private void EditingControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            char keyChar = char.ToUpper(e.KeyChar);
            var comboBox = sender as ComboBox;

            // Timeout: reset if user waits too long between key presses
            if ((DateTime.Now - _lastKeyPressTime).TotalSeconds > 10 || _lastKeyChar != keyChar)
            {
                _matchedIndexes.Clear();
                _matchIndex = 0;
            }

            _lastKeyPressTime = DateTime.Now;
            _lastKeyChar = keyChar;

            // Only populate matches if first time or reset
            if (_matchedIndexes.Count == 0)
            {
                for (int i = 0; i < comboBox.Items.Count; i++)
                {
                    string itemText = comboBox.GetItemText(comboBox.Items[i]).ToUpper();
                    if (itemText.StartsWith(keyChar.ToString()))
                    {
                        _matchedIndexes.Add(i);
                    }
                }
            }

            if (_matchedIndexes.Count > 0)
            {
                comboBox.SelectedIndex = _matchedIndexes[_matchIndex];
                _matchIndex = (_matchIndex + 1) % _matchedIndexes.Count;
            }
        }

        private void FilterItems(ComboBox comboBox)
        {
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                comboBox.DataSource = new BindingList<string>((IList<string>)this.DataSource);
                comboBox.Text = string.Empty;
                return;
            }

            var filteredItems = ((IList<string>)this.DataSource)
                .Where(item => item.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            comboBox.DataSource = new BindingList<string>(filteredItems);

            if (filteredItems.Count > 0)
            {
                comboBox.DroppedDown = true;
                comboBox.Text = _searchText;
                comboBox.SelectionStart = _searchText.Length;
            }
            else
            {
                comboBox.DroppedDown = false;
            }
        }

        private void ResetState()
        {
            _searchText = string.Empty;
            _matchedIndexes.Clear();
            _matchIndex = 0;
            _lastKeyChar = '\0';
            _lastKeyPressTime = DateTime.MinValue;
        }
    }
}