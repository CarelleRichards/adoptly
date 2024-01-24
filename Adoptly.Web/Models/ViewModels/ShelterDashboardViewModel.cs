using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adoptly.Web.Models;

public class ShelterDashboardViewModel
{
        [DisplayName("New Applicants")]
        public List<Application> NewApplicants { get; set; } = new();

        [InverseProperty("Shelter")]
        public virtual List<Pet> RecentPets { get; set; } = new();
}