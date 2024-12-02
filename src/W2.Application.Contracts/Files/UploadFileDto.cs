using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace W2.Files
{
    public class UploadFileDto
    {
        [Required]
        public List<IFormFile> Files { get; set; }
    }
}
