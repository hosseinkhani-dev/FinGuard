using FinGuard.Domain.Entities;
using FinGuard.Domain.Exceptions;
using FluentAssertions;

namespace FinGuard.UnitTests.DomainTests;

public class TransactionTests
{
    private readonly Guid _validTransactionFileId;
    private readonly DateTime _validDate;

    public TransactionTests()
    {
        _validTransactionFileId = Guid.NewGuid();
        _validDate = new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc);
    }

    [Fact]
    public void Constructor_WithValidData_ShouldInitializeCorrectly()
    {
        // Arrange
        var employeeName = "John Doe";
        var department = "Finance";
        var cardNumber = "1234-5678-9012-3456";
        var amount = 150.50m;
        var merchant = "Acme Corp";
        var currency = "usd";
        var description = "Software Subscription  ";

        // Act
        var transaction = new Transaction(
            _validTransactionFileId,
            employeeName,
            department,
            _validDate,
            cardNumber,
            amount,
            merchant,
            currency,
            description);

        // Assert
        transaction.Id.Should().NotBeEmpty();
        transaction.TransactionFileId.Should().Be(_validTransactionFileId);
        transaction.EmployeeName.Should().Be("John Doe");
        transaction.Department.Should().Be("Finance");
        transaction.TransactionDate.Should().Be(_validDate);
        transaction.CardNumber.Should().Be("1234-5678-9012-3456");
        transaction.Amount.Should().Be(amount);
        transaction.Merchant.Should().Be("Acme Corp");
        transaction.Currency.Should().Be("USD"); // Verifies ToUpperInvariant()
        transaction.Description.Should().Be("Software Subscription"); // Verifies Trim()
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidEmployeeName_ShouldThrowDomainException(string invalidName)
    {
        // Act
        Action act = () => new Transaction(_validTransactionFileId, invalidName, "Finance", _validDate, "1234", 10m, "Merchant", "USD", "Desc");

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Employee name is required.");
    }

    [Fact]
    public void Constructor_WithEmployeeNameTooLong_ShouldThrowDomainException()
    {
        // Arrange
        var longName = new string('A', 51);

        // Act
        Action act = () => new Transaction(_validTransactionFileId, longName, "Finance", _validDate, "1234", 10m, "Merchant", "USD", "Desc");

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Employee name cannot exceed 100 characters.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidDepartment_ShouldThrowDomainException(string invalidDept)
    {
        // Act
        Action act = () => new Transaction(_validTransactionFileId, "John Doe", invalidDept, _validDate, "1234", 10m, "Merchant", "USD", "Desc");

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Department is required.");
    }

    [Fact]
    public void Constructor_WithDepartmentTooLong_ShouldThrowDomainException()
    {
        // Arrange
        var longDept = new string('A', 51); // The code checks > 50, even though exception message says 100

        // Act
        Action act = () => new Transaction(_validTransactionFileId, "John Doe", longDept, _validDate, "1234", 10m, "Merchant", "USD", "Desc");

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Department cannot exceed 100 characters.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidCardNumber_ShouldThrowDomainException(string invalidCard)
    {
        // Act
        Action act = () => new Transaction(_validTransactionFileId, "John Doe", "Finance", _validDate, invalidCard, 10m, "Merchant", "USD", "Desc");

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Card number is required.");
    }

    [Fact]
    public void Constructor_WithCardNumberTooLong_ShouldThrowDomainException()
    {
        // Arrange
        var longCard = new string('1', 31);

        // Act
        Action act = () => new Transaction(_validTransactionFileId, "John Doe", "Finance", _validDate, longCard, 10m, "Merchant", "USD", "Desc");

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Card number cannot exceed 30 characters.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10.50)]
    public void Constructor_WithInvalidAmount_ShouldThrowDomainException(decimal invalidAmount)
    {
        // Act
        Action act = () => new Transaction(_validTransactionFileId, "John Doe", "Finance", _validDate, "1234", invalidAmount, "Merchant", "USD", "Desc");

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Amount must be greater than zero.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidMerchant_ShouldThrowDomainException(string invalidMerchant)
    {
        // Act
        Action act = () => new Transaction(_validTransactionFileId, "John Doe", "Finance", _validDate, "1234", 10m, invalidMerchant, "USD", "Desc");

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Merchant is required.");
    }

    [Fact]
    public void Constructor_WithMerchantTooLong_ShouldThrowDomainException()
    {
        // Arrange
        var longMerchant = new string('M', 101);

        // Act
        Action act = () => new Transaction(_validTransactionFileId, "John Doe", "Finance", _validDate, "1234", 10m, longMerchant, "USD", "Desc");

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Merchant cannot exceed 100 characters.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidCurrency_ShouldThrowDomainException(string invalidCurrency)
    {
        // Act
        Action act = () => new Transaction(_validTransactionFileId, "John Doe", "Finance", _validDate, "1234", 10m, "Merchant", invalidCurrency, "Desc");

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Currency is required.");
    }

    [Fact]
    public void Constructor_WithCurrencyTooLong_ShouldThrowDomainException()
    {
        // Arrange
        var longCurrency = new string('U', 11);

        // Act
        Action act = () => new Transaction(_validTransactionFileId, "John Doe", "Finance", _validDate, "1234", 10m, "Merchant", longCurrency, "Desc");

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Currency cannot exceed 10 characters.");
    }

    [Fact]
    public void Constructor_WithDescriptionTooLong_ShouldThrowDomainException()
    {
        // Arrange
        var longDescription = new string('D', 501);

        // Act
        Action act = () => new Transaction(_validTransactionFileId, "John Doe", "Finance", _validDate, "1234", 10m, "Merchant", "USD", longDescription);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Description cannot exceed 500 characters.");
    }
}
