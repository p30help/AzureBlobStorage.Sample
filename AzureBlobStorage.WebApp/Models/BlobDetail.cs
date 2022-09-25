namespace AzureBlobStorage.WebApp.Models
{
    public class BlobDetail
    {
        public string Name { get; set; }

        public IDictionary<string, string> Metadata { get; set; }
    }
}