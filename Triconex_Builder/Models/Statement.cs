using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;

namespace Triconex.Models
{
    public interface IInstruction
    {
        public string? Operator { get; }   
        public Address? Address { get; }      
        public IInstruction GetInstraction(string statement);
    }


    public class Address
    {
        public string? Content { get; set; }
        public string? MemoryArea { get; set; }  
        public string? Value { get; set; }
    }


    public class BitLogicInstruction : IInstruction
    {
        public string? Operator { get; private set; }
        public Address? Address { get; private set; } 
        public IInstruction GetInstraction(string statement)
        {
            var InstractionAddress = Regex.Match(statement.Trim(), @"\b(?!A\b|AN\b|O\b|ON\b|X\b|XN\b|R\b)\w+\s*\d*\.*\d*").Value;
            return new BitLogicInstruction  
            {
                Operator = Regex.Match(statement.Trim(), @"\S+").Value,
                Address = new Address
                {
                    Content = InstractionAddress,
                    MemoryArea = Regex.Match(InstractionAddress.Trim(), @"^\w+").Value,
                    Value = Regex.Match(InstractionAddress.Trim(), @"\d+\.\d+").Value,
                }
            };
        }
    }

    public class LoadAndTransferInstruction : IInstruction
    {
        public string? Operator { get; private set; }
        public Address? Address { get; private set; }

        public IInstruction GetInstraction(string statement)
        {
            var InstractionAddress = Regex.Match(statement.Trim(), @"\b(?!A\b|AN\b|O\b|ON\b|X\b|XN\b|R\b)\w+\s*\d*\.*\d*").Value;

            return new LoadAndTransferInstruction
            {
                Operator = Regex.Match(statement.Trim(), @"\S+").Value,
                Address = new Address
                {
                    Content = InstractionAddress,
                    MemoryArea = Regex.Match(InstractionAddress.Trim(), @"^\w+").Value,
                    Value = Regex.Match(InstractionAddress.Trim(), @"\d+\.\d+").Value,
                }
            };
        }
    }

    public class XInstruction : IInstruction
    {
        public string? Operator { get; private set; }
        public Address? Address { get; private set; }

        public IInstruction GetInstraction(string statement)
        {
            var InstractionAddress = Regex.Match(statement.Trim(), @"\b(?!A\b|AN\b|O\b|ON\b|X\b|XN\b|R\b)\w+\s*\d*\.*\d*").Value;

            return new XInstruction
            {
                Operator = Regex.Match(statement.Trim(), @"\S+").Value,
                Address = new Address
                {
                    Content = InstractionAddress,
                    MemoryArea = Regex.Match(InstractionAddress.Trim(), @"^\w+").Value,
                    Value = Regex.Match(InstractionAddress.Trim(), @"\d+\.\d+").Value,
                }
            };
        }
    }
}

