using Adoptly.Web.Data;
using Adoptly.Web.Models;

namespace Adoptly.Web.Managers;

// Manages the Adopters table in the database.

public class AdopterManager
{
    private readonly DataContext _context;

    public AdopterManager(DataContext context) => _context = context;

    // Get all adopters ordered by username.
    
    public List<Adopter> GetAll() => _context.Adopters.OrderBy(x => x.Username).ToList();

    // Get an adopter by username.
    
    public Adopter GetByUsername(string username) => _context.Adopters.FirstOrDefault(x => x.Username == username);

    // Get an adopter by email.
    
    public Adopter GetByEmail(string email) => _context.Adopters.Find(email);

    // Add an adopter to the database.
    
    public void Add(Adopter adopter)
    {
        _context.Adopters.Add(adopter);
        _context.SaveChanges();
    }

    // Update an adopter.
    
    public void Update(Adopter adopter)
    {
        _context.Adopters.Update(adopter);
        _context.SaveChanges();
    }

    // Delete an adopter from the database.
    
    public void Delete(Adopter adopter)
    {
        _context.Adopters.Remove(adopter);
        _context.SaveChanges();
    }
}