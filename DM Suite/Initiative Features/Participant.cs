using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM_Suite.Initiative_Features
{
    public class Participant
    {
        public string Name { get; set; }
        public int Initiative { get; set; }
        public bool Alive { get; set; }
        public string Session { get; set; }

        public Participant() { }
    }
}
