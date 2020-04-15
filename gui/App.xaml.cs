namespace GrunCS
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;
    using NDesk.Options;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            new MainWindow().ShowDialog();
        }
    }
}