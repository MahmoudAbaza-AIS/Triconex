using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Triconex.Models
{
    public class Network
    {
        public int Number { get; set; }
        public List<IInstruction> Instructions { get; set; }
    }
}