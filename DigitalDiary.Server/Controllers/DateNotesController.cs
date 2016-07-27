using DigitalDiary.Server.Models;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;

namespace DigitalDiary.Server.Controllers
{
    public class DateNotesController : ApiController
    {
        // GET: api/DateNotes/5
        [HttpGet]
        public string Get(string id)
        {
            return GetNotesForTheDay(id);
        }

        // PUT: api/DateNotes/5
        [HttpPut]
        public void Put(string id, [FromBody]DiaryDailyNotes value)
        {
            SaveNotesForTheDay(id, value.NotesForTheDay);
        }

        private string GetNotesForTheDay(string dateString)
        {
            if (IsDateStringValid(dateString))
            {
                DocumentSearchResult<DiaryDailyNotes> results = AzureSearchHelper.GetDocument(dateString);
                if (results != null && results.Results != null && results.Results.FirstOrDefault() != null)
                {
                    DiaryDailyNotes notes = results.Results.FirstOrDefault().Document;
                    string notesForTheDay = JsonConvert.SerializeObject(notes);
                    return notesForTheDay;
                }
            }

            return string.Empty;
        }

        private void SaveNotesForTheDay(string dateString, string notesString)
        {
            if (IsDateStringValid(dateString))
            {
                DiaryDailyNotes notes = new DiaryDailyNotes()
                {
                    DateString = dateString,
                    NotesForTheDay = notesString,
                    IsBookmarked = false,
                };

                var indexingResult = AzureSearchHelper.UploadDocument(notes);
                Debug.Print("SaveNotesForTheDay() status : " + indexingResult.Results.FirstOrDefault().Succeeded);
            }
        }

        private bool IsDateStringValid(string dateString)
        {
            DateTime temp;
            return DateTime.TryParse(dateString, out temp);
        }
    }
}
