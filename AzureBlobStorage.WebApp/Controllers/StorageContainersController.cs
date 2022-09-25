using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureBlobStorage.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace AzureBlobStorage.WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StorageContainersController : ControllerBase
    {
        private readonly ILogger<StorageContainersController> _logger;
        private readonly BlobServiceClient _blobClient;

        public StorageContainersController(ILogger<StorageContainersController> logger, BlobServiceClient blobClient)
        {
            _logger = logger;
            _blobClient = blobClient;
        }


        [HttpGet("GetContainers")]
        public async Task<IEnumerable<ContainerDetail>> GetContainers()
        {
            var containers = _blobClient.GetBlobContainersAsync();

            var list = new List<ContainerDetail>();
            await foreach (var item in containers)
            {
                list.Add(new ContainerDetail()
                {
                    Name = item.Name,
                    IsDeleted = item.IsDeleted
                });
            }

            return list;
        }


        [HttpPost("CreateNewContainer")]
        public async Task<IActionResult> CreateNewContainer(string blobContainerName, bool isPublic)
        {
            var publicType = (isPublic) ? Azure.Storage.Blobs.Models.PublicAccessType.Blob
                : Azure.Storage.Blobs.Models.PublicAccessType.None;

            var blobContainer = await _blobClient.CreateBlobContainerAsync(blobContainerName, publicType);

            return Ok($"Created");
        }

        [HttpDelete("DeleteContainer/{blobContainerName}")]
        public async Task<IActionResult> DeleteContainer(string blobContainerName)
        {
            var response = await _blobClient.DeleteBlobContainerAsync(blobContainerName);

            if(response.IsError)
            {
                return BadRequest(response.Content.ToString());
            }

            return Ok($"Deleted");
        }

    }
}