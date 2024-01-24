namespace Adoptly.Web.Utilities;

public class Utils
{
    private static readonly Random _random = new();

    // Converts a string value to an enum. 

    public static TEnum ConvertToEnum<TEnum>(string value) where TEnum : struct
    {
        if (Enum.TryParse(value, true, out TEnum result))
            return result;
        else
            return default;
    }

    // Returns a random enum value from all possible values.

    public static T RandomEnum<T>()
    {
        if (!typeof(T).IsEnum)
            return default;
        Array values = Enum.GetValues(typeof(T));
        T randomEnumValue = (T)values.GetValue(_random.Next(values.Length));
        return randomEnumValue;
    }

    // Returns a random object from a list of objects. 

    public static T RandomListItem<T>(List<T> list)
    {
        if (list.Count == 0)
            return default;
        return list[_random.Next(0, list.Count)];
    }

    // Returns a random bool value.

    public static bool RandomBool() => _random.Next(0, 2) == 1;

    // Returns a random pet age. A pet cannot be older than 15.

    public static double RandomAge() => (double)Math.Round(_random.Next(0, 150) / 10.0, 1);
}