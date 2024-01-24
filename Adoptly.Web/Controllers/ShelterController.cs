using Adoptly.Web.Filters;
using Adoptly.Web.Managers;
using Adoptly.Web.Models;
using Adoptly.Web.Models.Enums;
using Adoptly.Web.Services;
using Microsoft.AspNetCore.Mvc;
using SimpleHashing.Net;

namespace Adoptly.Web.Controllers;

public class ShelterController : Controller
{
    private readonly ShelterManager _shelterManager;
    private readonly PetManager _petManager;
    private readonly AdopterManager _adopterManager;
    private readonly LoginManager _loginManager;
    private readonly ApplicationManager _applicationManager;
    private readonly AddressManager _addressManager;
    private readonly MatchManager _matchManager;
    private readonly FileService _fileService;
    private readonly EmailService _emailService;
    private const int PageSize = 6;
    private const int PageSizeEditPets = 5;
    private const int FeaturedApplicationPetCount = 6;
    private static readonly ISimpleHash SimpleHash = new SimpleHash();
    
    public ShelterController(ShelterManager shelterManager, PetManager petManager, LoginManager loginManager,
        ApplicationManager applicationManager, AddressManager addressManager, MatchManager matchManager,
        AdopterManager adopterManager, FileService fileService, EmailService emailService)
    {
        _shelterManager = shelterManager;
        _petManager = petManager;
        _loginManager = loginManager;
        _applicationManager = applicationManager;
        _addressManager = addressManager;
        _adopterManager = adopterManager;
        _matchManager = matchManager;
        _fileService = fileService;
        _emailService = emailService;
    }

    [AuthorizeShelter]
    public IActionResult Index()
    {
        // Get all applications by shelter username.
        
        var applications = _applicationManager.GetAll(HttpContext.Session.GetString("Username"));
        
        // Get a list of pets owned by the shelter.
        
        List<Pet> recentPets = _petManager.GetByShelterUsername(HttpContext.Session.GetString("Username")).ToList();

        // Create a view model object containing recent applications and the oldest pet accounts created
        // by the shelter and return to the view.
        
        ShelterDashboardViewModel viewModel = new()
        {
            NewApplicants = applications
                .OrderByDescending(x => x.DateApplied)
                .Where(x => x.Archived == false && x.Status == ApplicationStatus.Received ||
                            x.Status == ApplicationStatus.Processing)
                .Take(FeaturedApplicationPetCount).ToList(),
            RecentPets = recentPets
                .OrderByDescending(x => x.FirstListed)
                .Take(PageSize)
                .ToList()
        };
        return View(viewModel);
    }

    [AuthorizeAnyUser]
    public IActionResult Profile(string id, int page = 1)
    {
        // Get shelter by username.
        
        Shelter shelter = _shelterManager.GetByUsername(id);

        // Check if the shelter is null and redirect to the home page.
        
        if (shelter is null)
            return RedirectToAction("Index", "Home");

        // Paginate shelter's pets.

        int totalPages = (int)Math.Ceiling((double)shelter.Pets.Count / PageSize);

        if (totalPages != 0 && page > totalPages)
            return RedirectToAction("Index", "Home");

        shelter.Pets = shelter.Pets.OrderByDescending(x => x.FirstListed)
            .Skip((page - 1) * PageSize).Take(PageSize).ToList();

        ViewBag.TotalResults = shelter.Pets.Count;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        // Return shelter to the view.
        
        return View(shelter);
    }

    [AuthorizeAdminOrShelter]
    public ActionResult EditProfile(int page = 1, string shelterUsername = null)
    {
        Shelter shelter = null;
        
        // Check if the authorised user is either an admin or a shelter.
        
        shelter = _shelterManager.GetByUsername(shelterUsername ?? HttpContext.Session.GetString("Username"));

        // Check if the shelter is null and redirect to the home page.
        
        if (shelter is null)
            return RedirectToAction("Index", "Home");

        // Paginate shelter's pets.

        int totalPages = (int)Math.Ceiling((double)shelter.Pets.Count / PageSizeEditPets);

        if (totalPages != 0 && page > totalPages)
            return RedirectToAction("Index", "Home");

        shelter.Pets = shelter.Pets.OrderByDescending(x => x.FirstListed)
            .Skip((page - 1) * PageSizeEditPets).Take(PageSizeEditPets).ToList();

        ViewBag.TotalResults = shelter.Pets.Count;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.Email = shelter.Login.Email;

        // Return shelter to the view.
        
        return View(shelter);
    }

    [HttpPost]
    [AuthorizeAdminOrShelter]
    public async Task<ActionResult> EditProfile(Shelter shelter, string username = null)
    {
        // Check if a profile picture is provided for the profile.
        
        if (shelter.ImageFile is not null && shelter.ImageFile.Length > 0)
        {
            string profilePicture = $"{shelter.Username}-{Guid.NewGuid()}.jpg";
            try
            {
                // Upload shelter profile picture to Azure's blob storage.
                
                bool uploaded = await _fileService.UploadFile(
                    shelter.ImageFile, profilePicture, BlobContainers.ShelterProfilePictures);

                if (!uploaded)
                    ModelState.AddModelError("ProfilePicture", "Problem uploading profile picture.");
                else
                {
                    // Delete the old profile picture if any exists.
                    
                    if (shelter.ProfilePicture is not null)
                        await _fileService.DeleteFile(shelter.ProfilePicture, BlobContainers.ShelterProfilePictures);

                    shelter.ProfilePicture = profilePicture;
                }
            }
            catch (IOException)
            {
                ModelState.AddModelError("ProfilePicture", "Invalid file type. Upload an image file.");
            }
        }

        if (!ModelState.IsValid)
        {
            // Get shelter from the session object.
            
            shelter = _shelterManager.GetByUsername(username ?? HttpContext.Session.GetString("Username"));

            if (shelter is null)
                return RedirectToAction("Index", "Home");

            // Paginate shelter's pets.

            int totalPages = (int)Math.Ceiling((double)shelter.Pets.Count / PageSizeEditPets);

            shelter.Pets = shelter.Pets.OrderByDescending(x => x.FirstListed)
                .Skip((PaginationViewModel.StartPage - 1) * PageSizeEditPets).Take(PageSizeEditPets).ToList();

            ViewBag.TotalResults = shelter.Pets.Count;
            ViewBag.CurrentPage = PaginationViewModel.StartPage;
            ViewBag.TotalPages = totalPages;

            return View(shelter);
        }

        // Update shelter.
        
        _shelterManager.Update(shelter);

        // Get a list of pets owned by the shelter.
        
        List<Pet> pets = _petManager.GetByShelterUsername(username ?? HttpContext.Session.GetString("Username"));

        // Update pet state property, if any, to match the shelter's state property.
        
        if (pets.Any())
            foreach (var pet in pets.Select(shelterPet => _petManager.GetById(shelterPet.Id)))
            {
                pet.State = shelter.State;
                _petManager.Update(pet);
            }

        return RedirectToAction("Profile", "Shelter", new { id = shelter.Username });
    }

    [AuthorizeShelter]
    public ActionResult CreatePetProfile()
    {
        // Return the view with a new view model object.
        
        return View(new CreatePetProfileViewModel
        {
            Shelter = _shelterManager.GetByUsername(HttpContext.Session.GetString("Username"))
        });
    }

    [HttpPost]
    [AuthorizeShelter]
    public async Task<IActionResult> CreatePetProfile(CreatePetProfileViewModel viewModel)
    {
        // Check if the authorised user is a shelter.
        
        Shelter shelter = _shelterManager.GetByUsername(HttpContext.Session.GetString("Username"));
        string profilePicture = null;

        // Check if a profile picture is provided for the profile.
        if (viewModel.ImageFile is not null && viewModel.ImageFile.Length > 0)
        {
            profilePicture = $"{shelter.Name}-{Guid.NewGuid()}.jpg";
            try
            {
                // Upload pet profile picture to Azure's blob storage.
                bool uploaded = await _fileService.UploadFile(
                    viewModel.ImageFile, profilePicture, BlobContainers.PetProfilePictures);

                if (!uploaded)
                    ModelState.AddModelError("ProfilePicture", "Problem uploading profile picture.");
            }
            catch (IOException)
            {
                ModelState.AddModelError("ProfilePicture", "Invalid file type. Upload an image file.");
            }
        }

        if (!ModelState.IsValid)
            return View(viewModel);

        // Create pet object and add to the database.
        
        Pet pet = new()
        {
            ShelterUsername = shelter.Username,
            Name = viewModel.Name,
            AnimalType = (AnimalType)viewModel.AnimalType,
            Description = viewModel.Description,
            ProfilePicture = profilePicture,
            State = shelter.State,
            Sex = (Sex)viewModel.Sex,
            Status = Status.Available,
            Age = viewModel.Age,
            Breed = viewModel.Breed,
            Colour = (Colour)viewModel.Colour,
            Desexed = (bool)viewModel.Desexed,
            AllergyFriendly = (bool)viewModel.AllergyFriendly,
            Independence = (ValueScale)viewModel.Independence,
            ActivityLevel = (ValueScale)viewModel.ActivityLevel,
            Budget = (ValueScale)viewModel.Budget,
        };

        _petManager.Add(pet);

        return RedirectToAction("Profile", "Pet", new { id = pet.Id });
    }

    [HttpPost]
    [AuthorizeAdminOrShelter]
    public async Task<ActionResult> DeleteProfilePicture(string username = null)
    {
        // Check if the authorised user is either an admin or a shelter.
        
        Shelter shelter = _shelterManager.GetByUsername(username ?? HttpContext.Session.GetString("Username"));

        // Check if the user does have a profile picture.
        
        if (shelter.ProfilePicture is not null)
        {
            // Delete user profile picture.
            
            await _fileService.DeleteFile(
                $"{shelter.Username}-profile-picture.jpg", BlobContainers.ShelterProfilePictures);

            shelter.ProfilePicture = null;
            _shelterManager.Update(shelter);
        }

        return RedirectToAction("Profile", "Shelter", new { id = shelter.Username });
    }

    [HttpPost]
    [AuthorizeAdminOrShelter]
    public async Task<IActionResult> UpdateEmail(UpdateEmailViewModel viewModel, string username = null)
    {
        // Check if the authorised user is either an admin or a shelter.
        
        Shelter shelter = _shelterManager.GetByUsername(username ?? HttpContext.Session.GetString("Username"));

        // Check if the entered email is the same in the database.
        
        if (shelter.Login.Email == viewModel.Email)
            ModelState.AddModelError("Email", "Please enter a new email address to update your account.");

        // Check if the entered email does not belong to another user.
        
        if (_loginManager.GetByEmail(viewModel.Email) is not null)
            ModelState.AddModelError("Email", "Email already exists.");

        // Paginate shelter's pets.

        int totalPages = (int)Math.Ceiling((double)shelter.Pets.Count / PageSizeEditPets);

        if (totalPages != 0 && PaginationViewModel.StartPage > totalPages)
            return RedirectToAction("Index", "Home");

        shelter.Pets = shelter.Pets.OrderByDescending(x => x.FirstListed)
            .Skip((PaginationViewModel.StartPage - 1) * PageSizeEditPets).Take(PageSizeEditPets).ToList();

        ViewBag.TotalResults = shelter.Pets.Count;
        ViewBag.CurrentPage = PaginationViewModel.StartPage;
        ViewBag.TotalPages = totalPages;
        ViewBag.DisplaySecurityTab = true;

        if (!ModelState.IsValid)
            return View(nameof(EditProfile), shelter);

        // Send verification email.
        
        await _emailService.SendEmailAsync(
            viewModel.Email, EmailType.ChangeEmail, shelter.Login.VerificationToken, shelter.Login.Email);

        ViewBag.DisplayEmailSuccess = true;

        return View(nameof(EditProfile), shelter);
    }

    [AuthorizeAnyUser]
    public ActionResult ProfilePicture(string id)
    {
        Shelter shelter = _shelterManager.GetByUsername(id);

        // Display default profile picture.

        if (shelter is null || shelter.ProfilePicture is null)
            return RedirectToAction("Get", "Images", new
            {
                containerName = "brand-assets",
                imgName = "shelter-default-profile-picture.png",
                contentType = "img/png"
            });

        // Display user's profile picture.

        return RedirectToAction("Get", "Images", new
        {
            containerName = "shelter-profile-pictures",
            imgName = shelter.ProfilePicture,
            contentType = "img/jpg"
        });
    }

    [HttpPost]
    [AuthorizeAdminOrShelter]
    public IActionResult EditPassword(EditPasswordViewModel viewModel, string username = null)
    {
        Shelter shelter = null;
        
        // Check if the authorised user is either an admin or a shelter.

        shelter = _shelterManager.GetByUsername(username ?? HttpContext.Session.GetString("Username"));

        int totalPages = (int)Math.Ceiling((double)shelter.Pets.Count / PageSizeEditPets);

        ViewBag.Email = shelter.Login.Email;
        ViewBag.DisplaySecurityTab = true;
        ViewBag.TotalResults = shelter.Pets.Count;
        ViewBag.CurrentPage = PaginationViewModel.StartPage;
        ViewBag.TotalPages = totalPages;

        // Check if the user is a shelter.
        
        if (username is null)
        {
            if (viewModel.OldPassword == null | viewModel.NewPassword == null | viewModel.ConfirmPassword == null)
                return View(nameof(EditProfile), shelter);

            // Check if the old password entered is correct.
            
            bool passwordCorrect = SimpleHash.Verify(viewModel.OldPassword, shelter.Login.PasswordHash);
            
            // Check if new and confirm passwords match.
            
            bool passwordMatch = viewModel.NewPassword == viewModel.ConfirmPassword;

            if (!passwordCorrect)
                ModelState.AddModelError("OldPassword", "Incorrect current password.");
        }

        // Remove old password from the model state if the user is an admin.
        if (username is not null)
            ModelState.Remove("OldPassword");

        if (!ModelState.IsValid)
            return View(nameof(EditProfile), shelter);

        // Update password.
        
        shelter.Login.PasswordHash = SimpleHash.Compute(viewModel.NewPassword);
        _loginManager.Update(shelter.Login);

        ViewBag.DisplayPasswordSuccess = true;

        return View(nameof(EditProfile), shelter);
    }

    [AuthorizeShelter]
    public ActionResult Applications(int page = 1)
    {
        Shelter shelter = _shelterManager.GetByUsername(HttpContext.Session.GetString("Username"));

        // Check if the authorised user is the shelter user who owns the applications.

        if (shelter is null)
            return RedirectToAction("Index", "Shelter");

        ViewBag.ShelterName = shelter.Name;

        // Get a list of pets by application count.
        
        List<Pet> petsByApplicationCount = _petManager.GetByApplicationCount(shelter.Username);

        int totalPages = (int)Math.Ceiling((double)petsByApplicationCount.Count / PageSize);

        if (totalPages != 0 && page > totalPages)
            return RedirectToAction("Index", "Home");

        // Paginate pets and order by first listed.
 
        petsByApplicationCount = petsByApplicationCount
            .OrderByDescending(x => x.FirstListed)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        ViewBag.TotalResults = petsByApplicationCount.Count;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        // Return pets by application count to the view.
        
        return View(petsByApplicationCount);
    }


    [AuthorizeShelter]
    public ActionResult PetApplications(int petId, int page = 1)
    {
        string id = HttpContext.Session.GetString("Username");

        // Check if the authorised user is the shelter user who owns the applications.

        if (id != _shelterManager.GetByUsername(id).Username)
            return RedirectToAction("Index", "Shelter");

        // Get a pet by pet id.

        Pet pet = _petManager.GetById(petId);

        if (pet is null)
            return RedirectToAction("Applications", "Shelter");

        // Get a list of applications by pet id.

        var applications = _applicationManager.GetByPetId(pet.Id);

        ViewBag.PetName = pet.Name;
        ViewBag.PetId = pet.Id;

        int totalPages = (int)Math.Ceiling((double)applications.Count / PageSize);

        if (totalPages != 0 && page > totalPages)
            return RedirectToAction("Index", "Home");
        
        // Paginate applications and order by date applied.

        applications = applications
            .OrderByDescending(x => x.DateApplied)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        ViewBag.TotalResults = applications.Count;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        // Return applications to the view.
        
        return View(applications);
    }

    [AuthorizeShelter]
    public ActionResult PetApplication(int petId, string adopterUsername)
    {
        Shelter shelter = _shelterManager.GetByUsername(HttpContext.Session.GetString("Username"));

        // Check if the authorised user is the shelter user who owns the application.
        
        if (shelter is null)
            return RedirectToAction("Index", "Adopter");

        // Get an application by adopter username and pet id.
        
        Application application = _applicationManager.Get(adopterUsername, petId);

        // Return application to the view.
        
        return View(application);
    }

    [AuthorizeShelter]
    public ActionResult ArchivedPetApplications(int petId, int page = 1)
    {
        string id = HttpContext.Session.GetString("Username");

        // Check if the authorised user is the shelter user who owns the applications.
        
        if (id != _shelterManager.GetByUsername(id).Username)
            return RedirectToAction("Index", "Shelter");

        // Get a pet by pet id.
        
        Pet pet = _petManager.GetById(petId);

        if (pet is null)
            return RedirectToAction("Applications", "Shelter");

        // Get a list of archived applications by pet id.
        
        var applications = _applicationManager.GetAllByPetId(pet.Id);

        ViewBag.PetName = pet.Name;
        ViewBag.PetId = pet.Id;

        int totalPages = (int)Math.Ceiling((double)applications.Count / PageSize);

        if (totalPages != 0 && page > totalPages)
            return RedirectToAction("Index", "Home");

        // Paginate applications and order by date applied.
        
        applications = applications
            .OrderByDescending(x => x.DateApplied)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        ViewBag.TotalResults = applications.Count;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        // Return applications to the view.
        
        return View(applications);
    }

    [AuthorizeShelter]
    public ActionResult ArchivedApplications(int page = 1)
    {
        string id = HttpContext.Session.GetString("Username");

        // Check if the authorised user is the shelter user who owns the applications. 
        
        if (id != _shelterManager.GetByUsername(id).Username)
            return RedirectToAction("Index", "Shelter");

        // Get a list of archived applications by shelter username.
        
        List<Application> applications = _applicationManager.GetAllArchived(id);

        if (applications is null)
            return RedirectToAction("Index", "Shelter");

        int totalPages = (int)Math.Ceiling((double)applications.Count / PageSize);

        if (totalPages != 0 && page > totalPages)
            return RedirectToAction("Index", "Home");

        // Paginate archived applications and order by date applied.
        
        applications = applications
            .OrderByDescending(x => x.DateApplied)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        ViewBag.ShelterName = _shelterManager.GetByUsername(id).Name;
        ViewBag.TotalResults = applications.Count;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        // Return applications to the view.
        
        return View(applications);
    }


    [AuthorizeShelter]
    [HttpPost]
    public ActionResult ProcessApplication(Application application)
    {
        string id = HttpContext.Session.GetString("Username");

        // Check if the authorised user is the shelter user who owns the application. 
        
        if (id != _shelterManager.GetByUsername(id).Username)
            return RedirectToAction("Index", "Shelter");

        // Process the application.
        
        application.DateProcessed = DateTime.UtcNow;
        application.Status = ApplicationStatus.Processing;
        application.Archived = false;
        _applicationManager.Update(application);

        // Change the status of the pet to on hold.
        
        Pet pet = _petManager.GetById(application.PetId);
        pet.Status = Status.OnHold;
        _petManager.Update(pet);

        return RedirectToAction("PetApplications", "Shelter", new { petId = application.PetId });
    }

    [HttpPost]
    [AuthorizeShelter]
    public async Task<ActionResult> AcceptApplication(Application acceptApplication)
    {
        string id = HttpContext.Session.GetString("Username");

        // Check if the authorised user is the shelter user who owns the application. 
        
        if (id != _shelterManager.GetByUsername(id).Username)
            return RedirectToAction("Index", "Shelter");

        // Accept the application.
        
        Application application = _applicationManager.Get(acceptApplication.AdopterUsername, acceptApplication.PetId);
        application.DateProcessed = DateTime.UtcNow;
        application.Status = ApplicationStatus.Accepted;
        application.Visited = false;
        _applicationManager.Update(application);
 
        // Change the status of the pet to adopted.
        
        Pet pet = _petManager.GetById(application.PetId);
        pet.Status = Status.Adopted;
        _petManager.Update(pet);

        var allApplicationsByPetId = _applicationManager.GetByPetId(application.PetId);

        // Check if there are any other applications for the adopted pet and archive them.
        
        foreach (Application applicationByPetId in allApplicationsByPetId.Where(x =>
                     application.AdopterUsername != x.AdopterUsername))
        {
            applicationByPetId.Status = ApplicationStatus.Closed;
            applicationByPetId.Archived = true;
            applicationByPetId.DateProcessed = DateTime.Now;
            _applicationManager.Update(applicationByPetId);
        }

        // Send email notification to adopter.

        await _emailService.SendEmailAsync(application.Adopter.Login.Email, EmailType.Notification, application);

        return RedirectToAction("PetApplications", "Shelter", new { petId = application.PetId });
    }

    [HttpPost]
    [AuthorizeShelter]
    public async Task<ActionResult> RejectApplication(Application rejectApplication)
    {
        string id = HttpContext.Session.GetString("Username");

        // Check if the authorised user is the shelter user who owns the application. 
        
        if (id != _shelterManager.GetByUsername(id).Username)
            return RedirectToAction("Index", "Shelter");

        // Reject the application.
        
        Application application = _applicationManager.Get(rejectApplication.AdopterUsername, rejectApplication.PetId);
        application.DateProcessed = DateTime.UtcNow;
        application.Status = ApplicationStatus.Rejected;
        application.Archived = true;
        _applicationManager.Update(application);

        // Send email notification to adopter.

        await _emailService.SendEmailAsync(application.Adopter.Login.Email, EmailType.Notification, application);

        return RedirectToAction("PetApplications", "Shelter", new { petId = application.PetId });
    }

    [HttpPost]
    [AuthorizeShelter]
    public ActionResult ArchiveApplication(Application application)
    {
        string id = HttpContext.Session.GetString("Username");

        // Check if the authorised user is the shelter user who owns the application. 
        
        if (id != _shelterManager.GetByUsername(id).Username)
            return RedirectToAction("Index", "Shelter");

        // Archive the application.
        application.DateProcessed = DateTime.UtcNow;
        application.Archived = true;
        application.Status = ApplicationStatus.Accepted;
        _applicationManager.Update(application);

        return RedirectToAction("PetApplications", "Shelter", new { petId = application.PetId });
    }

    [HttpPost]
    [AuthorizeAdminOrShelter]
    public async Task<ActionResult> DeleteAccount(Shelter shelter, string username = null)
    {
        string id = null;

        // Check if the authorised user is either an admin or a shelter.
        
        if (HttpContext.Session.GetString("Role") == nameof(Shelter))
            id = HttpContext.Session.GetString("Username");
        else if (HttpContext.Session.GetString("Role") == nameof(Admin))
            id = username;

        if (shelter.Username != id)
            return RedirectToAction("Index", "Shelter");

        // Delete the shelter's applications address.
        
        foreach (var application in _applicationManager.GetAll(id))
            _addressManager.Delete(_addressManager.GetById(application.AddressId));

        // Delete applications and matches associated with the shelter account.
        
        _applicationManager.DeleteAll(id);
        _matchManager.DeleteAllByShelterUsername(id);

        // Delete profile pictures of the shelter's pets from Azure's blob storage.
        
        foreach (var pet in _petManager.GetByShelterUsername(id).Where(pet => pet.ProfilePicture is not null))
            await _fileService.DeleteFile(pet.ProfilePicture, BlobContainers.PetProfilePictures);

        // Delete the shelter's pets.
        
        _petManager.DeleteAllByShelterUsername(id);

        // Delete shelter's profile picture form Azure's blob storage.
        
        if (shelter.ProfilePicture is not null)
            await _fileService.DeleteFile(shelter.ProfilePicture, BlobContainers.ShelterProfilePictures);

        // Delete shelter's login.
        
        _loginManager.Delete(_loginManager.GetByUsername(id));

        return HttpContext.Session.GetString("Role") == nameof(Admin)
            ? RedirectToAction("Users", "Admin")
            : RedirectToAction("Logout", "Login");
    }
}