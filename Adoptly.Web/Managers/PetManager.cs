using Adoptly.Web.Data;
using Adoptly.Web.Models;

namespace Adoptly.Web.Managers;

// Manages the Pets table in the database.

public class PetManager
{
    private readonly DataContext _context;

    public PetManager(DataContext context) => _context = context;

    // Get all pets ordered by pet name.
    
    public List<Pet> GetAll() => _context.Pets.OrderBy(x => x.Name).ToList();

    // Get a pet by id.
    
    public Pet GetById(int id) => _context.Pets.Find(id);

    // Get a list of pets by shelter username.
    
    public List<Pet> GetByShelterUsername(string shelterUsername) =>
        _context.Pets.Where(x => x.Shelter.Username == shelterUsername).ToList();

    // Get a list of pets by application count.
    
    public List<Pet> GetByApplicationCount(string id) => _context.Pets
        .Where(x => x.Applications.Count > 0 && x.Shelter.Username == id)
        .Where(y => y.Applications.Any(z => !z.Archived))
        .ToList();

    // Delete all pets by shelter username.
    
    public void DeleteAllByShelterUsername(string shelterUsername)
    {
        List<Pet> pets = GetByShelterUsername(shelterUsername).ToList();

        foreach (var pet in pets)
            _context.Pets.Remove(pet);

        _context.SaveChanges();
    }
    
    // Add a pet to the database.
    
    public void Add(Pet pet)
    {
        _context.Pets.Add(pet);
        _context.SaveChanges();
    }
    
    // Update a pet.

    public void Update(Pet pet)
    {
        _context.Pets.Update(pet);
        _context.SaveChanges();
    }

    // Delete a pet from the database.
    
    public void Delete(Pet pet)
    {
        _context.Pets.Remove(pet);
        _context.SaveChanges();
    }
}