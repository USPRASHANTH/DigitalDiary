using System;
using System.ComponentModel;

namespace DigitalDiary.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private DateTimeOffset datePickerDate;
        private NotifyTaskCompletion<string> currentDayNotes;
        public event PropertyChangedEventHandler PropertyChanged;

        public MainPageViewModel(DateTime datePickerDate)
        {
            DatePickerDate = new DateTimeOffset(datePickerDate);
            RecomputeCurrentDayNotes();
        }

        public DateTimeOffset DatePickerDate
        {
            get
            {
                return datePickerDate;
            }
            set
            {
                datePickerDate = value;
                OnPropertyChanged("DatePickerDate");
                OnPropertyChanged("DummyNotesForTheDay");
            }
        }

        public NotifyTaskCompletion<string> CurrentDayNotes
        {
            get
            {
                return currentDayNotes;
            }
            set
            {
                currentDayNotes = value;
                OnPropertyChanged("CurrentDayNotes");
            }
        }

        public void RecomputeCurrentDayNotes()
        {
            CurrentDayNotes = new NotifyTaskCompletion<string>(DigitalDiaryHttpClient.GetNotesForTheDayAsync(DatePickerDate.Date.ToString("dd-MMM-yy")));
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
