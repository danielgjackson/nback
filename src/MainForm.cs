using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;


namespace nback
{
    public partial class MainForm : Form
    {

        public static Settings Settings { get; private set; }
        private string settingsFile;

        //public Session Session { get { return speech.Session; } private set { speech.Session = value; } }

        private const string FILEDIALOG_FILTER = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
        private int newSessionNumber = 0;

        private Speech speech;

        public MainForm(string filename)
        {
            InitializeComponent();

            TextboxTraceListener textBoxTracer = new TextboxTraceListener(textBoxLog);
            textBoxTracer.TraceOutputOptions = TraceOptions.None;
            Trace.Listeners.Add(textBoxTracer);
            Trace.WriteLine("Started.");

            settingsFile = Settings.GetFullPath("settings.xml");
            Settings = new Settings();
            try
            {
                if (File.Exists(settingsFile))
                {
                    Settings = Settings.DeSerialize(new StreamReader(settingsFile));
                }
                else
                {
                    Trace.TraceWarning("WARNING: Settings file not found: " + settingsFile);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Problem reading settings file: " + settingsFile + " -- " + e.Message);
            }

            speech = new Speech();
            speech.SpeechStatusChanged += new Speech.SpeechEventDelegate(speech_SpeechStatusChanged);

            dataGridView.DataSource = speech.SpeechBindingSource;

            dataGridView.AutoGenerateColumns = false;
            dataGridView.AllowUserToAddRows = true;
            dataGridView.AllowUserToDeleteRows = true;
            dataGridView.AllowUserToOrderColumns = true;
            dataGridView.AllowUserToResizeColumns = true;
            dataGridView.AllowUserToResizeRows = false;
            dataGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;


            DataGridViewTextBoxColumn col1 = new DataGridViewTextBoxColumn();
            col1.ReadOnly = true;
            col1.ValueType = typeof(long); // typeof(DateTime);
            col1.HeaderText = "Speak time";
            col1.DataPropertyName = "SpeakTimestamp";
            col1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            //col1.DefaultCellStyle.Format = Entry.DATE_TIME_FORMAT;
            col1.Width = 120;
            dataGridView.Columns.Add(col1);

            DataGridViewTextBoxColumn col2 = new DataGridViewTextBoxColumn();
            col2.ReadOnly = true;
            col2.ValueType = typeof(int);
            col2.HeaderText = "Spoken";
            col2.DataPropertyName = "SpeakSymbol";
            col2.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //col2.DefaultCellStyle.Format = Entry.SYMBOL_FORMAT;
            col2.Width = 80;
            dataGridView.Columns.Add(col2);

            DataGridViewTextBoxColumn col3 = new DataGridViewTextBoxColumn();
            col3.ReadOnly = true;
            col3.ValueType = typeof(long); // typeof(DateTime);
            col3.HeaderText = "Receive time";
            col3.DataPropertyName = "ReceiveTimestamp";
            col3.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            //col3.DefaultCellStyle.Format = Entry.DATE_TIME_FORMAT;
            col3.Width = 120;
            dataGridView.Columns.Add(col3);

            DataGridViewTextBoxColumn col4 = new DataGridViewTextBoxColumn();
            col4.ReadOnly = true;
            col4.ValueType = typeof(int);
            col4.HeaderText = "Received";
            col4.DataPropertyName = "ReceiveSymbol";
            col4.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //col4.DefaultCellStyle.Format = Entry.SYMBOL_FORMAT;
            col4.Width = 80;
            dataGridView.Columns.Add(col4);

            DataGridViewTextBoxColumn col5 = new DataGridViewTextBoxColumn();
            col5.ReadOnly = true;
            col5.ValueType = typeof(float);
            col5.HeaderText = "n-back";
            col5.DataPropertyName = "NBack";
            col5.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col5.DefaultCellStyle.Format = "N2";
            col5.Width = 70;
            dataGridView.Columns.Add(col5);

            DataGridViewTextBoxColumn col6 = new DataGridViewTextBoxColumn();
            col6.ReadOnly = true;
            col6.ValueType = typeof(bool); // typeof(DateTime);
            col6.HeaderText = "Match";
            col6.DataPropertyName = "Match";
            col6.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //col6.DefaultCellStyle.Format = Entry.DATE_TIME_FORMAT;
            col6.Width = 70;
            dataGridView.Columns.Add(col6);

            DataGridViewTextBoxColumn col7 = new DataGridViewTextBoxColumn();
            col7.ReadOnly = true;
            col7.ValueType = typeof(bool);
            col7.HeaderText = "In time";
            col7.DataPropertyName = "InTIme";
            col7.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //col7.DefaultCellStyle.Format = "N2";
            col7.Width = 70;
            dataGridView.Columns.Add(col7);

            DataGridViewTextBoxColumn col8 = new DataGridViewTextBoxColumn();
            col8.ReadOnly = true;
            col8.ValueType = typeof(bool);
            col8.HeaderText = "Pass";
            col8.DataPropertyName = "Pass";
            col8.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //col8.DefaultCellStyle.Format = "N2";
            col8.Width = 70;
            dataGridView.Columns.Add(col8);

            speech.Enable = true;
            speech.Speak = true;
            speech.Generate = true;

            // Start update
            timerUpdate.Enabled = true;

            LoadFile(filename);

            UpdateSettings();

            SystemColorsChanged += MainForm_SystemColorsChanged;

            //toolStripButtonPlay.Focus();
        }

        void MainForm_SystemColorsChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        void speech_SpeechStatusChanged(object sender, Speech.SpeechEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Speech.SpeechEventDelegate(speech_SpeechStatusChanged), new object[] { sender, e });
                return;
            }
            /*
            if (e.Volume >= 0) { toolStripProgressBarVolume.Value = e.Volume; }
            if (e.Status != null) { toolStripLabelSpeechStatus.Text = e.Status; }
            if (e.Index >= 0)
            {
                if (e.Index < dataGridView.Rows.Count && (dataGridView.SelectedRows.Count != 1 || !dataGridView.Rows[e.Index].Selected))
                {
                    dataGridView.ClearSelection();
                    dataGridView.Rows[e.Index].Selected = true;
                }
            }
            */
            UpdateEnabled();
        }


        public MainForm() : this(null) { ; }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileNew();
            UpdateEnabled();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileOpen();
            UpdateEnabled();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileSave(false);
            UpdateEnabled();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileSave(true);
            UpdateEnabled();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            copyToolStripMenuItem_Click(sender, e);
            deleteToolStripMenuItem_Click(sender, e);
            UpdateEnabled();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(dataGridView.GetClipboardContent());
            UpdateEnabled();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ;
            UpdateEnabled();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                speech.SpeechBindingSource.Remove(row.DataBoundItem);
            }
            UpdateEnabled();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView.SelectAll();
            UpdateEnabled();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsForm optionsForm = new OptionsForm("Options", Settings.Clone());
            DialogResult result = optionsForm.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                Settings = (Settings)optionsForm.Settings;
                UpdateSettings();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new AboutBox()).ShowDialog(this);
        }


        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                Settings.Serialize(Settings, new StreamWriter(settingsFile));
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem writing settings file: " + ex.Message);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CheckSaveIfModified())
            {
                e.Cancel = true;
            }
            else
            {
                timerUpdate.Enabled = false;
                if (speech != null)
                {
                    speech.Dispose();
                }
            }
        }

        private void dataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show(this, "Data format error, please correct.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            e.Cancel = true;
            UpdateEnabled();
        }





        private void UpdateSettings()
        {
            //if (speech != null) { speech.UpdateInterval = Settings.Interval; }
        }

        private bool LoadFile(string filename)
        {
            Session session = null;

            if (filename != null && filename.Trim().Length > 0)
            {
                session = Session.Load(filename);
                if (session == null)
                {
                    MessageBox.Show(this, "Problem loading session: " + filename, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    MessageBox.Show(this, "Note that only the spoken/received tokens and timestamps are loaded.\r\n\r\nThe remaining match columns are calculated from the current settings only.\r\n\r\nOpen the file in Excel to see the original data.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }

            if (session == null)
            {
                string name;
                newSessionNumber++;
                name = "Session" + newSessionNumber;
                session = new Session(name);
            }
            //session.ClearModified();

            speech.Session = session;

            toolStripButtonRestart.PerformClick();

            return true;
        }

        private bool FileNew()
        {
            LoadFile(null);
            UpdateEnabled();
            return true;
        }

        private bool FileOpen()
        {
            if (!CheckSaveIfModified())
            {
                return false;
            }

            openFileDialog.Filter = FILEDIALOG_FILTER;
            DialogResult result = openFileDialog.ShowDialog(this);
            if (result != DialogResult.OK)
            {
                return false;
            }
            int errors = 0;
            foreach (string filename in openFileDialog.FileNames)
            {
                if (filename == null || filename.Trim().Length == 0)
                {
                    errors++;
                }
                else
                {
                    if (!LoadFile(filename)) { errors++; }
                }
            }
            UpdateEnabled();
            return (errors == 0);
        }

        private bool FileSave(bool forceSaveAs)
        {
            string saveFile = speech.Session.FromFile ? speech.Session.Filename : null;
            if (forceSaveAs || saveFile == null)
            {
                saveFileDialog.Filter = FILEDIALOG_FILTER;
                saveFileDialog.FileName = speech.Session.Name;
                DialogResult result = saveFileDialog.ShowDialog(this);
                if (result != DialogResult.OK)
                {
                    return false;
                }
                saveFile = saveFileDialog.FileName;
            }
            bool ret = speech.Session.Save(saveFile);
            if (!ret) { MessageBox.Show(this, "Problem saving: " + saveFile, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1); }
            UpdateEnabled();
            return ret;
        }

        private bool CheckSaveIfModified()
        {
            while (true)
            {
                if (!speech.Session.Modified)
                {
                    // Continue: File not modified
                    return true;
                }

                DialogResult result = MessageBox.Show(this, "Do you want to save changes to " + speech.Session.Name + "?", Application.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);

                if (result == DialogResult.No)
                {
                    // Continue: User discarding changes
                    return true;
                }
                else if (result == DialogResult.Yes)
                {
                    // Save: User saving changes
                    if (!FileSave(false))
                    {
                        // Do not continue: User cancelled save-as, or save failed
                        return false;
                    }

                    // If truly saved successfully, will fall out of the loop next time

                }
                else // result == DialogResult.Cancel
                {
                    // Do not continue: User cancelled
                    return false;
                }
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (filenames != null)
            {
                foreach (string filename in filenames)
                {
                    LoadFile(filename);
                }
            }
        }



        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            speech.Update();
        }

        private void toolbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip.Visible = !toolStrip.Visible;
            UpdateEnabled();
        }

        private void statusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStrip.Visible = !statusStrip.Visible;
            UpdateEnabled();
        }

        private void toolStripButtonGenerate_Click(object sender, EventArgs e)
        {
            speech.Generate = !speech.Generate;
            UpdateEnabled();
        }

        private void listenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            speech.Enable = !speech.Enable;
            UpdateEnabled();
        }

        private void speakToolStripMenuItem_Click(object sender, EventArgs e)
        {
            speech.Speak = !speech.Speak;
            UpdateEnabled();
        }

        private void simulateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            speech.Simulate = !speech.Simulate;
            UpdateEnabled();
        }

        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            speech.Play = true;
            UpdateEnabled();
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            speech.Play = false;
            UpdateEnabled();
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            speech.Play = false;
            speech.SpeechBindingSource.MoveFirst();
            UpdateEnabled();
        }

        private void splitContainerMain_SplitterMoved(object sender, SplitterEventArgs e)
        {
            UpdateEnabled();
        }

        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitContainerMain.Panel2Collapsed = !splitContainerMain.Panel2Collapsed;
            UpdateEnabled();
        }

        private void UpdateEnabled()
        {
            bool hasSelection = (dataGridView.SelectedCells.Count > 0);

            toolStripProgressBarVolume.Value = speech.Volume;
            toolStripStatusLabel.Image = imageListStatus.Images[speech.StatusImage];
            toolStripStatusLabel.Text = speech.StatusText;

            // Edit
            undoToolStripMenuItem.Enabled = false;
            redoToolStripMenuItem.Enabled = false;
            cutToolStripMenuItem.Enabled = hasSelection;
            copyToolStripMenuItem.Enabled = hasSelection;
            pasteToolStripMenuItem.Enabled = false;
            deleteToolStripMenuItem.Enabled = hasSelection;
            selectAllToolStripMenuItem.Enabled = (dataGridView.RowCount > 0);

            // View
            toolbarToolStripMenuItem.Checked = toolStrip.Visible;
            statusBarToolStripMenuItem.Checked = statusStrip.Visible;
            debugToolStripMenuItem.Checked = !splitContainerMain.Panel2Collapsed;

            // Speech - Listen
            if (speech == null || !speech.Enable)
            {
                // If we don't have a recognizer or it's not enabled, show unchecked...
                toolStripButtonListen.Checked = listenToolStripMenuItem.Checked = false;
                toolStripButtonListen.Text = "Listen";
                //listenToolStripMenuItem.Text = "&Listen";
                //toolStripButtonListen.BackColor = SystemColors.Control;
            }
            else if (!speech.Listening)
            {
                // We do have a recognizer that's enabled, but it's not currently listening, show indeterminate state...
                toolStripButtonListen.CheckState = listenToolStripMenuItem.CheckState = CheckState.Indeterminate;
                toolStripButtonListen.Text = "(Listen)";
                //listenToolStripMenuItem.Text = "(&Listen)";
                //toolStripButtonListen.BackColor = SystemColors.ControlLightLight;
            }
            else
            {
                // We do have an enabled and listening recognizer, show checked...
                toolStripButtonListen.Checked = listenToolStripMenuItem.Checked = true;
                toolStripButtonListen.Text = "Listen";
                //listenToolStripMenuItem.Text = "&Listen";
                //toolStripButtonListen.BackColor = SystemColors.Control;
            }


            // Speech - other options
            toolStripButtonSpeak.Checked = speakToolStripMenuItem.Checked = (speech == null) ? false : speech.Speak;
            toolStripButtonSimulate.Checked = simulateToolStripMenuItem.Checked = (speech == null) ? false : speech.Simulate;

            // Speech - playback
            toolStripButtonPlay.Checked = playToolStripMenuItem.Checked = (speech == null) ? false : speech.Play;
            toolStripButtonPause.Checked = pauseToolStripMenuItem.Checked = (speech == null) ? false : !speech.Play;
            toolStripButtonGenerate.Checked = generateToolStripMenuItem.Checked = (speech == null) ? false : speech.Generate;

            // Context Menu Strip - Toolbars
            toolbarToolStripMenuItem1.Checked = toolStrip.Visible;
            statusBarToolStripMenuItem1.Checked = statusStrip.Visible;
            debugToolStripMenuItem1.Checked = !splitContainerMain.Panel2Collapsed;

            // Application title
            string title = Application.ProductName;
            if (speech != null && speech.Session != null)
            {
                title = speech.Session.Name + (speech.Session.Modified ? "*" : "") + " - " + title;
            }
            if (this.Text != title)
            {
                this.Text = title;
            }

            /*
            // Status bar
            string status = "-";
            if (speech != null)
            {
                if (speech.Play)
                {
                    status = "Playing";
                }
                else
                {
                    status = "Stopped";
                }
            }
            // TODO: Calculate status label
            //if (toolStripStatusLabel.Text != status)
            //{
            //    toolStripStatusLabel.Text = status;
            //}
            */

            string listenString = speech != null ? speech.ListenString : "-";
            if (toolStripStatusLabelListenString.Text != listenString)
            {
                toolStripStatusLabelListenString.Text = listenString;
            }


            int x = speech.FractionX;
            int y = speech.FractionY;
            bool failed = speech.CurrentlyFailed;
            string fraction = "";
            fraction = (failed ? "Fail" : "Pass") + " (" + x + "/" + y + ")";

            if (failed)
            {
                toolStripLabelPassFail.Image = imageListPassStatus.Images[2];
            }
            else
            {
                toolStripLabelPassFail.Image = imageListPassStatus.Images[1];
            }
            toolStripLabelPassFail.Text = fraction;

        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            /*
            if (dataGridView.SelectedRows.Count > 0)
            {
                int index = dataGridView.SelectedRows[0].Index;
                speech.SelectedIndex = index;
            }
            */
            UpdateEnabled();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (!Speech.CheckPlatform(this))
            {
                this.Close();
            }
        }

    }
}
