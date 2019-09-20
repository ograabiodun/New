using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TaskHNG.Models
{
    [MetadataType(typeof(UserMetadata))]
    public partial class UserReg
    {
        public string ConfirmPassword { get; set; }
    }

    public class UserMetadata
    {
        [Display(Name = "First Name:")]
        [Required(AllowEmptyStrings = false, ErrorMessage ="FirstName Required")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name:")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "LastName Required")]
        public string LastName { get; set; }

        [Display(Name = "Email:")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email Required")]
        [DataType(DataType.EmailAddress)]
        public string EmailId { get; set; }

        [Display(Name ="Password:")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password Required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage ="Minimum 6 CHaracters Required")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password:")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage  = "Password do not Match")]
        public string ConfirmPassword { get; set; }
    }
}