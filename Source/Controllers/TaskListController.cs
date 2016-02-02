using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Swashbuckle.Swagger.Annotations;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using TaskAPI.Models;

namespace TaskAPI.Controllers
{
    [Route("api/[controller]")]
    public class TaskListController : Controller
    {
        private readonly TaskContext _context;
        public TaskListController(TaskContext context)
        {
            _context = context;
        }

        // GET: api/tasklist/8ab4fcbd993f49ce8a21103c713bf47a
        [HttpGet("{userId}")]
        public async Task<IEnumerable<TaskList>> GetAll(string userId)
        {
            return await _context.TaskLists.Where(p => p.UserId == userId).ToListAsync();
        }


        // POST api/tasklist
        [HttpPost]
        public async Task<ActionResult> Post([FromBody]CreateTaskListRequest request)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest();
            }

            else
            {
                var userExists = await _context.Users.AnyAsync(i => i.UserId == request.UserId);
                if (userExists)
                {
                    var itemExists = await _context.TaskLists.AnyAsync(i => i.Title == request.TaskListTitle && i.UserId == request.UserId);
                    if (itemExists)
                    {
                        return HttpBadRequest();
                    }
                    TaskList item = new Models.TaskList();
                    item.TaskListId = Guid.NewGuid().ToString().Replace("-", "");
                    item.UserId = request.UserId;
                    item.CreatedOnUtc = DateTime.UtcNow;
                    item.UpdatedOnUtc = DateTime.UtcNow;
                    item.Title = request.TaskListTitle;
                    _context.TaskLists.Add(item);
                    await _context.SaveChangesAsync();
                    Context.Response.StatusCode = 201;
                    return Ok();
                }
                else
                {
                    return HttpBadRequest();
                }
            }
        }

        // DELETE api/tasklist/5ab4fcbd993f49ce8a21103c713bf47a
        [HttpDelete]
        [SwaggerResponse(System.Net.HttpStatusCode.NoContent)]
        public async Task<IActionResult> Delete([FromBody]DeleteTaskListRequest request)
        {
            var item = await _context.TaskLists.FirstOrDefaultAsync(x => x.TaskListId == request.TaskListId 
            && x.UserId == request.UserId && x.IsDeleted != true);
            if (item == null)
            {
                return HttpNotFound();
            }
            item.IsDeleted = true;
            item.UpdatedOnUtc = DateTime.UtcNow;
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return new HttpStatusCodeResult(204); // 201 No Content
        }
    }
}
