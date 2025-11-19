namespace BOL;
using System.ComponentModel.DataAnnotations;
    
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    [EmailAddress]
    public string Email { get; set; }
    public string Role { get; set; }
    public string Password { get; set; }
    public string PasswordSalt { get; set; }
    public bool IsActive { get; set; }
    public bool HasRole(string role)
    {
        if (string.IsNullOrEmpty(Role))
            return false;

        return Role.Split(',').Contains(role.ToString(), StringComparer.OrdinalIgnoreCase);
    }

    // for Customer 
    public string MeterNumber { get; set; }
    public string ContactNo { get; set; }

}
