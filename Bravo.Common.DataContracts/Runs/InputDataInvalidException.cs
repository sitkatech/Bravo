using System;

namespace Bravo.Common.DataContracts.Runs
{
    public class InputDataInvalidException : Exception
    {
        public InputDataInvalidException(string message) : base(message)
        {
        }
    }
}