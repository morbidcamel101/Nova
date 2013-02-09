using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Nova.UI.DataService;
using System.Threading;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Data.Services.Client;
using System.Windows;
using System.Collections;
using System.Diagnostics;

namespace Nova.UI
{
    /// <summary>
    ///   Abstract base page class with some common functionality to implement.
    /// </summary>
    public abstract class ContentPage: Page // Maybe CustomerContentPage
    {
        public ContentPage()
        {
            ServiceUri = new Uri(Properties.Settings.Default.CustomerDataServiceUri);
        }

        #region Properties & Events
        private Timer scheduleTimer;
        private Action scheduleAction;

        public Uri ServiceUri 
        { 
            get; 
            private set; 
        }

        public ViewModel<CustomersContainer, Customer> CustomerModel { get; protected set; }
        public ViewModel<CustomersContainer, Location> LocationModel { get; protected set; }
        public ViewModel<CustomersContainer, CustomerTracking> TrackingModel { get; protected set; }
        public ViewModel<CustomersContainer, Category> CategoryModel { get; protected set; }

        /// <summary>
        ///   Shared list of changed customers.
        /// </summary>
        protected HashSet<Customer> pendingCustomers = new HashSet<Customer>();

        public bool ShouldSave
        {
            get { return pendingCustomers.Count > 0; }
        }


        public event EventHandler CheckSaveState;
        
        #endregion

        #region Exposed Methods
        /// <summary>
        ///   A helper method to dispatch UI calls accross threads. Implements all IViewModelTarget(T)
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

        private void ScheduleFire(object state)
        {
            try
            {
                if (scheduleAction != null)
                {
                    scheduleAction();
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
            }
            finally
            {
                scheduleTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }

        }

        /// <summary>
        ///   Schedule an action to fire according to the SearchMiliseconds setting
        /// </summary>
        /// <param name="action"></param>
        public void Schedule(Action action)
        {
            scheduleAction = action;
            if (scheduleTimer == null)
            {
                scheduleTimer = new Timer(ScheduleFire, action, Timeout.Infinite, Timeout.Infinite);
            }
            // First reset the timer
            scheduleTimer.Change(Timeout.Infinite, Timeout.Infinite);
            scheduleTimer.Change(Properties.Settings.Default.SearchMiliseconds, Timeout.Infinite);
        }

        protected internal void PrepareComboBox<T>(ViewModel<CustomersContainer, T> fetcher,
            ComboBox box,
            Func<T, Customer, bool> locate,
            Action<T, Customer> assign) where T : INotifyPropertyChanged
        {

            // If the location fetcher is busy, wait for it...
            if (fetcher.ViewSource == null && fetcher.IsBusy)
            {
                fetcher.WaitFor();
            }
            PrepareComboBox<T>((IViewModelDelegate<T>)this, box, locate, assign);
        }

        protected internal void PrepareComboBox<T>(IViewModelDelegate<T> model,
            ComboBox box,
            Func<T, Customer, bool> locate,
            Action<T, Customer> assign) where T : INotifyPropertyChanged
        {
            if (model.ViewSource != null && box.ItemsSource != model.ViewSource)
            {
                if (box.HasItems)
                {
                    try
                    {
                        box.ItemsSource = null;
                        box.Items.Clear();
                    }
                    catch (Exception ex)
                    {
                        // Really? Chicken-and-egg conundrum here
                        Debug.WriteLine(ex.ToString());
                    }
                }

                box.ItemsSource = model.ViewSource;
            }
            Customer cust = box.DataContext as Customer;
            if (cust != null)
            {
                // Lookup the location
                var lookup = model.ViewSource.Where(x => locate(x, cust)).FirstOrDefault();
                assign(lookup, cust);
                box.SelectedItem = lookup;
                
            }
            // I have too do this because the box does not change the property on the context
            box.Tag = typeof(T).Name;
            box.SelectionChanged -= Box_SelectionChanged;
            box.SelectionChanged += Box_SelectionChanged;
        }

        void Box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            if (box == null)
                return;

            Customer cust = box.DataContext as Customer;
            if (cust == null)
                return;

            switch(box.Tag.ToString())
            {
                case "Location":
                    cust.SetLocationSilent(box.SelectedItem as Location);
                    break;
                case "Category":
                    cust.SetCategorySilent(box.SelectedItem as Category);
                    break;

            }
            
        }

        /// <summary>
        ///   Handles the updates to the WCF data service of customer data.
        /// </summary>
        protected virtual void HandleCustomersCollectionChanged(ViewModel<CustomersContainer, Customer> fetcher, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    foreach(Customer cust in e.NewItems.OfType<Customer>())
                    {
                        string validate = Validate(fetcher.DataService, cust);
                        if (!string.IsNullOrEmpty(validate))
                        {
                            MessageBox.Show(validate, "Check Again");
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // TODO
                    break;
            }
        }

        /// <summary>
        ///   Adds or updates customer to the data service. If successful returns <c>null</c>, otherwise an error message.
        /// </summary>
        protected internal string Validate(CustomersContainer service, Customer cust)
        {
            if (cust.Category == null && cust.CategoryId != 0)
            {
                cust.Category = service.Categories.Where((c) => c.Id == cust.CategoryId).FirstOrDefault();
            }
            if (cust.Location == null && cust.LocationId != 0)
            {
                cust.Location = service.Locations.Where((l) => l.Id == cust.LocationId).FirstOrDefault();
            }
            if (string.IsNullOrEmpty(cust.Gender))
            {
                cust.Gender = "M";
            }
            else
            {
                cust.Gender = cust.Gender.ToUpper();
                if (cust.Gender != "M" && cust.Gender != "F")
                {
                    return "Invalid value specified for Gender. Needs to be 'M' (male) or 'F' (female).";
                }
            }
            if (cust.Category != null && cust.Category.Id != cust.CategoryId)
            {
                cust.CategoryId = cust.Category.Id;
            }
            if (cust.Location != null && cust.Location.Id != cust.LocationId)
            {
                cust.LocationId = cust.Location.Id;
            }
            if (cust.LocationId == 0 || cust.CategoryId == 0)
            {
                return "The Location as well as the Category of the customer must be set.";
            }
            if (string.IsNullOrEmpty(cust.Name))
            {
                return "Please specify a name.";
            }
            if (string.IsNullOrEmpty(cust.AddressLine1))
            {
                return "Please specify an address";
            }
            if (pendingCustomers.Add(cust))
            {
                if (CheckSaveState != null)
                {
                    CheckSaveState(this, EventArgs.Empty);
                }
                if (cust.Id != 0f)
                {
                    service.UpdateObject(cust);
                }
                else
                {
                    service.AddToCustomers(cust);
                }
            }
            return null;
        }

        /// <summary>
        ///   Saves changes to the data service.
        /// </summary>
        protected void SaveChanges(CustomersContainer service)
        {
            if (pendingCustomers.Count == 0)
                return;

            foreach (var cust in pendingCustomers)
            {
                // Ensure the links are set.
                string validate = Validate(service, cust);
                if (!string.IsNullOrEmpty(validate))
                {
                    throw new InvalidOperationException(validate); 
                }
            }
            service.SaveChanges();
            // We make sure we clear our pending hash set
            pendingCustomers.Clear();
            if (CheckSaveState != null)
            {
                CheckSaveState(this, EventArgs.Empty);
            }
        }

        protected void DiscardChanges()
        {   
            pendingCustomers.Clear();
        }

        /// <summary>
        ///   Must implement for saving customer data.
        /// </summary>
        public abstract void Save();

        /// <summary>
        ///   Must implement to discard customer data
        /// </summary>
        public abstract void Discard();

        /// <summary>
        ///   Happens before the search happens in the UI thread context.
        /// </summary>
        protected internal abstract void BeforeSearch();

        /// <summary>
        ///   Custom handling of the current search text for this page.
        /// </summary>
        protected internal abstract void PerformSearch(string search);

        protected internal abstract void AddCustomer(Customer newCustomer);
        #endregion



        
        
    }
}
