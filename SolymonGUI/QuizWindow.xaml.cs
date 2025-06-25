using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CybersecurityChatbotWPF
{
    public partial class QuizWindow : Window
    {
        private class QuizQuestion
        {
            public string Question { get; set; }
            public List<string> Options { get; set; } // For multiple choice; for true/false, just 2 options
            public int CorrectOptionIndex { get; set; }
            public string Explanation { get; set; }
            public bool IsTrueFalse => Options.Count == 2 &&
                                       (Options.Contains("True") && Options.Contains("False"));
        }

        private readonly List<QuizQuestion> questions = new()
        {
            new QuizQuestion
            {
                Question = "What is phishing?",
                Options = new List<string> { "A type of cyberattack that tricks users into giving personal info", "A method of encrypting data", "A firewall feature", "An antivirus software" },
                CorrectOptionIndex = 0,
                Explanation = "Phishing is a cyberattack where attackers impersonate trustworthy entities to steal personal information."
            },
            new QuizQuestion
            {
                Question = "True or False: You should use the same password for multiple accounts to make it easier to remember.",
                Options = new List<string> { "True", "False" },
                CorrectOptionIndex = 1,
                Explanation = "False. Using unique passwords for each account helps protect you if one password is compromised."
            },
            new QuizQuestion
            {
                Question = "What is the best way to create a strong password?",
                Options = new List<string> { "Use your birthdate", "Use a mix of letters, numbers, and symbols", "Use 'password123'", "Use your pet's name" },
                CorrectOptionIndex = 1,
                Explanation = "Strong passwords use a combination of letters, numbers, and symbols to make them hard to guess."
            },
            new QuizQuestion
            {
                Question = "True or False: Public Wi-Fi is always safe to use for online banking.",
                Options = new List<string> { "True", "False" },
                CorrectOptionIndex = 1,
                Explanation = "False. Public Wi-Fi can be insecure and prone to attacks; avoid sensitive activities on it."
            },
            new QuizQuestion
            {
                Question = "What does two-factor authentication (2FA) do?",
                Options = new List<string> { "Adds a second password", "Uses two different devices to login", "Requires two steps to verify identity", "None of the above" },
                CorrectOptionIndex = 2,
                Explanation = "2FA adds an extra layer of security by requiring two different forms of verification."
            },
            new QuizQuestion
            {
                Question = "True or False: Clicking on unknown email links can lead to malware infections.",
                Options = new List<string> { "True", "False" },
                CorrectOptionIndex = 0,
                Explanation = "True. Malicious links can install malware or steal your data."
            },
            new QuizQuestion
            {
                Question = "What is social engineering in cybersecurity?",
                Options = new List<string> { "A way to build software", "Manipulating people to gain confidential info", "Designing secure networks", "None of the above" },
                CorrectOptionIndex = 1,
                Explanation = "Social engineering tricks people into revealing confidential information."
            },
            new QuizQuestion
            {
                Question = "True or False: Antivirus software guarantees 100% protection from all threats.",
                Options = new List<string> { "True", "False" },
                CorrectOptionIndex = 1,
                Explanation = "False. Antivirus reduces risk but can’t guarantee complete protection."
            },
            new QuizQuestion
            {
                Question = "Which of the following is a sign of a phishing email?",
                Options = new List<string> { "Unexpected request for personal info", "Email from a known contact", "Correct grammar and spelling", "Secure website links" },
                CorrectOptionIndex = 0,
                Explanation = "Phishing emails often ask unexpectedly for personal information."
            },
            new QuizQuestion
            {
                Question = "True or False: Regularly updating your software helps keep your device secure.",
                Options = new List<string> { "True", "False" },
                CorrectOptionIndex = 0,
                Explanation = "True. Updates fix security vulnerabilities and improve protection."
            },
        };

        private int currentQuestionIndex = 0;
        private int score = 0;
        private int selectedOptionIndex = -1;

        public QuizWindow()
        {
            InitializeComponent();
            LoadQuestion();
        }

        private void LoadQuestion()
        {
            FeedbackTextBlock.Text = "";
            selectedOptionIndex = -1;
            NextButton.IsEnabled = false;

            if (currentQuestionIndex >= questions.Count)
            {
                ShowFinalScore();
                return;
            }

            var q = questions[currentQuestionIndex];
            QuestionTextBlock.Text = $"Q{currentQuestionIndex + 1}: {q.Question}";

            AnswerOptionsPanel.Children.Clear();

            for (int i = 0; i < q.Options.Count; i++)
            {
                RadioButton optionBtn = new()
                {
                    Content = q.Options[i],
                    Tag = i,
                    Margin = new Thickness(0, 0, 0, 10),
                    GroupName = "Answers"
                };
                optionBtn.Checked += OptionBtn_Checked;
                AnswerOptionsPanel.Children.Add(optionBtn);
            }
        }

        private void OptionBtn_Checked(object sender, RoutedEventArgs e)
        {
            var rb = sender as RadioButton;
            selectedOptionIndex = (int)rb.Tag;

            var q = questions[currentQuestionIndex];
            if (selectedOptionIndex == q.CorrectOptionIndex)
            {
                FeedbackTextBlock.Foreground = Brushes.Green;
                FeedbackTextBlock.Text = "Correct! " + q.Explanation;
                score++;
            }
            else
            {
                FeedbackTextBlock.Foreground = Brushes.Red;
                FeedbackTextBlock.Text = "Incorrect. " + q.Explanation;
            }
            NextButton.IsEnabled = true;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            currentQuestionIndex++;
            LoadQuestion();
        }

        private void ShowFinalScore()
        {
            QuestionTextBlock.Text = $"Quiz Complete! Your score: {score} out of {questions.Count}";

            string finalFeedback = score switch
            {
                var s when s >= 9 => "Excellent! You're a cybersecurity pro!",
                var s when s >= 6 => "Good job! You have solid cybersecurity knowledge.",
                var s when s >= 3 => "Not bad, but keep learning to stay safe online.",
                _ => "Keep learning and practicing to improve your cybersecurity skills."
            };

            FeedbackTextBlock.Text = finalFeedback;
            FeedbackTextBlock.Foreground = Brushes.DarkBlue;

            AnswerOptionsPanel.Children.Clear();
            NextButton.IsEnabled = false;
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

