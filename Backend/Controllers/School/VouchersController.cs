using System.Net;
using System.Threading.Tasks;
using Backend.DTOS.School.Attachments;
using Backend.DTOS.School.Vouchers;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.School
{
    [Route("api/[controller]")]
    [ApiController]
    public class VouchersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public VouchersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // POST api/vouchers
        [HttpPost]
        public async Task<ActionResult<APIResponse>> Add([FromBody] VouchersDTO vouchersDTO)
        {
            var response = new APIResponse();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Invalid voucher data.");
                    return BadRequest(response);
                }

                var voucher = await _unitOfWork.Vouchers.AddAsync(vouchersDTO);
                
                var attachments = new AttachmentDTO();
                if (vouchersDTO.Attachments != null && vouchersDTO.Attachments.Any())
                {
                    foreach (var fileUrl in vouchersDTO.Attachments)
                    {
                            attachments.StudentID = vouchersDTO.StudentID;
                            attachments.AttachmentURL = $"voucher_{vouchersDTO.StudentID}_{fileUrl}";
                            attachments.VoucherID = voucher.VoucherID;
                        await _unitOfWork.Attachments.AddAsync(attachments);
                    }
                }

                await _unitOfWork.CompleteAsync();

                response.Result = voucher;
                response.statusCode = HttpStatusCode.Created;
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        // GET api/vouchers
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetAll()
        {
            var response = new APIResponse();
            try
            {
                var vouchers = await _unitOfWork.Vouchers.GetAllAsync();
                response.Result = vouchers;
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        // GET api/vouchers/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<APIResponse>> Get(int id)
        {
            var response = new APIResponse();
            try
            {
                var voucher = await _unitOfWork.Vouchers.GetByIdAsync(id);
                if (voucher == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Voucher not found.");
                    return NotFound(response);
                }

                response.Result = voucher;
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        // PUT api/vouchers/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<APIResponse>> Update(int id, [FromBody] VouchersDTO dto)
        {
            var response = new APIResponse();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.BadRequest;
                    response.ErrorMasseges.Add("Invalid voucher data.");
                    return BadRequest(response);
                }

                var existing = await _unitOfWork.Vouchers.GetByIdAsync(id);
                if (existing == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Voucher not found.");
                    return NotFound(response);
                }

                await _unitOfWork.Vouchers.UpdateAsync(dto);
                await _unitOfWork.CompleteAsync();

                response.Result = "Voucher updated successfully.";
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        // DELETE api/vouchers/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            var response = new APIResponse();
            try
            {
                var existing = await _unitOfWork.Vouchers.GetByIdAsync(id);
                if (existing == null)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.NotFound;
                    response.ErrorMasseges.Add("Voucher not found.");
                    return NotFound(response);
                }

                await _unitOfWork.Vouchers.DeleteAsync(id);
                await _unitOfWork.CompleteAsync();

                response.Result = "Voucher deleted successfully.";
                response.statusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }
    }
}