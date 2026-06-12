using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace FreePresenter;

public partial class ViewerWindow : Window
{
    private bool _isVideo;
    private bool _isFullScreen;
    private int _screenIndex;

    public ViewerWindow()
    {
        InitializeComponent();
    }

    public void LoadFile(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        _isVideo = ext is ".mp4" or ".avi" or ".mov" or ".wmv" or ".mkv" or ".webm" or ".m4v";

        if (_isVideo)
        {
            imgDisplay.Visibility = Visibility.Collapsed;
            mediaDisplay.Visibility = Visibility.Visible;
            mediaDisplay.Source = new Uri(filePath);
            mediaDisplay.Play();
        }
        else
        {
            mediaDisplay.Visibility = Visibility.Collapsed;
            mediaDisplay.Close();
            imgDisplay.Visibility = Visibility.Visible;
            var img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.UriSource = new Uri(filePath);
            img.EndInit();
            imgDisplay.Source = img;
        }

        Title = Path.GetFileName(filePath);
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

    public void StopPlayback()
    {
        if (_isVideo)
            mediaDisplay.Close();
    }

    private void Media_Ended(object sender, RoutedEventArgs e)
    {
        mediaDisplay.Position = TimeSpan.Zero;
        mediaDisplay.Play();
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F11)
        {
            ShowFullScreen(_screenIndex, !_isFullScreen);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape)
        {
            if (_isFullScreen)
            {
                ShowFullScreen(_screenIndex, false);
                e.Handled = true;
            }
            return;
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
