namespace GhostlineChess
{
    partial class FrmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            boardPanel = new TableLayoutPanel();
            statusLabel = new Label();
            newGameButton = new Button();
            SuspendLayout();
            // 
            // boardPanel
            // 
            boardPanel.ColumnCount = 8;
            boardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            boardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            boardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30F));
            boardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30F));
            boardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30F));
            boardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30F));
            boardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30F));
            boardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30F));
            boardPanel.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
            boardPanel.Location = new Point(45, 42);
            boardPanel.Margin = new Padding(0);
            boardPanel.Name = "boardPanel";
            boardPanel.RowCount = 8;
            boardPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 23.2F));
            boardPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 76.8F));
            boardPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            boardPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            boardPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            boardPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            boardPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            boardPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            boardPanel.Size = new Size(840, 784);
            boardPanel.TabIndex = 0;
            // 
            // statusLabel
            // 
            statusLabel.AutoSize = true;
            statusLabel.Location = new Point(45, 847);
            statusLabel.Margin = new Padding(4, 0, 4, 0);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(123, 28);
            statusLabel.TabIndex = 1;
            statusLabel.Text = "The Hallowed Saints to move";
            // 
            // newGameButton
            // 
            newGameButton.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            newGameButton.Location = new Point(659, 843);
            newGameButton.Name = "newGameButton";
            newGameButton.Size = new Size(110, 40);
            newGameButton.TabIndex = 2;
            newGameButton.Text = "New Game\n";
            newGameButton.UseVisualStyleBackColor = true;
            newGameButton.Click += newGameButton_Click;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(12F, 28F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(983, 893);
            Controls.Add(newGameButton);
            Controls.Add(statusLabel);
            Controls.Add(boardPanel);
            Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "FrmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Ghostline Chess";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel boardPanel;
        private Label statusLabel;
        private Button newGameButton;
    }
}
