using AutoMapper;
using CoreUserIdentity._UserIdentity;
using CoreUserIdentity.Helpers;
using CoreUserIdentity.Models;
using CoreUserIdentity.Models.OAuth;
using FridgeServer.Data;
using FridgeServer.Helpers;
using FridgeServer.Models;
using FridgeServer.Models.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MLiberary;
using MLiberary.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VerficationEmailSender.SendGrid;

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
        Task<FridgeExternalRegisterLoginResults> Facebook_Login_Register(string code);
        Task<FridgeExternalRegisterLoginResults> Google_Login_Register(string code);
        Task<ApplicationUser> ChangePassword(string userid, string oldpassword, string newpassword);
        Task<ApplicationUser> UpdatePasswordForceAsync(string userId, string newPassword, string externalProvider = null);
        Task<bool> CheckExternalProvider(string userid, string externalprovider);
        Task<bool> Delete(string userid, string password);
        Task<ApplicationUser> GetFullUser(string userid);
        Task<bool> DeleteNoPassword(string userid);
        #endregion

        #region Friends Managing
        Task<UserFriend> AddFreindToMe(FriendRequestDto friendRequestDto);
        Task<bool> DeleteFriendship(FriendRequestDto friendRequestDto);
        Task<bool> DeleteFriendsToEachothers(string idA, string idB);
        Task<string> GenerateUserInvitaionCodeAsync(string id,bool force = false);
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
        Task<UserDto> GetUserById_include_roles_externalLogin(string userId);
        Task<UserDto> GetById_IncludeRole(string Id);
        Task<bool> IsUserNameExists(string username);
        Task<bool> IsUserameUnique(string username);
        Task<string> GenerateUserToken(string Id, int ExpiresInDays = 90);
        Task<string> GenerateUserToken(ApplicationUser User, int ExpiresInDays = 90);
        Task<string> UserIdOrFriendId(string userId, string GevenId);
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
        protected IUserIdentityManager<ApplicationUser,AppDbContext> identityManager;
        private IMapper mapper;
        private IExternalUserIdentityManager<ApplicationUser, AppDbContext> externalManager;
        #endregion

        #region Constructor
        public UserService(IOptions<AppSettings> options,
            AppDbContext _db,
            IUserIdentityManager<ApplicationUser, AppDbContext> _identityManager,
            IMapper _mapper,
            IExternalUserIdentityManager<ApplicationUser, AppDbContext> _externalManager
            )
        {
            appSettings = options.Value;
            db = _db;
            identityManager = _identityManager;
            mapper = _mapper;
            externalManager = _externalManager;
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

                var UpdateUser = await RegisterBasicInfo(user);

                var UpdatedUser = await GetById_IncludeRole(UpdateUser.Id);
                UpdatedUser.token = await GenerateUserToken(UpdateUser);
                return UpdatedUser;
            }
            catch (AppException ex)
            {
                throw ex;
            }
        }

        public async Task<UserDto> GetUserById_include_roles_externalLogin(string userId)
        {
            try
            {
                var _uesrDro = await identityManager.GetUserById_Roles_externalLogins(userId);
                if (_uesrDro==null)
                {
                    return null;
                }
                var userDto = mapper.Map<UserDto>(_uesrDro);
                userDto.userFriends = await GetFriends(userDto.Id);
                return userDto;
            }
            catch (AppException ex)
            {
                throw ex;
            }
        }

        public async Task<ApplicationUser> RegisterBasicInfo(ApplicationUser user)
        {
            user.secretId = CreateSecret(user.Id);
            user.userFriends = new List<UserFriend>();
            var UpdateUser = await Update_Db(user);
            return UpdateUser;
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

        public async Task<FridgeExternalRegisterLoginResults> Facebook_Login_Register(string code)
        {
            // Initiate Facebook app credentials
            var client_id = appSettings.facebookapp.client_id;
            var client_secret = appSettings.facebookapp.client_secret;
            var redirect_uri = appSettings.facebookapp.redirect_url;

            // Facebook info flow
            var userData = await externalManager.ExcuteOAuth_CodeFlow_facebook(client_id, client_secret, code, redirect_uri);
            // register or login user
            RegisterLoginResults registerLoginResults = await externalManager.LoginOrRegisterExternal(userData);
            FridgeExternalRegisterLoginResults results = await PackResults(registerLoginResults);
            return results;
        }
        public async Task<FridgeExternalRegisterLoginResults> Google_Login_Register(string code)
        {
            // Initiate Facebook app credentials
            var client_id = appSettings.googleapp.client_id;
            var client_secret = appSettings.googleapp.client_secret;
            var redirect_uri = appSettings.googleapp.redirect_url;

            var userData = await externalManager.ExcuteOAuth_CodeFlow_google(client_id, client_secret, code, redirect_uri);

            RegisterLoginResults registerLoginResults = await externalManager.LoginOrRegisterExternal(userData);
            FridgeExternalRegisterLoginResults results = await PackResults(registerLoginResults);
            return results;
        }
        public async Task<FridgeExternalRegisterLoginResults> PackResults(RegisterLoginResults registerLoginResults)
        {
            var resutlUser = registerLoginResults.User;

            // if operation is register
            if (registerLoginResults.isSuccessful && registerLoginResults.operation == "register")
            {
                // set basic register info
                var appicationUser = await GetById_Db(resutlUser.Id);
                var UpdatedUser = await RegisterBasicInfo(appicationUser);

                var _IdentityUserDto = await identityManager.ApplicationUser_ToUserDto(UpdatedUser);
                // convert User to UserDtor
                var userDto = mapper.Map<UserDto>(_IdentityUserDto);
                userDto.userFriends = appicationUser.userFriends;
                userDto.invitationcode = appicationUser.secretId;
                userDto.externalProvider = (await identityManager.GetExternalLoginAsync(userDto.Id))?.LoginProviderName;
                // pack results
                FridgeExternalRegisterLoginResults fridgeExternalRegister = new FridgeExternalRegisterLoginResults()
                {
                    errors = registerLoginResults.errors,
                    errorsDescription = registerLoginResults.errorsDescription,
                    operation = registerLoginResults.operation,
                    User = userDto
                };
                return fridgeExternalRegister;

            }

            // if operation is login
            if (registerLoginResults.isSuccessful && registerLoginResults.operation == "login")
            {
                var appicationUser = await GetById_Db(resutlUser.Id);
                var _IdentityUserDto = await identityManager.ApplicationUser_ToUserDto(appicationUser);
                // convert User to UserDtor
                var userDto = mapper.Map<UserDto>(_IdentityUserDto);
                userDto.userFriends = appicationUser.userFriends;
                userDto.invitationcode = appicationUser.secretId;
                userDto.externalProvider = (await identityManager.GetExternalLoginAsync(userDto.Id))?.LoginProviderName;
                // pack results
                FridgeExternalRegisterLoginResults fridgeExternalLogin = new FridgeExternalRegisterLoginResults()
                {
                    errors = registerLoginResults.errors,
                    errorsDescription = registerLoginResults.errorsDescription,
                    operation = registerLoginResults.operation,
                    User = userDto
                };
                return fridgeExternalLogin;
            }

            // if there is an error pack it and return the results
            FridgeExternalRegisterLoginResults fridgeExternalError = new FridgeExternalRegisterLoginResults()
            {
                errors = registerLoginResults.errors,
                errorsDescription = registerLoginResults.errorsDescription,
                operation = registerLoginResults.operation,
                User = null
            };
            return fridgeExternalError;
        }

        public async Task<ApplicationUser> ChangePassword(string userid,string oldpassword,string newpassword)
        {
            var user = await identityManager.UpdatePasswordAsync(userid, oldpassword, newpassword);
            return user;
        }

        public async Task<ApplicationUser> UpdatePasswordForceAsync(string userId, string newPassword,string externalProvider=null)
        {
            try
            {
                // has external provider
                if (!string.IsNullOrWhiteSpace(externalProvider))
                {
                    var result = await identityManager.CheckExternalProvider(userId, externalProvider);
                    // if provider mismatch
                    if (!result)
                    {
                        throw new AppException("bad provider");
                    }
                    var user = await identityManager.UpdatePasswordForceAsync(userId,newPassword);
                    return user;
                }
                else
                {
                    var user = await identityManager.UpdatePasswordForceAsync(userId, newPassword);
                    return user;
                }
            }
            catch (CoreUserAppException ex)
            {
                throw new AppException(ex.Message);
            }

        }

        public async Task<bool> CheckExternalProvider(string userid, string externalprovider)
        {
            var restuls = await identityManager.CheckExternalProvider(userid, externalprovider);
            return restuls;
        }

        public async Task<bool> Delete(string userid, string password)
        {
            var user = await identityManager.DeleteUserAsync(userid, password);
            var UserExsists = await db.Users.FirstOrDefaultAsync(u => u.Id == userid);
            if (UserExsists == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> DeleteNoPassword(string userid)
        {
            if (string.IsNullOrWhiteSpace(userid))
            {
                return false;
            }

            // get full user
            ApplicationUser applicationUser =await GetFullUser(userid);
            if (applicationUser == null)
            {
                return false;
            }

            // delete friendthips
            var friends = new List<UserFriend>();
            friends = applicationUser.userFriends.ToList();
            foreach (var friend in friends)
            {

                try
                {
                    var s =await DeleteFriendsToEachothers(applicationUser.Id, friend.friendUserId);
                }
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                        Debugger.Break();
                    throw ex;
                }
            }

            // delete groceries
            foreach (var grocery in applicationUser.userGroceries)
            {
                db.Entry(grocery).State = EntityState.Deleted;
            }

            // delete ExternalLogin other info
            foreach (var info in applicationUser.ExternalLogin.OtherUserInfo)
            {
                db.Entry(info).State = EntityState.Deleted;
            }

            // delete ExternalLogin
            db.Entry(applicationUser.ExternalLogin).State = EntityState.Deleted;

            // delete user
            var user = db.Remove(applicationUser);

            // save changes
            await db.SaveChangesAsync();

            // check the user was deleted
            var UserExsists =await db.Users.FirstOrDefaultAsync(u => u.Id == userid);

            // return the results
            if (UserExsists == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Friends Managing
        public async Task<UserFriend> AddFreindToMe(FriendRequestDto friendRequestDto)
        {
            // Validations 
            if (string.IsNullOrWhiteSpace(friendRequestDto.invetationCode)|| string.IsNullOrWhiteSpace(friendRequestDto.userId))
            {
                throw new AppException("Not Valid");
            }

            var invetationCode = friendRequestDto.invetationCode;
            var results = IsInvitationValid(invetationCode);
            if (!results.isSuccessful)
            {
                throw new AppException(results.errors);
            }
            string friendId = results.userId;
            string userId = friendRequestDto.userId;
            // Validations 
            if (userId.ToLower() == friendId.ToLower())
            {
                throw new AppException("Not Valid");
            }
            var friend = await db.Users.Include("userFriends").FirstOrDefaultAsync(x => x.Id == friendId);
            if (friend == null)
                throw new AppException("User not found");

            //if friend already added you
            if( friend.userFriends.Any(x=>x.friendUserId== userId) )
                throw new AppException("Friend already added you");

            var objectFriends =await AddFriendsToEachothers(friend.Id, userId);
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
            string userId = friendRequestDto.userId;
            string friendId = friendRequestDto.invetationCode;
            // Validations 
            if (string.IsNullOrWhiteSpace(friendId) || string.IsNullOrWhiteSpace(userId) || userId== friendId)
            {
                throw new AppException("Not Valid");
            }

            var friend = await db.Users.Include("userFriends").FirstOrDefaultAsync(x => x.Id == friendId);

            if (friend == null)
                throw new AppException("User not found");

            //if friend did not add you
            //if ( !friend.userFriends.Any(x => x.friendUserId == friendRequestDto.userId))
           //     throw new AppException("None Existent");


            var Successful = await DeleteFriendsToEachothers(userId, friendId);
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
        public async Task<string> GenerateUserInvitaionCodeAsync(string Id,bool force = false)
        {
            //check that the user exsists first
            if (force==false)
            {
                var user = await GetById_Manager(Id);
                if (user==null)
                {
                    throw new AppException("User Wasn't found");
                }
            }
            var invetationCode = CreateInvitationCode(Id); ;
            return invetationCode;
        }

        public InvitationDto StringToInvitation(string str)
        {
            var invitation = JsonConvert.DeserializeObject<InvitationDto>(str);
            return invitation;
        }
        public string InvitationToString(InvitationDto obj)
        {
            var invitation = JsonConvert.SerializeObject(obj);
            return invitation;
        }

        // Decryption
        public InvitationResult IsInvitationValid(string str)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    var results = new InvitationResult()
                    {
                        errors = "code is invalid"
                    };
                    return results;
                }
                string key = appSettings.jwt.SecretKey;
                string decoded = Encode_Decode.Decrypt(str, key);
                var invetation = StringToInvitation(decoded);
                bool invalid = M.isNull(invetation);
                if (invalid)
                {
                    var results = new InvitationResult()
                    {
                        errors = "code is invalid"
                    };
                    return results;
                }
                var now = M.GetUtcInSecound();
                bool IsExpired = invetation.expire < now;
                if (IsExpired)
                {
                    var results = new InvitationResult()
                    {
                        errors = "code is expired"
                    };
                    return results;
                }

                return new InvitationResult()
                {
                    userId = invetation.userId
                };
            }
            catch (Exception ex)
            {
                var results = new InvitationResult()
                {
                    errors = "code is invalid"
                };
                return results;
            }
        }

        // Encrypt
        public string CreateInvitationCode(string userid, int lifeInHours = 48)
        {
            try
            {
                var life = DateTime.UtcNow.AddHours(lifeInHours);
                InvitationDto results = new InvitationDto()
                {
                    userId = userid,
                    expire = M.DateTimeToUnixTime(life)
                };

                var str = InvitationToString(results);
                string key = appSettings.jwt.SecretKey;
                string encoded = Encode_Decode.Encrypt(str, key);
                return encoded;
            }
            catch (Exception ex)
            {
                return null;
            }

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
        public async Task<string> UserIdOrFriendId(string userId, string GevenId)
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

        public async Task<ApplicationUser> GetFullUser(string userid)
        {
            return await db.Users
                .Include(u=>u.ExternalLogin)
                    .ThenInclude(e=>e.OtherUserInfo)
                .Include("userFriends")
                .Include("userGroceries")
                .Include("userGroceries.moreInformations")
                .FirstOrDefaultAsync(u=>u.Id==userid)
                ;
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