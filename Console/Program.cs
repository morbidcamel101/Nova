using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nova.DataAccess.Sql;
using System.Collections;
using WcfContainer = Nova.Test.ConsoleApp.CustomersReference.CustomersContainer;
using Nova.Test.ConsoleApp.Properties;

namespace Nova.Test.ConsoleApp
{
    class Program
    {
        public static void WriteLine(string line, params string[] args)
        {
            if (args.Length > 0)
                line = string.Format(line, args);

            Console.WriteLine(line);
        }

        public static void Write(string line, params string[] args)
        {
            if (args.Length > 0)
                line = string.Format(line, args);

            Console.Write(line);
        }

        static void Main(string[] args)
        {
            do
            {
                WriteLine("NOVA Test Console");
                WriteLine("");
                WriteLine("CustomersServiceUri (settings): {0}", Settings.Default.CustomerServiceUri);
                WriteLine("  - Please edit Console.exe.config with a new uri");
                WriteLine(" [1] Direct Sql Customer Query");
                WriteLine(" [2] OData WCF Data Services Customer Query");
                WriteLine(" [Q]uit");
                Write("-> ");

                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        TestSqlDirect();
                        break;

                    case ConsoleKey.NumPad2:
                    case ConsoleKey.D2:
                        TestRestfull();
                        break;

                    case ConsoleKey.Q:
                        return;

                    default:
                        break;
                }
            }
            while (true);

        }

        private static void FeedbackQuery(string mode, string message, IEnumerable objects)
        {
            WriteLine("{0}----------------------------------------------------------------------", mode);
            WriteLine(mode+": "+message);
            Console.WriteLine(mode+": Enter to continue...");
            Console.ReadLine();
            WriteLine("");
            foreach (var obj in objects)
            {
                WriteLine(obj.ToString());
            }
            WriteLine("");
            WriteLine(mode+": "+message);
            WriteLine("------------------------===================---------------------------");
            
            
                
        }

        private static void TestSqlDirect()
        {
            string mode = "DIRECT SQL";
            using (CustomersContainer db = new CustomersContainer())
            {
                FeedbackQuery(mode,"Showing just the males!", db.Customers.Where(c => c.Gender == "M"));
                FeedbackQuery(mode,"Showing just the females!", db.Customers.Where(c => c.Gender == "F"));
                FeedbackQuery(mode,"Showing all those in Arizona...", db.Customers.Where( c => c.Location.State == "Arizona" ));

            }
        }

        private static void TestRestfull()
        {
            string mode = "WCF ODATA";
            var svc = new WcfContainer(new Uri(Nova.Test.ConsoleApp.Properties.Settings.Default.CustomerServiceUri));
            
            FeedbackQuery(mode,"Showing just the males!", svc.Customers.Where(c => c.Gender == "M"));
            FeedbackQuery(mode, "Showing just the females!", svc.Customers.Where(c => c.Gender == "F"));
            FeedbackQuery(mode, "Showing all those in Arizona...", svc.Customers.Where(c => c.Location.State == "Arizona"));
            
        }

    }
}
