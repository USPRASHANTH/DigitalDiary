using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Media.SpeechRecognition;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.UI.Core;
using System.Text;
using DigitalDiary.Model;
using DigitalDiary.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DigitalDiary
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SpeechRecognizer speechRecognizer;
        private CoreDispatcher dispatcher;
        private bool isListening;
        private StringBuilder dictatedTextBuilder;
        private MainPageViewModel viewModel;

        /// <summary>
        /// This HResult represents the scenario where a user is prompted to allow in-app speech, but 
        /// declines. This should only happen on a Phone device, where speech is enabled for the entire device,
        /// not per-app.
        /// </summary>
        private static uint HResultPrivacyStatementDeclined = 0x80045509;

        public MainPage()
        {
            this.InitializeComponent();
            isListening = false;
            dictatedTextBuilder = new StringBuilder();
            viewModel = new MainPageViewModel(DateTime.Now);
            DataContext = viewModel;
        }

        /// <summary>
        /// Begin recognition, or finish the recognition session. 
        /// </summary>
        /// <param name="sender">The button that generated this event</param>
        /// <param name="e">Unused event details</param>
        public async void ContinuousRecognize_Click(object sender, RoutedEventArgs e)
        {
            if (speechRecognizer == null)
            {
                return;
            }

            btnContinuousRecognize.IsEnabled = false;
            acceptPreviewButton.IsEnabled = false;
            clearPreviewButton.IsEnabled = false;

            if (isListening == false)
            {
                // The recognizer can only start listening in a continuous fashion if the recognizer is currently idle.
                // This prevents an exception from occurring.
                if (speechRecognizer.State == SpeechRecognizerState.Idle)
                {
                    DictationButtonText.Text = " Stop Dictation";
                    discardedTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                    try
                    {
                        isListening = true;
                        await speechRecognizer.ContinuousRecognitionSession.StartAsync();
                    }
                    catch (Exception ex)
                    {
                        if ((uint)ex.HResult == HResultPrivacyStatementDeclined)
                        {
                            // Show a UI link to the privacy settings.
                        }
                        else
                        {
                            var messageDialog = new Windows.UI.Popups.MessageDialog(ex.Message, "Exception");
                            await messageDialog.ShowAsync();
                        }

                        isListening = false;
                        DictationButtonText.Text = " Dictate";

                    }
                }
            }
            else
            {
                isListening = false;
                DictationButtonText.Text = " Dictate";

                if (speechRecognizer.State != SpeechRecognizerState.Idle)
                {
                    // Cancelling recognition prevents any currently recognized speech from
                    // generating a ResultGenerated event. StopAsync() will allow the final session to 
                    // complete.
                    try
                    {
                        await speechRecognizer.ContinuousRecognitionSession.StopAsync();

                        /*
                        // Ensure we don't leave any hypothesis text behind
                        dictationTextBox.Text = dictatedTextBuilder.ToString();
                        //*/

                        AddCurrentDictationToDailyNotes();
                    }
                    catch (Exception exception)
                    {
                        var messageDialog = new Windows.UI.Popups.MessageDialog(exception.Message, "Exception");
                        await messageDialog.ShowAsync();
                    }
                }
            }

            // btnContinuousRecognize.IsEnabled = speechListeningMode.IsOn && true;
            btnContinuousRecognize.IsEnabled = true;
            acceptPreviewButton.IsEnabled = true;
            clearPreviewButton.IsEnabled = true;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            // btnContinuousRecognize.IsEnabled = speechListeningMode.IsOn && true;
            // btnContinuousRecognize.IsEnabled = true;
            if (speechListeningMode.IsOn)
            {
                btnContinuousRecognize.IsEnabled = true;
                acceptPreviewButton.IsEnabled = true;
                clearPreviewButton.IsEnabled = true;
                await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage);
            }
        }

        private async void speechListeningMode_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;

            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    // Enable speech recognition
                    await this.EnableSpeechRecognition();
                }
                else
                {
                    // Disable speech recognition
                    await this.DisableSpeechRecognition();
                }
            }
        }

        private async Task EnableSpeechRecognition()
        {
            if (this.speechRecognizer == null)
            {
                // await this.InitializeSpeechRecognition();
                await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage);
            }

            await this.speechRecognizer.RecognizeAsync();
            btnContinuousRecognize.IsEnabled = true;
            acceptPreviewButton.IsEnabled = true;
            clearPreviewButton.IsEnabled = true;
        }

        private async Task DisableSpeechRecognition()
        {
            if (this.speechRecognizer != null)
            {
                if (speechRecognizer.State != SpeechRecognizerState.Idle)
                {
                    // Cancelling recognition prevents any currently recognized speech from
                    // generating a ResultGenerated event. StopAsync() will allow the final session to 
                    // complete.
                    try
                    {
                        await speechRecognizer.ContinuousRecognitionSession.StopAsync();

                        // Ensure we don't leave any hypothesis text behind
                        dictationTextBox.Text = dictatedTextBuilder.ToString();
                    }
                    catch (Exception exception)
                    {
                        var messageDialog = new Windows.UI.Popups.MessageDialog(exception.Message, "Exception");
                        await messageDialog.ShowAsync();
                    }
                }

                CleanupSpeechRecognizer();
                btnContinuousRecognize.IsEnabled = false;
            }
        }

        /*
        private async Task InitializeSpeechRecognition()
        {
            if (this.speechRecognizer != null)
            {
                this.speechRecognizer.Dispose();
                this.speechRecognizer = null;
            }

            this.speechRecognizer = new SpeechRecognizer(SpeechRecognizer.SystemSpeechLanguage);
            var dictationConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "dictation");
            speechRecognizer.Constraints.Add(dictationConstraint);
            SpeechRecognitionCompilationResult result = await speechRecognizer.CompileConstraintsAsync();

            if (result.Status != SpeechRecognitionResultStatus.Success)
            {
                // Grammar compilation failed.
            }
        }
        //*/

        /// <summary>
        /// Initialize Speech Recognizer and compile constraints.
        /// </summary>
        /// <param name="recognizerLanguage">Language to use for the speech recognizer</param>
        /// <returns>Awaitable task.</returns>
        private async Task InitializeRecognizer(Language recognizerLanguage)
        {
            CleanupSpeechRecognizer();

            this.speechRecognizer = new SpeechRecognizer(recognizerLanguage);

            // Provide feedback to the user about the state of the recognizer. This can be used to provide visual feedback in the form
            // of an audio indicator to help the user understand whether they're being heard.
            speechRecognizer.StateChanged += SpeechRecognizer_StateChanged;

            // Apply the dictation topic constraint to optimize for dictated freeform speech.
            var dictationConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "dictation");
            speechRecognizer.Constraints.Add(dictationConstraint);
            SpeechRecognitionCompilationResult result = await speechRecognizer.CompileConstraintsAsync();
            if (result.Status != SpeechRecognitionResultStatus.Success)
            {
                // rootPage.NotifyUser("Grammar Compilation Failed: " + result.Status.ToString(), NotifyType.ErrorMessage);
                btnContinuousRecognize.IsEnabled = false;
            }

            // Handle continuous recognition events. Completed fires when various error states occur. ResultGenerated fires when
            // some recognized phrases occur, or the garbage rule is hit. HypothesisGenerated fires during recognition, and
            // allows us to provide incremental feedback based on what the user's currently saying.
            speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;
        }

        /// <summary>
        /// Handle events fired when error conditions occur, such as the microphone becoming unavailable, or if
        /// some transient issues occur.
        /// </summary>
        /// <param name="sender">The continuous recognition session</param>
        /// <param name="args">The state of the recognizer</param>
        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (args.Status != SpeechRecognitionResultStatus.Success)
            {
                // If TimeoutExceeded occurs, the user has been silent for too long. We can use this to 
                // cancel recognition if the user in dictation mode and walks away from their device, etc.
                // In a global-command type scenario, this timeout won't apply automatically.
                // With dictation (no grammar in place) modes, the default timeout is 20 seconds.
                if (args.Status == SpeechRecognitionResultStatus.TimeoutExceeded)
                {
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        // rootPage.NotifyUser("Automatic Time Out of Dictation", NotifyType.StatusMessage);
                        DictationButtonText.Text = " Dictate";

                        AddCurrentDictationToDailyNotes();
                        /*
                        // Clear the dictation text box and append the dictation to the DailyNotes.
                        dictationTextBox.Text = string.Empty;
                        string existingNotes;
                        currentDayNotes.Document.Selection.GetText(Windows.UI.Text.TextGetOptions.FormatRtf, out existingNotes);
                        currentDayNotes.Document.Selection.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, existingNotes += dictatedTextBuilder.ToString());
                        dictatedTextBuilder.Clear();
                        //*/

                        isListening = false;
                    });
                }
                else
                {
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        // rootPage.NotifyUser("Continuous Recognition Completed: " + args.Status.ToString(), NotifyType.StatusMessage);
                        DictationButtonText.Text = " Dictate";
                        isListening = false;
                    });
                }
            }
        }

        /*
        private bool IsCommand(string commandText)
        {
            return commandText.StartsWith("command", StringComparison.OrdinalIgnoreCase) && commandText.Length > "commands ".Length;
        }

        private void ExecuteCommand(string commandText)
        {
            commandText = commandText.Substring(commandText.IndexOf(" "));

            if (commandText.IndexOf("search", StringComparison.OrdinalIgnoreCase) > 0)
            {
                // Start search
            }
            else if (commandText.IndexOf("previous day", StringComparison.OrdinalIgnoreCase) > 0)
            {
                // Go to previous day
                datePicker.Date = datePicker.Date.AddDays(-1);
            }
            else if (commandText.IndexOf("next day", StringComparison.OrdinalIgnoreCase) > 0)
            {
                // Go to next day
                datePicker.Date = datePicker.Date.AddDays(1);
            }
            else if (commandText.IndexOf("save", StringComparison.OrdinalIgnoreCase) > 0)
            {
                // Save notes
                saveButton_Click(this, new RoutedEventArgs());
            }
        }
        //*/

        /// <summary>
        /// While the user is speaking, update the textbox with the partial sentence of what's being said for user feedback.
        /// </summary>
        /// <param name="sender">The recognizer that has generated the hypothesis</param>
        /// <param name="args">The hypothesis formed</param>
        private async void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            string hypothesis = args.Hypothesis.Text;

            // Update the textbox with the currently confirmed text, and the hypothesis combined.
            string textboxContent = dictatedTextBuilder.ToString() + " " + hypothesis + " ...";
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                dictationTextBox.Text = textboxContent;
            });
        }

        /// <summary>
        /// Handle events fired when a result is generated. Check for high to medium confidence, and then append the
        /// string to the end of the stringbuffer, and replace the content of the textbox with the string buffer, to
        /// remove any hypothesis text that may be present.
        /// </summary>
        /// <param name="sender">The Recognition session that generated this result</param>
        /// <param name="args">Details about the recognized speech</param>
        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            // We may choose to discard content that has low confidence, as that could indicate that we're picking up
            // noise via the microphone, or someone could be talking out of earshot.
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
                args.Result.Confidence == SpeechRecognitionConfidence.High)
            {
                dictatedTextBuilder.Append(args.Result.Text + " ");

                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    discardedTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                    dictationTextBox.Text = dictatedTextBuilder.ToString();
                });
            }
            else
            {
                // In some scenarios, a developer may choose to ignore giving the user feedback in this case, if speech
                // is not the primary input mechanism for the application.
                // Here, just remove any hypothesis text by resetting it to the last known good.
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    dictationTextBox.Text = dictatedTextBuilder.ToString();
                    string discardedText = args.Result.Text;
                    if (!string.IsNullOrEmpty(discardedText))
                    {
                        discardedText = discardedText.Length <= 25 ? discardedText : (discardedText.Substring(0, 25) + "...");

                        discardedTextBlock.Text = "Discarded due to low/rejected Confidence: " + discardedText;
                        discardedTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                });
            }
        }

        /// <summary>
        /// Provide feedback to the user based on whether the recognizer is receiving their voice input.
        /// </summary>
        /// <param name="sender">The recognizer that is currently running.</param>
        /// <param name="args">The current state of the recognizer.</param>
        private async void SpeechRecognizer_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                // rootPage.NotifyUser(args.State.ToString(), NotifyType.StatusMessage);
            });
        }

        /// <summary>
        /// Automatically scroll the textbox down to the bottom whenever new dictated text arrives
        /// </summary>
        /// <param name="sender">The dictation textbox</param>
        /// <param name="e">Unused text changed arguments</param>
        private void dictationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            // Save today's notes to the server.
            DiaryDailyNotes notes = new DiaryDailyNotes()
            {
                DateString = datePicker.Date.ToString("dd-MMM-yy"),
                NotesForTheDay = currentDayNotes.Text,
                IsBookmarked = false
            };

            await DigitalDiaryHttpClient.SaveNotesForTheDayAsync(notes);
            // DigitalDiaryHttpClient.SaveNotesForTheDay(notes);
            // this.Frame.Navigate(typeof(ContinuousDictationScenario));
        }

        private void AddCurrentDictationToDailyNotes()
        {
            // Clear the dictation text box and append the dictation to the DailyNotes.
            dictationTextBox.Text = string.Empty;

            currentDayNotes.Text += dictatedTextBuilder.ToString();
            dictatedTextBuilder.Clear();
        }

        private void CleanupSpeechRecognizer()
        {
            if (speechRecognizer != null)
            {
                // cleanup prior to re-initializing this scenario.
                speechRecognizer.StateChanged -= SpeechRecognizer_StateChanged;
                speechRecognizer.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;
                speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;
                speechRecognizer.HypothesisGenerated -= SpeechRecognizer_HypothesisGenerated;

                this.speechRecognizer.Dispose();
                this.speechRecognizer = null;
            }
        }

        private void datePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            viewModel.RecomputeCurrentDayNotes();
            // var temp = viewModel.CurrentDayNotes.Result;
            /*
            currentDayNotes.Text = string.Empty;

            var datePicker = sender as DatePicker;
            if (datePicker != null)
            {
                var dateString = datePicker.Date.ToString("dd-MMM-yy");
                var notes = DigitalDiaryHttpClient.GetNotesForTheDay(datePicker.Date.ToString("dd-MMM-yy"));
                currentDayNotes.Text = notes;
            }
            //*/
        }

        private void acceptPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            currentDayNotes.Text += dictationTextBox.Text;
            dictationTextBox.Text = string.Empty;
            dictatedTextBuilder.Clear();
        }

        private void clearPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            dictationTextBox.Text = string.Empty;
            dictatedTextBuilder.Clear();
        }
    }
}
;