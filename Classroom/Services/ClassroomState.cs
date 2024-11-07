namespace Classroom.Services
{
    /// <summary>
    /// Represents the state of the classroom, including session status, student count, and voting logic.
    /// </summary>
    public class ClassroomState
    {
        /// <summary>
        /// Lock for synchronizing access to the classroom state.
        /// </summary>
        public readonly object AccessLock = new object();

        /// <summary>
        /// Indicates if a class session is currently active.
        /// </summary>
        public bool ClassInSession = false;

        /// <summary>
        /// Current count of students present in the classroom.
        /// </summary>
        public int StudentCount = 0;

        /// <summary>
        /// Minimum number of students required to start the class.
        /// </summary>
        public readonly int StartThreshold = 3;

        /// <summary>
        /// Determines if the votes are sufficient to start or end a class session.
        /// </summary>
        /// <param name="votesStart">Dictionary of votes where each entry represents a teacher's vote.</param>
        /// <returns>True if the number of votes to start is sufficient, otherwise false.</returns>
        public bool AreVotesStartSufficient(Dictionary<int, bool> votesStart)
        {
            // Check if there is more than one vote
            if (votesStart.Count <= 1)
            {
                return false;
            }

            // Count the number of positive votes
            int trueCount = votesStart.Values.Count(vote => vote);

            // Calculate the ratio of positive votes to the total votes
            double ratio = (double)trueCount / votesStart.Count;

            // Threshold for votes to be sufficient (e.g., 50% or more)
            double threshold = 0.5;

            // Return true if the ratio exceeds the threshold, otherwise false
            return ratio > threshold;
        }
    }
}
