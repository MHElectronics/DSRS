namespace BOL;
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string MeterId { get; set; }
    public string Email { get; set; }
    public string ContactNo { get; set; }
    public string Password { get; set; }
    public string PasswordSalt { get; set; }
    public bool IsActive { get; set; }
    public bool IsApproved { get; set; }
}
