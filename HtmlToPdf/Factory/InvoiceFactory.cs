using Bogus;
using HtmlToPdf.Models;

namespace HtmlToPdf.Factory;

internal sealed class InvoiceFactory
{
    public Invoice Create()
    {
        var faker = new Faker();

        return new Invoice
        {
            Number = faker.Random.Number(100_000, 1_000_000).ToString(),
            IssuedDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime),
            DueDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddDays(10).DateTime),
            SellerAddress = new Address(faker.Company.CompanyName(), faker.Address.StreetAddress(), faker.Address.City(), faker.Address.State(), faker.Internet.Email()),
            CustomerAddress = new Address(faker.Company.CompanyName(), faker.Address.StreetAddress(), faker.Address.City(), faker.Address.State(), faker.Internet.Email()),
            Items = Enumerable.Range(1, 100).Select(i => new Item(i, faker.Commerce.ProductName(), faker.Random.Decimal(10, 1000), faker.Random.Int(1,10))).ToArray()
        };
    }
}