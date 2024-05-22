using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace nback
{
    [Serializable]
    public class Entry : INotifyPropertyChanged
    {
        //public const string DATE_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss.ffff";
        //public const string DATE_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
        //public const string SYMBOL_FORMAT = "N";

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(name)); }
        }

        /*
        private DateTime timestamp;
        public DateTime Timestamp
        {
            get { return timestamp; }
            set { timestamp = value; this.NotifyPropertyChanged("Timestamp"); }
        }
        */

        private long speakTimestamp;
        public long SpeakTimestamp
        {
            get { return speakTimestamp; }
            set { speakTimestamp = value; this.NotifyPropertyChanged("SpeakTimestamp"); this.NotifyPropertyChanged("Delay"); this.NotifyPropertyChanged("Match"); }
        }

        private int speakSymbol;
        public int SpeakSymbol
        {
            get { return speakSymbol; }
            set { this.speakSymbol = value; this.NotifyPropertyChanged("SpeakSymbol"); this.NotifyPropertyChanged("Match"); }
        }

        private long receiveTimestamp;
        public long ReceiveTimestamp
        {
            get { return receiveTimestamp; }
            set { receiveTimestamp = value; this.NotifyPropertyChanged("ReceiveTimestamp"); this.NotifyPropertyChanged("Delay"); this.NotifyPropertyChanged("Match"); }
        }

        private int receiveSymbol;
        public int ReceiveSymbol
        {
            get { return receiveSymbol; }
            set { this.receiveSymbol = value; this.NotifyPropertyChanged("ReceiveSymbol"); this.NotifyPropertyChanged("Match"); }
        }

        /*
        public int Delay
        {
            get
            {
                //if (speakSymbol != receiveSymbol) { return 0; }
                if (receiveTimestamp == 0 || speakTimestamp == 0) { return 0; }
                return (int)(receiveTimestamp - speakTimestamp);
            }
        }
        */

        public float NBack
        {
            get
            {
                if (receiveTimestamp == 0 || speakTimestamp == 0 || MainForm.Settings.Interval == 0) { return 0.0f; }
                float nBack = (float)(receiveTimestamp - speakTimestamp - MainForm.Settings.Delay) / MainForm.Settings.Interval;
                return nBack;
            }
        }

        public bool Match
        {
            get
            {
                if (speakSymbol == receiveSymbol)
                {
                    // TODO: Depends on Delay amount
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool InTime
        {
            get
            {
                float nBack = NBack;
                if (nBack <= 0.0f) { return false; }
                return nBack >= MainForm.Settings.MinimumNBack && nBack <= MainForm.Settings.MaximumNBack;
            }
        }

        public bool Pass
        {
            get
            {
                return Match && InTime;
            }
        }

        private long simulateTimestamp;
        public long SimulateTimestamp
        {
            get { return simulateTimestamp; }
            set { simulateTimestamp = value; this.NotifyPropertyChanged("SimulateTimestamp"); }
        }

        public Entry()
        {
            //this();
        }

        public Entry(long speakTimestamp, int speakSymbol)
        {
            PropertyChanged = null;
            SpeakTimestamp = speakTimestamp;
            SpeakSymbol = speakSymbol;
        }

        public Entry(long speakTimestamp, int speakSymbol, long receiveTimestamp, int receiveSymbol) : this(speakTimestamp, speakSymbol)
        {
            ReceiveTimestamp = receiveTimestamp;
            ReceiveSymbol = receiveSymbol;
        }

        static public Entry FromString(string data)
        {
            //DateTime timestamp = DateTime.MinValue;
            long stimestamp = 0;
            int ssymbol = -1;
            long rtimestamp = 0;
            int rsymbol = -1;

            if (data == null) { return null; }
            data = data.Trim();
            string[] parts = data.Split(new char[] { ',' });


            if (parts.Length > 0) { long.TryParse(parts[0], out stimestamp); }
            if (parts.Length > 1) { int.TryParse(parts[1], out ssymbol); }
            if (parts.Length > 2) { long.TryParse(parts[2], out rtimestamp); }
            if (parts.Length > 3) { int.TryParse(parts[3], out rsymbol); }
            
            Entry entry = new Entry(stimestamp, ssymbol, rtimestamp, rsymbol);
            return entry;
        }

        public override string ToString()
        {
            //return timestamp.ToString(DATE_TIME_FORMAT) + "," + symbol + "";
            //return ReceiveTimestamp.ToString() + "," + speakSymbol + "";
            return SpeakTimestamp.ToString() + "," + SpeakSymbol + "," + ReceiveTimestamp.ToString() + "," + ReceiveSymbol.ToString() + "," + NBack.ToString() + "," + (Match ? "1" : "0") + "," + InTime.ToString() + "," + Pass.ToString() + "";
        }

    }
}
