using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using Nova.UI.DataService;
using System.Data.Services.Client;
using System.Threading;

namespace Nova.UI
{
    /// <summary>
    ///   A view model class and helper for fetching data in the background using LINQ over OData.
    /// </summary>
    public class ViewModel<S,T> 
        where S: DataServiceContext
        where T: INotifyPropertyChanged 
    {
        public ViewModel(Uri serviceUri, IViewModelDelegate<T> target)
        {
            ServiceUri = serviceUri;
            Target = target;
        }

        #region Properties & Fields
        public delegate IQueryable<T> QueryDelegate(S serviceContext);
        
        private BackgroundWorker dataFetcher;
        private ManualResetEvent waitHandle = new ManualResetEvent(true);
        
        public IViewModelDelegate<T> Target { get; private set; }

        public Uri ServiceUri { get; private set; }

        public S DataService { get; private set; }

        public bool IsBusy 
        {
            get { return dataFetcher != null && dataFetcher.IsBusy; }
        }

        public IQueryable<T> ViewSource
        {
            get { return Target.ViewSource; }
        }
        #endregion

        #region Public Methods
        public void StartQueryAsync(QueryDelegate query)
        {
            if (dataFetcher == null)
            {
                dataFetcher = new BackgroundWorker();
            }
            // Make sure we done start a thread concurrently
            if (dataFetcher.IsBusy)
            {
                if (!WaitFor(new TimeSpan(0, 0, 2)))
                    return;
            }

            Target.Dispatch(() => Target.IsLoading = true);
            waitHandle.Reset(); // Signal that we're busy
            
            dataFetcher.DoWork += new DoWorkEventHandler(DataFetcher_DoWork);
            dataFetcher.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DataFetcher_RunWorkerCompleted);
            dataFetcher.RunWorkerAsync(query);
        }

        public bool WaitFor()
        {
            return this.WaitFor(TimeSpan.MinValue);
        }


        public bool WaitFor(TimeSpan timeout)
        {
            if (dataFetcher == null)
                return true;

            if (timeout == TimeSpan.MinValue)
                return waitHandle.WaitOne();
            else
                return waitHandle.WaitOne(timeout);
        }
        #endregion

        #region Event Subscriptions
        void DataFetcher_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    MessageBox.Show(e.Error.Message, "Oops!", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
                    return;
                }
                var data = e.Result as IQueryable<T>;
                if (data == null)
                    MessageBox.Show("Oops! No data was returned.", "Nothing");

                
                Target.Dispatch( () => Target.ViewSource = data );
            }
            finally
            {
                Target.Dispatch( () => Target.IsLoading = false );
                waitHandle.Set(); // Signal that we are ready
            }
        }

        void DataFetcher_DoWork(object sender, DoWorkEventArgs e)
        {
            QueryDelegate query = (QueryDelegate)e.Argument;
            if (DataService == null)
            {
                // Initialize  OData service
                DataService = Activator.CreateInstance(typeof(S), ServiceUri) as S; 
            }
            e.Result = query(DataService);
        }
        #endregion



       
    }
}
