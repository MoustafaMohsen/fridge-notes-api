using FridgeServer.Data;
using FridgeServer.Helpers;
using FridgeServer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FridgeServer.Services
{
    public interface IGroceriesService
    {
        List<Grocery> GetGrocery(int id);
        Grocery GetGroceryById(int Groceryid, int userid);
        User AddGrocery(Grocery grocery, int id);
        void neededGrocery(Grocery grocery, int id);
        string boughtgrocery(Grocery grocery, int id);
        string editgrocery(Grocery grocery, int id);
        string removegrocery(Grocery grocery, int id);
        string deletegrocery(Grocery grocery, int id);
        string guessgrocery(Grocery grocery);
        bool GroceryExistsName(string name, int id);

    }
    public class GroceriesService : IGroceriesService
    {
        private const string passedValidation = "passed Validation";

        private AppDbContext db;
        private GuessTimeout guessTimeout;
        public GroceriesService( AppDbContext _db, GuessTimeout _guessTimeout)
        {
            db = _db;
            guessTimeout = _guessTimeout;
        }


        public List<Grocery> GetGrocery(int userid)
        {
            var Groceries = GetGroceryIncludefriends(userid).ToList();//GetUserGroceries(userid);
            ;

            return Groceries;
        }

        //update
        public IEnumerable<Grocery> GetGroceryIncludefriends(int userid)
        {
            //what's the diffrence between select and find
            var Fulluser = GetFullUser(userid);
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
        public static bool IsGroceryExcluded(int userid,string excludeids)
        {
            string pattern = @"(\d+)\,";
            string idString = "" + userid;
            var matches = Regex.Matches(excludeids, pattern);

            foreach (Match m in matches)
            {
                var matchedId = Regex.Match(m.ToString(), @"(\d+)").ToString();
                if(matchedId == idString)
                {
                    return true;
                }
            }
            return false;
        }

        public Grocery GetGroceryById(int Groceryid,int userid )
        {
            var grocery = GetSpecificGroceryForUser(Groceryid,userid);

            if (grocery == null)
            {
                throw new AppException( "Not Found" );
            }

            return grocery;
        }

        public User AddGrocery(Grocery grocery,int id)
        {
            //--------------add logic-------------//
            //Validation
            if (GroceryExistsName(grocery.name,id)) { throw new AppException("Username Already Exists, Try new one");  }//validate

            grocery.moreInformations = UpdateInformationsAdd(grocery.moreInformations);

            var queryable = db.Users.Find(id);
            grocery.owner = queryable.firstname;
            grocery.ownerid = queryable.id;
            queryable.userGroceries = new List<Grocery>();
            queryable.userGroceries.Add(grocery);


            var userEntity = db.Users.Update(queryable);
            //db.Entry(queryable).State = EntityState.Modified;
            db.SaveChanges();
            return userEntity.Entity;
        }

        public void neededGrocery(Grocery grocery, int userid)
        {

            var editgrocery = GetSpecificGroceryForUser(grocery.id,userid);

            if (editgrocery == null) { throw new AppException("No grocery was found"); }//validate

            editgrocery.timeout = 0;//override the timeout

            var more = UpdateInformationsList(editgrocery.moreInformations, false);
            editgrocery.moreInformations.Add(more);
            editgrocery.groceryOrBought = false;
            db.Entry(editgrocery).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new AppException("DbUpdateConcurrencyException");
            }
            return ;
        }

        public string boughtgrocery(Grocery grocery, int userid)
        {
            var editgrocery = GetSpecificGroceryForUser(grocery.id, userid);

            if (editgrocery == null) { throw new AppException("Id Not found");  }//validate

            editgrocery.timeout = HandleTimeout(editgrocery, grocery.timeout);



            var more = UpdateInformationsList(editgrocery.moreInformations, true);
            editgrocery.moreInformations.Add(more);
            editgrocery.groceryOrBought = true;
            db.Entry(editgrocery).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new AppException("DbUpdateConcurrencyException");
            }
            return "bought";
        }

        public string editgrocery(Grocery grocery, int userid)
        {
            var editgrocery = GetSpecificGroceryForUser(grocery.id, userid);

            if (editgrocery == null) throw new AppException("Not found");
            editgrocery.basic= grocery.basic;
            editgrocery.groceryOrBought= grocery.groceryOrBought;
            editgrocery.name= grocery.name;
            editgrocery.timeout= grocery.timeout;

            db.Entry(editgrocery).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroceryExistsId(grocery.id , userid))
                {
                    return "NotFound";
                }
                else
                {
                    throw new AppException("Not found");
                }
            }
            return "edited";
        }

        public string removegrocery(Grocery grocery, int userid)
        {

            var removegrocery  = GetSpecificGroceryForUser(grocery.id, userid);

            if (removegrocery == null)
                throw new AppException("Not found");

            if (removegrocery.moreInformations.Count <= 1)
            {
                return "Item has to be Deleted";
            }
            removegrocery.moreInformations.RemoveAt(grocery.moreInformations.Count - 1);
            removegrocery.groceryOrBought = removegrocery.moreInformations.Last().bought;

            //edit
            db.Entry(removegrocery).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {

                throw new AppException("DbUpdateConcurrencyException");
            }
            return "Ok";
        }

        public string deletegrocery(Grocery grocery, int userid)
        {
            var user = GetFullUser(userid);
            var Deletegrocery = user.userGroceries.FirstOrDefault(g=>g.id==grocery.id);

            if (Deletegrocery == null)
            {
                throw new AppException("Not found");
            }
            user.userGroceries.Remove(Deletegrocery);
            db.Users.Update(user);
            try
            {
                db.SaveChanges();
                return "Deleted";
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new AppException("DbUpdateConcurrencyException");
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

        public bool GroceryExistsName(string name,int id)
        {
            if (string.IsNullOrEmpty(name)) return true;
            return db.Users.Any(u => u.id == id && u.userGroceries.Any(e => e.name.ToLower() == name.ToLower()));
        }


        //============ Private Methods ===========//
        private User GetFullUser(int id)
        {
            var user = db.Users
                .Include("userGroceries")
                .Include("userGroceries.moreInformations")
                .Include("userFriends")
                .FirstOrDefault(u => u.id == id);
            return user;
        }

        private List<Grocery> GetUserGroceries(int userid)
        {
            var groceries = db.userGroceries
                .Include("moreInformations")
                .Where(u => u.Userid == userid)
                .ToList();
            return groceries;
        }

        private List<UserFriend> GetUserFriends(int userid)
        {
            var friends = db.userFriends
                .Where(u => u.Userid == userid)
                .ToList();
            return friends;
        }

        private Grocery GetSpecificGroceryForUser(int groceryid, int userid)
        {
            var groceries = GetFullUser(userid)
                .userGroceries
                .FirstOrDefault(g=>g.id==groceryid)
            ;
            return groceries;
        }

        private bool GroceryExistsId(int Groceryid, int id)
        {
            return db.Users.Any(u => u.id == id && u.userGroceries.Any(e => e.id == Groceryid ));
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
