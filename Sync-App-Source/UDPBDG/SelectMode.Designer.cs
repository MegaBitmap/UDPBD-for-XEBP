namespace UDPBDG;

partial class SelectMode
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
        udpbd_button = new Button();
        udpfs_button = new Button();
        udpfs_bd_button = new Button();
        udpbd_label = new Label();
        udpfs_label = new Label();
        udpfs_bd_label = new Label();
        SuspendLayout();
        // 
        // udpbd_button
        // 
        udpbd_button.Location = new Point(59, 15);
        udpbd_button.Margin = new Padding(50, 6, 50, 6);
        udpbd_button.Name = "udpbd_button";
        udpbd_button.Size = new Size(480, 60);
        udpbd_button.TabIndex = 7;
        udpbd_button.Text = "UDPBD-Server/VexFAT (udpbd)";
        udpbd_button.UseVisualStyleBackColor = true;
        udpbd_button.Click += Udpbd_button_Click;
        // 
        // udpfs_button
        // 
        udpfs_button.Location = new Point(59, 150);
        udpfs_button.Margin = new Padding(50, 6, 50, 6);
        udpfs_button.Name = "udpfs_button";
        udpfs_button.Size = new Size(480, 60);
        udpfs_button.TabIndex = 9;
        udpfs_button.Text = "UDPFS FileSystem Server (udpfs)";
        udpfs_button.UseVisualStyleBackColor = true;
        udpfs_button.Click += Udpfs_button_Click;
        // 
        // udpfs_bd_button
        // 
        udpfs_bd_button.Location = new Point(59, 300);
        udpfs_bd_button.Margin = new Padding(50, 6, 50, 6);
        udpfs_bd_button.Name = "udpfs_bd_button";
        udpfs_bd_button.Size = new Size(480, 60);
        udpfs_bd_button.TabIndex = 10;
        udpfs_bd_button.Text = "UDPFS BlockDevice Server (udpfs_bd)";
        udpfs_bd_button.UseVisualStyleBackColor = true;
        udpfs_bd_button.Click += Udpfs_bd_button_Click;
        // 
        // udpbd_label
        // 
        udpbd_label.Font = new Font("Noto Sans", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
        udpbd_label.Location = new Point(12, 81);
        udpbd_label.Name = "udpbd_label";
        udpbd_label.Size = new Size(600, 41);
        udpbd_label.TabIndex = 11;
        udpbd_label.Text = "This uses the original UDPBD protocol created by Rick Gaiser in 2022.";
        udpbd_label.TextAlign = ContentAlignment.TopCenter;
        // 
        // udpfs_label
        // 
        udpfs_label.Font = new Font("Noto Sans", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
        udpfs_label.Location = new Point(12, 216);
        udpfs_label.Name = "udpfs_label";
        udpfs_label.Size = new Size(600, 50);
        udpfs_label.TabIndex = 12;
        udpfs_label.Text = "The UDPFS mode provides filesystem access with improved reliability\r\nthanks to Go-Back-N automatic repeat requests.";
        udpfs_label.TextAlign = ContentAlignment.TopCenter;
        // 
        // udpfs_bd_label
        // 
        udpfs_bd_label.Font = new Font("Noto Sans", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
        udpfs_bd_label.Location = new Point(12, 366);
        udpfs_bd_label.Name = "udpfs_bd_label";
        udpfs_bd_label.Size = new Size(600, 50);
        udpfs_bd_label.TabIndex = 13;
        udpfs_bd_label.Text = "This mode provides block-device access with improved reliability.\r\nA fixed size exFAT VHD file is used as the block-device to read data from.";
        udpfs_bd_label.TextAlign = ContentAlignment.TopCenter;
        // 
        // SelectMode
        // 
        AutoScaleDimensions = new SizeF(11F, 29F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoScroll = true;
        ClientSize = new Size(624, 441);
        Controls.Add(udpfs_bd_label);
        Controls.Add(udpfs_label);
        Controls.Add(udpbd_label);
        Controls.Add(udpfs_bd_button);
        Controls.Add(udpfs_button);
        Controls.Add(udpbd_button);
        Font = new Font("Noto Sans", 14.2499981F);
        HelpButton = true;
        Icon = Resources.Icon;
        Margin = new Padding(5, 6, 5, 6);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "SelectMode";
        Text = "Select one of the following UDP modes:";
        HelpButtonClicked += SelectMode_HelpButtonClicked;
        ResumeLayout(false);
    }

    #endregion

    private Button udpbd_button;
    private Button udpfs_button;
    private Button udpfs_bd_button;
    private Label udpbd_label;
    private Label udpfs_label;
    private Label udpfs_bd_label;
}