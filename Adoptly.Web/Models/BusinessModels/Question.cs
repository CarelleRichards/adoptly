namespace Adoptly.Web.Models;

public class Question
{
    public string Prompt { get; init; }
	public Dictionary<int, string> Options { get; init; }
}