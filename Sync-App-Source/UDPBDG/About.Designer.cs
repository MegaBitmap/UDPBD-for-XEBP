namespace UDPBDG;

partial class About
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
        RichTextBox = new RichTextBox();
        SuspendLayout();
        // 
        // RichTextBox
        // 
        RichTextBox.BorderStyle = BorderStyle.None;
        RichTextBox.Dock = DockStyle.Fill;
        RichTextBox.Location = new Point(0, 0);
        RichTextBox.Name = "RichTextBox";
        RichTextBox.ReadOnly = true;
        RichTextBox.Size = new Size(800, 450);
        RichTextBox.TabIndex = 0;
        RichTextBox.Text = resources.GetString("RichTextBox.Text");
        RichTextBox.LinkClicked += RichTextBox_LinkClicked;
        // 
        // About
        // 
        AutoScaleDimensions = new SizeF(11F, 25F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(RichTextBox);
        Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        Icon = Resources.Icon;
        Margin = new Padding(5);
        Name = "About";
        Text = "About";
        ResumeLayout(false);
    }

    #endregion

    private RichTextBox RichTextBox;
}