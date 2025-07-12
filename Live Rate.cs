using DocumentFormat.OpenXml.Drawing.Diagrams;
using Live_Rate_Application.MarketWatch;
using Microsoft.Win32;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace Live_Rate_Application
{
    public partial class Live_Rate : Form
    {

        [DllImport("oleaut32.dll", PreserveSig = false)]
        static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);
        private readonly Helper.Common CommonClass;
        // In Live_Rate.cs
        public bool IsConnected
        {
            get { return connectionViewMode == ConnectionViewMode.Connect; }
            set { connectionViewMode = value ? ConnectionViewMode.Connect : ConnectionViewMode.Disconnect; }
        }
        private SocketIO socket = null;
        public static readonly string AppFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Live Rate");
        bool isLoadedSymbol = false;
        public static List<string> selectedSymbols = new List<string>();
        public List<string> FileLists = new List<string>();
        // DataTable Variables
        static DataTable marketDataTable = new DataTable();
        private readonly object tableLock = new object();

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


        public Live_Rate()
        {
            InitializeComponent();

            CommonClass = new Helper.Common(this);
            CommonClass.StartInternetMonitor();

            this.KeyPreview = true; // Allow form to detect key presses
            // Enable double buffering for the form
            this.DoubleBuffered = true;
            // Set control styles for better performance
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);

            MenuLoad();


            InitializeSocket();
            InitializeDataTable();
            this.WindowState = FormWindowState.Maximized;
            dataGridView1.Dock = DockStyle.Fill;
            this.FormClosed += LiveRate_FormClosed;
            saveToolStripMenuItem.Enabled = false;
        }

        private void Live_Rate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close(); // Close the login form
                Application.Exit(); // Terminate the application
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

        private void Live_Rate_Load(object sender, EventArgs e)
        {
            dataGridView1.ContextMenuStrip = Tools;
        }

        private void UpdateUI(Action action)
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
            Application.Exit();
        }

        private void DataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                dataGridView1.ClearSelection();
                //dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected = true;
            }
        }

        private void ExportToXSLXToolStripMenuItem_Click(object sender, EventArgs e) =>
            // Run Excel operations in a separate thread
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
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
                            // 3. Proper cleanup in reverse order
                            if (tempWorksheet != null) Marshal.ReleaseComObject(tempWorksheet);
                            if (tempWorkbook != null) Marshal.ReleaseComObject(tempWorkbook);
                            if (tempApp != null) Marshal.ReleaseComObject(tempApp);

                            // 4. Force garbage collection
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            GC.Collect();
                        }


                        // Release COM objects
                        Marshal.ReleaseComObject(tempWorksheet);
                        Marshal.ReleaseComObject(tempWorkbook);
                        Marshal.ReleaseComObject(tempApp);

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

                    if (!isLoadedSymbol)
                    {
                        // Flush any data collected so far
                        RefreshExcelFromDataTable(marketDataTable);
                    }
                    else 
                    {
                        SymbolExportToExcel();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Excel export error: " + ex.Message);
                }
            });

        private void RefreshExcelFromDataTable(DataTable data) =>
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

        private void UpdateGrid()
        {
            if (dataGridView1.InvokeRequired)
            {
                if (!isLoadedSymbol)
                {
                    dataGridView1.BeginInvoke(new Action(UpdateGridInternal));
                }
                else 
                {
                    dataGridView1.BeginInvoke(new Action(UpdateGridBySelectedSymbol));
                }
            }
            else
            {
                if (!isLoadedSymbol)
                    UpdateGridInternal();
                else
                    UpdateGridBySelectedSymbol();
            }
        }

        private void InitializeDataGridView()
        {
            // Clear existing columns if any
            dataGridView1.Columns.Clear();

            // Add columns manually to match your DataTable structure
            dataGridView1.Columns.Add("Symbol", "Symbol");
            dataGridView1.Columns.Add("Bid", "Bid");
            dataGridView1.Columns.Add("Ask", "Ask");
            dataGridView1.Columns.Add("High", "High");
            dataGridView1.Columns.Add("Low", "Low");
            dataGridView1.Columns.Add("Open", "Open");
            dataGridView1.Columns.Add("Close", "Close");
            dataGridView1.Columns.Add("LTP", "LTP");
            dataGridView1.Columns.Add("DateTime", "DateTime");

            // Configure column properties
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                column.ReadOnly = true;
            }

        }

        private void UpdateGridInternal()
        {
            int FixedRowCount = 17;

            // Instrument mapping dictionary
            var instruments = new Dictionary<string, string>
                {
                    {"XAUUSD", "Gold Spot"},
                    {"XAGUSD3", "Silver Spot"},
                    {"XPTUSD", "Platinum Spot"},
                    {"XPDUSD", "Palladium Spot"},
                    {"INRSPOT", "INR Spot"},
                    {"EURUSD", "EUR/USD"},
                    {"GLD", "Gold Future"},
                    {"SLR", "Silver Future"},
                    {"PTAM", "Platinum AM Fix"},
                    {"PDAM", "Palladium AM Fix"},
                    {"GOLDAM", "Gold AM Fix"},
                    {"SILVERFIX", "Silver Fix"},
                    {"PTPM", "Platinum PM Fix"},
                    {"PDPM", "Palladium PM Fix"},
                    {"GOLDPM", "Gold PM Fix"},
                    {"GOLD", "Gold COMEX"},
                    {"DGINRSPOT", "Domestic Gold INR Spot"}
            };

            // First update the DataTable with proper instrument names
            foreach (DataRow row in marketDataTable.Rows)
            {
                if (row[0] != null && instruments.TryGetValue(row[0].ToString(), out string displayName))
                {
                    row[0] = displayName;
                }
            }

            if (dataGridView1.IsDisposed) return;

            dataGridView1.SuspendLayout();
            try
            {
                // Ensure columns exist
                if (dataGridView1.Columns.Count == 0)
                {
                    InitializeDataGridView();

                    // Set default styles for all columns
                    var headerStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter,
                        Font = new System.Drawing.Font(dataGridView1.Font.FontFamily, 13.50f, FontStyle.Bold)
                    };

                    // Set default styles for all columns
                    var defaultStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter,
                        Font = new System.Drawing.Font(dataGridView1.Font.FontFamily, 15f, FontStyle.Regular)
                    };

                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        column.DefaultCellStyle = defaultStyle;
                        column.HeaderCell.Style = headerStyle;
                    }

                    if (dataGridView1.Columns.Count > 0)
                    {
                        dataGridView1.Columns[0].DefaultCellStyle = headerStyle;
                        dataGridView1.Columns[0].HeaderCell.Style = headerStyle;
                    }
                }

                // Ensure we have exactly 17 rows
                while (dataGridView1.Rows.Count < FixedRowCount)
                {
                    dataGridView1.Rows.Add();
                }
                while (dataGridView1.Rows.Count > FixedRowCount)
                {
                    dataGridView1.Rows.RemoveAt(dataGridView1.Rows.Count - 1);
                }

                int rowsToUpdate = Math.Min(FixedRowCount, marketDataTable.Rows.Count);

                // Update cell values with formatting
                for (int i = 0; i < rowsToUpdate; i++)
                {
                    for (int j = 0; j < dataGridView1.Columns.Count; j++)
                    {
                        // Skip Symbol columns (assuming j=0 is Symbol)
                        if (j == 0 || j == dataGridView1.Columns.Count - 1)
                        {
                            dataGridView1.Rows[i].Cells[j].Value = marketDataTable.Rows[i][j]?.ToString();
                            dataGridView1.Rows[i].Cells[j].Style = new DataGridViewCellStyle
                            {
                                Alignment = DataGridViewContentAlignment.MiddleLeft,
                                ForeColor = System.Drawing.Color.Black
                            };
                            continue;
                        }

                        // Get current and new values
                        object currentValueObj = dataGridView1.Rows[i].Cells[j].Value;
                        string currentValueStr = currentValueObj?.ToString() ?? string.Empty;
                        //string newValueStr = marketDataTable.Rows[i][j]?.ToString() ?? string.Empty;

                        // Update cell value
                        var value = marketDataTable.Rows[i][j];

                        if (value != DBNull.Value && double.TryParse(value.ToString(), out double number))
                        {
                            dataGridView1.Rows[i].Cells[j].Value = number.ToString("F2");
                        }
                        else
                        {
                            dataGridView1.Rows[i].Cells[j].Value = string.Empty;
                        }


                        // Create cell style
                        var cellStyle = new DataGridViewCellStyle
                        {
                            Alignment = DataGridViewContentAlignment.MiddleRight,
                        };

                        // Try to parse as decimal for comparison
                        if (decimal.TryParse(currentValueStr, out decimal currentDecimal) &&
                            decimal.TryParse(marketDataTable.Rows[i][j]?.ToString(), out decimal newDecimal))
                        {
                            if (newDecimal > currentDecimal)
                            {
                                cellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                                cellStyle.ForeColor = System.Drawing.Color.Green;
                            }
                            else if (newDecimal < currentDecimal)
                            {
                                cellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                                cellStyle.ForeColor = System.Drawing.Color.Red;
                            }
                        }

                        dataGridView1.Rows[i].Cells[j].Style = cellStyle;
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    }
                }

                // Clear remaining rows
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
                dataGridView1.ResumeLayout();
                RefreshExcelFromDataTable(marketDataTable);
            }
        }

        protected void InitializeDataTable()
        {
            if (marketDataTable == null)
                marketDataTable = new DataTable();

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

        private void RefreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Refresh logic here
            statusLabel.Text = "Ready";

        }

        private void NewMarketWatchMenuItem_Click(object sender, EventArgs e)
        {
            marketWatchViewMode = MarketWatchViewMode.New;

            // Hide the existing DataGridView
            dataGridView1.Visible = false;
            dataGridView1.Rows.Clear();
            socket.DisconnectAsync();

            // Remove any existing editable grid
            var existingGrid = this.Controls.Find("editableMarketWatchGridView", true).FirstOrDefault();
            existingGrid?.Dispose();

            // Create and add the new editable grid with your column structure
            var editableGrid = new EditableMarketWatchGrid();
            this.Controls.Add(editableGrid);
            editableGrid.BringToFront();
            editableGrid.Focus();

            toolsMenuItem.Enabled = false;

            // Update menu items
            saveToolStripMenuItem.Enabled = true;
            newMarketWatchMenuItem.Enabled = false;

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

        private void DefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isLoadedSymbol = false;
            LiveRateGrid();
        }

        public void LiveRateGrid() 
        {
            marketWatchViewMode = MarketWatchViewMode.Default;
            socket.ConnectAsync();

            // Hide the DataGridView
            dataGridView1.Visible = true;
            dataGridView1.BringToFront();
            dataGridView1.Focus();
            newMarketWatchMenuItem.Enabled = true;
            saveToolStripMenuItem.Enabled = false;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                EditableMarketWatchGrid editableMarketWatchGrid = new EditableMarketWatchGrid();
                editableMarketWatchGrid.SaveSymbols();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Problem while Saving File for {ex}","Saving Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private void MenuLoad()
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
                    MenuLoad();
                    SetActiveMenuItem(clickedItem);
                };
                defaultMenuItem.Enabled = false;
                openCTRLOToolStripMenuItem.DropDownItems.Add(defaultMenuItem);

                // Add each file as a menu item with a click handler
                foreach (string fileName in fileNames)
                {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem(fileName);
                    menuItem.Click += (sender, e) => {
                        // Handle file selection here
                        string selectedFile = (sender as ToolStripMenuItem).Text;
                        LoadSymbol(Path.Combine(selectedFile + ".slt"));
                        SetActiveMenuItem(menuItem);
                    };
                    openCTRLOToolStripMenuItem.DropDownItems.Add(menuItem);
                }
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
                if(item.Text == activeItem.Text)
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

        private void UpdateGridBySelectedSymbol()
        {
            int FixedRowCount = 17;

            // Instrument mapping dictionary
            var instruments = new Dictionary<string, string>
            {
                {"XAUUSD", "Gold Spot"},
                {"XAGUSD3", "Silver Spot"},
                {"XPTUSD", "Platinum Spot"},
                {"XPDUSD", "Palladium Spot"},
                {"INRSPOT", "INR Spot"},
                {"EURUSD", "EUR/USD"},
                {"GLD", "Gold Future"},
                {"SLR", "Silver Future"},
                {"PTAM", "Platinum AM Fix"},
                {"PDAM", "Palladium AM Fix"},
                {"GOLDAM", "Gold AM Fix"},
                {"SILVERFIX", "Silver Fix"},
                {"PTPM", "Platinum PM Fix"},
                {"PDPM", "Palladium PM Fix"},
                {"GOLDPM", "Gold PM Fix"},
                {"GOLD", "Gold COMEX"},
                {"DGINRSPOT", "Domestic Gold INR Spot"}
            };

            // Create a filtered view of the DataTable with only selected symbols
            DataTable filteredTable = marketDataTable.Clone();

            foreach (DataRow row in marketDataTable.Rows)
            {
                string symbol = row[0]?.ToString();

                // Check if this symbol is in the selectedSymbols list
                if (selectedSymbols.Contains(symbol))
                {
                    // Create a new row in the filtered table
                    DataRow newRow = filteredTable.NewRow();

                    // Copy all values
                    for (int i = 0; i < marketDataTable.Columns.Count; i++)
                    {
                        newRow[i] = row[i];
                    }

                    // Update the display name if needed
                    if (instruments.TryGetValue(symbol, out string displayName))
                    {
                        newRow[0] = displayName;
                    }

                    filteredTable.Rows.Add(newRow);
                }
            }

            if (dataGridView1.IsDisposed) return;

            dataGridView1.SuspendLayout();
            try
            {
                // Ensure columns exist
                if (dataGridView1.Columns.Count == 0)
                {
                    InitializeDataGridView();

                    // Set default styles for all columns
                    var headerStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter,
                        Font = new System.Drawing.Font(dataGridView1.Font.FontFamily, 13.50f, FontStyle.Bold)
                    };

                    // Set default styles for all columns
                    var defaultStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter,
                        Font = new System.Drawing.Font(dataGridView1.Font.FontFamily, 15f, FontStyle.Regular)
                    };

                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        column.DefaultCellStyle = defaultStyle;
                        column.HeaderCell.Style = headerStyle;
                    }

                    if (dataGridView1.Columns.Count > 0)
                    {
                        dataGridView1.Columns[0].DefaultCellStyle = headerStyle;
                        dataGridView1.Columns[0].HeaderCell.Style = headerStyle;
                    }
                }

                // Ensure we have exactly 17 rows (or as many as selected symbols)
                int actualRowCount = Math.Min(FixedRowCount, filteredTable.Rows.Count);

                while (dataGridView1.Rows.Count < actualRowCount)
                {
                    dataGridView1.Rows.Add();
                }
                while (dataGridView1.Rows.Count > actualRowCount)
                {
                    dataGridView1.Rows.RemoveAt(dataGridView1.Rows.Count - 1);
                }

                // Update cell values with formatting
                for (int i = 0; i < actualRowCount; i++)
                {
                    for (int j = 0; j < dataGridView1.Columns.Count; j++)
                    {
                        // Skip Symbol columns (assuming j=0 is Symbol)
                        if (j == 0 || j == dataGridView1.Columns.Count - 1)
                        {
                            dataGridView1.Rows[i].Cells[j].Value = filteredTable.Rows[i][j]?.ToString();
                            dataGridView1.Rows[i].Cells[j].Style = new DataGridViewCellStyle
                            {
                                Alignment = DataGridViewContentAlignment.MiddleLeft,
                                ForeColor = System.Drawing.Color.Black
                            };
                            continue;
                        }

                        // Get current and new values
                        object currentValueObj = dataGridView1.Rows[i].Cells[j].Value;
                        string currentValueStr = currentValueObj?.ToString() ?? string.Empty;

                        // Update cell value
                        var value = filteredTable.Rows[i][j];

                        if (value != DBNull.Value && double.TryParse(value.ToString(), out double number))
                        {
                            dataGridView1.Rows[i].Cells[j].Value = number.ToString("F2");
                        }
                        else
                        {
                            dataGridView1.Rows[i].Cells[j].Value = string.Empty;
                        }

                        // Create cell style
                        var cellStyle = new DataGridViewCellStyle
                        {
                            Alignment = DataGridViewContentAlignment.MiddleRight,
                        };

                        // Try to parse as decimal for comparison
                        if (decimal.TryParse(currentValueStr, out decimal currentDecimal) &&
                            decimal.TryParse(filteredTable.Rows[i][j]?.ToString(), out decimal newDecimal))
                        {
                            if (newDecimal > currentDecimal)
                            {
                                cellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                                cellStyle.ForeColor = System.Drawing.Color.Green;
                            }
                            else if (newDecimal < currentDecimal)
                            {
                                cellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                                cellStyle.ForeColor = System.Drawing.Color.Red;
                            }
                        }

                        dataGridView1.Rows[i].Cells[j].Style = cellStyle;
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    }
                }

                // Clear remaining rows if we have fewer selected symbols than FixedRowCount
                for (int i = actualRowCount; i < FixedRowCount; i++)
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
                dataGridView1.ResumeLayout();
                SymbolExportToExcel(); // Refresh Excel with filtered data
            }
        }

        public void SymbolExportToExcel()=>
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

                if (workbook == null || worksheet == null || fileopen == false)
                {
                    CleanupExcelResources();
                    return;
                }

                if (dataGridView1 == null || worksheet == null || workbook == null || excelApp == null)
                {
                    return;
                }

                try
                {

                    // Add headers
                    for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    {
                        worksheet.Cells[1, i + 1] = dataGridView1.Columns[i].HeaderText;
                        ((Excel.Range)worksheet.Cells[1, i + 1]).Font.Bold = true;
                    }

                    int rowsToPreserve = dataGridView1.Rows.Count;
                    if (dataGridView1.AllowUserToAddRows)
                    {
                        // Exclude the "new row" if present
                        if (dataGridView1.Rows.Count > 0 && dataGridView1.Rows[dataGridView1.Rows.Count - 1].IsNewRow)
                        {
                            rowsToPreserve--;
                        }
                    }

                    // Clear all rows except header and first N rows (where N = rowsToPreserve)
                    Excel.Range usedRange = worksheet.UsedRange;
                    if (usedRange != null && usedRange.Rows.Count > 1 + rowsToPreserve)
                    {
                        // Calculate the range to clear (rows after header + preserved rows)
                        int firstRowToClear = 2 + rowsToPreserve; // Row numbers start at 1 in Excel
                        int lastRowInSheet = usedRange.Rows.Count;

                        Excel.Range rowsToClear = worksheet.Range[
                            worksheet.Cells[firstRowToClear, 1],
                            worksheet.Cells[lastRowInSheet, usedRange.Columns.Count]];

                        rowsToClear.ClearContents();
                        rowsToClear.ClearFormats();
                    }

                    // Add data
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        for (int j = 0; j < dataGridView1.Columns.Count; j++)
                        {
                            object value = dataGridView1.Rows[i].Cells[j].Value;
                            Excel.Range cell = (Excel.Range)worksheet.Cells[i + 2, j + 1];
                            cell.Value = value;

                            // Apply formatting based on DataGridView cell
                            if (dataGridView1.Rows[i].Cells[j].Style.ForeColor == System.Drawing.Color.Green)
                            {
                                cell.Font.Color = Excel.XlRgbColor.rgbGreen;
                            }
                            else if (dataGridView1.Rows[i].Cells[j].Style.ForeColor == System.Drawing.Color.Red)
                            {
                                cell.Font.Color = Excel.XlRgbColor.rgbRed;
                            }

                            // Copy alignment
                            if (dataGridView1.Rows[i].Cells[j].Style.Alignment == DataGridViewContentAlignment.MiddleRight)
                            {
                                cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                            }
                            else if (dataGridView1.Rows[i].Cells[j].Style.Alignment == DataGridViewContentAlignment.MiddleLeft)
                            {
                                cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                            }
                            else
                            {
                                cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                            }

                            // Format numbers
                            if (value != null && (value is double || value is decimal || value is int))
                            {
                                cell.NumberFormat = "0.00";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error exporting to Excel: {ex.Message}");
                }
            });

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileLists == null || FileLists.Count == 0)
            {
                MessageBox.Show("No files available to delete.", "Information",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var selectionForm = new Form())
            {
                selectionForm.Text = "Select Files to Delete";
                selectionForm.Width = 600;
                selectionForm.Height = 500;
                selectionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                selectionForm.StartPosition = FormStartPosition.CenterParent;
                selectionForm.BackColor = Color.White;
                selectionForm.Font = new Font("Segoe UI", 9);
                selectionForm.Icon = SystemIcons.WinLogo;

                var headerPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 50,
                    BackColor = Color.FromArgb(0, 120, 215) 
                };

                var headerLabel = new Label
                {
                    Text = "Select Files to Delete",
                    Dock = DockStyle.Fill,
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Padding = new Padding(15, 0, 0, 0)
                };
                headerPanel.Controls.Add(headerLabel);

                // Search box for filtering
                var searchBox = new TextBox
                {
                    Dock = DockStyle.Top,
                    Height = 30,
                    Margin = new Padding(10, 10, 10, 5),
                    Font = new Font("Segoe UI", 9),
                    Text = "Search Here..."
                    
                };

                // Modern list view with checkboxes
                var listView = new ListView
                {
                    Dock = DockStyle.Fill,
                    CheckBoxes = true,
                    View = View.Details,
                    FullRowSelect = true,
                    GridLines = false,
                    MultiSelect = false,
                    BorderStyle = BorderStyle.None,
                    BackColor = SystemColors.Window
                };

                // Modern column headers
                listView.Columns.Add("File Name", 300);
                listView.Columns.Add("Path", 250);

                // Add files to list view
                foreach (string filePath in FileLists)
                {
                    var item = new ListViewItem(Path.GetFileName(filePath));
                    item.SubItems.Add(filePath);
                    item.Tag = filePath; // Store full path in tag
                    listView.Items.Add(item);
                }

                // Selection controls panel
                var controlsPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 50,
                    BackColor = Color.FromArgb(240, 240, 240)
                };

                // Modern flat buttons
                var selectAllButton = new Button
                {
                    Text = "Select All",
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(0, 120, 215),
                    Height = 30,
                    Width = 80,
                    Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                    Margin = new Padding(10, 10, 0, 10)
                };


                var deleteButton = new Button
                {
                    Text = "Delete Selected",
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    Height = 30,
                    Width = 120,
                    Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                    Margin = new Padding(0, 10, 90, 10)
                };

                var cancelButton = new Button
                {
                    Text = "Cancel",
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(0, 120, 215),
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
                        MessageBox.Show("Please select at least one file to delete.",
                                        "No Selection",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                        return;
                    }

                    // Modern confirmation dialog
                    var confirmResult = MessageBox.Show($"Are you sure you want to delete {selectedFiles.Count} file(s)?",
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
                        resultMessage.AppendLine($"Successfully deleted {successCount} file(s).");

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
                    // Refresh your file list if needed
                    // FileLists.RemoveAll(f => !File.Exists(f));
                }
            }
        }
    }
}
