using DigitalDiary.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DigitalDiary
{
    class DigitalDiaryHttpClient
    {
        public static void SaveNotesForTheDay(DiaryDailyNotes notes)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:1641/");
                var response = client.PutAsJsonAsync<DiaryDailyNotes>("api/DateNotes/" + notes.DateString, notes).Result;
            }
        }

        public static string GetNotesForTheDay(string dateString)
        {
            DiaryDailyNotes notes = new DiaryDailyNotes();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:1641/");
                var response = client.GetAsync("api/DateNotes/" + dateString).Result;

                var tempNotes = response.Content.ReadAsAsync<Object>().Result;
                notes = JsonConvert.DeserializeObject<DiaryDailyNotes>(tempNotes.ToString());
                // notes = response.Content.ReadAsAsync<DiaryDailyNotes>().Result;
            }

            return notes == null ? string.Empty : notes.NotesForTheDay;
        }

        public IEnumerable<DiaryDailyNotes> GetSearchResults(string searchString)
        {
            IEnumerable<DiaryDailyNotes> searchResults = new List<DiaryDailyNotes>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:1641/");
                var response = client.GetAsync("api/Search/" + searchString).Result;
                searchResults = response.Content.ReadAsAsync<IEnumerable<DiaryDailyNotes>>().Result;
            }

            return searchResults;
        }

        /*
        public static Task SaveNotesForTheDayAsync(DiaryDailyNotes notes)
        {
            return new Task(() => 
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:1641/");
                    client.PutAsJsonAsync<DiaryDailyNotes>("api/DateNotes/" + notes.DateString, notes);
                }
            });
        }

        public static Task<string> GetNotesForTheDayAsync(string dateString)
        {
            return new Task<string>(() => 
            {
                DiaryDailyNotes notes = new DiaryDailyNotes();
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:1641/");
                    var response = client.GetAsync("api/DateNotes/" + dateString).Result;
                    // notes = response.Content.ReadAsAsync<DiaryDailyNotes>().Result;
                    // return notes == null ? string.Empty : notes.NotesForTheDay;
                    return "test string for the date : " + dateString;
                }
            });
        }

        public static Task<IEnumerable<DiaryDailyNotes>> GetSearchResultsAsync(string searchString)
        {
            return new Task<IEnumerable<DiaryDailyNotes>>(() => 
            {
                IEnumerable<DiaryDailyNotes> searchResults = new List<DiaryDailyNotes>();
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:1641/");
                    var response = client.GetAsync("api/Search/" + searchString).Result;
                    searchResults = response.Content.ReadAsAsync<IEnumerable<DiaryDailyNotes>>().Result;
                }

                return searchResults;
            });
        }
        //*/

        public static async Task SaveNotesForTheDayAsync(DiaryDailyNotes notes)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:1641/");
                await client.PutAsJsonAsync<DiaryDailyNotes>("api/DateNotes/" + notes.DateString, notes);
            }
        }

        public static async Task<string> GetNotesForTheDayAsync(string dateString)
        {
            DiaryDailyNotes notes = new DiaryDailyNotes();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:1641/");
                var response = await client.GetAsync("api/DateNotes/" + dateString);
                var tempNotes = await response.Content.ReadAsAsync<Object>();
                notes = JsonConvert.DeserializeObject<DiaryDailyNotes>(tempNotes.ToString());
                return notes == null ? string.Empty : notes.NotesForTheDay;
            }
        }

        public static async Task<IEnumerable<DiaryDailyNotes>> GetSearchResultsAsync(string searchString)
        {
            IEnumerable<DiaryDailyNotes> searchResults = new List<DiaryDailyNotes>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:1641/");
                var response = await client.GetAsync("api/Search/" + searchString);
                searchResults = await response.Content.ReadAsAsync<IEnumerable<DiaryDailyNotes>>();
            }

            return searchResults;
        }
    }
}
