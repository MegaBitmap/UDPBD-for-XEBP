namespace UDPBDG;

partial class Settings
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
        SyncSNL = new Button();
        SyncXEBP = new Button();
        StartServerButton = new Button();
        ShowServerCheckbox = new CheckBox();
        MountButton = new Button();
        SetPathButton = new Button();
        GameCountLabel = new Label();
        GamePathLabel = new Label();
        SuspendLayout();
        // 
        // SyncSNL
        // 
        SyncSNL.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        SyncSNL.Location = new Point(12, 198);
        SyncSNL.Name = "SyncSNL";
        SyncSNL.Size = new Size(600, 60);
        SyncSNL.TabIndex = 2;
        SyncSNL.Text = "Sync to Simple Neutrino Loader";
        SyncSNL.UseVisualStyleBackColor = true;
        SyncSNL.Click += SyncSNL_Click;
        // 
        // SyncXEBP
        // 
        SyncXEBP.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        SyncXEBP.Location = new Point(12, 264);
        SyncXEBP.Name = "SyncXEBP";
        SyncXEBP.Size = new Size(600, 60);
        SyncXEBP.TabIndex = 3;
        SyncXEBP.Text = "Sync to XEB+ Neutrino Launcher";
        SyncXEBP.UseVisualStyleBackColor = true;
        SyncXEBP.Click += SyncXEBP_Click;
        // 
        // StartServerButton
        // 
        StartServerButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        StartServerButton.Location = new Point(12, 369);
        StartServerButton.Name = "StartServerButton";
        StartServerButton.Size = new Size(600, 60);
        StartServerButton.TabIndex = 5;
        StartServerButton.Text = "Save Settings and Start Server";
        StartServerButton.UseVisualStyleBackColor = true;
        StartServerButton.Click += StartServerButton_Click;
        // 
        // ShowServerCheckbox
        // 
        ShowServerCheckbox.Anchor = AnchorStyles.Bottom;
        ShowServerCheckbox.AutoSize = true;
        ShowServerCheckbox.Location = new Point(150, 330);
        ShowServerCheckbox.Name = "ShowServerCheckbox";
        ShowServerCheckbox.Size = new Size(323, 33);
        ShowServerCheckbox.TabIndex = 4;
        ShowServerCheckbox.Text = "Show Console on Server Startup";
        ShowServerCheckbox.UseVisualStyleBackColor = true;
        // 
        // MountButton
        // 
        MountButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        MountButton.Location = new Point(12, 12);
        MountButton.Name = "MountButton";
        MountButton.Size = new Size(600, 60);
        MountButton.TabIndex = 6;
        MountButton.Text = "Mount Virtual exFAT Drive";
        MountButton.UseVisualStyleBackColor = true;
        MountButton.Click += MountButton_Click;
        // 
        // SetPathButton
        // 
        SetPathButton.Location = new Point(12, 78);
        SetPathButton.Name = "SetPathButton";
        SetPathButton.Size = new Size(300, 60);
        SetPathButton.TabIndex = 1;
        SetPathButton.Text = "Choose Game Path";
        SetPathButton.UseVisualStyleBackColor = true;
        SetPathButton.Click += SetPathButton_Click;
        // 
        // GameCountLabel
        // 
        GameCountLabel.AutoSize = true;
        GameCountLabel.Location = new Point(318, 94);
        GameCountLabel.Name = "GameCountLabel";
        GameCountLabel.Size = new Size(155, 29);
        GameCountLabel.TabIndex = 6;
        GameCountLabel.Text = "0 Games Found";
        // 
        // GamePathLabel
        // 
        GamePathLabel.AutoSize = true;
        GamePathLabel.Location = new Point(12, 141);
        GamePathLabel.Name = "GamePathLabel";
        GamePathLabel.Size = new Size(132, 29);
        GamePathLabel.TabIndex = 7;
        GamePathLabel.Text = "Path is Unset";
        // 
        // Settings
        // 
        AutoScaleDimensions = new SizeF(11F, 29F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoScroll = true;
        ClientSize = new Size(624, 441);
        Controls.Add(GamePathLabel);
        Controls.Add(GameCountLabel);
        Controls.Add(SetPathButton);
        Controls.Add(MountButton);
        Controls.Add(ShowServerCheckbox);
        Controls.Add(StartServerButton);
        Controls.Add(SyncXEBP);
        Controls.Add(SyncSNL);
        Font = new Font("Noto Sans", 14.2499981F, FontStyle.Regular, GraphicsUnit.Point, 0);
        HelpButton = true;
        Icon = Resources.Icon;
        Margin = new Padding(5);
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(0, 480);
        Name = "Settings";
        Text = "UDPBDG Settings";
        HelpButtonClicked += Settings_HelpButtonClicked;
        FormClosing += Settings_FormClosing;
        Shown += Settings_Shown;
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Button SyncSNL;
    private Button SyncXEBP;
    private Button StartServerButton;
    private CheckBox ShowServerCheckbox;
    private Button MountButton;
    private Button SetPathButton;
    private Label GameCountLabel;
    private Label GamePathLabel;
}