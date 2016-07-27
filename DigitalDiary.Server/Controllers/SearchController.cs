using DigitalDiary.Server.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace DigitalDiary.Server.Controllers
{
    public class SearchController : ApiController
    {
        // GET: api/Search/5
        public string Get(string id)
        {
            return GetSearchResults(id);
        }

        private string GetSearchResults(string searchText)
        {
            var searchResults = AzureSearchHelper.Search(searchText);
            List<DiaryDailyNotes> searchResultList = new List<DiaryDailyNotes>();

            foreach (var result in searchResults.Results)
            {
                searchResultList.Add(result.Document);
            }

            return JsonConvert.SerializeObject(searchResultList);
        }
    }
}
