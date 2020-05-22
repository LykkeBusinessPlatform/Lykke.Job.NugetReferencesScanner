using System.Net;
using Lykke.NuGetReferencesScanner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;

namespace Lykke.NuGetReferencesScanner.Controllers
{
    [Route("api/[controller]")]
    public class IsAliveController : Controller
    {
        /// <summary>
        /// Checks service is alive
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IsAliveResponse), (int)HttpStatusCode.OK)]
        public IActionResult Get()
        {
            var app = PlatformServices.Default.Application;
            return Ok(
                new IsAliveResponse
                {
                    Name = app.ApplicationName,
                    Version = app.ApplicationVersion,
                });
        }
    }
}
