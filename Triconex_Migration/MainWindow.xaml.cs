using Microsoft.Win32;
using System.Windows;
using System.Windows.Media.Imaging;
using TriconexMigration.Application.Models;
using TriconexMigration.Application.Services;

namespace Triconex_Migration;

public partial class MainWindow : Window
{
    private readonly FileService fileService;

    public MainWindow(FileService fileService)
    {
        this.fileService = fileService;
        InitializeComponent();
        SetImageSource();
    }

    private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new()
        {
            Filter = "All files (*.*)|*.*"
        };
        if (openFileDialog.ShowDialog() == true)
        {
            txtFilePath.Text = openFileDialog.FileName;
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var FileData = fileService.GetFile(txtFilePath.Text);
            if (FileData.Count != 0)
            {
                ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, txtFilePath.Text);
                fileService.CreateFile(programBuilder);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error Parsing File: {ex.Message}");
        }
    }

    private void SetImageSource()
    {
        try
        {
            logoImage.Source = new BitmapImage(new Uri("pack://application:,,,/icon/logo.png", UriKind.Absolute));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading image: {ex.Message}");
        }
    }
}