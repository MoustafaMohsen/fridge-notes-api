using FridgeServer.Data;
using FridgeServer.Helpers;
using FridgeServer.Models;
using FridgeServer.Models.Dto;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FridgeServer.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User GetById(int id);
        User Create(User user, string password);
        User Update(User user, string password = null);
        User Delete(int id);
        string GenerateUserToken(int id, int ExpiresInDays);
        bool IsUserNameExistsInDb(string Username);
        UserFriend AddFreindToMe(FriendRequestDto friendRequestDto);
        string GenerateUserInvitaionCode(int id);
    }
    public class UserService : IUserService
    {
        private AppDbContext db;
        private AppSettings appSettings;
        public UserService(IOptions<AppSettings> options, AppDbContext _db)
        {
            appSettings = options.Value;
            db = _db;
        }
        public User Create(User usr, string password)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(usr.username))
                throw new AppException("Password or Username is Empty");
            if (IsUserNameExistsInDb(usr.username))
                throw new AppException("Username Already Exists, Try new one");


            byte[] PasswordHash, PasswordSalt;
            CreatePasswordHash(password, out PasswordHash, out PasswordSalt);
            usr.passwordHash = PasswordHash;
            usr.passwordSalt = PasswordSalt;
            usr.SecretId = RandomString(8);

            var AddedEntity = db.Users.Add(usr);
            db.SaveChanges();
            return AddedEntity.Entity;
        }

        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(username))
                throw new AppException("Password or Username is Empty");
            var user = db.Users.FirstOrDefault(x => x.username == username);
            if (user == null)
                return null;
            if (VerifyPasswordHash(password, user.passwordHash, user.passwordSalt) == false)
                return null;
            return user;
        }

        public User Update(User userParam, string password = null)
        {
            var user = db.Users.Find(userParam.id);

            //If username has Changed
            if (user.username != userParam.username)
            {
                if (IsUserNameExistsInDb(userParam.username))
                    throw new AppException("username " + user.username + " AlreadyTaken");
            }
            user.username = userParam.username;
            user.firstname = userParam.firstname;
            user.lastname = userParam.lastname;
            //If password was entred
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] PasswordSalt, PasswordHash;
                CreatePasswordHash(password, out PasswordHash, out PasswordSalt);
                user.passwordHash = PasswordHash;
                user.passwordSalt = PasswordSalt;
            }
            var Updateentity = db.Users.Update(user);
            db.SaveChanges();
            return Updateentity.Entity;
        }

        public User Delete(int userid)
        {
            var user = db.Users.Find(userid);
            if (user == null)
                throw new AppException("User was not found");
            var DeleteEntitr = db.Users.Remove(user);
            db.SaveChanges();
            return DeleteEntitr.Entity;

        }

        public UserFriend AddFreindToMe(FriendRequestDto friendRequestDto)
        {
            var invetationCode = friendRequestDto.invetationCode;
            //Do logic to conver invetation code to code, for now no logic
            //CHANGE LATER
            var code = invetationCode;
            var SecretId = EncryptionHelper.Decrypt(code);
            var user = db.Users.FirstOrDefault(x => x.SecretId == SecretId);
            if (user == null)
                throw new AppException("Uesr not found");
            var arefreinds = user.friends.Any(x => x.id == friendRequestDto.userId);
            var friend = new UserFriend()
            {
                friendUsername = user.username,
                friendEncryptedCode = code,
                friendUserId = user.id,
                AreFriends = arefreinds
            };
            AddFriend(friend, user.id);
            return friend;
        }

        public User AddFriend(UserFriend friend, int friendId)
        {
            var user = db.Users.Find(friendId);
            if (user == null)
                throw new AppException("User not found");

            user.friends.Add(friend);
            var entity = db.Users.Update(user);
            db.SaveChanges();
            return entity.Entity;
        }

        public string GenerateUserInvitaionCode(int id)
        {
            var secret = db.Users.Find(id).SecretId;
            var code = EncryptionHelper.Encrypt(secret);
            //Do logic to conver invetation code to code, for now no logic
            //CHANGE LATER
            var invetationCode = code;
            return invetationCode;
        }
        //Get all users
        public IEnumerable<User> GetAll()
        {
            return db.Users;
        }

        public User GetById(int id)
        {
            return db.Users.Find(id);
        }

        public bool IsUserNameExistsInDb(string username)
        {
            var user = db.Users.FirstOrDefault(x => x.username == username);
            if (user == null)
                return false;
            return true;
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
        //Helper Methods
        public string GenerateUserToken(int id,int ExpiresInDays)
        {
            var Key = Encoding.ASCII.GetBytes( appSettings.Secret);

            var TokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, id.ToString()), new Claim(ClaimTypes.NameIdentifier, id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(ExpiresInDays),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Key) ,SecurityAlgorithms.HmacSha256Signature)
                
            };

            var TokenHandler = new JwtSecurityTokenHandler();

            var Token = TokenHandler.CreateToken(TokenDescriptor);
            var TokenStr = TokenHandler.WriteToken(Token);

            return TokenStr;
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }//Class
}
