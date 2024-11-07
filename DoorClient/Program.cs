namespace DoorClient
{
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Net.Client;
    using GrpcService1;
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    class DoorClient
    {
        // A list of potential names and surnames for use with generated students.
        private readonly List<string> NAMES = new List<string> { "John", "Peter", "Jack", "Steve" };
        private readonly List<string> SURNAMES = new List<string> { "Johnson", "Peterson", "Jackson", "Steveson" };

        // Logger instance for DoorClient.
        private Logger mLog = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Configures the logging subsystem for the application.
        /// </summary>
        private void ConfigureLogging()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var console = new NLog.Targets.ConsoleTarget("console")
            {
                Layout = @"${date:format=HH\:mm\:ss}|${level}| ${message} ${exception}"
            };
            config.AddTarget(console);
            config.AddRuleForAllLevels(console);
            LogManager.Configuration = config;
        }

        /// <summary>
        /// Executes the primary program logic, including setting up the gRPC connection,
        /// handling student generation, and sending data to the server.
        /// </summary>
        private void Run()
        {
            // Initialize logging configuration
            ConfigureLogging();

            var rnd = new Random();

            // Main loop to manage server connection and recover from potential errors
            while (true)
            {
                try
                {
                    // Connect to the gRPC server and initialize the Classroom service client
                    var channel = GrpcChannel.ForAddress("http://127.0.0.1:5000");
                    var classroomService = new Classroom.ClassroomClient(channel);

                    // Initialize the door descriptor
                    int uniqueId = 0; // Placeholder: method to retrieve a unique ID for the door
                    var door = new Door
                    {
                        DoorId = uniqueId,
                        AmountOfStudents = 0,
                        IsOpened = true,
                        Name = "Main entrance door"
                    };

                    // Log door initialization data
                    mLog.Info($"Door {door.DoorId} initialized. Description: {door.Name}.");

                    // Door interaction logic loop
                    while (true)
                    {
                        mLog.Info("Door interaction loop active.");

                        // Check if the class is currently in session
                        if (!classroomService.IsClassInSession(new Empty()).IsInSession)
                        {
                            // Simulate random student arrival
                            int arrivingStudents = rnd.Next(-6, 10);
                            door.AmountOfStudents += arrivingStudents;
                            mLog.Info($"{arrivingStudents} students arrived at Door {door.DoorId}. Total: {door.AmountOfStudents}");

                            // Notify the classroom service of the new student count
                            var studentRequest = new StudentGenerationRequest { Door = door };
                            classroomService.GeneratedNumberOfStudents(studentRequest);
                            mLog.Info($"{arrivingStudents} students sent to server. Total students at Door {door.DoorId}: {door.AmountOfStudents}");

                            // Wait before generating the next batch of students
                            Thread.Sleep(rnd.Next(1000, 3000));
                        }
                        else
                        {
                            mLog.Info($"Door {door.DoorId} is closed; no students may enter.");
                            Thread.Sleep(2000); // Delay before rechecking the session status
                        }
                    }
                }
                catch (Exception e)
                {
                    // Log exceptions and retry connection after a delay
                    mLog.Warn(e, "Unhandled exception caught. Restarting main loop.");
                    Thread.Sleep(2000);
                }
            }
        }

        /// <summary>
        /// Program entry point that initializes and runs the DoorClient.
        /// </summary>
        static void Main(string[] args)
        {
            var self = new DoorClient();
            self.Run();
        }
    }
}
