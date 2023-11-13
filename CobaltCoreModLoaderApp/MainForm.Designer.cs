namespace CobaltCoreModLoaderApp
{
    partial class MainForm
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
            tabControl1 = new System.Windows.Forms.TabControl();
            tpMain = new System.Windows.Forms.TabPage();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            clbModLibrary = new System.Windows.Forms.CheckedListBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            tbPath = new System.Windows.Forms.TextBox();
            btnFindGame = new System.Windows.Forms.Button();
            btnWarmupMods = new System.Windows.Forms.Button();
            btnAddAssembly = new System.Windows.Forms.Button();
            btnLoadFolder = new System.Windows.Forms.Button();
            btnRemoveMod = new System.Windows.Forms.Button();
            cbCloseOnLaunch = new System.Windows.Forms.CheckBox();
            cbStartDevMode = new System.Windows.Forms.CheckBox();
            btnLaunchCobaltCore = new System.Windows.Forms.Button();
            tabControl1.SuspendLayout();
            tpMain.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tpMain);
            tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl1.Location = new System.Drawing.Point(0, 0);
            tabControl1.Margin = new System.Windows.Forms.Padding(5);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(684, 461);
            tabControl1.TabIndex = 0;
            // 
            // tpMain
            // 
            tpMain.Controls.Add(tableLayoutPanel1);
            tpMain.Location = new System.Drawing.Point(4, 34);
            tpMain.Margin = new System.Windows.Forms.Padding(5);
            tpMain.Name = "tpMain";
            tpMain.Padding = new System.Windows.Forms.Padding(5);
            tpMain.Size = new System.Drawing.Size(676, 423);
            tpMain.TabIndex = 5;
            tpMain.Text = "Core";
            tpMain.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoScroll = true;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            tableLayoutPanel1.Controls.Add(clbModLibrary, 0, 2);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label2, 0, 1);
            tableLayoutPanel1.Controls.Add(tbPath, 1, 0);
            tableLayoutPanel1.Controls.Add(btnFindGame, 2, 0);
            tableLayoutPanel1.Controls.Add(btnWarmupMods, 0, 4);
            tableLayoutPanel1.Controls.Add(btnAddAssembly, 0, 3);
            tableLayoutPanel1.Controls.Add(btnLoadFolder, 1, 3);
            tableLayoutPanel1.Controls.Add(btnRemoveMod, 2, 3);
            tableLayoutPanel1.Controls.Add(cbCloseOnLaunch, 0, 5);
            tableLayoutPanel1.Controls.Add(cbStartDevMode, 0, 6);
            tableLayoutPanel1.Controls.Add(btnLaunchCobaltCore, 0, 7);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            tableLayoutPanel1.Location = new System.Drawing.Point(5, 5);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 8;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            tableLayoutPanel1.Size = new System.Drawing.Size(666, 413);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // clbModLibrary
            // 
            tableLayoutPanel1.SetColumnSpan(clbModLibrary, 3);
            clbModLibrary.Dock = System.Windows.Forms.DockStyle.Fill;
            clbModLibrary.FormattingEnabled = true;
            clbModLibrary.Location = new System.Drawing.Point(3, 86);
            clbModLibrary.Name = "clbModLibrary";
            clbModLibrary.Size = new System.Drawing.Size(660, 114);
            clbModLibrary.TabIndex = 6;
            clbModLibrary.ItemCheck += clbModLibrary_ItemCheck;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = System.Windows.Forms.DockStyle.Fill;
            label1.Location = new System.Drawing.Point(5, 0);
            label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(150, 50);
            label1.TabIndex = 0;
            label1.Text = "Game Path:";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(label2, 3);
            label2.Dock = System.Windows.Forms.DockStyle.Fill;
            label2.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label2.Location = new System.Drawing.Point(5, 50);
            label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(656, 33);
            label2.TabIndex = 1;
            label2.Text = "Mod List";
            label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbPath
            // 
            tbPath.Dock = System.Windows.Forms.DockStyle.Fill;
            tbPath.Location = new System.Drawing.Point(165, 5);
            tbPath.Margin = new System.Windows.Forms.Padding(5);
            tbPath.Name = "tbPath";
            tbPath.Size = new System.Drawing.Size(336, 33);
            tbPath.TabIndex = 2;
            // 
            // btnFindGame
            // 
            btnFindGame.AutoSize = true;
            btnFindGame.Dock = System.Windows.Forms.DockStyle.Fill;
            btnFindGame.Location = new System.Drawing.Point(511, 5);
            btnFindGame.Margin = new System.Windows.Forms.Padding(5);
            btnFindGame.Name = "btnFindGame";
            btnFindGame.Size = new System.Drawing.Size(150, 40);
            btnFindGame.TabIndex = 3;
            btnFindGame.Text = "Locate Game";
            btnFindGame.UseVisualStyleBackColor = true;
            // 
            // btnWarmupMods
            // 
            tableLayoutPanel1.SetColumnSpan(btnWarmupMods, 3);
            btnWarmupMods.Dock = System.Windows.Forms.DockStyle.Fill;
            btnWarmupMods.Location = new System.Drawing.Point(3, 246);
            btnWarmupMods.Name = "btnWarmupMods";
            btnWarmupMods.Size = new System.Drawing.Size(660, 34);
            btnWarmupMods.TabIndex = 8;
            btnWarmupMods.Text = "Warmup";
            btnWarmupMods.UseVisualStyleBackColor = true;
            btnWarmupMods.Click += btnWarmupMods_Click;
            // 
            // btnAddAssembly
            // 
            btnAddAssembly.Dock = System.Windows.Forms.DockStyle.Fill;
            btnAddAssembly.Location = new System.Drawing.Point(3, 206);
            btnAddAssembly.Name = "btnAddAssembly";
            btnAddAssembly.Size = new System.Drawing.Size(154, 34);
            btnAddAssembly.TabIndex = 9;
            btnAddAssembly.Text = "Add Mod";
            btnAddAssembly.UseVisualStyleBackColor = true;
            btnAddAssembly.Click += btnAddAssembly_Click;
            // 
            // btnLoadFolder
            // 
            btnLoadFolder.AutoSize = true;
            btnLoadFolder.Dock = System.Windows.Forms.DockStyle.Fill;
            btnLoadFolder.Location = new System.Drawing.Point(163, 206);
            btnLoadFolder.Name = "btnLoadFolder";
            btnLoadFolder.Size = new System.Drawing.Size(340, 34);
            btnLoadFolder.TabIndex = 10;
            btnLoadFolder.Text = "Load Folder";
            btnLoadFolder.UseVisualStyleBackColor = true;
            btnLoadFolder.Click += btnLoadFolder_Click;
            // 
            // btnRemoveMod
            // 
            btnRemoveMod.AutoSize = true;
            btnRemoveMod.Dock = System.Windows.Forms.DockStyle.Fill;
            btnRemoveMod.Location = new System.Drawing.Point(509, 206);
            btnRemoveMod.Name = "btnRemoveMod";
            btnRemoveMod.Size = new System.Drawing.Size(154, 34);
            btnRemoveMod.TabIndex = 11;
            btnRemoveMod.Text = "Remove Mod";
            btnRemoveMod.UseVisualStyleBackColor = true;
            btnRemoveMod.Click += btnRemoveMod_Click;
            // 
            // cbCloseOnLaunch
            // 
            cbCloseOnLaunch.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(cbCloseOnLaunch, 3);
            cbCloseOnLaunch.Dock = System.Windows.Forms.DockStyle.Fill;
            cbCloseOnLaunch.Location = new System.Drawing.Point(3, 286);
            cbCloseOnLaunch.Name = "cbCloseOnLaunch";
            cbCloseOnLaunch.Size = new System.Drawing.Size(660, 34);
            cbCloseOnLaunch.TabIndex = 12;
            cbCloseOnLaunch.Text = "Close on Launch";
            cbCloseOnLaunch.UseVisualStyleBackColor = true;
            // 
            // cbStartDevMode
            // 
            cbStartDevMode.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(cbStartDevMode, 3);
            cbStartDevMode.Dock = System.Windows.Forms.DockStyle.Fill;
            cbStartDevMode.Location = new System.Drawing.Point(3, 326);
            cbStartDevMode.Name = "cbStartDevMode";
            cbStartDevMode.Size = new System.Drawing.Size(660, 34);
            cbStartDevMode.TabIndex = 13;
            cbStartDevMode.Text = "Start in Dev Mode";
            cbStartDevMode.UseVisualStyleBackColor = true;
            // 
            // btnLaunchCobaltCore
            // 
            btnLaunchCobaltCore.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(btnLaunchCobaltCore, 3);
            btnLaunchCobaltCore.Dock = System.Windows.Forms.DockStyle.Fill;
            btnLaunchCobaltCore.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnLaunchCobaltCore.Location = new System.Drawing.Point(3, 366);
            btnLaunchCobaltCore.Name = "btnLaunchCobaltCore";
            btnLaunchCobaltCore.Size = new System.Drawing.Size(660, 44);
            btnLaunchCobaltCore.TabIndex = 14;
            btnLaunchCobaltCore.Text = "Launch Cobalt Core";
            btnLaunchCobaltCore.UseVisualStyleBackColor = true;
            btnLaunchCobaltCore.Click += btnLaunchCobaltCore_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(11F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(684, 461);
            Controls.Add(tabControl1);
            Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Margin = new System.Windows.Forms.Padding(5);
            MinimumSize = new System.Drawing.Size(700, 500);
            Name = "MainForm";
            Text = "Cobalt Core Mod Loader";
            tabControl1.ResumeLayout(false);
            tpMain.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbPath;
        private System.Windows.Forms.Button btnFindGame;
        private System.Windows.Forms.CheckedListBox clbModLibrary;
        private System.Windows.Forms.Button btnWarmupMods;
        private System.Windows.Forms.Button btnAddAssembly;
        private System.Windows.Forms.Button btnLoadFolder;
        private System.Windows.Forms.Button btnRemoveMod;
        private System.Windows.Forms.CheckBox cbCloseOnLaunch;
        private System.Windows.Forms.CheckBox cbStartDevMode;
        private System.Windows.Forms.Button btnLaunchCobaltCore;
    }
}