using GoblinWars.Domain.Impl.Models.Races;
using GoblinWars.Domain.Interfaces.Factory;
using GoblinWars.Domain.Interfaces.Models;
using GoblinWars.Domain.Interfaces.Services;
using GoblinWars.DTO.Domain;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goblin.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OriginalController: ControllerBase
    {
        private IGoblinService _goblinService;
        private IGoblinDomainFactory _goblinFactory;
        private IRaceDomainFactory _raceFactory;

        public OriginalController(IGoblinService goblinService, IGoblinDomainFactory goblinFactory, IRaceDomainFactory raceFactory)
        {
            _goblinService = goblinService;
            _goblinFactory = goblinFactory;
            _raceFactory = raceFactory;
        }

        [HttpGet("generate-originals")]
        public ActionResult<StatusResult> GenerateOriginals()
        {
            try
            {
                _goblinService.GenerateOriginal();
                return new StatusResult();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
