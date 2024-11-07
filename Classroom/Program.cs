using Classroom.Services;
using NLog;
using System.Net;

namespace Classroom
{
    /// <summary>
    /// Main class for starting the classroom gRPC server.
    /// </summary>
    public class Program
    {
        // Logger instance for logging server activities
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            var self = new Program();
            self.Run(args);
        }

        /// <summary>
        /// Initializes logging and starts the gRPC server.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        private void Run(string[] args)
        {
            // Configure logging
            ConfigureLogging();

            // Log server startup event
            log.Info("Server is about to start");

            // Start the gRPC server
            StartServer(args);
        }

        /// <summary>
        /// Configures the logging subsystem.
        /// </summary>
        private void ConfigureLogging()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Configure console output for logging
            var console = new NLog.Targets.ConsoleTarget("console")
            {
                Layout = @"${date:format=HH\:mm\:ss}|${level}| ${message} ${exception}"
            };
            config.AddTarget(console);
            config.AddRuleForAllLevels(console);

            // Apply logging configuration
            LogManager.Configuration = config;
        }

        /// <summary>
        /// Sets up and starts the gRPC server.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        private void StartServer(string[] args)
        {
            // Create a new WebApplication builder
            var builder = WebApplication.CreateBuilder(args);

            // Add gRPC services to the DI container
            builder.Services.AddGrpc();

            // Register ClassroomServiceImpl as a singleton in DI
            builder.Services.AddSingleton<ClassroomServiceImpl>();

            // Configure Kestrel to listen on port 5000
            builder.WebHost.ConfigureKestrel(opts =>
            {
                opts.Listen(IPAddress.Any, 5000);  // Listen on all network interfaces
            });

            // Build and configure the web app
            var app = builder.Build();

            // Enable routing for the app
            app.UseRouting();

            // Map the gRPC ClassroomService
            app.MapGrpcService<ClassroomServiceImpl>();

            // Run the server
            app.Run();

            // Log server startup success
            log.Info("Server started and listening on http://127.0.0.1:5000");
        }
    }
}
