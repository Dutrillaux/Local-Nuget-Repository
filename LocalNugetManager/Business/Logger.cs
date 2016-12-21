using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace LocalNugetManager.Business
{
    public static class Logger
    {
        public static void LogException(Exception ex)
        {
            Debug.WriteLine(ex);
#if DEBUG
            MessageBox.Show(ex + Environment.NewLine + ex.StackTrace);
#endif
        }

        public static void LogMessage(string message)
        {
            Debug.WriteLine(message);
#if DEBUG
            MessageBox.Show(message);
#endif
        }
    }
}
