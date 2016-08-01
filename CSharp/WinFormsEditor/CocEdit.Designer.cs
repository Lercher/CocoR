namespace WinFormsEditor
{
    partial class CocEdit
    {

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.Drawing.Font ft = new System.Drawing.Font("Consolas", 12);

            this.HSplit = new System.Windows.Forms.SplitContainer();
            this.VSplit = new System.Windows.Forms.SplitContainer();
            this.listAutocomplete = new System.Windows.Forms.ListView();
            this.textSource = new System.Windows.Forms.RichTextBox();
            this.textLog = new System.Windows.Forms.TextBox();
            //
            this.HSplit.Panel1.SuspendLayout();
            this.HSplit.Panel2.SuspendLayout();
            this.HSplit.SuspendLayout();
            this.VSplit.Panel1.SuspendLayout();
            this.VSplit.Panel2.SuspendLayout();
            this.VSplit.SuspendLayout();
            this.listAutocomplete.SuspendLayout();
            this.textSource.SuspendLayout();
            this.SuspendLayout();
            // 
            // HSplit
            // 
            this.HSplit.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.HSplit.Name = "HSplit";
            this.HSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HSplit.Orientation = System.Windows.Forms.Orientation.Horizontal; 
            this.HSplit.SplitterWidth = 5;
            // 
            // VSplit
            // 
            this.VSplit.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.VSplit.Name = "VSplit";
            this.VSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VSplit.Orientation = System.Windows.Forms.Orientation.Vertical; 
            this.VSplit.SplitterWidth = 5;           
            // 
            // textSource
            // 
            this.textSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textSource.Multiline = true;
            this.textSource.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textSource.Font = ft;
            // 
            // textLog
            // 
            this.textLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textLog.Multiline = true;
            this.textLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textLog.Font = ft;
            // 
            // Panels
            // 
            this.HSplit.Panel1.Controls.Add(this.VSplit);
            this.HSplit.Panel2.Controls.Add(this.textLog);
            this.VSplit.Panel1.Controls.Add(this.textSource);
            this.VSplit.Panel2.Controls.Add(this.listAutocomplete);
            //
            this.HSplit.SplitterDistance = 300;
            this.VSplit.SplitterDistance = 400;
            // 
            // listAutocomplete
            // 
            this.listAutocomplete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listAutocomplete.Location = new System.Drawing.Point(0, 0);
            this.listAutocomplete.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listAutocomplete.Name = "listAutocomplete";
            this.listAutocomplete.Size = new System.Drawing.Size(382, 268);
            this.listAutocomplete.TabIndex = 0;
            this.listAutocomplete.View = System.Windows.Forms.View.Details;
            this.listAutocomplete.Font = ft;
            this.listAutocomplete.Columns.Add(new System.Windows.Forms.ColumnHeader());
            this.listAutocomplete.Columns.Add(new System.Windows.Forms.ColumnHeader());

            // 
            // MySplitContainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 1000);
            this.Controls.Add(this.HSplit);
            this.MaximizeBox = true;
            this.Name = "CocEdit";
            this.Text = "Coco/R Editor";
            this.VSplit.Panel1.ResumeLayout(false);
            this.VSplit.Panel2.ResumeLayout(false);
            this.VSplit.ResumeLayout(false);
            this.HSplit.Panel1.ResumeLayout(false);
            this.HSplit.Panel2.ResumeLayout(false);
            this.HSplit.ResumeLayout(false);
            this.listAutocomplete.ResumeLayout(false);
            this.textSource.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer HSplit;
        private System.Windows.Forms.SplitContainer VSplit;
        private System.Windows.Forms.ListView listAutocomplete;
        private System.Windows.Forms.RichTextBox textSource;
        private System.Windows.Forms.TextBox textLog;
    }
}

