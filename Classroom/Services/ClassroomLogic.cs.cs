using GrpcService1;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Classroom.Services
{
    /// <summary>
    /// Classroom logic for managing student count, voting, and session state. Thread-safe.
    /// </summary>
    class ClassroomLogic
    {
        // Unique ID counter for assigning IDs to teachers and doors
        private int LastUniqueId = 0;

        // Logger for logging actions within ClassroomLogic
        private Logger mLog = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Dictionary to track votes for starting the class session.
        /// </summary>
        private readonly Dictionary<int, bool> VotesStart = new Dictionary<int, bool>();

        /// <summary>
        /// Dictionary to track votes for ending the class session.
        /// </summary>
        private readonly Dictionary<int, bool> VotesEnd = new Dictionary<int, bool>();

        // Instance of ClassroomState to hold the current class session status and student count
        private readonly ClassroomState mState = new ClassroomState();

        // Accumulates the number of students generated from doors
        private int generatedStudentCount { get; set; }

        /// <summary>
        /// Increments the count of generated students based on input from the door client.
        /// </summary>
        public void UpdateStudentCount(int additionalStudents)
        {
            lock (mState.AccessLock)
            {
                generatedStudentCount += additionalStudents;
            }
        }

        /// <summary>
        /// Determines if there are enough students to initiate the voting process to start the class.
        /// </summary>
        public bool CanVotingStart()
        {
            lock (mState.AccessLock)
            {
                bool sufficientStudents = generatedStudentCount >= mState.StartThreshold && !mState.ClassInSession;
                if (sufficientStudents)
                {
                    mLog.Info("There are enough students to start the voting process.");
                }
                return sufficientStudents;
            }
        }

        /// <summary>
        /// Generates and returns a unique ID for a new teacher or door.
        /// </summary>
        public int GetUniqueId()
        {
            lock (mState.AccessLock)
            {
                LastUniqueId += 1;
                return LastUniqueId;
            }
        }

        /// <summary>
        /// Returns the current session state, indicating whether the class is in session.
        /// </summary>
        public bool IsClassInSession()
        {
            lock (mState.AccessLock)
            {
                return mState.ClassInSession;
            }
        }

        /// <summary>
        /// Registers a teacher's vote to start the class session.
        /// </summary>
        public bool VoteToStartClass(Teacher teacher)
        {
            lock (mState.AccessLock)
            {
                if (mState.ClassInSession)
                {
                    mLog.Info($"Teacher {teacher.TeacherId} cannot vote; class is already in session.");
                    return false;
                }

                VotesStart[teacher.TeacherId] = teacher.HasVotedToStart;
                mLog.Info($"Teacher {teacher.TeacherId} voted to start the class.");

                if (VotesStart.Count == LastUniqueId && mState.AreVotesStartSufficient(VotesStart))
                {
                    StartClass();
                    mLog.Info("Votes were sufficient to start the class.");
                    return true;
                }

                mLog.Info("Votes to start the class were not sufficient.");
                return false;
            }
        }

        /// <summary>
        /// Registers a teacher's vote to end the class session.
        /// </summary>
        public bool VoteToEndClass(Teacher teacher)
        {
            lock (mState.AccessLock)
            {
                VotesEnd[teacher.TeacherId] = teacher.HasVotedToEnd;
                mLog.Info($"Teacher {teacher.TeacherId} voted to end the class.");

                if (VotesEnd.Count == LastUniqueId && mState.AreVotesStartSufficient(VotesEnd))
                {
                    EndClass();
                    mLog.Info("Votes were sufficient to end the class.");
                    return true;
                }

                mLog.Info("Votes to end the class were not sufficient.");
                return false;
            }
        }

        /// <summary>
        /// Starts the class session, resets votes, and logs the action.
        /// </summary>
        private void StartClass()
        {
            lock (mState.AccessLock)
            {
                mState.ClassInSession = true;
                VotesStart.Clear();
                mLog.Info("Class has started.");
            }
        }

        /// <summary>
        /// Ends the class session, clears votes, and decreases the student count randomly.
        /// </summary>
        private void EndClass()
        {
            lock (mState.AccessLock)
            {
                mState.ClassInSession = false;
                VotesEnd.Clear();

                var rnd = new Random();
                int studentsLeaving = rnd.Next(1, mState.StudentCount + 1);
                mState.StudentCount = Math.Max(0, mState.StudentCount - studentsLeaving);

                mLog.Info($"Class has ended. {studentsLeaving} students left the classroom.");
            }
        }
    }
}
