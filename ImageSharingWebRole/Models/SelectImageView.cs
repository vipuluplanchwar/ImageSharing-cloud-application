using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageSharingWebRole.Models
{
    public class SelectImageView
    {
        public int Id { get; set; }
        public string Caption { get; set; }
        public bool Approved { get; set; }
        public bool Delete { get; set; }

        public SelectImageView()
        { }

        public SelectImageView(int Id, string Caption, bool Approved)
        {
            this.Id = Id;
            this.Caption = Caption;
            this.Approved = Approved;
        }
    }
}