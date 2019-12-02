using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DanaTask_2.Models;

namespace DanaTask_2.Controllers
{
    public class AccountController : Controller
    {
        private DatabaseContext db = new DatabaseContext();

        #region Login
        [HttpGet]
        public ActionResult Login()
        {
            ViewBag.Errors = new string[] { };
            return View(new User());
        }

        [HttpPost]
        public ActionResult Login([Bind(Include = "Email, Password")] User user)
        {
            List<string> errors = new List<string>();

            User dbUser = db.Users.FirstOrDefault(x => x.Email == user.Email);
            if (dbUser != null)
            {
                if (dbUser.Password == user.Password)
                {
                    Session["Email"] = dbUser.Email;
                    Session["Role"] = dbUser.Role;
                    Session["Name"] = dbUser.FullName;
                    Session["Id"] = dbUser.Id;

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    errors.Add($"Wrong password for user {user.Email}");
                }
            }
            else
            {
                errors.Add($"User with email {user.Email} does not exists");
            }

            ViewBag.Errors = errors.ToArray();
            return View(user);
        }
        #endregion //LOGIN
        #region Registration
        [HttpGet]
        public ActionResult Register()
        {
            ViewBag.Errors = new string[] { };
            return View(new User());
        }

        [HttpPost]
        public ActionResult Register([Bind(Include = "Name, SecondName, Email, Password, RePassword")] User user)
        {
            List<string> errors = new List<string>();

            if (!string.IsNullOrWhiteSpace(user.Email) && 
                !string.IsNullOrWhiteSpace(user.Name) && !string.IsNullOrWhiteSpace(user.SecondName) && 
                !string.IsNullOrWhiteSpace(user.Password) && !string.IsNullOrWhiteSpace(user.RePassword))
            {
                User dbUser = db.Users.FirstOrDefault(x => x.Email == user.Email);
                if (dbUser == null)
                {
                    if (new EmailAddressAttribute().IsValid(user.Email))
                    {
                        if (user.Password == user.RePassword)
                        {
                            user.Role = "User";

                            db.Users.Add(user);
                            db.SaveChanges();

                            return RedirectToAction("Login");
                        }
                        else
                        {
                            errors.Add("Passwords do not match");
                        }
                    }
                    else
                    {
                        errors.Add($"Email {user.Email} is not valid");
                    }
                }
                else
                {
                    errors.Add($"Email {user.Email} is already in use");
                }
            }
            else
            {
                errors.Add("All fields is required");
            }

            ViewBag.Errors = errors.ToArray();
            return View(user);
        }
        #endregion //REGISTRATION

        [HttpGet]
        public ActionResult Logout()
        {
            Session["Email"] = null;
            Session["Role"] = null;
            Session["Name"] = null;
            Session["Id"] = null;

            return RedirectToAction("Index", "Home");
        }
    }
}