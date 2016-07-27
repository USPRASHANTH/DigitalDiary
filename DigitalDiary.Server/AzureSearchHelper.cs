using DigitalDiary.Server.Models;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace DigitalDiary.Server
{
    public static class AzureSearchHelper
    {
        private const string SearchIndexName = "digitaldiary-notesindex";

        private static SearchServiceClient _serviceClient;
        private static SearchIndexClient _indexClient;

        private static string GetSearchServiceName()
        {
            return ConfigurationManager.AppSettings["SearchServiceName"];
        }

        private static string GetAdminApiKey()
        {
            return ConfigurationManager.AppSettings["SearchServiceAdminApiKey"];
        }

        private static SearchServiceClient GetSearchServiceClient()
        {
            if (_serviceClient == null)
            {
                _serviceClient = new SearchServiceClient(GetSearchServiceName(), new SearchCredentials(GetAdminApiKey()));
            }

            return _serviceClient;
        }

        private static SearchIndexClient GetSearchIndexClient()
        {
            if (_indexClient == null)
            {
                _indexClient = GetSearchServiceClient().Indexes.GetClient(SearchIndexName);
            }

            return _indexClient;
        }

        public static async Task<DocumentIndexResult> UploadDocumentAsync(DiaryDailyNotes notesForTheDay)
        {
            var actions = new IndexAction<DiaryDailyNotes>[] { IndexAction.Upload(notesForTheDay) };
            var indexBatch = IndexBatch.New(actions);
            return await GetSearchIndexClient().Documents.IndexAsync(indexBatch);
        }

        public static async Task<DocumentSearchResult<DiaryDailyNotes>> GetDocumentAsync(string dateString)
        {
            SearchParameters sp = new SearchParameters()
            {
                Filter = "DateString eq '" + dateString + "'",
            };

            return await GetSearchIndexClient().Documents.SearchAsync<DiaryDailyNotes>(dateString, sp);
        }

        public static async Task<DocumentSearchResult<DiaryDailyNotes>> SearchAsync(string searchText)
        {
            SearchParameters sp = new SearchParameters()
            {
                SearchMode = SearchMode.Any,
                SearchFields = new List<string>() { "NotesForTheDay" },
                Top = 20
            };

            return await GetSearchIndexClient().Documents.SearchAsync<DiaryDailyNotes>(searchText, sp);
        }

        public static DocumentIndexResult UploadDocument(DiaryDailyNotes notesForTheDay)
        {
            var actions = new IndexAction<DiaryDailyNotes>[] { IndexAction.Upload(notesForTheDay) };
            var indexBatch = IndexBatch.New(actions);
            return GetSearchIndexClient().Documents.Index(indexBatch);
        }

        public static DocumentSearchResult<DiaryDailyNotes> GetDocument(string dateString)
        {
            SearchParameters sp = new SearchParameters()
            {
                Filter = "DateString eq '" + dateString + "'"
            };

            return GetSearchIndexClient().Documents.Search<DiaryDailyNotes>("*", sp);
        }

        public static DocumentSearchResult<DiaryDailyNotes> Search(string searchText)
        {
            SearchParameters sp = new SearchParameters()
            {
                SearchMode = SearchMode.Any,
                SearchFields = new List<string>() { "NotesForTheDay" },
                Top = 20
            };

            return GetSearchIndexClient().Documents.Search<DiaryDailyNotes>(searchText, sp);
        }
    }
}
