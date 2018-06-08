﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using FridgeServer.Data;
using FridgeServer.Models;
using Microsoft.EntityFrameworkCore;

namespace FridgeServer.Services
{

    public class GuessTimeout
    {
        private AppDbContext db;
        public GuessTimeout(AppDbContext _db)
        {
            db = _db;
        }

        public  long GuessById(int id)
        {
            // db.Grocery.Include("MoreInformations").Include("Details")
            var grocery =  db.Grocery.Include("MoreInformations").SingleOrDefault(m => m.Id == id);
            if (grocery.MoreInformations==null) return 9999999;


            var lifeTimes = new List<lifetimeList>();

           // = grocery.MoreInformations.ConvertAll<lifetimeList>(new Converter<MoreInformations, lifetimeList>(s =>{  return 0;}));

            foreach (var item in grocery.MoreInformations)//extracting the lifetimes only
            {
                var x = item.LifeTime;
                var y = item.No;
                var XxX = new lifetimeList()
                {
                    LifeTime = (long)x,
                    No = (int)item.No

                };
                lifeTimes.Add(XxX);
            }

            return Guessingmath(lifeTimes);        
        }


        public long GuessByInformation(List<MoreInformations> more)
        {

            //Convert Moreinformatio To lifetimes&No
            var lifeTimes = new List<lifetimeList>() {
                new lifetimeList{LifeTime=0,No=1}
            };
            lifetimeList ArrayHolder;
            foreach (var item in more)//extracting the lifetimes only
            {
                var HoldLifeTime = item?.LifeTime==null? 0: item.LifeTime;
                var HoldNo = item?.No==null?0:item.No;
                ArrayHolder = new lifetimeList()
                {
                    LifeTime = (long)HoldLifeTime,
                    No = (int)item.No,
                    bought = item.Bought,
                    typeOfNo=item.typeOfNo
                };
                lifeTimes.Add(ArrayHolder);
            }

            
            return Guessingmath(lifeTimes);//Maths of Guessing Timeout
        }



        public static long Guessingmath(List<lifetimeList> lifeTimes)
        {
            int SumOfzeros = 0;
            long SumOfLifeTimes=0;
            long avrage=0;

            foreach (var item in lifeTimes)
            {
                if (item.bought)
                {
                    var x = SetMagnitude(item.typeOfNo);

                    SumOfLifeTimes += x*(item.LifeTime);
                    SumOfzeros += (item.LifeTime) == 0 ? 1 : 0;
                }
            }//SumOfLifTime and count the zeros 
            if ((lifeTimes.Count - SumOfzeros)!=0)
            {
                avrage = SumOfLifeTimes / (lifeTimes.Count - SumOfzeros);//avraging valid liftimes
            }
            
            if (avrage==0)
            {
                avrage = 3600 * 24 * 5;//the default is 5 days
            }
            return avrage;
        }

        public static long SetMagnitude(string typeOfNo) {
            switch (typeOfNo.ToLower())
            {
                default:
                    break;
            }
            return 1;
        }


        public class lifetimeList
        {
            public long LifeTime { get; set; }//the lifetime number
            public int No { get; set; }
            public bool bought { get; set; }
            public string typeOfNo { get; set; }
        }



    }
}