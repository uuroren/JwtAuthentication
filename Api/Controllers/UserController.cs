using JwtAuthentication.Api.Extensions;
using JwtAuthentication.Authenticators;
using JwtAuthentication.Catalog.User;
using JwtAuthentication.Common;
using JwtAuthentication.Models.Login;
using JwtAuthentication.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
 
using System.Security.Claims;
using System.Text;

namespace JwtAuthentication.Api.Controllers {
    [Authorize]
    [Route("user")]
    [ApiController]
    public class UserController:ControllerBase {
        private readonly JwtAuthenticationManager _authenticationManager;
        private readonly IRepository<User> _userRepository;

        public UserController(JwtAuthenticationManager authenticationManager,
            IRepository<User> userRepository) {

            _authenticationManager = authenticationManager;
            _userRepository = userRepository;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthenticationModel model) {

            User user = new User() {
                Phone = model.Phone,
                HashPassword = HashingExtensions.Hashing(model.Password),
                Password = model.Password,
                Email = model.Email,
                AccessToken = "",
                Roles = new List<string>() { "User" },
                SmsCode = new Random().Next(111111,999999).ToString(),
                TokenExpiry = DateTime.Now,
                CreatedAt = DateTime.Now,
            };

            await _userRepository.CreateAsync(user);

            return Ok(new { StatusCode = 200,Message = "User Created Success!" });
        }

        [AllowAnonymous]
        [HttpPost("login")]

        public async Task<IActionResult> Login([FromBody] AuthenticationModel model) {
            User user = null;

            user = await _userRepository.GetAsync(o => o.Email == model.Email && o.Password == model.Password || o.Phone == model.Phone && o.Password == model.Password);

            if(user == null) {
                return BadRequest(new { Message = "User not existing" });
            }

            TimeSpan diff = DateTime.Now - user.LastSmsSent;
            if(diff.TotalMinutes < 3)
                return Ok(new { Message = "You need to wait 3 minutes before requesting another sms" });

            user.SmsCode = new Random().Next(111111,999999).ToString();

            user.LastSmsSent = DateTime.Now;

            await _userRepository.UpdateAsync(user);

            return Ok(new { Message = "Sms Verification Required" });

        }

        [AllowAnonymous]
        [HttpPost("smsVerified")]
        public async Task<IActionResult> SmsVerified([FromBody] LoginModel model) {

            User user = await _userRepository.GetAsync(o => o.Phone == model.Phone
                         && o.SmsCode == model.SmsCode || o.Email == model.Email && o.SmsCode == model.SmsCode);

            if(user == null) {
                return BadRequest(new { Message = "User not existing" });
            }

            user = await _authenticationManager.Authenticate(user);

            await _userRepository.UpdateAsync(user);

            return Ok(new AuthenticationResult {
                Token = user.AccessToken,
                RefreshToken = user.RefreshToken,
                Expiry = user.TokenExpiry,
            });
        }

        [HttpGet("GetUsers")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetUsers() {
            var user = _authenticationManager.GetCurrentUser(HttpContext.User);
            var dbUser = await _userRepository.GetAsync(user.Id);

            if(dbUser == null)
                return NotFound();

            var users = await _userRepository.GetAllAsync();
            return Ok(users);
        }
    }
}
