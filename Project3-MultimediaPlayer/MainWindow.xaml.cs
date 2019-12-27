
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
using TagLib;

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
        int ShuffleMode = 0; //0: nonShuffle, 1:Shuffle
        int RepeatMode = 0; //0: Forever, 1: 1 song

        public MainWindow()
        {
            InitializeComponent();
            _player.MediaEnded += _player_MediaEnded;
            _player.MediaOpened += _player_MediaOpened;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);

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
                    if (ShuffleMode == 1)
                    {
                        var random = new Random();
                        _lastIndex = random.Next(_fullPaths.Count());
                    }
                    else _lastIndex = 0;
                    PlaySelectedIndex(_lastIndex);
                }
            }
        }


        private void PlaySelectedIndex(int i)
        {
            try
            {
                string filename = _fullPaths[i].FullName;
                _player.Open(new Uri(filename, UriKind.Absolute));

                var detailsong = TagLib.File.Create(filename);
                var artists = detailsong.Tag.Artists;
                //var picture = detailsong.Tag.Pictures[0];
                try
                {
                    var image = detailsong.Tag.Pictures[0];
                    MemoryStream ms = new MemoryStream(image.Data.Data);
                    ms.Seek(0, SeekOrigin.Begin);
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    imageofSong.Source = bitmap;
                }
                catch{
                    imageofSong.Source = new BitmapImage(new Uri(@"/Images/compact-disc.png", UriKind.Relative));
                }

                string artist = "";
                foreach (var item in artists)
                {
                    artist = artist + item + " ";
                }

                nameofSong.Content = _fullPaths[i].Name;
                nameofArtist.Content = artist;
                _player.Play();
                _isPlaying = true;
                _timer.Start();
            }catch
            {
                MessageBox.Show("Some errors occurred. Close app and try again!", "Sorry about that");
            }
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

            if (RepeatMode == 1) // repeat 1 song
            {
                if (_playedList.Count() > 0) 
                    _playedList.Clear(); 
                return currentPlay;
            }

            if(ShuffleMode == 0) //non shuffle
            {
                nextsong = currentPlay + 1 ;     
                if(RepeatMode == 0) //repeat forever
                {
                    if (nextsong >= _fullPaths.Count())
                    {
                        nextsong = 0;
                    }
                }
                if(_playedList.Count() > 0) _playedList.Clear();
            }
            else // shuffle
            {
                
                if(ShuffleMode == 1)
                {
                    _playedList.Add(currentPlay);

                    if (_playedList.Count() == _fullPaths.Count())
                    {
                        _playedList.Clear();
                    }

                    
                    for (int j = 2; j >= 0; j--)
                    {
                        Random random = new Random();
                        int playnext = random.Next(100);
                        playnext %= _fullPaths.Count();
                        bool flag = isExistInList(_playedList, playnext);
                        if (!flag && playnext != currentPlay)
                        {
                            nextsong = playnext;
                            return nextsong;
                        }
                    }

                    for (int k = 1; ; k++)
                    {
                        int temp = currentPlay + k^2;
                        temp %= _fullPaths.Count();
                        if (isExistInList(_playedList, temp) == false)
                        {
                            nextsong = temp;
                            break;
                        }
                    }
                }
            }
            return nextsong;
        }

        private bool isExistInList(List <int> list, int i)
        {
            foreach(var item in list)
            {
                if (item == i) return true;
            }
            return false;
        }

        private void  timer_Tick(object sender, EventArgs e)
        {
            //await Task.Run(() => _player.NaturalDuration.TimeSpan);
            if (_player.Source != null && _player.NaturalDuration.HasTimeSpan)
            {
                var filename = _fullPaths[_lastIndex].Name;
                //var converter = new NameConverter();
                //var shortname = converter.Convert(filename, null, null, null);

                var currentPos = _player.Position.TotalSeconds;
               
               

                progessMusic.Minimum = 0;
                progessMusic.Maximum = _player.NaturalDuration.TimeSpan.TotalSeconds;
                progessMusic.Value = currentPos;


                timeNow.Content = _player.Position.ToString(@"mm\:ss");
                totalTime.Content = _player.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
            }
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
                foreach (var file in screen.FileNames)
                {
                    var info = new FileInfo(file);
                    if (!_fullPaths.Contains(info))
                    {
                        _fullPaths.Add(info);
                    }
                }
                playlistListBox.ItemsSource = _fullPaths ;
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

        private void progessMusic_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            var pos = Convert.ToInt32(progessMusic.Value);
            var newDuration = new TimeSpan(0, 0, pos);
            _player.Position = newDuration;
        }

        private void progessMusic_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var pos = Convert.ToInt32(progessMusic.Value);
            var newDuration = new TimeSpan(0, 0, pos);
            _player.Position = newDuration;
        }

        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            if(ShuffleMode == 0) //current: non shuffle
            {
                ShuffleMode = 1; //switch to shuffle
                shuffleImage.Source = new BitmapImage(new Uri(@"/Images/shuffle1.png", UriKind.Relative));
            }
            else //current: shuffle
            {
                ShuffleMode = 0; // switch to nonShuffle
                shuffleImage.Source = new BitmapImage(new Uri(@"/Images/shuffle.png", UriKind.Relative));
            }
        }

        private void repeatButton_Click(object sender, RoutedEventArgs e)
        {
            if(RepeatMode == 0 ) //current: repeat forever
            {
                RepeatMode = 1; // switch to repeat 1 song

                repeatImage.Source = new BitmapImage(new Uri(@"/Images/repeat1.png", UriKind.Relative));
            }
            else // current: repeat 1 song
            {
                RepeatMode = 0; //switch to repeat forever
                repeatImage.Source = new BitmapImage(new Uri(@"/Images/repeat.png", UriKind.Relative));
            }
        }
    }
}

