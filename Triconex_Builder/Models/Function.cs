using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Triconex.Models
{
    public class Function
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public List<Network> Network { get; set; } = new List<Network>();
    }
}
