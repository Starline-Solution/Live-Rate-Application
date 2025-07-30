using Live_Rate_Application;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public class AlertCreationPanel : Form
{
    List<string> columns = new List<string>();
    List<string> symbols = new List<string>();

    private Label lblHeader;
    private Label lblAlertInfo;
    private Label lblAlertName;
    private TextBox txtAlertName;
    private Label lblMessage;
    private TextBox txtMessage;

    private Label lblSymbol;
    private Label lblColumn;
    private Label lblCondition;
    private Label lblValue;

    private ComboBox cmbSymbol;
    private ComboBox cmbColumn;
    private ComboBox cmbCondition;
    private TextBox txtValue;

    private Label lblNotifications;
    private CheckBox chkStatusBar;
    private CheckBox chkPopup;
    private CheckBox chkBeep;

    private Button btnSave;

    public AlertCreationPanel()
    {
        DefaultDataLoad();
        InitializeComponents();
    }

    public void DefaultDataLoad() 
    {
        Live_Rate live_Rate = Live_Rate.CurrentInstance;
        columns = live_Rate.columnPreferences;
        symbols = live_Rate.symbolMaster;
        columns.Remove("Symbol");
        columns.Remove("DateTime");

        // If symbol list is empty, get symbols from DataTable
        if (symbols.Count == 0 && live_Rate.marketDataTable != null)
        {
            symbols = live_Rate.marketDataTable
                .AsEnumerable()
                .Select(row => row.Field<string>("Symbol"))
                .Distinct()
                .ToList();
        }

    }


    private void InitializeComponents()
    {
        // Form setup
        this.Text = "Add Alert";
        this.Size = new Size(480, 450);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.White;

        // Header
        lblHeader = new Label
        {
            Text = "Create New Alert",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Location = new Point(20, 15),
            AutoSize = true
        };

        // GroupBox for alert details
        GroupBox grpAlertInfo = new GroupBox
        {
            Text = "Alert Details",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Location = new Point(20, 50),
            Size = new Size(420, 130),
            BackColor = Color.White
        };

        lblAlertName = new Label { Text = "Name", Location = new Point(20, 30), AutoSize = true };
        txtAlertName = new TextBox { Location = new Point(120, 27), Width = 250, Text = "GOLD LABEL" };

        lblMessage = new Label { Text = "Message", Location = new Point(20, 65), AutoSize = true };
        txtMessage = new TextBox { Location = new Point(120, 62), Width = 250, Text = "Gold Volume Reach" };

        grpAlertInfo.Controls.AddRange(new Control[] { lblAlertName, txtAlertName, lblMessage, txtMessage });

        // GroupBox for condition
        GroupBox grpCondition = new GroupBox
        {
            Text = "Trigger Condition",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Location = new Point(20, 190),
            Size = new Size(420, 80),
            BackColor = Color.White
        };

        cmbSymbol = new ComboBox { Location = new Point(20, 35), Width = 80, DropDownStyle = ComboBoxStyle.DropDown };
        cmbSymbol.Items.AddRange(symbols.ToArray());
        if (symbols.Count > 0)
            cmbSymbol.SelectedIndex = 0;

        cmbColumn = new ComboBox { Location = new Point(110, 35), Width = 90, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbColumn.Items.AddRange(columns.ToArray());
        if (columns.Count > 0)
            cmbColumn.SelectedIndex = 0;

        cmbCondition = new ComboBox { Location = new Point(210, 35), Width = 80, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbCondition.Items.AddRange(new[] { ">", "<", "=", ">=", "<=", "!=" });
        cmbCondition.SelectedIndex = 0;

        txtValue = new TextBox { Location = new Point(300, 35), Width = 90, Text = "3346.5000" };

        grpCondition.Controls.AddRange(new Control[] { cmbSymbol, cmbColumn, cmbCondition, txtValue });

        // Notifications section
        GroupBox grpNotifications = new GroupBox
        {
            Text = "Notifications",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Location = new Point(20, 280),
            Size = new Size(420, 60),
            BackColor = Color.White
        };

        chkStatusBar = new CheckBox { Text = "Status Bar", Location = new Point(20, 25), AutoSize = true, Checked = true, FlatStyle = FlatStyle.Flat };
        chkPopup = new CheckBox { Text = "Popup", Location = new Point(140, 25), AutoSize = true, Checked = true, FlatStyle = FlatStyle.Flat };
        chkBeep = new CheckBox { Text = "Beep", Location = new Point(240, 25), AutoSize = true, Checked = true, FlatStyle = FlatStyle.Flat };

        grpNotifications.Controls.AddRange(new Control[] { chkStatusBar, chkPopup, chkBeep });

        // Save Button
        btnSave = new Button
        {
            Text = "Save Alert",
            Location = new Point(160, 350),
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (sender, e) => SaveAlert();

        // Add all controls
        this.Controls.AddRange(new Control[]
        {
        lblHeader,
        grpAlertInfo,
        grpCondition,
        grpNotifications,
        btnSave
        });

        this.BringToFront();
    }

    private void SaveAlert()
    {
        // Implement save logic here
        MessageBox.Show("Alert saved successfully!");
        this.Close();
    }

    // Public properties to access alert data
    public string AlertName => txtAlertName.Text;
    public string Message => txtMessage.Text;
    public string Symbol => cmbSymbol.Text;
    public string Column => cmbColumn.Text;
    public string Condition => cmbCondition.Text;
    public string Value => txtValue.Text;
    public bool StatusBarNotification => chkStatusBar.Checked;
    public bool PopupNotification => chkPopup.Checked;
    public bool BeepNotification => chkBeep.Checked;
}