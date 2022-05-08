using Microsoft.AspNetCore.Mvc.Rendering;
using RomDiscord.Models;
using System.Text.Json;

namespace RomDiscord.Util
{
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }


        public static SessionData SessionData(this HttpContext context)
		{
            var value = context.Session.Get<SessionData>("Data");
            if (value == null)
                return new SessionData();
            return value;
		}

        public static string ActiveClass(this IHtmlHelper htmlHelper, string? controllers = null, string? actions = null, string cssClass = "active", string inActiveClass = "text-white")
        {
            var currentController = htmlHelper?.ViewContext.RouteData.Values["controller"] as string;
            var currentAction = htmlHelper?.ViewContext.RouteData.Values["action"] as string;

            var acceptedControllers = (controllers ?? currentController ?? "").Split(',');
            var acceptedActions = (actions ?? currentAction ?? "").Split(',');

            return acceptedControllers.Contains(currentController) && acceptedActions.Contains(currentAction)
                ? cssClass
                : inActiveClass;
        }
    }
}
