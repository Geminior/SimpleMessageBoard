namespace SimpleMessageBoard.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;

    public static class ControllerExtensions
    {
        public static int GetUserId(this ControllerBase c)
        {
            var u = c.User;
            if (u == null)
            {
                return -1;
            }

            var id = u.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (id == null || !int.TryParse(id, out var userId))
            {
                return -1;
            }

            return userId;
        }
    }
}
