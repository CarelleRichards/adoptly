using Adoptly.Web.Data;
using Adoptly.Web.Models;

namespace Adoptly.Web.Managers;

// Manages the Applications table in the database.

public class ApplicationManager
{
    private readonly DataContext _context;

    public ApplicationManager(DataContext context) => _context = context;

    // Get all applications by shelter username.
    
    public List<Application> GetAll(string username) =>
        _context.Applications.Where(x => x.Pet.Shelter.Username == username).ToList();

    // Get all non-archived applications by shelter username.
    
    public List<Application> GetAllNoneArchived(string username) =>
        _context.Applications.Where(x => x.Pet.Shelter.Username == username && x.Archived == false).ToList();

    // Get all archived applications by shelter username.
    
    public List<Application> GetAllArchived(string username) =>
        _context.Applications.Where(x => x.Pet.Shelter.Username == username && x.Archived == true).ToList();

    // Get an application by adopter username and pet id.
    
    public Application Get(string username, int petId) => _context.Applications.Find(username, petId);

    // Get all applications by adopter username.
    
    public List<Application> GetByAdopterUsername(string adopterUsername) =>
        _context.Applications.Where(x => x.AdopterUsername == adopterUsername).ToList();
    
    // Get all non-archived applications by adopter username.
    
    public List<Application> GetNonArchivedByAdopterUsername(string adopterUsername) =>
        _context.Applications.Where(x => x.AdopterUsername == adopterUsername && x.Archived == false).ToList();
    
    // Get all archived applications by adopter username.
    
    public List<Application> GetArchivedByAdopterUsername(string adopterUsername) =>
        _context.Applications.Where(x => x.AdopterUsername == adopterUsername && x.Archived == true).ToList();

    // Get all non-archived applications by pet id.
    
    public List<Application> GetByPetId(int petId) =>
        _context.Applications.Where(x => x.PetId == petId && x.Archived == false).ToList();
    
    // Get all archived applications by pet id.
    
    public List<Application> GetAllByPetId(int petId) =>
        _context.Applications.Where(x => x.PetId == petId && x.Archived == true).ToList();
    
    // Delete all applications by adopter username.
    
    public void DeleteAllByAdopterUsername(string adopterUsername)
    {
        List<Application> applications = GetByAdopterUsername(adopterUsername).ToList();

        foreach (var application in applications)
            _context.Applications.Remove(application);

        _context.SaveChanges();
    }
    
    // Delete all applications by shelter username.
    
    public void DeleteAll(string shelterUsername)
    {
        List<Application> applications = GetAll(shelterUsername).ToList();

        foreach (var application in applications)
            _context.Applications.Remove(application);

        _context.SaveChanges();
    }
    
    // Get all applications by pet id.
    
    public List<Application> GetAllNonArchivedByPetId(int petId) =>
        _context.Applications.Where(x => x.PetId == petId).ToList();
    
    // Delete all applications by pet id.
    
    public void DeleteAllByPetId(int petId)
    {
        List<Application> applications = GetAllNonArchivedByPetId(petId).ToList();

        foreach (var application in applications)
            _context.Applications.Remove(application);

        _context.SaveChanges();
    }
    
    // Update an application.
    
    public void Update(Application application)
    {
        _context.Applications.Update(application);
        _context.SaveChanges();
    }

    // Add an application to the database.
    
    public void Add(Application application)
    {
        _context.Applications.Add(application);
        _context.SaveChanges();
    }

    // Delete an application from the database.
    
    public void Delete(Application application)
    {
        _context.Applications.Remove(application);
        _context.SaveChanges();
    }
}