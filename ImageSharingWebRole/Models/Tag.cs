using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ImageSharingWebRole.Models
{
    public class Tag
    {
        [Key]
        public virtual int Id { get; set; }
        [MaxLength(20)]
        public virtual String name { get; set; }

        public virtual ICollection<Image> Images { get; set; }

        public Tag()
        {
            name = string.Empty;
        }

        public Tag(string Name)
        {
            name = Name;
        }
    }
}