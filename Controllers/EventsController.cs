using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EventApi.Infrastructure;
using EventApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private EventDbContext db;
        public EventsController(EventDbContext dbContext)
        {
            db = dbContext;
        }

        //GET /api/events
        [HttpGet(Name ="GetAll")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<List<EventInfo>> GetEvents()
        {
            var events = db.Events.ToList();
            return Ok(events); //return with status code 200
        }

        //POST /api/events
        [Authorize]
        [HttpPost(Name = "AddEvent")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<EventInfo>> AddEventAsync([FromBody]EventInfo eventInfo)
        {
            if(ModelState.IsValid)
            { 
            var result= await db.Events.AddAsync(eventInfo);
            await db.SaveChangesAsync();
            return CreatedAtRoute("GetById", new { id = result.Entity.id }, result.Entity);
                //return CreatedAtAction(nameof(GetEvent), new { id=result.Entity.id }, result.Entity); //returns the status code 201
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        //GET /api/events/{id}
        [HttpGet("{id}", Name = "GetById")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<EventInfo>> GetEventAsync([FromRoute] int id)
        {
            var eventInfo = await db.Events.FindAsync(id);
            if (eventInfo != null)
                return eventInfo;
            else
                return NotFound("Item you are searching not found");
        }
    }
}