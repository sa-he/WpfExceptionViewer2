using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace WpfExceptionViewer2
{
    /// <summary>
    /// A WPF window for viewing Exceptions and inner Exceptions, including all their properties.
    /// </summary>
    public partial class ExceptionViewer : Window
    {
        private static string _defaultTitle;

        private static string _product;

        // This is used to dynamically calculate the mainGrid.MaxWidth when the Window is resized,
        // since I can't quite get the behavior I want without it.  See CalcMaxTreeWidth().
        private double _chromeWidth;

        private double _large;

        private double _med;

        // Font sizes based on the "normal" size.
        private double _small;

        /// <summary>
        /// The exception and header message cannot be null.  If owner is specified, this window
        /// uses its Style and will appear centered on the Owner.  You can override this before
        /// calling ShowDialog().
        /// </summary>
        public ExceptionViewer(string headerMessage, Exception e)
            : this(headerMessage, e, null)
        {
        }

        /// <summary>
        /// The exception and header message cannot be null.  If owner is specified, this window
        /// uses its Style and will appear centered on the Owner.  You can override this before
        /// calling ShowDialog().
        /// </summary>
        public ExceptionViewer(string headerMessage, Exception e, Window owner)
        {
            InitializeComponent();

            if (owner != null)
            {
                // This hopefully makes our window look like it belongs to the main app.
                Style = owner.Style;

                // This seems to make the window appear on the same monitor as the owner.
                Owner = owner;

                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            docViewer.Background = treeView1.Background;

            // We use three font sizes.  The smallest is based on whatever the "standard"
            // size is for the current system/app, taken from an arbitrary control.
            _small = treeView1.FontSize;
            _med = _small * 1.1;
            _large = _small * 1.2;

            Title = DefaultTitle;

            BuildTree(e, headerMessage);
        }

        /// <summary>
        /// The default title to use for the ExceptionViewer window.  Automatically initialized
        /// to "Error - [ProductName]" where [ProductName] is taken from the application's
        /// AssemblyProduct attribute (set in the AssemblyInfo.cs file).  You can change this
        /// default, or ignore it and set Title yourself before calling ShowDialog().
        /// </summary>
        public static string DefaultTitle
        {
            get
            {
                if (_defaultTitle == null)
                {
                    if (string.IsNullOrEmpty(Product))
                    {
                        _defaultTitle = "Error";
                    }
                    else
                    {
                        _defaultTitle = "Error - " + Product;
                    }
                }

                return _defaultTitle;
            }

            set
            {
                _defaultTitle = value;
            }
        }

        /// <summary>
        /// Gets the value of the AssemblyProduct attribute of the app.
        /// If unable to lookup the attribute, returns an empty string.
        /// </summary>
        public static string Product
        {
            get
            {
                if (_product == null)
                {
                    _product = GetProductName();
                }

                return _product;
            }
        }

        // Tries to get the assembly to extract the product name from.
        private static Assembly GetAppAssembly()
        {
            Assembly _appAssembly = null;

            try
            {
                // This is supposedly how Windows.Forms.Application does it.
                _appAssembly = Application.Current.MainWindow.GetType().Assembly;
            }
            catch
            { }

            // If the above didn't work, try less desireable ways to get an assembly.

            if (_appAssembly == null)
            {
                _appAssembly = Assembly.GetEntryAssembly();
            }

            if (_appAssembly == null)
            {
                _appAssembly = Assembly.GetExecutingAssembly();
            }

            return _appAssembly;
        }

        // Initializes the Product property.
        private static string GetProductName()
        {
            var result = "";

            try
            {
                var _appAssembly = GetAppAssembly();

                var customAttributes = _appAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);

                if ((customAttributes != null) && (customAttributes.Length > 0))
                {
                    result = ((AssemblyProductAttribute)customAttributes[0]).Product;
                }
            }
            catch
            { }

            return result;
        }

        private static string RenderDictionary(IDictionary data)
        {
            var result = new StringBuilder();

            foreach (var key in data.Keys)
            {
                if (key != null && data[key] != null)
                {
                    result.AppendLine(key.ToString() + " = " + data[key].ToString());
                }
            }

            if (result.Length > 0) result.Length = result.Length - 1;
            return result.ToString();
        }

        private static string RenderEnumerable(IEnumerable data)
        {
            var result = new StringBuilder();

            foreach (var obj in data)
            {
                result.AppendFormat("{0}\n", obj);
            }

            if (result.Length > 0) result.Length = result.Length - 1;
            return result.ToString();
        }

        // Adds the exception as a new top-level node to the tree with child nodes
        // for all the exception's properties.
        private void AddException(Exception e)
        {
            // Create a list of Inlines containing all the properties of the exception object.
            // The three most important properties (message, type, and stack trace) go first.

            var exceptionItem = CreateNewTreeViewItem();

            var inlines = new List<Inline>();
            var properties = e.GetType().GetProperties();

            exceptionItem.Header = e.GetType();
            exceptionItem.Tag = inlines;

            Inline inline = new Bold(new Run(e.GetType().ToString()))
            {
                FontSize = _large
            };
            inlines.Add(inline);

            AddProperty(inlines, "Message", e.Message);
            AddProperty(inlines, "Stack Trace", e.StackTrace);

            foreach (var info in properties)
            {
                // Skip InnerException because it will get a whole
                // top-level node of its own.

                if (info.Name != "InnerException")
                {
                    var value = info.GetValue(e, null);

                    if (value != null)
                    {
                        if (value is string)
                        {
                            if (string.IsNullOrEmpty(value as string)) continue;
                        }
                        else if (value is IDictionary)
                        {
                            value = RenderDictionary(value as IDictionary);
                            if (string.IsNullOrEmpty(value as string)) continue;
                        }
                        else if (value is IEnumerable && !(value is string))
                        {
                            value = RenderEnumerable(value as IEnumerable);
                            if (string.IsNullOrEmpty(value as string)) continue;
                        }

                        if (info.Name != "Message" &&
                            info.Name != "StackTrace")
                        {
                            // Add the property to list for the exceptionItem.
                            AddProperty(inlines, info.Name, value);
                        }

                        // Create a TreeViewItem for the individual property.
                        var propertyItem = new TreeViewItem();
                        var propertyInlines = new List<Inline>();

                        propertyItem.Header = info.Name;
                        propertyItem.Tag = propertyInlines;
                        exceptionItem.Items.Add(propertyItem);
                        AddProperty(propertyInlines, info.Name, value);
                    }
                }
            }
        }

        // Adds the string to the list of Inlines, substituting
        // LineBreaks for an newline chars found.
        private void AddLines(List<Inline> inlines, string str)
        {
            var lines = str.Split('\n');

            inlines.Add(new Run(lines[0].Trim('\r')));

            foreach (var line in lines.Skip(1))
            {
                inlines.Add(new LineBreak());
                inlines.Add(new Run(line.Trim('\r')));
            }
        }

        private void AddProperty(List<Inline> inlines, string propName, object propVal)
        {
            inlines.Add(new LineBreak());
            inlines.Add(new LineBreak());
            var inline = new Bold(new Run(propName + ":"))
            {
                FontSize = _med
            };
            inlines.Add(inline);
            inlines.Add(new LineBreak());

            if (propVal is string)
            {
                // Might have embedded newlines.

                AddLines(inlines, propVal as string);
            }
            else
            {
                inlines.Add(new Run(propVal.ToString()));
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            // Build a FlowDocument with Inlines from all top-level tree items.

            var inlines = new List<Inline>();
            var doc = new FlowDocument();
            var para = new Paragraph();

            doc.FontSize = _small;
            doc.FontFamily = treeView1.FontFamily;
            doc.TextAlignment = TextAlignment.Left;

            foreach (TreeViewItem treeItem in treeView1.Items)
            {
                if (inlines.Any())
                {
                    // Put a line of underscores between each exception.

                    inlines.Add(new LineBreak());
                    inlines.Add(new Run("____________________________________________________"));
                    inlines.Add(new LineBreak());
                }

                inlines.AddRange(treeItem.Tag as List<Inline>);
            }

            para.Inlines.AddRange(inlines);
            doc.Blocks.Add(para);

            // Now place the doc contents on the clipboard in both
            // rich text and plain text format.

            var range = new TextRange(doc.ContentStart, doc.ContentEnd);
            var data = new DataObject();

            using (Stream stream = new MemoryStream())
            {
                range.Save(stream, DataFormats.Rtf);
                data.SetData(DataFormats.Rtf, Encoding.UTF8.GetString((stream as MemoryStream).ToArray()));
            }

            data.SetData(DataFormats.StringFormat, range.Text);
            Clipboard.SetDataObject(data);

            // The Inlines that were being displayed are now in the temporary document we just built,
            // causing them to disappear from the viewer.  This puts them back.

            ShowCurrentItem();
        }

        // Builds the tree in the left pane.
        // Each TreeViewItem.Tag will contain a list of Inlines
        // to display in the right-hand pane When it is selected.
        private void BuildTree(Exception e, string summaryMessage)
        {
            // The first node in the tree contains the summary message and all the
            // nested exception messages.

            var firstItem = CreateNewTreeViewItem();

            var inlines = new List<Inline>();
            firstItem.Header = "All Messages";

            var inline = new Bold(new Run(summaryMessage))
            {
                FontSize = _large
            };
            inlines.Add(inline);

            // Now add top-level nodes for each exception while building
            // the contents of the first node.
            while (e != null)
            {
                inlines.Add(new LineBreak());
                inlines.Add(new LineBreak());
                AddLines(inlines, e.Message);

                AddException(e);
                e = e.InnerException;
            }

            firstItem.Tag = inlines;
            firstItem.IsSelected = true;
            firstItem.Focus();
        }

        private void CalcMaxTreeWidth()
        {
            // This prevents the GridSplitter from being dragged beyond the right edge of the window.
            // Another way would be to use star sizing for all Grid columns including the left
            // Grid column (i.e. treeCol), but that causes the width of that column to change when the
            // window's width changes, which I don't like.

            mainGrid.MaxWidth = ActualWidth - _chromeWidth;
            treeCol.MaxWidth = mainGrid.MaxWidth - textCol.MinWidth;
        }

        // Determines the page width for the Inlilness that causes no wrapping.
        private double CalcNoWrapWidth(IEnumerable<Inline> inlines)
        {
            double pageWidth = 0;
            var tb = new TextBlock();
            var size = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (var inline in inlines)
            {
                tb.Inlines.Clear();
                tb.Inlines.Add(inline);
                tb.Measure(size);

                if (tb.DesiredSize.Width > pageWidth) pageWidth = tb.DesiredSize.Width;
            }

            return pageWidth;
        }

        private void chkWrap_Checked(object sender, RoutedEventArgs e)
        {
            ShowCurrentItem();
        }

        private void chkWrap_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowCurrentItem();
        }

        private TreeViewItem CreateNewTreeViewItem()
        {
            var exceptionItem = new TreeViewItem();
            if (Background is not null)
                exceptionItem.Background = Background;
            if (Foreground is not null)
                exceptionItem.Foreground = Foreground;
            treeView1.Items.Add(exceptionItem);
            return exceptionItem;
        }

        private void ExpressionViewerWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                CalcMaxTreeWidth();
            }
        }

        private void ShowCurrentItem()
        {
            if (treeView1.SelectedItem != null)
            {
                var inlines = (treeView1.SelectedItem as TreeViewItem).Tag as List<Inline>;
                var doc = new FlowDocument
                {
                    FontSize = _small,
                    FontFamily = treeView1.FontFamily,
                    TextAlignment = TextAlignment.Left,
                    Background = docViewer.Background
                };

                if (chkWrap.IsChecked == false)
                {
                    doc.PageWidth = CalcNoWrapWidth(inlines) + 50;
                }

                var para = new Paragraph();
                para.Inlines.AddRange(inlines);
                doc.Blocks.Add(para);
                docViewer.Document = doc;
            }
        }

        private void treeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ShowCurrentItem();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // The grid column used for the tree started with Width="Auto" so it is now exactly
            // wide enough to fit the longest exception (up to the MaxWidth set in XAML).
            // Changing the width to a fixed pixel value prevents it from changing if the user
            // resizes the window.

            treeCol.Width = new GridLength(treeCol.ActualWidth, GridUnitType.Pixel);
            _chromeWidth = ActualWidth - mainGrid.ActualWidth;
            CalcMaxTreeWidth();
        }
    }
}