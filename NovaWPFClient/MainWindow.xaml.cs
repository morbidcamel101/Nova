using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Nova.UI.DataService;
using System.Threading;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Data.Services.Client;

namespace Nova.UI
{
    /// <summary>
    /// Main customer view for Nova Sample Question
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CustomerServiceUri = new Uri(Properties.Settings.Default.CustomerDataServiceUri);
            Pages = new List<ContentPage>();
        }

        #region Properties & Fields
        public Uri CustomerServiceUri { get; private set; }
        public List<ContentPage> Pages { get; set; }
        public ContentPage CurrentContentPage { get; private set; }
        #endregion

        #region Private Methods
        /// <summary>
        ///   A helper method to dispatch UI calls accross threads. IViewModelTarget(T)
        /// </summary>
        public void Dispatch(Action handler)
        {
            if (Thread.CurrentThread != this.Dispatcher.Thread)
            {
                this.Dispatcher.BeginInvoke(handler);
            }
            else
            {
                handler();
            }
        }

        /// <summary>
        ///   Factory method for pages
        /// </summary>
        private T GetContentPage<T>() where T : ContentPage, new()
        {
            T page = Pages.OfType<T>().FirstOrDefault();
            if (page == null)
            {
                page = new T();
                page.CheckSaveState += new EventHandler(Page_CheckSaveState);
                Pages.Add(page);
            }
            return page;
        }

        public void Navigate<T>() where T : ContentPage, new()
        {
            ConfirmSave();
            textBoxSearch.Text = string.Empty;
            contentFrame.Navigate(CurrentContentPage = GetContentPage<T>());
            labelTitle.Content = CurrentContentPage.Title;
        }

        private bool ConfirmSave(MessageBoxButton buttons = MessageBoxButton.YesNo)
        {
            if (CurrentContentPage != null && CurrentContentPage.ShouldSave)
            {
                switch (MessageBox.Show("Do you want to upload your changes to the cloud?", "Save", buttons, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        CurrentContentPage.Save();
                        break;
                    case MessageBoxResult.No:
                        CurrentContentPage.Discard();
                        break;
                    case MessageBoxResult.Cancel:
                        return false;
                }
            }
            return true;
        }

        private void ShowException(Exception exception)
        {
            Dispatch(() => MessageBox.Show(exception.Message, "Oops!", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));

        }
        #endregion

        #region Event Subscriptions
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.DispatcherUnhandledException += (s, e1) => { ShowException(e1.Exception); e1.Handled = true; };

            buttonSave.Visibility = Visibility.Hidden;
            // Go to the initial page
            Navigate<CustomersPage>();
        }

        private void RunSearch()
        {
            if (CurrentContentPage == null)
                return;

            CurrentContentPage.Dispatch(() => CurrentContentPage.PerformSearch(textBoxSearch.Text));
            
        }

       

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box == null || CurrentContentPage == null)
                return;

            CurrentContentPage.BeforeSearch();
            CurrentContentPage.Schedule(RunSearch);
        }

        

        private void Customers_Click(object sender, RoutedEventArgs e)
        {
            Navigate<CustomersPage>();
        }

        private void Locations_Click(object sender, RoutedEventArgs e)
        {
            Navigate<LocationSummary>();
        }

        private void Categories_Click(object sender, RoutedEventArgs e)
        {
            Navigate<CategorySummary>();
        }

        void Page_CheckSaveState(object sender, EventArgs e)
        {
            buttonSave.Visibility = CurrentContentPage != null && CurrentContentPage.ShouldSave ? Visibility.Visible : Visibility.Hidden;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentContentPage == null)
                return;

            CurrentContentPage.Save();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = !ConfirmSave(MessageBoxButton.YesNoCancel);


        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            NewCustomerWindow newCust = new NewCustomerWindow(CurrentContentPage);
            newCust.ShowDialog();
            if (newCust.NewCustomer == null)
                return;

            CurrentContentPage.AddCustomer(newCust.NewCustomer);
            textBoxSearch.Text = newCust.Name;
            
        }
        #endregion

       

        
    }
}
