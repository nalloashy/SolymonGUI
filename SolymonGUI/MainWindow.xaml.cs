using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using NAudio.Wave;
using System.Windows.Threading;
using Microsoft.VisualBasic; // For InputBox

namespace CybersecurityChatbotWPF
{
    public partial class MainWindow : Window
    {
        private class UserMemory
        {
            public string Name { get; set; }
            public List<string> DiscussedTopics { get; } = new();
            public Dictionary<string, int> TopicInterestLevel { get; } = new();
            public string FavoriteTopic { get; set; }
            public string CurrentSentiment { get; set; } = "neutral";
        }

        private class CyberTask
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime? ReminderDate { get; set; }
            public bool IsCompleted { get; set; }

            public override string ToString()
            {
                string status = IsCompleted ? "[Completed]" : "[Pending]";
                string reminder = ReminderDate.HasValue ? $" (Reminder: {ReminderDate.Value:g})" : "";
                return $"{status} {Title} - {Description}{reminder}";
            }
        }

        private UserMemory currentUser = new();
        private List<CyberTask> tasks = new();
        private string currentTopic = null;
        private int conversationDepth = 0;
        private const int MaxConversationDepth = 3;
        private readonly Random random = new();
        private DispatcherTimer reminderTimer;

        private readonly List<string> ContinuationPhrases = new()
        { "yes", "more", "explain", "details", "continue", "go on", "tell me more", "please" };

        private readonly Dictionary<string, string[]> SentimentIndicators = new()
        {
            { "positive", new[] { "great", "awesome", "thank", "helpful", "cool", "interesting", "love", "happy" }},
            { "negative", new[] { "worried", "scared", "frustrated", "angry", "annoyed", "confused", "overwhelmed", "stressed" }},
            { "curious", new[] { "why", "how", "what if", "explain", "curious", "question", "wonder", "clarify" }}
        };

        // Enhanced keyword responses with synonyms and keywords list for basic NLP
        private readonly Dictionary<string, (string audio, List<string> responses, List<string> keywords)> KeywordResponsesExtended = new(StringComparer.OrdinalIgnoreCase)
        {
            { "phishing", ("phishing.wav", new List<string> {
                "Phishing is a cyberattack where attackers trick you into revealing personal info.",
                "Phishing emails can look very convincing. Be cautious.",
                "Always verify links before clicking to avoid phishing scams."
            }, new List<string> { "phishing", "phish", "phisher", "fake email", "scam email" }) },

            { "malware", ("malware.wav", new List<string> {
                "Malware is software designed to harm or exploit your device.",
                "Always keep your antivirus updated to defend against malware.",
                "Be cautious about what you download to prevent malware infections."
            }, new List<string> { "malware", "virus", "trojan", "spyware", "ransomware" }) }
        };

        public MainWindow()
        {
            InitializeComponent();
            BotNameLabel.Content = "CYBERSECURITY AWARENESS BOT";
            AppendChat("Bot: Hello! What's your name?");
            reminderTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
            reminderTimer.Tick += ReminderTimer_Tick;
            reminderTimer.Start();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string input = UserInputBox.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            AppendChat($"You: {input}");
            UserInputBox.Clear();

            ProcessInput(input);
        }

        private void ProcessInput(string input)
        {
            if (string.IsNullOrEmpty(currentUser.Name))
            {
                currentUser.Name = input;
                AppendChat($"Bot: Welcome, {currentUser.Name}! Ask me about cybersecurity topics like phishing or malware.");
                return;
            }

            string normalizedInput = input.ToLower();
            currentUser.CurrentSentiment = DetectSentiment(normalizedInput);

            if (normalizedInput == "exit")
            {
                AppendChat($"Bot: Goodbye {currentUser.Name}, stay safe online!");
                Application.Current.Shutdown();
                return;
            }

            // Use enhanced basic NLP keyword detection to detect intent
            string detectedIntent = DetectIntent(normalizedInput);

            if (detectedIntent != null)
            {
                currentTopic = detectedIntent;
                conversationDepth = 1;

                if (!currentUser.DiscussedTopics.Contains(detectedIntent))
                    currentUser.DiscussedTopics.Add(detectedIntent);

                if (currentUser.TopicInterestLevel.ContainsKey(detectedIntent))
                    currentUser.TopicInterestLevel[detectedIntent]++;
                else
                    currentUser.TopicInterestLevel[detectedIntent] = 1;

                currentUser.FavoriteTopic = currentUser.TopicInterestLevel.OrderByDescending(x => x.Value).First().Key;

                var data = KeywordResponsesExtended[detectedIntent];
                PlayAudio(data.audio);
                AppendChat($"Bot: {GetResponseForSentiment(data.responses, currentUser.CurrentSentiment)}");
            }
            else if (ContinuationPhrases.Any(p => normalizedInput.Contains(p)) && currentTopic != null)
            {
                ContinueConversation();
            }
            else
            {
                AppendChat("Bot: I didn't understand. Please ask about topics like phishing or malware.");
            }
        }

        // Detect intent using keyword matching with regex word boundary for better accuracy
        private string DetectIntent(string input)
        {
            foreach (var intent in KeywordResponsesExtended)
            {
                foreach (var keyword in intent.Value.keywords)
                {
                    // Regex pattern for word boundary
                    var pattern = $@"\b{Regex.Escape(keyword)}\b";
                    if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                        return intent.Key;
                }
            }
            return null;
        }

        private void ContinueConversation()
        {
            if (conversationDepth >= MaxConversationDepth)
            {
                AppendChat($"Bot: We've covered {currentTopic} well! Ask about something else?");
                currentTopic = null;
                conversationDepth = 0;
                return;
            }

            var data = KeywordResponsesExtended[currentTopic];
            conversationDepth++;
            AppendChat($"Bot: {GetResponseForSentiment(data.responses, currentUser.CurrentSentiment)}");
        }

        private string DetectSentiment(string input)
        {
            foreach (var kvp in SentimentIndicators)
            {
                if (kvp.Value.Any(term => input.Contains(term)))
                    return kvp.Key;
            }
            return "neutral";
        }

        private string GetResponseForSentiment(List<string> responses, string sentiment)
        {
            return sentiment switch
            {
                "positive" => responses[0] + " I'm glad you're interested!",
                "negative" => responses[1] + " Don't worry, I'm here to help.",
                "curious" => responses[2] + " Good question!",
                _ => responses[random.Next(responses.Count)]
            };
        }

        private void AppendChat(string text)
        {
            ChatTextBox.AppendText(text + Environment.NewLine);
            ChatTextBox.ScrollToEnd();
        }

        private void PlayAudio(string fileName)
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio", fileName);
            if (System.IO.File.Exists(path))
            {
                new Thread(() =>
                {
                    using var audioFile = new AudioFileReader(path);
                    using var outputDevice = new WaveOutEvent();
                    outputDevice.Init(audioFile);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                        Thread.Sleep(100);
                }).Start();
            }
        }

        // Task Assistant
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var title = Interaction.InputBox("Enter task title:", "Add Task", "");
            if (string.IsNullOrWhiteSpace(title)) return;

            var description = Interaction.InputBox("Enter task description:", "Add Task", "");
            DateTime? reminderDate = null;

            var reminderInput = Interaction.InputBox("Set reminder (days from now, or leave blank):", "Add Task", "");
            if (int.TryParse(reminderInput, out int days))
            {
                reminderDate = DateTime.Now.AddDays(days);
            }

            var task = new CyberTask
            {
                Title = title,
                Description = description,
                ReminderDate = reminderDate,
                IsCompleted = false
            };

            tasks.Add(task);
            RefreshTaskList();
            AppendChat($"Bot: Task '{title}' added. {(reminderDate.HasValue ? $"I'll remind you on {reminderDate.Value:g}." : "")}");
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedItem is CyberTask task)
            {
                tasks.Remove(task);
                RefreshTaskList();
                AppendChat($"Bot: Task '{task.Title}' deleted.");
            }
        }

        private void MarkCompleted_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedItem is CyberTask task)
            {
                task.IsCompleted = true;
                RefreshTaskList();
                AppendChat($"Bot: Task '{task.Title}' marked as completed.");
            }
        }

        private void RefreshTaskList()
        {
            TaskListBox.ItemsSource = null;
            TaskListBox.ItemsSource = tasks;
        }

        private void ReminderTimer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            foreach (var task in tasks.Where(t => t.ReminderDate.HasValue && !t.IsCompleted && t.ReminderDate.Value <= now).ToList())
            {
                AppendChat($"Bot: Reminder - {task.Title}: {task.Description}");
                task.ReminderDate = null;
            }
            RefreshTaskList();
        }

        // Button to launch quiz window (unchanged from your previous code)
        private void LaunchQuizButton_Click(object sender, RoutedEventArgs e)
        {
            var quizWindow = new QuizWindow();
            quizWindow.Owner = this;
            quizWindow.ShowDialog();
        }
    }
}

