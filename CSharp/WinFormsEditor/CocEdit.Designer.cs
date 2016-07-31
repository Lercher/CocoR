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
            this.VSplit = new System.Windows.Forms.SplitContainer();
            this.listAutocomplete = new System.Windows.Forms.ListView();
            this.textSource = new System.Windows.Forms.TextBox();
            this.VSplit.Panel1.SuspendLayout();
            this.VSplit.Panel2.SuspendLayout();
            this.VSplit.SuspendLayout();
            this.listAutocomplete.SuspendLayout();
            this.textSource.SuspendLayout();
            this.SuspendLayout();
            // 
            // VSplit
            // 
            this.VSplit.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.VSplit.Name = "VSplit";
            this.VSplit.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.VSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VSplit.Orientation = System.Windows.Forms.Orientation.Vertical; 
            this.VSplit.SplitterWidth = 5;           
            // 
            // textSource
            // 
            this.textSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textSource.Multiline = true;
            this.textSource.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textSource.Font = new System.Drawing.Font("Consolas", 12);
            // 
            // VSplit.Panel1
            // 
            this.VSplit.Panel1.Controls.Add(this.textSource);
            // 
            // VSplit.Panel2
            // 
            this.VSplit.Panel2.Controls.Add(this.listAutocomplete);
            this.VSplit.Size = new System.Drawing.Size(582, 270);
            this.VSplit.SplitterDistance = 400;
            this.VSplit.TabIndex = 0;
            this.VSplit.Text = "VSplit";
            // 
            // listAutocomplete
            // 
            this.listAutocomplete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listAutocomplete.Location = new System.Drawing.Point(0, 0);
            this.listAutocomplete.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listAutocomplete.Name = "listAutocomplete";
            this.listAutocomplete.Size = new System.Drawing.Size(382, 268);
            this.listAutocomplete.TabIndex = 0;
            this.listAutocomplete.View = System.Windows.Forms.View.List;
            // 
            // MySplitContainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 800);
            this.Controls.Add(this.VSplit);
            this.MaximizeBox = true;
            this.Name = "CocEdit";
            this.Text = "Coco/R Editor";
            this.VSplit.Panel1.ResumeLayout(false);
            this.VSplit.Panel2.ResumeLayout(false);
            this.VSplit.ResumeLayout(false);
            this.listAutocomplete.ResumeLayout(false);
            this.textSource.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer VSplit;
        private System.Windows.Forms.ListView listAutocomplete;
        private System.Windows.Forms.TextBox textSource;
    }
}

