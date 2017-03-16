using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageSharingWebRole.Models
{
    [Serializable()]
    public class ValidationRequest
    {
        public int ImageId { get; set; }
        public string UserId { get; set; }
    }
}