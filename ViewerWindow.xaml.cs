using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace FreePresenter;

public partial class ViewerWindow : Window
{
    private bool _isVideo;

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

    public void ShowFullScreen(int screenIndex)
    {
        var screens = ScreenUtilities.GetScreens();
        if (screenIndex < 0 || screenIndex >= screens.Length)
            screenIndex = 0;

        var screen = screens[screenIndex];

        // Position on the target screen and allow resizing.
        // Use a normal window chrome so the window can be resized by the user.
        WindowStyle = WindowStyle.SingleBorderWindow;
        ResizeMode = ResizeMode.CanResize;
        WindowState = WindowState.Normal;
        Left = screen.Bounds.Left;
        Top = screen.Bounds.Top;
        Width = screen.Bounds.Width;
        Height = screen.Bounds.Height;
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
        if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        // Close only this viewer window instead of shutting down the whole application.
        Close();
    }
}
