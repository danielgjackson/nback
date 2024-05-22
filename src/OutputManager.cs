using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace nback
{
    public class TextboxTraceListener : TraceListener
    {

        // TextboxTraceListener textBoxTracer = new TextboxTraceListener(textBoxLog);
        // textBoxTracer.TraceOutputOptions = TraceOptions.None;
        // Trace.Listeners.Add(textBoxTracer);
        // Trace.WriteLine("Begin logging");

        private delegate void OutputDelegate(string message);

        private TextBox textBox;
        private int maxLength = 30000;

        public TextboxTraceListener(TextBox textBox)
        {
            this.textBox = textBox;
            this.TraceOutputOptions = TraceOptions.None;
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType type, int id)
        {
            return;
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType type, int id, string message)
        {
            string prefix = "";
            if (type != TraceEventType.Information) { prefix = type.ToString().ToUpper() + ": "; }
            WriteLine(prefix + message);
        }

        public override void Write(string message)
        {
            if (textBox.InvokeRequired)
            {
                textBox.BeginInvoke(new OutputDelegate(Write), message);
            }
            else
            {
                bool caretAtEnd = (textBox.SelectionStart == textBox.Text.Length && textBox.SelectionLength == 0);

                if (textBox.Text.Length + message.Length > maxLength)
                {
                    int remove = textBox.Text.Length + message.Length - maxLength;
                    if (remove < 0) { remove = 0; }
                    if (remove > textBox.Text.Length) { remove = textBox.Text.Length; }
                    textBox.Text = textBox.Text.Remove(0, remove);
                }
                textBox.AppendText(message);

                if (caretAtEnd)
                {
                    textBox.Select(textBox.Text.Length, 0);
                    textBox.ScrollToCaret();
                }
            }
        }

        public override void WriteLine(string message) { Write(message + Environment.NewLine); }


        //public override void Write(object o) { Write(o.ToString()); }
        //public override void Write(object o, string category) { Write(category + " -- " + o.ToString()); }
        //public override void Write(string message, string category) { Write(category + " -- " + message); }
        //public override void WriteLine(object o) { WriteLine(o.ToString()); }
        //public override void WriteLine(object o, string category) { WriteLine(category + " -- " + o.ToString()); }
        //public override void WriteLine(string message, string category) { WriteLine(category + " -- " + message); }

    }

    /*
    class OutputManager
    {
        public static void PrintLine(string line)
        {
            if (line != null)
            {
                Print(line + "\r\n");
            }
        }

        public static void Print(string text)
        {
            MainForm topLevelForm = Program.TopLevelForm;
            if (text != null)
            {
                Console.Write(text);
                topLevelForm.Invoke(new MainForm.OutputDelegate(topLevelForm.OutputText), text);
            }
        }
    }
    */


/*
    // LogRequestResponse.Log(request, response);
    static class LogRequestResponse
    {
        static void Log(BaseRequestType request, BaseResponseMessageType response)
        {
            using (StreamWriter requestWriter = new StreamWriter(request.GetType().Name + ".xml"))
            {
                XmlSerializer requestSerializer = new XmlSerializer(request.GetType());
                requestSerializer.Serialize(requestWriter, request);
            }
            using (StreamWriter responseWriter = new StreamWriter(response.GetType().Name + ".xml"))
            {
                XmlSerializer responseSerializer = new XmlSerializer(response.GetType());
                responseSerializer.Serialize(responseWriter, response);
            }
        }
    }
*/


}

