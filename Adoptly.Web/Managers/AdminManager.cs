using Adoptly.Web.Data;
using Adoptly.Web.Models;

namespace Adoptly.Web.Managers;

// Manages the Admins table in the database.

public class AdminManager
{
    private readonly DataContext _context;

    public AdminManager(DataContext context) => _context = context;

    // Get all admins ordered by username.
    
    public List<Admin> GetAll() => _context.Admins.OrderBy(x => x.Username).ToList();

    // Get an admin by username.
    
    public Admin GetByUsername(string username) => _context.Admins.FirstOrDefault(x => x.Username == username);

    // Get an admin by email.
    
    public Admin GetByEmail(string email) => _context.Admins.Find(email);

    // Add an admin to the database.
    
    public void Add(Admin admin)
    {
        _context.Admins.Add(admin);
        _context.SaveChanges();
    }

    // Update an admin.
    
    public void Update(Admin admin)
    {
        _context.Admins.Update(admin);
        _context.SaveChanges();
    }
}