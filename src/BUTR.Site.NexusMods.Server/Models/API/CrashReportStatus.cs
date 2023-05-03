using System.ComponentModel.DataAnnotations;

namespace BUTR.Site.NexusMods.Server.Models.API;

public enum CrashReportStatus
{
    [Display(Name = "New")]
    New,
    [Display(Name = "Being Looked At")]
    BeingLookedAt,
    [Display(Name = "Not My Fault")]
    NotMyFault,
    [Display(Name = "Known Issue")]
    Known,
    [Display(Name = "Duplicate")]
    Duplicate,
    [Display(Name = "Needs More Info")]
    NeedsMoreInfo,
    [Display(Name = "Fixed")]
    Fixed
}