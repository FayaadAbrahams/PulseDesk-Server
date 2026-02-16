using Microsoft.AspNetCore.Mvc;
using PulseDesk.Models;
using PulseDesk.Models.Enums;


namespace PulseDesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PulseDeskController : ControllerBase
    {
        [HttpGet(Name = "getallusers")]
        public IEnumerable<User> Get()
        {
            return [.. Enumerable.Range(1, 5).Select(static index => new User
            {
                Id = Guid.NewGuid().ToString(),
                RoleName = UserRole.Admin,
                Name = index.ToString(),
                Surname = (index * 1000).ToString(),
                BirthDate = DateTime.Now,
                Password = Guid.NewGuid().ToString(),
                Email = "test-mail@mail.com",
            })];
        }


    }
}
