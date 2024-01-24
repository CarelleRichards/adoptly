using Adoptly.Web.Data;
using Adoptly.Web.Models;

namespace Adoptly.Web.Managers;

// Manages the Matches table in the database.

public class MatchManager
{
    private readonly DataContext _context;

    public MatchManager(DataContext context) => _context = context;

    // Get all matches ordered by adopter username.
    
    public List<Match> GetAll() => _context.Matches.OrderBy(x => x.AdopterUsername).ToList();

    // Get all matches by shelter username.
    
    public List<Match> GetAllByShelterUsername(string shelterUsername) =>
        _context.Matches.Where(x => x.Pet.Shelter.Username == shelterUsername).ToList();

    // Get a match by adopter email and pet id.
    
    public Match Get(string adopterEmail, int petId) => _context.Matches.Find(adopterEmail, petId);

    // Get a list of matches by adopter username.
    
    public List<Match> GetByAdopterUsername(string adopterUsername) =>
        _context.Matches.Where(x => x.AdopterUsername == adopterUsername).ToList();
    
    // Get a list of matches by pet id.
    
    public List<Match> GetByPetId(int petId) =>
        _context.Matches.Where(x => x.PetId == petId).ToList();
    
    // Delete all matches by adopter username.
    
    public void DeleteAllByAdopterUsername(string adopterUsername)
    {
        List<Match> matches = GetByAdopterUsername(adopterUsername).ToList();

        foreach (var match in matches)
            _context.Matches.Remove(match);

        _context.SaveChanges();
    }
    
    // Delete all matches by shelter username.
    
    public void DeleteAllByShelterUsername(string shelterUsername)
    {
        List<Match> matches = GetAllByShelterUsername(shelterUsername).ToList();

        foreach (var match in matches)
            _context.Matches.Remove(match);

        _context.SaveChanges();
    }
    
    // Delete all matches by pet id.
    
    public void DeleteAllByPetId(int petId)
    {
        List<Match> matches = GetByPetId(petId).ToList();

        foreach (var match in matches)
            _context.Matches.Remove(match);

        _context.SaveChanges();
    }

    // Update a match.
    
    public void Update(Match match)
    {
        _context.Matches.Update(match);
        _context.SaveChanges();
    }

    // Add a match to the database.
    
    public void Add(Match match)
    {
        _context.Matches.Add(match);
        _context.SaveChanges();
    }

    // Delete a match from the database.
    
    public void Delete(Match match)
    {
        _context.Matches.Remove(match);
        _context.SaveChanges();
    }
}