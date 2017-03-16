using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ImageSharingWorkerRole.DAL
{
    public class MessageQueue
    {
        public static bool USE_BLOB_STORAGE = bool.Parse(Microsoft.Azure.CloudConfigurationManager.GetSetting("USE_BLOB_STORAGE"));
        public static string ACCOUNT = Microsoft.Azure.CloudConfigurationManager.GetSetting("AccountName");
        public static string AccountKey = Microsoft.Azure.CloudConfigurationManager.GetSetting("AccountKey");

        public static bool EnqueResponseMessage(string ImageCaption,string UserName, bool isValidated)
        {
            if (USE_BLOB_STORAGE)
            {
                try
                {
                    StorageCredentials credentials = new StorageCredentials(ACCOUNT, AccountKey);
                    CloudStorageAccount cs = new CloudStorageAccount(credentials, true);
                    CloudStorageAccount account =
                        CloudStorageAccount.Parse(Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageConnectionString"));

                    // Create the queue client.
                    CloudQueueClient queueClient = account.CreateCloudQueueClient();

                    // Retrieve a reference to a container.
                    CloudQueue queue = queueClient.GetQueueReference(UserName);

                    // Create the queue if it doesn't already exist
                    queue.CreateIfNotExists();

                    ImageSharingWebRole.Models.ValidationInfo vi = new ImageSharingWebRole.Models.ValidationInfo();
                    vi.ImageCaption = ImageCaption;
                    vi.isValidated = isValidated;

                    // Create a message and add it to the queue.
                    CloudQueueMessage message = new CloudQueueMessage(vi.ClassToByteArray());
                    queue.AddMessage(message);
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else return false;
        }
    }
}
