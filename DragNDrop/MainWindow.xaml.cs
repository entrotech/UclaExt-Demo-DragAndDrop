using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace DragNDrop
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

        #region Case 1: User presses button and uses Open File Dialog to get file

        // This uses the OpenFileDialog to allow the user to browse for a file
        // that we can then access programmatically.

        private void FileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                FileNameTextBox.Text = dlg.FileName;
                LoadImageFromFile(dlg.FileName);
            }
        }

        #endregion

        #region Case 2: Copy from Clipboard when Button is Pressed
        
        //User Copies a file from Windows Explorer to the Clipboard, 
        // then presses the Copy from Clipboard Button to copy the
        // file name.
        
        private void ClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            IDataObject data = Clipboard.GetDataObject();
            if (data.GetDataPresent("FileDrop"))
            {
                string[] selectedFiles = data.GetData("FileDrop") as string[];
                FileNameTextBox.Text = selectedFiles[0];
                LoadImageFromFile(selectedFiles[0]);
            }
        }

        #endregion

        #region Case 3: Support Paste to TextBox 

        // User Copies a file from Windows Explorer to the Clipboard, then
        // sets focus to the TextBox and Pastes.
        // I could not yet figure out a way to allow the user to paste to
        // the Image Control - probably because it is normally not an 
        // editable control.

        private void OnPreviewExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                if (Clipboard.ContainsData(DataFormats.FileDrop))
                {
                    string[] myData = Clipboard.GetData("FileDrop") as string[];
                    FileNameTextBox.Text = myData[0].ToString();
                }
            }
        }

        private void OnPreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                e.CanExecute = true;
                e.Handled = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CommandManager.AddPreviewCanExecuteHandler(this.FileNameTextBox, OnPreviewCanExecute);
            CommandManager.AddPreviewExecutedHandler(this.FileNameTextBox, OnPreviewExecute);

            CommandManager.AddPreviewCanExecuteHandler(this.ImageControl, OnPreviewCanExecute);
            CommandManager.AddPreviewExecutedHandler(this.ImageControl, OnPreviewExecute);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            CommandManager.RemovePreviewCanExecuteHandler(this.FileNameTextBox, OnPreviewCanExecute);
            CommandManager.RemovePreviewExecutedHandler(this.FileNameTextBox, OnPreviewExecute);

            CommandManager.RemovePreviewCanExecuteHandler(this.ImageControl, OnPreviewCanExecute);
            CommandManager.RemovePreviewExecutedHandler(this.ImageControl, OnPreviewExecute);
        }

        #endregion

        #region Case 4:  Drag N Drop Implementation Events

        // Open Windows Explorer and drag a file from there
        // into the application window over either the TextBox
        // or the ImageControl.
        // There were a few tricky parts:
        // For the TextBox, you need to handle the PreviewDragEnter and
        // PreviewDragOver events, instead of the DragEnter and DragOver
        // events because the TextBox has it's own handlers for the latter 
        // events, which apprently set e.Handled = true and prevent further
        // handling, but the Preview* versions work.

        // For the ImageControl, I had to put an image in the control to
        // start with (instead of starting with it empty). Not sure why.

        private void File_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void File_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void File_Drop(object sender, DragEventArgs e)
        {
            var strs = (string[])e.Data.GetData(DataFormats.FileDrop);
            string fileName = strs[0];
            FileNameTextBox.Text = fileName;

            LoadImageFromFile(fileName);

            e.Handled = true;
        }

        #endregion

        #region Private Methods

        private void LoadImageFromFile(string fileName)
        {
            try
            {
                var bm = new BitmapImage(new Uri(fileName));
                ImageControl.Source = bm;
            }
            catch (NotSupportedException)
            {
                // File type cannot be converted to an image 
                // for display in the Image Control
            }
        }

        #endregion
    }
}
