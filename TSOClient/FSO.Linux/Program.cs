using System;
using System.IO;
using FSO.Client;
using FSO.Common.Rendering.Framework.IO;
using FSO.Common;

namespace FSO.Linux
{
    public static class Program 
    {
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            FSOProgram.ShowDialog = ShowDialog;
            
            if (new FSOProgram().InitWithArguments(args))
                new GameStartProxy().Start(false);
        }

        public static void ShowDialog(string text)
        {
            Console.WriteLine(text);
            Environment.Exit(0);
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject;

            if (exception is OutOfMemoryException)
            {
                Console.WriteLine(e.ExceptionObject.ToString(), "Out of Memory! FreeSO needs to close.");
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine(e.ExceptionObject.ToString(), "A fatal error occured! Screenshot this dialog and post it on Discord.");
                Environment.Exit(0);
            }
        }
    }
}