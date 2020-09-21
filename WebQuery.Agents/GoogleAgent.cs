using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Windows.Forms;
using System.Threading;

namespace WebQuery.Agents
{
    public class GoogleAgent : BaseWebAgent, IWebAgent, IDisposable
    {
        private WebBrowser _browser;
        private Dictionary<string, string> _dataAttributes;
        private List<string> _items;
        private string _item;
        private SearchContext _context;

        public GoogleAgent(WebBrowser browser)
        {
            _browser = browser ?? new WebBrowser();
            _browser.ScriptErrorsSuppressed = true;

            _dataAttributes = new Dictionary<string, string>();
            _dataAttributes.Add(@"title", "title");
            _dataAttributes.Add(@"kc:/local:edit info", "");
            _dataAttributes.Add(@"kc:/collection/knowledge_panels/local_reviewable:star_score", null);
            _dataAttributes.Add(@"kc:/local:one line summary", "summary");
            _dataAttributes.Add(@"kc:/location/location:address", "Address");
            _dataAttributes.Add(@"kc:/location/location:hours", null);
            _dataAttributes.Add(@"kc:/collection/knowledge_panels/has_phone:phone", "Phone");
            _dataAttributes.Add(@"kc:/local:pending edits", null);
            _dataAttributes.Add(@"kc:/local:edit info Suggest an edit", null);
            _dataAttributes.Add(@"kc:/local:place qa", null);
            _dataAttributes.Add(@"kc:/collection/knowledge_panels/local_reviewable:review_summary", null);
            _dataAttributes.Add(@"kc:/collection/knowledge_panels/local_reviewable:reviews", null);
            _dataAttributes.Add(@"kc:/local:merchant_description", null);
            _dataAttributes.Add(@"kc:/common/topic:social media presence", null);

            _dataAttributes.Add(@"subtitle", "subtitle");
            _dataAttributes.Add(@"image", "image");
            _dataAttributes.Add(@"visit_official_site", "website");
            _dataAttributes.Add(@"description", "Description");
            _dataAttributes.Add(@"kc:/business/issuer:stock quote", "Stock price");
            _dataAttributes.Add(@"kc:/organization/organization:ceo", "CEO");
            _dataAttributes.Add(@"kc:/organization/organization:headquarters", "Headquarters");
            _dataAttributes.Add(@"hw:/collection/organizations:no of employees", "Number of employees");
            _dataAttributes.Add(@"hw:/collection/organizations:revenue", "Revenue");
            _dataAttributes.Add(@"hw:/collection/organizations:subsidiaries", "Subsidiaries");
            _dataAttributes.Add(@"okra:kp_placeholder/fact_text", "fact_text");
            _dataAttributes.Add(@"kc:/business/issuer:sideways", null);

            _dataAttributes.Add(@"kc:/local:sideways refinements", null);
            _dataAttributes.Add(@"kc:/local:recently opened", null);
            _dataAttributes.Add(@"kc:/local:covid uncertainty warning", null);
            _dataAttributes.Add(@"kc:/organization/organization:founded", "Founded");
            _dataAttributes.Add(@"kc:/location/location:third_party_aggregator_ratings", null);

            _dataAttributes.Add(@"kc:/business/business_operation:founder", "Founder");
            _dataAttributes.Add(@"hw:/collection/organizations:type", "Type");
        }

        public void Dispose()
        {
            _browser = null;
        }

        public void Query(string[] items, SearchContext context)
        {
            if (items == null || items.Length == 0) return;

            _items = (from item in items where !string.IsNullOrWhiteSpace(item.Trim()) select item.Trim()).ToList();
            _context = context;

            var thread = new Thread(DoSearchNext);

            thread.Start();
        }

        private string GetUrl()
        {
            switch (_context)
            { 
                case SearchContext.Company :
                    var q = HttpUtility.UrlEncode($"{_item} company");
                    return $"https://www.google.com/search?q={q}";
                default:
                    return null;
            }
        }

        private void DoSearchNext()
        {
            _item = _items[0].Trim();

            _items.RemoveAt(0);

            var url = GetUrl();

            if (string.IsNullOrWhiteSpace(url)) return;

            _browser.DocumentCompleted += OnDocumentCompleted;

            _browser.Navigate(url);
        }

        private void ParsePage(WebBrowser wb)
        {
            wb.Stop();

            var html = wb.DocumentText;

            var elements = from HtmlElement element in wb.Document.All where !string.IsNullOrWhiteSpace(element.GetAttribute("data-attrid")) select element;

            var result = new Dictionary<string, string>();

            foreach (var e in elements)
            {
                var attrName = e.GetAttribute("data-attrid");

                var attrValue = e.InnerText?.Trim();

                if (attrName.Equals("title") && "See results about".Equals(attrValue))
                {
                    var links = e.Parent.Parent.Parent.Parent.GetElementsByTagName("a");

                    if (links.Count == 1)
                    {
                        links[0].InvokeMember("click");

                        wb.DocumentCompleted += OnDocumentCompleted;

                        return;

                    }
                    else
                        continue;
                }

                if (_dataAttributes.ContainsKey(attrName))
                {
                    var name = _dataAttributes[attrName];


                    if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(attrValue))
                    {
                        if (attrValue.StartsWith($"{name}: "))
                        {
                            attrValue = attrValue.Substring($"{name}: ".Length);
                        }

                        if (attrValue.StartsWith($"{name}"))
                        {
                            attrValue = attrValue.Substring($"{name}".Length);
                        }

                        if (!result.ContainsKey(name))
                        {
                            result.Add(name, attrValue.Replace("\r\n", " "));
                        }
                        else
                        {
                            // duplicate 
                        }
                    }
                }
                else
                {
                    // attrName
                }
            }

            try
            {
                var args = new ItemCompleteArgs()
                {
                    Item = _item,
                    Context = _context,
                    Result = result,
                };

                DoItemComplete(args);
            }
            catch
            {
                // ignore
            }

            if (_items.Any())
            {
                DoSearchNext();
            }
            else
            {
                DoComplete();
            }
        }

        private void OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var wb = sender as WebBrowser;

            if (wb.IsBusy) return;

            wb.DocumentCompleted -= OnDocumentCompleted;

            ParsePage(wb);
        }

    }

}
