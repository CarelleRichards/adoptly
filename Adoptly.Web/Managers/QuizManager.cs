using Adoptly.Web.Data;
using Adoptly.Web.Models;

namespace Adoptly.Web.Managers;

// Manages the Quizes table in the database.

public class QuizManager
{
    private readonly DataContext _context;

    public QuizManager(DataContext context) => _context = context;

    // Get all quizzes ordered by adopter's username.
    
    public List<Quiz> GetAll() => _context.Quiz.OrderBy(x => x.AdopterUsername).ToList();

    // Get a quiz by adopter username.
    
    public Quiz Get(string adopterUsername) => _context.Quiz.Find(adopterUsername);

    // Update a quiz.
    
    public void Update(Quiz quiz)
    {
        _context.Quiz.Update(quiz);
        _context.SaveChanges();
    }

    // Add a quiz to the database.
    
    public void Add(Quiz quiz)
    {
        _context.Quiz.Add(quiz);
        _context.SaveChanges();
    }

    // Delete a quiz from the database.
    
    public void Delete(Quiz quiz)
    {
        if (quiz is not null)
        {
            _context.Quiz.Remove(quiz);
            _context.SaveChanges();
        }
    }
}