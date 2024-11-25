using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace W2.MinIO
{
    public class MinIOService : IMinIOService, ISingletonDependency
    {
        private readonly IMinioClient _minioClient;
        private readonly Configurations.MinIOConfiguration _minIOConfiguration;
        public MinIOService(IOptions<Configurations.MinIOConfiguration> minIOConfiguration)
        {
            _minIOConfiguration = minIOConfiguration.Value;
            _minioClient = new MinioClient()
                .WithEndpoint(_minIOConfiguration.Endpoint)
                .WithCredentials(_minIOConfiguration.AccessKey, _minIOConfiguration.SecretKey)
                .WithSSL()
                .Build();
        }

        public async Task<object> UploadFile(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                throw new UserFriendlyException("File is null or empty");
            }
            try
            {
                var urlList = new List<string>();
                foreach (var file in files)
                {
                    var objectName = $"w2/upload/{file.FileName}";
                    var bucketName = _minIOConfiguration.BucketName;
                    var contentType = file.ContentType;
                    var filestream = file.OpenReadStream();
                    var putObjectArgs = new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithStreamData(filestream)
                        .WithObjectSize(filestream.Length)
                        .WithContentType(contentType);
                    var result = await _minioClient.PutObjectAsync(putObjectArgs);
                    urlList.Add($"{_minIOConfiguration.PublicImageUrl}/{bucketName}/{objectName}");
                }
                return new
                { 
                    urls = string.Join(",", urlList)
                };


            }
            catch (MinioException ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetPresignedObject(string FileName)
        {
            var args = new PresignedGetObjectArgs()
                .WithBucket(_minIOConfiguration.BucketName)
                .WithObject($"w2/upload/{FileName}")
                .WithExpiry(60*5);
            String url = await _minioClient.PresignedGetObjectAsync(args);

            return url;
        }
    }
}
