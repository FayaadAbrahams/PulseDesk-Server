using Microsoft.AspNetCore.Mvc;

namespace PulseDesk.Controllers
{
    public class BaseController: ControllerBase
    {
        protected int CurrentUserId => int.Parse(User.FindFirst("nameid")!.Value);

        protected string CurrentUserRole => (User.FindFirst("role")!.Value);
        protected string CurrentUserEmail => (User.FindFirst("email")!.Value);

    }
}
