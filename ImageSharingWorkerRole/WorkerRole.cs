using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using ImageSharingWebRole.Models;
using ImageSharingWebRole.DAL;
using Microsoft.Azure;
using ImageSharingWorkerRole.DAL;

namespace ImageSharingWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {

        // The name of your queue
        string QueueName = ValidationQueue.QueueName;

        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        QueueClient Client;
        ManualResetEvent CompletedEvent = new ManualResetEvent(false);

        private ImageSharingWorkerRole.DAL.ApplicationDbContext db = new ImageSharingWorkerRole.DAL.ApplicationDbContext();

        public override void Run()
        {
            Trace.WriteLine("Starting processing of messages");

            //Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
            Client.OnMessage(ValidateImage);

            CompletedEvent.WaitOne();
        }

        private void ValidateImage(BrokeredMessage ReceivedMessage)
        {
            try
            {
                Trace.WriteLine("Processing Service Bus message: " + ReceivedMessage.SequenceNumber.ToString());

                ValidationRequest ValidationReq = ReceivedMessage.GetBody<ValidationRequest>();

                Image image = db.Images.Find(ValidationReq.ImageId);
                bool isValidated = false;

                if (image != null)
                {
                    Trace.WriteLine("Processing Image ID: " + image.Id);
                    isValidated = ImageOperations.Validate(image.Id);
                    if (isValidated)
                    {
                        image.Validated = true;
                        Trace.WriteLine("Message Processed and Image validation passed for image ID :  " + image.Id);
                    }
                    else
                    {
                        //Delete image from BLOB
                        List<int> lstImageDelete = new List<int>();
                        lstImageDelete.Add(image.Id);
                        ImageOperations.DeleteBlobs(lstImageDelete);

                        //Delete database record of image
                        db.Images.Remove(image);

                        Trace.WriteLine("Message Processed and Image validation failed for image ID : " + image.Id);
                    }
                    db.SaveChanges();

                    //Reply to User by Queues
                    ImageSharingWorkerRole.DAL.MessageQueue.EnqueResponseMessage(image.Caption, image.User.Email.Substring(0, image.User.Email.IndexOf('@')), isValidated);
                    Trace.WriteLine("Queue Message enqueued");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                throw ex;
            }
            finally
            {
                ReceivedMessage.Complete();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // Create the queue if it does not exist already
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!namespaceManager.QueueExists(QueueName))
            {
                namespaceManager.CreateQueue(QueueName);
            }

            // Initialize the connection to Service Bus Queue
            Client = QueueClient.CreateFromConnectionString(connectionString, QueueName);
            
            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            Client.Close();
            CompletedEvent.Set();
            base.OnStop();
        }
    }
}
