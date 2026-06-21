import re

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'r') as f:
    text = f.read()

nav_search = r'''                    Point mousePosition = e.GetPosition\(GridFileContent\);

                    double previousItem = GridFileContent.ActualWidth \* 0.08;
                    double nextItem = GridFileContent.ActualWidth \* 0.92;

                    int nextFileIndex = FilesListView.SelectedIndex \+ 1;
                    int previousFileIndex = FilesListView.SelectedIndex - 1;

                    if \(mousePosition.X < previousItem\)
                    \{
                        FilesListView.SelectedIndex--;
                        System.Windows.Input.Mouse.SetCursor\(System.Windows.Input.Cursors.None\);
                        System.Windows.Input.Mouse.OverrideCursor = null;
                        System.Windows.Input.Mouse.UpdateCursor\(\);
                    \}

                    if \(mousePosition.X > nextItem\)
                    \{
                        FilesListView.SelectedIndex\+\+;
                        System.Windows.Input.Mouse.SetCursor\(System.Windows.Input.Cursors.None\);
                        System.Windows.Input.Mouse.OverrideCursor = null;
                        System.Windows.Input.Mouse.UpdateCursor\(\);
                    \}'''

nav_replace = r'''                    bool goPrevious = false;
                    bool goNext = false;

                    if (ZoomableImageElementNoBorder.Visibility == Visibility.Visible && ZoomableImageElementNoBorder.Source != null)
                    {
                        var image = ZoomableImageElementNoBorder;
                        double imageWidth = image.Source.Width;
                        double imageHeight = image.Source.Height;
                        double controlWidth = image.ActualWidth;
                        double controlHeight = image.ActualHeight;

                        double scaleX = controlWidth / imageWidth;
                        double scaleY = controlHeight / imageHeight;
                        double scale = Math.Min(scaleX, scaleY);

                        double drawnWidth = imageWidth * scale;
                        double offsetX = (controlWidth - drawnWidth) / 2;

                        Point p = e.GetPosition(image);

                        if (p.X >= offsetX && p.X <= offsetX + drawnWidth)
                        {
                            double relativeX = p.X - offsetX;
                            if (relativeX < drawnWidth * 0.15) goPrevious = true;
                            else if (relativeX > drawnWidth * 0.85) goNext = true;
                        }
                    }
                    else
                    {
                        Point p = e.GetPosition(GridFileContent);
                        double w = GridFileContent.ActualWidth;
                        if (w > 0)
                        {
                            if (p.X < w * 0.15) goPrevious = true;
                            else if (p.X > w * 0.85) goNext = true;
                        }
                    }

                    if (goPrevious)
                    {
                        FilesListView.SelectedIndex--;
                        System.Windows.Input.Mouse.SetCursor(System.Windows.Input.Cursors.None);
                        System.Windows.Input.Mouse.OverrideCursor = null;
                        System.Windows.Input.Mouse.UpdateCursor();
                    }
                    else if (goNext)
                    {
                        FilesListView.SelectedIndex++;
                        System.Windows.Input.Mouse.SetCursor(System.Windows.Input.Cursors.None);
                        System.Windows.Input.Mouse.OverrideCursor = null;
                        System.Windows.Input.Mouse.UpdateCursor();
                    }'''

text = re.sub(nav_search, nav_replace, text)

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'w') as f:
    f.write(text)
