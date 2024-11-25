using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using W2.MinIO;

namespace W2.Files
{
    public class FileAppService : W2AppService, IFileAppService
    {
       private readonly IMinIOService _minIOService;
        public FileAppService(IMinIOService minIOService)
        {
            _minIOService = minIOService;
        }

        public async Task<object> CreateUploadAsync([FromForm] UploadFileDto input)
        {
            return await _minIOService.UploadFile(input.Files);
        }

        public async Task<string> GetPresignedUrlAsync(string FileName)
        {
            return await _minIOService.GetPresignedObject(FileName);
        }
    }
}
