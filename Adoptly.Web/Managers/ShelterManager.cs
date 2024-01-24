using Adoptly.Web.Data;
using Adoptly.Web.Models;

namespace Adoptly.Web.Managers;

// Manages the Shelters table in the database.

public class ShelterManager
{
    private readonly DataContext _context;

    public ShelterManager(DataContext context) => _context = context;

    // Get all shelters ordered by username.
    
    public List<Shelter> GetAll() => _context.Shelters.OrderBy(x => x.Username).ToList();

    // Get a shelter by username.
    
    public Shelter GetByUsername(string username) => _context.Shelters.FirstOrDefault(x => x.Username == username);

    // Get a shelter by email address.
    
    public Shelter GetByEmail(string email) => _context.Shelters.Find(email);

    // Add a shelter to the database.
    
    public void Add(Shelter shelter)
    {
        _context.Shelters.Add(shelter);
        _context.SaveChanges();
    }

    // Update a shelter.
    
    public void Update(Shelter shelter)
    {
        _context.Shelters.Update(shelter);
        _context.SaveChanges();
    }

    // Delete a shelter from the database.
    
    public void Delete(Shelter shelter)
    {
        _context.Shelters.Remove(shelter);
        _context.SaveChanges();
    }
}