namespace DigitalDiary.Server.Models
{
    /// <summary>
    /// Model class which represents a document in Azure search index.
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
