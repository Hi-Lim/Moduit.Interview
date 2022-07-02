using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moduit.Interview.Common.Responses;
using Moduit.Interview.Service;
using System.Collections;
using System.Collections.Generic;

namespace Moduit.Interview.Api.AspNetCore.Controller
{
    [AllowAnonymous]
    [Route("[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly IModuitIntegrationService moduitIntegrationService;

        public QuestionController(IModuitIntegrationService moduitIntegrationService)
        {
            this.moduitIntegrationService = moduitIntegrationService;
        }

        [HttpGet]
        [Route("one")]
        [ProducesResponseType(typeof(QuestionOneResponse), 200)]
        public IActionResult GetQuestionOne()
        {
            return Ok(moduitIntegrationService.GetQuestionOne());
        }
        
        [HttpGet]
        [Route("two")]
        [ProducesResponseType(typeof(QuestionTwoResponse), 200)]
        public IActionResult GetQuestionTwo()
        {
            return Ok(moduitIntegrationService.GetQuestionTwo());
        }
        
        [HttpGet]
        [Route("three")]
        [ProducesResponseType(typeof(IList<QuestionOneResponse>), 200)]
        public IActionResult GetQuestionThree()
        {
            return Ok(moduitIntegrationService.GetQuestionThree());
        }
    }
}
