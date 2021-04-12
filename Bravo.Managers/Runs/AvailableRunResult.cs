using System.Collections.Generic;

namespace Bravo.Managers.Runs
{
    public class AvailableRunResult
    {
        public string FileName { get; set; }
        public List<string> AvailableSubTypes { get; set; }
        public List<string> AvailableFileTypes { get; set; }
    }
}