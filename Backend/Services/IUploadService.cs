using System.IO;
using System.Threading.Tasks;

namespace Backend.Services
{
    public interface IUploadService
    {
        Task<bool> LoadSampleDatasetAsync();
        Task<bool> UploadCsvAsync(Stream fileStream);
    }
}
