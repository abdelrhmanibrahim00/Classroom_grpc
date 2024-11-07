using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class Door
    {
        public int DoorId { get; set; }
        public int AmountOfStudents { get; set; }
        public bool IsClosed { get; set; }
        public bool IsOpened { get; set; }
        public string Name { get; set; }

        public Door(int doorId, int amountofstudents, bool isClosed, bool isOpened, string name)
        {
            DoorId = doorId;
            AmountOfStudents = amountofstudents;
            IsClosed = isClosed;
            IsOpened = isOpened;
            Name = name;
        }
        
    }
}
