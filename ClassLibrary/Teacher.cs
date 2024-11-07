using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class Teacher
    {
        public int TeacherId { get; set; }
        public bool HasVotedToStart { get; set; }
        public bool HasVotedToEnd { get; set; }
        public string Name { get; set; }

        public Teacher(int teacherID, bool hasVotedToStart, bool hasVotedToEnd, string name)
        {
            this.TeacherId = teacherID;
            this.HasVotedToStart = hasVotedToStart;

        }
    }
}
