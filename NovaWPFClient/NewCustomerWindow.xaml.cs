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
using System.Windows.Shapes;
using Nova.UI.DataService;

namespace Nova.UI
{
    /// <summary>
    /// Interaction logic for NewCustomer.xaml
    /// </summary>
    public partial class NewCustomerWindow : Window
    {
        public NewCustomerWindow(ContentPage currentPage)
        {
            InitializeComponent();
            CurrentPage = currentPage;
            
        }

        public ContentPage CurrentPage { get; private set; }
        
        public Customer NewCustomer {get; private set; }

        private Customer tempCustomer;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            System.Windows.Data.CollectionViewSource customersViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("customersViewSource")));
            // Load data by setting the CollectionViewSource.Source property:
            // customersViewSource.Source = [generic data source]

            Customer cust = tempCustomer = new Customer()
            {
                Gender = "M",
                DOB = new DateTime(1970, 1, 1),
                Location = CurrentPage.LocationModel.DataService.Locations.FirstOrDefault(),
                Category = CurrentPage.CategoryModel.DataService.Categories.FirstOrDefault()
            };
            List<Customer> list = new List<Customer>();
            list.Add(cust);
            customersViewSource.Source = list;
        }
        
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string validate = CurrentPage.Validate(CurrentPage.CustomerModel.DataService, tempCustomer);
            if (!string.IsNullOrEmpty(validate))
            {
                MessageBox.Show(validate, "Oops", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            NewCustomer = tempCustomer;
            this.Close();
        }

        private void Ignore_Click(object sender, RoutedEventArgs e)
        {
            NewCustomer = null;
            this.Close();
        }

        
        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            if (box == null)
                return;

            switch (box.Name)
            {
                case "comboLocation":
                    CurrentPage.PrepareComboBox<Location>(((IViewModelDelegate<Location>)CurrentPage), box, (l, c) => l.Id == c.LocationId, (l, c) => c.SetLocationSilent(l));
                    break;
                case "comboCategory":
                    CurrentPage.PrepareComboBox<Category>(((IViewModelDelegate<Category>)CurrentPage), box, (ct, c) => ct.Id == c.CategoryId, (ct, c) => c.SetCategorySilent(ct));
                    break;

            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            if (box == null)
                return;

            
            switch (box.Name)
            {
                case "comboLocation":
                    tempCustomer.Location = box.SelectedItem as Location;
                    break;
                case "comboCategory":
                    tempCustomer.Category = box.SelectedItem as Category;
                    break;
            }

        }
    }
}
