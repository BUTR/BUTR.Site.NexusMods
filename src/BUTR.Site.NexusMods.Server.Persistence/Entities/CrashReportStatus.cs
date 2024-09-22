using System.ComponentModel.DataAnnotations;

namespace BUTR.Site.NexusMods.Server.Models;

public enum CrashReportStatus
{
    [Display(Name = "New")]
    New = 1,
    [Display(Name = "Being Looked At")]
    BeingLookedAt = 2,
    [Display(Name = "Not My Fault")]
    NotMyFault = 3,
    [Display(Name = "Known Issue")]
    Known = 4,
    [Display(Name = "Duplicate")]
    Duplicate = 5,
    [Display(Name = "Needs More Info")]
    NeedsMoreInfo = 6,
    [Display(Name = "Fixed")]
    Fixed = 7,
}