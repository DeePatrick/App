using App;
using App.DataAccess;
using App.Models;
using App.Repositories;
using App.Services;
using AutoFixture;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace TestRefactoring.UnitTests
{
    public class CustomerServiceTests
    {
        private readonly CustomerService _sut;
        private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        private readonly ICompanyRepository _companyRepository = Substitute.For<ICompanyRepository>();
        private readonly ICustomerCreditService _customerCreditService = Substitute.For<ICustomerCreditService>();
        private readonly ICustomerDataAccess _customerDataAccess = Substitute.For<ICustomerDataAccess>();
        private readonly IFixture _fixture = new Fixture();

        public CustomerServiceTests()
        {
            _sut = new CustomerService(_dateTimeProvider,_companyRepository, _customerCreditService,_customerDataAccess);
        }

        [Fact]
        public void AddCustomer_ShouldCreateCustomer_UsingValidParameters()
        {
            //Arrange
            const int companyId = 1;
            const string firname = "Patrick";
            const string surname = "Okudo";
            const string email = "okudopato@gmail.com";
            var dateOfBirth = new DateTime(1983, 10, 30);


            var company = _fixture.Build<Company>()
                .With(c => c.Id, companyId)
                .Create();
            _dateTimeProvider.DateTimeNow.Returns(new DateTime(2022, 08, 13));
            _companyRepository.GetById(companyId).Returns(company);
            _customerCreditService.GetCreditLimit(firname, surname, dateOfBirth).Returns(501);

            //Act
            var result = _sut.AddCustomer(firname, surname, email, dateOfBirth, companyId);

            //Assert
            result.Should().BeTrue();
            _customerDataAccess.Received(1).AddCustomer(Arg.Any<Customer>());

        }

        [Theory]
        [InlineData("", "Okudo","okudopato@gmail.com", 1983)]
        [InlineData("Patrick", "", "okudopato@gmail.com", 1983)]
        [InlineData("Patrick", "Okudo", "okudo@com", 1983)]
        [InlineData("Patrick", "Okudo", "okudo.com", 1983)]
        [InlineData("Patrick", "Okudo", "okudopato@gmail.com", 2003)]
        public void AddCustomer_ShouldNotCreateCustomer_WithInvalidInputDetails(string firstName, string surname, string email, int yearOfBirth)
        {
            //Arrange
            const int companyId = 1;
            var dateOfBirth = new DateTime(yearOfBirth, 1, 1);
            var company = _fixture.Build<Company>()
                .With(c => c.Id, () => companyId)
                .Create();
            _dateTimeProvider.DateTimeNow.Returns(new DateTime(2022, 08, 13));
            _companyRepository.GetById(Arg.Is(companyId)).Returns(company);
            _customerCreditService.GetCreditLimit(Arg.Is(firstName), Arg.Is(surname), Arg.Is(dateOfBirth)).Returns(501);

            //Act
            var result = _sut.AddCustomer(firstName, surname, email, dateOfBirth, 1);

            //Arrange
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("OrdinaryClient", true, 501, 501)]
        [InlineData("ImportantClient", true, 501, 1002)]
        [InlineData("VeryImportantClient", false, 0, 0)]
        public void AddCustomer_ShouldCreateCustomerWithCorrectCreditLimit_BasedCompanyNameClassification(string companyName, bool hasCreditlimit,int initialCreditLimit, int finalCreditLimit)
        {
            //Arrange
            const int companyId = 1;
            const string firstName = "Patrick";
            const string surname = "Okudo";
            const string email = "okudopato@gmail.com";
            var dateOfBirth = new DateTime(1983, 10, 30);
            var company = _fixture.Build<Company>()
                .With(c => c.Id, companyId)
                .With(c => c.Name , companyName)
                .Create();

            _dateTimeProvider.DateTimeNow.Returns(new DateTime(2022, 08, 13));
            _companyRepository.GetById(Arg.Is(companyId)).Returns(company);
            _customerCreditService.GetCreditLimit(Arg.Is(firstName), Arg.Is(surname), Arg.Is(dateOfBirth)).Returns(initialCreditLimit);
            
            //Act

            var result = _sut.AddCustomer(firstName, surname, email, dateOfBirth, companyId);

            //Assert
            result.Should().BeTrue();
            _customerDataAccess.Received(1)
                .AddCustomer(Arg.Is<Customer>(c => c.HasCreditLimit == hasCreditlimit && c.CreditLimit == finalCreditLimit));

        }

        [Fact]
        public void AddCustomer_ShouldNotCreateCustomer_WithCreditLimitLessthan500()
        {
            //Arrange
            const int companyId = 1;
            const string firstName = "Patrick";
            const string surname = "Okudo";
            const string email = "okudopato@gmail.com";
            var dateOfBirth = new DateTime(1983, 10, 30);
            var company = _fixture.Build<Company>()
                .With(c => c.Id, () => companyId)
                .Create();

            _dateTimeProvider.DateTimeNow.Returns(new DateTime(2022, 08, 13));
            _companyRepository.GetById(Arg.Is(companyId)).Returns(company);
            _customerCreditService.GetCreditLimit(Arg.Is(firstName), Arg.Is(surname), Arg.Is(dateOfBirth)).Returns(499);

            //Act

            var result = _sut.AddCustomer(firstName, surname, email, dateOfBirth, companyId);

            //Assert
            result.Should().BeFalse();

        }
    }
}