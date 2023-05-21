using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CityInfo.API.Controllers
{
    [Route("api/files")]
    [Authorize]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider;

        public FilesController(FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
        {
            _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider ?? 
                throw new System.ArgumentNullException(nameof(fileExtensionContentTypeProvider));
        }

        [HttpGet("{fileId}")]
        public ActionResult GetFile(string fileId)
        {
            var pathToFile = "C:\\Users\\Antonia.Grosan\\Desktop\\projects\\webapis\\webAPI-asp-dot-net-core6" +
                "\\CityInfo\\CityInfo.API\\FeedbackOn5Fingers.pdf";

            var fileExists = System.IO.File.Exists(pathToFile);

            if (!fileExists)
            {
                return NotFound();
            }

            if(!_fileExtensionContentTypeProvider.TryGetContentType(pathToFile, out var contentType)) 
            {
                contentType = "application/octet-stream";
            }

            var stream = System.IO.File.OpenRead(pathToFile);            
            return File(stream, contentType, Path.GetFileName(pathToFile));
        }
    }
}
