using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace SystemTrayApp
{
    public class NotifyIconViewModel
    {
        public static string Url { get; set; } = "https://localhost:55000";

        public ICommand OpenBrowserCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => true,

                    CommandAction = () =>
                    {
                        Process.Start(new ProcessStartInfo(Url) { UseShellExecute = true });
                    }
                };
            }
        }

        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
            }
        }
    }
}
