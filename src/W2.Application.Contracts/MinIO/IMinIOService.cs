using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace W2.MinIO
{
    public interface IMinIOService
    {
        Task<object> UploadFile(List<IFormFile> Files);
        Task<string> GetPresignedObject(string FileName);
    }
}
