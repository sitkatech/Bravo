using System;

namespace Bravo.Common.DataContracts.Runs
{
    public class OutputDataInvalidException : Exception
    {
        public OutputDataInvalidException(string message) : this(message, RunStatus.InvalidOutput)
        {
        }

        public OutputDataInvalidException(string message, RunStatus status) : base(message)
        {
            Status = status;
        }

        public RunStatus Status { get; set; }
    }
}