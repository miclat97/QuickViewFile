using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuickViewFile.Helpers;
using QuickViewFile.Models;
using QuickViewFile.ViewModel;

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

            string path = vm.SelectedItem?.FullPath ?? vm.ActiveListItems.FirstOrDefault()?.FullPath ?? Directory.GetCurrentDirectory();

            MainWindow mainWindow = new MainWindow(path);
            mainWindow.Show();
            this.Close();
        }

        private void AppWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Add || e.Key == Key.OemPlus)
            {
                vm.ThumbnailSize += 20;
                vm.ThumbnailFontSize += 2;
                if (vm.ThumbnailSize > 800) vm.ThumbnailSize = 800;
                if (vm.ThumbnailFontSize > 40) vm.ThumbnailFontSize = 40;
                e.Handled = true;
            }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                vm.ThumbnailSize -= 20;
                vm.ThumbnailFontSize -= 2;
                if (vm.ThumbnailSize < 50) vm.ThumbnailSize = 50;
                if (vm.ThumbnailFontSize < 8) vm.ThumbnailFontSize = 8;
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
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter)
            {
                Application.Current.Dispatcher.BeginInvoke(async () =>
                {
                    await vm.OnFileDoubleClick(vm.SelectedItem);
                    // Refresh thumbnails if we entered a directory
                    if (vm.SelectedItem?.IsDirectory == true || vm.SelectedItem?.Name == "..")
                    {
                         _ = vm.LoadThumbnailsAsync();
                    }
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
                    // Refresh thumbnails if we entered a directory
                    if (file.IsDirectory || file.Name == "..")
                    {
                         _ = vm.LoadThumbnailsAsync();
                    }
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

        private void PasteFiles_Click(object sender, RoutedEventArgs e)
        {
            if (_clipboardFiles.Count == 0 || _currentOperation == FileOperation.None) return;

            string targetDirectory = Directory.GetCurrentDirectory(); // Gets current folder

            try
            {
                foreach (string sourcePath in _clipboardFiles)
                {
                    string itemName = Path.GetFileName(sourcePath);
                    string destinationPath = Path.Combine(targetDirectory, itemName);

                    if (Directory.Exists(sourcePath))
                    {
                        if (_currentOperation == FileOperation.Copy)
                            DirectoryOperationHelper.CopyDirectoryRecursive(sourcePath, destinationPath);
                        else if (_currentOperation == FileOperation.Move)
                            Directory.Move(sourcePath, destinationPath);
                    }
                    else if (File.Exists(sourcePath))
                    {
                        if (_currentOperation == FileOperation.Copy)
                            File.Copy(sourcePath, destinationPath, true);
                        else if (_currentOperation == FileOperation.Move)
                            File.Move(sourcePath, destinationPath, true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error pasting files: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (_currentOperation == FileOperation.Move)
            {
                _clipboardFiles.Clear();
                PasteButton.Visibility = Visibility.Collapsed;
                _currentOperation = FileOperation.None;
            }

            vm.RefreshFiles();
            _ = vm.LoadThumbnailsAsync();
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
    }
}
