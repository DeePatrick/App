namespace App.DataAccess
{
    public class CustomerDataAccessProxy : ICustomerDataAccess
    {
        public void AddCustomer(Customer customer)
        {
            CustomerDataAccess.AddCustomer(customer);
        }
    }
}