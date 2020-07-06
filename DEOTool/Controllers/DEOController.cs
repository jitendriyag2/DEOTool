using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DEOTool.Controllers
{
    public class SearchVM
    {
        public string Keyword { get; set; }
        public int PageNo { get; set; }

    }
    public class LinkData
    {
        public string Link { get; set; }
        public string Avaliable { get; set; }
        public string Label { get; set; }
        public string EngineName { get; set; }
    }
    public class ValidationData
    {
        public string result { get; set; }
        public string code { get; set; }
    }

    public class DEOController : Controller
    {

        [HttpGet]
        public async Task<IActionResult> GetSearchLinks()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetSearchLinks(SearchVM searchVM)
        {
            int PageNo = (searchVM.PageNo - 1) * 10;

            HttpClient client = new HttpClient();
            string currentSearchEngine = "https://www.google.com/search?q=";
            string SearcUrl = $"{currentSearchEngine}{searchVM.Keyword}&start={PageNo}";
            List<LinkData> links = new List<LinkData>();
            var msg = await client.GetAsync(SearcUrl);
            if (msg.IsSuccessStatusCode)
            {

                string HtmlCode = await msg.Content.ReadAsStringAsync();
                Regex regex = new Regex("<div class=\"BNeawe UPmit AP7Wnd\">.*?</div>");
                MatchCollection matches = regex.Matches(HtmlCode);
                HttpClient clientValidation = new HttpClient();
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        LinkData link = new LinkData();
                        link.EngineName = "Google";
                        string linkk = match.Groups[0].Value;
                        link.Label = linkk;
                        string httpFilter = @"((http\://|https\://|ftp\://)|(www.))+(([a-zA-Z0-9\.-]+\.[a-zA-Z]{2,4})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(/[a-zA-Z0-9%:/-_\?\.'~]*)?";
                        Regex mm = new Regex(httpFilter);
                        linkk = mm.Match(linkk).Groups[4].Value;
                        //link.Label = mm.Match(linkk).Groups[0].Value;
                        var valMsg = await clientValidation.GetAsync($"http://jitus.in/validate.php?username={linkk}");
                        var valOutput = await valMsg.Content.ReadAsStringAsync();
                        var val = JsonConvert.DeserializeObject<ValidationData>(valOutput);
                        if (val != null)
                        {
                            if (val.result.Contains("Success"))
                            {
                                link.Avaliable = "Yes";
                            }
                            else
                            {
                                link.Avaliable = "No";
                            }
                        }
                        link.Link = linkk;
                        links.Add(link);
                    }
                }
                return View("Show", links);
            }
            else
            {
                return NoContent();
            }
        }
    }
}
