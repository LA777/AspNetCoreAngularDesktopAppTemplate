using Hardcodet.Wpf.TaskbarNotification;
using System.IO.Pipes;
using System.Security.Principal;
using System.Windows;
using SystemTrayApp.Pipe;

namespace SystemTrayApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon? _notifyIcon;

        private const string PipeName = "Pipe-Name-MyApp";
        private const string PipeServerVerificationKey = "Pipe-Key-42";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            foreach (var argument in e.Args)
            {
                var isUrl = Uri.TryCreate(argument, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (!isUrl)
                {
                    continue;
                }

                NotifyIconViewModel.Url = argument;
                break;
            }

            _notifyIcon = (TaskbarIcon)FindResource("NotifyIcon") ?? throw new InvalidOperationException();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Task.Run(() =>
            {
                using var pipeClientStream = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
                pipeClientStream.Connect();
                var streamString = new PipeStreamWrapper(pipeClientStream);
                var inputText = streamString.ReadStringAsync(default).GetAwaiter().GetResult();

                if (inputText != PipeServerVerificationKey)
                {
                    return;
                }

                streamString.WriteStringAsync("command:exit", CancellationToken.None).GetAwaiter().GetResult();
            });

            _notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }
}
