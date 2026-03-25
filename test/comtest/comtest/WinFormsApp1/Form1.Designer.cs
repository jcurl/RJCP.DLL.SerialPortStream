namespace WinFormsApp1
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    partial class Form1
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
            if (disposing && (components != null)) {
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
            cmdListPorts = new Button();
            lstOutput = new ListBox();
            cmdOpen = new Button();
            cmdClose = new Button();
            SuspendLayout();
            // 
            // cmdListPorts
            // 
            cmdListPorts.Location = new Point(609, 12);
            cmdListPorts.Name = "cmdListPorts";
            cmdListPorts.Size = new Size(179, 63);
            cmdListPorts.TabIndex = 0;
            cmdListPorts.Text = "ListPorts";
            cmdListPorts.UseVisualStyleBackColor = true;
            cmdListPorts.Click += cmdListPorts_Click;
            // 
            // lstOutput
            // 
            lstOutput.FormattingEnabled = true;
            lstOutput.Location = new Point(12, 12);
            lstOutput.Name = "lstOutput";
            lstOutput.Size = new Size(591, 424);
            lstOutput.TabIndex = 1;
            // 
            // cmdOpen
            // 
            cmdOpen.Location = new Point(609, 81);
            cmdOpen.Name = "cmdOpen";
            cmdOpen.Size = new Size(179, 61);
            cmdOpen.TabIndex = 2;
            cmdOpen.Text = "Open";
            cmdOpen.UseVisualStyleBackColor = true;
            cmdOpen.Click += cmdOpen_Click;
            // 
            // cmdClose
            // 
            cmdClose.Location = new Point(609, 148);
            cmdClose.Name = "cmdClose";
            cmdClose.Size = new Size(179, 61);
            cmdClose.TabIndex = 3;
            cmdClose.Text = "Close";
            cmdClose.UseVisualStyleBackColor = true;
            cmdClose.Click += cmdClose_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(cmdClose);
            Controls.Add(cmdOpen);
            Controls.Add(lstOutput);
            Controls.Add(cmdListPorts);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Button cmdListPorts;
        private ListBox lstOutput;
        private Button cmdOpen;
        private Button cmdClose;
    }
}
