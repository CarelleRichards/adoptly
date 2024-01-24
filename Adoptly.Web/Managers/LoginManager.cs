using Adoptly.Web.Data;
using Adoptly.Web.Models;

namespace Adoptly.Web.Managers;

// Manages the Logins table in the database.

public class LoginManager
{
    private readonly DataContext _context;
    private readonly AdopterManager _adopterManager;
    private readonly ShelterManager _shelterManager;
    private readonly AdminManager _adminManager;

    public LoginManager(DataContext context, AdopterManager adopterManager,
        AdminManager adminManager, ShelterManager shelterManager)
    {
        _context = context;
        _adopterManager = adopterManager;
        _shelterManager = shelterManager;
        _adminManager = adminManager;
    }

    // Get all logins ordered by email.
    
    public List<Login> GetAll() => _context.Logins.OrderBy(x => x.Email).ToList();

    // Get a login by user id.
    
    public Login GetById(int id) => _context.Logins.Find(id);

    // Get a login by user email.
    
    public Login GetByEmail(string email) => _context.Logins.FirstOrDefault(x => x.Email == email);

    // Add a login to the database.
    
    public void Add(Login login)
    {
        _context.Logins.Add(login);
        _context.SaveChanges();
    }

    // Update a login.
    
    public void Update(Login login)
    {
        _context.Logins.Update(login);
        _context.SaveChanges();
    }

    // Get a login by username.
    
    public Login GetByUsername(string username)
    {
        Adopter adopter = _adopterManager.GetByUsername(username);

        if (adopter is not null)
            return adopter.Login;

        Shelter shelter = _shelterManager.GetByUsername(username);

        if (shelter is not null)
            return shelter.Login;

        Admin admin = _adminManager.GetByUsername(username);

        if (admin is not null)
            return admin.Login;

        return null;
    }

    // Delete a login from the database.
    
    public void Delete(Login login)
    {
        if (login.User is Shelter)
            _shelterManager.Delete(_shelterManager.GetByUsername(login.User.Username));

        else if (login.User is Adopter)
            _adopterManager.Delete(_adopterManager.GetByUsername(login.User.Username));

        _context.Logins.Remove(login);
        _context.SaveChanges();
    }
}