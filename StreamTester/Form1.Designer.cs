namespace StreamTester
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.buttonTestOnSave = new System.Windows.Forms.Button();
            this.streamSwitch = new System.Windows.Forms.CheckBox();
            this.radioButtonEasy = new System.Windows.Forms.RadioButton();
            this.radioButtonNormal = new System.Windows.Forms.RadioButton();
            this.radioButtonHard = new System.Windows.Forms.RadioButton();
            this.radioButtonExpert = new System.Windows.Forms.RadioButton();
            this.buttonTestOnce = new System.Windows.Forms.Button();
            this.checkBoxAutoplay = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBoxStartTime = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.beatmapName = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxQuick = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelDrop = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.console = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.panelDrop.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonTestOnSave
            // 
            this.buttonTestOnSave.Enabled = false;
            this.buttonTestOnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTestOnSave.Location = new System.Drawing.Point(155, 231);
            this.buttonTestOnSave.Name = "buttonTestOnSave";
            this.buttonTestOnSave.Size = new System.Drawing.Size(227, 52);
            this.buttonTestOnSave.TabIndex = 0;
            this.buttonTestOnSave.Text = "Automatically test on beatmap save";
            this.buttonTestOnSave.UseVisualStyleBackColor = true;
            this.buttonTestOnSave.Click += new System.EventHandler(this.button1_Click);
            // 
            // streamSwitch
            // 
            this.streamSwitch.AutoSize = true;
            this.streamSwitch.Checked = true;
            this.streamSwitch.CheckState = System.Windows.Forms.CheckState.Checked;
            this.streamSwitch.Location = new System.Drawing.Point(17, 24);
            this.streamSwitch.Name = "streamSwitch";
            this.streamSwitch.Size = new System.Drawing.Size(105, 17);
            this.streamSwitch.TabIndex = 1;
            this.streamSwitch.Text = "Stream Switches";
            this.toolTip1.SetToolTip(this.streamSwitch, "Allow stream switches.");
            this.streamSwitch.UseVisualStyleBackColor = true;
            // 
            // radioButtonEasy
            // 
            this.radioButtonEasy.AutoSize = true;
            this.radioButtonEasy.Location = new System.Drawing.Point(28, 27);
            this.radioButtonEasy.Name = "radioButtonEasy";
            this.radioButtonEasy.Size = new System.Drawing.Size(48, 17);
            this.radioButtonEasy.TabIndex = 2;
            this.radioButtonEasy.Text = "Easy";
            this.radioButtonEasy.UseVisualStyleBackColor = true;
            // 
            // radioButtonNormal
            // 
            this.radioButtonNormal.AutoSize = true;
            this.radioButtonNormal.Checked = true;
            this.radioButtonNormal.Location = new System.Drawing.Point(28, 50);
            this.radioButtonNormal.Name = "radioButtonNormal";
            this.radioButtonNormal.Size = new System.Drawing.Size(58, 17);
            this.radioButtonNormal.TabIndex = 3;
            this.radioButtonNormal.TabStop = true;
            this.radioButtonNormal.Text = "Normal";
            this.radioButtonNormal.UseVisualStyleBackColor = true;
            // 
            // radioButtonHard
            // 
            this.radioButtonHard.AutoSize = true;
            this.radioButtonHard.Location = new System.Drawing.Point(28, 73);
            this.radioButtonHard.Name = "radioButtonHard";
            this.radioButtonHard.Size = new System.Drawing.Size(48, 17);
            this.radioButtonHard.TabIndex = 4;
            this.radioButtonHard.Text = "Hard";
            this.radioButtonHard.UseVisualStyleBackColor = true;
            // 
            // radioButtonExpert
            // 
            this.radioButtonExpert.AutoSize = true;
            this.radioButtonExpert.Location = new System.Drawing.Point(28, 96);
            this.radioButtonExpert.Name = "radioButtonExpert";
            this.radioButtonExpert.Size = new System.Drawing.Size(55, 17);
            this.radioButtonExpert.TabIndex = 5;
            this.radioButtonExpert.Text = "Expert";
            this.radioButtonExpert.UseVisualStyleBackColor = true;
            // 
            // buttonTestOnce
            // 
            this.buttonTestOnce.Enabled = false;
            this.buttonTestOnce.Location = new System.Drawing.Point(13, 231);
            this.buttonTestOnce.Name = "buttonTestOnce";
            this.buttonTestOnce.Size = new System.Drawing.Size(119, 52);
            this.buttonTestOnce.TabIndex = 6;
            this.buttonTestOnce.Text = "Test Once";
            this.buttonTestOnce.UseVisualStyleBackColor = true;
            this.buttonTestOnce.Click += new System.EventHandler(this.buttonTestOnce_Click);
            // 
            // checkBoxAutoplay
            // 
            this.checkBoxAutoplay.AutoSize = true;
            this.checkBoxAutoplay.Checked = true;
            this.checkBoxAutoplay.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoplay.Location = new System.Drawing.Point(17, 47);
            this.checkBoxAutoplay.Name = "checkBoxAutoplay";
            this.checkBoxAutoplay.Size = new System.Drawing.Size(67, 17);
            this.checkBoxAutoplay.TabIndex = 7;
            this.checkBoxAutoplay.Text = "Autoplay";
            this.toolTip1.SetToolTip(this.checkBoxAutoplay, "Start with autoplay enabled.");
            this.checkBoxAutoplay.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButtonEasy);
            this.groupBox1.Controls.Add(this.radioButtonNormal);
            this.groupBox1.Controls.Add(this.radioButtonHard);
            this.groupBox1.Controls.Add(this.radioButtonExpert);
            this.groupBox1.Location = new System.Drawing.Point(15, 94);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(116, 123);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Initial Difficulty";
            // 
            // textBoxStartTime
            // 
            this.textBoxStartTime.Location = new System.Drawing.Point(98, 32);
            this.textBoxStartTime.Name = "textBoxStartTime";
            this.textBoxStartTime.Size = new System.Drawing.Size(100, 20);
            this.textBoxStartTime.TabIndex = 6;
            this.textBoxStartTime.Text = "0";
            this.toolTip1.SetToolTip(this.textBoxStartTime, "Start time in milliseconds");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Start Time (ms)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Beatmap";
            // 
            // beatmapName
            // 
            this.beatmapName.Location = new System.Drawing.Point(98, 6);
            this.beatmapName.Name = "beatmapName";
            this.beatmapName.ReadOnly = true;
            this.beatmapName.Size = new System.Drawing.Size(281, 20);
            this.beatmapName.TabIndex = 11;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxQuick);
            this.groupBox2.Controls.Add(this.streamSwitch);
            this.groupBox2.Controls.Add(this.checkBoxAutoplay);
            this.groupBox2.Location = new System.Drawing.Point(153, 94);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(137, 123);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Options";
            // 
            // checkBoxQuick
            // 
            this.checkBoxQuick.AutoSize = true;
            this.checkBoxQuick.Checked = true;
            this.checkBoxQuick.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxQuick.Location = new System.Drawing.Point(17, 70);
            this.checkBoxQuick.Name = "checkBoxQuick";
            this.checkBoxQuick.Size = new System.Drawing.Size(107, 17);
            this.checkBoxQuick.TabIndex = 8;
            this.checkBoxQuick.Text = "Quick Combinate";
            this.toolTip1.SetToolTip(this.checkBoxQuick, "Bypass score/hp calculations to combinate super-fast for testing.");
            this.checkBoxQuick.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.AllowMerge = false;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.statusStrip1.Location = new System.Drawing.Point(0, 437);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(401, 18);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 13;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(276, 13);
            this.toolStripStatusLabel1.Text = "for osu!stream mappers only. please do not distribute :)";
            // 
            // panelDrop
            // 
            this.panelDrop.AllowDrop = true;
            this.panelDrop.BackColor = System.Drawing.Color.Salmon;
            this.panelDrop.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelDrop.Controls.Add(this.label4);
            this.panelDrop.Controls.Add(this.label3);
            this.panelDrop.Location = new System.Drawing.Point(204, 32);
            this.panelDrop.Name = "panelDrop";
            this.panelDrop.Size = new System.Drawing.Size(175, 52);
            this.panelDrop.TabIndex = 14;
            this.panelDrop.DragDrop += new System.Windows.Forms.DragEventHandler(this.panel1_DragDrop);
            this.panelDrop.DragEnter += new System.Windows.Forms.DragEventHandler(this.panel1_DragEnter);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(3, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(166, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Drop a beatmap folder here!";
            // 
            // console
            // 
            this.console.Location = new System.Drawing.Point(9, 295);
            this.console.Multiline = true;
            this.console.Name = "console";
            this.console.Size = new System.Drawing.Size(373, 129);
            this.console.TabIndex = 15;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(13, 27);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(146, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "(osu! songs folder or dropbox)";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(401, 455);
            this.Controls.Add(this.console);
            this.Controls.Add(this.panelDrop);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.beatmapName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxStartTime);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonTestOnce);
            this.Controls.Add(this.buttonTestOnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "osu!stream tester";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panelDrop.ResumeLayout(false);
            this.panelDrop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonTestOnSave;
        private System.Windows.Forms.CheckBox streamSwitch;
        private System.Windows.Forms.RadioButton radioButtonEasy;
        private System.Windows.Forms.RadioButton radioButtonNormal;
        private System.Windows.Forms.RadioButton radioButtonHard;
        private System.Windows.Forms.RadioButton radioButtonExpert;
        private System.Windows.Forms.Button buttonTestOnce;
        private System.Windows.Forms.CheckBox checkBoxAutoplay;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBoxStartTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox beatmapName;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBoxQuick;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Panel panelDrop;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox console;
        private System.Windows.Forms.Label label4;
    }
}

