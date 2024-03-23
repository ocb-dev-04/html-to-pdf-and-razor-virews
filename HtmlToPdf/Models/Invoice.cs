namespace HtmlToPdf.Models;

internal sealed record Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Number { get; set; }
    public DateOnly IssuedDate { get; set; }
    public DateOnly DueDate { get; set; }
    public Address SellerAddress { get; set; }
    public Address CustomerAddress { get; set; }
    public Item[] Items { get; set; }
}

internal sealed record Address(
    string CompanyName,
    string Street,
    string City,
    string State,
    string Email);

internal sealed record Item(
    int Id,
    string Name,
    decimal Price,
    int Quantity);