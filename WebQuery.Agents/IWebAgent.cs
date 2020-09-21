using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebQuery.Agents
{
    interface IWebAgent
    {
        void Query(string[] items, SearchContext context);
    }
}
