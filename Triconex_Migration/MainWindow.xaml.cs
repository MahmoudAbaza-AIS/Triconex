using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Triconex.Models;
using Triconex.Services;

namespace Triconex_Migration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                txtFilePath.Text = openFileDialog.FileName;
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var FileData = FileService.GetFile(txtFilePath.Text);
            if (FileData.Any())
            {
                ProgramBuilder programBuilder = new ProgramBuilder().GetProgramBuilder(FileData, txtFilePath.Text);
                FileService.CreateFile(programBuilder);
            }
        }
    }
}