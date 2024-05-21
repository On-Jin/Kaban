// using System.Security.Claims;
// using Kaban.Data;
// using Kaban.Models;
// using Microsoft.AspNetCore.Authentication;
//
// namespace Kaban;
//
// public class CustomAuthenticationMiddleware : IMiddleware
// {
//     public async Task InvokeAsync(HttpContext context, RequestDelegate next)
//     {
//         Console.WriteLine($"CustomAuthenticationMiddleware {context.Request.Path}");
//         
//         foreach (var claimsIdentity in context.User.Identities)
//         {
//             Console.WriteLine($"Identity : {claimsIdentity.Name}");
//         }
//         if (context.User.Identity.IsAuthenticated)
//         {
//             // Access the claims
//             var claims = context.User.Claims;
//                 
//             // Perform actions based on claims
//             foreach (var claim in claims)
//             {
//                 Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
//                 // Perform your logic here based on the claims
//             }
//         }
//         else
//         {
//             Console.WriteLine("New shadow user");
//             var db = context.RequestServices.GetRequiredService<AppDbContext>();
//
//             User user = new User();
//             db.Users.Add(user);
//             db.SaveChanges();
//
//             var ci = new ClaimsPrincipal(new[]
//             {
//                 new ClaimsIdentity(new List<Claim>()
//                     {
//                         new Claim(ClaimTypes.Name, user.Id.ToString())
//                     },
//                     CookieAuthenticationDefaults.AuthenticationScheme)
//             });
//             var authProperties = new AuthenticationProperties
//             {
//                 IsPersistent = true,
//             };
//             await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
//                 new ClaimsPrincipal(ci),
//                 authProperties);
//             Console.WriteLine("SignIn !");
//         }
//         await next.Invoke(context);
//     }
// }