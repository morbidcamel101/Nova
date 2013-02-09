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
using System.Collections.Specialized;

namespace Nova.UI
{
    /// <summary>
    /// Interaction logic for LocationSummary.xaml
    /// </summary>
    public partial class LocationSummary : ContentPage, 
        IViewModelDelegate<Location>,
        IViewModelDelegate<Customer>,
        IViewModelDelegate<Category>
    {
        public LocationSummary():base()
        {
            InitializeComponent();
        }

        #region Properties & Fields
        public CollectionViewSource LocationsViewSource { get; private set; }
        public CollectionViewSource CustomersViewSource { get; private set; }
        public ViewModel<CustomersContainer, Location> LocationsModel { get; private set; }
        public ViewModel<CustomersContainer, Customer> CustomerModel { get; private set; }
        public ViewModel<CustomersContainer, Category> CategoryModel { get; private set; }
    
        
        #endregion

        #region Override
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
           
            if (LocationsModel.WaitFor(new TimeSpan(0,0,3)))
            {
                LocationsModel.StartQueryAsync((s) => s.Locations.Where(l => l.City.Contains(search) 
                    || l.Country.Contains(search) || l.State.Contains(search)).OrderBy( l => l.Country + l.State + l.City ) );
            }
           
        }
        #endregion

        #region IViewModelTarget Implementations
        IQueryable<Location> IViewModelDelegate<Location>.ViewSource
        {
            get
            {
                return LocationsViewSource.Source as IQueryable<Location>;
            }
            set
            {
                LocationsViewSource.Source = value;
            }
        }

        bool IViewModelDelegate<Location>.IsLoading
        {
            get
            {
                return labelLoading.Visibility == System.Windows.Visibility.Visible;
            }
            set
            {
                labelLoading.Visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }

        bool IViewModelDelegate<Customer>.IsLoading
        {
            get
            {
                return labelCustomersLoading.Visibility == System.Windows.Visibility.Visible;
            }
            set
            {
                labelCustomersLoading.Visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private CustomersCollection customers;
        IQueryable<Customer> IViewModelDelegate<Customer>.ViewSource
        {
            get
            {
                return CustomersViewSource.Source as IQueryable<Customer>;
            }
            set
            {

                var col = customers = new CustomersCollection();
                col.Load(value);
                col.CollectionChanged += Customers_CollectionChanged;

                CustomersViewSource.Source = col;
            }
        }

        private Category[] categories;
        IQueryable<Category> IViewModelDelegate<Category>.ViewSource
        {
            get
            {
                return categories == null ? null : categories.AsQueryable();
            }
            set
            {
                categories = value.ToArray();
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

        #region Event Subscriptions
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LocationsViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("locationsViewSource")));
            LocationsModel = new ViewModel<CustomersContainer, Location>(ServiceUri, this);
            LoadDefaultData();
            
            CustomersViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("customersViewSource")));
            CustomerModel = new ViewModel<CustomersContainer, Customer>(ServiceUri, this);

            CategoryModel = new ViewModel<CustomersContainer, Category>(ServiceUri, this);
            CategoryModel.StartQueryAsync((s) => s.Categories.OrderBy((c) => c.Name));

            webBrowser.Navigate(new Uri(Properties.Settings.Default.CustomerLocationStatsUri));
        }

        private void LoadDefaultData()
        {
            LocationsModel.StartQueryAsync((s) => s.Locations.OrderBy(l => l.Country + l.State + l.City).Take(Properties.Settings.Default.MaxLocations));
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {

            ComboBox box = sender as ComboBox;
            if (box == null || customers == null)
                return;

            customers.BeginUpdate();
            try
            {
                switch (box.Name)
                {
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

        private void listBoxLocation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource != listBoxLocation)
                return;

            foreach (var sel in e.AddedItems)
            {
                Location loc = sel as Location;
                if (loc == null)
                    continue;

                labelCustomersLoading.Visibility = System.Windows.Visibility.Visible;
                Schedule(() =>
                {
                    CustomerModel.StartQueryAsync((s) => s.Customers.Where(c => c.LocationId == loc.Id).OrderBy(c => c.Name));
                });
            }
        }

        void Customers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HandleCustomersCollectionChanged(CustomerModel, e);
        }
        #endregion
    }
}
