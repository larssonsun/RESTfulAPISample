﻿using System.Net.Mime;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RESTfulAPISample.Api.Resource;
using RESTfulAPISample.Core.DomainModel;
using RESTfulAPISample.Core.Interface;

namespace RESTfulAPISample.Api.Controller
{
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticateService _authService;
        private readonly IMapper _mapper;
        public AuthenticationController(IAuthenticateService authService, IMapper mapper)
        {
            this._authService = authService;
            this._mapper = mapper;
        }
        
        [AllowAnonymous]
        [HttpPost("request-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult RequestToken([FromBody] LoginRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Request");
            }

            var loginRequest = _mapper.Map<LoginRequest>(request);

            string token;
            if (_authService.IsAuthenticated(loginRequest, out token))
            {
                return Ok(token);
            }

            return BadRequest("Invalid Request");
        }
    }
}