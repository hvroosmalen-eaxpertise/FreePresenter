using System.IO;
using System.Windows;

namespace FreePresenter;

public partial class MainWindow : Window
{
    private ViewerWindow? _viewer;

    private static readonly HashSet<string> ImageExts =
        [".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".tif", ".webp"];

    private static readonly HashSet<string> VideoExts =
        [".mp4", ".avi", ".mov", ".wmv", ".mkv", ".webm", ".m4v"];

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            dropBorder.BorderBrush = System.Windows.Media.Brushes.DodgerBlue;
            dropBorder.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x2a, 0x3a, 0x4a));
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void Window_DragLeave(object sender, DragEventArgs e)
    {
        dropBorder.BorderBrush = System.Windows.Media.Brushes.Gray;
        dropBorder.Background = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(0x1e, 0x1e, 0x1e));
        e.Handled = true;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        dropBorder.BorderBrush = System.Windows.Media.Brushes.Gray;
        dropBorder.Background = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(0x1e, 0x1e, 0x1e));

        if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
            e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
        {
            OpenFile(files[0]);
        }
        e.Handled = true;
    }

    private void OpenFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        if (!ImageExts.Contains(ext) && !VideoExts.Contains(ext))
        {
            txtFileInfo.Text = "Unsupported file type. Drop an image or video.";
            return;
        }

        txtFileInfo.Text = $"{Path.GetFileName(filePath)} ({(ImageExts.Contains(ext) ? "image" : "video")})";

        if (_viewer != null && _viewer.IsVisible)
        {
            _viewer.StopPlayback();
            _viewer.LoadFile(filePath);
        }
        else
        {
            _viewer = new ViewerWindow();
            _viewer.LoadFile(filePath);
            var screens = ScreenUtilities.GetScreens();
            int targetScreen = screens.Length > 1 ? 1 : 0;
            _viewer.ShowFullScreen(targetScreen);
            _viewer.Show();
            _viewer.Closed += (_, _) =>
            {
                Activate();
                Topmost = true;
                Topmost = false;
            };
        }
    }
}
