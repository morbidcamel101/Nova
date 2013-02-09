using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Nova.UI.DataService;
using System.Diagnostics;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Nova.UI
{
    /// <summary>
    ///   To make the implementation cleaner, this collection fires a collection changed (replace) on a customer property change event.
    /// </summary>
    public class CustomersCollection: ObservableCollection<Customer>
    {
        protected override void InsertItem(int index, Customer item)
        {
            item.PropertyChanged -= Handle_PropertyChanged;
            item.PropertyChanged += Handle_PropertyChanged;
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            this[index].PropertyChanged -= Handle_PropertyChanged;
            base.RemoveItem(index);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (Updating)
                return;

            base.OnCollectionChanged(e);
        }

        public void Load(IEnumerable<Customer> value)
        {
            foreach (var cust in value)
            {   
                this.Add(cust);
                
            }
        }

        public void Unhook()
        {
            foreach (Customer cust in this)
            {
                cust.PropertyChanged -= Handle_PropertyChanged;
            }
        }

        

        public bool Updating {get; private set;}

        public void BeginUpdate()
        {
            Updating = true;
        }

        public void EndUpdate()
        {
            Updating = false;
        }

        void Handle_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var propChanged = (INotifyPropertyChanged)sender;
            propChanged.PropertyChanged -= Handle_PropertyChanged;
            try
            {
                // Bubble up a collection changed replace event
                List<Customer> items = new List<Customer>();
                items.Add((Customer)sender);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, items, items, IndexOf((Customer)sender)));
            }
            finally
            {
                propChanged.PropertyChanged += Handle_PropertyChanged;
            }
        }
    }
}
