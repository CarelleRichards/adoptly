using Adoptly.Web.Data;
using Adoptly.Web.Models;

namespace Adoptly.Web.Managers;

// Manages the Addresses table in the database. 

public class AddressManager
{
    private readonly DataContext _context;

    public AddressManager(DataContext context) => _context = context;

    // Get an address by address id.
    
    public Address GetById(int id) => _context.Addresses.FirstOrDefault(x => x.Id == id);
    
    // Add an address to the database.
    
    public void Add(Address address)
    {
        _context.Addresses.Add(address);
        _context.SaveChanges();
    }

    // Update an address.
    
    public void Update(Address address)
    {
        _context.Addresses.Update(address);
        _context.SaveChanges();
    }

    // Delete an address from the database.
    
    public void Delete(Address address)
    {
        _context.Addresses.Remove(address);
        _context.SaveChanges();
    }
}