using Adoptly.Web.Managers;
using Adoptly.Web.Models;
using Adoptly.Web.Models.Enums;
using Adoptly.Web.Utilities;

namespace Adoptly.Web.Services;

public class MatchmakerService
{
    private readonly MatchManager _matchManager;
    private readonly PetManager _petManager;
    private readonly QuizManager _quizManager;

    private Dictionary<int, double> _petScores; // Key = PetId, value = score.
    private static readonly double _scoreThreshold = 2.5; // Score must be above 50%.

    public static readonly double _totalQuestions = 5;
    public double TotalQuestions { get => _totalQuestions; } 

    public MatchmakerService(PetManager petManager, MatchManager matchManager, QuizManager quizManager)
    {
        _petManager = petManager;
        _matchManager = matchManager;
        _quizManager = quizManager;
    }

    public void UpdateQuiz(Adopter adopter, Quiz newQuiz)
    {
        Quiz quiz = _quizManager.Get(adopter.Username);

        // Delete old matches and quiz answers.

        _matchManager.DeleteAllByAdopterUsername(adopter.Username);
        _quizManager.Delete(quiz);

        // Add new quiz answers.

        newQuiz.AdopterUsername = adopter.Username;
        _quizManager.Add(newQuiz);
    }

    public List<Match> GetMatches(Adopter adopter)
    {
        if (adopter is null || adopter.Quiz is null || !ValidationMethods.Validate(adopter.Quiz, out _))
            return new List<Match>();

        // Remove any matches that are no longer up for adoption.

        foreach (var oldMatch in adopter.Matches)
            if (oldMatch.Pet.Status != Status.Available)
                _matchManager.Delete(oldMatch);

        List<Match> newMatches = GetMatchesFromQuiz(adopter.Quiz);

        // Add all new matches if adopter has no previous matches.

        if (adopter.Matches.Count == 0 && newMatches.Count > 0)
            foreach (var match in newMatches)
                _matchManager.Add(match);

        // Add new matches and update old match details if match score has changed.

        else if (newMatches.Count > 0)
            foreach (var newMatch in newMatches)
            {
                Match oldMatch = adopter.Matches.FirstOrDefault(x => x.PetId == newMatch.PetId);

                if (oldMatch is null)
                    _matchManager.Add(newMatch);
                else if (oldMatch is not null && !oldMatch.IsEqual(newMatch))
                {
                    oldMatch.Score = newMatch.Score;
                    oldMatch.DateMatched = DateTime.UtcNow;
                    _matchManager.Update(oldMatch);
                }
            }

        // Order by highest to lowest matchmaking score.

        return adopter.Matches.OrderByDescending(x => x.Score).ThenByDescending(x => x.PetId).ToList();
    }

    private List<Match> GetMatchesFromQuiz(Quiz quiz)
	{
        List<Pet> pets = _petManager.GetAll();
        _petScores = new();

        // Evaluate and filter pets according to quiz answers.

        foreach (var pet in pets)
        {
            if (pet.Status != Status.Available || FilterQuestion1(quiz.Answer1, pet))
                continue;

            EvaluateQuestion2(quiz.Answer2, pet);
            EvaluateQuestion3(quiz.Answer3, pet);
            EvaluateQuestion4(quiz.Answer4, pet);
            EvaluateQuestion5(quiz.Answer5, pet);
        }
      
        List<Match> matches = new();

        // Remove pets that scored below the threshold and create list with remaining pets.

        foreach (var petScore in _petScores)
        {
            if (petScore.Value < _scoreThreshold)
                _petScores.Remove(petScore.Key);
            else            
                matches.Add(new Match()
                {
                    AdopterUsername = quiz.Adopter.Username,
                    PetId = pets.FirstOrDefault(x => x.Id == petScore.Key).Id,
                    Score = petScore.Value
                });
        }
        return matches;
	}

    // What type of animal are you looking for?

    private static bool FilterQuestion1(int quizAnswer, Pet pet)
    {
        // Dog.

        if (quizAnswer == 1 && pet.AnimalType == AnimalType.Dog)
            return false;

        // Cat.

        else if (quizAnswer == 2 && pet.AnimalType == AnimalType.Cat)
            return false;

        // No preference.

        else if (quizAnswer == 3)
            return false;

        return true;
    }

    // How much time will you have to spend with your pet?

    private void EvaluateQuestion2(int quizAnswer, Pet pet)
    {
        // I have a busy schedule, so I am not at home very often.
        
        if (quizAnswer == 1 && pet.Independence == ValueScale.High)
            AddScore(pet.Id);

        // I have a few hours of home time daily.

        else if (quizAnswer == 2 && pet.Independence != ValueScale.Low)
            AddScore(pet.Id);

        // I enjoy a substantial amount of home time every day.

        else if (quizAnswer == 3)
            AddScore(pet.Id);
    }

    // Do you or does anyone in your house have any allergies?

    private void EvaluateQuestion3(int quizAnswer, Pet pet)
    {
        // Cats.

        if (quizAnswer == 1 && pet.AnimalType == AnimalType.Dog ||
            (pet.AnimalType == AnimalType.Cat && pet.AllergyFriendly))
            AddScore(pet.Id);

        // Dogs.

        else if (quizAnswer == 2 && pet.AnimalType == AnimalType.Cat ||
            (pet.AnimalType == AnimalType.Dog && pet.AllergyFriendly))
            AddScore(pet.Id);

        // Cats and dogs.

        else if (quizAnswer == 3 && pet.AllergyFriendly)
            AddScore(pet.Id);

        // None.

        else if (quizAnswer == 4)
            AddScore(pet.Id);
    }

    // How often do you exercise?

    private void EvaluateQuestion4(int quizAnswer, Pet pet)
    {
        // A run or walk everyday is essential.

        if (quizAnswer == 1)
            AddScore(pet.Id);

        // I like to take a walk occasionally.

        else if (quizAnswer == 2 && pet.AnimalType == AnimalType.Cat ||
            (pet.AnimalType == AnimalType.Dog && pet.ActivityLevel != ValueScale.High))
            AddScore(pet.Id);

        // Exercise is just not my thing.

        else if (quizAnswer == 3 && pet.AnimalType == AnimalType.Cat ||
            (pet.AnimalType == AnimalType.Dog && pet.ActivityLevel == ValueScale.Low))
            AddScore(pet.Id);
    }

    // Which of the following best describes your pet budget?

    private void EvaluateQuestion5(int quizAnswer, Pet pet)
    {
        // I have a limited budget to cover my pet’s basic needs.

        if (quizAnswer == 1 && pet.Budget == ValueScale.Low)
            AddScore(pet.Id);

        // I have a medium sized budget, so I don’t mind if my pet’s needs cost a little extra.

        else if(quizAnswer == 2 && pet.Budget != ValueScale.High)
            AddScore(pet.Id);

        // I’m willing to spend any amount of money to look after my new friend.

        else if (quizAnswer == 3)
            AddScore(pet.Id);
    }

    // Add pet to score tracker if they aren't already in it and increment score. 

    private void AddScore(int petId)
    {
        if (_petScores.ContainsKey(petId))
            _petScores[petId] = _petScores[petId] + 1;
        else
            _petScores.Add(petId, 1);
    }
}