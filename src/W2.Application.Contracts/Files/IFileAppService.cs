using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace W2.Files
{
    public interface IFileAppService: IApplicationService
    {
        Task<object> CreateUploadAsync(UploadFileDto input);
        Task<string> GetPresignedUrlAsync(string FileName);
    }
}