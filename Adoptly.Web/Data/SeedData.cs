using System.Text;
using Adoptly.Web.Managers;
using Adoptly.Web.Models;
using Adoptly.Web.Models.Enums;
using SimpleHashing.Net;
using Adoptly.Web.Utilities;

namespace Adoptly.Web.Data;

// A class for populating the database with sample and/or test data.

// IMPORTANT: 
// 1. Before using this class, you must uncomment the declaration and assignment of _pass.
// 2. You need to set _pass to your desired password.
// 3. You must uncomment PasswordHash in MakeLogin().

public static class SeedData
{
    private static readonly Random _random = new();
    private static readonly List<Shelter> _shelters = new();
    private static readonly int _petsPerShelter = 20;
    private static readonly ISimpleHash _simpleHash = new SimpleHash();
    private static readonly string _path = "wwwroot/assets/img/seed";
    private static readonly string _petContainer = "pet-profile-pictures";
    private static readonly string _shelterContainer = "shelter-profile-pictures";
    private static readonly string _adopterContainer = "adopter-profile-pictures";
    private static readonly string _adminContainer = "admin-profile-pictures";

    // Add a password here and uncomment before running.

    // private readonly static string _pass = "";

    private readonly static List<string> _names = new()
    {
        "Alex", "Charlie", "Casey", "Taylor", "Reese", "Riley", "River", "Muffin", "Noodle", "Jet",
        "Jellybean", "Tofu", "Pickles", "Sam", "Cookie", "Echo", "Cricket", "Pixel", "Peanut", "Fuzzball",
        "Blitz", "Waffles", "Tater Tot", "Mischief", "Rocket", "Thunderbolt", "Rhythm", "Spark", "Rascal",
        "Skittles", "Squeak", "Squish", "Socks", "Zing", "Wiggle", "Zippy", "Squeaky", "Munchkin"
    };

    private readonly static List<string> _catDescriptions = new()
    {
        "Meet this purr-fect feline friend, a bundle of joy who'll fill your home with love and laughter.",
        "A graceful beauty with enchanting eyes, a heart full of love and a soft purr that soothes the soul. This cat will be your loving companion.",
        "Pure joy and love bundled in one sweet feline, ready to bring happiness to your life. Find endless laughter, cuddles and affection in this charming cat.",
        "Playful and sprightly, this kitty is your ticket to endless amusement and delightful surprises.",
        "Playful, spirited and full of charm, this cat will keep you entertained with its quirky antics.",
        "Seeking a loyal feline friend? This cat's devotion is unwavering, providing constant companionship.",
        "A bundle of fluff and love, this cat is a cuddle connoisseur, always ready for warm snuggles and purrs.",
        "Get ready for daily doses of affection with this cuddle bug, ready to shower you with love.",
        "This mischievous little kitty will keep you on your toes with its playful pranks and antics.",
        "This cuddle guru knows how to make your days cozy, with purrs and affection in abundance."
    };

    private readonly static List<string> _dogDescriptions = new()
    {
        "If you crave snuggles and affection, you've found your match. This dog's love is as warm as a cosy blanket on a winter's night",
        "Meet this cheerful canine companion! Always ready to brighten your day with boundless enthusiasm and a heart full of love.",
        "A furry friend who's all about spreading joy! This dog's infectious happiness is sure to brighten your day.",
        "The perfect cuddle buddy. Sweet, gentle and loving, this dog's affectionate nature will melt your heart.",
        "Meet this delightful little kitty, a constant source of laughter and happiness, ready to brighten your days.",
        "Looking for an unbreakable bond? This dog's loyalty is like a warm embrace, a friend who's always got your back.",
        "Sweet and gentle, this dog's affectionate nature will envelop you in warmth and love, filling your heart.",
        "This dog is a bundle of joy, always ready to brighten your day with a wagging tail and a cheerful heart.",
        "Unwavering loyalty is this dog's middle name, offering steadfast friendship through all of life's moments.",
        "Meet the bounciest ball of happiness, always ready to spread laughter and positivity."
    };

    // This function will populate the database with many users and pets.

    public static void Initialise(IServiceProvider serviceProvider)
    {
        DataContext context = serviceProvider.GetRequiredService<DataContext>();
        BlobManager blobManager = serviceProvider.GetRequiredService<BlobManager>();
        Initialise(context, blobManager);
    }

    // This function will populate the database with an admin user. 

    public static void AddAdminOnly(IServiceProvider serviceProvider)
    {
        DataContext context = serviceProvider.GetRequiredService<DataContext>();

        Login login = MakeLogin("admin@adoptly.com");

        context.Logins.Add(login);
        context.SaveChanges();

        // Enter desired admin username, first name and last name here.

        Admin admin = new()
        {
            LoginId = login.Id,
            Username = "Adoptly_Admin",
            FirstName = "Michael",
            LastName = "Jordan",
        };

        context.Admins.Add(admin);
        context.SaveChanges();

    }

    public static void Initialise(DataContext context, BlobManager blobManager)
    {
        // Check if data exists and stop if it does.

        if (context.Logins.Any())
            return;

        // Clean up blob storage.

        Task task1 = DeleteProfilePictures(_petContainer, blobManager);
        Task task2 = DeleteProfilePictures(_adopterContainer, blobManager);
        Task task3 = DeleteProfilePictures(_shelterContainer, blobManager);
        Task task4 = DeleteProfilePictures(_adminContainer, blobManager);
        Task.WaitAll(task1, task2, task3, task4);

        // Create and insert data into database.

        Login login1 = MakeLogin("john.smith@gmail.com");
        Login login2 = MakeLogin("contact@friendsforever.com");
        Login login3 = MakeLogin("adopt@secondchances.com");
        Login login4 = MakeLogin("admin@adoptly.com");
        Login login5 = MakeLogin("sally.rose@hotmail.com");
        Login login6 = MakeLogin("rescue@hopefultails.com");

        context.Logins.AddRange(login1, login2, login3, login4, login5, login6);
        context.SaveChanges();

        Adopter adopter1 = new()
        {
            LoginId = login1.Id,
            Username = "JohnSmith88",
            FirstName = "John",
            LastName = "Smith",
            State = Utils.RandomEnum<State>()
        };

        Adopter adopter2 = new()
        {
            LoginId = login5.Id,
            Username = "Sally_Rose",
            FirstName = "Sally",
            LastName = "Rose",
            State = Utils.RandomEnum<State>()
        };

        Admin admin = new()
        {
            LoginId = login4.Id,
            Username = "MichaelJordan_Adoptly",
            FirstName = "Michael",
            LastName = "Jordan",
        };

        Shelter shelter1 = new()
        {
            LoginId = login2.Id,
            Username = "FriendsForever1",
            Name = "Friends Forever",
            State = Utils.RandomEnum<State>()
        };

        Shelter shelter2 = new()
        {
            LoginId = login3.Id,
            Username = "Second_Chances_Pet_Rescue",
            Name = "Second Chances Pet Rescue",
            State = Utils.RandomEnum<State>()
        };

        Shelter shelter3 = new()
        {
            LoginId = login6.Id,
            Username = "HopefulTails",
            Name = "Hopeful Tails Animal Rescue",
            State = Utils.RandomEnum<State>()
        };

        // Images from Asier (n.d.), Gelpi (n.d.) and Myvisuals (n.d.).

        adopter2.ProfilePicture = UploadPicture(adopter2, "Adopter3.jpg", blobManager).Result;
        adopter1.ProfilePicture = UploadPicture(adopter1, "Adopter1.jpg", blobManager).Result;

        // Image from Wayhome Studio (n.d.).

        admin.ProfilePicture = UploadPicture(admin, "Admin1.jpg", blobManager).Result;

        // Images from anastasy_helter (n.d. a), anastasy_helter (n.d. b) and anastasy_helter (n.d. c).

        shelter1.ProfilePicture = UploadPicture(shelter1, "Shelter1.jpg", blobManager).Result;
        shelter2.ProfilePicture = UploadPicture(shelter2, "Shelter2.jpg", blobManager).Result;
        shelter3.ProfilePicture = UploadPicture(shelter2, "Shelter3.jpg", blobManager).Result;

        context.Adopters.AddRange(adopter1, adopter2);
        context.Admins.Add(admin);
        context.Shelters.AddRange(shelter1, shelter2, shelter3);
        context.SaveChanges();

        _shelters.Add(shelter1);
        _shelters.Add(shelter2);
        _shelters.Add(shelter3);

        foreach (Shelter shelter in _shelters)
            for (int i = 0; i <= _petsPerShelter; i++)
            {
                AnimalType animal = Utils.RandomEnum<AnimalType>();

                // Images from alimyakubov (n.d.), Andreaobzerova(n.d.), aylabaha(n.d.a - b), 
                // DaraGor(n.d.), dionoanomalia(n.d.), Firn(n.d.), FurryFritz(n.d.a - b), Georgina(n.d.), 
                // imageBROKER(n.d.), Irina(n.d.), Iulia(n.d.), master1305(n.d.), MeganBetteridge(n.d.a - b), 
                // New Africa(n.d.a-b), OlgaOvcharenko(n.d.), Pixel - Shot(n.d.a - k), Richard(n.d.), 
                // Richli(n.d.), Sandra(n.d.a - g), vaneeva(n.d.) and Volchanskiy(n.d.)

                List<string> fileNames = Directory.GetFiles(GetPath(animal)).Select(Path.GetFileName).ToList();
                string profilePicture = Utils.RandomListItem(fileNames);

                Pet pet = MakeRandomPet(shelter, profilePicture, animal);
                context.Pets.Add(pet);
                context.SaveChanges();

                pet.ProfilePicture = UploadPicture(pet, profilePicture, blobManager).Result;
                pet.StatusLastUpdated = DateTime.UtcNow.AddDays(_random.Next(-91, -1));
                context.Pets.Update(pet);
                context.SaveChanges();
            }
    }

    // Creates a Login object using a given email.
    // You must uncomment the PasswordHash for seeding.

    private static Login MakeLogin(string email)
    {
        return new Login()
        {
            Email = email,
            Verified = true,
            DateCreated = DateTime.UtcNow.AddDays(_random.Next(-280, -271)),
            DateVerified = DateTime.UtcNow.AddDays(_random.Next(-270, -264)),
            // PasswordHash = _simpleHash.Compute(_pass)
        };
    }

    // Creates a random pet for the given shelter.
    // You must specify the AnimalType and provide the file name for the profile picture.
    // The profile picture must be named like "Breed_Colour.jpg".
    // Use camel case for breed and colour names.
    // Colour must valid, see the Colour Enum for more information. 

    private static Pet MakeRandomPet(Shelter shelter, string profilePicture, AnimalType animal)
    {
        return new Pet()
        {
            ShelterUsername = shelter.Username,
            Name = Utils.RandomListItem(_names),
            AnimalType = animal,
            Description = Utils.RandomListItem(animal == AnimalType.Dog ? _dogDescriptions : _catDescriptions),
            State = shelter.State,
            Sex = Utils.RandomEnum<Sex>(),
            Status = Utils.RandomEnum<Status>(),
            Age = Utils.RandomAge(),
            Breed = ExtractBreed(profilePicture),
            Colour = Utils.ConvertToEnum<Colour>(ExtractColour(profilePicture)),
            Desexed = Utils.RandomBool(),
            AllergyFriendly = Utils.RandomBool(),
            Independence = Utils.RandomEnum<ValueScale>(),
            ActivityLevel = Utils.RandomEnum<ValueScale>(),
            Budget = Utils.RandomEnum<ValueScale>(),
            FirstListed = DateTime.UtcNow.AddDays(_random.Next(-263, -90))
        };
    }

    // Returns the breed of an animal based on it's picture file name. 

    private static string ExtractBreed(string fileName)
    {
        StringBuilder breed = new();

        for (int i = 0; i < fileName.Length; i++)
        {
            char currentChar = fileName[i];

            if (char.IsUpper(currentChar) && i > 0)
                breed.Append(' ');
            else if (currentChar == '_')
                break;
            breed.Append(currentChar);
        }
        return breed.ToString();
    }

    // Returns the colour of an animal based on it's picture file name. 

    private static string ExtractColour(string fileName)
    {
        StringBuilder colour = new();
        bool afterUnderscore = false;

        foreach (char currentChar in fileName)
        {
            if (afterUnderscore)
            {
                if (char.IsUpper(currentChar))
                    colour.Append(' ');
                else if (currentChar == '.')
                    break;
                colour.Append(currentChar);
            }
            else if (currentChar == '_')
                afterUnderscore = true;
        }
        return colour.ToString();
    }

    // Returns the appropriate file path for the given animal type.

    private static string GetPath(AnimalType animal) => $"{_path}/{(animal == AnimalType.Dog ? "dogs" : "cats")}";

    // Returns the appropriate file path for the given user type.

    private static string GetPath(User user) =>
        $"{_path}/{(user is Adopter ? "adopters" : user is Admin ? "admins" : "shelters")}";

    // Upload a user profile picture to Azure Blob Storage. 

    private static async Task<string> UploadPicture(User user, string profilePicture, BlobManager blobManager)
    {
        string path = $"{GetPath(user)}/{profilePicture}";
        string fileName = $"{user.Username}-{Guid.NewGuid()}.jpg";
        await blobManager.UploadFileBlobAsync(path, fileName,
            user is Adopter ? _adopterContainer : user is Shelter ? _shelterContainer : _adminContainer);
        return fileName;
    }

    // Upload a pet profile picture to Azure Blob Storage.

    private static async Task<string> UploadPicture(Pet pet, string profilePicture, BlobManager blobManager)
    {
        string path = $"{GetPath(pet.AnimalType)}/{profilePicture}";
        string fileName = $"{pet.Id}-{Guid.NewGuid()}.jpg";
        await blobManager.UploadFileBlobAsync(path, fileName, _petContainer);
        return fileName;
    }

    // Clear all the profile pictures from Azure Blob Storage.

    private static async Task DeleteProfilePictures(string container, BlobManager blobManager)
    {
        var blobs = await blobManager.ListBlobsAsync(container);
        foreach (var blob in blobs)
            await blobManager.DeleteBlobAsync(blob, container);
    }
}