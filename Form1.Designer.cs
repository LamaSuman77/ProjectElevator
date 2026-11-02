namespace ElevatorSystem;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    private Label lblTitle;
    private GroupBox grpHall;
    private Button btnRequestFloor1;
    private Button btnRequestFloor2;
    private Button btnRequestFloor3;

    private Label lblShaftTitleA;
    private Label lblShaftTitleB;
    private Panel pnlShaftA;
    private Panel pnlShaftB;
    private Panel pnlCarA;
    private Panel pnlCarB;
    private PictureBox picCarA;
    private PictureBox picCarB;
    private Label lblCarA;
    private Label lblCarB;
    private Label lblLineA1;
    private Label lblLineA2;
    private Label lblLineA3;
    private Label lblLineB1;
    private Label lblLineB2;
    private Label lblLineB3;

    private GroupBox grpFloorDisplays;
    private Label lblFloorStatusA3;
    private Label lblFloorStatusA2;
    private Label lblFloorStatusA1;
    private Label lblFloorStatusB3;
    private Label lblFloorStatusB2;
    private Label lblFloorStatusB1;

    private GroupBox grpPanelA;
    private Label lblCurrentFloorA;
    private Label lblArrowA;
    private Button btnPanelAFloor1;
    private Button btnPanelAFloor2;
    private Button btnPanelAFloor3;
    private Button btnEmergencyA;
    private Button btnResetA;

    private GroupBox grpPanelB;
    private Label lblCurrentFloorB;
    private Label lblArrowB;
    private Button btnPanelBFloor1;
    private Button btnPanelBFloor2;
    private Button btnPanelBFloor3;
    private Button btnEmergencyB;
    private Button btnResetB;

    private Label lblStatusA;
    private Label lblStatusB;
    private Button btnLog;
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
        grpHall = new GroupBox();
        btnRequestFloor3 = new Button();
        btnRequestFloor2 = new Button();
        btnRequestFloor1 = new Button();
        lblShaftTitleA = new Label();
        lblShaftTitleB = new Label();
        pnlShaftA = new Panel();
        pnlCarA = new Panel();
        picCarA = new PictureBox();
        lblCarA = new Label();
        lblLineA3 = new Label();
        lblLineA2 = new Label();
        lblLineA1 = new Label();
        pnlShaftB = new Panel();
        pnlCarB = new Panel();
        picCarB = new PictureBox();
        lblCarB = new Label();
        lblLineB3 = new Label();
        lblLineB2 = new Label();
        lblLineB1 = new Label();
        grpFloorDisplays = new GroupBox();
        lblFloorStatusA3 = new Label();
        lblFloorStatusA2 = new Label();
        lblFloorStatusA1 = new Label();
        lblFloorStatusB3 = new Label();
        lblFloorStatusB2 = new Label();
        lblFloorStatusB1 = new Label();
        grpPanelA = new GroupBox();
        lblCurrentFloorA = new Label();
        lblArrowA = new Label();
        btnPanelAFloor1 = new Button();
        btnPanelAFloor2 = new Button();
        btnPanelAFloor3 = new Button();
        btnEmergencyA = new Button();
        btnResetA = new Button();
        grpPanelB = new GroupBox();
        lblCurrentFloorB = new Label();
        lblArrowB = new Label();
        btnPanelBFloor1 = new Button();
        btnPanelBFloor2 = new Button();
        btnPanelBFloor3 = new Button();
        btnEmergencyB = new Button();
        btnResetB = new Button();
        lblStatusA = new Label();
        lblStatusB = new Label();
        btnLog = new Button();
        dgvLog = new DataGridView();
        lblDataPath = new Label();
        grpHall.SuspendLayout();
        pnlShaftA.SuspendLayout();
        pnlCarA.SuspendLayout();
        pnlShaftB.SuspendLayout();
        pnlCarB.SuspendLayout();
        grpFloorDisplays.SuspendLayout();
        grpPanelA.SuspendLayout();
        grpPanelB.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvLog).BeginInit();
        SuspendLayout();

        // Form
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(240, 242, 245);
        ClientSize = new Size(1180, 780);
        Font = new Font("Segoe UI", 9F);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Elevator Control System - Assignment 1 (3 Floors, 2 Cars)";
        Load += Form1_Load;
        FormClosing += Form1_FormClosing;

        // Title
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI Semibold", 16F);
        lblTitle.Location = new Point(20, 12);
        lblTitle.Name = "lblTitle";
        lblTitle.Text = "Building elevator control - floors 1, 2 and 3";

        // Hall requests
        grpHall.Controls.Add(btnRequestFloor3);
        grpHall.Controls.Add(btnRequestFloor2);
        grpHall.Controls.Add(btnRequestFloor1);
        grpHall.Location = new Point(20, 55);
        grpHall.Name = "grpHall";
        grpHall.Size = new Size(150, 430);
        grpHall.Text = "Outside requests";

        btnRequestFloor3.Location = new Point(18, 40);
        btnRequestFloor3.Name = "btnRequestFloor3";
        btnRequestFloor3.Size = new Size(114, 70);
        btnRequestFloor3.Text = "Floor 3";
        btnRequestFloor3.UseVisualStyleBackColor = true;
        btnRequestFloor3.Click += btnRequestFloor3_Click;

        btnRequestFloor2.Location = new Point(18, 175);
        btnRequestFloor2.Name = "btnRequestFloor2";
        btnRequestFloor2.Size = new Size(114, 70);
        btnRequestFloor2.Text = "Floor 2";
        btnRequestFloor2.UseVisualStyleBackColor = true;
        btnRequestFloor2.Click += btnRequestFloor2_Click;

        btnRequestFloor1.Location = new Point(18, 310);
        btnRequestFloor1.Name = "btnRequestFloor1";
        btnRequestFloor1.Size = new Size(114, 70);
        btnRequestFloor1.Text = "Floor 1";
        btnRequestFloor1.UseVisualStyleBackColor = true;
        btnRequestFloor1.Click += btnRequestFloor1_Click;

        // Shaft titles
        lblShaftTitleA.AutoSize = true;
        lblShaftTitleA.Font = new Font("Segoe UI Semibold", 10F);
        lblShaftTitleA.Location = new Point(200, 55);
        lblShaftTitleA.Name = "lblShaftTitleA";
        lblShaftTitleA.Text = "Shaft A";

        lblShaftTitleB.AutoSize = true;
        lblShaftTitleB.Font = new Font("Segoe UI Semibold", 10F);
        lblShaftTitleB.Location = new Point(330, 55);
        lblShaftTitleB.Name = "lblShaftTitleB";
        lblShaftTitleB.Text = "Shaft B";

        // Shaft A
        pnlShaftA.BackColor = Color.FromArgb(55, 65, 80);
        pnlShaftA.BorderStyle = BorderStyle.FixedSingle;
        pnlShaftA.Controls.Add(pnlCarA);
        pnlShaftA.Controls.Add(lblLineA3);
        pnlShaftA.Controls.Add(lblLineA2);
        pnlShaftA.Controls.Add(lblLineA1);
        pnlShaftA.Location = new Point(190, 80);
        pnlShaftA.Name = "pnlShaftA";
        pnlShaftA.Size = new Size(110, 405);

        lblLineA3.BackColor = Color.FromArgb(90, 100, 115);
        lblLineA3.ForeColor = Color.White;
        lblLineA3.Location = new Point(0, 18);
        lblLineA3.Name = "lblLineA3";
        lblLineA3.Size = new Size(108, 18);
        lblLineA3.Text = "  3";
        lblLineA3.TextAlign = ContentAlignment.MiddleLeft;

        lblLineA2.BackColor = Color.FromArgb(90, 100, 115);
        lblLineA2.ForeColor = Color.White;
        lblLineA2.Location = new Point(0, 188);
        lblLineA2.Name = "lblLineA2";
        lblLineA2.Size = new Size(108, 18);
        lblLineA2.Text = "  2";
        lblLineA2.TextAlign = ContentAlignment.MiddleLeft;

        lblLineA1.BackColor = Color.FromArgb(90, 100, 115);
        lblLineA1.ForeColor = Color.White;
        lblLineA1.Location = new Point(0, 355);
        lblLineA1.Name = "lblLineA1";
        lblLineA1.Size = new Size(108, 18);
        lblLineA1.Text = "  1";
        lblLineA1.TextAlign = ContentAlignment.MiddleLeft;

        pnlCarA.BackColor = Color.FromArgb(46, 134, 193);
        pnlCarA.BorderStyle = BorderStyle.FixedSingle;
        pnlCarA.Controls.Add(picCarA);
        pnlCarA.Controls.Add(lblCarA);
        pnlCarA.Location = new Point(28, 330);
        pnlCarA.Name = "pnlCarA";
        pnlCarA.Size = new Size(54, 58);
        pnlCarA.BringToFront();

        picCarA.BackColor = Color.Transparent;
        picCarA.Dock = DockStyle.Fill;
        picCarA.Name = "picCarA";
        picCarA.SizeMode = PictureBoxSizeMode.Zoom;

        lblCarA.Dock = DockStyle.Fill;
        lblCarA.Font = new Font("Segoe UI Semibold", 12F);
        lblCarA.ForeColor = Color.White;
        lblCarA.Name = "lblCarA";
        lblCarA.Text = "A";
        lblCarA.TextAlign = ContentAlignment.MiddleCenter;

        // Shaft B
        pnlShaftB.BackColor = Color.FromArgb(55, 65, 80);
        pnlShaftB.BorderStyle = BorderStyle.FixedSingle;
        pnlShaftB.Controls.Add(pnlCarB);
        pnlShaftB.Controls.Add(lblLineB3);
        pnlShaftB.Controls.Add(lblLineB2);
        pnlShaftB.Controls.Add(lblLineB1);
        pnlShaftB.Location = new Point(320, 80);
        pnlShaftB.Name = "pnlShaftB";
        pnlShaftB.Size = new Size(110, 405);

        lblLineB3.BackColor = Color.FromArgb(90, 100, 115);
        lblLineB3.ForeColor = Color.White;
        lblLineB3.Location = new Point(0, 18);
        lblLineB3.Name = "lblLineB3";
        lblLineB3.Size = new Size(108, 18);
        lblLineB3.Text = "  3";
        lblLineB3.TextAlign = ContentAlignment.MiddleLeft;

        lblLineB2.BackColor = Color.FromArgb(90, 100, 115);
        lblLineB2.ForeColor = Color.White;
        lblLineB2.Location = new Point(0, 188);
        lblLineB2.Name = "lblLineB2";
        lblLineB2.Size = new Size(108, 18);
        lblLineB2.Text = "  2";
        lblLineB2.TextAlign = ContentAlignment.MiddleLeft;

        lblLineB1.BackColor = Color.FromArgb(90, 100, 115);
        lblLineB1.ForeColor = Color.White;
        lblLineB1.Location = new Point(0, 355);
        lblLineB1.Name = "lblLineB1";
        lblLineB1.Size = new Size(108, 18);
        lblLineB1.Text = "  1";
        lblLineB1.TextAlign = ContentAlignment.MiddleLeft;

        pnlCarB.BackColor = Color.FromArgb(39, 174, 96);
        pnlCarB.BorderStyle = BorderStyle.FixedSingle;
        pnlCarB.Controls.Add(picCarB);
        pnlCarB.Controls.Add(lblCarB);
        pnlCarB.Location = new Point(28, 330);
        pnlCarB.Name = "pnlCarB";
        pnlCarB.Size = new Size(54, 58);

        picCarB.BackColor = Color.Transparent;
        picCarB.Dock = DockStyle.Fill;
        picCarB.Name = "picCarB";
        picCarB.SizeMode = PictureBoxSizeMode.Zoom;

        lblCarB.Dock = DockStyle.Fill;
        lblCarB.Font = new Font("Segoe UI Semibold", 12F);
        lblCarB.ForeColor = Color.White;
        lblCarB.Name = "lblCarB";
        lblCarB.Text = "B";
        lblCarB.TextAlign = ContentAlignment.MiddleCenter;

        // Floor status displays (3 per elevator)
        grpFloorDisplays.Controls.Add(lblFloorStatusA3);
        grpFloorDisplays.Controls.Add(lblFloorStatusA2);
        grpFloorDisplays.Controls.Add(lblFloorStatusA1);
        grpFloorDisplays.Controls.Add(lblFloorStatusB3);
        grpFloorDisplays.Controls.Add(lblFloorStatusB2);
        grpFloorDisplays.Controls.Add(lblFloorStatusB1);
        grpFloorDisplays.Location = new Point(450, 55);
        grpFloorDisplays.Name = "grpFloorDisplays";
        grpFloorDisplays.Size = new Size(250, 250);
        grpFloorDisplays.Text = "Floor status displays";

        lblFloorStatusA3.BorderStyle = BorderStyle.FixedSingle;
        lblFloorStatusA3.Location = new Point(15, 35);
        lblFloorStatusA3.Name = "lblFloorStatusA3";
        lblFloorStatusA3.Size = new Size(105, 55);
        lblFloorStatusA3.TextAlign = ContentAlignment.MiddleCenter;

        lblFloorStatusA2.BorderStyle = BorderStyle.FixedSingle;
        lblFloorStatusA2.Location = new Point(15, 100);
        lblFloorStatusA2.Name = "lblFloorStatusA2";
        lblFloorStatusA2.Size = new Size(105, 55);
        lblFloorStatusA2.TextAlign = ContentAlignment.MiddleCenter;

        lblFloorStatusA1.BorderStyle = BorderStyle.FixedSingle;
        lblFloorStatusA1.Location = new Point(15, 165);
        lblFloorStatusA1.Name = "lblFloorStatusA1";
        lblFloorStatusA1.Size = new Size(105, 55);
        lblFloorStatusA1.TextAlign = ContentAlignment.MiddleCenter;

        lblFloorStatusB3.BorderStyle = BorderStyle.FixedSingle;
        lblFloorStatusB3.Location = new Point(130, 35);
        lblFloorStatusB3.Name = "lblFloorStatusB3";
        lblFloorStatusB3.Size = new Size(105, 55);
        lblFloorStatusB3.TextAlign = ContentAlignment.MiddleCenter;

        lblFloorStatusB2.BorderStyle = BorderStyle.FixedSingle;
        lblFloorStatusB2.Location = new Point(130, 100);
        lblFloorStatusB2.Name = "lblFloorStatusB2";
        lblFloorStatusB2.Size = new Size(105, 55);
        lblFloorStatusB2.TextAlign = ContentAlignment.MiddleCenter;

        lblFloorStatusB1.BorderStyle = BorderStyle.FixedSingle;
        lblFloorStatusB1.Location = new Point(130, 165);
        lblFloorStatusB1.Name = "lblFloorStatusB1";
        lblFloorStatusB1.Size = new Size(105, 55);
        lblFloorStatusB1.TextAlign = ContentAlignment.MiddleCenter;

        // Control panel A
        grpPanelA.Controls.Add(lblCurrentFloorA);
        grpPanelA.Controls.Add(lblArrowA);
        grpPanelA.Controls.Add(btnPanelAFloor1);
        grpPanelA.Controls.Add(btnPanelAFloor2);
        grpPanelA.Controls.Add(btnPanelAFloor3);
        grpPanelA.Controls.Add(btnEmergencyA);
        grpPanelA.Controls.Add(btnResetA);
        grpPanelA.Location = new Point(720, 55);
        grpPanelA.Name = "grpPanelA";
        grpPanelA.Size = new Size(210, 250);
        grpPanelA.Text = "Control panel A";

        lblCurrentFloorA.BackColor = Color.Black;
        lblCurrentFloorA.Font = new Font("Consolas", 22F, FontStyle.Bold);
        lblCurrentFloorA.ForeColor = Color.Lime;
        lblCurrentFloorA.Location = new Point(20, 30);
        lblCurrentFloorA.Name = "lblCurrentFloorA";
        lblCurrentFloorA.Size = new Size(100, 50);
        lblCurrentFloorA.Text = "1";
        lblCurrentFloorA.TextAlign = ContentAlignment.MiddleCenter;

        lblArrowA.BackColor = Color.Black;
        lblArrowA.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        lblArrowA.ForeColor = Color.Lime;
        lblArrowA.Location = new Point(125, 30);
        lblArrowA.Name = "lblArrowA";
        lblArrowA.Size = new Size(60, 50);
        lblArrowA.Text = "-";
        lblArrowA.TextAlign = ContentAlignment.MiddleCenter;

        btnPanelAFloor1.Location = new Point(20, 100);
        btnPanelAFloor1.Name = "btnPanelAFloor1";
        btnPanelAFloor1.Size = new Size(50, 50);
        btnPanelAFloor1.Text = "1";
        btnPanelAFloor1.UseVisualStyleBackColor = true;
        btnPanelAFloor1.Click += btnPanelAFloor1_Click;

        btnPanelAFloor2.Location = new Point(80, 100);
        btnPanelAFloor2.Name = "btnPanelAFloor2";
        btnPanelAFloor2.Size = new Size(50, 50);
        btnPanelAFloor2.Text = "2";
        btnPanelAFloor2.UseVisualStyleBackColor = true;
        btnPanelAFloor2.Click += btnPanelAFloor2_Click;

        btnPanelAFloor3.Location = new Point(140, 100);
        btnPanelAFloor3.Name = "btnPanelAFloor3";
        btnPanelAFloor3.Size = new Size(50, 50);
        btnPanelAFloor3.Text = "3";
        btnPanelAFloor3.UseVisualStyleBackColor = true;
        btnPanelAFloor3.Click += btnPanelAFloor3_Click;

        btnEmergencyA.BackColor = Color.FromArgb(192, 57, 43);
        btnEmergencyA.ForeColor = Color.White;
        btnEmergencyA.Location = new Point(20, 162);
        btnEmergencyA.Name = "btnEmergencyA";
        btnEmergencyA.Size = new Size(80, 36);
        btnEmergencyA.Text = "Emergency";
        btnEmergencyA.UseVisualStyleBackColor = false;
        btnEmergencyA.Click += btnEmergencyA_Click;

        btnResetA.Location = new Point(110, 162);
        btnResetA.Name = "btnResetA";
        btnResetA.Size = new Size(75, 36);
        btnResetA.Text = "Reset";
        btnResetA.UseVisualStyleBackColor = true;
        btnResetA.Click += btnResetA_Click;

        // Control panel B
        grpPanelB.Controls.Add(lblCurrentFloorB);
        grpPanelB.Controls.Add(lblArrowB);
        grpPanelB.Controls.Add(btnPanelBFloor1);
        grpPanelB.Controls.Add(btnPanelBFloor2);
        grpPanelB.Controls.Add(btnPanelBFloor3);
        grpPanelB.Controls.Add(btnEmergencyB);
        grpPanelB.Controls.Add(btnResetB);
        grpPanelB.Location = new Point(950, 55);
        grpPanelB.Name = "grpPanelB";
        grpPanelB.Size = new Size(210, 250);
        grpPanelB.Text = "Control panel B";

        lblCurrentFloorB.BackColor = Color.Black;
        lblCurrentFloorB.Font = new Font("Consolas", 22F, FontStyle.Bold);
        lblCurrentFloorB.ForeColor = Color.Lime;
        lblCurrentFloorB.Location = new Point(20, 30);
        lblCurrentFloorB.Name = "lblCurrentFloorB";
        lblCurrentFloorB.Size = new Size(100, 50);
        lblCurrentFloorB.Text = "1";
        lblCurrentFloorB.TextAlign = ContentAlignment.MiddleCenter;

        lblArrowB.BackColor = Color.Black;
        lblArrowB.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        lblArrowB.ForeColor = Color.Lime;
        lblArrowB.Location = new Point(125, 30);
        lblArrowB.Name = "lblArrowB";
        lblArrowB.Size = new Size(60, 50);
        lblArrowB.Text = "-";
        lblArrowB.TextAlign = ContentAlignment.MiddleCenter;

        btnPanelBFloor1.Location = new Point(20, 100);
        btnPanelBFloor1.Name = "btnPanelBFloor1";
        btnPanelBFloor1.Size = new Size(50, 50);
        btnPanelBFloor1.Text = "1";
        btnPanelBFloor1.UseVisualStyleBackColor = true;
        btnPanelBFloor1.Click += btnPanelBFloor1_Click;

        btnPanelBFloor2.Location = new Point(80, 100);
        btnPanelBFloor2.Name = "btnPanelBFloor2";
        btnPanelBFloor2.Size = new Size(50, 50);
        btnPanelBFloor2.Text = "2";
        btnPanelBFloor2.UseVisualStyleBackColor = true;
        btnPanelBFloor2.Click += btnPanelBFloor2_Click;

        btnPanelBFloor3.Location = new Point(140, 100);
        btnPanelBFloor3.Name = "btnPanelBFloor3";
        btnPanelBFloor3.Size = new Size(50, 50);
        btnPanelBFloor3.Text = "3";
        btnPanelBFloor3.UseVisualStyleBackColor = true;
        btnPanelBFloor3.Click += btnPanelBFloor3_Click;

        btnEmergencyB.BackColor = Color.FromArgb(192, 57, 43);
        btnEmergencyB.ForeColor = Color.White;
        btnEmergencyB.Location = new Point(20, 162);
        btnEmergencyB.Name = "btnEmergencyB";
        btnEmergencyB.Size = new Size(80, 36);
        btnEmergencyB.Text = "Emergency";
        btnEmergencyB.UseVisualStyleBackColor = false;
        btnEmergencyB.Click += btnEmergencyB_Click;

        btnResetB.Location = new Point(110, 162);
        btnResetB.Name = "btnResetB";
        btnResetB.Size = new Size(75, 36);
        btnResetB.Text = "Reset";
        btnResetB.UseVisualStyleBackColor = true;
        btnResetB.Click += btnResetB_Click;

        // Movement labels
        lblStatusA.BorderStyle = BorderStyle.FixedSingle;
        lblStatusA.Location = new Point(450, 320);
        lblStatusA.Name = "lblStatusA";
        lblStatusA.Size = new Size(350, 70);
        lblStatusA.Text = "Elevator A: Idle at Floor 1";
        lblStatusA.TextAlign = ContentAlignment.MiddleLeft;
        lblStatusA.Padding = new Padding(8, 0, 8, 0);

        lblStatusB.BorderStyle = BorderStyle.FixedSingle;
        lblStatusB.Location = new Point(810, 320);
        lblStatusB.Name = "lblStatusB";
        lblStatusB.Size = new Size(350, 70);
        lblStatusB.Text = "Elevator B: Idle at Floor 1";
        lblStatusB.TextAlign = ContentAlignment.MiddleLeft;
        lblStatusB.Padding = new Padding(8, 0, 8, 0);

        btnLog.Location = new Point(20, 500);
        btnLog.Name = "btnLog";
        btnLog.Size = new Size(150, 36);
        btnLog.Text = "Log";
        btnLog.UseVisualStyleBackColor = true;
        btnLog.Click += btnLog_Click;

        lblDataPath.AutoSize = true;
        lblDataPath.ForeColor = Color.FromArgb(80, 80, 80);
        lblDataPath.Location = new Point(180, 508);
        lblDataPath.Name = "lblDataPath";
        lblDataPath.Text = "Database file: Data\\ElevatorLog.mdf (relative to the exe)";

        dgvLog.AllowUserToAddRows = false;
        dgvLog.AllowUserToDeleteRows = false;
        dgvLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvLog.BackgroundColor = Color.White;
        dgvLog.Location = new Point(20, 545);
        dgvLog.Name = "dgvLog";
        dgvLog.ReadOnly = true;
        dgvLog.RowHeadersVisible = false;
        dgvLog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvLog.Size = new Size(1140, 215);

        Controls.Add(lblTitle);
        Controls.Add(grpHall);
        Controls.Add(lblShaftTitleA);
        Controls.Add(lblShaftTitleB);
        Controls.Add(pnlShaftA);
        Controls.Add(pnlShaftB);
        Controls.Add(grpFloorDisplays);
        Controls.Add(grpPanelA);
        Controls.Add(grpPanelB);
        Controls.Add(lblStatusA);
        Controls.Add(lblStatusB);
        Controls.Add(btnLog);
        Controls.Add(lblDataPath);
        Controls.Add(dgvLog);

        grpHall.ResumeLayout(false);
        pnlCarA.ResumeLayout(false);
        pnlShaftA.ResumeLayout(false);
        pnlCarB.ResumeLayout(false);
        pnlShaftB.ResumeLayout(false);
        grpFloorDisplays.ResumeLayout(false);
        grpPanelA.ResumeLayout(false);
        grpPanelB.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvLog).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
