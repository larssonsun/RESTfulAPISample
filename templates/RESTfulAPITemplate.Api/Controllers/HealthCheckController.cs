﻿using Microsoft.AspNetCore.Mvc;

namespace RESTfulAPITemplate.Api.Controller
{
    [Route("[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        /// <summary>
        /// 健康检查接口
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get() => Ok();
    }
}