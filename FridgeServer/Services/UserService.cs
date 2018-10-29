using FridgeServer.Data;
using FridgeServer.Helpers;
using FridgeServer.Models;
using FridgeServer.Models.Dto;
using Microsoft.EntityFrameworkCore;
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
        User FindById(int id);
        User Create(User user, string password);
        User Update(User user, string password = null);
        User Delete(int id);
        string GenerateUserToken(int id, int ExpiresInDays);
        bool IsUserNameExistsInDb(string Username);
        object AddFreindToMe(FriendRequestDto friendRequestDto);
        object DeleteFriendship(FriendRequestDto friendRequestDto);
        string GenerateUserInvitaionCode(int id);
        int? IdValidation(int userId, int GevenId);
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
            usr.secretId = RandomString(8);
            usr.userGroceries = new List<Grocery>();

            var AddedEntity = db.Users.Add(usr);
            db.SaveChanges();
            return AddedEntity.Entity;
        }

        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(username))
                throw new AppException("Password or Username is Empty");
            var user = db.Users.Include("userFriends").FirstOrDefault(x => x.username == username);
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
            {   //Don't Allow username Changes (for now)
                throw new AppException("username " + user.username + " AlreadyTaken");

                if (IsUserNameExistsInDb(userParam.username))
                    throw new AppException("username " + user.username + " AlreadyTaken");

            }
            //user.username = userParam.username;
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

        public object AddFreindToMe(FriendRequestDto friendRequestDto)
        {
            var invetationCode = friendRequestDto.invetationCode;

            //Do logic to conver invetation code to code, for now no logic
            //CHANGE LATER
            var SecretId = invetationCode;//EncryptionHelper.Decrypt(code);
            var friend = db.Users.Include("userFriends").FirstOrDefault(x => x.secretId == SecretId);
            if (friend == null)
                throw new AppException("User not found");
            if (friend.id == friendRequestDto.userId)
                throw new AppException("Not Valid");

            //if friend already added you
            if( friend.userFriends.Any(x=>x.friendUserId==friendRequestDto.userId) )
                throw new AppException("Friend already added you");

            var objectFriends =AddFriendsToEachothers(friend.id,friendRequestDto.userId);
            return objectFriends;
        }

        public object AddFriendsToEachothers(int idA,int idB)
        {
            var userA = db.Users.Include("userFriends").FirstOrDefault(x => x.id == idA);
            var userB = db.Users.Include("userFriends").FirstOrDefault(x => x.id == idB);
            if (userA == null)
                throw new AppException("User not found");
            if (userB == null)
                throw new AppException("User not found");

            var codeA = userA.secretId;
            var codeB = userB.secretId;
            // if friend
            if (userA.userFriends.Any(f => f.friendUserId == idB))
            {
                throw new AppException("already added");
            }

            if (userB.userFriends.Any(f => f.friendUserId == idA))
            {
                throw new AppException("already added");
            }


            var UserAFriendObject = new UserFriend()
            {
                friendUsername = userA.username,
                friendEncryptedCode = codeA,
                friendUserId = userA.id,
                AreFriends = true
            };


            var UserBFriendObject = new UserFriend()
            {
                friendUsername = userB.username,
                friendEncryptedCode = codeB,
                friendUserId = userB.id,
                AreFriends = true
            };

            userA.userFriends.Add(UserBFriendObject);
            var entityA=db.Users.Update(userA);

            userB.userFriends.Add(UserAFriendObject);
            var entityB=db.Users.Update(userB);

            db.SaveChanges();

            return new { objectA = entityA.Entity, objectB = entityB.Entity };
        }

        public object DeleteFriendship(FriendRequestDto friendRequestDto)
        {
            var invetationCode = friendRequestDto.invetationCode;

            //Do logic to conver invetation code to code, for now no logic
            //CHANGE LATER
            var SecretId = invetationCode;//EncryptionHelper.Decrypt(code);
            var friend = db.Users.Include("userFriends").FirstOrDefault(x => x.secretId == SecretId);
            if (friend == null)
                throw new AppException("User not found");
            if (friend.id == friendRequestDto.userId)
                throw new AppException("Not Valid");

            //if friend did not add you
            if ( !friend.userFriends.Any(x => x.friendUserId == friendRequestDto.userId))
                throw new AppException("None Existent");


            var objectFriends = DeleteFriendsToEachothers(friend.id, friendRequestDto.userId);
            return objectFriends;
        }
        public object DeleteFriendsToEachothers(int idA, int idB)
        {
            var userA = db.Users.Include("userFriends").FirstOrDefault(x => x.id == idA);
            var userB = db.Users.Include("userFriends").FirstOrDefault(x => x.id == idB);
            if (userA == null)
                throw new AppException("User not found");
            if (userB == null)
                throw new AppException("User not found");

            var codeA = userA.secretId;
            var codeB = userB.secretId;
            // if friend
            var ObjectBinUserA = userA.userFriends.FirstOrDefault(f=>f.friendUserId == idB);
            var ObjectAinUserB = userB.userFriends.FirstOrDefault(f=>f.friendUserId == idA);

            if (ObjectBinUserA == null)
            {
                throw new AppException("not found");
            }

            if (ObjectAinUserB == null)
            {
                throw new AppException("not found");
            }


            userA.userFriends.Remove(ObjectBinUserA);
            var entityA = db.Users.Update(userA);

            userB.userFriends.Remove(ObjectAinUserB);
            var entityB = db.Users.Update(userB);

            db.SaveChanges();

            return new { objectA = entityA.Entity, objectB = entityB.Entity };
        }
        public int? IdValidation(int userId,int GevenId)
        {
            var user = GetById(userId);
            int? returnid = null;
            for (int i = 0; i < user.userFriends.Count; i++)
            {
                var friend = user.userFriends[i];
                if (friend.friendUserId == GevenId)
                {
                    returnid = friend.friendUserId;
                }
            }
            return returnid;
        }
        public string GenerateUserInvitaionCode(int id)
        {
            var secret = db.Users.Find(id).secretId;
            var code = secret;// EncryptionHelper.Encrypt(secret);
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
            return db.Users.Include("userFriends").FirstOrDefault(x => x.id == id);
        }

        public User FindById(int id)
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
