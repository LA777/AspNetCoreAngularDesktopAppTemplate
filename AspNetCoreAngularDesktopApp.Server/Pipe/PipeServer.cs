using System.IO.Pipes;

namespace AspNetCoreAngularDesktopApp.Server.Pipe
{
    public interface IPipeServer
    {
        Task<string> RunAndGetTextReplyAsync(CancellationToken cancellationToken = default);
    }

    public class PipeServer : IPipeServer
    {
        private static readonly int ThreadsCount = 1;
        private readonly ILogger<PipeServer> _logger;
        private const string PipeName = "Pipe-Name-MyApp";
        private const string PipeServerVerificationKey = "Pipe-Key-42";

        public PipeServer(ILogger<PipeServer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> RunAndGetTextReplyAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Pipe Server starting...");
                await using var pipeServerStream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, ThreadsCount);
                await pipeServerStream.WaitForConnectionAsync(cancellationToken);
                _logger.LogInformation("Pipe Server waiting for connections...");

                var streamString = new PipeStreamWrapper(pipeServerStream);

                _logger.LogInformation("Pipe Server writing verification key...");
                await streamString.WriteStringAsync(PipeServerVerificationKey, cancellationToken);

                var textReply = await streamString.ReadStringAsync(cancellationToken);
                _logger.LogInformation($"Pipe Server got request: '{textReply}'.");

                return textReply;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Pipe Server error.");
            }

            return string.Empty;
        }
    }
}
