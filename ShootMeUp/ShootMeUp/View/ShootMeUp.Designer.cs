namespace ShootMeUp
{
    partial class ShootMeUp
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
            ticker = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // ticker
            // 
            ticker.Enabled = true;
            ticker.Interval = 1;
            ticker.Tick += NewFrame;
            // 
            // ShootMeUp
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(528, 505);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "ShootMeUp";
            Text = "Shoot Me Up";
            Load += ShootMeUp_Load;
            KeyDown += ShootMeUp_KeyDown;
            KeyUp += ShootMeUp_KeyUp;
            MouseClick += ShootMeUp_MouseClick;
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer ticker;
    }
}