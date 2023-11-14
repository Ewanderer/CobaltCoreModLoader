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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpMain = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.clbModLibrary = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbPath = new System.Windows.Forms.TextBox();
            this.btnFindGame = new System.Windows.Forms.Button();
            this.btnWarmupMods = new System.Windows.Forms.Button();
            this.btnAddAssembly = new System.Windows.Forms.Button();
            this.btnLoadFolder = new System.Windows.Forms.Button();
            this.btnRemoveMod = new System.Windows.Forms.Button();
            this.cbCloseOnLaunch = new System.Windows.Forms.CheckBox();
            this.cbStartDevMode = new System.Windows.Forms.CheckBox();
            this.btnLaunchCobaltCore = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tpMain.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpMain);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(684, 461);
            this.tabControl1.TabIndex = 0;
            // 
            // tpMain
            // 
            this.tpMain.Controls.Add(this.tableLayoutPanel1);
            this.tpMain.Location = new System.Drawing.Point(4, 34);
            this.tpMain.Margin = new System.Windows.Forms.Padding(5);
            this.tpMain.Name = "tpMain";
            this.tpMain.Padding = new System.Windows.Forms.Padding(5);
            this.tpMain.Size = new System.Drawing.Size(676, 423);
            this.tpMain.TabIndex = 5;
            this.tpMain.Text = "Core";
            this.tpMain.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoScroll = true;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tableLayoutPanel1.Controls.Add(this.clbModLibrary, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tbPath, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnFindGame, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnWarmupMods, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.btnAddAssembly, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.btnLoadFolder, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.btnRemoveMod, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.cbCloseOnLaunch, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.cbStartDevMode, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.btnLaunchCobaltCore, 0, 7);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(5, 5);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 8;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(666, 413);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // clbModLibrary
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.clbModLibrary, 3);
            this.clbModLibrary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clbModLibrary.FormattingEnabled = true;
            this.clbModLibrary.Location = new System.Drawing.Point(3, 86);
            this.clbModLibrary.Name = "clbModLibrary";
            this.clbModLibrary.Size = new System.Drawing.Size(660, 114);
            this.clbModLibrary.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(5, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(150, 50);
            this.label1.TabIndex = 0;
            this.label1.Text = "Game Path:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.label2, 3);
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(5, 50);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(656, 33);
            this.label2.TabIndex = 1;
            this.label2.Text = "Mod List";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbPath
            // 
            this.tbPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbPath.Location = new System.Drawing.Point(165, 5);
            this.tbPath.Margin = new System.Windows.Forms.Padding(5);
            this.tbPath.Name = "tbPath";
            this.tbPath.Size = new System.Drawing.Size(336, 33);
            this.tbPath.TabIndex = 2;
            // 
            // btnFindGame
            // 
            this.btnFindGame.AutoSize = true;
            this.btnFindGame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnFindGame.Location = new System.Drawing.Point(511, 5);
            this.btnFindGame.Margin = new System.Windows.Forms.Padding(5);
            this.btnFindGame.Name = "btnFindGame";
            this.btnFindGame.Size = new System.Drawing.Size(150, 40);
            this.btnFindGame.TabIndex = 3;
            this.btnFindGame.Text = "Locate Game";
            this.btnFindGame.UseVisualStyleBackColor = true;
            // 
            // btnWarmupMods
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.btnWarmupMods, 3);
            this.btnWarmupMods.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnWarmupMods.Location = new System.Drawing.Point(3, 246);
            this.btnWarmupMods.Name = "btnWarmupMods";
            this.btnWarmupMods.Size = new System.Drawing.Size(660, 34);
            this.btnWarmupMods.TabIndex = 8;
            this.btnWarmupMods.Text = "Warmup";
            this.btnWarmupMods.UseVisualStyleBackColor = true;
            // 
            // btnAddAssembly
            // 
            this.btnAddAssembly.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddAssembly.Location = new System.Drawing.Point(3, 206);
            this.btnAddAssembly.Name = "btnAddAssembly";
            this.btnAddAssembly.Size = new System.Drawing.Size(154, 34);
            this.btnAddAssembly.TabIndex = 9;
            this.btnAddAssembly.Text = "Add Mod";
            this.btnAddAssembly.UseVisualStyleBackColor = true;
            // 
            // btnLoadFolder
            // 
            this.btnLoadFolder.AutoSize = true;
            this.btnLoadFolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLoadFolder.Location = new System.Drawing.Point(163, 206);
            this.btnLoadFolder.Name = "btnLoadFolder";
            this.btnLoadFolder.Size = new System.Drawing.Size(340, 34);
            this.btnLoadFolder.TabIndex = 10;
            this.btnLoadFolder.Text = "Load Folder";
            this.btnLoadFolder.UseVisualStyleBackColor = true;
            // 
            // btnRemoveMod
            // 
            this.btnRemoveMod.AutoSize = true;
            this.btnRemoveMod.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRemoveMod.Location = new System.Drawing.Point(509, 206);
            this.btnRemoveMod.Name = "btnRemoveMod";
            this.btnRemoveMod.Size = new System.Drawing.Size(154, 34);
            this.btnRemoveMod.TabIndex = 11;
            this.btnRemoveMod.Text = "Remove Mod";
            this.btnRemoveMod.UseVisualStyleBackColor = true;
            // 
            // cbCloseOnLaunch
            // 
            this.cbCloseOnLaunch.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.cbCloseOnLaunch, 3);
            this.cbCloseOnLaunch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbCloseOnLaunch.Location = new System.Drawing.Point(3, 286);
            this.cbCloseOnLaunch.Name = "cbCloseOnLaunch";
            this.cbCloseOnLaunch.Size = new System.Drawing.Size(660, 34);
            this.cbCloseOnLaunch.TabIndex = 12;
            this.cbCloseOnLaunch.Text = "Close on Launch";
            this.cbCloseOnLaunch.UseVisualStyleBackColor = true;
            // 
            // cbStartDevMode
            // 
            this.cbStartDevMode.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.cbStartDevMode, 3);
            this.cbStartDevMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbStartDevMode.Location = new System.Drawing.Point(3, 326);
            this.cbStartDevMode.Name = "cbStartDevMode";
            this.cbStartDevMode.Size = new System.Drawing.Size(660, 34);
            this.cbStartDevMode.TabIndex = 13;
            this.cbStartDevMode.Text = "Start in Dev Mode";
            this.cbStartDevMode.UseVisualStyleBackColor = true;
            // 
            // btnLaunchCobaltCore
            // 
            this.btnLaunchCobaltCore.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.btnLaunchCobaltCore, 3);
            this.btnLaunchCobaltCore.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLaunchCobaltCore.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnLaunchCobaltCore.Location = new System.Drawing.Point(3, 366);
            this.btnLaunchCobaltCore.Name = "btnLaunchCobaltCore";
            this.btnLaunchCobaltCore.Size = new System.Drawing.Size(660, 44);
            this.btnLaunchCobaltCore.TabIndex = 14;
            this.btnLaunchCobaltCore.Text = "Launch Cobalt Core";
            this.btnLaunchCobaltCore.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 461);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MinimumSize = new System.Drawing.Size(700, 500);
            this.Name = "MainForm";
            this.Text = "Cobalt Core Mod Loader";
            this.tabControl1.ResumeLayout(false);
            this.tpMain.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

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