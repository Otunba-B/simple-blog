using MarkTest.Authentication;
using MarkTest.Data.Entities;
using MarkTest.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MarkTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        BloggContext _bloggContext;

        public AuthenticationController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, BloggContext bloggContext)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
            _bloggContext = bloggContext;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            var user = await userManager.FindByNameAsync(model.Username);
            if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new
                {
                    userId = user.Id,
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        //[HttpGet]
        //[Route("register")]
        //public IActionResult RegisterViewModel()
        //{
        //    return RegisterViewModel(); 
        //}

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {

            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            return Ok(new Response { Status = "Success", Message = "User Created Successfully!" });

        }

        [HttpPost]
        [Route("add-role")]
        public async Task<IActionResult> AddRole(string role)
        {
            IdentityRole identityRole = new IdentityRole { Name = role };
            var result = await roleManager.CreateAsync(identityRole);

            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            return Ok(new Response { Status = "Success", Message = "Role Created Successfully!" });

        }

        [HttpPost]
        [Route("add-claim")]
        [Authorize]
        public async Task<IActionResult> AddClaim(string role, string[] claims)
        {

            IdentityRole identityRole = await roleManager.FindByNameAsync(role);
            if (identityRole == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Role do not exist." });
            }

            for (int i = 0; i < claims.Length; i++)
            {
                new Claim("Permission", claims[i]);
                IdentityResult result = await roleManager.AddClaimAsync(identityRole, new Claim("Permission", claims[i]));
                if (!result.Succeeded)
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Claim creation failed! Please check the details and try again." });
            }

            return Ok(new Response { Status = "Success", Message = "Claim Created Successfully!" });
        }

        [HttpPost]
        [Route("add-role-to-user")]
        public async Task<IActionResult> RoleAuthenticate(string Username, string role)
        {
            var user = await userManager.FindByNameAsync(Username);
            bool identityRole = await roleManager.RoleExistsAsync(role);
            if (user != null && identityRole)
            {
                var result = await userManager.AddToRoleAsync(user, role);
                if (result.Succeeded)
                    return Ok(new Response { Status = "Success", Message = "User added to role successfully" });
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User exists in role" });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Operation failed! Please check your input and try again." });

            }

        }

        [HttpPost]
        [Route("create-post")]
        //[Authorize]
        public async Task<IActionResult> CreatePost(string Username, PostMatch postMessage)
        {
            var user = await userManager.FindByNameAsync(Username);
            var userRole = await userManager.IsInRoleAsync(user, "admin");
            if (user == null || !userRole)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Check the username, or visit our register page to have access to this feature" });
            }

            else if (user != null && userRole)
            {
                //var roleName = await userManager.GetUsersInRoleAsync("Admin");

                //if (roleName.Contains(user))
                //{

                    Post post = new Post
                    {
                        Author = postMessage.Author,
                        Category = postMessage.Category,
                        Photo = postMessage.Photo,
                        PostDate = postMessage.PostDate,
                        PostTitle = postMessage.PostTitle,
                        PostId = postMessage.PostId,
                        status = postMessage.status,
                        BlogPost = postMessage.BlogPost
                    };
                    _bloggContext.Posts.Add(post);
                    _bloggContext.SaveChanges();
                    return Ok(new Response { Status = "Success", Message = "Post Approved and Created. " });
                //}
               
            }
            else
                return Ok(new Response { Status = "Pending Approval", Message = "An Admin has to approve your post first." });
        }

        [HttpPost]
        [Route("add-comment")]
        //[Authorize]
        public async Task<IActionResult> AddComment (string Username, CommentMatch commentMessage)
        {
            var user = await userManager.FindByNameAsync(Username);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "You need to be a registered member of our community" });
            }
            else if (user != null)
            {
                var comment = new Comment
                {
                    UserComment = commentMessage.UserComment,
                    CommentDate = DateTime.Now,
                    PostId = commentMessage.PostId,

                };
                var postCheck = _bloggContext.Posts.Single(p => p.PostId == comment.PostId);
                if (postCheck != null)
                {
                    _bloggContext.Comments.Add(comment);
                    _bloggContext.SaveChanges();
                    return Ok(new Response { Status = "Success", Message = "Comment Added" });
                }
                else
                    return StatusCode(StatusCodes.Status404NotFound, new Response { Message = "Invalid Post ID. Post does not exist", Status = "Error" });
            }
            else
                return StatusCode(500, new Response { Status = "Error", Message = "Check your input and try again" });
        }

        [HttpPost]
        [Route("like-post")]
        //[Authorize]
        public async Task<IActionResult> LikePost(string Username, LikePostMatch postLike)
        {
            var user = await userManager.FindByNameAsync(Username);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "You need to be a registered member of our community" });
            }
            else
            {
                var like = new Like
                {
                    PostId = postLike.PostId,
                    LikeDate = DateTime.Now,
                };
                try
                {
                    var postCheck = _bloggContext.Posts.Single(p => p.PostId == like.PostId);
                    if (postCheck != null)
                    {
                        _bloggContext.Likes.Add(like);
                        _bloggContext.SaveChanges();
                        return Ok(new Response { Status = "Success", Message = "Post Liked " });
                    }
                }
                catch (Exception)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new Response { Message = "Invalid Post ID. Post does not exist", Status = "Error" });
                }
            }
            return StatusCode(500, new Response { Status = "Error", Message = "Check your input and try again" });
        }

        [HttpPost]
        [Route("like-comment")]
        //[Authorize]
        public async Task<IActionResult> LikeComment(string Username, LikeCommentMatch likeComment)
        {
            var user = await userManager.FindByNameAsync(Username);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "You need to be a registered member of our community" });
            }
            else
            {
                var like = new Like
                {
                    //PostId = likeComment.PostId,
                    LikeDate = DateTime.Now,
                    CommentId = likeComment.CommentId
                };
                try
                {
                    var commentCheck = _bloggContext.Comments.Single(p => p.CommentId == like.CommentId);
                    if (commentCheck != null)
                    {
                        _bloggContext.Likes.Add(like);
                        _bloggContext.SaveChanges();
                        return Ok(new Response { Status = "Success", Message = "Comment Liked " });
                    }
                }
                catch (Exception)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new Response { Message = "Invalid Comment ID. Comment does not exist", Status = "Error" });
                }
            }
            return StatusCode(500, new Response { Status = "Error", Message = "Check your input and try again" });
        }
    }
}
