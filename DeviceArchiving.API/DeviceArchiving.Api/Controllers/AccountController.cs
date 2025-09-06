using DeviceArchiving.Data.Dto;
using DeviceArchiving.Data.Dto.Users;
using DeviceArchiving.Service;
using DeviceArchiving.Service.AccountServices;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }



        [HttpPost("authenticate")]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<BaseResponse<AuthenticationResponse>>> AuthenticateAsync([FromBody] AuthenticationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(BaseResponse<AuthenticationResponse>.Failure("Invalid request"));

            var response = await _accountService.AuthenticateAsync(request);
            return response.Success ? Ok(response) : Unauthorized(response);
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAsync([FromBody] AuthenticationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(BaseResponse<string>.Failure("Invalid request"));

            if (request.AdminPassword == AppSession.AdminPassword && request.Admin ==AppSession.Admin)
            {
                var response = await _accountService.AddUserAsync(request);
                return response.Success ? Ok(response) : BadRequest(response);
            }
            else
            {
                return BadRequest(BaseResponse<string>.Failure("Invalid"));
            }

        }
    }
}
