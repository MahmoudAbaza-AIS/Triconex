using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Triconex.Models
{
    public class FunctionBlockAttribute
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public string InitialValue { get; set; }
    }
}
