namespace ElevatorSystem;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    private Label lblTitle;
    private BuildingView buildingView;
    private ElevatorPanel panelA;
    private ElevatorPanel panelB;
    private Label lblStatusA;
    private Label lblStatusB;
    private Button btnLog;
    private Button btnRefreshLog;
    private Button btnClearLog;
    private DataGridView dgvLog;
    private Label lblDataPath;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Creates every control on the main window and places it on the form.
    /// </summary>
    private void InitializeComponent()
    {
        lblTitle = new Label();
        buildingView = new BuildingView();
        panelA = new ElevatorPanel();
        panelB = new ElevatorPanel();
        lblStatusA = new Label();
        lblStatusB = new Label();
        btnLog = new Button();
        btnRefreshLog = new Button();
        btnClearLog = new Button();
        dgvLog = new DataGridView();
        lblDataPath = new Label();
        ((System.ComponentModel.ISupportInitialize)dgvLog).BeginInit();
        SuspendLayout();

        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(240, 242, 245);
        ClientSize = new Size(1240, 860);
        Font = new Font("Segoe UI", 9F);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Elevator Control System - Assignment 1 (3 Floors, 2 Cars)";
        Load += Form1_Load;
        FormClosing += Form1_FormClosing;

        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI Semibold", 16F);
        lblTitle.Location = new Point(16, 10);
        lblTitle.Name = "lblTitle";
        lblTitle.Text = "Building elevator control - floors 1, 2 and 3";

        buildingView.Location = new Point(16, 48);
        buildingView.Name = "buildingView";
        buildingView.Size = new Size(520, 540);

        panelA.Location = new Point(550, 48);
        panelA.Name = "panelA";
        panelA.SetCarName(AppConstants.ElevatorNameA);

        panelB.Location = new Point(800, 48);
        panelB.Name = "panelB";
        panelB.SetCarName(AppConstants.ElevatorNameB);

        lblStatusA.BorderStyle = BorderStyle.FixedSingle;
        lblStatusA.Location = new Point(550, 490);
        lblStatusA.Name = "lblStatusA";
        lblStatusA.Size = new Size(230, 98);
        lblStatusA.Text = "Elevator A: Idle at Floor 1";
        lblStatusA.TextAlign = ContentAlignment.MiddleLeft;
        lblStatusA.Padding = new Padding(8, 0, 8, 0);

        lblStatusB.BorderStyle = BorderStyle.FixedSingle;
        lblStatusB.Location = new Point(800, 490);
        lblStatusB.Name = "lblStatusB";
        lblStatusB.Size = new Size(230, 98);
        lblStatusB.Text = "Elevator B: Idle at Floor 1";
        lblStatusB.TextAlign = ContentAlignment.MiddleLeft;
        lblStatusB.Padding = new Padding(8, 0, 8, 0);

        btnLog.Location = new Point(16, 600);
        btnLog.Name = "btnLog";
        btnLog.Size = new Size(110, 36);
        btnLog.Text = "Log";
        btnLog.UseVisualStyleBackColor = true;
        btnLog.Click += btnLog_Click;

        btnRefreshLog.Location = new Point(134, 600);
        btnRefreshLog.Name = "btnRefreshLog";
        btnRefreshLog.Size = new Size(110, 36);
        btnRefreshLog.Text = "Refresh";
        btnRefreshLog.UseVisualStyleBackColor = true;
        btnRefreshLog.Click += btnRefreshLog_Click;

        btnClearLog.Location = new Point(252, 600);
        btnClearLog.Name = "btnClearLog";
        btnClearLog.Size = new Size(110, 36);
        btnClearLog.Text = "Clear Log";
        btnClearLog.UseVisualStyleBackColor = true;
        btnClearLog.Click += btnClearLog_Click;

        lblDataPath.AutoSize = true;
        lblDataPath.ForeColor = Color.FromArgb(80, 80, 80);
        lblDataPath.Location = new Point(376, 608);
        lblDataPath.Name = "lblDataPath";
        lblDataPath.Text = "Database file: Data\\ElevatorLog.mdf (relative to the exe)";

        dgvLog.AllowUserToAddRows = false;
        dgvLog.AllowUserToDeleteRows = false;
        dgvLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvLog.BackgroundColor = Color.White;
        dgvLog.Location = new Point(16, 644);
        dgvLog.Name = "dgvLog";
        dgvLog.ReadOnly = true;
        dgvLog.RowHeadersVisible = false;
        dgvLog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvLog.Size = new Size(1200, 196);

        Controls.Add(lblTitle);
        Controls.Add(buildingView);
        Controls.Add(panelA);
        Controls.Add(panelB);
        Controls.Add(lblStatusA);
        Controls.Add(lblStatusB);
        Controls.Add(btnLog);
        Controls.Add(btnRefreshLog);
        Controls.Add(btnClearLog);
        Controls.Add(lblDataPath);
        Controls.Add(dgvLog);

        ((System.ComponentModel.ISupportInitialize)dgvLog).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
