namespace ProjectUnderTest
{
    using System.Collections.Generic;

    using DAL;

    public class ClassUnderTest
    {
        public List<string> MethodUnderTest()
        {
            return new CustomerDal().GetAllCustomerNames(1, "boteco-da-esquina");
        }
    }
}
