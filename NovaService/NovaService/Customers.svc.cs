using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.ServiceModel.Web;
using System.Web;
using Nova.DataAccess.Sql;
using System.Linq.Expressions;

namespace NovaService
{
    // I use a data service to expose the entity framework model through the OData protocol.
    public class Customers : DataService< CustomersContainer >
    {
        // This method is called only once to initialize service-wide policies.
        public static void InitializeService(DataServiceConfiguration config)
        {
            // TODO: set rules to indicate which entity sets and service operations are visible, updatable, etc.
            // Examples:
            config.SetEntitySetAccessRule("*", EntitySetRights.All);
            // config.SetServiceOperationAccessRule("MyServiceOperation", ServiceOperationRights.All);
            //config.SetEntitySetPageSize("*", 25);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
        }

        [QueryInterceptor("CustomerTrackings")]
        public Expression<Func<CustomerTracking, bool>> OnCustomerTrackingQuery()
        {
            // TODO Here we can filter the data for a specific user.
            return t => true;

        }

        private string GetOperation(UpdateOperations operation)
        {
            // returns C.r.U.D
            switch (operation)
            {
                case UpdateOperations.Add:
                    return "C";
                case UpdateOperations.Change:
                    return "U";
                case UpdateOperations.Delete:
                    return "D";
                default:
                    return "U";
            }
        }

        [ChangeInterceptor("Customers")]
        public void OnChangeCustomers(Customer customer, UpdateOperations operations)
        {
            string operation = GetOperation(operations);
            customer.CustomerTrackings.Add(CustomerTracking.CreateCustomerTracking(0, operation, DateTime.Now, customer.Id));

        }

    }
}
