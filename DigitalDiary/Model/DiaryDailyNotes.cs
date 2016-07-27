using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalDiary.Model
{
    /// <summary>
    /// This class represents the contract to talk with the server.
    /// </summary>
    public class DiaryDailyNotes
    {
        /// <summary>
        /// Date string which should be in the format "dd-MMM-yy".
        /// </summary>
        public string DateString { get; set; }

        /// <summary>
        /// This represents the notes for current date.
        /// </summary>
        public string NotesForTheDay { get; set; }

        /// <summary>
        /// True if current day notes is bookmarked.
        /// </summary>
        public bool IsBookmarked { get; set; }
    }
}
