using System.Net;
using System.Threading.Tasks;
using Backend.Common;
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
                            attachments.AttachmentURL = $"voucher_{vouchersDTO.VoucherID}_{fileUrl}";
                            attachments.VoucherID = voucher.VoucherID;
                        await _unitOfWork.Attachments.AddAsync(attachments);
                    }
                }

                await _unitOfWork.CompleteAsync();

                response.Result = voucher.VoucherID!;
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
        // GET api/vouchers?pageNumber=1&pageSize=10 (for pagination)
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
        {
            var response = new APIResponse();
            try
            {
                // If pagination parameters are provided, use paginated endpoint
                if (pageNumber.HasValue && pageSize.HasValue)
                {
                    if (pageNumber.Value <= 0 || pageSize.Value <= 0)
                    {
                        response.IsSuccess = false;
                        response.statusCode = HttpStatusCode.BadRequest;
                        response.ErrorMasseges.Add("Page number and page size must be greater than 0.");
                        return BadRequest(response);
                    }

                    var vouchers = await _unitOfWork.Vouchers.GetVouchersPaginatedAsync(pageNumber.Value, pageSize.Value);
                    var totalCount = await _unitOfWork.Vouchers.GetTotalVouchersCountAsync();

                    var paginatedResult = new
                    {
                        data = vouchers,
                        pageNumber = pageNumber.Value,
                        pageSize = pageSize.Value,
                        totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize.Value)
                    };

                    response.Result = paginatedResult;
                    response.statusCode = HttpStatusCode.OK;
                    return Ok(response);
                }
                else
                {
                    // Return all vouchers if no pagination parameters
                    var vouchers = await _unitOfWork.Vouchers.GetAllAsync();
                    response.Result = vouchers;
                    response.statusCode = HttpStatusCode.OK;
                    return Ok(response);
                }
            }
            catch (System.Exception ex)
            {
                response.IsSuccess = false;
                response.statusCode = HttpStatusCode.InternalServerError;
                response.ErrorMasseges.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        // POST api/vouchers/page
        [HttpPost("page")]
        public async Task<ActionResult<PagedResult<VouchersReturnDTO>>> GetVouchersWithFilters(
            [FromBody] FilterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Clamp values to avoid abuse (e.g., pageSize=100000)
            const int maxPageSize = 100;
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 4;
            if (request.PageSize > maxPageSize) request.PageSize = maxPageSize;

            var vouchers = await _unitOfWork.Vouchers.GetVouchersPaginatedAsync(request.PageNumber, request.PageSize);
            var totalCount = await _unitOfWork.Vouchers.GetTotalVouchersCountAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return Ok(new PagedResult<VouchersReturnDTO>(
                vouchers,
                request.PageNumber,
                request.PageSize,
                totalCount,
                totalPages
            ));
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
                dto.VoucherID = id;
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
                    response.Result = "Voucher not found.";
                    response.ErrorMasseges.Add("Voucher not found.");
                    return NotFound(response);//here is the error?
                }

              var voucherDeleted=  await _unitOfWork.Vouchers.DeleteAsync(id);
                if (!voucherDeleted)
                {
                    response.IsSuccess = false;
                    response.statusCode = HttpStatusCode.InternalServerError;
                    response.ErrorMasseges.Add("Failed to delete voucher.");
                    return StatusCode((int)HttpStatusCode.InternalServerError, response);
                }
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
        // GET api/vouchers/guardian
        [HttpGet("vouchersGuardian")]
        public async Task<ActionResult<APIResponse>> GetVouchersGuardian([FromQuery] int? guardianID = null)
        {
            var response = new APIResponse();
            try
            {
                var vouchers = await _unitOfWork.Vouchers.GetAllVouchersGuardian(guardianID);
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
    }
}