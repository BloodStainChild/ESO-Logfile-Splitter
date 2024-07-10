namespace Custom_Message
{
    partial class CustomMessage
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
            this.label_text = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label_text
            // 
            this.label_text.BackColor = System.Drawing.SystemColors.Info;
            this.label_text.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_text.Font = new System.Drawing.Font("Microsoft Sans Serif", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_text.Location = new System.Drawing.Point(0, 0);
            this.label_text.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_text.Name = "label_text";
            this.label_text.Size = new System.Drawing.Size(900, 308);
            this.label_text.TabIndex = 1;
            this.label_text.Text = "TEST";
            this.label_text.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CustomMessage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Info;
            this.ClientSize = new System.Drawing.Size(900, 308);
            this.Controls.Add(this.label_text);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "CustomMessage";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CustomMessage";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label_text;
    }
}