/*
 * Created by SharpDevelop.
 * User: LB07270
 * Date: 30/5/2011
 * Time: 11:23
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace PlatCPL.Interfaces
{
	partial class MatlabFilesAnalyzer
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the control.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.button1 = new System.Windows.Forms.Button();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer3 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.button5 = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.trackBar1 = new System.Windows.Forms.TrackBar();
			this.label1 = new System.Windows.Forms.Label();
			this.button3 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.listView2 = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.button4 = new System.Windows.Forms.Button();
			this.button6 = new System.Windows.Forms.Button();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.splitContainer3.Panel1.SuspendLayout();
			this.splitContainer3.Panel2.SuspendLayout();
			this.splitContainer3.SuspendLayout();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(3, 3);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 0;
			this.button1.Text = "Open";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.B_Load);
			// 
			// richTextBox1
			// 
			this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBox1.Location = new System.Drawing.Point(0, 0);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.Size = new System.Drawing.Size(724, 107);
			this.richTextBox1.TabIndex = 1;
			this.richTextBox1.Text = "";
			// 
			// listView1
			// 
			this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
									this.columnHeader6,
									this.columnHeader7});
			this.listView1.Location = new System.Drawing.Point(3, 32);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(622, 133);
			this.listView1.TabIndex = 2;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "Variable";
			this.columnHeader6.Width = 299;
			// 
			// columnHeader7
			// 
			this.columnHeader7.Text = "File";
			this.columnHeader7.Width = 388;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = new System.Drawing.Point(3, 3);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer3);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.richTextBox1);
			this.splitContainer1.Size = new System.Drawing.Size(724, 456);
			this.splitContainer1.SplitterDistance = 345;
			this.splitContainer1.TabIndex = 3;
			// 
			// splitContainer3
			// 
			this.splitContainer3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer3.Location = new System.Drawing.Point(3, 3);
			this.splitContainer3.Name = "splitContainer3";
			// 
			// splitContainer3.Panel1
			// 
			this.splitContainer3.Panel1.Controls.Add(this.splitContainer2);
			// 
			// splitContainer3.Panel2
			// 
			this.splitContainer3.Panel2.Controls.Add(this.pictureBox1);
			this.splitContainer3.Panel2.Controls.Add(this.button4);
			this.splitContainer3.Size = new System.Drawing.Size(718, 339);
			this.splitContainer3.SplitterDistance = 628;
			this.splitContainer3.TabIndex = 4;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.button6);
			this.splitContainer2.Panel1.Controls.Add(this.listView1);
			this.splitContainer2.Panel1.Controls.Add(this.button1);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.button5);
			this.splitContainer2.Panel2.Controls.Add(this.textBox1);
			this.splitContainer2.Panel2.Controls.Add(this.trackBar1);
			this.splitContainer2.Panel2.Controls.Add(this.label1);
			this.splitContainer2.Panel2.Controls.Add(this.button3);
			this.splitContainer2.Panel2.Controls.Add(this.button2);
			this.splitContainer2.Panel2.Controls.Add(this.listView2);
			this.splitContainer2.Size = new System.Drawing.Size(628, 339);
			this.splitContainer2.SplitterDistance = 168;
			this.splitContainer2.TabIndex = 3;
			// 
			// button5
			// 
			this.button5.Location = new System.Drawing.Point(576, 7);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(38, 23);
			this.button5.TabIndex = 4;
			this.button5.Text = "Export2Ascii";
			this.button5.UseVisualStyleBackColor = true;
			this.button5.Click += new System.EventHandler(this.B_Export);
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(454, 7);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(100, 20);
			this.textBox1.TabIndex = 3;
			this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox1KeyDown);
			// 
			// trackBar1
			// 
			this.trackBar1.Enabled = false;
			this.trackBar1.Location = new System.Drawing.Point(165, 3);
			this.trackBar1.Maximum = 50;
			this.trackBar1.Name = "trackBar1";
			this.trackBar1.Size = new System.Drawing.Size(283, 45);
			this.trackBar1.TabIndex = 2;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(560, 10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(10, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "-";
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(84, 7);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(75, 23);
			this.button3.TabIndex = 0;
			this.button3.Text = "Remove";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.B_Remove);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(3, 7);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 0;
			this.button2.Text = "Add";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.B_Add);
			// 
			// listView2
			// 
			this.listView2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.listView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
									this.columnHeader1,
									this.columnHeader2,
									this.columnHeader3,
									this.columnHeader4,
									this.columnHeader5});
			this.listView2.Location = new System.Drawing.Point(3, 51);
			this.listView2.Name = "listView2";
			this.listView2.Size = new System.Drawing.Size(622, 113);
			this.listView2.TabIndex = 0;
			this.listView2.UseCompatibleStateImageBehavior = false;
			this.listView2.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Variable";
			this.columnHeader1.Width = 280;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "MinT";
			this.columnHeader2.Width = 79;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "MaxT";
			this.columnHeader3.Width = 74;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "CustomT";
			this.columnHeader4.Width = 78;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Value";
			this.columnHeader5.Width = 139;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBox1.Location = new System.Drawing.Point(3, 32);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(80, 304);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(3, 3);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(75, 23);
			this.button4.TabIndex = 1;
			this.button4.Text = "Draw";
			this.button4.UseVisualStyleBackColor = true;
			this.button4.Click += new System.EventHandler(this.B_Redraw);
			// 
			// button6
			// 
			this.button6.Location = new System.Drawing.Point(84, 3);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(75, 23);
			this.button6.TabIndex = 3;
			this.button6.Text = "Test";
			this.button6.UseVisualStyleBackColor = true;
			this.button6.Click += new System.EventHandler(this.B_Test);
			// 
			// MatlabFilesAnalyzer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Name = "MatlabFilesAnalyzer";
			this.Size = new System.Drawing.Size(730, 462);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer3.Panel1.ResumeLayout(false);
			this.splitContainer3.Panel2.ResumeLayout(false);
			this.splitContainer3.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.Panel2.PerformLayout();
			this.splitContainer2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TrackBar trackBar1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.ColumnHeader columnHeader7;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.ListView listView2;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.SplitContainer splitContainer3;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.Button button1;
	}
}
