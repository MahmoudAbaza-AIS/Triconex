using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Triconex.Models
{
    public class FunctionBlock  
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
        public List<FunctionBlockAttribute>? Inputs { get; set; }
        public List<FunctionBlockAttribute>? Outputs{ get; set; }
        public List<FunctionBlockAttribute>? LocalVariables{ get; set; }  
        public List<Network>? Network { get; set; }  = new List<Network>();
    }
}
