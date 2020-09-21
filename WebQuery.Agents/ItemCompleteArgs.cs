using System.Collections.Generic;

namespace WebQuery.Agents
{
    public class ItemCompleteArgs
    {
        public string Item { get; set; }
        public SearchContext Context { get; set; }
        public Dictionary<string, string> Result { get; set; }
    }
}
