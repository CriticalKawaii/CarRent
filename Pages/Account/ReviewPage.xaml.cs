﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WpfApp.Classes;

namespace WpfApp.Pages.Account
{
    public partial class ReviewPage : Page
    {
        private Review _review;
        private Booking _booking;
        private bool _isNewReview = true;

        public ReviewPage(Booking booking)
        {
            InitializeComponent();

            if (booking == null)
            {
                MessageBox.Show("Ошибка: информация о бронировании не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService.GoBack();
                return;
            }

            _booking = booking;

            _review = DBEntities.GetContext().Reviews
                .FirstOrDefault(r => r.UserID == SessionManager.CurrentUser.UserID &&
                                     r.VehicleID == booking.VehicleID);
            if (_review == null)
            {
                _review = new Review
                {
                    UserID = SessionManager.CurrentUser.UserID,
                    VehicleID = booking.VehicleID,
                    Rating = 5, 
                    CreatedAt = DateTime.Now
                };
                _isNewReview = true;
            }
            else
            {
                _isNewReview = false;
            }

            DataContext = _booking;

            this.DataContext = new { Booking = _booking, Review = _review };
            
            Loaded += ReviewPage_Loaded;
        }

        private async void ReviewPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_booking.Vehicle == null)
                {
                    using (var context = new DBEntities())
                    {
                        _booking.Vehicle = await context.Vehicles
                            .Include("VehicleImages")
                            .FirstOrDefaultAsync(v => v.VehicleID == _booking.VehicleID);
                    }
                }

                if (_booking.Vehicle != null)
                {
                    await _booking.Vehicle.GetImageSourceAsync();

                    this.DataContext = null;
                    this.DataContext = new { Booking = _booking, Review = _review };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading vehicle image: {ex.Message}");
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            _review.Rating = (int)RatingBar.Value;

            if (_review.Rating < 1 || _review.Rating > 5)
            {
                MessageBox.Show("Пожалуйста, выберите оценку от 1 до 5", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_review.Comment))
            {
                MessageBox.Show("Пожалуйста, добавьте комментарий к вашему отзыву", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_isNewReview)
                {
                    DBEntities.GetContext().Reviews.Add(_review);
                }

                DBEntities.GetContext().SaveChanges();


                MessageBox.Show(_isNewReview ? "Отзыв успешно добавлен" : "Отзыв успешно обновлен",
                    "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при сохранении отзыва: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ButtonGoBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
