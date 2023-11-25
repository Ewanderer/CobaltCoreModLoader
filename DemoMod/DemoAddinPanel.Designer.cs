namespace DemoMod
{
    partial class DemoAddinPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblDesc = new Label();
            tbValue = new TextBox();
            btnGenerateRandom = new Button();
            SuspendLayout();
            // 
            // lblDesc
            // 
            lblDesc.BackColor = System.Drawing.Color.Blue;
            lblDesc.Dock = DockStyle.Top;
            lblDesc.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            lblDesc.ForeColor = System.Drawing.Color.White;
            lblDesc.Location = new Point(0, 0);
            lblDesc.Name = "lblDesc";
            lblDesc.Size = new Size(526, 34);
            lblDesc.TabIndex = 0;
            lblDesc.Text = "Check EWandererDemoCard.Name";
            lblDesc.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tbValue
            // 
            tbValue.Dock = DockStyle.Top;
            tbValue.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            tbValue.Location = new Point(0, 34);
            tbValue.Name = "tbValue";
            tbValue.Size = new Size(526, 35);
            tbValue.TabIndex = 1;
            // 
            // btnGenerateRandom
            // 
            btnGenerateRandom.BackColor = System.Drawing.Color.Yellow;
            btnGenerateRandom.Dock = DockStyle.Fill;
            btnGenerateRandom.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            btnGenerateRandom.Location = new Point(0, 69);
            btnGenerateRandom.Name = "btnGenerateRandom";
            btnGenerateRandom.Size = new Size(526, 371);
            btnGenerateRandom.TabIndex = 2;
            btnGenerateRandom.Text = "Generate Random Text";
            btnGenerateRandom.UseVisualStyleBackColor = false;
            btnGenerateRandom.Click += btnGenerateRandom_Click;
            // 
            // DemoAddinPanel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Green;
            Controls.Add(btnGenerateRandom);
            Controls.Add(tbValue);
            Controls.Add(lblDesc);
            Name = "DemoAddinPanel";
            Size = new Size(526, 440);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblDesc;
        public TextBox tbValue;
        private Button btnGenerateRandom;
    }
}
