using FileService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

namespace FileService.Controllers
{
    [ApiController]
    [Route("/")]
    public class MainController : ControllerBase
    {
        private static readonly List<StoredFileInfo> Files = new List<StoredFileInfo>();
        [HttpPost]
        public string UploadFile()
        {
            return "UploadFile";
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
