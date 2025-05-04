using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.Controls
{
    public partial class GalleryDialog : UserControl
    {
        public event EventHandler CloseRequested;

        public GalleryDialog()
        {
            InitializeComponent();
        }

        public void SetTitle(string title)
        {
            TitleText.Text = title;
        }

        public void LoadImages(List<string> imageUrls)
        {
            Gallery.LoadImages(imageUrls);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
