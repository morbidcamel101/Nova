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
    /// Interaction logic for CategorySummary.xaml
    /// </summary>
    public partial class CategorySummary : ContentPage,
        IViewModelDelegate<Location>,
        IViewModelDelegate<Customer>,
        IViewModelDelegate<Category>
    {
        public CategorySummary()
        {
            InitializeComponent();
        }

        #region Properties & Fields
        public CollectionViewSource CategoryViewSource { get; private set; }
        public CollectionViewSource CustomersViewSource { get; private set; }
        public ViewModel<CustomersContainer, Location> LocationsModel { get; private set; }
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
            // Note: In this case it might make more sense to fire off Customer queries instead within the current category ?
            if (CategoryModel.WaitFor(new TimeSpan(0, 0, 3)))
            {
                CategoryModel.StartQueryAsync((s) => s.Categories.Where( (c) => c.Name.Contains(search) ).OrderBy( c => c.Name ).Take(100) );
            }

        }
        #endregion

        #region IViewModelTarget Implementations
        IQueryable<Category> IViewModelDelegate<Category>.ViewSource
        {
            get
            {
                return CategoryViewSource.Source as IQueryable<Category>;
            }
            set
            {
                CategoryViewSource.Source = value;
            }
        }


        bool IViewModelDelegate<Category>.IsLoading
        {
            get
            {
                return labelLoading.Visibility == System.Windows.Visibility.Visible; ;
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

        private Location[] locations;
        IQueryable<Location> IViewModelDelegate<Location>.ViewSource
        {
            get
            {
                return locations == null ? null : locations.AsQueryable();
            }
            set
            {
                locations = value.ToArray();
            }
        }

        private bool locationsLoading;
        bool IViewModelDelegate<Location>.IsLoading
        {
            get
            {
                return locationsLoading;
            }
            set
            {
                locationsLoading = value;
            }
        }
        #endregion

        #region Event Subscriptions
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CategoryViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("categoryViewSource")));
            CategoryModel = new ViewModel<CustomersContainer, Category>(ServiceUri, this);
            LoadDefaultData();
            
            LocationsModel = new ViewModel<CustomersContainer, Location>(ServiceUri, this);
            LocationsModel.StartQueryAsync((s) => s.Locations.OrderBy(l => l.Country + l.State + l.City).Take(Properties.Settings.Default.MaxLocations));

            CustomersViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("customersViewSource")));
            CustomerModel = new ViewModel<CustomersContainer, Customer>(ServiceUri, this);

            webBrowser.Navigate(new Uri(Properties.Settings.Default.CustomerCategoryStatsUri));
        }

        private void LoadDefaultData()
        {
            CategoryModel.StartQueryAsync((s) => s.Categories.OrderBy((c) => c.Name).Take(100));
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
                    case "comboLocation":
                        PrepareComboBox(LocationsModel, box, (l, c) => l.Id == c.LocationId, (l, c) => c.SetLocationSilent(l));
                        break;
                }
            }
            finally
            {
                customers.EndUpdate();
            }
        }

        private void listBoxCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource != listBoxCategory)
                return;

            foreach (var sel in e.AddedItems)
            {
                Category cat = sel as Category;
                if (cat == null)
                    continue;

                labelCustomersLoading.Visibility = System.Windows.Visibility.Visible;
                Schedule(() =>
                {
                    CustomerModel.StartQueryAsync((s) => s.Customers.Where(c => c.CategoryId == cat.Id).OrderBy(c => c.Name).Take(Properties.Settings.Default.MaxViewCustomers));
                });
                break;
            }
        }

        void Customers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HandleCustomersCollectionChanged(CustomerModel, e);
        }
        #endregion
    }
}
