using System;
using System.Windows.Forms;

namespace CybersecurityChatBot
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // .NET 6+ WinForms bootstrap — replaces the two legacy calls
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
