namespace BOL;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
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
}
