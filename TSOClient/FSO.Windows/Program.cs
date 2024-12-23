using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using FSO.Common.Rendering.Framework.IO;
using FSO.Client;
using FSO.Client.UI.Panels;
using System.Windows.Forms;

namespace FSO.Windows
{
    public static class Program
    {
        public static bool UseDX = true;

        public static void Init()
        {
            ClipboardHandler.Default = new WinFormsClipboard();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            FSOProgram.ShowDialog = ShowDialog;
        }

        public static void Main(string[] args)
        {
            Init();
            if (new FSOProgram().InitWithArguments(args))
                new GameStartProxy().Start(UseDX);
        }

        public static void ShowDialog(string text)
        {
            MessageBox.Show(text);
            Environment.Exit(0);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject;

            if (exception is OutOfMemoryException)
            {
                MessageBox.Show(e.ExceptionObject.ToString(), "Out of Memory! FreeSO needs to close.");
                Environment.Exit(0);
            }
            else
            {
                MessageBox.Show(e.ExceptionObject.ToString(), "A fatal error occured! Screenshot this dialog and post it on Discord.");
                Environment.Exit(0);
            }
        }
    }
}
