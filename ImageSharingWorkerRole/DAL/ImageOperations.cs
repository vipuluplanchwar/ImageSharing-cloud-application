using ImageSharingWebRole.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ImageSharingWorkerRole.DAL
{
    public class ImageOperations
    {
        public static bool USE_BLOB_STORAGE = bool.Parse(Microsoft.Azure.CloudConfigurationManager.GetSetting("USE_BLOB_STORAGE"));
        public static string ACCOUNT = Microsoft.Azure.CloudConfigurationManager.GetSetting("AccountName");
        public static string AccountKey = Microsoft.Azure.CloudConfigurationManager.GetSetting("AccountKey");
        public static string CONTAINER = Microsoft.Azure.CloudConfigurationManager.GetSetting("Container");

        public static bool Validate(int ID)
        {
            if (USE_BLOB_STORAGE)
            {
                StorageCredentials credentials = new StorageCredentials(ACCOUNT, AccountKey);
                CloudStorageAccount cs = new CloudStorageAccount(credentials, true);
                CloudStorageAccount account =
                    CloudStorageAccount.Parse(Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageConnectionString"));

                CloudBlobClient client = account.CreateCloudBlobClient();
                CloudBlobContainer container = client.GetContainerReference(CONTAINER);
                CloudBlockBlob blob = container.GetBlockBlobReference(FilePath(null, ID));

                MemoryStream ms = new MemoryStream();
                blob.DownloadToStream(ms);

                System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
                if (image.RawFormat.Guid == System.Drawing.Imaging.ImageFormat.Jpeg.Guid)
                {
                    return true;
                }
                else return false;
            }
            else return false;
        }

        public static bool DeleteBlobs(List<int> lstImageIds)
        {
            if (USE_BLOB_STORAGE)
            {
                StorageCredentials credentials = new StorageCredentials(ACCOUNT, AccountKey);
                CloudStorageAccount cs = new CloudStorageAccount(credentials, true);
                CloudStorageAccount account =
                    CloudStorageAccount.Parse(Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageConnectionString"));

                CloudBlobClient client = account.CreateCloudBlobClient();
                CloudBlobContainer container = client.GetContainerReference(CONTAINER);

                foreach (var item in lstImageIds)
                {
                    container.GetBlockBlobReference(FilePath(null, item)).DeleteIfExists();
                }
            }
            return true;
        }

        public static string FilePath(HttpServerUtilityBase server,
                                      int imageId)
        {
            if (USE_BLOB_STORAGE)
            {
                return FileName(imageId);
            }
            else
            {
                string imgFileName = server.MapPath("~/Content/Images/" + FileName(imageId));
                return imgFileName;
            }
        }

        public static string FileName(int imageId)
        {
            return imageId + ".jpg";
        }
    }
}
