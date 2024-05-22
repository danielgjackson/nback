using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace nback
{
    public partial class OptionsForm : Form
    {
        private Object settings;
        public Object Settings
        {
            get { return settings; }
        }

        public OptionsForm(String title, Object settings) : this(title, settings, true)
        {
            ;
        }

        public OptionsForm(String title, Object settings, bool allowCancel)
        {
            this.settings = settings;
            InitializeComponent();
            this.Text = title;
            this.propertyGrid.ToolbarVisible = false;
            this.propertyGrid.SelectedObject = this.settings;
            if (!allowCancel)
            {
                this.cancelButton.Visible = allowCancel;
                this.okButton.Location = this.cancelButton.Location;
            }
        }

        private static void PropertyGrid_MoveSplitter(PropertyGrid propertyGrid, int x)
        {
            try
            {
                object propertyGridView = typeof(PropertyGrid).InvokeMember("gridView", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, propertyGrid, null);
                propertyGridView.GetType().InvokeMember("MoveSplitterTo", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, propertyGridView, new object[] { x });
            }
            catch (Exception)
            {
                // Silently consume exception
            }
        }


        private void OptionsForm_Load(object sender, EventArgs e)
        {
            PropertyGrid_MoveSplitter(propertyGrid, 260);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

    }
}