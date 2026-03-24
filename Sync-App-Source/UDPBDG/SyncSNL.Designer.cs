namespace UDPBDG;

partial class SyncSNL
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        LogLabel = new Label();
        LogPanel = new Panel();
        PS2IPLabel = new Label();
        PS2IPTextBox = new TextBox();
        ConnectButton = new Button();
        ConnectLabel = new Label();
        InstallMC0Button = new Button();
        InstallMC1Button = new Button();
        InstallMassButton = new Button();
        VMCCheckbox = new CheckBox();
        SyncButton = new Button();
        FreeSpaceLabel = new Label();
        AutoStartCheckbox = new CheckBox();
        LogPanel.SuspendLayout();
        SuspendLayout();
        // 
        // LogLabel
        // 
        LogLabel.AutoSize = true;
        LogLabel.Location = new Point(3, 0);
        LogLabel.Name = "LogLabel";
        LogLabel.Size = new Size(0, 20);
        LogLabel.TabIndex = 0;
        // 
        // LogPanel
        // 
        LogPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        LogPanel.AutoScroll = true;
        LogPanel.Controls.Add(LogLabel);
        LogPanel.Location = new Point(0, 350);
        LogPanel.Name = "LogPanel";
        LogPanel.Size = new Size(624, 91);
        LogPanel.TabIndex = 1;
        // 
        // PS2IPLabel
        // 
        PS2IPLabel.AutoSize = true;
        PS2IPLabel.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        PS2IPLabel.Location = new Point(12, 15);
        PS2IPLabel.Name = "PS2IPLabel";
        PS2IPLabel.Size = new Size(231, 25);
        PS2IPLabel.TabIndex = 3;
        PS2IPLabel.Text = "Enter the PS2's IP address:";
        // 
        // PS2IPTextBox
        // 
        PS2IPTextBox.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        PS2IPTextBox.Location = new Point(249, 12);
        PS2IPTextBox.MaxLength = 15;
        PS2IPTextBox.Name = "PS2IPTextBox";
        PS2IPTextBox.PlaceholderText = "Type Here . . .";
        PS2IPTextBox.Size = new Size(140, 33);
        PS2IPTextBox.TabIndex = 8;
        PS2IPTextBox.Text = "192.168.0.10";
        // 
        // ConnectButton
        // 
        ConnectButton.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        ConnectButton.Location = new Point(395, 11);
        ConnectButton.Name = "ConnectButton";
        ConnectButton.Size = new Size(217, 33);
        ConnectButton.TabIndex = 1;
        ConnectButton.Text = "Connect via PS2Net";
        ConnectButton.UseVisualStyleBackColor = true;
        ConnectButton.Click += ConnectButton_Click;
        // 
        // ConnectLabel
        // 
        ConnectLabel.AutoSize = true;
        ConnectLabel.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        ConnectLabel.Location = new Point(486, 47);
        ConnectLabel.Name = "ConnectLabel";
        ConnectLabel.Size = new Size(126, 25);
        ConnectLabel.TabIndex = 4;
        ConnectLabel.Text = "Disconnected";
        // 
        // InstallMC0Button
        // 
        InstallMC0Button.Enabled = false;
        InstallMC0Button.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        InstallMC0Button.Location = new Point(9, 125);
        InstallMC0Button.Name = "InstallMC0Button";
        InstallMC0Button.Size = new Size(600, 33);
        InstallMC0Button.TabIndex = 3;
        InstallMC0Button.Text = "Install to Memory Card Slot 1 (mc0)";
        InstallMC0Button.UseVisualStyleBackColor = true;
        InstallMC0Button.Click += InstallMC0Button_Click;
        // 
        // InstallMC1Button
        // 
        InstallMC1Button.Enabled = false;
        InstallMC1Button.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        InstallMC1Button.Location = new Point(9, 164);
        InstallMC1Button.Name = "InstallMC1Button";
        InstallMC1Button.Size = new Size(600, 33);
        InstallMC1Button.TabIndex = 4;
        InstallMC1Button.Text = "Install to Memory Card Slot 2 (mc1)";
        InstallMC1Button.UseVisualStyleBackColor = true;
        InstallMC1Button.Click += InstallMC1Button_Click;
        // 
        // InstallMassButton
        // 
        InstallMassButton.Enabled = false;
        InstallMassButton.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        InstallMassButton.Location = new Point(9, 203);
        InstallMassButton.Name = "InstallMassButton";
        InstallMassButton.Size = new Size(600, 33);
        InstallMassButton.TabIndex = 5;
        InstallMassButton.Text = "Install to USB Drive (mass)";
        InstallMassButton.UseVisualStyleBackColor = true;
        InstallMassButton.Click += InstallMassButton_Click;
        // 
        // VMCCheckbox
        // 
        VMCCheckbox.AutoSize = true;
        VMCCheckbox.Enabled = false;
        VMCCheckbox.Location = new Point(150, 281);
        VMCCheckbox.Name = "VMCCheckbox";
        VMCCheckbox.Size = new Size(283, 24);
        VMCCheckbox.TabIndex = 6;
        VMCCheckbox.Text = "Use Virtual Memory Cards (exFAT only)";
        VMCCheckbox.UseVisualStyleBackColor = true;
        // 
        // SyncButton
        // 
        SyncButton.Enabled = false;
        SyncButton.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        SyncButton.Location = new Point(9, 311);
        SyncButton.Name = "SyncButton";
        SyncButton.Size = new Size(600, 33);
        SyncButton.TabIndex = 7;
        SyncButton.Text = "Sync Game List, UDP mode, and IP address";
        SyncButton.UseVisualStyleBackColor = true;
        SyncButton.Click += SyncButton_Click;
        // 
        // FreeSpaceLabel
        // 
        FreeSpaceLabel.AutoSize = true;
        FreeSpaceLabel.Location = new Point(12, 102);
        FreeSpaceLabel.Name = "FreeSpaceLabel";
        FreeSpaceLabel.Size = new Size(504, 20);
        FreeSpaceLabel.TabIndex = 10;
        FreeSpaceLabel.Text = "*The memory card must have at least 1.2MB (1280KB) of unused free space\r\n";
        // 
        // AutoStartCheckbox
        // 
        AutoStartCheckbox.AutoSize = true;
        AutoStartCheckbox.Location = new Point(150, 75);
        AutoStartCheckbox.Name = "AutoStartCheckbox";
        AutoStartCheckbox.Size = new Size(345, 24);
        AutoStartCheckbox.TabIndex = 2;
        AutoStartCheckbox.Text = "Set PS2BBL to autostart Simple Neutrino Loader";
        AutoStartCheckbox.UseVisualStyleBackColor = true;
        // 
        // SyncSNL
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoScroll = true;
        ClientSize = new Size(624, 441);
        Controls.Add(AutoStartCheckbox);
        Controls.Add(FreeSpaceLabel);
        Controls.Add(SyncButton);
        Controls.Add(VMCCheckbox);
        Controls.Add(InstallMassButton);
        Controls.Add(InstallMC1Button);
        Controls.Add(InstallMC0Button);
        Controls.Add(ConnectLabel);
        Controls.Add(ConnectButton);
        Controls.Add(PS2IPTextBox);
        Controls.Add(PS2IPLabel);
        Controls.Add(LogPanel);
        Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        HelpButton = true;
        Icon = Resources.Icon;
        Margin = new Padding(3, 4, 3, 4);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "SyncSNL";
        Text = "Sync to Simple Neutrino Loader";
        HelpButtonClicked += SyncSNL_HelpButtonClicked;
        LogPanel.ResumeLayout(false);
        LogPanel.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label LogLabel;
    private Panel LogPanel;
    private Label PS2IPLabel;
    private TextBox PS2IPTextBox;
    private Button ConnectButton;
    private Label ConnectLabel;
    private Button InstallMC0Button;
    private Button InstallMC1Button;
    private Button InstallMassButton;
    private CheckBox VMCCheckbox;
    private Button SyncButton;
    private Label FreeSpaceLabel;
    private CheckBox AutoStartCheckbox;
}