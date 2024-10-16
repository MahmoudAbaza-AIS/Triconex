using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Triconex.Services;

namespace Triconex.Models
{
    public class ProgramBuilder     
    {
        public string Content { get; set; }
        public List<FunctionBlock> FunctionBlocks { get; set; }
        public List<Function> Functions { get; set; }
        public List<Network>? StandaloneNetwork { get; set; }


        public ProgramBuilder GetProgramBuilder(List<string> FileData, string Content)
        {
            var FunctionBlocks = FunctionBlockService.GetFunctionBlockDetails(FileData);
            var Functions = FunctionService.GetFunctionsDetails(FileData);
            var StandaloneNetwork = new List<Network>();

            if (FunctionBlocks.Count() == 0 && Functions.Count() == 0)
                StandaloneNetwork = NetworkService.GetNetworksDetails(FileData);

            return new ProgramBuilder()
            {
                Content = Content,
                FunctionBlocks = FunctionBlocks,
                Functions = Functions,
                StandaloneNetwork = StandaloneNetwork
            };
        }
    }
}
