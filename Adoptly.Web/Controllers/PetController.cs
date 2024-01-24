using Adoptly.Web.Filters;
using Adoptly.Web.Managers;
using Adoptly.Web.Models;
using Adoptly.Web.Models.Enums;
using Adoptly.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Adoptly.Web.Controllers;

public class PetController : Controller
{
    private readonly PetManager _petManager;
    private readonly ShelterManager _shelterManager;
    private readonly AdopterManager _adopterManager;
    private readonly ApplicationManager _applicationManager;
    private readonly AddressManager _addressManager;
    private readonly MatchManager _matchManager;
    private readonly FileService _fileService;
    private readonly EmailService _emailService;

    public PetController(PetManager petManager, ShelterManager shelterManager, MatchManager matchManager,
        AdopterManager adopterManager, ApplicationManager applicationManager, AddressManager addressManager,
        FileService fileService, EmailService emailService)
    {
        _petManager = petManager;
        _shelterManager = shelterManager;
        _adopterManager = adopterManager;
        _matchManager = matchManager;
        _applicationManager = applicationManager;
        _addressManager = addressManager;
        _fileService = fileService;
        _emailService = emailService;
    }

    public IActionResult Profile(int id)
    {
        // Get pet by id.
        
        Pet pet = _petManager.GetById(id);
        string username = HttpContext.Session.GetString("Username");
        
        // Get adopter by username.
        
        Adopter adopter = _adopterManager.GetByUsername(username);

        // Check if the adopter has already applied for the pet.
        
        if (adopter is not null)
        {
            Application application = _applicationManager.Get(adopter.Username, pet.Id);

            ViewBag.AdopterUsername = adopter.Username;
            ViewBag.Applied = application is not null;
        }

        return pet is not null ? View(pet) : RedirectToAction("Index", "Home");
    }

    [AuthorizeAdminOrShelter]
    public ActionResult EditProfile(int id)
    {
        // Check if the authorised user is not an admin.
        
        if (HttpContext.Session.GetString("Role") != nameof(Admin))
        {
            // Get shelter by username.
            
            Shelter shelter = _shelterManager.GetByUsername(HttpContext.Session.GetString("Username"));

            // Check if the shelter does not own the pet, and return to the pet profile.
            
            if (!shelter.Pets.Contains(_petManager.GetById(id)))
                return RedirectToAction("Profile", "Pet", new { id });
        }

        // Get pet by id.
        
        Pet pet = _petManager.GetById(id);

        return pet is not null ? View(pet) : RedirectToAction("Profile", "Pet", new { id });
    }

    [HttpPost]
    [AuthorizeAdminOrShelter]
    public async Task<ActionResult> EditProfile(Pet pet)
    {
        // Check if a profile picture is provided for the profile.
        
        if (pet.ImageFile is not null && pet.ImageFile.Length > 0)
        {
            string profilePicture = $"{pet.Id}-{Guid.NewGuid()}.jpg";
            try
            {
                // Upload pet profile picture to Azure's blob storage.
                
                bool uploaded = await _fileService.UploadFile(
                    pet.ImageFile, profilePicture, BlobContainers.PetProfilePictures);

                if (!uploaded)
                    ModelState.AddModelError("ProfilePicture", "Problem uploading profile picture.");
                else
                {
                    // Delete the old profile picture if any exists.
                    
                    if (pet.ProfilePicture is not null)
                        await _fileService.DeleteFile(pet.ProfilePicture, BlobContainers.PetProfilePictures);
                    pet.ProfilePicture = profilePicture;
                }
            }
            catch (IOException)
            {
                ModelState.AddModelError("ProfilePicture", "Invalid file type. Upload an image file.");
            }
        }

        if (!ModelState.IsValid)
            return View(pet);

        // Update pet.
        
        _petManager.Update(pet);

        return RedirectToAction("Profile", "Pet", new { id = pet.Id });
    }

    [AuthorizeAdopter]
    public IActionResult ApplicationForm(int petId, string shelterUsername)
    {
        string id = HttpContext.Session.GetString("Username");
        
        // Get authorised adopter by username.
        
        Adopter adopter = _adopterManager.GetByUsername(id);

        // Check if the adopter is null and redirect to the home page.
        
        if (adopter is null)
            return RedirectToAction("Index", "Home");

        // Get pet by id
        
        Pet pet = _petManager.GetById(petId);

        // Check if pet status is unavailable or on hold, and return to the home page.
        
        if (pet?.Status is not (Status.Available or Status.OnHold))
            return RedirectToAction("Index", "Home");

        Shelter shelter = _shelterManager.GetByUsername(shelterUsername);

        Application application = _applicationManager.Get(adopter.Username, pet.Id);

        // Check if the adopter has already applied for this pet, and return to the home page.
        
        if (application is not null)
            return RedirectToAction("Index", "Home");

        // Return a view model object with the adopter's details to the view.
        
        return View(new ApplicationFormViewModel
        {
            PetId = pet.Id,
            PetName = pet.Name,
            ShelterName = shelter.Name,
            FirstName = adopter.FirstName,
            LastName = adopter.LastName,
            Email = adopter.Login.Email,
            State = adopter.State
        });
    }

    [HttpPost]
    [AuthorizeAdopter]
    public async Task<IActionResult> ApplicationForm(ApplicationFormViewModel viewModel)
    {
        string id = HttpContext.Session.GetString("Username");
        
        // Get authorised adopter by username.
        
        Adopter adopter = _adopterManager.GetByUsername(id);

        // Check if the adopter is null and redirect to the home page.
        
        if (adopter is null)
            return RedirectToAction("Index", "Home");

        // Get pet by id.
        
        Pet pet = _petManager.GetById(viewModel.PetId);

        // Check if pet status is unavailable or on hold, and return to the home page.
        
        if (pet?.Status is not (Status.Available or Status.OnHold))
            return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
            return View(viewModel);

        // Create and add an address object to the database.
        
        Address address = new()
        {
            Address1 = viewModel.Address1,
            Address2 = viewModel.Address2,
            City = viewModel.City,
            State = (State)viewModel.State,
            Postcode = viewModel.Postcode,
            ContactNumber = viewModel.ContactNumber
        };
        
        _addressManager.Add(address);

        // Create and add an application object to the database.
        
        Application application = new()
        {
            AdopterUsername = adopter.Username,
            PetId = pet.Id,
            AddressId = address.Id,
            AdoptionReason = viewModel.AdoptionReason
        };

        _applicationManager.Add(application);

        // Send email notification to shelter.

        await _emailService.SendEmailAsync(pet.Shelter.Login.Email, EmailType.Notification, application);

        return RedirectToAction("ApplicationReceived", "Pet",
            new { petId = viewModel.PetId, shelterName = viewModel.ShelterName });
    }

    [AuthorizeAdopter]
    public IActionResult EditApplicationForm(int petId, string shelterUsername)
    {
        string id = HttpContext.Session.GetString("Username");
        
        // Get authorised adopter by username.
        
        Adopter adopter = _adopterManager.GetByUsername(id);

        // Check if the adopter is null and redirect to the home page.
        
        if (adopter is null)
            return RedirectToAction("Index", "Home");

        // Get pet by id.
        
        Pet pet = _petManager.GetById(petId);

        // Check if pet status is unavailable or on hold, and return to the home page.
        
        if (pet?.Status is not (Status.Available or Status.OnHold))
            return RedirectToAction("Index", "Home");

        Shelter shelter = _shelterManager.GetByUsername(shelterUsername);
        Application application = _applicationManager.Get(adopter.Username, pet.Id);

        // Check if the application status is not received or processing, and return to the home page.
        
        if (application?.Status is not (ApplicationStatus.Received or ApplicationStatus.Processing))
            return RedirectToAction("Index", "Home");

        // Return a view model object with the application's details to the view.
        
        return View(new ApplicationFormViewModel
        {
            PetId = application.PetId,
            PetName = pet.Name,
            ShelterName = shelter.Name,
            FirstName = adopter.FirstName,
            LastName = adopter.LastName,
            Email = adopter.Login.Email,
            Address1 = application.Address.Address1,
            Address2 = application.Address.Address2,
            City = application.Address.City,
            State = application.Address.State,
            Postcode = application.Address.Postcode,
            Country = application.Address.Country,
            ContactNumber = application.Address.ContactNumber,
            AdoptionReason = application.AdoptionReason,
        });
    }

    [AuthorizeAdopter]
    [HttpPost]
    public IActionResult EditApplicationForm(ApplicationFormViewModel viewModel)
    {
        string id = HttpContext.Session.GetString("Username");
        
        // Get authorised adopter by username.
        
        Adopter adopter = _adopterManager.GetByUsername(id);

        // Check if the adopter is null and redirect to the home page.

        if (adopter is null)
            return RedirectToAction("Index", "Home");

        // Get pet by id.
        
        Pet pet = _petManager.GetById(viewModel.PetId);

        // Check if pet status is unavailable or on hold, and return to the home page.

        if (pet?.Status is not (Status.Available or Status.OnHold))
            return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
            return View(viewModel);

        // Update application.
        
        Application application = _applicationManager.Get(adopter.Username, viewModel.PetId);

        Address address = _addressManager.GetById(application.AddressId);
        address.Address1 = viewModel.Address1;
        address.Address2 = viewModel.Address2;
        address.City = viewModel.City;
        address.State = (State)viewModel.State;
        address.Postcode = viewModel.Postcode;
        address.ContactNumber = viewModel.ContactNumber;

        _addressManager.Update(address);

        application.AdoptionReason = viewModel.AdoptionReason;
        _applicationManager.Update(application);

        return RedirectToAction("Application", "Adopter", new { petId = viewModel.PetId });
    }


    public IActionResult ApplicationReceived(int petId, string shelterName)
    {
        // Get authorised adopter by username.

        Adopter adopter = _adopterManager.GetByUsername(HttpContext.Session.GetString("Username"));

        // Check if the adopter is null and redirect to the home page.

        if (adopter is null)
            return RedirectToAction("Index", "Home");

        // Get pet by id.
        
        Pet pet = _petManager.GetById(petId);

        // Check if the pet is null, and return to the home page.
        
        if (pet is null)
            return RedirectToAction("Index", "Home");

        // Get application by adopter username and pet id.
        
        Application application = _applicationManager.Get(adopter.Username, pet.Id);

        // Check if application is null, and return to the homepage.
        
        if (application is null)
            return RedirectToAction("Index", "Home");

        // Set the pet and shelter name, and return to the view.
        
        ViewBag.PetName = pet.Name;
        ViewBag.ShelterName = shelterName;

        return View();
    }

    [HttpPost]
    [AuthorizeAdminOrShelter]
    public async Task<ActionResult> DeleteProfile(Pet pet)
    {
        
        // Delete pet's profile picture from Azure's blob storage.
        
        await _fileService.DeleteFile(pet.ProfilePicture, BlobContainers.PetProfilePictures);
        
        // Get pet by id.
        
        _applicationManager.DeleteAllByPetId(pet.Id);
        
        // Delete pet.
        
        _matchManager.DeleteAllByPetId(pet.Id);
        _petManager.Delete(pet);
        return RedirectToAction("Profile", "Shelter", new { id = pet.Shelter.Username });
    }

    [HttpPost]
    [AuthorizeAdminOrShelter]
    public async Task<ActionResult> DeleteProfilePicture(Pet selectedPet)
    {
        // Get pet by id.
        
        Pet pet = _petManager.GetById(selectedPet.Id);

        // Check if the pet does have a profile picture.
        
        if (pet.ProfilePicture is not null)
        {
            // Delete pet profile picture.
            
            await _fileService.DeleteFile(pet.ProfilePicture, BlobContainers.ShelterProfilePictures);
            pet.ProfilePicture = null;
            _petManager.Update(pet);
        }

        return RedirectToAction("Profile", "Pet", new { id = pet.Id });
    }

    public ActionResult ProfilePicture(int id)
    {
        Pet pet = _petManager.GetById(id);

        // Display default profile picture.

        if (pet is null || pet.ProfilePicture is null)
            return RedirectToAction("Get", "Images", new
            {
                containerName = "brand-assets",
                imgName = "pet-default-profile-picture.png",
                contentType = "img/png"
            });

        // Display pet's profile picture.

        return RedirectToAction("Get", "Images", new
        {
            containerName = "pet-profile-pictures",
            imgName = pet.ProfilePicture,
            contentType = "img/jpg"
        });
    }
}