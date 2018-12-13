using AutoMapper;
using FridgeServer._UserIdentity;
using FridgeServer._UserIdentity.Dto;
using FridgeServer.Data;
using FridgeServer.EmailService.SendGrid;
using FridgeServer.Helpers;
using FridgeServer.Models;
using FridgeServer.Models._UserIdentity;
using FridgeServer.Models.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FridgeServer.Services
{
    // TODO : Encrypt Secret invetaion and set a time limit
    public interface IUserService
    {
        #region User Creation and Delete
        Task<UserDto> Create(UserDto registrationUser, string password, string host = "", string urlpath = "", bool sendEmail= true, string role = "", bool confirmEmail = false);
        Task<ApplicationUser> Update(UserDto userParam, bool requirePassword = true);
        Task<ApplicationUser> Update_Db(ApplicationUser userParam);
        Task<UserDto> Login(LoginUserDto loginUserDto);
        Task<ApplicationUser> ChangePassword(string userid, string oldpassword, string newpassword);
        Task<bool> Delete(string userid, string password);
        #endregion

        #region Friends Managing
        Task<UserFriend> AddFreindToMe(FriendRequestDto friendRequestDto);
        Task<bool> DeleteFriendship(FriendRequestDto friendRequestDto);
        Task<bool> DeleteFriendsToEachothers(string idA, string idB);
        Task<string> GenerateUserInvitaionCodeAsync(string id);
        bool IdInFriends(List<UserFriend> userfriends, string friendId);
        Task<bool> AreFriendsAsync(string userId, string GevenId);
        Task<List<UserFriend>> GetFriends(string userId);
        #endregion

        #region Email Verfication
        Task<SendEmailResponse> sendEmailVerfication(string userId, string host = "", string urlpath="", bool force = false);
        Task<IdentityResult> VerifyEmailAsync(string userId, string emailToken);
        #endregion

        #region Helper Methods
        Task<ApplicationUser> GetById_Db(string id);
        Task<ApplicationUser> GetById_Manager(string Id);
        Task<UserDto> GetById_IncludeRole(string Id);
        Task<bool> IsUserNameExists(string username);
        Task<bool> IsUserameUnique(string username);
        Task<string> GenerateUserToken(string Id, int ExpiresInDays = 90);
        Task<string> GenerateUserToken(ApplicationUser User, int ExpiresInDays = 90);
        Task<string> UserIdOrFriendIs(string userId, string GevenId);
        #endregion

        #region Admin Methods
        IQueryable<ApplicationUser> GetAllUsing_Manager();
        Task<List<ApplicationUser>> GetAllUsing_Db();
        #endregion
    }
    public class UserService : IUserService
    {
        #region Protected Members
        private AppDbContext db;
        private AppSettings appSettings;
        protected IUserIdentityManager identityManager;
        private IMapper mapper;
        #endregion

        #region Constructor
        public UserService(IOptions<AppSettings> options,
            AppDbContext _db,
            IUserIdentityManager _identityManager,
            IMapper _mapper
            )
        {
            appSettings = options.Value;
            db = _db;
            identityManager = _identityManager;
            mapper = _mapper;
        }
        #endregion

        #region User Creation and Delete
        public async Task<UserDto> Create(UserDto registrationUser, string password,string host="",string urlpath="", bool sendEmail = true, string role = "", bool confirmEmail = false)
        {
            try
            {
                var apphost = string.IsNullOrWhiteSpace(host) ? appSettings.apphost : host;
                var _urlpath = string.IsNullOrWhiteSpace(urlpath) ? appSettings.appVerPath : urlpath; ;

                ApplicationUser user = await identityManager.RegisterAsync(registrationUser, apphost, _urlpath, sendEmail,role,confirmEmail);
                user.secretId = CreateSecret(user.Id);
                user.userFriends = new List<UserFriend>();
                var UpdateUser = await Update_Db(user);

                var UpdatedUser = await GetById_IncludeRole(UpdateUser.Id);
                UpdatedUser.token = await GenerateUserToken(UpdateUser);
                return UpdatedUser;
            }
            catch (AppException ex)
            {
                throw ex;
            }
        }

        public async Task<ApplicationUser> Update(UserDto userParam, bool requirePassword = true)
        {
            var user = await identityManager.UpdateInfoAsync(userParam, requirePassword);
            return user;
        }
        public async Task<ApplicationUser> Update_Db(ApplicationUser userParam)
        {
            var user =  db.Users.Update(userParam);
            await db.SaveChangesAsync();
            return user.Entity;
        }
        public async Task<UserDto> Login(LoginUserDto loginUserDto)
        {
            var userIDTO = await identityManager.LogInAsync(loginUserDto);
            UserDto userDto;
            try
            {

                userDto = mapper.Map<UserDto>(userIDTO);
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                throw ex;
            }
            return userDto;
        }
        public async Task<ApplicationUser> ChangePassword(string userid,string oldpassword,string newpassword)
        {
            var user = await identityManager.UpdatePasswordAsync(userid, oldpassword, newpassword);
            return user;
        }
        public async Task<bool> Delete(string userid, string password)
        {
            var user = await identityManager.DeleteUserAsync(userid, password);
            return user;
        }
        #endregion

        #region Friends Managing
        public async Task<UserFriend> AddFreindToMe(FriendRequestDto friendRequestDto)
        {
            var invetationCode = friendRequestDto.invetationCode;

            //Do logic to conver invetation code to code, for now no logic
            //CHANGE LATER
            var SecretId = invetationCode;//EncryptionHelper.Decrypt(code);
            var friend = await db.Users.Include("userFriends").FirstOrDefaultAsync(x => x.secretId == SecretId);
            if (friend == null)
                throw new AppException("User not found");
            if (friend.Id == friendRequestDto.userId)
                throw new AppException("Not Valid");

            //if friend already added you
            if( friend.userFriends.Any(x=>x.friendUserId==friendRequestDto.userId) )
                throw new AppException("Friend already added you");

            var objectFriends =await AddFriendsToEachothers(friend.Id,friendRequestDto.userId);
            return objectFriends;
        }
        public async Task<UserFriend> AddFriendsToEachothers(string idA, string idB)
        {
            var userA = await db.Users.Include("userFriends").FirstOrDefaultAsync(x => x.Id == idA);
            var userB = await db.Users.Include("userFriends").FirstOrDefaultAsync(x => x.Id == idB);
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
                friendUsername = userA.UserName,
                friendEncryptedCode = codeA,
                friendUserId = userA.Id,
                AreFriends = true
            };


            var UserBFriendObject = new UserFriend()
            {
                friendUsername = userB.UserName,
                friendEncryptedCode = codeB,
                friendUserId = userB.Id,
                AreFriends = true
            };

            userA.userFriends.Add(UserBFriendObject);
            var entityA=db.Users.Update(userA);

            userB.userFriends.Add(UserAFriendObject);
            var entityB=db.Users.Update(userB);

            await db.SaveChangesAsync();

            return UserBFriendObject;
        }
        public async Task<bool> DeleteFriendship(FriendRequestDto friendRequestDto)
        {
            var invetationCode = friendRequestDto.invetationCode;

            //Do logic to conver invetation code to code, for now no logic
            //CHANGE LATER
            var SecretId = invetationCode;//EncryptionHelper.Decrypt(code);
            var friend = await db.Users.Include("userFriends").FirstOrDefaultAsync(x => x.secretId == SecretId);
            if (friend == null)
                throw new AppException("User not found");
            if (friend.Id == friendRequestDto.userId)
                throw new AppException("Not Valid");

            //if friend did not add you
            if ( !friend.userFriends.Any(x => x.friendUserId == friendRequestDto.userId))
                throw new AppException("None Existent");


            var Successful = await DeleteFriendsToEachothers(friend.Id, friendRequestDto.userId);
            return Successful;
        }
        public async Task<bool> DeleteFriendsToEachothers(string idA, string idB)
        {
            var userA = await db.Users.Include("userFriends").FirstOrDefaultAsync(x => x.Id == idA);
            var userB = await db.Users.Include("userFriends").FirstOrDefaultAsync(x => x.Id == idB);
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

            try
            {
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new AppException(ex.Message);
            }

        }
        // TODO: Add secret Encryption
        public async Task<string> GenerateUserInvitaionCodeAsync(string id)
        {
            var user = await GetById_Manager(id);
            var secret = user.secretId;
            string code = "";
            // EncryptionHelper.Encrypt(secret);
            //Do logic to conver invetation code to code, for now no logic
            //CHANGE LATER
            code = secret;
            var invetationCode = secret;
            return invetationCode;
        }
        public bool IdInFriends(List<UserFriend> userfriends,string friendId)
        {
            for (int i = 0; i < userfriends.Count; i++)
            {
                var friend = userfriends[i];
                if (friend.friendUserId == friendId)
                {
                    return true;
                }
            }
            return false;
        }
        public async Task<bool> AreFriendsAsync(string userId, string GevenId)
        {
            var user = await GetById_Db(userId);
            return IdInFriends(user.userFriends,GevenId);
        }
        public async Task<List<UserFriend>> GetFriends(string userId)
        {
            var friends = db.userFriends.Where(f => f.ApplicationUserId == userId).Select(
                f=>new UserFriend()
                    {
                        id=f.id,
                        ApplicationUserId=f.ApplicationUserId,
                        AreFriends=f.AreFriends,
                       friendEncryptedCode=f.friendEncryptedCode,
                       friendUserId=f.friendUserId,
                       friendUsername=f.friendUsername
                    }
                );
            if (friends == null)
            {
                return new List<UserFriend>();
            }
            return await friends.ToListAsync();
        }
        public async Task<string> UserIdOrFriendIs(string userId, string GevenId)
        {
            if (string.IsNullOrWhiteSpace(userId) ||string.IsNullOrWhiteSpace(GevenId) )
            {
                return null;
            }
            if (userId== GevenId)
            {
                return userId;
            }
            var areFriends = await AreFriendsAsync(userId, GevenId);
            if (areFriends)
            {
                return GevenId;
            }
            return null;
        }
        #endregion

        #region Helper Methods
        public async Task<ApplicationUser> GetById_Db(string id)
        {
            return await db.Users.Include("userFriends").FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<ApplicationUser> GetById_Manager(string Id)
        {
            return await identityManager.GetUserById(Id);
        }
        public async Task<UserDto> GetById_IncludeRole(string Id)
        {
            try
            {
                var _uesrDro= await identityManager.GetUserById_includeRoleAsync(Id);
                var userDto = mapper.Map<UserDto>(_uesrDro);
                userDto.userFriends = await GetFriends( userDto.Id);
                return userDto;
            }
            catch (AppException ex)
            {
                throw ex;
            }
        }
        public IQueryable<ApplicationUser> GetAllUsing_Manager()
        {
            return identityManager.GetAllUsersAsync();
        }
        public async Task< List<ApplicationUser> > GetAllUsing_Db()
        {
            return await db.Users
                .Include("userFriends")
                .Include("userGroceries")
                .Include("userGroceries.moreInformations")
                .ToListAsync();
        }

        public async Task<bool> IsUserNameExists(string username)
        {
            return !( await identityManager.IsUserameUnique(username) );
        }
        public async Task<bool> IsUserameUnique(string username)
        {
            return await identityManager.IsUserameUnique(username);
        }

        public async Task<string> GenerateUserToken(string Id,int ExpiresInDays=90)
        {
            return await identityManager.GenerateTokenAsync(Id, ExpiresInDays);
        }
        public async Task<string> GenerateUserToken(ApplicationUser User, int ExpiresInDays = 90)
        {
            return await identityManager.GenerateTokenAsync(User, ExpiresInDays);
        }

        //TOOD: encrypt the id in the secret
        public string CreateSecret(string Input)
        {
            //Encrypt the Secret here
            var encrypted = Input;
            return encrypted;
        }
        #endregion

        #region Email Verification
        public async Task<IdentityResult> VerifyEmailAsync(string userId, string emailToken)
        {
            IdentityResult identityResult;
            try
            {
                identityResult = await identityManager.VerifyEmailAsync(userId, emailToken);
                return identityResult;
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                throw ex;
            }
        }
        public async Task<SendEmailResponse> sendEmailVerfication(string userId, string host="", string urlpath = "", bool force=false)
        {
            var apphost = string.IsNullOrWhiteSpace(host) ? appSettings.apphost : host;
            var _urlpath = string.IsNullOrWhiteSpace(urlpath) ? appSettings.appVerPath : urlpath;
            try
            {
                return await identityManager.sendEmailVerfication(userId, apphost, _urlpath, force);
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                throw ex;
            }
        }
        #endregion

        #region General Internal Methods
        public bool ValueChanged(string OldValue, string NewValue)
        {
            if (OldValue.ToUpper() != NewValue.ToUpper())
            {
                return true;
            }
            return false;
        }
        public string UpdateInput(string OldValue, string NewValue)
        {
            if (String.IsNullOrWhiteSpace(NewValue))
            {
                return OldValue;
            }
            if (ValueChanged(OldValue, NewValue))
            {
                return NewValue;
            }
            return OldValue;
        }
        #endregion

    }//Class
}