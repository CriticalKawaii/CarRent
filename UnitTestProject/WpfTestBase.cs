using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;

namespace UnitTestProject
{
    public class WpfTestBase
    {
        [TestInitialize]
        public void BaseSetup()
        {
            if (Application.Current == null)
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/WpfApp;component/ResourceDictionaries/RentACarStyle.xaml")
            });
        }
    }
}
