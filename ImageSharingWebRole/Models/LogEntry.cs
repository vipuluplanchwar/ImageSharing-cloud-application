using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Core;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.DataServices;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImageSharingWebRole.Models
{
    public class LogEntry : TableEntity
    {
        public LogEntry()
        {

        }

        public LogEntry(int imageId)
        {
            CreateKeys(imageId);
        }

        public DateTime EntryDate { get; set; }
        public string UserId { get; set; }
        public string Caption { get; set; }
        public string Uri { get; set; }
        public int ImageId { get; set; }

        public void CreateKeys(int imageId)
        {
            EntryDate = DateTime.UtcNow;
            PartitionKey = EntryDate.ToString("MMddyyyy");

            this.ImageId = imageId;

            RowKey = string.Format("{0}-s{1:10}_{2}",
                ImageId,
                DateTime.MaxValue.Ticks - EntryDate.Ticks,
                Guid.NewGuid());
        }
    }
}