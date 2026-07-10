namespace GearFlow.Modules.Users.Core.Entities;

public class Customer
{
    public Guid Id { get; }

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public DateTime CreatedAt { get; }

    public UserAccount? UserAccount { get; set; }


    private Customer() { } // EF

    public Customer(string firstName, string lastName, string email, string phoneNumber, DateTime utcNow)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        CreatedAt = utcNow;
    }
}
