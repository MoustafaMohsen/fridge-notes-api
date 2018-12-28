using FridgeServer.Data;
using FridgeServer.Helpers;
using FridgeServer.Models;
using Microsoft.EntityFrameworkCore;
using MLiberary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FridgeServer.Services
{
    public interface IGroceriesService
    {
        Task<List<Grocery>> GetGrocery(string userId);
        Task<Grocery> GetGroceryById(int Groceryid, string userId);
        Task<ApplicationUser> AddGrocery(Grocery grocery, string userId);
        Task<bool> CheckGroceryState(int Groceryid, string userId, bool bought);
        bool CheckGroceryState(Grocery grocery, bool bought);
        Task neededGrocery(Grocery grocery, string userId, bool checkfirst);
        Task<string> boughtgrocery(Grocery grocery, string userId, bool checkfirst);
        Task<string> editgrocery(Grocery grocery, string userId);
        Task<string> removegrocery(Grocery grocery, string userId);
        Task<string> deletegrocery(Grocery grocery, string userId);
        string guessgrocery(Grocery grocery);
        Task<bool> GroceryExistsName(string name, string userId);
        bool IsGoodGrocery(Grocery grocery, bool strict = false);

    }
    public class GroceriesService : IGroceriesService
    {
        private const string passedValidation = "passed Validation";

        private AppDbContext db;
        private GuessTimeout guessTimeout;
        private IUserService userService;
        public GroceriesService( AppDbContext _db, GuessTimeout _guessTimeout, IUserService _userService)
        {
            db = _db;
            guessTimeout = _guessTimeout;
            userService = _userService;
        }


        public async Task<List<Grocery>> GetGrocery(string userId)
        {
            var Groceries = (await GetGroceryIncludefriends(userId) ).ToList();//GetUserGroceries(userid);
            ;

            return Groceries;
        }

        //update
        public async Task<IEnumerable<Grocery>> GetGroceryIncludefriends(string userId)
        {
            //what's the diffrence between select and find
            var Fulluser = await GetFullUser(userId);
            var Groceries = Fulluser.userGroceries;

            var friends = Fulluser.userFriends;

            List<string> friendsSecrets=new List<string>();
            for (int i = 0; i < friends.Count; i++)
            {
                //Decript Secrets
                var friend = friends[i];

                //If not friends skip
                if(!friend.AreFriends)
                    continue;
                //
                var friendSecret = friend.friendEncryptedCode;//EncryptionHelper.Decrypt(friend.friendEncryptedCode);
                friendsSecrets.Add(friendSecret);
            }

            //Load groceries 
            var UsersQuerable = db.Users
                .Include("userGroceries")
                .Include("userGroceries.moreInformations")
                ;
            var Users = UsersQuerable.Where(u => CompareSecretWithSecrets(u.secretId, friendsSecrets))//.Include("moreInformations")
                .ToList()
                ;
            var FriendsGroceries = new List<Grocery>();
            for (int i = 0; i < Users.Count(); i++)
            {
                var user = Users[i];

                FriendsGroceries.AddRange(user.userGroceries);
            }

            Groceries.AddRange(FriendsGroceries);

            return Groceries;
        }

        public static bool CompareSecretWithSecrets(string secretId, List<string> secrets)
        {
            for (int i = 0; i < secrets.Count; i++)
            {
                if (secretId == secrets[i])
                    return true;
            }
            return false;
        }

        // DOES NOT WORK
        // TODO:Update regex pattaren
        public static bool ToDo_IsGroceryExcluded(string userId,string excludeids)
        {
            string pattern = @"(\d+)\,";
            var matches = Regex.Matches(excludeids, pattern);

            foreach (Match m in matches)
            {
                var matchedId = Regex.Match(m.ToString(), @"(\d+)").ToString();
                if(matchedId == userId)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<Grocery> GetGroceryById(int Groceryid,string userId )
        {
            var grocery = await GetSpecificGroceryForUser(Groceryid, userId);

            if (grocery == null)
            {
                throw new AppException( "Not Found" );
            }

            return grocery;
        }

        public async Task<bool> CheckGroceryState(int Groceryid, string userId,bool bought)
        {
            var grocery = await GetSpecificGroceryForUser(Groceryid, userId);

            if (grocery == null)
            {
                throw new AppException("Not Found");
            }
            var results = grocery.groceryOrBought == bought;
            return results;
        }

        public bool CheckGroceryState(Grocery grocery, bool bought)
        {
            if (grocery == null)
            {
                throw new AppException("Not Found");
            }
            var results = grocery.groceryOrBought == bought;
            return results;
        }
        public bool IsGoodGrocery(Grocery grocery,bool strict = false)
        {
            var IsGood = true;
            if (M.isNull(grocery))
            {
                return !IsGood;
            }
            IsGood = !(
                string.IsNullOrWhiteSpace(grocery.name) ||
                string.IsNullOrWhiteSpace(grocery.ownerid) ||
                string.IsNullOrWhiteSpace(grocery.owner)
                )
                ;
            if (strict)
            {
                IsGood = !(
                    string.IsNullOrWhiteSpace(grocery.name) ||
                    string.IsNullOrWhiteSpace(grocery.ownerid) ||
                    string.IsNullOrWhiteSpace(grocery.owner) ||
                    M.isNullOr0(grocery.id)
                    )
                    ;
            }
            else
            {
                IsGood = !(
                    string.IsNullOrWhiteSpace(grocery.name) ||
                    string.IsNullOrWhiteSpace(grocery.ownerid) ||
                    string.IsNullOrWhiteSpace(grocery.owner)
                    )
                    ;
            }
            return IsGood;
        }
        public async Task<ApplicationUser> AddGrocery(Grocery grocery,string userId)
        {
            if ( !IsGoodGrocery(grocery) )
            {
                throw new AppException("Bad Item");
            }
            //--------------add logic-------------//
            //Validation
            if (await GroceryExistsName(grocery.name, userId)) { throw new AppException("Grocery Name Already Exists, Try new one");  }//validate

            grocery.moreInformations = UpdateInformationsAdd(grocery.moreInformations);

            var user = await GetFullUser(userId);
            grocery.owner = user.FirstName;
            grocery.ownerid = user.Id;
            //If userGroceries is empty initial it
            if (M.isNull(user.userGroceries))
            {
                user.userGroceries = new List<Grocery>();
            }
            user.userGroceries.Add(grocery);
            try
            {
                var userEntity = db.Users.Update(user);
                await db.SaveChangesAsync();
                return userEntity.Entity;
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                throw new AppException("Database Error");
            }
        }

        public async Task neededGrocery(Grocery grocery, string userId,bool checkfirst)
        {

            var editgrocery = await GetSpecificGroceryForUser(grocery.id, userId);

            if (editgrocery == null) { throw new AppException("No grocery was found"); }//validate
            // check state first
            if (checkfirst)
            {
                bool supposedToBe = grocery.groceryOrBought;
                var results = CheckGroceryState(editgrocery, supposedToBe);
                if (results)
                {
                    throw new AppException("Item Already updated");
                }
            }
            editgrocery.timeout = 0;//override the timeout

            var more = UpdateInformationsList(editgrocery.moreInformations, false);
            editgrocery.moreInformations.Add(more);
            editgrocery.groceryOrBought = false;

            try
            {
                db.Entry(editgrocery).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new AppException("DbUpdateConcurrencyException");
            }
            catch(Exception ex)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                throw new AppException("Database Error");
            }
            return ;
        }

        public async Task<string> boughtgrocery(Grocery grocery, string userId,bool checkfirst)
        {
            var editgrocery = await GetSpecificGroceryForUser(grocery.id, userId);

            if (editgrocery == null) { throw new AppException("Id Not found");  }//validate
            if (checkfirst)
            {
                bool supposedToBe = grocery.groceryOrBought;
                var results = CheckGroceryState(editgrocery, supposedToBe);
                if (results)
                {
                    throw new AppException("Item Already updated");
                }
            }
            editgrocery.timeout = HandleTimeout(editgrocery, grocery.timeout);



            var more = UpdateInformationsList(editgrocery.moreInformations, true);
            editgrocery.moreInformations.Add(more);
            editgrocery.groceryOrBought = true;
            try
            {
                db.Entry(editgrocery).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new AppException("DbUpdateConcurrencyException");
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                throw new AppException("Database Error");
            }
            return "bought";
        }

        public async Task<string> editgrocery(Grocery grocery, string userId)
        {
            var editgrocery = await GetSpecificGroceryForUser(grocery.id, userId);

            if (editgrocery == null) throw new AppException("Not found");
            editgrocery.basic= grocery.basic;
            editgrocery.groceryOrBought= grocery.groceryOrBought;
            editgrocery.name= grocery.name;
            editgrocery.timeout= grocery.timeout;


            try
            {
                db.Entry(editgrocery).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await GroceryExistsId(grocery.id , userId))
                {
                    return "NotFound";
                }
                else
                {
                    throw new AppException("Not found");
                }
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                throw new AppException("Database Error");
            }
            return "edited";
        }

        public async Task<string> removegrocery(Grocery grocery, string userId)
        {

            var removegrocery  = await GetSpecificGroceryForUser(grocery.id, userId);

            if (removegrocery == null)
                throw new AppException("Not found");

            if (removegrocery.moreInformations.Count <= 1)
            {
                return "Item has to be Deleted";
            }
            removegrocery.moreInformations.RemoveAt(grocery.moreInformations.Count - 1);
            removegrocery.groceryOrBought = removegrocery.moreInformations.Last().bought;

            //edit

            try
            {
                db.Entry(removegrocery).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {

                throw new AppException("DbUpdateConcurrencyException");
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                throw new AppException("Database Error");
            }
            return "Ok";
        }

        public async Task<string> deletegrocery(Grocery grocery, string userId)
        {
            var user = await GetFullUser(userId);
            var Deletegrocery = user.userGroceries.FirstOrDefault(g=>g.id==grocery.id);

            if (Deletegrocery == null)
            {
                throw new AppException("Not found");
            }
            user.userGroceries.Remove(Deletegrocery);
            try
            {
                db.Users.Update(user);
                await db.SaveChangesAsync();
                return "Deleted";
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new AppException("DbUpdateConcurrencyException");
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                throw new AppException("Database Error");
            }
        }

        public string guessgrocery(Grocery grocery)
        {
            if (MoreisNullOrEmpty(grocery?.moreInformations))
            {
                throw new AppException("Null Or Empty");
            }
            return "" + HandleTimeout(grocery, 0);
        }

        public async Task<bool> GroceryExistsName(string name,string userId)
        {
            if (string.IsNullOrEmpty(name)) return true;
            return await db.Users.AnyAsync(u => u.Id == userId && u.userGroceries.Any(e => e.name.ToLower() == name.ToLower()));
        }


        //============ Private Methods ===========//
        private async Task<ApplicationUser> GetFullUser(string Id)
        {
            var user = await db.Users
                .Include("userGroceries")
                .Include("userGroceries.moreInformations")
                .Include("userFriends")
                .FirstOrDefaultAsync(u => u.Id == Id);
            return user;
        }

        private async Task<List<Grocery>> GetUserGroceries(string userId)
        {
            var groceries = await db.userGroceries
                .Include("moreInformations")
                .Where(u => u.ApplicationUserId == userId)
                .ToListAsync();
            return groceries;
        }

        private List<UserFriend> GetUserFriends(string userId)
        {
            var friends = db.userFriends
                .Where(u => u.ApplicationUserId == userId)
                .ToList();
            return friends;
        }

        private async Task<Grocery> GetSpecificGroceryForUser(int groceryid, string userId)
        {
            var groceries =(await GetFullUser(userId))
                .userGroceries
                .FirstOrDefault(g=>g.id==groceryid)
            ;
            return groceries;
        }

        private async Task<bool> GroceryExistsId(int Groceryid, string userId)
        {
            return await db.Users.AnyAsync(u => u.Id == userId && u.userGroceries.Any(e => e.id == Groceryid ));
        }

        private long HandleTimeout(Grocery grocery, long? timeout)
        {
            if ((timeout <= 0) || timeout == null)//if no timeout or invalid
            {
                grocery.timeout = guessTimeout.GuessByInformation(grocery.moreInformations);
                return (long)grocery.timeout;
            }
            else
            {
                return (long)timeout;//+ (long)Alarm.DateTimeToUnixTime(DateTime.Now); ;
            }
        }

        private List<MoreInformation> UpdateLifetimeanddate(List<MoreInformation> more, int lastDateBy = 1)
        {
            int HoldNo = (int)more.Last().no;
            string HoldtypeOfNo = more.Last().typeOfNo;
            var now = (long)M.DateTimeToUnixTime(DateTime.Now);
            long HoldLastDate = (long)more[more.Count - lastDateBy].date;

            MoreInformation listUpdate = new MoreInformation()
            {
                bought = more.Last().bought,
                date = now,
                typeOfNo = HoldtypeOfNo,
                no = HoldNo,
                lifeTime = now - HoldLastDate
            };

            return replaceLast(more, listUpdate);
        }

        private List<MoreInformation> replaceLast(List<MoreInformation> more, MoreInformation listUpdate)
        {
            try
            {
                more[more.Count - 1] = listUpdate;
                return more;
            }
            catch (Exception ex) { throw new AppException("Exception:", ex); ; }

        }

        private MoreInformation UpdateInformationsList(List<MoreInformation> more, bool bought)
        {
            long HoldLastDate = M.isNull(more.Last().date) ?
                (long)M.DateTimeToUnixTime(DateTime.Now) //if it Null
                : (long)more.Last().date;//if Date has Value

            int HoldNo = M.isNull(more.Last().no) ?
                1 //if it Null
                : (int)more.Last().no;

            string HoldtypeOfNo = M.isNull(more.Last().typeOfNo) ?
                "" //if it Null
                : more.Last().typeOfNo;

            var now = (long)M.DateTimeToUnixTime(DateTime.Now);

            MoreInformation listUpdate = new MoreInformation()
            {
                bought = bought,
                date = now,
                lifeTime = now - HoldLastDate,
                typeOfNo = HoldtypeOfNo,
                no = HoldNo
            };

            return listUpdate;
        }

        private string validatePost(Grocery grocery)
        {

            //check for basic and timeout errors
            if ((grocery.basic && (grocery.timeout == null || grocery.timeout <= 0))
               || (!grocery.basic && (grocery.timeout != null || grocery.timeout >= 0 || grocery.timeout < 0))
               )
            {
                return "no timeout";
            }

            return passedValidation;
        }

        private List<MoreInformation> UpdateInformationsAdd(List<MoreInformation> more)
        {
            int HoldNo = (int)more.Last().no;
            string HoldtypeOfNo = more.Last().typeOfNo;
            var now = (long)M.DateTimeToUnixTime(DateTime.Now);

            MoreInformation listUpdate = new MoreInformation()
            {
                bought = more.Last().bought,
                date = now,
                typeOfNo = HoldtypeOfNo,
                no = HoldNo,
                lifeTime = 0
            };

            return replaceLast(more, listUpdate);
        }

        private bool MoreisNullOrEmpty(List<MoreInformation> Object)
        {
            if (Object == null)
            {
                return true;
            }
            if (Object.Count == 0)
            {
                return true;
            }

            return false;
        }


    }//Class
}
