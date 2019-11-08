using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityAPI.Helpers;
using IdentityAPI.Infrastructure;
using IdentityAPI.Models;
using IdentityAPI.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace IdentityAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private IdentityDBContext db;
        private IConfiguration config;
        public IdentityController(IdentityDBContext dbContext, IConfiguration configuration) {
            this.db = dbContext;
            this.config = configuration;
        }

        [HttpPost("register", Name = "RegisterUser")]
        [ProducesResponseType((int)HttpStatusCode.Created)] // Now swager can identify the status code as 201
        [ProducesResponseType((int)HttpStatusCode.BadRequest)] // and 400
        public async Task<ActionResult<dynamic>> Register(User user)
        {
            TryValidateModel(user);
            if (ModelState.IsValid)
            {
                user.Status = "Not Verified";
                await db.Users.AddAsync(user);
                await db.SaveChangesAsync();
                await SendVerificationMailAsync(user);
                return Created("", new
                {
                    user.Id, user.Fullname,user.Username,user.Email
                });
            }
            else {
                return BadRequest(ModelState);
            }
        }

        [HttpPost("token", Name ="GetToken")]
        public ActionResult<dynamic> GetToken(LoginModel model)
        {
            TryValidateModel(model);
            if (ModelState.IsValid)
            {
                var user = db.Users.SingleOrDefault(s => s.Username == model.Username && s.Password == model.Password && s.Status== "Verified");
                if (user != null)
                {
                    var token = GenerateToke(user);
                    return Ok(new {user.Fullname, user.Email,user.Username, user.Role, Token = token });
                }
                else {
                    return Unauthorized();
                }
            }
            else {
                return BadRequest(ModelState);
            }
        }

        [NonAction]
        private string GenerateToke(User user)
        {
            var claims = new List<Claim> {
               new Claim(JwtRegisteredClaimNames.Sub, user.Fullname),
               new Claim(JwtRegisteredClaimNames.Email, user.Email),
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
           };

            claims.Add(new Claim(JwtRegisteredClaimNames.Aud, "catalogapi"));
            claims.Add(new Claim(JwtRegisteredClaimNames.Aud, "paymentapi"));
            claims.Add(new Claim(JwtRegisteredClaimNames.Aud, "basketapi"));
            claims.Add(new Claim(JwtRegisteredClaimNames.Aud, "orderapi"));
            claims.Add(new Claim(ClaimTypes.Role, user.Role));
          

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetValue<string>("Jwt:secret")));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: config.GetValue<string>("Jwt:issuer"),
                audience: null, //config.GetValue<string>("Jwt:audience"),
                claims:claims,
                expires:DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
                );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;
        }

        [NonAction]
        private async Task SendVerificationMailAsync(User user)
        {
            var userObj = new {
                user.Id,user.Fullname,user.Username,user.Email
            };

            var messageText = JsonConvert.SerializeObject(userObj);
            StorageAccountHelper storageHelper = new StorageAccountHelper();
            storageHelper.StorageConnectionString = config.GetConnectionString("StorageConnection");
            await storageHelper.SendMessageAsync(messageText, "users");
        }

    }
}