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
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Services.Client;

namespace Nova.UI
{
    /// <summary>
    /// Interaction logic for Customers.xaml
    /// </summary>
    public partial class CustomersPage : ContentPage, 
        IViewModelDelegate<Customer>,
        IViewModelDelegate<Location>,
        IViewModelDelegate<CustomerTracking>,
        IViewModelDelegate<Category>
    {
        public CustomersPage():base()
        {   
            InitializeComponent();
        }

        #region Properties & Fields
        public CollectionViewSource CustomerViewSource { get; private set; }
        public CollectionViewSource TrackingViewSource { get; private set; }
        #endregion

        #region IViewModelTarget Members
        /// <summary>
        ///   Property explicitly implemented for the Customer Fetcher
        /// </summary>
        private CustomersCollection customers;
        IQueryable<Customer> IViewModelDelegate<Customer>.ViewSource
        {
            get
            {
                return CustomerViewSource.Source as IQueryable<Customer>;
            }
            set
            {

                var col = customers = new CustomersCollection();
                col.Load(value);
                col.CollectionChanged += Customers_CollectionChanged;
                
                CustomerViewSource.Source = col;
            }
        }

        Location[] locationViewSource;
        IQueryable<Location> IViewModelDelegate<Location>.ViewSource
        {
            get
            {
                return locationViewSource.AsQueryable();

            }
            set
            {
                locationViewSource = value.ToArray();

            }
        }

        public bool IsLoading
        {
            get
            {
                return labelLoading.Visibility == System.Windows.Visibility.Visible;
            }
            set
            {
                // Because I set this property in a thread context, I need to dispatch it
                labelLoading.Visibility = value ? Visibility.Visible : Visibility.Hidden;
                Visibility itemVis = value ? Visibility.Hidden : Visibility.Visible;
                customersDataGrid.Visibility = itemVis;
            }
        }

        IQueryable<CustomerTracking> IViewModelDelegate<CustomerTracking>.ViewSource
        {
            get
            {
                return TrackingViewSource.Source as IQueryable<CustomerTracking>;
            }
            set
            {
                TrackingViewSource.Source = value;
            }
        }


        bool IViewModelDelegate<CustomerTracking>.IsLoading
        {
            get
            {
                return false; // TODO - Make loading label for tracking
            }
            set
            {
                return;
            }
        }

        Category[] categoryViewSource;
        IQueryable<Category> IViewModelDelegate<Category>.ViewSource
        {
            get
            {
                return categoryViewSource != null ? categoryViewSource.AsQueryable() : null;
            }
            set
            {
                categoryViewSource = value.ToArray();
            }
        }

        private bool categoryLoading;
        bool IViewModelDelegate<Category>.IsLoading
        {
            get
            {
                return categoryLoading;
            }
            set
            {
                categoryLoading = value;
            }
        }
        #endregion

        #region Overrides
        protected internal override void AddCustomer(Customer newCustomer)
        {
            customers.Add(newCustomer);
        }

        public override void Save()
        {
            this.SaveChanges(CustomerModel.DataService);
        }

        public override void Discard()
        {
            // Recreate the fetcher ... Microsoft why is there no discard?
            CustomerModel = new ViewModel<CustomersContainer, Customer>(ServiceUri, this);
            this.DiscardChanges();
        }

        protected internal override void BeforeSearch()
        {
            labelLoading.Content = "Searching...";
            labelLoading.Visibility = System.Windows.Visibility.Visible;
        }

        protected internal override void PerformSearch(string search)
        {
            Dispatch(() =>
            {
                labelLoading.Content = "Loading...";
                labelLoading.Visibility = System.Windows.Visibility.Hidden;
            });
            if (string.IsNullOrEmpty(search))
            {
                LoadDefaultData();
                return;
            }
            if (CustomerModel.WaitFor(new TimeSpan(0, 0, 3)))
            {
                CustomerModel.StartQueryAsync((s) => s.Customers.Where(c => c.Name.Contains(search)));
            }
            
        }
        #endregion

        #region Event Subscriptions
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CustomerViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("customersViewSource")));
            TrackingViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("customerTrackingsViewSource")));

            // Reviewer Note: This class handles the OData queries to the WCF DataService
            CustomerModel = new ViewModel<CustomersContainer, Customer>(ServiceUri, this);
            LocationModel = new ViewModel<CustomersContainer, Location>(ServiceUri, this);
            TrackingModel = new ViewModel<CustomersContainer, CustomerTracking>(ServiceUri, this);
            CategoryModel = new ViewModel<CustomersContainer, Category>(ServiceUri, this);

            // Fetch data needed
            LoadDefaultData();
            LocationModel.StartQueryAsync((s) => s.Locations.OrderBy(l => l.Country + l.State + l.City));
            CategoryModel.StartQueryAsync((s) => s.Categories.OrderBy(c => c.Name));
        }

        private void LoadDefaultData()
        {
            CustomerModel.StartQueryAsync((s) => s.Customers.OrderBy(c => c.Name).Take(Properties.Settings.Default.MaxViewCustomers));
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {

            ComboBox box = sender as ComboBox;
            if (box == null)
                return;

            customers.BeginUpdate();
            try
            {
                switch (box.Name)
                {
                    case "comboLocation":
                        PrepareComboBox(LocationModel, box, (l, c) => l.Id == c.LocationId, (l, c) => c.SetLocationSilent(l));
                        break;
                    case "comboCategory":
                        PrepareComboBox(CategoryModel, box, (ct, c) => ct.Id == c.CategoryId, (ct, c) => c.SetCategorySilent(ct));
                        break;

                }
            }
            finally
            {
                customers.EndUpdate();
            }
        }

        private void customersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource != customersDataGrid)
                return;
            try
            {
                foreach (Customer item in e.AddedItems.OfType<Customer>())
                {
                    TrackingModel.StartQueryAsync((s) => s.CustomerTrackings.Where(t => t.CustomerId == item.Id));
                    break;
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
            }

        }

        void Customers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HandleCustomersCollectionChanged(CustomerModel, e);
        }

        
        #endregion

    }

        
}
