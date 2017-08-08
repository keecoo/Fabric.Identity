﻿using System.Net;
using Fabric.Identity.API.Services;
using Fabric.Identity.API.Validation;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Fabric.Identity.API.Management
{
    [Authorize(Policy = "RegistrationThreshold", ActiveAuthenticationSchemes = "Bearer")]
    [ApiVersion("1.0")]
    [Route("api/identityresource")]
    [Route("api/v{version:apiVersion}/identityresource")]
    public class IdentityResourceController : BaseController<IdentityResource>
    {
        private readonly IDocumentDbService _documentDbService;
        
        public IdentityResourceController(IDocumentDbService documentDbService, IdentityResourceValidator validator, ILogger logger) 
            : base(validator, logger)
        {
            _documentDbService = documentDbService;
        }

        // GET api/values/5
        // [HttpGet("{id}", Name = GetIdentityResourceRouteName)]
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var identityResource = _documentDbService.GetDocument<IdentityResource>(id).Result;

            if (identityResource == null)
            {
                return CreateFailureResponse($"The specified client with id: {id} was not found",
                    HttpStatusCode.NotFound);
            }
            return Ok(identityResource);
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody] IdentityResource value)
        {
            return ValidateAndExecute(value, () =>
            {
                var id = value.Name;
                _documentDbService.AddDocument(id, value);
                return CreatedAtAction("Get", new { id }, value);
            });
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] IdentityResource value)
        {
            return ValidateAndExecute(value, () =>
            {
                _documentDbService.UpdateDocument(id, value);
                return NoContent();
            });
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            _documentDbService.DeleteDocument<IdentityResource>(id);

            return NoContent();
        }
    }
}
