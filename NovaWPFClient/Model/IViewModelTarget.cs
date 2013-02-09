using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Nova.UI
{
    /// <summary>
    ///   Implements the basic principal of the Nova Sample data view model. A means to indicate that it's loading and the data source.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IViewModelDelegate<T> where T : INotifyPropertyChanged
    {
        /// <summary>
        ///   This is the main data source for the background target of type T
        /// </summary>
        IQueryable<T> ViewSource { get; set; }

        /// <summary>
        ///   Buts the target into a loading state
        /// </summary>
        bool IsLoading { get; set; }

        /// <summary>
        ///  A method that needs to be implemented to dispatch cross thread calls. Handled in ContentPage base class.
        /// </summary>
        void Dispatch(Action action);
    }
}
