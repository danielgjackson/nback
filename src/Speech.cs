#define USE_INPROC
#if USE_INPROC
using SpeechRecognizer = nback.SpeechRecognizerInProc;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.Diagnostics;
using System.Windows.Forms;

namespace nback
{
    public class Speech : IDisposable
    {

        // Speech
        private static string[] NUMBERS = new string[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };


        private SpeechSynthesizer speechSynthesizer;
        private SpeechRecognizer speechRecognizer;
        private SpeechSynthesizer speechSynthesizerResponse;

        private const int SEARCH_LENGTH = 10;
        private const int INDEX_FIRST = 1;
        private const int INDEX_LAST = 9;

        private long speechStartTime = 0;
        private List<int> speechHypotheses = new List<int>();
        private List<int> speechRecognitions = new List<int>();

        private Grammar[] grammars;
        private Session session;
        public Session Session
        {
            get { return session; }
            set
            {
                session = value;
                bindingSource.DataSource = session == null ? null : session.Entries;
                FireStatusChanged();
            }
        }
        private BindingSource bindingSource = new BindingSource();
        public BindingSource SpeechBindingSource
        {
            get { return bindingSource; }
        }

        private Settings Settings
        {
            get { return MainForm.Settings; }
        }

        private string statusText;
        public string StatusText
        {
            get { return statusText; }
            private set { statusText = value; FireStatusChanged(); }
        }

        public enum Status { info, warning, error };

        private Status status;
        public int StatusImage
        {
            get { return (int)status; }
        }


        private string listenString = "";
        public string ListenString
        { 
            get { return listenString; }
            set { listenString = value; FireStatusChanged(); }
        }

        private int volume;
        public int Volume
        {
            get { return volume; }
            private set { volume = value; FireStatusChanged(); }
        }

        //private static long currentTime = 0;
        public long Now
        {
            get 
            {
                return Now2;
                //currentTime = speechRecognizer == null ? 0 : (speechRecognizer.AudioPosition.Ticks / TimeSpan.TicksPerMillisecond);
                //return currentTime;
                //return DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond; 
                //return 0;
            } 
        }
        public long Now2
        {
            get 
            {
                return DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond; 
            }
        }

        private long lastUpdated = 0;

        private Random random = new Random();


        public int FractionX { get; private set; }
        public int FractionY { get; private set; }
        public bool CurrentlyFailed { get; private set; }

        public Speech()
        {
            CurrentlyFailed = false;
            FractionX = 0;
            FractionY = 0;
            //speechSynthesizer.SetOutputToWaveFile();

            // Create speech synthesizer
            speechSynthesizer = new SpeechSynthesizer();
            //speechSynthesizer.SelectVoice("Microsoft Mike");
            speechSynthesizer.SelectVoiceByHints(VoiceGender.Male, VoiceAge.NotSet);

            // Create output speech synthesizer
            speechSynthesizerResponse = new SpeechSynthesizer();
            //speechSynthesizerResponse.SelectVoice("Microsoft Mary");
            speechSynthesizerResponse.SelectVoiceByHints(VoiceGender.Female, VoiceAge.NotSet);
            speechSynthesizerResponse.Volume = 0;

            System.Diagnostics.Trace.TraceInformation("SYNTHESIZER: " + speechSynthesizer.Voice + " (simulate: " + speechSynthesizerResponse.Voice + ").");

            // Create speech recognizer
            speechRecognizer = new SpeechRecognizerInProc();
            speechRecognizer = new SpeechRecognizer();
            System.Diagnostics.Trace.TraceInformation("RECOGNIZER: " + "[" + speechRecognizer.RecognizerInfo.Id + "] " + speechRecognizer.RecognizerInfo.Name + " - \"" + speechRecognizer.RecognizerInfo.Description + "\".");

            speechRecognizer.Enabled = false;
            speechRecognizer.UnloadAllGrammars();
            speechRecognizer.MaxAlternates = 10;
            speechRecognizer.StateChanged += speechRecognizer_StateChanged;
            speechRecognizer.AudioLevelUpdated += speechRecognizer_AudioLevelUpdated;
            speechRecognizer.SpeechDetected += speechRecognizer_SpeechDetected;
            speechRecognizer.SpeechHypothesized += speechRecognizer_SpeechHypothesized;
            speechRecognizer.SpeechRecognitionRejected += speechRecognizer_SpeechRecognitionRejected;
            speechRecognizer.SpeechRecognized += speechRecognizer_SpeechRecognized;
            speechRecognizer.AudioSignalProblemOccurred += speechRecognizer_AudioSignalProblemOccurred;

            grammars = new Grammar[NUMBERS.Length];
            try
            {
                for (int i = 0; i < grammars.Length; i++)
                {
                    SemanticResultValue numberValue = new SemanticResultValue(NUMBERS[i], i);
                    SemanticResultKey numberKey = new SemanticResultKey("number", numberValue);
                    GrammarBuilder grammarBuilder = new GrammarBuilder(numberKey);
                    grammarBuilder.Culture = speechRecognizer.RecognizerInfo.Culture;       // ADDED
                    grammars[i] = new Grammar(grammarBuilder);
                    grammars[i].Name = i.ToString();
                    //grammars[i].SpeechRecognized += Speech_GrammarSpeechRecognized;
                    speechRecognizer.LoadGrammar(grammars[i]);
                }
            }
            catch (InvalidOperationException e)
            {
                MessageBox.Show("ERROR: " + e.Message + " -- possible culture mis-match.");
            }

            //speechRecognizer.Enabled = true;
        }

        public static bool CheckPlatform(IWin32Window owner)
        {
            System.Diagnostics.Trace.TraceInformation("PLATFORM: " + Environment.OSVersion.VersionString + "");
#if USE_INPROC
            System.Diagnostics.Trace.TraceInformation("NOTE: This build is using an SpInprocRecognizer.");
            return true;
#else
            if (Environment.OSVersion.Version.Major > 6 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 1))
            {
                return (MessageBox.Show(owner, 
                    "The version of the speech recognizer on your operating system (" + Environment.OSVersion.VersionString + ") " +
                    "is known to cause problems with the current version of this software.\r\n" +
                    "\r\n" +
                    "System commands will be recognized in addition to user responses " +
                    "which will seriously interfere with the operation of the software.\r\n" +
                    "\r\n" +
                    "(The recognizer should be created as a CLSID_SpInprocRecognizer instead of a CLSID_SpSharedRecognizer.)\r\n" +
                    "\r\n" +
                    "The recommended platform for this version is Windows XP.\r\n" +
                    "\r\n" +
                    "Would you like to continue anyway despite these problems?"
                    , "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == DialogResult.OK);
            }
            return true;
#endif
        }

        public bool Enable
        {
            get { return speechRecognizer.Enabled; }
            set 
            {
                if (speechRecognizer.Enabled != value)
                {
                    speechRecognizer.Enabled = value;
                    if (!Listening)
                    {
                        if (speechRecognizer.Enabled)
                        {
                            SpeechStatus(Status.error, "<recognizer enabled but not listening - use Windows speech toolbar>");
                        }
                        else
                        {
                            SpeechStatus(Status.warning, "<recognizer disabled and stopped>");
                        }
                    }
                    else
                    {
                        if (speechRecognizer.Enabled)
                        {
                            SpeechStatus(Status.info, "<recognizer enabled and listening>");
                        }
                        else
                        {
                            SpeechStatus(Status.warning, "<recognizer disabled but still listening>");
                        }
                    }
                }
                SetLikely();
            }
        }

        public bool Listening
        {
            get { return speechRecognizer.State == RecognizerState.Listening; }
        }

        private bool speak;
        public bool Speak
        {
            get { return speak; }
            set { if (speak != value) { speak = value; FireStatusChanged(); } }
        }

        private bool generate;
        public bool Generate
        {
            get { return generate; }
            set { if (generate != value) { generate = value; FireStatusChanged(); } }
        }

        public bool Simulate
        {
            get
            {
                return speechSynthesizerResponse.Volume != 0;
            }

            set
            {
                speechSynthesizer.Volume = value ? 10 : 100;
                speechSynthesizerResponse.Volume = value ? 100 : 0;
                FireStatusChanged();
            }
        }

        private bool play = false;
        public bool Play { get { return play; } set { if (play != value) { play = value; FireStatusChanged(); } } }

        void speechRecognizer_StateChanged(object sender, System.Speech.Recognition.StateChangedEventArgs e)
        {
//currentTime = speechRecognizer == null ? 0 : (((SpeechRecognizer)sender).AudioPosition.Ticks / TimeSpan.TicksPerMillisecond);
            Trace.TraceInformation("StateChanged: " + e.RecognizerState);
            if (e.RecognizerState == RecognizerState.Stopped)
            {
                Volume = 0;
                if (((SpeechRecognizer)sender).Enabled)
                {
                    SpeechStatus(Status.warning, "<recognizer stopped but enabled>");
                }
                else
                {
                    SpeechStatus(Status.warning, "<recognizer stopped and disabled>");
                }
            }
            else if (e.RecognizerState == RecognizerState.Listening)
            {
                if (((SpeechRecognizer)sender).Enabled)
                {
                    SpeechStatus(Status.info, "<recognizer listening and enabled>");
                }
                else
                {
                    SpeechStatus(Status.warning, "<recognizer listening but not enabled>");
                }
            }
            FireStatusChanged();
        }

        void speechRecognizer_AudioLevelUpdated(object sender, AudioLevelUpdatedEventArgs e)
        {
//currentTime = speechRecognizer == null ? 0 : (((SpeechRecognizer)sender).AudioPosition.Ticks / TimeSpan.TicksPerMillisecond);
            //Trace.TraceInformation("AudioLevelUpdated: " + e.AudioLevel);
            Volume = e.AudioLevel;
        }

        void speechRecognizer_SpeechDetected(object sender, SpeechDetectedEventArgs e)
        {
//currentTime = speechRecognizer == null ? 0 : (((SpeechRecognizer)sender).AudioPosition.Ticks / TimeSpan.TicksPerMillisecond);
            Trace.TraceInformation("SpeechDetected: " + e.AudioPosition);
            speechStartTime = Now;
            speechHypotheses.Clear();
            speechRecognitions.Clear();
            //numberList.AddHypothesis(-1, speechStartTime);
            SpeechStatus(Status.info, "");
        }

        void speechRecognizer_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
//currentTime = speechRecognizer == null ? 0 : (((SpeechRecognizer)sender).AudioPosition.Ticks / TimeSpan.TicksPerMillisecond);
            Trace.TraceInformation("SpeechHypothesized: " + e.Result.Text);
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, SemanticValue> kv in e.Result.Semantics)
            {
                if (kv.Key == "number")
                {
                    int v = (int)(kv.Value.Value);
                    speechHypotheses.Add(v);
                    if (sb.Length > 0) { sb.Append(","); }
                    sb.Append(v);
//                    numberList.AddHypothesis(v, speechStartTime);
                    Trace.TraceInformation("NUMBER: " + v + " (" + e.Result.Confidence + ")");
                }
            }
            System.Collections.ObjectModel.ReadOnlyCollection<RecognizedPhrase> phrases = e.Result.Alternates;
            if (phrases.Count > 1)
            {
                foreach (RecognizedPhrase phrase in phrases)
                {
                    Trace.TraceInformation("ALTERNATES: " + phrase.Text + " (" + phrase.Confidence + ")");
                    foreach (KeyValuePair<string, SemanticValue> kv in phrase.Semantics)
                    {
                        if (kv.Key == "number")
                        {
                            int v = (int)(kv.Value.Value);
                            speechHypotheses.Add(v);
                        }
                    }
                }
            }
            string numberString = sb.ToString();
            //Trace.TraceInformation("NUMBERS: " + numberString);
            SpeechStatus(Status.info, numberString);
        }

        void speechRecognizer_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
//currentTime = speechRecognizer == null ? 0 : (((SpeechRecognizer)sender).AudioPosition.Ticks / TimeSpan.TicksPerMillisecond);
            //int number = -1;
            Trace.TraceInformation("SpeechRecognitionRejected: " + e.Result.Text);
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, SemanticValue> kv in e.Result.Semantics)
            {
                if (kv.Key == "number")
                {
                    int v = (int)(kv.Value.Value);
                    if (Settings.MatchHypothesis)
                    {
                        speechHypotheses.Add(v);
                    }
                    if (sb.Length > 0) { sb.Append(","); }
                    sb.Append(v);
//                    numberList.AddDiscarded(v, speechStartTime);
                    //Trace.TraceInformation("NUMBER: " + v);
                }
            }
            string numberString = sb.ToString();
            //Trace.TraceInformation("NUMBERS: " + numberString);
            SpeechStatus(Status.info, numberString);
            System.Collections.ObjectModel.ReadOnlyCollection<RecognizedPhrase> phrases = e.Result.Alternates;
            if (phrases.Count > 1)
            {
                foreach (RecognizedPhrase phrase in phrases)
                {
                    Trace.TraceInformation("ALTERNATES: " + phrase.Text + " (" + phrase.Confidence + ")");
                    if (phrase.Confidence > Settings.MatchHypothesisAlternatesConfidence)
                    {
                        foreach (KeyValuePair<string, SemanticValue> kv in phrase.Semantics)
                        {
                            if (kv.Key == "number")
                            {
                                int v = (int)(kv.Value.Value);
                                speechHypotheses.Add(v);
                            }
                        }
                    }
                }
            }
            ProcessMatch();
        }


        void speechRecognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
//currentTime = speechRecognizer == null ? 0 : (((SpeechRecognizer)sender).AudioPosition.Ticks / TimeSpan.TicksPerMillisecond);
            Trace.TraceInformation("SpeechRecognized: " + e.Result.Text);

            /*
            RecognitionResult result = eventArgs.Result;
            RecognizedAudio audio = result.GetAudioForWordRange(result.Words[3], result.Words[result.Words.Count - 1]);
            MemoryStream memoryStream = new MemoryStream();
            audio.WriteToAudioStream(memoryStream);
            string filename = System.IO.Path.GetTempPath() + "Sound" + (new Random()).Next().ToString() + ".wav";
            FileStream waveStream = new FileStream(filename, FileMode.Create);
            audio.WriteToWaveStream(waveStream);
            waveStream.Flush();
            waveStream.Close();
            */

            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, SemanticValue> kv in e.Result.Semantics)
            {
                if (kv.Key == "number")
                {
                    int v = (int)(kv.Value.Value);
                    speechRecognitions.Add(v);
                    if (sb.Length > 0) { sb.Append(","); }
                    sb.Append(v);
//                    numberList.AddInput(v, speechStartTime);
                    //Trace.TraceInformation("NUMBER: " + v);
                }
            }
            string numberString = sb.ToString();
            //Trace.TraceInformation("NUMBERS: " + numberString);
            SpeechStatus(Status.info, numberString);
            System.Collections.ObjectModel.ReadOnlyCollection<RecognizedPhrase> phrases = e.Result.Alternates;
            if (phrases.Count > 1)
            {
                foreach (RecognizedPhrase phrase in phrases)
                {
                    Trace.TraceInformation("ALTERNATES: " + phrase.Text + " (" + phrase.Confidence + ")");
                    if (phrase.Confidence > Settings.MatchRecognitionAlternatesConfidence)
                    {
                        foreach (KeyValuePair<string, SemanticValue> kv in phrase.Semantics)
                        {
                            if (kv.Key == "number")
                            {
                                int v = (int)(kv.Value.Value);
                                //speechHypotheses.Add(v);
                                speechRecognitions.Add(v);
                            }
                        }
                    }
                }
            }
            SpeechStatus(Status.info, numberString);
            ProcessMatch();
        }

        void speechRecognizer_AudioSignalProblemOccurred(object sender, AudioSignalProblemOccurredEventArgs e)
        {
//currentTime = speechRecognizer == null ? 0 : (((SpeechRecognizer)sender).AudioPosition.Ticks / TimeSpan.TicksPerMillisecond);
            Trace.TraceInformation("AudioSignalProblem: " + e.AudioSignalProblem.ToString());
            SpeechStatus(Status.warning, "Problem: " + e.AudioSignalProblem.ToString());
        }

        void SpeechStatus(Status status, string text)
        {
            this.status = status;
            StatusText = text;
        }


        private bool hasBeenPlaying = false;

        public void Update()
        {
            long now2 = Now2;
            if (!Play) { hasBeenPlaying = false; lastUpdated = 0; }
            else { if (lastUpdated == 0) { lastUpdated = now2; } }

            if (Play && Settings.Interval <= 0) { Play = false; }

            if (Play && (now2 < lastUpdated || now2 >= lastUpdated + Settings.Interval))
            {
                long advance = (now2 - (lastUpdated + Settings.Interval));
                if (advance > Settings.Interval) { advance = 0; }
                lastUpdated = now2 - advance;

                // If generating and list empty or at end -- generate new item
                if (Generate && (bindingSource.Count <= 0 || bindingSource.Position + 1 >= bindingSource.Count))
                {
                    int symbol;
                    bool recent;
                    do
                    {
                        symbol = random.Next(INDEX_FIRST, INDEX_LAST);
                        // Prevent duplicates in close sequence
                        recent = false;
                        for (int i = 0; i < 4; i++)
                        {
                            if (bindingSource.Position - i >= 0)
                            {
                                recent |= session.Entries[bindingSource.Position - i].SpeakSymbol == symbol;
                            }
                        }
                    } while (recent);
//long now = currentTime = speechRecognizer == null ? 0 : (speechRecognizer.AudioPosition.Ticks / TimeSpan.TicksPerMillisecond);
long now = now2;
                    Entry entry = new Entry(now, symbol, 0, -1);
                    session.Entries.Add(entry);
                }
                //bindingSource.MoveLast();

                bool okToPlay = true;
                if (hasBeenPlaying)
                {
                    // Move to next item
                    if (!Generate && bindingSource.Position + 1 >= bindingSource.Count)
                    {
                        okToPlay = false;
                        //Play = false;
                        //bindingSource.MoveFirst();
                        //bindingSource.Position = -1;
                        //FireStatusChanged();
                    }
                    else
                    {
                        bindingSource.MoveNext();
                    }
                }
                hasBeenPlaying = true;

                if (Play && okToPlay)
                {
                    // If nothing selected, select first
                    if (bindingSource.Position < 0)
                    {
                        bindingSource.MoveFirst();
                    }

//Trace.WriteLine(" --- (" + bindingSource.Position + "," + bindingSource.Count + ")");

                    // Speak current item
                    if (Speak && bindingSource.Position >= 0)
                    {
//long now = currentTime = speechRecognizer == null ? 0 : (speechRecognizer.AudioPosition.Ticks / TimeSpan.TicksPerMillisecond);
long now = now2;

                        session.Entries[bindingSource.Position].SpeakTimestamp = now;
                        session.Entries[bindingSource.Position].ReceiveTimestamp = 0;
                        session.Entries[bindingSource.Position].SimulateTimestamp = 0;
                        session.Entries[bindingSource.Position].ReceiveSymbol = -1;
                        int symbol = session.Entries[bindingSource.Position].SpeakSymbol;
                        speechSynthesizer.SpeakAsync(NUMBERS[symbol].ToString());

                        SetLikely();
                    }
                }

            }
            else if (Simulate && Settings.SimulateBack >= 0)
            {
                int index = (int)(bindingSource.Position + ((float)(now2 - lastUpdated) / Settings.Interval) - Settings.SimulateBack);
                if (index >= 0 && session.Entries.Count > 0 && session.Entries[index].SimulateTimestamp == 0)
                {
                    session.Entries[index].SimulateTimestamp = now2;
                    int symbol = session.Entries[index].SpeakSymbol;
                    speechSynthesizerResponse.SpeakAsync(NUMBERS[symbol].ToString());
                }
            }


SetLikely();


bool failed = false;
int fx = 0, fy = 0;
if (MainForm.Settings.MatchFractionDenominator > 0 && MainForm.Settings.MatchFractionNumerator > 0 && bindingSource.Position >= MainForm.Settings.MatchFractionDenominator + 1)
{
    int num = 0;
    for (int i = 0; i < MainForm.Settings.MatchFractionDenominator; i++)
    {
        Entry entry = session.Entries[bindingSource.Position - i - 1];
        bool pass = entry.Pass;
        if (pass) { fx++;  num++; }
        fy++;
    }

    failed = (num < MainForm.Settings.MatchFractionNumerator);
}
CurrentlyFailed = failed;
FractionX = fx;
FractionY = fy;

            /*
            long now = NumberList.GetTimeNow;

            // Add next number
            NumberList.NumberItem lastItem = numberList.LastItem;
            if (lastItem == null || now > (lastItem.Time + numberList.Interval))
            {
                if (checkBoxRun.Checked)
                {
                    int n;
                    do
                    {
                        n = random.Next(1, NUMBERS.Length);
                    } while (lastItem != null && n == lastItem.Number);
                    numberList.AddPreOutput(n);
                }
            }

            // Speak next number
            NumberList.NumberItem firstPreOutput = numberList.FirstPreOutput;
            if (firstPreOutput != null && now > (firstPreOutput.Time + numberList.OutputTime))
            {
                if (checkBoxRun.Checked && checkBoxOutput.Checked)
                {
                    speechSynthesizer.SpeakAsync(firstPreOutput.Number.ToString());
                }
                firstPreOutput.Type = NumberList.NumberItem.ItemType.OutputPending;
                NumberList.NumberItem[] notYetMatched = numberList.NotYetMatched;
                SetLikely(notYetMatched);
                if (notYetMatched.Length > 0)
                {
                    speechSynthesizerResponse.SpeakAsync(notYetMatched[0].Number.ToString());
                }
            }

            // Update visual
            numberList.Time = now;
            numberList.Invalidate();

            //UpdateEnabled();
            */
FireStatusChanged();
        }

        private void ProcessMatch()
        {
            StringBuilder sb = new StringBuilder();
            foreach (int i in speechRecognitions) { sb.Append(i).Append("; "); }
            sb.Append(" | ");
            foreach (int i in speechHypotheses) { sb.Append(i).Append("; "); }
            string debugString = sb.ToString();
            Trace.TraceInformation("MATCHING: " + debugString);

            long now = Now;
            //long elapsed = now - speechStartTime;
            bool placed = false;

            // Find suitable unmatched point for start of search
            int startIndex = -1;
            for (int i = 0; i < SEARCH_LENGTH; i++)
            {
                if (bindingSource.Position - i >= 0)
                {
                    Entry entry = session.Entries[bindingSource.Position - i];
                    long elapsed = now - entry.SpeakTimestamp;
                    bool matched = entry.ReceiveSymbol == entry.SpeakSymbol;
                    if (elapsed > Settings.MatchTimeout) { break; }
                    if (matched && speechRecognitions.Contains(entry.SpeakSymbol))
                    {
                        startIndex = bindingSource.Position - i;    // allow overwrite prematurely matched
                        break;
                    }
                    if (matched) { break; }
                    startIndex = bindingSource.Position - i;
                }
            }

            // If we have a point, try to match this value
            if (startIndex >= 0)
            {
                for (int i = startIndex; i <= bindingSource.Position; i++)
                {
                    Entry entry = session.Entries[i];
                    int value = entry.SpeakSymbol;
                    if (speechRecognitions.Contains(value) || speechHypotheses.Contains(value))
                    {
                        // Special case overwriting same match, shift overwritten data up one if also unmatched
                        if (entry.ReceiveSymbol == value && i >= 1)
                        {
                            if (session.Entries[i - 1].SpeakSymbol != session.Entries[i - 1].ReceiveSymbol)
                            {
                                session.Entries[i - 1].ReceiveSymbol = entry.ReceiveSymbol;
                                session.Entries[i - 1].ReceiveTimestamp = entry.ReceiveTimestamp;
                            }
                        }

                        entry.ReceiveSymbol = value;
                        entry.ReceiveTimestamp = now;
                        placed = true;
                        break;
                    }
                }
            }

            // If failed to find a match, find next suitable unfilled spot
            if (!placed)
            {
                startIndex = -1;
                for (int i = 0; i < SEARCH_LENGTH; i++)
                {
                    if (bindingSource.Position - i >= 0)
                    {
                        Entry entry = session.Entries[bindingSource.Position - i];
                        long elapsed = now - entry.SpeakTimestamp;
                        bool filled = (entry.ReceiveSymbol >= 0);
                        if (filled || elapsed > Settings.MatchTimeout) { break; }
                        startIndex = bindingSource.Position - i;
                    }
                }

                // If we have a point, try to fill-in this value
                if (startIndex >= 0)
                {
                    for (int i = startIndex; i <= bindingSource.Position; /*i++*/)  // Will always break out, can't loop
                    {
                        Entry entry = session.Entries[i];
                        int value = entry.SpeakSymbol;
                        if (speechRecognitions.Contains(value) || speechHypotheses.Contains(value))
                        {
                            entry.ReceiveTimestamp = now;
                            entry.ReceiveSymbol = value;
                            placed = true;
                            break;
                        }
                        else
                        {
                            int fillin = -1;
                            if (speechRecognitions.Count > 0) { fillin = speechRecognitions[0]; }
                            if (fillin == -1 && speechHypotheses.Count > 0) { fillin = speechHypotheses[0]; }
                            if (fillin >= 0)
                            {
                                entry.ReceiveTimestamp = now;
                                entry.ReceiveSymbol = fillin;
                                placed = true;
                            }
                            break;
                        }
                    }
                }
            }

            speechStartTime = 0;
            speechHypotheses.Clear();
            speechRecognitions.Clear();

            SetLikely();
        }


        private bool priorityAvailable = true;          // Assume can set priority (will clear if causes exception)
        private bool weightAvailable = true;            // Assume can set weight (will clear if causes exception)

        private void SetLikely()
        {
            long now = Now;
            List<int> likely = new List<int>();

            for (int i = 0; i < SEARCH_LENGTH; i++)
            {
                if (bindingSource.Position - i >= 0)
                {
                    Entry entry = session.Entries[bindingSource.Position - i];
                    long elapsed = now - entry.SpeakTimestamp;
                    bool matched = entry.ReceiveSymbol == entry.SpeakSymbol;
                    bool possibleEarlyMatched = matched && (bindingSource.Position - i - 1 >= 0 && session.Entries[bindingSource.Position - i - 1].SpeakSymbol != session.Entries[bindingSource.Position - i - 1].ReceiveSymbol);
                    if (elapsed > Settings.MatchTimeout || (matched && !possibleEarlyMatched)) { break; }
                    if (elapsed > Settings.ExpectedAfter) { likely.Add(entry.SpeakSymbol); }
                    if (matched) { break; } // Placing here allows overwrite of premature match
                }
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < grammars.Length; i++)
            {
                bool found = likely.Contains(i);
                //sb.Append(found ? "" + i + " " : "_ ");
                sb.Append(found ? "[" + i + "]" : " " + i + " ");
            }
            string newListenString = sb.ToString();

            bool recognizeAny = MainForm.Settings.RecognizeAny;
            int lowerPriority = MainForm.Settings.UnexpectedPriority; // 1
            float lowerWeight = MainForm.Settings.UnexpectedWeight; // 0.3

            if (newListenString != ListenString)
            {
                ListenString = newListenString;
                //Trace.TraceInformation("LISTENING: " + ListenString);
                for (int i = 0; i < grammars.Length; i++)
                {
                    bool found = likely.Contains(i);

                    bool enabled = (recognizeAny) ? (i >= INDEX_FIRST && i <= INDEX_LAST) : found;
                    int priority = found ? 100 : lowerPriority;
                    float weight = found ? 1.0f : lowerWeight;

                    if (grammars[i].Enabled != enabled) { grammars[i].Enabled = enabled ? true : false; }

                    if (priorityAvailable)
                    {
                        try
                        {
                            if (grammars[i].Priority != priority) { grammars[i].Priority = priority; }
                        }
                        catch { priorityAvailable = false; }
                    }

                    if (weightAvailable)
                    {
                        try
                        {
                            if (grammars[i].Weight != weight) { grammars[i].Weight = weight; }
                        }
                        catch { weightAvailable = false; }
                    }

                }
            }

        }

        // Called before shutdown
        public void Dispose()
        {
            this.Play = false;
            this.Enable = false;
            this.Speak = false;

            speechRecognizer.Enabled = false;
            //speechRecognizer.RequestRecognizerUpdate();
            speechRecognizer.Dispose();

            speechSynthesizer.Pause();
            speechSynthesizer.Dispose();

            speechSynthesizerResponse.Pause();
            speechSynthesizerResponse.Dispose();
        }


        #region Events
        public class SpeechEventArgs : EventArgs
        {
            public Speech Speech { get; private set; }
            /*
            public string Status { get; private set; }
            public int Volume { get; private set; }
            public bool End { get; private set; }
            public int Index { get; private set; }
            */

            private SpeechEventArgs(Speech speech) : base()
            {
                this.Speech = speech;
                /*
                this.Status = null;
                this.Volume = -1;
                this.End = false;
                this.Index = -1;
                */
            }

            public static SpeechEventArgs Create(Speech speech)
            {
                SpeechEventArgs e = new SpeechEventArgs(speech);
                return e;
            }

            /*
            public static SpeechEventArgs CreateForStatus(Speech speech, string status)
            {
                SpeechEventArgs e = new SpeechEventArgs(speech);
                e.Status = status;
                return e;
            }

            public static SpeechEventArgs CreateForVolume(Speech speech, int volume)
            {
                SpeechEventArgs e = new SpeechEventArgs(speech);
                e.Volume = volume;
                return e;
            }

            public static SpeechEventArgs CreateForEnd(Speech speech)
            {
                SpeechEventArgs e = new SpeechEventArgs(speech);
                e.End = true;
                return e;
            }

            public static SpeechEventArgs CreateForIndex(Speech speech, int index)
            {
                SpeechEventArgs e = new SpeechEventArgs(speech);
                e.Index = index;
                return e;
            }
            */
        }

        // The delegate used to call the event handlers
        public delegate void SpeechEventDelegate(object sender, SpeechEventArgs e);

        // Changed event
        public event SpeechEventDelegate SpeechStatusChanged;


        private void FireStatusChanged()
        {
            if (SpeechStatusChanged != null)
            {
                SpeechStatusChanged.Invoke(this, SpeechEventArgs.Create(this));
            }
        }

        /*
            if (this.InvokeRequired)
            {
                this.Invoke(new SpeechEventDelegate(Speech_SpeechChanged), new object[] { sender, e });
                return;
            }
        */

        #endregion


    }
}
