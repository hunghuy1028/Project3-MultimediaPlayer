﻿
using Gma.System.MouseKeyHook;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Forms;

namespace Project3_MultimediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MediaPlayer _player = new MediaPlayer();
        DispatcherTimer _timer;
        int _lastIndex = -1;
        private IKeyboardMouseEvents _hook;


        public MainWindow()
        {
            InitializeComponent();
            _player.MediaEnded += _player_MediaEnded;
            _player.MediaOpened += _player_MediaOpened;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);

            _timer.Tick += timer_Tick;

            // Dang ky su kien hook
            _hook = Hook.GlobalEvents();
            _hook.KeyUp += KeyUp_hook;
        }

        private void _player_MediaOpened(object sender, EventArgs e)
        {

        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (playlistListBox.SelectedIndex >= 0)
            {
                _lastIndex = playlistListBox.SelectedIndex;
                PlaySelectedIndex(_lastIndex);
            }
            else
            {
                System.Windows.MessageBox.Show("No file selected!");
            }
        }

        private void PlaySelectedIndex(int i)
        {

            string filename = _fullPaths[i].FullName;

            _player.Open(new Uri(filename, UriKind.Absolute));

            System.Threading.Thread.Sleep(500);
            //var duration = _player.NaturalDuration.TimeSpan;
            //var testDuration = new TimeSpan(duration.Hours, duration.Minutes, duration.Seconds - 20);
            //_player.Position = testDuration;

            _player.Play();
            _isPlaying = true;
            _timer.Start();
        }

        private void _player_MediaEnded(object sender, EventArgs e)
        {
            _lastIndex++;
            PlaySelectedIndex(_lastIndex);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(500);
            if (_player.Source != null)
            {
                var filename = _fullPaths[_lastIndex].Name;
                var converter = new NameConverter();
                var shortname = converter.Convert(filename, null, null, null);

                var currentPos = _player.Position.ToString(@"mm\:ss");
                var duration = _player.NaturalDuration.TimeSpan.ToString(@"mm\:ss");


                Title = String.Format($"{currentPos} / {duration} - {shortname}");
            }
            else
                Title = "No file selected...";
        }

        bool _isPlaying = false;

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isPlaying)
            {
                _player.Pause();
            }
            else
            {
                _player.Play();
            }
            _isPlaying = !_isPlaying;
        }

        BindingList<FileInfo> _fullPaths = new BindingList<FileInfo>();
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            var screen = new Microsoft.Win32.OpenFileDialog();
            if (screen.ShowDialog() == true)
            {
                var info = new FileInfo(screen.FileName);
                _fullPaths.Add(info);
                playlistListBox.ItemsSource = _fullPaths;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            playlistListBox.ItemsSource = _fullPaths;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void KeyUp_hook(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Control && e.Shift && (e.KeyCode == Keys.E))
            {
                //System.Windows.MessageBox.Show("Ctrl + Shift + E pressed"); ;
                _lastIndex++;
                PlaySelectedIndex(_lastIndex);
            }
        }


        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _hook.KeyUp -= KeyUp_hook;
            _hook.Dispose();

        }
    }
}

