namespace BankApi.Client.Models;

public class Bank
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Country { get; set; } = "";
}