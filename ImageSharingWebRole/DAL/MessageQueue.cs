using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Configuration;
using ImageSharingWebRole.Models;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ImageSharingWebRole.DAL
{
    public class MessageQueue
    {
        public static bool USE_BLOB_STORAGE = bool.Parse(ConfigurationManager.AppSettings["USE_BLOB_STORAGE"]);
        public static string ACCOUNT = ConfigurationManager.AppSettings["AccountName"];
        public static string AccountKey = ConfigurationManager.AppSettings["AccountKey"];

        public static List<ValidationInfo> GetResponseMessages(string UserName)
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

                    // Fetch the queue attributes.
                    queue.FetchAttributes();

                    // Retrieve the cached approximate message count.
                    int? cachedMessageCount = queue.ApproximateMessageCount;

                    List<ValidationInfo> lstResponseMessage = new List<ValidationInfo>();

                    for (int i = 0; i < cachedMessageCount; i++)
                    {
                        // Peek at the next message
                        CloudQueueMessage peekedMessage = queue.PeekMessage();

                        lstResponseMessage.Add(ByteArrayToClass(peekedMessage.AsBytes));
                    }

                    return lstResponseMessage;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else return new List<ValidationInfo>();
        }

        public static bool DeleteResponseMessages(string UserName)
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

                // Fetch the queue attributes.
                queue.FetchAttributes();

                // Retrieve the cached approximate message count.
                int? cachedMessageCount = queue.ApproximateMessageCount;

                for (int i = 0; i < cachedMessageCount; i++)
                {
                    // Get the next message
                    CloudQueueMessage retrievedMessage = queue.GetMessage();

                    //Process the message in less than 30 seconds, and then delete the message
                    queue.DeleteMessage(retrievedMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool ClearAllQueues()
        {
            try
            {
                CloudStorageAccount account =
                    CloudStorageAccount.Parse(Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageConnectionString"));

                // Create the queue client.
                CloudQueueClient queueClient = account.CreateCloudQueueClient();
                
                foreach (var item in queueClient.ListQueues())
                {
                    string strQueueName = item.Name;
                    CloudQueue queue = queueClient.GetQueueReference(strQueueName);
                    queue.Delete();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        private static ValidationInfo ByteArrayToClass(byte[] buffer)
        {
            try
            {
                System.Xml.Serialization.XmlSerializer xmlS = new System.Xml.Serialization.XmlSerializer(typeof(ValidationInfo));
                MemoryStream ms = new MemoryStream(buffer);
                System.Xml.XmlTextWriter xmlTW = new System.Xml.XmlTextWriter(ms, System.Text.Encoding.UTF8);

                return (ValidationInfo)xmlS.Deserialize(ms);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}