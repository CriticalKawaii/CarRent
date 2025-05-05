using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.Classes
{
    public static class AsyncOperationHelper
    {
        /// <typeparam name="T">Тип возврата операции</typeparam>
        /// <param name="operation">Асинхронная операция, которую необходимо выполнить</param>
        /// <param name="progressBar">Индикатор выполнения, который будет отображаться во время выполнения операции</param>
        /// <param name="disableControls">Дополнительные элементы управления, которые можно отключить на время работы</param>
        /// <returns>Результат операции</returns>
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

            var originalVisibility = progressBar.Visibility;
            progressBar.Visibility = Visibility.Visible;

            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
            finally
            {
                progressBar.Visibility = originalVisibility;
                for (int i = 0; i < disableControls.Length; i++)
                {
                    disableControls[i].IsEnabled = originalEnabledStates[i];
                }
            }
        }

        /// <param name="operation">Асинхронная операция, которую необходимо выполнить</param>
        /// <param name="progressBar">Индикатор выполнения, который будет отображаться во время выполнения операции</param>
        /// <param name="disableControls">Дополнительные элементы управления, которые можно отключить на время работы</param>
        public static async Task RunWithProgressAsync(
            Func<Task> operation,
            ProgressBar progressBar,
            params UIElement[] disableControls)
        {
            var originalEnabledStates = new bool[disableControls.Length];
            for (int i = 0; i < disableControls.Length; i++)
            {
                originalEnabledStates[i] = disableControls[i].IsEnabled;
                disableControls[i].IsEnabled = false;
            }

            var originalVisibility = progressBar.Visibility;
            progressBar.Visibility = Visibility.Visible;

            try
            {
                await operation();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
            finally
            {
                progressBar.Visibility = originalVisibility;
                for (int i = 0; i < disableControls.Length; i++)
                {
                    disableControls[i].IsEnabled = originalEnabledStates[i];
                }
            }
        }
    }
}