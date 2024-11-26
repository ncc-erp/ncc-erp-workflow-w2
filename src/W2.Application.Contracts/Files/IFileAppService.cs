using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace W2.Files
{
    public interface IFileAppService: IApplicationService
    {
        Task<object> CreateUploadAsync(UploadFileDto input);
        Task<string> GetPresignedUrlAsync(string FileName);
    }
}