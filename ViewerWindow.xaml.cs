using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LibVLCSharp.Shared;

namespace FreePresenter;

public partial class ViewerWindow : Window
{
    private LibVLC? _libVlc;
    private MediaPlayer? _mediaPlayer;
    private Media? _currentMedia;
    private bool _isVideo;
    private bool _isFullScreen;
    private int _screenIndex;
    private bool _isSeeking;
    private bool _isMuted;
    private DispatcherTimer? _hideTimer;
    private DispatcherTimer? _statusClearTimer;
    private DateTime _lastPositionUpdate = DateTime.MinValue;
    private const int PositionThrottleMs = 200;
    private const int ShowTransportBarThreshold = 60;

    public ViewerWindow()
    {
        InitializeComponent();
    }

    public void LoadFile(string filePath)
    {
        transportBar.Visibility = Visibility.Collapsed;
        txtStatus.Text = "";

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        _isVideo = ext is ".mp4" or ".avi" or ".mov" or ".wmv" or ".mkv" or ".webm" or ".m4v";

        if (_isVideo)
        {
            imgDisplay.Visibility = Visibility.Collapsed;
            videoView.Visibility = Visibility.Visible;
            SetStatus("Loading...");
            PlayVideo(filePath);
        }
        else
        {
            StopVideo();
            videoView.Visibility = Visibility.Collapsed;
            imgDisplay.Visibility = Visibility.Visible;
            imgDisplay.Source = LoadImageWithExifOrientation(filePath);
        }

        Title = Path.GetFileName(filePath);
    }

    private static BitmapSource LoadImageWithExifOrientation(string filePath)
    {
        var rotation = Rotation.Rotate0;

        try
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext is ".jpg" or ".jpeg" or ".tiff" or ".tif")
            {
                var decoder = BitmapDecoder.Create(
                    new Uri(filePath),
                    BitmapCreateOptions.DelayCreation,
                    BitmapCacheOption.None);

                if (decoder?.Frames[0].Metadata is BitmapMetadata metadata)
                {
                    var queryResult = metadata.GetQuery("/app1/ifd/{ushort=274}");
                    if (queryResult is ushort orientation)
                    {
                        rotation = orientation switch
                        {
                            6 => Rotation.Rotate90,
                            3 => Rotation.Rotate180,
                            8 => Rotation.Rotate270,
                            _ => Rotation.Rotate0
                        };
                    }
                }
            }
        }
        catch
        {
        }

        var img = new BitmapImage();
        img.BeginInit();
        img.CacheOption = BitmapCacheOption.OnLoad;
        img.Rotation = rotation;
        img.UriSource = new Uri(filePath);
        img.EndInit();
        return img;
    }

    private void PlayVideo(string filePath)
    {
        try
        {
            if (_libVlc == null)
            {
                _libVlc = new LibVLC();
                _mediaPlayer = new MediaPlayer(_libVlc);
                videoView.MediaPlayer = _mediaPlayer;

                _mediaPlayer.EndReached += OnMediaEndReached;
                _mediaPlayer.EncounteredError += OnMediaError;
                _mediaPlayer.Playing += OnMediaPlaying;
                _mediaPlayer.Stopped += OnMediaStopped;
                _mediaPlayer.PositionChanged += OnPositionChanged;
                _mediaPlayer.LengthChanged += OnLengthChanged;
            }
            else
            {
                _mediaPlayer?.Stop();
                _currentMedia?.Dispose();
                _currentMedia = null;
            }

            _currentMedia = new Media(_libVlc, new Uri(filePath));
            _mediaPlayer?.Play(_currentMedia);
        }
        catch (Exception ex)
        {
            SetStatus($"Failed: {ex.Message}");
        }
    }

    private void StopVideo()
    {
        if (_mediaPlayer != null)
        {
            _mediaPlayer.Stop();
        }
        _currentMedia?.Dispose();
        _currentMedia = null;
        _hideTimer?.Stop();
        _statusClearTimer?.Stop();
    }

    public void StopPlayback()
    {
        if (_isVideo)
            StopVideo();
    }

    private void SetStatus(string text)
    {
        txtStatus.Text = text;
        _statusClearTimer?.Stop();
        if (text.Length > 0)
        {
            _statusClearTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(4)
            };
            _statusClearTimer.Tick += (_, _) =>
            {
                txtStatus.Text = "";
                _statusClearTimer.Stop();
            };
            _statusClearTimer.Start();
        }
    }

    private void OnMediaPlaying(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            txtStatus.Text = "";
            ShowTransportBar();
        });
    }

    private void OnMediaStopped(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            btnPlayPause.Content = "▶";
        });
    }

    private void OnMediaEndReached(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            btnPlayPause.Content = "▶";
            seekSlider.Value = seekSlider.Maximum;
        });
    }

    private void OnMediaError(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (_mediaPlayer?.Media?.State == VLCState.Error)
                SetStatus("Playback failed - unsupported codec or corrupted file");
            else
                SetStatus("Playback error");
        });
    }

    private void OnLengthChanged(object? sender, MediaPlayerLengthChangedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (!_isSeeking && _mediaPlayer != null)
                UpdateTimeDisplay();
        });
    }

    private void OnPositionChanged(object? sender, MediaPlayerPositionChangedEventArgs e)
    {
        var now = DateTime.UtcNow;
        if ((now - _lastPositionUpdate).TotalMilliseconds < PositionThrottleMs)
            return;
        _lastPositionUpdate = now;

        Dispatcher.Invoke(() =>
        {
            if (!_isSeeking && _mediaPlayer != null)
            {
                seekSlider.Value = e.Position * seekSlider.Maximum;
                UpdateTimeDisplay();
            }
        });
    }

    private void UpdateTimeDisplay()
    {
        if (_mediaPlayer == null) return;
        var total = TimeSpan.FromMilliseconds(_mediaPlayer.Length);
        var current = TimeSpan.FromMilliseconds(_mediaPlayer.Time);
        txtTime.Text = $"{(int)current.TotalMinutes:D2}:{current.Seconds:D2} / {(int)total.TotalMinutes:D2}:{total.Seconds:D2}";
    }

    private void ShowTransportBar()
    {
        if (_isVideo && _mediaPlayer != null)
        {
            transportBar.Visibility = Visibility.Visible;
            UpdatePlayPauseButton();
            StartHideTimer(3);
        }
    }

    private void StartHideTimer(double seconds)
    {
        if (_hideTimer == null)
        {
            _hideTimer = new DispatcherTimer();
            _hideTimer.Tick += (_, _) =>
            {
                if (_isFullScreen)
                    transportBar.Visibility = Visibility.Collapsed;
                _hideTimer?.Stop();
            };
        }
        else
        {
            _hideTimer.Stop();
        }
        _hideTimer.Interval = TimeSpan.FromSeconds(seconds);
        _hideTimer.Start();
    }

    private void UpdatePlayPauseButton()
    {
        btnPlayPause.Content = _mediaPlayer?.IsPlaying == true ? "⏸" : "▶";
    }

    public void ShowFullScreen(int screenIndex, bool fullScreen = false)
    {
        var screens = ScreenUtilities.GetScreens();
        if (screenIndex < 0 || screenIndex >= screens.Length)
            screenIndex = 0;

        var screen = screens[screenIndex];

        _isFullScreen = fullScreen;
        _screenIndex = screenIndex;

        if (fullScreen)
        {
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;
            ShowInTaskbar = false;
            btnClose.Visibility = Visibility.Collapsed;
            transportBar.Visibility = Visibility.Collapsed;

            Left = screen.Bounds.Left;
            Top = screen.Bounds.Top;
            Width = screen.Bounds.Width;
            Height = screen.Bounds.Height;
            WindowState = WindowState.Maximized;
        }
        else
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResize;
            WindowState = WindowState.Normal;
            Left = screen.Bounds.Left + 100;
            Top = screen.Bounds.Top + 100;
            Width = 1280;
            Height = 720;
            Topmost = false;
            ShowInTaskbar = true;
            btnClose.Visibility = Visibility.Visible;
            if (_mediaPlayer?.IsPlaying == true)
                ShowTransportBar();
        }
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (WindowState == WindowState.Maximized && !_isFullScreen)
        {
            var screens = ScreenUtilities.GetScreens();
            var screenIndex = 0;
            for (int i = 0; i < screens.Length; i++)
            {
                var s = screens[i];
                if (Left >= s.Bounds.Left && Left < s.Bounds.Right)
                {
                    screenIndex = i;
                    break;
                }
            }
            ShowFullScreen(screenIndex, true);
        }
        else if (WindowState == WindowState.Normal && _isFullScreen)
        {
            ShowFullScreen(_screenIndex, false);
        }
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isVideo || _mediaPlayer == null || !_isFullScreen)
            return;

        var pos = e.GetPosition(this);
        if (pos.Y >= ActualHeight - ShowTransportBarThreshold)
            ShowTransportBar();
    }

    private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
    {
        if (_mediaPlayer == null) return;

        if (_mediaPlayer.IsPlaying)
            _mediaPlayer.Pause();
        else
            _mediaPlayer.Play();
        UpdatePlayPauseButton();
        StartHideTimer(3);
    }

    private void SeekSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        _isSeeking = true;
        _hideTimer?.Stop();
    }

    private void SeekSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        _isSeeking = false;
        StartHideTimer(3);
    }

    private void SeekSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isSeeking && _mediaPlayer != null)
        {
            var pos = (float)(e.NewValue / seekSlider.Maximum);
            _mediaPlayer.Position = pos;
        }
    }

    private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_mediaPlayer != null)
        {
            _mediaPlayer.Volume = (int)e.NewValue;
            _isMuted = e.NewValue == 0;
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.F11:
                ShowFullScreen(_screenIndex, !_isFullScreen);
                e.Handled = true;
                return;

            case Key.Escape when _isFullScreen:
                ShowFullScreen(_screenIndex, false);
                e.Handled = true;
                return;

            case Key.Space when _isVideo:
                BtnPlayPause_Click(sender, new RoutedEventArgs());
                e.Handled = true;
                return;

            case Key.Left when _isVideo && _mediaPlayer != null:
                _mediaPlayer.Time = Math.Max(0, _mediaPlayer.Time - 10000);
                e.Handled = true;
                return;

            case Key.Right when _isVideo && _mediaPlayer != null:
                _mediaPlayer.Time = Math.Min(_mediaPlayer.Length, _mediaPlayer.Time + 10000);
                e.Handled = true;
                return;

            case Key.Up when _isVideo && _mediaPlayer != null:
                _mediaPlayer.Volume = Math.Min(100, _mediaPlayer.Volume + 10);
                volumeSlider.Value = _mediaPlayer.Volume;
                e.Handled = true;
                return;

            case Key.Down when _isVideo && _mediaPlayer != null:
                _mediaPlayer.Volume = Math.Max(0, _mediaPlayer.Volume - 10);
                volumeSlider.Value = _mediaPlayer.Volume;
                e.Handled = true;
                return;

            case Key.M when _isVideo && _mediaPlayer != null:
                _isMuted = !_isMuted;
                _mediaPlayer.Volume = _isMuted ? 0 : (int)volumeSlider.Value;
                e.Handled = true;
                return;
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        StopVideo();
        _statusClearTimer?.Stop();
        _hideTimer?.Stop();
        _mediaPlayer?.Dispose();
        _libVlc?.Dispose();
        base.OnClosed(e);
    }
}