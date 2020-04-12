using FileService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using FileService.Utilities;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;

namespace FileService.Controllers
{
    [ApiController]
    [Route("/")]
    public class MainController : ControllerBase
    {
        private static readonly List<StoredFileInfo> Files = new List<StoredFileInfo>();
        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions =
        {
            "txt"
        };

        [HttpPost]
        public async Task<IActionResult> UploadFile()
        {
            if(!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest();
            }

            var file = new StoredFileInfo();
            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), new FormOptions().MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            while(section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                
                if (hasContentDispositionHeader)
                {
                    if(contentDisposition.IsFileDisposition())
                    {
                        // Don't trust the file name sent by the client. To display the file name, HTML-encode the value.
                        var trustedFileNameForDisplay = WebUtility.HtmlEncode(contentDisposition.FileName.Value);
                        var trustedFileNameForFileStorage = Path.GetRandomFileName();

                        var streamedFileContent = await FileHelper.ProcessStreamedFile(section, contentDisposition, ModelState, _permittedExtensions, _fileSizeLimit);

                        if (!ModelState.IsValid)
                        {
                            return BadRequest(ModelState);
                        }

                        var trustedFilePath = trustedFileNameForFileStorage;
                        using (var targetStream = System.IO.File.Create(trustedFilePath))
                        {
                            await targetStream.WriteAsync(streamedFileContent);
                            file.FilePath = trustedFilePath;
                            file.FileName = trustedFileNameForDisplay;   
                        }
                    }
                    else if (contentDisposition.IsFormDisposition())
                    {
                        var content = new StreamReader(section.Body).ReadToEnd();
                        if (contentDisposition.Name == "userId" && int.TryParse(content, out var userId))
                        {
                            file.UserId = userId.ToString();
                        }

                        if (contentDisposition.Name == "comment")
                        {
                            file.Comment = content;
                        }

                        if (contentDisposition.Name == "isPrimary" && bool.TryParse(content, out var isPrimary))
                        {
                            file.IsPrimary = isPrimary;
                        }
                    }
                    section = await reader.ReadNextSectionAsync();
                }
                Files.Add(file);

                return Created(nameof(MainController), file);
            }

            return null;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadFile([FromRoute]string id)
        {
            foreach(var item in Directory.GetFiles("repo"))
            {
                Files.Add(new StoredFileInfo()
                {
                    FileName = "test",
                    FilePath = item,
                    Id = id,
                });
            }

            var file = Files.FirstOrDefault(p => p.Id == id);
            if (file != null)
            {
                var filePath = file.FilePath;
                var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(bytes, GetMimeTypes(Path.GetExtension(file.FilePath)), file.FileName);
            }
            else
            {
                return BadRequest($"File with GUID={id} Not Found.");
            }
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteFile([FromRoute]string id)
        {
            var file = Files.FirstOrDefault(p => p.Id == id);
            if (file != null)
            {
                System.IO.File.Delete(file.FilePath);
                return Ok();
            }
            else
            {
                return BadRequest($"File with GUID={id} Not Found.");
            }
        }
        [HttpHead]
        public string GetInformation()
        {
            return "Info";
        }
        private static string GetMimeTypes(string ext)
        {
            switch (ext)
            {
                case ".txt": return "text/plain";
                case ".csv": return "text/csv";
                case ".pdf": return "application/pdf";
                case ".doc": return "application/vnd.ms-word";
                case ".xls": return "application/vnd.ms-excel";
                case ".ppt": return "application/vnd.ms-powerpoint";
                case ".docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".pptx": return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case ".png": return "image/png";
                case ".jpg": return "image/jpeg";
                case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                default: return "application/octet-stream";
            }
        }
    }
}
