﻿using System;
using System.IO;
using System.Reflection;
using AspNet.Identity.MongoDB;
using Microsoft.AspNet.Identity;
using MongoDB.Driver;
using Warehouse.Server.Identity;

namespace Warehouse.Utils.CreateUser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length < 2)
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                string exeName = Path.GetFileName(codeBase);

                Console.WriteLine("usage: {0} <username> <password>", exeName);
                return;
            }

            var username = args[0];
            var password = args[1];

            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetServer().GetDatabase("skill");
            var users = database.GetCollection<IdentityUser>("users");

            var context = new IdentityContext(users);
            var store = new UserStore<IdentityUser>(context);
            var manager = new UserManager<IdentityUser>(store);

            var user = new ApplicationUser { UserName = username };
            var result = manager.Create(user, password);
            if (result.Succeeded)
            {
                Console.WriteLine("user created!");
                Console.ReadKey();
                return;
            }

            foreach (var error in result.Errors)
            {
                Console.WriteLine(error);
            }
            Console.ReadKey();
        }
    }
}