using AccVerificationWithTokens.Data;
using AccVerificationWithTokens.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AccVerificationWithTokens.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        public static User users = new User();
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;


        public UserController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }




        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {



            if (_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest("User Already Exists");
            }

            CreatePasswordHash(request.Password,
                out byte[] passwordHash,
                out byte[] passwordSalt);



            users.UserName = request.UserName;
            users.Email = request.Email;
            users.Password = request.Password;
            users.PasswordHash = passwordHash;
            users.PasswordSalt = passwordSalt;
            users.VerificationToken = CreateToken(users);


            _context.Users.Add(users);

            await _context.SaveChangesAsync();
            return Ok("User Succsessful Created");
        }

        [HttpGet("{id}")]

        public async Task<IActionResult> GetByID(int id)
        {
            //First way
            var find = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (find == null)
            {
                return NotFound("404 Not Found");
            }
            return Ok(find);


            //Second way

            /*foreach (var item in _context.Users)
            {
                if (item == null)
                {
                    return NotFound("404");
                }
                else
                {
                    return Ok(item);
                }
            }
            return Ok();*/


        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound("User Not Found! ");
            }
            if (user.Password != request.Password)
            {
                return BadRequest("Password is incorrect");
            }
            if (user.VerifiedAt == null)
            {
                return BadRequest("Not Verified! ");
            }
            return Ok($"Welcome, {user.Email}: ");
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var find = await _context.Users.FirstOrDefaultAsync(a => a.VerificationToken == token);
            if (find == null)
            {
                return BadRequest("invalid Token");
            }

            find.VerifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return Ok("User Verified");
        }






        [HttpPost("Forgot-Password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var find = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (find == null)
            {
                return NotFound("User Not Found");
            }
            find.PasswordResetToken = CreateToken(users);
            find.ResetTokenExpires = DateTime.Now.AddDays(1);
            await _context.SaveChangesAsync();
            return Ok("You Can Reset Password");
        }

        [HttpPost("Reset-Password")]

        public async Task<IActionResult> ResetPassword(ForgotPasswordRequest request)
        {
            var find = await _context.Users.FirstOrDefaultAsync(f => f.PasswordResetToken == request.Token);

            if (find == null || find.ResetTokenExpires < DateTime.Now)
            {
                return BadRequest("It's Invalid Token");
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            find.PasswordHash = passwordHash;
            find.PasswordSalt = passwordSalt;
            find.Password = request.Password;
            find.PasswordResetToken = null;
            find.ResetTokenExpires = DateTime.MinValue;

            await _context.SaveChangesAsync();
            return Ok("Password Reseted");


        }


        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            };
 
        }

        /* private bool VerifyPasswordHash(string password, byte[] passwordHash)
         {
             using (var hmac = new HMACSHA512())
             {
                 var computedHash = hmac
                     .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                 return computedHash.SequenceEqual(passwordHash);
             };

         }*/



        [HttpPut]
        public async Task<ActionResult> Update(RegisterRequest userr)
        {
            var find = await _context.Users.FindAsync(userr.ID);


            find.UserName = userr.UserName;
            find.Email = userr.Email;

            await _context.SaveChangesAsync();
            return Ok(await _context.Users.ToArrayAsync());
        }



        [HttpDelete("{id}")]

        public async Task<ActionResult<User>> Delete(int id)
        {
            var find = await _context.Users.FindAsync(id);

            if (find == null)
            {
                return NotFound("404 Not Found! ");
            }

            _context.Users.Remove(find);
            await _context.SaveChangesAsync();
            return Ok($"{find.Email} Succsessful Delete");
        }



        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name,user.UserName)
                    };
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

            var key = new JwtSecurityToken(_configuration["Jwt:Issuer"],
              _configuration["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: creds);
            var jwt = new JwtSecurityTokenHandler().WriteToken(key);


            return jwt;
        }

        // რენდომ ტოკენი (უბრალოდ ბაიტებში გადაყავს.)
        /* private string CreateRandomToken()
         {
             return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
         }*/











    }
}
