using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;

namespace SoundPlayerApp
{
    public partial class MainWindow : Window
    {
        // Импортируем mciSendString из winmm.dll
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string command, StringBuilder returnValue, int returnLength, IntPtr winHandle);

        private DispatcherTimer timer = new DispatcherTimer();
        private BitmapImage gifBitmap;
        private BitmapDecoder decoder;
        private int currentFrame = 0;
        private string currentFilePath = "";

        public MainWindow()
        {
            InitializeComponent();
            PlaybackSlider.PreviewMouseLeftButtonDown += PlaybackSlider_PreviewMouseLeftButtonDown;
            PlaybackSlider.PreviewMouseLeftButtonUp += PlaybackSlider_PreviewMouseLeftButtonUp;
            gifBitmap = new BitmapImage(new Uri("C:\\Users\\makak\\source\\repos\\SoundPlayerWPF\\SoundPlayerWPF\\papich_dance.gif"));
            decoder = BitmapDecoder.Create(gifBitmap.UriSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            PlaybackSlider.PreviewMouseMove += PlaybackSlider_PreviewMouseMove;
            timer.Interval = TimeSpan.FromMilliseconds(100); // Обновление каждую секунду
            timer.Tick += Timer_Tick;
        }

        private void PlaySound(string filePath)
        {
            if (filePath==currentFilePath)
            {
                
                string resumeCommand = "resume MyAudio";
                mciSendString(resumeCommand, null, 0, IntPtr.Zero);
            }
            if (filePath=="")
            {
                
                string resumeCommand = "resume MyAudio";
                mciSendString(resumeCommand, null, 0, IntPtr.Zero);
            }
            else
            {
                string stopCommand = "stop MyAudio";
                mciSendString(stopCommand, null, 0, IntPtr.Zero);
                string command = "close MyAudio";
                mciSendString(command, null, 0, IntPtr.Zero);

                currentFilePath = filePath;
                
                string openCommand = $"open \"{currentFilePath}\" type mpegvideo alias MyAudio";
                mciSendString(openCommand, null, 0, IntPtr.Zero);

            
                string playCommand = "play MyAudio"; 
                mciSendString(playCommand, null, 0, IntPtr.Zero);
            }

            timer.Start();
        }

        private void StopSound()
        {
            
            string stopCommand = "stop MyAudio";
            mciSendString(stopCommand, null, 0, IntPtr.Zero);
            timer.Stop();
        }

        private void OnPlayButtonClicked(object sender, RoutedEventArgs e)
        {
            PlaySound(""); // Используем текущий путь к файлу
        }

        private void OnStopButtonClicked(object sender, RoutedEventArgs e)
        {
            StopSound();
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Выберите файл",
                Filter = "Audio Files|*.mp3;*.wav;*.wma" 
            };

            if (openFileDialog.ShowDialog() == true)
            {
                PlaySound(openFileDialog.FileName);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            StringBuilder currentPosition = new StringBuilder(128);
            mciSendString("status MyAudio position", currentPosition, currentPosition.Capacity, IntPtr.Zero);

            currentFrame += 1;
            if (currentFrame >= decoder.Frames.Count && decoder.ToString() == "file:///C:/Users/makak/source/repos/SoundPlayerWPF/SoundPlayerWPF/papich_dance.gif")
            {
                gifBitmap = new BitmapImage(new Uri("C:\\Users\\makak\\Downloads\\1662223933382.gif"));
                decoder = BitmapDecoder.Create(gifBitmap.UriSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                currentFrame = 0;
            }
            GifImage.Source = decoder.Frames[currentFrame];
            if (int.TryParse(currentPosition.ToString(), out int positionInMs))
            {
                TimeSpan currentTime = TimeSpan.FromMilliseconds(positionInMs);
                CurrentTime.Text = $"Сейчас на: {currentTime.Minutes}:{currentTime.Seconds}";

                PlaybackSlider.Value = positionInMs / 1000.0; 
            }
            else
            {
                CurrentTime.Text = "Ошибка получения времени.";
            }
        }

        private bool isSliderChanging = false; 


        private void PlaybackSlider_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isSliderChanging)
            {
               
                Point position = e.GetPosition(PlaybackSlider);
                double newValue = (position.X / PlaybackSlider.ActualWidth) * (PlaybackSlider.Maximum - PlaybackSlider.Minimum) + PlaybackSlider.Minimum;
                double possition = PlaybackSlider.Value = Math.Max(PlaybackSlider.Minimum, Math.Min(newValue, PlaybackSlider.Maximum));
                string setPositionCommand = $"seek MyAudio to {(possition * 1000)}"; 
                mciSendString(setPositionCommand, null, 0, IntPtr.Zero);

                string playCommand = "play MyAudio from " + (possition * 1000);
                mciSendString(playCommand, null, 0, IntPtr.Zero);
            }
        }


        private void PlaybackSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isSliderChanging = true;
        }

     
        private void PlaybackSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isSliderChanging = false; 
        }



    }
}
