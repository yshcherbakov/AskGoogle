using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebQuery.Agents
{

    public enum SearchContext { Company }

    public abstract class BaseWebAgent
    {
        public delegate void ItemCompleteHandler(object sender, ItemCompleteArgs e);

        public event ItemCompleteHandler ItemComplete;
        public event EventHandler Complete;

        protected void DoItemComplete(ItemCompleteArgs args)
        {
            ItemComplete?.Invoke(this, args);
        }

        protected void DoComplete()
        {
            Complete?.Invoke(this, new EventArgs());
        }
    }
}
