using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using TaskAPI.Models;

namespace TaskAPI.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly TaskContext _context;
        public UserController(TaskContext context)
        {
            _context = context;
        }

        // GET: api/user
        [HttpGet]
        public async Task<IEnumerable<User>> GetAll()
        {
            return await _context.Users.Where(p=>p.IsDeleted != true).ToListAsync();
        }

        // GET api/user/sample@mail.com
        [HttpGet("{email}")]
        public async Task<User> Get(string email)
        {
            var item = await _context.Users.FirstOrDefaultAsync(x => x.EmailAddress == email && x.IsDeleted != true);
            return item;
        }

        // POST api/user
        [HttpPost]
        public async Task<ActionResult> Post([FromBody]CreateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest();
            }

            else
            {
                var itemExists = await _context.Users.AnyAsync(i => i.EmailAddress == request.EmailAddress && i.IsDeleted != true);
                if (itemExists)
                {
                    return HttpBadRequest();
                }
                User item = new Models.User();
                item.UserId = Guid.NewGuid().ToString().Replace("-", "");
                item.CreatedOnUtc = DateTime.UtcNow;
                item.UpdatedOnUtc = DateTime.UtcNow;
                item.EmailAddress = request.EmailAddress;
                _context.Users.Add(item);
                await _context.SaveChangesAsync();
                Context.Response.StatusCode = 201;
                return Ok();
            }
        }

        // DELETE api/user/3ab4fcbd993f49ce8a21103c713bf47a
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody]DeleteUserRequest request)
        {
            var item = await _context.Users.FirstOrDefaultAsync(x => x.UserId == request.UserId && x.IsDeleted != true);
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
