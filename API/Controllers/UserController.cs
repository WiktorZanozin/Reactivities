using System.Net;
using Application.User;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Application.Errors;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [AllowAnonymous]
    public class UserController:BaseController
    {
        [HttpPost("login")]
         public async Task<ActionResult<User>> Login(Login.Query query)
         {
           return await Mediator.Send(query);
         }

         [HttpPost("register")]
         public async Task<ActionResult<User>> Register(Register.Command command)
         {
           return await Mediator.Send(command);
         }

         [HttpGet]
         public async Task<ActionResult<User>> CurrentUser()
         {
           return await Mediator.Send(new CurrentUser.Query());
         }
        
    }
}