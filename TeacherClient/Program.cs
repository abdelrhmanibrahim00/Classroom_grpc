namespace TeacherClient
{
    using Google.Protobuf;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Core;
    using Grpc.Net.Client;
    using GrpcService1;
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Client representing a teacher in the classroom simulation.
    /// </summary>
    class TeacherClient
    {
        // List of potential names for teachers.
        private static readonly List<string> NAMES = new List<string>
        {
            "Alice", "Bob", "Charlie", "Diana", "Eve", "Frank", "Grace",
            "Hank", "Ivy", "Jack", "Karen", "Leo", "Mia", "Nina"
        };

        // Random number generator for random selections
        private static Random random = new Random();
        private static Teacher teacher = new Teacher
        {
            TeacherId = 0,
            HasVotedToStart = false,
            HasVotedToEnd = false,
            Name = GetRandomName()
        };

        // Logger instance for this class
        Logger mLog = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Selects a random name from the NAMES list.
        /// </summary>
        /// <returns>A randomly chosen name.</returns>
        private static string GetRandomName()
        {
            int randomIndex = random.Next(NAMES.Count);
            return NAMES[randomIndex];
        }

        /// <summary>
        /// Configures the logging system.
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
        /// Initiates the voting process for starting the class.
        /// </summary>
        /// <param name="teacher">The teacher voting.</param>
        /// <param name="classroomService">Classroom service client.</param>
        private void StartTeacherVoting(Teacher teacher, Classroom.ClassroomClient classroomService)
        {
            mLog.Info("Teachers are voting every 2 seconds to start the class.");
            Thread.Sleep(2000); // Wait between votes

            // 60% chance for the teacher to vote to start the class
            teacher.HasVotedToStart = random.NextDouble() >= 0.4;
            mLog.Info("Teacher has voted to start the class.");
            classroomService.VoteStartClass(teacher); // Send vote to server
        }

        /// <summary>
        /// Initiates the voting process for ending the class.
        /// </summary>
        /// <param name="teacher">The teacher voting.</param>
        /// <param name="classroomService">Classroom service client.</param>
        private void EndTeacherVoting(Teacher teacher, Classroom.ClassroomClient classroomService)
        {
            mLog.Info("Teachers are voting every 2 seconds to end the class.");
            Thread.Sleep(2000); // Wait between votes

            // 60% chance for the teacher to vote to end the class
            teacher.HasVotedToEnd = random.NextDouble() >= 0.4;
            mLog.Info("Teacher has voted to end the class.");
            classroomService.VoteEndClass(teacher); // Send vote to server
        }

        /// <summary>
        /// Main logic loop that handles teacher voting based on classroom session state.
        /// </summary>
        private void Run()
        {
            // Set up logging
            ConfigureLogging();

            // Connect to the gRPC server and create classroom service client
            var channel = GrpcChannel.ForAddress("http://127.0.0.1:5000");
            var classroomService = new Classroom.ClassroomClient(channel);

            while (true)
            {
                try
                {
                    mLog.Info("Starting main loop for teacher activities.");

                    // Assign a unique ID to the teacher if not yet assigned
                    if (teacher.TeacherId == 0)
                    {
                        teacher.TeacherId = classroomService.GetUniqueId(new Empty()).UniqueId;
                        mLog.Info($"Assigned ID {teacher.TeacherId} to teacher {teacher.Name}.");
                    }

                    mLog.Info($"Teacher {teacher.Name} is present in the classroom.");

                    // Check if class is not in session and vote to start if there are enough students
                    if (!classroomService.IsClassInSession(new Empty()).IsInSession)
                    {
                        mLog.Info("Class is not currently in session.");

                        mLog.Info("Teacher is checking for sufficient students to start class.");
                        if (classroomService.EnoughStudents(new Empty()).EnoughStudents)
                        {
                            mLog.Info("Sufficient students present; starting voting process to begin class.");
                            StartTeacherVoting(teacher, classroomService);
                        }
                        else
                        {
                            mLog.Info("Insufficient students to start the class.");
                        }
                    }

                    // If class is in session, initiate voting to end the session
                    if (classroomService.IsClassInSession(new Empty()).IsInSession)
                    {
                        mLog.Info("Class is in session; initiating voting to end the class.");
                        EndTeacherVoting(teacher, classroomService);
                    }

                    // Simulate class duration before checking again
                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    // Log and handle unexpected exceptions
                    mLog.Warn(e, "Unhandled exception encountered; restarting main loop.");
                    Thread.Sleep(2000); // Pause before retrying to avoid console spamming
                }
            }
        }

        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        static void Main(string[] args)
        {
            var self = new TeacherClient();
            self.Run();
        }
    }
}
