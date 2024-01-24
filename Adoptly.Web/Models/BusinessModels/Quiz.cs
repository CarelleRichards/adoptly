using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adoptly.Web.Models;

public class Quiz
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [ForeignKey("Adopter")]
    public string AdopterUsername { get; set; }
    public virtual Adopter Adopter { get; set; }

    [NotMapped]
    private static readonly Dictionary<int, Question> _questions = new()
    {
        { 1, new Question()
        {
            Prompt = "What type of animal are you looking for?",
            Options = new Dictionary<int, string>
            {
                { 1, "Dog." },
                { 2, "Cat." },
                { 3, "No preference." }
            }
        }
        },
        { 2, new Question()
        {
            Prompt = "How much time will you have to spend with your pet?",
            Options = new Dictionary<int, string>
            {
                { 1, "I have a busy schedule, so I am not at home very often." },
                { 2, "I have a few hours of home time daily." },
                { 3, "I enjoy a substantial amount of home time every day." },
            }
        }
        },
        { 3, new Question()
        {
            Prompt = "Do you or does anyone in your house have any allergies?",
            Options = new Dictionary<int, string>
            {
                { 1, "Cats." },
                { 2, "Dogs." },
                { 3, "Cats and dogs." },
                { 4, "None." },
            }
        }
        },
        { 4, new Question()
        {
            Prompt = "How often do you exercise?",
            Options = new Dictionary<int, string>
            {
                { 1, "A run or walk everyday is essential." },
                { 2, "I like to take a walk occasionally." },
                { 3, "Exercise is just not my thing." },
            }
        }
        },
        { 5, new Question()
        {
            Prompt = "Which of the following best describes your pet budget?",
            Options = new Dictionary<int, string>
            {
                { 1, "I have a limited budget to cover my pet’s basic needs." },
                { 2, "I have a medium sized budget, so I don’t mind if my pet’s needs cost a little extra." },
                { 3, "I’m willing to spend any amount of money to look after my new friend." },
            }
        }
        }
    };

    [NotMapped]
    public ReadOnlyDictionary<int, Question> Questions => _questions.AsReadOnly();

    [Required(ErrorMessage = "You must answer this question.")]
    [Range(1, 3, ErrorMessage = "You must select a valid option.")]
    public int Answer1 { get; set; }

    [Required(ErrorMessage = "You must answer this question.")]
    [Range(1, 3, ErrorMessage = "You must select a valid option.")]
    public int Answer2 { get; set; }

    [Required(ErrorMessage = "You must answer this question.")]
    [Range(1, 4, ErrorMessage = "You must select a valid option.")]
    public int Answer3 { get; set; }

    [Required(ErrorMessage = "You must answer this question.")]
    [Range(1, 3, ErrorMessage = "You must select a valid option.")]
    public int Answer4 { get; set; }

    [Required(ErrorMessage = "You must answer this question.")]
    [Range(1, 3, ErrorMessage = "You must select a valid option.")]
    public int Answer5 { get; set; }
}