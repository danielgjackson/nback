using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace nback
{

    [Serializable]
    public class Session
    {

        private const int CURRENT_FILE_VERSION = 1;
        private int fileVersion;
        [Browsable(false)]
        public int FileVersion
        {
            get { return CURRENT_FILE_VERSION; }
            set { fileVersion = value; }
        }

        private bool modified;
        [ReadOnly(true)]
        public bool Modified { get { return modified; } }

        private string filename;
        [ReadOnly(true)]
        public string Filename { get { return filename; } }

        [ReadOnly(true)]
        public string Name { get { return Path.GetFileNameWithoutExtension(filename); } }

        private bool fromFile;
        [Browsable(false)]
        public bool FromFile { get { return fromFile; } }

        private BindingList<Entry> entries = new BindingList<Entry>();
        [Browsable(false)]
        public BindingList<Entry> Entries
        {
            get { return entries; }
            //private set { entries = value; }
        }

        [ReadOnly(true)]
        public int TotalEntries { get { return Entries.Count; } }


        public Session() : this(null, null) { }

        public Session(string name) : this(name, null) { }

        public Session(string name, ICollection<Entry> initialEntries)
        {
            filename = (name == null) ? "Session" : name;
            fromFile = false;
            //SetModified();
            modified = false;

            this.entries.Clear();
            if (initialEntries != null)
            {
                foreach (Entry entry in initialEntries)
                {
                    this.entries.Add(entry);
                }
            }
            this.entries.AllowEdit = true;
            this.entries.AllowNew = false;
            this.entries.AllowRemove = true;
            this.entries.ListChanged += new ListChangedEventHandler(entries_ListChanged);
        }

        void entries_ListChanged(object sender, ListChangedEventArgs e)
        {
            SetModified();
            //FireEntriesChanged();
        }

        private static void Serialize(Session session, StreamWriter sw)
        {
            XmlTextWriter writer = new XmlTextWriter(sw);
            XmlSerializer serializer = new XmlSerializer(typeof(Session));
            serializer.Serialize(sw, session);
            writer.Close();
        }

        private static Session DeSerialize(TextReader reader)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Session));
            Session session = (Session)serializer.Deserialize(reader);
            reader.Close();
            return session;
        }


        public void Clear()
        {
            if (entries.Count > 0)
            {
                //FireEntriesRemoved(0, entries.Count);
                entries.Clear();
                //FireEntriesChanged();
            }

            SetModified();
        }


        public bool New(string name)
        {
            Clear();
            filename = name;
            fromFile = false;
            SetModified();
            modified = false;
            return true;
        }



        private static List<Entry> LoadEntries(string loadFile)
        {
            StreamReader sr = new StreamReader(loadFile);
            List<Entry> entries = new List<Entry>();
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line != null)
                {
                    Entry entry = Entry.FromString(line);
                    entries.Add(entry);
                }
            }
            sr.Close();
            return entries;
        }

        private static bool SaveEntries(string saveFile, ICollection<Entry> entries)
        {
            StreamWriter sw = new StreamWriter(saveFile);
            foreach (Entry entry in entries)
            {
                sw.WriteLine(entry.ToString());
            }
            sw.Close();
            return true;
        }

        public static Session Load(string loadFile)
        {
            List<Entry> entries = LoadEntries(loadFile);
            Session session = new Session(Path.GetFileNameWithoutExtension(loadFile), entries);
            session.filename = loadFile;
            session.fromFile = true;
            session.modified = false;
            return session;
        }

        public bool Save(string saveFile)
        {
            if (!SaveEntries(saveFile, entries)) { return false; }
            this.filename = saveFile;
            this.fromFile = true;
            this.modified = false;
            return true;
        }

        public static Session LoadDeSerialize(string loadFile)
        {
            StreamReader sr = new StreamReader(loadFile);
            Session session = DeSerialize(sr);
            //sr.Close();
            //session.Clear();
            session.filename = loadFile;
            session.fromFile = true;
            session.modified = false;
            return session;
        }

        public bool SaveSerialize(string saveFile)
        {
            StreamWriter sw = new StreamWriter(saveFile);
            Serialize(this, sw);
            //sw.Close();
            filename = saveFile;
            fromFile = true;
            modified = false;
            return true;
        }


        public void SetModified()
        {
            modified = true;
            FireSessionChanged();
        }

        public void ClearModified()
        {
            modified = false;
        }

        private void FireSessionChanged()
        {
            modified = true;
            SessionEventArgs e = new SessionEventArgs(this);
            if (SessionChanged != null) { SessionChanged.Invoke(this, e); }
        }

#region Events
        public class SessionEventArgs : EventArgs
        {
            private Session session;
            public Session Session { get { return session; } }
            public SessionEventArgs(Session session) : base()
            {
                this.session = session;
            }
        }

        // The delegate used to call the event handlers
        public delegate void SessionEventDelegate(object sender, SessionEventArgs e);

        // Changed event
        public event SessionEventDelegate SessionChanged;
        //public event SessionEventDelegate SessionPropertyChanged;
#endregion

    }
}
