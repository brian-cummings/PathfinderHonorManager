﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.Service.Interfaces;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;

namespace PathfinderHonorManager.Controllers
{
    [ApiController]
    [Route("api/pathfinders/{pathfinderId:guid}/[controller]")]
    [Authorize("ReadPathfinders")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class PathfinderHonorsController : ApiController
    {
        private readonly IPathfinderHonorService _pathfinderHonorService;

        public PathfinderHonorsController(IPathfinderHonorService pathfinderHonorService)
        {
            _pathfinderHonorService = pathfinderHonorService;
        }

        // GET Pathfinders/{id}/Honors
        /// <summary>
        /// Get Pathfinder by Id
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Outgoing.PathfinderHonorDto>>> GetAll(Guid pathfinderId, CancellationToken token)
        {
            var pathfinder = await _pathfinderHonorService.GetAllAsync(pathfinderId, token);

            if (pathfinder == default)
            {
                return NotFound();
            }

            return Ok(pathfinder);
        }

        // GET Pathfinders/{id}/Honors/{honorId}
        /// <summary>
        /// Get PathfinderHonor by Id
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("{honorId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(Guid pathfinderId, Guid honorId, CancellationToken token)
        {
            var pathfinder = await _pathfinderHonorService.GetByIdAsync(pathfinderId, honorId, token);

            if (pathfinder == default)
            {
                return NotFound();
            }

            return Ok(pathfinder);
        }

        // POST Pathfinders/{id}/Honors
        /// <summary>
        /// Add a new PathfinderHonor
        /// </summary>
        /// <param name="newPathfinderHonor"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(Outgoing.PathfinderHonorDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostAsync(Guid pathfinderId, [FromBody] Incoming.PostPathfinderHonorDto newPathfinderHonor, CancellationToken token)
        {
            try
            {
                var pathfinderHonor = await _pathfinderHonorService.AddAsync(pathfinderId, newPathfinderHonor, token);

                return CreatedAtRoute(
                    routeValues: GetByIdAsync(pathfinderHonor.PathfinderID, pathfinderHonor.HonorID, token),
                    pathfinderHonor);
            }
            catch (FluentValidation.ValidationException ex)
            {
                UpdateModelState(ex);
                return ValidationProblem(ModelState);
            }
            catch (DbUpdateException ex)
            {
                return ValidationProblem(ex.Message);
            }
        }

        // PUT Pathfinders/{id}/Honors/{honorId}
        /// <summary>
        /// Update a PathfinderHonor
        /// </summary>
        /// <param name="honorId"></param>
        /// <param name="incomingPathfinderHonor"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPut("{honorId:guid}")]
        [ProducesResponseType(typeof(Outgoing.PathfinderHonorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutAsync(Guid pathfinderId, Guid honorId, [FromBody] Incoming.PutPathfinderHonorDto incomingPathfinderHonor, CancellationToken token)
        {
            try
            {
                var pathfinderHonor = await _pathfinderHonorService.UpdateAsync(pathfinderId, honorId, incomingPathfinderHonor, token);

                return pathfinderHonor != default
                ? Ok(pathfinderHonor)
                : NotFound();
            }
            catch (FluentValidation.ValidationException ex)
            {
                UpdateModelState(ex);
                return ValidationProblem(ModelState);
            }
            catch (DbUpdateException ex)
            {
                return ValidationProblem(ex.Message);
            }
        }
    }
}