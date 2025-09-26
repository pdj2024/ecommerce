using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop;
using Models;
using Radzen;
using Services;
using System;
using System.Security.Claims;
using System.Text.Json;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PacificWeb.Controllers
{
    public partial class LoginController : Controller
    {


        public HttpContext? HttpContext => _httpContextAccessor.HttpContext;

        private readonly IJSRuntime _jsRuntime;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthenticationStateProvider _authProvider;


        [Inject]
        private NavigationManager _navigationManager { get; set; }


        public LoginController(NavigationManager navigationManager, IJSRuntime jsRuntime, AuthenticationStateProvider authProvider, IHttpContextAccessor httpContextAccessor)
        {
            _navigationManager = navigationManager;
            _jsRuntime = jsRuntime;
            _authProvider = authProvider;
            _httpContextAccessor = httpContextAccessor;

        }



        [HttpPost("/account/login")]
        public async Task<IActionResult> Login([FromBody] DataUser Model)
        {
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(Model, new JsonSerializerOptions { WriteIndented = true }));
            Model.Email = "brandonhonanta1@gmail.com";
            Model.Password = "123";
            if (Model.Email == "brandonhonanta1@gmail.com" && Model.Password == "123")
            {
                Dictionary<string, List<string>> masUserMenu = new Dictionary<string, List<string>>();

                masUserMenu.Add(Model.Email, new List<string> { "Akses" });

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, Model.Email),
                    new Claim(ClaimTypes.Name, Model.No_Id),
                };

                var thisUsersAccountPolicies = masUserMenu[Model.Email];

                foreach (var userAccountPolicy in thisUsersAccountPolicies)
                {
                    claims.Add(new Claim(userAccountPolicy, "true"));
                }


                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext?.SignInAsync(principal);

                return Ok(new { Status = true });
            }

            return Unauthorized(new { Status = false });
        }

    }
}
