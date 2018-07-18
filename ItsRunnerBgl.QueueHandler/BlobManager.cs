using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ItsRunnerBgl.Utility
{
    public class BlobManager
    {
        private CloudBlobClient _client;

        /// <summary>
        /// Initializes a connection to the cloud storage account.
        /// </summary>
        /// <param name="cs">Connection string</param>
        public BlobManager(string cs)
        {
            var storageAccount = CloudStorageAccount.Parse(cs);
            _client = storageAccount.CreateCloudBlobClient();
        }


        public CloudBlobContainer GetContainerReference(string containerName)
        {
            return _client.GetContainerReference(containerName);
        }

        public async Task<string> UploadByteBlob(CloudBlobContainer blob, string blobName, byte[] data)
        {
            return await UploadByteBlob(blob, blobName, "", data);
        }

        /// <summary>
            /// Uploads a byte array to a blob container.
            /// </summary>
            /// <param name="blob">Container reference</param>
            /// <param name="blobName">Blob name</param>
            /// <param name="data">Data to upload</param>
            /// <returns>URL of the uploaded image</returns>
            public async Task<string> UploadByteBlob(CloudBlobContainer blob, string blobName, string contentType, byte[] data)
        {
            CloudBlockBlob cloudBlockBlob = blob.GetBlockBlobReference(blobName);
            await cloudBlockBlob.UploadFromByteArrayAsync(data, 0, data.Length);
            if (contentType.Length > 0)
            {
                cloudBlockBlob.Properties.ContentType = contentType;
                await cloudBlockBlob.SetPropertiesAsync();
            }
            return cloudBlockBlob.Uri.AbsoluteUri;
        }
    }
}
