using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    using Replay;


    public class CustomerDal
    {
        [Replay]
        public List<string> GetAllCustomerNames(int clientId, string storeId)
        {
            return new List<string>() { "A", "B", "C", "D" };
        }
    }
}
