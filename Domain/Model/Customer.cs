using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.DataAccess.Sql
{
    // Note: Rest of class in entity model designer file
    public partial class Customer
    {
        public override string ToString()
        {
            return string.Format(@"Customer > ""{0}"". Gender: {1}. DOB: {2}", Name, Gender, DOB);
        }
    }
}
