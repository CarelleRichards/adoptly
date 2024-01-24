using System.Data;
using System.Text;
using Adoptly.Web.Filters;
using Adoptly.Web.Managers;
using Adoptly.Web.Models;
using Adoptly.Web.Models.Enums;
using Adoptly.Web.Services;
using Adoptly.Web.Utilities;
using Google.DataTable.Net.Wrapper;
using Microsoft.AspNetCore.Mvc;
using SimpleHashing.Net;

namespace Adoptly.Web.Controllers;

public class AdminController : Controller
{
    private readonly AdminManager _adminManager;
    private readonly AdopterManager _adopterManager;
    private readonly LoginManager _loginManager;
    private readonly ShelterManager _shelterManager;
    private readonly PetManager _petManager;
    private readonly FileService _fileService;
    private readonly EmailService _emailService;
    private const int NewUsersCount = 6;
    private const int PageSize = 6;
    private const int MonthsInYear = 12;
    private static readonly ISimpleHash SimpleHash = new SimpleHash();

    public AdminController(AdminManager adminManager, FileService fileService, ShelterManager shelterManager,
        LoginManager loginManager, EmailService emailService, PetManager petManager, AdopterManager adopterManager)
    {
        _adminManager = adminManager;
        _loginManager = loginManager;
        _fileService = fileService;
        _emailService = emailService;
        _petManager = petManager;
        _adopterManager = adopterManager;
        _shelterManager = shelterManager;
    }

    [AuthorizeAdmin]
    public IActionResult Index()
    {
        return View(new AdminDashboardViewModel()
        {
            AdminReport = CreateReport(),
            NewestUsers = GetNewestUsers()
        });
    }

    [AuthorizeAdmin]
    public IActionResult Filter(AllUsersViewModel viewModel)
    {
        TempData.Put("AllUsersViewModel", viewModel);
        return RedirectToAction("Users");
    }

    [HttpPost]
    [AuthorizeAdmin]
    public IActionResult Search(AllUsersViewModel viewModel)
    {
        // Get login by user email.
        
        Login login = _loginManager.GetByEmail(viewModel.UserEmail);

        // Return login to the view if it exists.
        
        if (login is null)
            return RedirectToAction("Users", "Admin", new { userExists = false });

        return login.User is Adopter
            ? RedirectToAction("Profile", "Adopter", new { id = login.User.Username })
            : RedirectToAction("Profile", "Shelter", new { id = login.User.Username });
    }

    [AuthorizeAdmin]
    public IActionResult Users(int page = 1, bool userExists = true)
    {
        if (page < PaginationViewModel.StartPage)
            return RedirectToAction("Index", "Home");

        // Get a list of all user logins.
        
        List<Login> logins = _loginManager.GetAll().Where(x => x.User is not Admin).ToList();

        int totalPages = (int)Math.Ceiling((double)logins.Count / PageSize);

        if (totalPages != 0 && page > totalPages)
            return RedirectToAction("Index", "Home");

        ViewBag.TotalPages = totalPages;
        ViewBag.CurrentPage = page;
        ViewBag.TotalResults = logins.Count;

        AllUsersViewModel viewModel = TempData.ContainsKey("AllUsersViewModel")
            ? TempData.Get<AllUsersViewModel>("AllUsersViewModel")
            : new();
        
        // Paginate and return logins by date created.
        
        if (viewModel.SortOrder == UsersSortOrder.OldestAccount)
            viewModel.Logins = logins
                .OrderBy(x => x.DateCreated)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        else
            viewModel.Logins = logins
                .OrderByDescending(x => x.DateCreated)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

        // Check if the search query does not return a login, and add to the model state error.
        
        if (!userExists)
            ModelState.AddModelError("UserEmail", "User does not exist!");

        userExists = true;

        return View(viewModel);
    }

    [AuthorizeAdmin]
    public FileContentResult DownloadOverview()
    {
        const string CsvFormat = "\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\" \n";
        const string AverageAdoptionTimeFormat = "{0:%d} days, {0:%h} hours, {0:%m} minutes, {0:%s} seconds";

        StringBuilder csv = new(string.Format(CsvFormat, "Pets adopted", "Pets available", "Dogs available",
            "Cats available", "Adopter accounts", "Shelter accounts", "Average adoption time"));

        AdminReportViewModel adminReport = CreateReport();

        csv.Append(string.Format(CsvFormat,
            adminReport.PetsAdopted, adminReport.PetsAvailable, adminReport.DogsAvailable,
            adminReport.CatsAvailable, adminReport.AdopterAccounts, adminReport.ShelterAccounts,
            string.Format(AverageAdoptionTimeFormat, adminReport.AverageAdoptionTime)));

        string fileName = $"adoptlyOverviewReport--{DateTime.UtcNow:yyyy-dd-M--HH-mm-ss}--utc.csv";

        return File(new UTF8Encoding().GetBytes(csv.ToString()), "text/csv", fileName);
    }

    [AuthorizeAdmin]
    public FileContentResult DownloadMonthlyAdoptions()
    {
        const string CsvFormat =
            "\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\" \n";

        StringBuilder csv = new(string.Format(CsvFormat, "January", "February", "March", "April",
            "May", "June", "July", "August", "September", "October", "November", "December"));

        Dictionary<int, int> monthlyAdoptions = GetMonthlyAdoptions(_petManager.GetAll());

        foreach (var month in monthlyAdoptions)
            csv.Append($"\"{month.Value}\",");

        csv.Length--;
        csv.Append('\n');

        DateTime timeCreated = DateTime.UtcNow;

        string fileName =
            $"adoptly{timeCreated:MMMM}AdoptionsReport{timeCreated.Year}--{timeCreated:yyyy-dd-M--HH-mm-ss}--utc.csv";

        return File(new UTF8Encoding().GetBytes(csv.ToString()), "text/csv", fileName);
    }

    private AdminReportViewModel CreateReport()
    {
        List<Pet> allPets = _petManager.GetAll();
        List<Adopter> allAdopters = _adopterManager.GetAll();
        List<Shelter> allShelters = _shelterManager.GetAll();

        return new AdminReportViewModel()
        {
            PetsAdopted = allPets
                .Where(x => x.Status == Status.Adopted)
                .ToList().Count,
            PetsAvailable = allPets
                .Where(x => x.Status == Status.Available)
                .ToList().Count,
            DogsAvailable = allPets
                .Where(x => x.Status == Status.Available)
                .Where(x => x.AnimalType == AnimalType.Dog)
                .ToList().Count,
            CatsAvailable = allPets
                .Where(x => x.Status == Status.Available)
                .Where(x => x.AnimalType == AnimalType.Cat)
                .ToList().Count,
            AdopterAccounts = allAdopters
                .Where(x => x.Login.Verified)
                .ToList().Count,
            ShelterAccounts = allShelters
                .Where(x => x.Login.Verified)
                .ToList().Count,
            AverageAdoptionTime = GetAverageAdoptionTime(allPets),
            MonthlyAdoptions = GetMonthlyAdoptionsJson(allPets)
        };
    }

    // Returns the number of adoptions per month for the current 
    // year in Google DataTable json format for Google Charts.

    private static string GetMonthlyAdoptionsJson(List<Pet> allPets)
    {
        Dictionary<int, int> monthlyAdoptions = GetMonthlyAdoptions(allPets);

        Google.DataTable.Net.Wrapper.DataTable table = new();

        table.AddColumn(new Column(ColumnType.String, "Month", "Month"));
        table.AddColumn(new Column(ColumnType.Number, "Adoptions", "Adoptions"));
        table.AddColumn(new Column(ColumnType.String) { Role = ColumnRole.Tooltip });

        foreach (var month in monthlyAdoptions)
        {
            Row row = table.NewRow();
            row.AddCellRange(new Cell[]
            {
                new Cell(month.Key.ToString("00")),
                new Cell(month.Value),
                new Cell($"{month.Value} adoption(s)")
            });
            table.AddRow(row);
        }

        return table.GetJson();
    }

    // Returns the number of adoptions per month for the current year.
    // Dictionary key = month and value = number of adoptions.

    private static Dictionary<int, int> GetMonthlyAdoptions(List<Pet> allPets)
    {
        Dictionary<int, int> monthlyAdoptions = new();

        // Start each month at 0 adoptions.

        for (int i = 1; i <= MonthsInYear; i++)
            monthlyAdoptions.Add(i, 0);

        // Filter out pets that haven't been adopted in the current year.

        List<Pet> adoptedPets = allPets
            .Where(x => x.Status == Status.Adopted)
            .Where(x => x.StatusLastUpdated.Year == DateTime.Now.Year)
            .ToList();

        // Calculate adoptions for each month.

        foreach (Pet pet in adoptedPets)
            monthlyAdoptions[pet.StatusLastUpdated.Month]++;

        return monthlyAdoptions;
    }

    // Returns average adoption time from a given list of pets.

    private static TimeSpan GetAverageAdoptionTime(List<Pet> allPets)
    {
        List<TimeSpan> timesToAdoption = new();

        foreach (Pet pet in allPets)
            if (pet.Status == Status.Adopted)
                timesToAdoption.Add(pet.StatusLastUpdated - pet.FirstListed);

        return timesToAdoption.Average();
    }

    // Returns the top newest verified users in the system.

    private List<UserViewModel> GetNewestUsers()
    {
        List<Login> logins = _loginManager.GetAll()
            .Where(x => x.Verified)
            .Where(x => x.User is not Admin)
            .OrderByDescending(x => x.DateVerified)
            .Take(NewUsersCount)
            .ToList();

        List<UserViewModel> users = new();

        foreach (var login in logins)
            users.Add(new UserViewModel()
            {
                Username = login.User.Username,
                ProfilePicture = login.User.ProfilePicture,
                DateVerified = login.DateVerified,
                UserType = login.User is Adopter ? UserType.Adopter : UserType.Shelter
            });

        return users;
    }

    [AuthorizeAdmin]
    public IActionResult Profile(string id)
    {
        // Get admin by username
        
        Admin admin = _adminManager.GetByUsername(id);
        
        // Return admin to the view.
        
        return admin is not null ? View(admin) : RedirectToAction("Index", "Home");
    }

    [AuthorizeAdmin]
    public ActionResult EditProfile()
    {
        // Check if the authorised user is accessing the view.

        Admin admin = _adminManager.GetByUsername(HttpContext.Session.GetString("Username"));
        ViewBag.Email = admin.Login.Email;
        
        // Return admin to the view.

        return admin is not null ? View(admin) : RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [AuthorizeAdmin]
    public async Task<IActionResult> EditProfile(Admin admin)
    {
        // Check if a profile picture is provided for the profile.

        if (admin.ImageFile is not null && admin.ImageFile.Length > 0)
        {
            string profilePicture = $"{admin.Username}-{Guid.NewGuid()}.jpg";
            try
            {
                // Upload admin profile picture to Azure's blob storage.

                bool uploaded = await _fileService.UploadFile(
                    admin.ImageFile, profilePicture, BlobContainers.AdminProfilePictures);

                if (!uploaded)
                    ModelState.AddModelError("ProfilePicture", "Problem uploading profile picture.");
                else
                {
                    // Delete the old profile picture if any exists.

                    if (admin.ProfilePicture is not null)
                        await _fileService.DeleteFile(admin.ProfilePicture, BlobContainers.AdminProfilePictures);
                    admin.ProfilePicture = profilePicture;
                }
            }
            catch (IOException)
            {
                ModelState.AddModelError("ProfilePicture", "Invalid file type. Upload an image file.");
            }
        }

        if (!ModelState.IsValid)
            return View(admin);

        // Update admin.

        _adminManager.Update(admin);

        return RedirectToAction("Profile", "Admin", new { id = admin.Username });
    }

    [HttpPost]
    [AuthorizeAdmin]
    public async Task<ActionResult> DeleteProfilePicture()
    {
        // Check if the authorised user is accessing the view.

        Admin admin = _adminManager.GetByUsername(HttpContext.Session.GetString("Username"));

        // Check if the user does have a profile picture.

        if (admin.ProfilePicture is not null)
        {
            // Delete user profile picture.

            await _fileService.DeleteFile($"{admin.Username}-profile-picture.jpg", BlobContainers.AdminProfilePictures);
            admin.ProfilePicture = null;
            _adminManager.Update(admin);
        }

        return RedirectToAction("Profile", "Admin", new { id = admin.Username });
    }

    [HttpPost]
    [AuthorizeAdmin]
    public async Task<IActionResult> UpdateEmail(UpdateEmailViewModel viewModel)
    {
        // Check if the authorised user is accessing the view.

        Admin admin = _adminManager.GetByUsername(HttpContext.Session.GetString("Username"));

        ViewBag.Email = admin.Login.Email;
        ViewBag.DisplaySecurityTab = true;

        // Check if the entered email is the same in the database.

        if (_loginManager.GetByEmail(viewModel.Email) is not null)
            ModelState.AddModelError("Email", "Email already exists.");

        // Check if the entered email does not belong to another user.

        if (admin.Login.Email == viewModel.Email)
            ModelState.AddModelError("Email", "Please enter a new email address to update your account.");

        if (!ModelState.IsValid)
            return View(nameof(EditProfile), admin);

        // Send verification email.

        await _emailService.SendEmailAsync(
            viewModel.Email, EmailType.ChangeEmail, admin.Login.VerificationToken, admin.Login.Email);

        ViewBag.DisplayEmailSuccess = true;

        return View(nameof(EditProfile), admin);
    }

    [AuthorizeAdmin]
    public ActionResult ProfilePicture(string id)
    {
        Admin admin = _adminManager.GetByUsername(id);

        // Display default profile picture.

        if (admin is null || admin.ProfilePicture is null)
            return RedirectToAction("Get", "Images", new
            {
                containerName = "brand-assets",
                imgName = "admin-default-profile-picture.png",
                contentType = "img/png"
            });

        // Display user's profile picture.

        return RedirectToAction("Get", "Images", new
        {
            containerName = "admin-profile-pictures",
            imgName = admin.ProfilePicture,
            contentType = "img/jpg"
        });
    }

    [HttpPost]
    [AuthorizeAdmin]
    public IActionResult EditPassword(EditPasswordViewModel viewModel)
    {
        // Check if the authorised user is accessing the view.

        Admin admin = _adminManager.GetByUsername(HttpContext.Session.GetString("Username"));

        ViewBag.Email = admin.Login.Email;
        ViewBag.DisplaySecurityTab = true;

        
        if (viewModel.NewPassword == null | viewModel.ConfirmPassword == null)
            return View(nameof(EditProfile), admin);
        
        // Check if new and confirm passwords match.

        bool passwordMatch = viewModel.NewPassword == viewModel.ConfirmPassword;

        // Remove old password from the model state.

        ModelState.Remove("OldPassword");
        
        if (!ModelState.IsValid)
            return View(nameof(EditProfile), admin);

        // Update password.

        admin.Login.PasswordHash = SimpleHash.Compute(viewModel.NewPassword);
        _loginManager.Update(admin.Login);
        ViewBag.DisplayPasswordSuccess = true;
        return View(nameof(EditProfile), admin);
    }
}