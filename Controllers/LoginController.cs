using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Security.Claims;
using System.Text.Json;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PacificWeb.Controllers
{
    public partial class LoginController : Controller
    {
        private readonly IWebHostEnvironment environment;

        public LoginController(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        [Inject]
        private NavigationManager navigationManager { get; set; }

        [HttpPost("/account/login")]
        public async Task Login([FromForm] LoginViewModel Model)
        {
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(Model, new JsonSerializerOptions { WriteIndented = true }));

            if (Model.Email == "brandonhonanta1@gmail.com" && Model.Password == "123")
            {
                Dictionary<string, List<string>> masUserMenu = new Dictionary<string, List<string>>();

                masUserMenu.Add(Model.Email, new List<string> { "Akses" });

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, Model.Email),
                };

                var thisUsersAccountPolicies = masUserMenu[Model.Email];

                foreach (var userAccountPolicy in thisUsersAccountPolicies)
                {
                    claims.Add(new Claim(userAccountPolicy, "true"));
                }


                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext?.SignInAsync(principal);

                navigationManager.NavigateTo("/", forceLoad: true);
            }
        }

    }
}
