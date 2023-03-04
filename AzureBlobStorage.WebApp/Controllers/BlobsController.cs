using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using AzureBlobStorage.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace AzureBlobStorage.WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BlobsController : ControllerBase
    {
        private readonly ILogger<BlobsController> _logger;
        private readonly BlobServiceClient _blobClient;

        public BlobsController(ILogger<BlobsController> logger, BlobServiceClient blobClient)
        {
            _logger = logger;
            _blobClient = blobClient;
        }

        [HttpGet("GetBlobs")]
        public async Task<IEnumerable<BlobDetail>> GetBlobs()
        {
            var container = GetBlobContainer();

            var blob = container.GetBlobsAsync();

            var list = new List<BlobDetail>();
            await foreach (var item in blob)
            {
                list.Add(new BlobDetail()
                {
                    Name = item.Name,
                    Metadata = item.Metadata
                });
            }

            return list;
        }

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var container = GetBlobContainer();

            try
            {
                var blobClient = container.GetBlobClient(file.FileName);
                var res = await blobClient.UploadAsync(file.OpenReadStream(), 
                    new BlobHttpHeaders { ContentType = file.ContentType });

                return Ok($"Uploaed - {res.Value.BlobSequenceNumber}");
            }
            catch(Azure.RequestFailedException exp)
            {
                if(exp.ErrorCode == "BlobAlreadyExists")
                {
                    return BadRequest("Blob aleardy exists");
                }

                throw;
            }
            catch (Exception exp)
            {
                throw;
            }
        }


        [HttpGet("ReadFileUri")]
        public async Task<IActionResult> ReadFileUri(string blobName)
        {
            var container = GetBlobContainer();

            try
            {
                var blobClient = container.GetBlobClient(blobName);

                return Ok(blobClient.Uri.ToString());
            }
            catch (Exception exp)
            {
                throw;
            }
        }

        [HttpGet("GetFileUrl")]
        public async Task<IActionResult> GetFileUrl(string blobName)
        {
            var container = GetBlobContainer();

            try
            {
                var blobClient = container.GetBlobClient(blobName);
                var sasBuilder = new BlobSasBuilder(BlobContainerSasPermissions.Read, DateTimeOffset.Now.AddMinutes(10))
                {
                    ContentDisposition = "attachment; filename=" + blobName
                };

                var url = blobClient.GenerateSasUri(sasBuilder);
                return Ok(url.AbsoluteUri);
            }
            catch (Exception exp)
            {
                throw;
            }
        }

        [HttpGet("ReadFile")]
        public async Task<IActionResult> ReadFile(string blobName)
        {
            var container = GetBlobContainer();

            try
            {
                var blobClient = container.GetBlobClient(blobName);

                var memoryStream = new MemoryStream();
                var blob = await blobClient.DownloadToAsync(memoryStream);

                return File(memoryStream, blobName);
            }
            catch (Exception exp)
            {
                throw;
            }
        }

        [HttpDelete("DeleteFile/{blobName}")]
        public async Task<IActionResult> DeleteFile(string blobName)
        {
            var container = GetBlobContainer();

            try
            {
                var blobClient = container.GetBlobClient(blobName);

                var delRes = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

                if(delRes.Value)
                {
                    return Ok("Deleted");
                }

                return BadRequest("Not Exist");
            }
            catch (Exception exp)
            {
                throw;
            }
        }

        private BlobContainerClient GetBlobContainer(string blobContainer = "vrgfiles")
        {
            var container = _blobClient.GetBlobContainerClient("vrgfiles");

            return container;
        }

    }
}