using QuickViewFile.Helpers;
using QuickViewFile.Models;
using QuickViewFile.ViewModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuickViewFile
{
    public partial class MainWindowThumbnails : Window
    {
        private FilesListViewModel vm;
        private ConfigModel _config;

        private enum FileOperation { None, Copy, Move }
        private FileOperation _currentOperation = FileOperation.None;
        private List<string> _clipboardFiles = new List<string>();

        public MainWindowThumbnails(FilesListViewModel viewModel)
        {
            _config = ConfigHelper.LoadConfig();
            vm = viewModel;
            DataContext = vm;
            InitializeComponent();

            vm.SelectedFileChanged += Vm_SelectedFileChanged;

            // Make sure thumbnails are loaded when the window opens
            vm.IsThumbnailMode = true;
            _ = vm.LoadThumbnailsAsync();

            ThumbnailsListView.Focus();
        }

        private void Vm_SelectedFileChanged(ItemList? item)
        {
            if (item != null)
            {
                ThumbnailsListView.ScrollIntoView(item);
            }
        }

        private void ListModeButton_Click(object sender, RoutedEventArgs e)
        {
            vm.IsThumbnailMode = false;
            vm.CancelThumbnails();

            foreach (var item in vm.ActiveListItems)
            {
                item.ThumbnailImageSource = null;
                item.IsVideoThumbnail = false;
                item.ThumbnailTextPreview = null;
            }

            string path = vm.SelectedItem?.FullPath ?? vm.ActiveListItems.FirstOrDefault()?.FullPath ?? Directory.GetCurrentDirectory();

            MainWindow mainWindow = new MainWindow(path);
            mainWindow.Show();
            this.Close();
        }


        private void AppWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F5)
            {
                if (DataContext is QuickViewFile.ViewModel.FilesListViewModel vm1)
                    vm1.RefreshFiles();
                e.Handled = true;
                return;
            }

            if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                if (DataContext is QuickViewFile.ViewModel.FilesListViewModel vm2)
                {
                    if (e.Key == System.Windows.Input.Key.A)
                    {
                        foreach (var item in vm2.ActiveListItems)
                        {
                            item.IsChecked = true;
                        }
                        e.Handled = true;
                        return;
                    }
                    else if (e.Key == System.Windows.Input.Key.C)
                    {
                        CopyFiles_Click(null, null);
                        e.Handled = true;
                        return;
                    }
                    else if (e.Key == System.Windows.Input.Key.X)
                    {
                        MoveFiles_Click(null, null);
                        e.Handled = true;
                        return;
                    }
                    else if (e.Key == System.Windows.Input.Key.V)
                    {
                        PasteFiles_Click(null, null);
                        e.Handled = true;
                        return;
                    }
                }
            }

            if (e.Key == System.Windows.Input.Key.Add || e.Key == System.Windows.Input.Key.OemPlus)
            {
                vm.ThumbnailSize += 20;
                if (vm.ThumbnailSize > 800) vm.ThumbnailSize = 800;
                e.Handled = true;
            }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                vm.ThumbnailSize -= 20;
                if (vm.ThumbnailSize < 50) vm.ThumbnailSize = 50;
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                // Uncheck all files
                foreach (var file in vm.ActiveListItems)
                {
                    file.IsChecked = false;
                }
                e.Handled = true;
            }
        }

        private void FilesListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var focusedElement = Keyboard.FocusedElement as ListViewItem;
                if (focusedElement == null && ThumbnailsListView.SelectedItem != null)
                {
                    focusedElement = ThumbnailsListView.ItemContainerGenerator.ContainerFromItem(ThumbnailsListView.SelectedItem) as ListViewItem;
                }

                if (focusedElement != null)
                {
                    var itemData = ThumbnailsListView.ItemContainerGenerator.ItemFromContainer(focusedElement) as ItemList;
                    if (itemData != null)
                    {
                        itemData.IsChecked = !itemData.IsChecked;
                    }
                }
                else if (ThumbnailsListView.SelectedItem is ItemList selItem)
                {
                    selItem.IsChecked = !selItem.IsChecked;
                }
                e.Handled = true;
                return;
            }

            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                char ASCIINumberWhichUserWantToSelect = e.Key.ToString()[0];
                var itemToSelect = ThumbnailsListView.Items.Cast<ItemList>().Skip(ThumbnailsListView.SelectedIndex + 1)
                    .FirstOrDefault(item => !string.IsNullOrEmpty(item.Name) && char.ToUpper(item.Name[0]) == ASCIINumberWhichUserWantToSelect);

                if (itemToSelect is null)
                {
                    itemToSelect = ThumbnailsListView.Items.Cast<ItemList>()
                        .FirstOrDefault(item => !string.IsNullOrEmpty(item.Name) && char.ToUpper(item.Name[0]) == ASCIINumberWhichUserWantToSelect);
                }
                if (itemToSelect is not null)
                {
                    ThumbnailsListView.SelectedItem = itemToSelect;
                    ThumbnailsListView.ScrollIntoView(itemToSelect);
                    if (DataContext is FilesListViewModel viewm && ThumbnailsListView.SelectedItem is ItemList sel) viewm.SelectedItem = sel;

                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (ThumbnailsListView.ItemContainerGenerator.ContainerFromItem(ThumbnailsListView.SelectedItem) is ListViewItem item)
                        {
                            item.Focus();
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
                e.Handled = true;
                return;
            }

            try
            {
                bool handled = false;

                // Calculate items per row based on ActualWidth and ThumbnailSize (plus margin logic approx)
                double itemTotalWidth = vm.ThumbnailSize + 10; // 5 margin on each side
                int itemsPerRow = Math.Max(1, (int)(ThumbnailsListView.ActualWidth / itemTotalWidth));

                if (e.Key == Key.Right)
                {
                    if (ThumbnailsListView.SelectedIndex < ThumbnailsListView.Items.Count - 1)
                        ThumbnailsListView.SelectedIndex++;
                    handled = true;
                }
                else if (e.Key == Key.Left)
                {
                    if (ThumbnailsListView.SelectedIndex > 0)
                        ThumbnailsListView.SelectedIndex--;
                    handled = true;
                }
                else if (e.Key == Key.Down)
                {
                    if (ThumbnailsListView.SelectedIndex + itemsPerRow < ThumbnailsListView.Items.Count)
                        ThumbnailsListView.SelectedIndex += itemsPerRow;
                    else
                        ThumbnailsListView.SelectedIndex = ThumbnailsListView.Items.Count - 1;
                    handled = true;
                }
                else if (e.Key == Key.Up)
                {
                    if (ThumbnailsListView.SelectedIndex - itemsPerRow >= 0)
                        ThumbnailsListView.SelectedIndex -= itemsPerRow;
                    else
                        ThumbnailsListView.SelectedIndex = 0;
                    handled = true;
                }
                else if (e.Key == Key.PageDown)
                {
                    ThumbnailsListView.SelectedIndex = Math.Min(ThumbnailsListView.Items.Count - 1, ThumbnailsListView.SelectedIndex + (itemsPerRow * 3));
                    handled = true;
                }
                else if (e.Key == Key.PageUp)
                {
                    ThumbnailsListView.SelectedIndex = Math.Max(0, ThumbnailsListView.SelectedIndex - (itemsPerRow * 3));
                    handled = true;
                }
                else if (e.Key == Key.Home)
                {
                    ThumbnailsListView.SelectedIndex = 0;
                    handled = true;
                }
                else if (e.Key == Key.End)
                {
                    ThumbnailsListView.SelectedIndex = ThumbnailsListView.Items.Count - 1;
                    handled = true;
                }

                if (handled)
                {
                    ThumbnailsListView.SetCurrentValue(ListView.SelectedIndexProperty, ThumbnailsListView.SelectedIndex);
                    ThumbnailsListView.ScrollIntoView(ThumbnailsListView.SelectedItem);

                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (ThumbnailsListView.ItemContainerGenerator.ContainerFromItem(ThumbnailsListView.SelectedItem) is ListViewItem item)
                        {
                            item.Focus();
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded);

                    e.Handled = true;
                    return;
                }
            }
            catch { }

            if (e.Key == Key.Enter)
            {
                Application.Current.Dispatcher.BeginInvoke(async () =>
                {
                    await vm.OnFileDoubleClick(vm.SelectedItem);
                });
                e.Handled = true;
            }
        }

        private void FilesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is ItemList file)
            {
                Application.Current.Dispatcher.BeginInvoke(async () =>
                {
                    await vm.OnFileDoubleClick(file);
                });
            }
        }

        private void FilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThumbnailsListView.SelectedItem is ItemList selected)
            {
                vm.SelectedItem = selected;
            }
        }

        // File operations logic matching MainWindow
        private IEnumerable<ItemList> GetSelectedOrCheckedItems()
        {
            var checkedItems = ThumbnailsListView.Items.Cast<ItemList>().Where(i => i.IsChecked).ToList();
            if (checkedItems.Count > 0) return checkedItems;
            var selectedItems = ThumbnailsListView.SelectedItems.Cast<ItemList>().ToList();
            return selectedItems;
        }

        private void UpdateClipboardFiles()
        {
            var itemsToProcess = GetSelectedOrCheckedItems();
            if (itemsToProcess.Any())
            {
                _clipboardFiles.Clear();
                foreach (var item in itemsToProcess)
                {
                    if (item.Name == "..") continue;
                    _clipboardFiles.Add(item.FullPath);
                }

                if (_clipboardFiles.Count > 0)
                {
                    PasteButton.Visibility = Visibility.Visible;
                    int folderCount = _clipboardFiles.Count(f => Directory.Exists(f));
                    int fileCount = _clipboardFiles.Count - folderCount;

                    string itemsLabel = "";
                    if (fileCount > 0 && folderCount > 0) itemsLabel = $"Paste ({folderCount} folders, {fileCount} files)";
                    else if (folderCount > 0) itemsLabel = $"Paste ({folderCount} folders)";
                    else itemsLabel = $"Paste ({fileCount} files)";

                    PasteButton.Content = itemsLabel;
                }
            }
        }

        private void MoveFiles_Click(object sender, RoutedEventArgs e)
        {
            _currentOperation = FileOperation.Move;
            UpdateClipboardFiles();
        }

        private void CopyFiles_Click(object sender, RoutedEventArgs e)
        {
            _currentOperation = FileOperation.Copy;
            UpdateClipboardFiles();
        }


        private async void PasteFiles_Click(object sender, RoutedEventArgs e)
        {
            if (_clipboardFiles.Count > 0 && DataContext is QuickViewFile.ViewModel.FilesListViewModel vm)
            {
                var clipboardCopy = new System.Collections.Generic.List<string>(_clipboardFiles);
                string targetDir = vm.FolderPath;
                int currentOp = _currentOperation == FileOperation.Copy ? 1 : (_currentOperation == FileOperation.Move ? 2 : 0);

                FileOperationsPanel.Visibility = Visibility.Collapsed;
                ProgressPanel.Visibility = Visibility.Visible;
                ThumbnailsListView.IsEnabled = false;
                OperationProgressBar.Value = 0;
                OperationStatusText.Text = "Preparing...";

                _pasteCts = new System.Threading.CancellationTokenSource();

                await PasteLogic.PerformPasteAsync(clipboardCopy, targetDir, currentOp, this, OperationProgressBar, OperationStatusText, _pasteCts, () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        FileOperationsPanel.Visibility = Visibility.Visible;
                        ProgressPanel.Visibility = Visibility.Collapsed;
                        ThumbnailsListView.IsEnabled = true;

                        _clipboardFiles.Clear();
                        _currentOperation = FileOperation.None;
                        PasteButton.Visibility = Visibility.Collapsed;
                        CancelPasteButton.Visibility = Visibility.Collapsed;
                        MoveButton.Visibility = Visibility.Visible;
                        CopyButton.Visibility = Visibility.Visible;
                        DeleteButton.Visibility = Visibility.Visible;
                        NewFolderButton.Visibility = Visibility.Visible;
                        vm.RefreshFiles();
                        _ = vm.LoadThumbnailsAsync();

                        _pasteCts?.Dispose();
                        _pasteCts = null;
                    });
                });
            }
        }

        private void DeleteFiles_Click(object sender, RoutedEventArgs e)
        {
            var itemsToDelete = GetSelectedOrCheckedItems().ToList();
            if (itemsToDelete.Count > 0)
            {
                var validItems = itemsToDelete.Where(x => x.Name != ".." && !x.IsAlternativeDataStream).ToList();
                if (validItems.Count == 0) return;

                var result = MessageBox.Show($"Are you sure you want to delete {validItems.Count} items?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        foreach (var item in validItems)
                        {
                            if (item.IsDirectory) DirectoryOperationHelper.DeleteDirectoryRecursive(item.FullPath);
                            else File.Delete(item.FullPath);
                        }
                        vm.RefreshFiles();
                        _ = vm.LoadThumbnailsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting items: {ex.Message}", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void FileFullPathTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (DataContext is QuickViewFile.ViewModel.FilesListViewModel vm && !string.IsNullOrWhiteSpace(FileFullPathTextBox.Text))
                {
                    vm.RefreshFiles(FileFullPathTextBox.Text);
                }
                e.Handled = true;
            }
        }

        private void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is QuickViewFile.ViewModel.FilesListViewModel vm)
            {
                var dialog = new InputDialog("Enter new folder name:");
                dialog.Owner = this;
                if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Answer))
                {
                    try
                    {
                        string newPath = System.IO.Path.Combine(vm.FolderPath, dialog.Answer);
                        if (!System.IO.Directory.Exists(newPath))
                        {
                            System.IO.Directory.CreateDirectory(newPath);
                            vm.RefreshFiles();
                        }
                        else
                        {
                            MessageBox.Show("Folder already exists.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to create folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CancelPaste_Click(object sender, RoutedEventArgs e)
        {
            _clipboardFiles.Clear();
            _currentOperation = FileOperation.None;
            PasteButton.Visibility = Visibility.Collapsed;
            CancelPasteButton.Visibility = Visibility.Collapsed;
            MoveButton.Visibility = Visibility.Visible;
            CopyButton.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Visible;
            NewFolderButton.Visibility = Visibility.Visible;
        }

        private System.Threading.CancellationTokenSource? _pasteCts;

        private void CancelOperation_Click(object sender, RoutedEventArgs e)
        {
            if (_pasteCts != null)
                _pasteCts.Cancel();
        }
}
}
