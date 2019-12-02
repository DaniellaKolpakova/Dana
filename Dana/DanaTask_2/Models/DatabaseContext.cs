using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace DanaTask_2.Models
{
    public class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Task> Tasks { get; set; }

        public DatabaseContext() : base()
        {
            if (!Database.Exists())
                Database.Create();

            if (Users.ToList().Count == 0)
            {
                Users.Add(new User()
                {
                    Email = "admin",
                    Password = "admin",
                    Name = "Dana",
                    SecondName = "Kolpakova",
                    Role = "Admin"
                });

                Tasks.Add(new Task()
                {
                    UserId = 1,
                    Date = DateTime.Now,
                    Title = "Zadacha",
                    Description = "Sdelatb proekt",
                    Status = "In Progress"
                });

                SaveChanges();
            }
        }
    }
}