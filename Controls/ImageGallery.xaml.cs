using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp.Classes;

namespace WpfApp.Controls
{
    public partial class ImageGallery : UserControl
    {
        private int _currentIndex = 0;

        public class ThumbnailItem
        {
            public ImageSource ImageSource { get; set; }
            public int Index { get; set; }
            public bool IsSelected { get; set; }
            public string ImageUrl { get; set; }
        }

        private ObservableCollection<ThumbnailItem> _thumbnails;

        public ImageGallery()
        {
            InitializeComponent();

            _thumbnails = new ObservableCollection<ThumbnailItem>();
            ThumbnailsControl.ItemsSource = _thumbnails;

            UpdateNavigationButtons();
        }

        public void LoadImages(List<string> imageUrls)
        {
            _thumbnails.Clear();

            if (imageUrls == null || imageUrls.Count == 0)
            {
                NoImagesText.Visibility = Visibility.Visible;
                MainImage.Visibility = Visibility.Collapsed;
                PreviousButton.Visibility = Visibility.Collapsed;
                NextButton.Visibility = Visibility.Collapsed;
                return;
            }

            NoImagesText.Visibility = Visibility.Collapsed;
            MainImage.Visibility = Visibility.Visible;

            // Load images asynchronously
            LoadImagesAsync(imageUrls);
        }

        private async void LoadImagesAsync(List<string> imageUrls)
        {
            for (int i = 0; i < imageUrls.Count; i++)
            {
                try
                {
                    var imageUrl = imageUrls[i];

                    // Use ImageCache for efficient loading
                    var bitmap = await ImageCache.GetImageAsync(imageUrl);

                    _thumbnails.Add(new ThumbnailItem
                    {
                        ImageSource = bitmap,
                        Index = i,
                        IsSelected = i == 0,
                        ImageUrl = imageUrl
                    });

                    // If this is the first image, set it as the main image
                    if (i == 0)
                    {
                        MainImage.Source = bitmap;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading image: {ex.Message}");
                    // Just skip problematic images
                }
            }

            // Only show navigation buttons if we have multiple images
            PreviousButton.Visibility = _thumbnails.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            NextButton.Visibility = _thumbnails.Count > 1 ? Visibility.Visible : Visibility.Collapsed;

            UpdateNavigationButtons();

            // Force refresh of the thumbnails
            var temp = _thumbnails.ToList();
            ThumbnailsControl.ItemsSource = null;
            ThumbnailsControl.ItemsSource = temp;
        }


        private void UpdateNavigationButtons()
        {
            PreviousButton.IsEnabled = _currentIndex > 0;
            NextButton.IsEnabled = _currentIndex < _thumbnails.Count - 1;
        }

        private void SelectThumbnail(int index)
        {
            if (index < 0 || index >= _thumbnails.Count)
                return;

            _currentIndex = index;

            // Update selection state
            for (int i = 0; i < _thumbnails.Count; i++)
            {
                _thumbnails[i].IsSelected = (i == index);
            }

            // Update the main image
            MainImage.Source = _thumbnails[index].ImageSource;

            UpdateNavigationButtons();

            // Force refresh of the thumbnails
            ThumbnailsControl.ItemsSource = null;
            ThumbnailsControl.ItemsSource = _thumbnails;
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex > 0)
            {
                SelectThumbnail(_currentIndex - 1);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex < _thumbnails.Count - 1)
            {
                SelectThumbnail(_currentIndex + 1);
            }
        }

        private void ThumbnailImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image image && image.Tag is int index)
            {
                SelectThumbnail(index);
            }
        }

        public string GetCurrentImageUrl()
        {
            if (_thumbnails.Count > 0 && _currentIndex >= 0 && _currentIndex < _thumbnails.Count)
            {
                return _thumbnails[_currentIndex].ImageUrl;
            }
            return null;
        }

        public List<string> GetAllImageUrls()
        {
            List<string> urls = new List<string>();
            foreach (var thumbnail in _thumbnails)
            {
                urls.Add(thumbnail.ImageUrl);
            }
            return urls;
        }

        public int GetCurrentIndex()
        {
            return _currentIndex;
        }
    }

    // Add this converter to your App.xaml resources
    public class BooleanToSelectionBorderConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? new SolidColorBrush(Colors.Aqua) : new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}