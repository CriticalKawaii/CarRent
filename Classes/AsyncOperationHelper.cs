using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.Classes
{
    /// <summary>
    /// Helper class for managing asynchronous operations with UI feedback
    /// </summary>
    public static class AsyncOperationHelper
    {
        /// <summary>
        /// Runs an asynchronous operation with progress indicator
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <param name="operation">The async operation to perform</param>
        /// <param name="progressBar">The progress bar to show during the operation</param>
        /// <param name="disableControls">Optional controls to disable during the operation</param>
        /// <returns>Result of the operation</returns>
        public static async Task<T> RunWithProgressAsync<T>(
            Func<Task<T>> operation,
            ProgressBar progressBar,
            params UIElement[] disableControls)
        {
            // Store original state of controls
            var originalEnabledStates = new bool[disableControls.Length];
            for (int i = 0; i < disableControls.Length; i++)
            {
                originalEnabledStates[i] = disableControls[i].IsEnabled;
                disableControls[i].IsEnabled = false;
            }

            // Show progress
            var originalVisibility = progressBar.Visibility;
            progressBar.Visibility = Visibility.Visible;

            try
            {
                // Run the operation
                return await operation();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
            finally
            {
                // Hide progress and restore controls
                progressBar.Visibility = originalVisibility;
                for (int i = 0; i < disableControls.Length; i++)
                {
                    disableControls[i].IsEnabled = originalEnabledStates[i];
                }
            }
        }

        /// <summary>
        /// Runs an asynchronous operation with progress indicator (no return value)
        /// </summary>
        /// <param name="operation">The async operation to perform</param>
        /// <param name="progressBar">The progress bar to show during the operation</param>
        /// <param name="disableControls">Optional controls to disable during the operation</param>
        public static async Task RunWithProgressAsync(
            Func<Task> operation,
            ProgressBar progressBar,
            params UIElement[] disableControls)
        {
            // Store original state of controls
            var originalEnabledStates = new bool[disableControls.Length];
            for (int i = 0; i < disableControls.Length; i++)
            {
                originalEnabledStates[i] = disableControls[i].IsEnabled;
                disableControls[i].IsEnabled = false;
            }

            // Show progress
            var originalVisibility = progressBar.Visibility;
            progressBar.Visibility = Visibility.Visible;

            try
            {
                // Run the operation
                await operation();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
            finally
            {
                // Hide progress and restore controls
                progressBar.Visibility = originalVisibility;
                for (int i = 0; i < disableControls.Length; i++)
                {
                    disableControls[i].IsEnabled = originalEnabledStates[i];
                }
            }
        }
    }
}