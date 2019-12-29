
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
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using TagLib;
using System.Windows.Media.Animation;

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
        Stack<int> _previousList = new Stack<int>();
        private IKeyboardMouseEvents _hook;
        int ShuffleMode = 0; //0: nonShuffle, 1:Shuffle
        int RepeatMode = 0; //0: Forever, 1: 1 song
        bool _isDragProgressBar = false;

        public MainWindow()
        {
            InitializeComponent();
            _player.MediaEnded += _player_MediaEnded;
            _player.MediaOpened += _player_MediaOpened;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(750);

            _timer.Tick += timer_Tick;

            // Dang ky su kien hook
            _hook = Hook.GlobalEvents();
            _hook.KeyUp += KeyUp_hook;
        }

        private void _player_MediaOpened(object sender, EventArgs e)
        {

        }

        bool firstTimePlay = true;
        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            SB.Begin();
            if (playlistListBox.SelectedIndex != _lastIndex || firstTimePlay == true)
            {
                firstTimePlay = false;
                _previousList.Push(_lastIndex);
                PlayPause_Image.Source = new BitmapImage(new Uri(@"/Images/pause.png", UriKind.Relative));
                if (_fullPaths.Count() > 0)
                {
                    if (playlistListBox.SelectedIndex >= 0)
                    {
                        _lastIndex = playlistListBox.SelectedIndex;
                        if (_player.Position.TotalSeconds > 0)
                        {
                            _player.Play();
                            _isPlaying = true;
                            _timer.Start();

                        }
                        else PlaySelectedIndex(_lastIndex);
                    }
                    else
                    {
                        if (ShuffleMode == 1)
                        {
                            var random = new Random();
                            _lastIndex = random.Next(_fullPaths.Count());
                        }
                        //  else _lastIndex = 0;
                        if (_player.Position.TotalSeconds > 0)
                        {
                            _player.Play();
                            _isPlaying = true;
                            _timer.Start();
                        }
                        else PlaySelectedIndex(_lastIndex);
                    }
                }
            }
            else
            {
                pause();
            }
        }

        private void LoadDetailSong(int i)
        {
            try
            {
                string filename = _fullPaths[i].FullName;
                var detailsong = TagLib.File.Create(filename);
                var artists = detailsong.Tag.Artists;
                //var picture = detailsong.Tag.Pictures[0];
                string artist = "";
                foreach (var item in artists)
                {
                    artist = artist + item + " ";
                }

                nameofSong.Content = _fullPaths[i].Name;
                nameofArtist.Content = artist;
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
                catch
                {
                    imageofSong.Source = new BitmapImage(new Uri(@"/Images/compact-disc.png", UriKind.Relative));
                }
                timeNow.Content = _player.Position.ToString(@"mm\:ss");
                totalTime.Content = detailsong.Properties.Duration.ToString(@"mm\:ss");
                progessMusic.Minimum = 0;
                progessMusic.Maximum = detailsong.Properties.Duration.TotalSeconds;
            }
            catch
            { }
        }

        private void PlaySelectedIndex(int i)
        {
            try
            {
                string filename = _fullPaths[i].FullName;
                _player.Open(new Uri(filename, UriKind.Absolute));
                

                LoadDetailSong(i);
                _player.Play();
                _isPlaying = true;
                _timer.Start();

            }
            catch
            {
                MessageBox.Show("Some errors occurred. Close app and try again!", "Sorry about that");
            }
        }


        private void _player_MediaEnded(object sender, EventArgs e)
        {
            int i = PlayNextSong(_lastIndex);
            _lastIndex = i;
            
            PlaySelectedIndex(i);
            _previousList.Push(_lastIndex);
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

            if (ShuffleMode == 0) //non shuffle
            {
                nextsong = currentPlay + 1;
                if (RepeatMode == 0) //repeat forever
                {
                    if (nextsong >= _fullPaths.Count())
                    {
                        nextsong = 0;
                    }
                }
                if (_playedList.Count() > 0) _playedList.Clear();
            }
            else // shuffle
            {

                if (ShuffleMode == 1)
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
                            playlistListBox.SelectedIndex = nextsong;
                            return nextsong;
                        }
                    }

                    for (int k = 1; ; k++)
                    {
                        int temp = currentPlay + k ^ 2;
                        temp %= _fullPaths.Count();
                        if (isExistInList(_playedList, temp) == false)
                        {
                            nextsong = temp;
                            break;
                        }
                    }
                }
            }
            playlistListBox.SelectedIndex = nextsong;
            return nextsong;
        }

        private bool isExistInList(List<int> list, int i)
        {
            foreach (var item in list)
            {
                if (item == i) return true;
            }
            return false;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //await Task.Run(() => _player.NaturalDuration.TimeSpan);
            if (_player.Source != null)
            {
                var filename = _fullPaths[_lastIndex].Name;
                //var converter = new NameConverter();
                //var shortname = converter.Convert(filename, null, null, null);

                var currentPos = _player.Position.TotalSeconds;



                progessMusic.Minimum = 0;
                //progessMusic.Maximum = _player.NaturalDuration.TimeSpan.TotalSeconds;
                if (!_isDragProgressBar)
                {
                    progessMusic.Value = currentPos;
                }


                timeNow.Content = _player.Position.ToString(@"mm\:ss");
                //totalTime.Content = _player.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
            }
        }

        bool _isPlaying = false;

        private void pause()
        {
            if (_isPlaying)
            {
                SB.Pause();
                _player.Pause();
                PlayPause_Image.Source = new BitmapImage(new Uri(@"/Images/play.png", UriKind.Relative));
            }
            else
            {
                SB.Resume();
                _player.Play();
                PlayPause_Image.Source = new BitmapImage(new Uri(@"/Images/pause.png", UriKind.Relative));
            }
            _isPlaying = !_isPlaying;
        }

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
                playlistListBox.ItemsSource = _fullPaths;
            }
        }
        Storyboard SB;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
             SB= (Storyboard)FindResource("Storyboard");
            try
            {
                var filename = "playlist.txt";
                StreamReader reader = new StreamReader(filename);
                RepeatMode = int.Parse(reader.ReadLine());
                if(RepeatMode == 1)
                {
                    repeatImage.Source =new BitmapImage(new Uri(@"/Images/repeat1.png", UriKind.Relative));
                }
                ShuffleMode = int.Parse(reader.ReadLine());
                if(ShuffleMode == 1)
                {
                    shuffleImage.Source = new BitmapImage(new Uri(@"/Images/shuffle1.png", UriKind.Relative));
                }
                int currentPlayIndex = int.Parse(reader.ReadLine());
                _lastIndex = currentPlayIndex;
                var pos = int.Parse(reader.ReadLine());
                
                string line = reader.ReadLine();
                while (line != null)
                {
                    var fileInfo = new FileInfo(line);
                    _fullPaths.Add(fileInfo);
                    line = reader.ReadLine();
                }
                reader.Close();
                playlistListBox.SelectedIndex = currentPlayIndex;
                playlistListBox.ItemsSource = _fullPaths;
                if (pos >= 0)
                {
                    _player.Open(new Uri(_fullPaths[_lastIndex].FullName, UriKind.Absolute));
                    _player.Position = new TimeSpan(0, 0, pos);
                    
                    LoadDetailSong(_lastIndex);

                    progessMusic.Value = pos;
 
                }
            }
            catch
            {
                MessageBox.Show("Loaded fail!");
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void KeyUp_hook(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            RoutedEventArgs a = new RoutedEventArgs();
            if (e.Control && e.Shift && (e.KeyCode == Keys.A)) //next song
            {
                nextButton_Click(sender, a);
            }
            if (e.Control && e.Shift && (e.KeyCode == Keys.S)) //play
            {
                playButton_Click(sender, a);
            }
            if (e.Control && e.Shift && (e.KeyCode == Keys.Q)) //previous song
            {
                previous_Button_Click(sender, a);
            }
            if (e.Control && e.Shift && (e.KeyCode == Keys.W)) //stop
            {
                stopButton_Click(sender, a);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
            
            var filename = "playlist.txt";
            var writer = new StreamWriter(filename);
            

            if (_fullPaths.Count > 0)
            {
                if (_lastIndex == -1)
                {
                    _lastIndex = 0;
                }
                writer.WriteLine(RepeatMode);
                writer.WriteLine(ShuffleMode);
                writer.WriteLine(_lastIndex);
                writer.WriteLine((int)_player.Position.TotalSeconds);
                foreach (var item in _fullPaths)
                {
                    writer.WriteLine(item);
                }
            }
            writer.Close();
            _hook.KeyUp -= KeyUp_hook;
            _hook.Dispose();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_fullPaths.Count() > 0 && _lastIndex != -1)
            {
                _isPlaying = true;
                
                _player_MediaEnded(sender, e);
            }
        }
        
        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
           
            SB.Stop();

            
            PlayPause_Image.Source = new BitmapImage(new Uri(@"/Images/play.png", UriKind.Relative));
            firstTimePlay = true;
            _player.Stop();
        }


        private void progessMusic_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            var pos = Convert.ToInt32(progessMusic.Value);
            var newDuration = new TimeSpan(0, 0, pos);
            _player.Position = newDuration;
            _isDragProgressBar = false;
        }

        private void progessMusic_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var pos = Convert.ToInt32(progessMusic.Value);
            var newDuration = new TimeSpan(0, 0, pos);
            _player.Position = newDuration;
        }

        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShuffleMode == 0) //current: non shuffle
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
            if (RepeatMode == 0) //current: repeat forever
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

        private void AddPlayList_Button_Click(object sender, RoutedEventArgs e)
        {
            // save file dialog
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog()
            {
                Title = "Save text Files",
                CheckPathExists = true,
                DefaultExt = "txt",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 2,
                RestoreDirectory = true,
            };

            // get filename
            string filename = "";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filename = saveFileDialog.FileName;

                var writer = new StreamWriter(filename);
                if (_lastIndex == -1)
                {
                    writer.WriteLine(0);
                }
                else
                {
                    writer.WriteLine(_lastIndex);
                }
                foreach (var item in _fullPaths)
                {
                    writer.WriteLine(item);
                }

                writer.Close();
            }
        }

        private void LoadPlayList_Button_Click(object sender, RoutedEventArgs e)
        {
            string line = "";
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "txt files (*.txt)|*.txt";
            openFile.FilterIndex = 1;
            openFile.RestoreDirectory = true;
            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    var reader = new StreamReader(openFile.FileName);
                    _fullPaths.Clear();
                    stopButton_Click(sender, e);
                    _isPlaying = false;
                    int currentPlayIndex = int.Parse(reader.ReadLine());
                    _lastIndex = currentPlayIndex;
                    line = reader.ReadLine();
                    while (line != null)
                    {
                        var fileInfo = new FileInfo(line);
                        _fullPaths.Add(fileInfo);
                        line = reader.ReadLine();
                    }
                    reader.Close();
                    playlistListBox.SelectedIndex = currentPlayIndex;
                }
                catch
                {
                    MessageBox.Show("Make sure you open correct your playlist", "Cant load your playlist");
                }
            }
        }

        private void playlistListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            playlistListBox.Dispatcher.BeginInvoke(
                    (Action)(() =>
                    {
                        playlistListBox.UpdateLayout();
                        if (playlistListBox.SelectedItem !=
                            null)
                            playlistListBox.ScrollIntoView(
                                playlistListBox.SelectedItem);
                    }));

            if (_isPlaying)
            {
                PlayPause_Image.Source = new BitmapImage(new Uri(@"/Images/pause.png", UriKind.Relative));
            }
            else
            {
                PlayPause_Image.Source = new BitmapImage(new Uri(@"/Images/play.png", UriKind.Relative));
            }
        }

        private void previous_Button_Click(object sender, RoutedEventArgs e)
        {
            if (_fullPaths.Count() > 0 && _lastIndex != -1)
            {
                
                _isPlaying = true;
               
                int i = PlayPreviousSong(_lastIndex);
                _lastIndex = i;

                PlaySelectedIndex(i);
            }
        }

        private int PlayPreviousSong(int currentPlay)
        {
            int nextsong = currentPlay;

            if (RepeatMode == 1) // repeat 1 song
            {
               
                return currentPlay;
            }

            if (ShuffleMode == 0) //non shuffle
            {
                nextsong = currentPlay - 1;
                if (RepeatMode == 0) //repeat forever
                {
                    if (nextsong < 0)
                    {
                        nextsong = _fullPaths.Count()-1;
                    }
                }
                
            }
            else // shuffle
            {

                if (ShuffleMode == 1)
                {
                    if (_previousList.Count() > 1)
                    {
                        _previousList.Pop();
                        nextsong = _previousList.Peek();
                    }
                    else
                    {
                       nextsong = PlayNextSong(currentPlay);
                    }
                }
            }
            playlistListBox.SelectedIndex = nextsong;
            return nextsong;
        }

        private void progessMusic_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _isDragProgressBar = true;
        }

        private void playlistListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var src = VisualTreeHelper.GetParent((DependencyObject)e.OriginalSource);
            var srcType = src.GetType();
            if (srcType == typeof(System.Windows.Controls.ListBoxItem) || srcType == typeof(GridViewRowPresenter))
            {
                PlaySelectedIndex(playlistListBox.SelectedIndex);
                _lastIndex = playlistListBox.SelectedIndex;
            }
           
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

            var selectedIndex = playlistListBox.SelectedIndex;
            if (selectedIndex != _lastIndex)
            {
                if (selectedIndex < _lastIndex)
                {
                    _lastIndex--;
                    _fullPaths.Remove(_fullPaths[selectedIndex]);
                }
                else
                {
                    _fullPaths.Remove(_fullPaths[selectedIndex]);
                }
            }
            else
            {
                MessageBox.Show("You cant remove a song is playing");
            }
        }
    }
}

