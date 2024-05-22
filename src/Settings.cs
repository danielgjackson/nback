using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO.Ports;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace nback
{
    // CategoryAttribute, DescriptionAttribute, BrowsableAttribute, ReadOnlyAttribute, DefaultValueAttribute, DefaultPropertyAttribute, Browsable
    // [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(System.Drawing.Design.UITypeEditor)), Description("Path to file"), Category("File")]


    [Serializable]
    [XmlRootAttribute("Settings", Namespace = "", IsNullable = false)]
    //[DefaultPropertyAttribute("???")]
    public class Settings : ICloneable
    {
        private const int CURRENT_FILE_VERSION = 1;
        private const int DEFAULT_INTERVAL = 1500;
        private const float DEFAULT_SIMULATE_BACK = 1.0f;
        private const float DEFAULT_MINIMUM_NBACK = 0.8f;
        private const float DEFAULT_MAXIMUM_NBACK = 2.2f;
        private const float DEFAULT_MATCH_LIMIT_NBACK = 3.3f;
        private const float DEFAULT_DELAY = 850;
        private const int DEFAULT_MATCH_FRACTION_NUMERATOR = 2;
        private const int DEFAULT_MATCH_FRACTION_DENOMINATOR = 4;
        private const bool DEFAULT_RECOGNIZE_ANY = true;
        private const int DEFAULT_UNEXPECTED_PRIORITY = 1;
        private const float DEFAULT_UNEXPECTED_WEIGHT = 0.3f;
        private const int DEFAULT_EXPECTED_AFTER = 500;
        private const bool DEFAULT_MATCH_HYPOTHESIS = false;
        private const float DEFAULT_MATCH_HYPOTHESIS_ALTERNATES_CONFIDENCE = 1.0f;
        private const float DEFAULT_MATCH_RECOGNITION_ALTERNATES_CONFIDENCE = 1.0f;


        public static string GetFullPath(string filePath)
        {
            string applicationStartupPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);  // Could use Application.StartupPath
            string absolutePath = Path.Combine(applicationStartupPath, filePath);  // Combine() deals with Path.IsPathRooted()
            return absolutePath;
        }

        public Settings()
        {
            fileVersion = -1;
        }

        public Object Clone()
        {
            return MemberwiseClone();
        }

        public static void Serialize(Settings settings, StreamWriter sw)
        {
            XmlTextWriter writer = new XmlTextWriter(sw);
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            serializer.Serialize(sw, settings);
            writer.Close();
        }

        public static Settings DeSerialize(TextReader reader)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            Settings settings = (Settings)serializer.Deserialize(reader);
            reader.Close();
            return settings;
        }

        private int fileVersion;
        [Browsable(false)]
        public int FileVersion
        {
            get { return CURRENT_FILE_VERSION; }
            set { fileVersion = value; }
        }


        // Interval
        private int interval = DEFAULT_INTERVAL;
        [CategoryAttribute("1. Output"), DisplayName("Inter-stimulus interval (ms)"), DescriptionAttribute("Speak Interval (milliseconds)"), DefaultValueAttribute(DEFAULT_INTERVAL)]
        public int Interval
        {
            get { return interval; }
            set { interval = value; }
        }

        // Minimum likely
        private int expectedAfter = DEFAULT_EXPECTED_AFTER; 
        [CategoryAttribute("2. Speech Recognition"), DisplayName("Numbers become expected after (ms)"), DescriptionAttribute("Minimum time after starting to speak that a grammar becomes likely"), DefaultValueAttribute(DEFAULT_EXPECTED_AFTER)]
        public int ExpectedAfter
        {
            get { return expectedAfter; }
            set { expectedAfter = value; }
        }

        // Grammar weight
        private float unexpectedWeight = DEFAULT_UNEXPECTED_WEIGHT;
        [CategoryAttribute("2. Speech Recognition"), DisplayName("Unexpected numbers have lower weighting (0-1)"), DescriptionAttribute("[Unsupported in MS SAPI] Lower weighting for unexpected number recognition grammars (0-1), so recognition of unexpected numbers is less likely."), DefaultValueAttribute(DEFAULT_UNEXPECTED_WEIGHT)]
        public float UnexpectedWeight
        {
            get { return unexpectedWeight; }
            set { unexpectedWeight = value; }
        }

        // Grammar priority
        private int unexpectedPriority = DEFAULT_UNEXPECTED_PRIORITY;
        [CategoryAttribute("2. Speech Recognition"), DisplayName("Unexpected numbers have lower priority (0-100)"), DescriptionAttribute("[Unsupported in MS SAPI] Lower priority for unexpected number recognition grammars (0-100), so unexpected numbers are more likely to lose in a tie."), DefaultValueAttribute(DEFAULT_UNEXPECTED_PRIORITY)]
        public int UnexpectedPriority
        {
            get { return unexpectedPriority; }
            set { unexpectedPriority = value; }
        }

        // Recognize any
        private bool recognizeAny = DEFAULT_RECOGNIZE_ANY;
        [Browsable(false)] //[CategoryAttribute("2. Speech Recognition"), DisplayName("Recognize numbers even when unexpected"), DescriptionAttribute("Allow recognition of any numbers even if not expected, otherwise will disable recognition grammars for unexpected numbers"), DefaultValueAttribute(DEFAULT_RECOGNIZE_ANY)]
        public bool RecognizeAny
        {
            get { return recognizeAny; }
            set { recognizeAny = value; }
        }

        // Grammar disable (= NOT of RecognizeAny)
        [XmlIgnore, CategoryAttribute("2. Speech Recognition"), DisplayName("Unexpected numbers are ignored"), DescriptionAttribute("Completely disable recognition grammars for unexpected numbers (they will not be recognized)."), DefaultValueAttribute(!DEFAULT_RECOGNIZE_ANY)]
        public bool GrammarDisable
        {
            get { return !RecognizeAny; }
            set { RecognizeAny = !value; }
        }



        // Roundtrip estimate
        private float delay = DEFAULT_DELAY;
        [CategoryAttribute("3. Matching"), DisplayName("Estimated recognition delay (ms)"), DescriptionAttribute("Estimated sum of output prompt delay (generating sound and user hearing token) and response delay (user speaking response and recognizer deciding on a final speech recognition hypothesis)."), DefaultValueAttribute(DEFAULT_DELAY)]
        public float Delay
        {
            get { return delay; }
            set { delay = value; }
        }

        // Match timeout
        private float matchLimit = DEFAULT_MATCH_LIMIT_NBACK;
        [CategoryAttribute("3. Matching"), DisplayName("Maximum age to match against (n)"), DescriptionAttribute("Upper limit for matching a token (whether within correct time or not)"), DefaultValueAttribute(DEFAULT_MATCH_LIMIT_NBACK)]
        public float MatchLimit
        {
            get { return matchLimit; }
            set { matchLimit = value; }
        }

        // Min. Match
        private float minimumNBack = DEFAULT_MINIMUM_NBACK;
        [CategoryAttribute("3. Matching"), DisplayName("Minimum n-back time (n)"), DescriptionAttribute("Minimum correct n-back match."), DefaultValueAttribute(DEFAULT_MINIMUM_NBACK)]
        public float MinimumNBack
        {
            get { return minimumNBack; }
            set { minimumNBack = value; }
        }

        // Max. Match
        private float maximumNBack = DEFAULT_MAXIMUM_NBACK;
        [CategoryAttribute("3. Matching"), DisplayName("Maximum n-back time (n)"), DescriptionAttribute("Maximum correct n-nack match."), DefaultValueAttribute(DEFAULT_MAXIMUM_NBACK)]
        public float MaximumNBack
        {
            get { return maximumNBack; }
            set { maximumNBack = value; }
        }


        // Match Fraction Numerator
        private int matchFractionNumerator = DEFAULT_MATCH_FRACTION_NUMERATOR;
        [CategoryAttribute("4. Success Metric"), DisplayName("'Pass' when correctly matched X="), DescriptionAttribute("'X out of Y' Matching, 'X' value"), DefaultValueAttribute(DEFAULT_MATCH_FRACTION_NUMERATOR)]
        public int MatchFractionNumerator
        {
            get { return matchFractionNumerator; }
            set { matchFractionNumerator = value; }
        }

        // Match Fraction Denominator
        private int matchFractionDenominator = DEFAULT_MATCH_FRACTION_DENOMINATOR;
        [CategoryAttribute("4. Success Metric"), DisplayName("...out of the previous Y="), DescriptionAttribute("'X out of Y' Matching, 'Y' value"), DefaultValueAttribute(DEFAULT_MATCH_FRACTION_DENOMINATOR)]
        public int MatchFractionDenominator
        {
            get { return matchFractionDenominator; }
            set { matchFractionDenominator = value; }
        }






        // Match Timeout
        [Browsable(false)]
        public float MatchTimeout
        {
            get { return matchLimit * interval; }
        }

        // Simulate Back
        private float simulateBack = DEFAULT_SIMULATE_BACK;
        [Browsable(false), CategoryAttribute("1. Output"), DisplayName("Simulate output n-back (n)"), DescriptionAttribute("Simulate n-back number"), DefaultValueAttribute(DEFAULT_SIMULATE_BACK)]
        public float SimulateBack
        {
            get { return simulateBack; }
            set { simulateBack = value; }
        }



        // Hypothesis Alternates Confidence
        private bool matchHypothesis = DEFAULT_MATCH_HYPOTHESIS;
        [Browsable(false), CategoryAttribute("3. Matching"), DisplayName("Match Hypothesis"), DescriptionAttribute("Match using hypothesis in addition to recognized number."), DefaultValueAttribute(DEFAULT_MATCH_HYPOTHESIS)]
        public bool MatchHypothesis
        {
            get { return matchHypothesis; }
            set { matchHypothesis = value; }
        }

        // Hypothesis Alternates Confidence
        private float matchHypothesisAlternatesConfidence = DEFAULT_MATCH_HYPOTHESIS_ALTERNATES_CONFIDENCE;
        [Browsable(false), CategoryAttribute("3. Matching"), DisplayName("Match Hypothesis Alternates Confidence (0-1)"), DescriptionAttribute("Match using hypothesis alternates with specified minimum confidence."), DefaultValueAttribute(DEFAULT_MATCH_HYPOTHESIS_ALTERNATES_CONFIDENCE)]
        public float MatchHypothesisAlternatesConfidence
        {
            get { return matchHypothesisAlternatesConfidence; }
            set { matchHypothesisAlternatesConfidence = value; }
        }

        // Recognition Alternates Confidence
        private float matchRecognitionAlternatesConfidence = DEFAULT_MATCH_RECOGNITION_ALTERNATES_CONFIDENCE;
        [Browsable(false), CategoryAttribute("3. Matching"), DisplayName("Match Recognition Alternates Confidence (0-1)"), DescriptionAttribute("Match using recognition alternates with specified minimum confidence."), DefaultValueAttribute(DEFAULT_MATCH_RECOGNITION_ALTERNATES_CONFIDENCE)]
        public float MatchRecognitionAlternatesConfidence
        {
            get { return matchRecognitionAlternatesConfidence; }
            set { matchRecognitionAlternatesConfidence = value; }
        }



    }

}
