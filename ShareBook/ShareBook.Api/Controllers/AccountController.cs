using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Infra.CrossCutting.Identity;
using ShareBook.Infra.CrossCutting.Identity.Interfaces;
using ShareBook.Service;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowAllHeaders")]
    [GetClaimsFilter]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IApplicationSignInManager _signManager;
        private readonly IMapper _mapper;

        public AccountController(IUserService userService,
                                 IMapper mapper,
                                 IApplicationSignInManager signManager)
        {
            _userService = userService;
            _signManager = signManager;
            _mapper = mapper;
        }

        #region [ GET ]
        [Authorize("Bearer")]
        [HttpGet]
        public UserVM Get()
        {
            var id = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            var user = _userService.Find(id);

            var userVM = _mapper.Map<User, UserVM>(user);
            return userVM;
        }

        [Authorize("Bearer")]
        [HttpGet("Profile")]
        public object Profile()
        {
            var id = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            return new { profile = _userService.Find(id).Profile.ToString() };
        }

        [Authorize("Bearer")]
        [HttpGet("ListFacilitators/{userIdDonator}")]
        public IActionResult ListFacilitators(Guid userIdDonator)
        {
            var facilitators = _userService.GetFacilitators(userIdDonator);
            var facilitatorsClean = _mapper.Map<List<UserFacilitatorVM>>(facilitators);
            return Ok(facilitatorsClean);
        }
        #endregion

        #region [ POST ]
        [HttpPost("Register")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(409)]
        public IActionResult Post([FromBody] RegisterUserVM registerUser, [FromServices] SigningConfigurations signingConfigurations, [FromServices] TokenConfigurations tokenConfigurations)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState
                                        .Values
                                        .SelectMany(x => x.Errors.Select(error => error.ErrorMessage))
                                        .ToList());

            var user = _mapper.Map<RegisterUserVM, User>(registerUser);

            var result = _userService.Insert(user);

            if (result.Success)
                return Ok(_signManager.GenerateTokenAndSetIdentity(result.Value, signingConfigurations, tokenConfigurations));

            return Conflict(result);
        }

        [HttpPost("Login")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        public IActionResult Login([FromBody] LoginUserVM loginUserVM, [FromServices] SigningConfigurations signingConfigurations, [FromServices] TokenConfigurations tokenConfigurations)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState
                                        .Values
                                        .SelectMany(x => x.Errors.Select(error => error.ErrorMessage))
                                        .ToList());

            var user = _mapper.Map<LoginUserVM, User>(loginUserVM);

            var result = _userService.AuthenticationByEmailAndPassword(user);

            if (!result.Success)
                return NotFound(result);

            var response = new Result
            {
                Value = _signManager.GenerateTokenAndSetIdentity(result.Value, signingConfigurations, tokenConfigurations)
            };

            return Ok(response);

        }

        [HttpPost("ForgotMyPassword")]
        [ProducesResponseType(typeof(Result), 200)]
        [ProducesResponseType(404)]
        public IActionResult ForgotMyPassword([FromBody] ForgotMyPasswordVM forgotMyPasswordVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState
                                        .Values
                                        .SelectMany(x => x.Errors.Select(error => error.ErrorMessage))
                                        .ToList());

            var result = _userService.GenerateHashCodePasswordAndSendEmailToUser(forgotMyPasswordVM.Email);

            if (result.Success)
                return Ok(result);

            return NotFound(result);
        }

        #endregion

        #region [ PUT ]
        [Authorize("Bearer")]
        [HttpPut]
        [ProducesResponseType(typeof(Result<User>), 200)]
        [ProducesResponseType(409)]
        public IActionResult Update([FromBody] UpdateUserVM updateUserVM, [FromServices] SigningConfigurations signingConfigurations, [FromServices] TokenConfigurations tokenConfigurations)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState
                                        .Values
                                        .SelectMany(x => x.Errors.Select(error => error.ErrorMessage))
                                        .ToList());

            var user = _mapper.Map<UpdateUserVM, User>(updateUserVM);

            user.Id = new Guid(Thread.CurrentPrincipal?.Identity?.Name);

            var result = _userService.Update(user);

            if (!result.Success)
                return Conflict(result);

            return Ok(_signManager.GenerateTokenAndSetIdentity(result.Value, signingConfigurations, tokenConfigurations));
        }

        [Authorize("Bearer")]
        [HttpPut("ChangePassword")]
        public Result<User> ChangePassword([FromBody] ChangePasswordUserVM changePasswordUserVM)
        {
            var user = new User() { Password = changePasswordUserVM.OldPassword };
            user.Id = new Guid(Thread.CurrentPrincipal?.Identity?.Name);

            return _userService.ValidOldPasswordAndChangeUserPassword(user, changePasswordUserVM.NewPassword);
        }

        [HttpPut("ChangeUserPasswordByHashCode")]
        [ProducesResponseType(typeof(Result<User>), 200)]
        [ProducesResponseType(404)]
        public IActionResult ChangeUserPasswordByHashCode([FromBody] ChangeUserPasswordByHashCodeVM changeUserPasswordByHashCodeVM)
        {
            var result = _userService.ConfirmHashCodePassword(changeUserPasswordByHashCodeVM.HashCodePassword);
            if (!result.Success)
                return NotFound(result);

            var resultChangePasswordUser = _userService.ChangeUserPassword(result.Value as User, changeUserPasswordByHashCodeVM.NewPassword);
            if (!resultChangePasswordUser.Success)
                return BadRequest(resultChangePasswordUser);

            return Ok(resultChangePasswordUser);
        }
        #endregion

        [HttpGet]
        [Route("ForceException")]
        public IActionResult ForceException()
        {
            var teste = 1 / Convert.ToInt32("Teste");
            return BadRequest();
        }
    }
}