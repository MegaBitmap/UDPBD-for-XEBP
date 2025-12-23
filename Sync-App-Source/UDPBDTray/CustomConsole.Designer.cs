namespace UDPBDTray
{
    partial class CustomConsole
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomConsole));
            MainPanel = new Panel();
            MainLabel = new Label();
            CopyButton = new Button();
            MainPanel.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.AutoScroll = true;
            MainPanel.Controls.Add(MainLabel);
            MainPanel.Dock = DockStyle.Fill;
            MainPanel.Font = new Font("Consolas", 10.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MainPanel.Location = new Point(0, 0);
            MainPanel.Name = "MainPanel";
            MainPanel.Size = new Size(985, 510);
            MainPanel.TabIndex = 1;
            // 
            // MainLabel
            // 
            MainLabel.AutoSize = true;
            MainLabel.Location = new Point(0, 0);
            MainLabel.Name = "MainLabel";
            MainLabel.Size = new Size(272, 17);
            MainLabel.TabIndex = 0;
            MainLabel.Text = "ERROR: FAILED TO ALLOCATE CONSOLE";
            // 
            // CopyButton
            // 
            CopyButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CopyButton.Location = new Point(835, 12);
            CopyButton.Name = "CopyButton";
            CopyButton.Size = new Size(120, 25);
            CopyButton.TabIndex = 2;
            CopyButton.Text = "Copy Full History";
            CopyButton.UseVisualStyleBackColor = true;
            CopyButton.Click += CopyButton_Click;
            // 
            // CustomConsole
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(12, 12, 12);
            ClientSize = new Size(985, 510);
            Controls.Add(CopyButton);
            Controls.Add(MainPanel);
            ForeColor = Color.FromArgb(204, 204, 204);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(2, 3, 2, 3);
            Name = "CustomConsole";
            Text = "Form1";
            FormClosed += CustomConsole_FormClosed;
            Load += CustomConsole_Load;
            MainPanel.ResumeLayout(false);
            MainPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel MainPanel;
        private Label MainLabel;
        private Button CopyButton;
    }
}