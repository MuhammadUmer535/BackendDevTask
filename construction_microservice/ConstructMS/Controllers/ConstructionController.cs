using ConstructMS.DTOs;
using ConstructMS.Entities;
using ConstructMS.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ConstructMS.Controllers;

[ApiController]
[Route("[controller]")]
public class ConstructionController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    public ConstructionController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost("SubmitRequest")]
    public async Task<APIResponse> SubmitRequest(ConstructionRequest apiRequest)
    {
        try
        {
            var data = await _unitOfWork.ConstructionRequests.AddAsync(apiRequest);
            return new APIResponse
            {
                code = "200",
                message = "Success"
            };
        }
        catch(Exception ex)
        {
            return new APIResponse
            {
                code = "500",
                message = ex.ToString()
            };
        }
    }

    [HttpPost("UpdateRequestStatus")]
    public async Task<APIResponse> UpdateRequestStatus(ConstructionRequest apiRequest)
    {
        try
        {
            var data = await _unitOfWork.ConstructionRequests.UpdateAsync(apiRequest);
            return new APIResponse
            {
                code = "200",
                message = "Success"
            };
        }
        catch(Exception ex)
        {
            return new APIResponse
            {
                code = "500",
                message = ex.ToString()
            };
        }
    }

    [HttpGet("GetAllRequests")]
    public async Task<IReadOnlyList<ConstructionRequest>> GetAllRequests()
    {
        IReadOnlyList<ConstructionRequest> requests = await _unitOfWork.ConstructionRequests.GetAllAsync();
        return requests;
    }

    [HttpGet("GetRequestByID")]
    public async Task<ConstructionRequest> GetRequestByID(int requestID)
    {
        ConstructionRequest request = await _unitOfWork.ConstructionRequests.GetByIdAsync(requestID);
        return request;
    }
}
