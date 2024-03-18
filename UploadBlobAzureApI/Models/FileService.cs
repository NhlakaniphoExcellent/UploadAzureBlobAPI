using Azure.Storage;
using Azure.Storage.Blobs;
using System.Runtime.Serialization;

namespace UploadBlobAzureApI.Models
{
    // This is for acccessing and securing our storage azure account in container using key in "Access Keys"
    public class FileService
    {
        private readonly string _storageaccount = "firststorage001";
        private readonly string _key = "kP382sum5oyIth3r0rmfYfe0ffn9zy7OeBL/FaDOs90XkMrxgaPy6RaD5tQlrPoLxAuGRXL+r0bu+AStyV/U+g==";
        private readonly BlobContainerClient _fileContainer;

        public FileService()
        {
            var credential = new StorageSharedKeyCredential(_key, _storageaccount);
            var BlobUri = $"https//{ _storageaccount }.blob.core.windows.net";
            var blobserviceClient = new BlobServiceClient(new Uri(BlobUri), credential);
            _fileContainer = blobserviceClient.GetBlobContainerClient("files");
        }

        // This method lists through all files in azure storage (container) database.
        public async Task<List<BlobDTO>> ListAsync()
        {
            List<BlobDTO> files = new List<BlobDTO>();

            await foreach(var file in _fileContainer.GetBlobsAsync())
            {
                string uri = _fileContainer.Uri.ToString();
                var name = file.Name;
                var FullUri = $"{uri}/{name}";

                files.Add(new BlobDTO
                {
                    Uri = FullUri,
                    Name = name,
                    ContentType = file.Properties.ContentType
                });
            }

            return files;
        }

        // Once it reaches here, this method uploads a new file and can upload one asynchronously
        public async Task<BlobResponseDTO> UploadAsync(IFormFile blob)
        {
            BlobResponseDTO response = new();
            BlobClient client = _fileContainer.GetBlobClient(blob.FileName);

            await using (Stream? data = blob.OpenReadStream())
            {
                await client.UploadAsync(data);
            }

            response.Status = $"File {blob.FileName} Uploaded Successfully!!";
            response.Error = false;
            response.Blob.Uri = client.Uri.AbsoluteUri;
            response.Blob.Name = client.Name;

            return response;
        }

        // This method downloads a file and can download files asynchronously
        public async Task<BlobDTO?> DownloadAsync(string blobFileName)
        {
            BlobClient file = _fileContainer.GetBlobClient(blobFileName);

            if(await file.ExistsAsync())
            {
                var data = await file.OpenReadAsync();
                Stream blobContent = data;

                var content = await file.DownloadAsync();

                string name =blobFileName;    
                string contentType = content.Value.Details.ContentType;

                return new BlobDTO
                {

                    Content = blobContent,
                    Name = name,
                    ContentType = contentType
                };
            }
            return null;
        }

        // This method deletes a file in the azure file storage
        public async Task<BlobResponseDTO> DeleteAsync(string blobFileName)
        {
            BlobClient file = _fileContainer.GetBlobClient(blobFileName);

            await file.DeleteAsync();

            return new BlobResponseDTO { Error = false, Status = $"File: {blobFileName} has been Deleted!!" };
        }


    }
}
