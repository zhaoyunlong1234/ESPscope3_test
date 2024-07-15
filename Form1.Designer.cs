namespace ESPscope2
{

    partial class ESPScope
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ESPScope));
            button1 = new Button();
            panelControl = new Panel();
            checkBoxVolt = new CheckBox();
            checkBoxCurrent = new CheckBox();
            buttonClear = new Button();
            buttonAddSpan = new Button();
            buttonAddLine = new Button();
            buttonFollow = new Button();
            comboBoxPorts = new ComboBox();
            labelCom = new Label();
            buttonOpen = new Button();
            panelGuide = new Panel();
            formsPlotGuide = new ScottPlot.WinForms.FormsPlot();
            timerFlashPlot = new System.Windows.Forms.Timer(components);
            panelMain = new Panel();
            formsPlotMain = new ScottPlot.WinForms.FormsPlot();
            panelControl.SuspendLayout();
            panelGuide.SuspendLayout();
            panelMain.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(715, 327);
            button1.Margin = new Padding(2);
            button1.Name = "button1";
            button1.Size = new Size(7, 7);
            button1.TabIndex = 0;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            // 
            // panelControl
            // 
            panelControl.BackColor = Color.White;
            panelControl.Controls.Add(checkBoxVolt);
            panelControl.Controls.Add(checkBoxCurrent);
            panelControl.Controls.Add(buttonClear);
            panelControl.Controls.Add(buttonAddSpan);
            panelControl.Controls.Add(buttonAddLine);
            panelControl.Controls.Add(buttonFollow);
            panelControl.Controls.Add(comboBoxPorts);
            panelControl.Controls.Add(labelCom);
            panelControl.Controls.Add(buttonOpen);
            panelControl.Dock = DockStyle.Right;
            panelControl.Location = new Point(1090, 0);
            panelControl.Margin = new Padding(2);
            panelControl.Name = "panelControl";
            panelControl.Size = new Size(138, 855);
            panelControl.TabIndex = 2;
            // 
            // checkBoxVolt
            // 
            checkBoxVolt.AutoSize = true;
            checkBoxVolt.Checked = true;
            checkBoxVolt.CheckState = CheckState.Checked;
            checkBoxVolt.ForeColor = SystemColors.Control;
            checkBoxVolt.Location = new Point(28, 349);
            checkBoxVolt.Name = "checkBoxVolt";
            checkBoxVolt.Size = new Size(88, 24);
            checkBoxVolt.TabIndex = 9;
            checkBoxVolt.Text = "Voltage";
            checkBoxVolt.UseVisualStyleBackColor = true;
            checkBoxVolt.CheckedChanged += checkBoxVolt_CheckedChanged;
            // 
            // checkBoxCurrent
            // 
            checkBoxCurrent.AutoSize = true;
            checkBoxCurrent.Checked = true;
            checkBoxCurrent.CheckState = CheckState.Checked;
            checkBoxCurrent.ForeColor = SystemColors.Control;
            checkBoxCurrent.Location = new Point(28, 319);
            checkBoxCurrent.Name = "checkBoxCurrent";
            checkBoxCurrent.Size = new Size(86, 24);
            checkBoxCurrent.TabIndex = 8;
            checkBoxCurrent.Text = "Current";
            checkBoxCurrent.UseVisualStyleBackColor = true;
            checkBoxCurrent.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // buttonClear
            // 
            buttonClear.Location = new Point(11, 275);
            buttonClear.Margin = new Padding(2);
            buttonClear.Name = "buttonClear";
            buttonClear.Size = new Size(118, 28);
            buttonClear.TabIndex = 7;
            buttonClear.Text = "Clear";
            buttonClear.UseVisualStyleBackColor = true;
            buttonClear.Click += buttonClear_Click;
            // 
            // buttonAddSpan
            // 
            buttonAddSpan.Location = new Point(11, 231);
            buttonAddSpan.Margin = new Padding(2);
            buttonAddSpan.Name = "buttonAddSpan";
            buttonAddSpan.Size = new Size(118, 28);
            buttonAddSpan.TabIndex = 6;
            buttonAddSpan.Text = "Add Span";
            buttonAddSpan.UseVisualStyleBackColor = true;
            buttonAddSpan.Click += button3_Click;
            // 
            // buttonAddLine
            // 
            buttonAddLine.Location = new Point(11, 188);
            buttonAddLine.Margin = new Padding(2);
            buttonAddLine.Name = "buttonAddLine";
            buttonAddLine.Size = new Size(118, 28);
            buttonAddLine.TabIndex = 5;
            buttonAddLine.Text = "Add Line";
            buttonAddLine.UseVisualStyleBackColor = true;
            buttonAddLine.Click += buttonAddLine_Click;
            // 
            // buttonFollow
            // 
            buttonFollow.Location = new Point(11, 142);
            buttonFollow.Margin = new Padding(2);
            buttonFollow.Name = "buttonFollow";
            buttonFollow.Size = new Size(118, 28);
            buttonFollow.TabIndex = 4;
            buttonFollow.Text = "Follow";
            buttonFollow.UseVisualStyleBackColor = true;
            buttonFollow.Click += buttonFollow_Click;
            // 
            // comboBoxPorts
            // 
            comboBoxPorts.FormattingEnabled = true;
            comboBoxPorts.Location = new Point(11, 52);
            comboBoxPorts.Margin = new Padding(2);
            comboBoxPorts.Name = "comboBoxPorts";
            comboBoxPorts.Size = new Size(119, 28);
            comboBoxPorts.TabIndex = 2;
            comboBoxPorts.SelectedIndexChanged += comboBoxPorts_SelectedIndexChanged;
            // 
            // labelCom
            // 
            labelCom.AutoSize = true;
            labelCom.ForeColor = SystemColors.ControlLightLight;
            labelCom.Location = new Point(11, 21);
            labelCom.Margin = new Padding(2, 0, 2, 0);
            labelCom.Name = "labelCom";
            labelCom.Size = new Size(47, 20);
            labelCom.TabIndex = 1;
            labelCom.Text = "Com:";
            // 
            // buttonOpen
            // 
            buttonOpen.Location = new Point(11, 92);
            buttonOpen.Margin = new Padding(2);
            buttonOpen.Name = "buttonOpen";
            buttonOpen.Size = new Size(118, 28);
            buttonOpen.TabIndex = 0;
            buttonOpen.Text = "Open";
            buttonOpen.UseVisualStyleBackColor = true;
            buttonOpen.Click += buttonOpen_Click;
            // 
            // panelGuide
            // 
            panelGuide.Controls.Add(formsPlotGuide);
            panelGuide.Dock = DockStyle.Bottom;
            panelGuide.Location = new Point(0, 751);
            panelGuide.Margin = new Padding(2);
            panelGuide.Name = "panelGuide";
            panelGuide.Size = new Size(1090, 104);
            panelGuide.TabIndex = 3;
            // 
            // formsPlotGuide
            // 
            formsPlotGuide.DisplayScale = 1.5F;
            formsPlotGuide.Dock = DockStyle.Bottom;
            formsPlotGuide.Location = new Point(0, -6);
            formsPlotGuide.Margin = new Padding(2);
            formsPlotGuide.Name = "formsPlotGuide";
            formsPlotGuide.Size = new Size(1090, 110);
            formsPlotGuide.TabIndex = 2;
            // 
            // timerFlashPlot
            // 
            timerFlashPlot.Interval = 20;
            timerFlashPlot.Tick += timerFlashPlot_Tick;
            // 
            // panelMain
            // 
            panelMain.Controls.Add(formsPlotMain);
            panelMain.Dock = DockStyle.Fill;
            panelMain.Location = new Point(0, 0);
            panelMain.Margin = new Padding(2);
            panelMain.Name = "panelMain";
            panelMain.Size = new Size(1090, 751);
            panelMain.TabIndex = 4;
            // 
            // formsPlotMain
            // 
            formsPlotMain.DisplayScale = 1.5F;
            formsPlotMain.Dock = DockStyle.Fill;
            formsPlotMain.Location = new Point(0, 0);
            formsPlotMain.Margin = new Padding(2);
            formsPlotMain.Name = "formsPlotMain";
            formsPlotMain.Size = new Size(1090, 751);
            formsPlotMain.TabIndex = 3;
            formsPlotMain.Load += formsPlotMain_Load;
            // 
            // ESPScope
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1228, 855);
            Controls.Add(panelMain);
            Controls.Add(panelGuide);
            Controls.Add(panelControl);
            Controls.Add(button1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(2);
            Name = "ESPScope";
            Text = "ESPScope";
            Load += Form1_Load;
            panelControl.ResumeLayout(false);
            panelControl.PerformLayout();
            panelGuide.ResumeLayout(false);
            panelMain.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
        private Panel panelControl;
        private ComboBox comboBoxPorts;
        private Label labelCom;
        private Button buttonOpen;
        private Panel panelGuide;
        private ScottPlot.WinForms.FormsPlot formsPlotGuide;
        private System.Windows.Forms.Timer timerFlashPlot;
        private Panel panelMain;
        private Button buttonFollow;
        private Button buttonAddLine;
        private Button buttonAddSpan;
        private ScottPlot.WinForms.FormsPlot formsPlotMain;
        private Button buttonClear;
        private CheckBox checkBoxCurrent;
        private CheckBox checkBoxVolt;
    }
}