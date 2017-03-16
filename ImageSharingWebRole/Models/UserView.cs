using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ImageSharingWebRole.Models
{
    public class UserView
    {
        [Required]
        [RegularExpression(@"[a-zA-Z0-9_]+", ErrorMessage = "User Id must only consist of characters, numbers or underscore")]
        public String UserId { get; set; }

        [Required]
        public bool ADA { get; set; }
    }
}