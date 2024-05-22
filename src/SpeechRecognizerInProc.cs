using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech.Recognition;

namespace nback
{
    public class SpeechRecognizerInProc : IDisposable
    {
        private SpeechRecognitionEngine rec;

        public SpeechRecognizerInProc()
        {
		    rec = new SpeechRecognitionEngine();
		    rec.SetInputToDefaultAudioDevice();

            rec.AudioLevelUpdated += new EventHandler<AudioLevelUpdatedEventArgs>(rec_AudioLevelUpdated);
            rec.AudioSignalProblemOccurred += new EventHandler<AudioSignalProblemOccurredEventArgs>(rec_AudioSignalProblemOccurred);
            rec.SpeechDetected += new EventHandler<SpeechDetectedEventArgs>(rec_SpeechDetected);
            rec.SpeechHypothesized += new EventHandler<SpeechHypothesizedEventArgs>(rec_SpeechHypothesized);
            rec.SpeechRecognitionRejected += new EventHandler<SpeechRecognitionRejectedEventArgs>(rec_SpeechRecognitionRejected);
            rec.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(rec_SpeechRecognized);
        }

        void rec_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (SpeechRecognized != null) { SpeechRecognized.Invoke(sender, e); }
        }

        void rec_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            if (SpeechRecognitionRejected != null) { SpeechRecognitionRejected.Invoke(sender, e); }
        }

        void rec_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            if (SpeechHypothesized != null) { SpeechHypothesized.Invoke(sender, e); }
        }

        void rec_SpeechDetected(object sender, SpeechDetectedEventArgs e)
        {
            if (SpeechDetected != null) { SpeechDetected.Invoke(sender, e); }
        }

        void rec_AudioSignalProblemOccurred(object sender, AudioSignalProblemOccurredEventArgs e)
        {
            if (AudioSignalProblemOccurred != null) { AudioSignalProblemOccurred.Invoke(sender, e); }
        }

        void rec_AudioLevelUpdated(object sender, AudioLevelUpdatedEventArgs e)
        {
            if (AudioLevelUpdated != null) { AudioLevelUpdated.Invoke(sender, e); }
        }


        //public SpeechAudioFormatInfo AudioFormat { get; }
        //public int AudioLevel { get; }
        //public TimeSpan AudioPosition { get; }
        //public AudioState AudioState { get; }

        bool enabled = false;       // TODO: Do this property properly
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                if (!enabled && value)
                {
                    rec.RecognizeAsync(RecognizeMode.Multiple);
                    enabled = true;
                }
                else if (enabled && !value)
                {
                    rec.RecognizeAsyncStop();
                    enabled = false;
                }
            }
        }

        //public ReadOnlyCollection<Grammar> Grammars { get; }

        public int MaxAlternates
        {
            get { return rec.MaxAlternates; }
            set { rec.MaxAlternates = value; }
        }

        //public bool PauseRecognizerOnRecognition { get; set; }
        //public TimeSpan RecognizerAudioPosition { get; }
        //public RecognizerInfo RecognizerInfo { get; }

        public RecognizerState State
        {
            get
            {
                // TODO: Fix this properly
                return enabled ? RecognizerState.Listening : RecognizerState.Stopped;
            }
            set
            {
                if (StateChanged != null)
                {
                    StateChangedEventArgs stateChangedEventArgs = null;
                    //stateChangedEventArgs = new StateChangedEventArgs();
                    //stateChangedEventArgs.RecognizerState = enabled ? RecognizerState.Listening : RecognizerState.Stopped;
                    StateChanged.Invoke(this, stateChangedEventArgs);
                }
            }
        }

        public event EventHandler<AudioLevelUpdatedEventArgs> AudioLevelUpdated;
        public event EventHandler<AudioSignalProblemOccurredEventArgs> AudioSignalProblemOccurred;
        //public event EventHandler<AudioStateChangedEventArgs> AudioStateChanged;
        //public event EventHandler<EmulateRecognizeCompletedEventArgs> EmulateRecognizeCompleted;
        //public event EventHandler<LoadGrammarCompletedEventArgs> LoadGrammarCompleted;
        //public event EventHandler<RecognizerUpdateReachedEventArgs> RecognizerUpdateReached;
        public event EventHandler<SpeechDetectedEventArgs> SpeechDetected;
        public event EventHandler<SpeechHypothesizedEventArgs> SpeechHypothesized;
        public event EventHandler<SpeechRecognitionRejectedEventArgs> SpeechRecognitionRejected;
        public event EventHandler<SpeechRecognizedEventArgs> SpeechRecognized;
        public event EventHandler<StateChangedEventArgs> StateChanged;


        public void Dispose()
        {
            ;
        }

        protected virtual void Dispose(bool disposing)
        {
            ;
        }

        //public RecognitionResult EmulateRecognize(string inputText);
        //public RecognitionResult EmulateRecognize(RecognizedWordUnit[] wordUnits, CompareOptions compareOptions);
        //public RecognitionResult EmulateRecognize(string inputText, CompareOptions compareOptions);
        //public void EmulateRecognizeAsync(string inputText);
        //public void EmulateRecognizeAsync(RecognizedWordUnit[] wordUnits, CompareOptions compareOptions);
        //public void EmulateRecognizeAsync(string inputText, CompareOptions compareOptions);

        public void LoadGrammar(Grammar grammar)
        {
            rec.LoadGrammar(grammar);
        }

        //public void LoadGrammarAsync(Grammar grammar);
        //public void RequestRecognizerUpdate();
        //public void RequestRecognizerUpdate(object userToken);
        //public void RequestRecognizerUpdate(object userToken, TimeSpan audioPositionAheadToRaiseUpdate);

        public void UnloadAllGrammars() { rec.UnloadAllGrammars(); }

        //public void UnloadGrammar(Grammar grammar);


        public RecognizerInfo RecognizerInfo
        {
            get { return rec.RecognizerInfo; }
        }




    }
}
