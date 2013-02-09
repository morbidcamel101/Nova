using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.Test.ConsoleApp.CustomersReference
{
    public partial class Customer 
    {
        public override string ToString()
        {
            return string.Format(@"Web Customer > ""{0}"". Gender: {1}. DOB: {2}", Name, Gender, DOB);
        }
    }
}
