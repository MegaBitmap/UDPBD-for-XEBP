namespace UDPBDG;

partial class SyncXEBP
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
        AutoStartCheckbox = new CheckBox();
        SyncButton = new Button();
        VMCCheckbox = new CheckBox();
        ConnectLabel = new Label();
        ConnectButton = new Button();
        PS2IPTextBox = new TextBox();
        PS2IPLabel = new Label();
        LogPanel = new Panel();
        ArtCheckbox = new CheckBox();
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
        // AutoStartCheckbox
        // 
        AutoStartCheckbox.AutoSize = true;
        AutoStartCheckbox.Location = new Point(150, 69);
        AutoStartCheckbox.Name = "AutoStartCheckbox";
        AutoStartCheckbox.Size = new Size(223, 24);
        AutoStartCheckbox.TabIndex = 2;
        AutoStartCheckbox.Text = "Set PS2BBL to autostart XEB+";
        AutoStartCheckbox.UseVisualStyleBackColor = true;
        // 
        // SyncButton
        // 
        SyncButton.Enabled = false;
        SyncButton.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        SyncButton.Location = new Point(12, 159);
        SyncButton.Name = "SyncButton";
        SyncButton.Size = new Size(600, 33);
        SyncButton.TabIndex = 5;
        SyncButton.Text = "Sync Game List and IP address";
        SyncButton.UseVisualStyleBackColor = true;
        SyncButton.Click += SyncButton_Click;
        // 
        // VMCCheckbox
        // 
        VMCCheckbox.AutoSize = true;
        VMCCheckbox.Location = new Point(150, 129);
        VMCCheckbox.Name = "VMCCheckbox";
        VMCCheckbox.Size = new Size(283, 24);
        VMCCheckbox.TabIndex = 4;
        VMCCheckbox.Text = "Use Virtual Memory Cards (exFAT only)";
        VMCCheckbox.UseVisualStyleBackColor = true;
        // 
        // ConnectLabel
        // 
        ConnectLabel.AutoSize = true;
        ConnectLabel.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        ConnectLabel.Location = new Point(486, 41);
        ConnectLabel.Name = "ConnectLabel";
        ConnectLabel.Size = new Size(126, 25);
        ConnectLabel.TabIndex = 16;
        ConnectLabel.Text = "Disconnected";
        // 
        // ConnectButton
        // 
        ConnectButton.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        ConnectButton.Location = new Point(395, 5);
        ConnectButton.Name = "ConnectButton";
        ConnectButton.Size = new Size(217, 33);
        ConnectButton.TabIndex = 1;
        ConnectButton.Text = "Connect via PS2Net";
        ConnectButton.UseVisualStyleBackColor = true;
        ConnectButton.Click += ConnectButton_Click;
        // 
        // PS2IPTextBox
        // 
        PS2IPTextBox.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        PS2IPTextBox.Location = new Point(249, 6);
        PS2IPTextBox.MaxLength = 15;
        PS2IPTextBox.Name = "PS2IPTextBox";
        PS2IPTextBox.PlaceholderText = "Type Here . . .";
        PS2IPTextBox.Size = new Size(140, 33);
        PS2IPTextBox.TabIndex = 6;
        PS2IPTextBox.Text = "192.168.0.10";
        // 
        // PS2IPLabel
        // 
        PS2IPLabel.AutoSize = true;
        PS2IPLabel.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        PS2IPLabel.Location = new Point(12, 9);
        PS2IPLabel.Name = "PS2IPLabel";
        PS2IPLabel.Size = new Size(231, 25);
        PS2IPLabel.TabIndex = 15;
        PS2IPLabel.Text = "Enter the PS2's IP address:";
        // 
        // LogPanel
        // 
        LogPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        LogPanel.AutoScroll = true;
        LogPanel.Controls.Add(LogLabel);
        LogPanel.Location = new Point(0, 198);
        LogPanel.Name = "LogPanel";
        LogPanel.Size = new Size(624, 243);
        LogPanel.TabIndex = 13;
        // 
        // ArtCheckbox
        // 
        ArtCheckbox.AutoSize = true;
        ArtCheckbox.Location = new Point(150, 99);
        ArtCheckbox.Name = "ArtCheckbox";
        ArtCheckbox.Size = new Size(196, 24);
        ArtCheckbox.TabIndex = 3;
        ArtCheckbox.Text = "Download Game Artwork";
        ArtCheckbox.UseVisualStyleBackColor = true;
        // 
        // SyncXEBP
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(624, 441);
        Controls.Add(ArtCheckbox);
        Controls.Add(AutoStartCheckbox);
        Controls.Add(SyncButton);
        Controls.Add(VMCCheckbox);
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
        Name = "SyncXEBP";
        Text = "Sync to XEB+ Neutrino Launcher";
        HelpButtonClicked += SyncXEBP_HelpButtonClicked;
        LogPanel.ResumeLayout(false);
        LogPanel.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label LogLabel;
    private CheckBox AutoStartCheckbox;
    private Button SyncButton;
    private CheckBox VMCCheckbox;
    private Label ConnectLabel;
    private Button ConnectButton;
    private TextBox PS2IPTextBox;
    private Label PS2IPLabel;
    private Panel LogPanel;
    private CheckBox ArtCheckbox;
}