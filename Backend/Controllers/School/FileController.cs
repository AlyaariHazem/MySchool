using Backend.Interfaces;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly mangeFilesService _mangeFilesService;

        public FileController(mangeFilesService mangeFilesService)
        {
            _mangeFilesService = mangeFilesService;
        }

        [HttpPost("uploadImage")]
        public async Task<ActionResult<APIResponse>> UploadImage([FromForm] IFormFile file, [FromForm] string folderName, [FromForm] int itemId)
        {
            var response = new APIResponse();
            try
            {
                if (file == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("No file uploaded.");
                    return BadRequest(response);
                }

                var filePaths = await _mangeFilesService.UploadImage(file, folderName, itemId);

                response.Result = filePaths;
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        [HttpPost("uploadFiles")]
        public async Task<ActionResult<APIResponse>> UploadAttachments([FromForm] List<IFormFile> files, [FromForm] string folderName = "Attachments", [FromForm] int voucherId = 0)
        {
            var response = new APIResponse();
            try
            {
                if (files == null || !files.Any())
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("No files uploaded.");
                    return BadRequest(response);
                }

                var filePaths = await _mangeFilesService.UploadAttachments(files, folderName, voucherId);

                response.Result = filePaths;
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        [HttpDelete("deleteFile")]
        public ActionResult<APIResponse> DeleteFile([FromQuery] string folderName, [FromQuery] int itemId)
        {
            var response = new APIResponse();
            try
            {
                if (string.IsNullOrEmpty(folderName) || itemId == 0)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Invalid parameters.");
                    return BadRequest(response);
                }

                var filePath = _mangeFilesService.RemoveFile(folderName, itemId);

                if (filePath == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("File not found.");
                    return NotFound(response);
                }

                response.Result = filePath;
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }
    }
}
