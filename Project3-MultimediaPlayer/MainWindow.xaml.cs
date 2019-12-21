
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
using MessageBox = System.Windows.MessageBox;

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
        List<int> _playedList = new List<int>(); 
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
            if (_fullPaths.Count() > 0)
            {
                if (playlistListBox.SelectedIndex >= 0)
                {
                    _lastIndex = playlistListBox.SelectedIndex;
                    PlaySelectedIndex(_lastIndex);
                }
                else
                {
                    _lastIndex = 0;
                    PlaySelectedIndex(_lastIndex);
                }
            }
        }

        private void PlaySelectedIndex(int i)
        {
           

            string filename = _fullPaths[i].FullName;
            _player.Open(new Uri(filename, UriKind.Absolute));
            var converter = new NameConverter();
            var shortname = converter.Convert(filename, null, null, null);
        
            nowPlay.Content = "Now playing: " + shortname;
            _player.Play();
            _isPlaying = true;
            _timer.Start();

        }

        private void _player_MediaEnded(object sender, EventArgs e)
        {
            
            int i = PlayNextSong(_lastIndex);
            _lastIndex = i;
            
            PlaySelectedIndex(i);
        }

        private int PlayNextSong(int currentPlay)
        {
            int nextsong = currentPlay;

            if (Song.IsChecked == true)
            {
                if (_fullPaths != null) _fullPaths.Clear(); 
                return nextsong; }

            if(nonShuffle.IsChecked == true)
            {
                nextsong = currentPlay + 1 ;     
                if(Forever.IsChecked == true)
                {
                    if (nextsong >= _fullPaths.Count())
                    {
                        nextsong = 0;
                    }
                }
                if(_fullPaths !=null) _fullPaths.Clear();
            }
            else
            {
                
                if(Forever.IsChecked == true)
                {
                    if (_playedList.Count() == _fullPaths.Count())
                    {
                        _playedList.Clear();
                    }
                    _playedList.Add(currentPlay);
                    Random random = new Random();
                    int playnext = 0;
                    for (int j = 2; j >= 0; j--)
                    {
                        playnext = random.Next(1000);
                        playnext %= _fullPaths.Count();
                        bool flag = isExistInList(_playedList, playnext);
                        if (flag)
                        {
                            j--;
                        }
                        else
                        {
                            _playedList.Add(playnext);
                            nextsong = playnext;
                            return nextsong;

                        }
                    }

                    for (int k = 1; ; k++)
                    {
                        int temp = playnext + k;
                        temp %= _fullPaths.Count();
                        if (isExistInList(_playedList, temp) == false)
                        {
                            _playedList.Add(playnext);
                            nextsong = playnext;
                            break;
                        }
                    }


                    
                }
            }
            return nextsong;
        }

        private bool isExistInList(List<int> list, int i)
        {
            foreach(var item in list)
            {
                if (item == i) return true;
            }
            return false;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (_player.Source != null)
            {
                var filename = _fullPaths[_lastIndex].Name;
                var converter = new NameConverter();
                var shortname = converter.Convert(filename, null, null, null);

                //var currentPos = _player.Position.ToString(@"mm\:ss");
                //var duration = _player.NaturalDuration.TimeSpan.ToString(@"mm\:ss");


                //Title = String.Format($"{currentPos} / {duration} - {shortname}");
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
            screen.Multiselect = true;
            screen.Filter = "Sound files | *.mp3; *.wma; *.MP3";
            if (screen.ShowDialog() == true)
            {
                foreach (String file in screen.FileNames)
                {
                    var info = new FileInfo(file);
                    _fullPaths.Add(info);
                }
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

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if(_fullPaths.Count() > 0 && _lastIndex != -1)
                _player_MediaEnded(sender, e);
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            _player.Stop();
        }

        private void newPlayList_Button_Click(object sender, RoutedEventArgs e)
        {
            var newplaylist = playlistListBox.SelectedItems;

            var filename = "playlist1.txt";
            var writer = new StreamWriter(filename);

            if(newplaylist!= null)
            {
                foreach(var item in newplaylist)
                {
                    writer.WriteLine(item);
                }
            }
            writer.Close();
        }
    }
}

