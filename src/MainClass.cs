using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Threading;

namespace nback
{
    static class MainClass
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        static void Main(string[] args)
        {
            try
            {
                // Exception handling
                Application.ThreadException += Application_ThreadException;                         // UI thread exceptions
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);       // Force UI exceptions through our handler
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;     // non-UI thread exceptions

                // Run the application
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Defaults
                string filename = null;

                // Parse command-line options
                int positionalParameter = 0;
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith("-"))
                    {
                        string option = args[i].ToLower();
                        if (option == "--help")
                        {
                            MessageBox.Show(null, "Command-line options:\r\n\r\n" +
                                                  "<filename>\tFile to open\r\n" +
                                                  "[--help]\tShow help\r\n"
                                            , Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(null, "Ignoring command-line option: " + args[i], Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        if (positionalParameter == 0)
                        {
                            filename = args[i];
                        }
                        else
                        {
                            MessageBox.Show(null, "Ignoring command-line option: " + args[i], Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        positionalParameter++;
                    }
                }

                MainForm mainForm = new MainForm(filename);
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                string error =
                    "Sorry, a fatal application error occurred (exception in main function).\r\n\r\n" +
                    "Exception: " + ex.ToString() + "\r\n\r\n" +
                    "Stack trace: " + ex.StackTrace + "";
                MessageBox.Show(error, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }


        // Unhandled UI exceptions (can ignore and resume)
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs t)
        {
            DialogResult result = DialogResult.Abort;
            try
            {
                Exception ex = (Exception)t.Exception;
                string error =
                    "Sorry, an application error occurred (unhandled UI exception).\r\n\r\n" +
                    "Exception: " + ex.ToString() + "\r\n\r\n" +
                    "Stack trace: " + ex.StackTrace + "";
                result = MessageBox.Show(error, Application.ProductName, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
            }
            catch { ; }
            finally
            {
                if (result == DialogResult.Abort) { Application.Exit(); }
            }
        }

        // Unhandled non-UI exceptions (cannot prevent the application from terminating)
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;
                string error =
                    "Sorry, a fatal application error occurred (unhandled non-UI exception).\r\n\r\n" +
                    "Exception: " + ex.ToString() + "\r\n\r\n" +
                    "Stack trace: " + ex.StackTrace + "";
                MessageBox.Show(error, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { ; }
            finally
            {
                Environment.Exit(-1);       // Not Application.Exit, this will prevent the Windows error message
            }
        }



    }
}
