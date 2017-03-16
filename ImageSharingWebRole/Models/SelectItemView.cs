using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageSharingWebRole.Models
{
    public class SelectItemView
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public bool Active { get; set; }
        public bool Checked { get; set; }

        public SelectItemView()
        {
        }

        public SelectItemView(string Id, string UserName, bool Active)
        {
            this.Id = Id;
            this.UserName = UserName;
            this.Active = Active;
        }
    }
}