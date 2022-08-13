using App.DataAccess;
using App.Repositories;
using App.Services;
using System;

namespace App
{
    public class CustomerService
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICustomerCreditService _customerCreditService;
        private readonly ICustomerDataAccess _customerDataAccess;

        public CustomerService(IDateTimeProvider dateTimeProvider, ICompanyRepository companyRepository, ICustomerCreditService customerCreditService, ICustomerDataAccess customerDataAccess)
        {
            _dateTimeProvider = dateTimeProvider;
            _companyRepository = companyRepository;
            _customerCreditService = customerCreditService;
            _customerDataAccess = customerDataAccess;
        }

        public CustomerService() : this(new DateTimeProvider(), new CompanyRepository(), new CustomerCreditServiceClient(), new CustomerDataAccessProxy())
        {

        }

        public bool AddCustomer(string firname, string surname, string email, DateTime dateOfBirth, int companyId)
        {
            if (string.IsNullOrEmpty(firname) || string.IsNullOrEmpty(surname))
            {
                return false;
            }

            if (!(email.Contains("@") && email.Contains(".")))
            {
                return false;
            }

            var now = _dateTimeProvider.DateTimeNow;
            var age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;

            if (age < 21)
            {
                return false;
            }

            var company = _companyRepository.GetById(companyId);

            var customer = new Customer
            {
                Company = company,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                Firstname = firname,
                Surname = surname
            };

            if (company.Name == "VeryImportantClient")
            {
                // Skip credit check
                customer.HasCreditLimit = false;
            }
            else if (company.Name == "ImportantClient")
            {
                // Do credit check and double credit limit
                customer.HasCreditLimit = true;

                var creditLimit = _customerCreditService.GetCreditLimit(customer.Firstname, customer.Surname, customer.DateOfBirth);
                creditLimit = creditLimit * 2;
                customer.CreditLimit = creditLimit;

            }
            else
            {
                // Do credit check
                customer.HasCreditLimit = true;

                var creditLimit = _customerCreditService.GetCreditLimit(customer.Firstname, customer.Surname, customer.DateOfBirth);
                customer.CreditLimit = creditLimit;

            }

            if (customer.HasCreditLimit && customer.CreditLimit < 500)
            {
                return false;
            }

            _customerDataAccess.AddCustomer(customer);

            return true;
        }
    }
}
