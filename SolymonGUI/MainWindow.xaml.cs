using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using NAudio.Wave;

namespace CybersecurityChatbot
{
    public partial class MainWindow : Window
    {
        private readonly ChatbotEngine _chatbot = new ChatbotEngine();
        private WaveOutEvent _waveOut;

        public MainWindow()
        {
            InitializeComponent();
            InitializeChatbot();
            DisplayAsciiArt();
            FocusManager.SetFocusedElement(this, UserInput);
        }

        private void DisplayAsciiArt()
        {
            AsciiArt.Text = @"  ____        _                              
 / ___|  ___ | |_   _ _ __ ___   ___  _ __  
 \___ \ / _ \| | | | | '_ ` _ \ / _ \| '_ \ 
  ___) | (_) | | |_| | | | | | | (_) | | | |
 |____/ \___/|_|\__, |_| |_| |_|\___/|_| |_|
                 |___/                      
   CYBERSECURITY AWARENESS BOT";
        }

        private void InitializeChatbot()
        {
            _chatbot.OnResponse += (sender, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    ChatDisplay.AppendText($"BOT: {e.Message}\n\n");
                    ChatDisplay.ScrollToEnd();
                    if (!string.IsNullOrEmpty(e.AudioFile))
                        PlayAudio(e.AudioFile);
                });
            };

            _chatbot.Initialize();
            ChatDisplay.AppendText("BOT: Hello! I'm your Cybersecurity Assistant. How can I help you today?\n\n");
        }

        private void SendButton_Click(object sender, RoutedEventArgs e) => ProcessUserInput();

        private void UserInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                ProcessUserInput();
        }

        private void ProcessUserInput()
        {
            string input = UserInput.Text.Trim();
            if (!string.IsNullOrEmpty(input))
            {
                ChatDisplay.AppendText($"YOU: {input}\n\n");
                _chatbot.ProcessInput(input);
                UserInput.Clear();
            }
        }

        private void PlayAudio(string audioFile)
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio", audioFile);
                if (File.Exists(filePath))
                {
                    _waveOut?.Stop();
                    _waveOut?.Dispose();

                    var audioFileReader = new AudioFileReader(filePath);
                    _waveOut = new WaveOutEvent();
                    _waveOut.Init(audioFileReader);
                    _waveOut.Play();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Audio error: {ex.Message}");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _waveOut?.Dispose();
            base.OnClosed(e);
        }
    }
}