using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DanaTask_2.Models;

namespace DanaTask_2.Controllers
{
    public class AdminController : Controller
    {
        private DatabaseContext db = new DatabaseContext();
        [HttpGet]
        public ActionResult Tasks(DateTime? min, DateTime? max, string title = "", string desc = "", string status = "Any")
        {
            //Если юзер не админ/не залогинен, выдаем ошибку 403
            if (Session["Role"] == null || (string)Session["Role"] != "Admin")
                return new HttpStatusCodeResult(403);

            Task[] tasks = db.Tasks.ToArray();
            //Поиск
            tasks = tasks.Where(x => x.Title.Contains(title)).ToArray();
            tasks = tasks.Where(x => x.Description.Contains(desc)).ToArray();
            if (status != "Any")
                tasks = tasks.Where(x => x.Status == status).ToArray();

            if (min != null)
                tasks = tasks.Where(x => x.Date >= min.Value).ToArray();
            if (max != null)
                tasks = tasks.Where(x => x.Date <= max.Value).ToArray();

            ViewBag.Tasks = tasks;
            ViewBag.Users = db.Users.ToArray();

            ViewBag.Min = min;
            ViewBag.Max = max;
            ViewBag.TaskTitle = title;
            ViewBag.Desc = desc;
            ViewBag.Status = status;

            return View();
        }

        [HttpGet]
        public ActionResult Users()
        {
            if (Session["Role"] == null || (string)Session["Role"] != "Admin")
                return new HttpStatusCodeResult(403);

            ViewBag.Users = db.Users.ToList();

            return View();
        }
        [HttpGet]
        public ActionResult MakeAdmin(int userId = -1)
        {
            if (Session["Role"] == null || (string)Session["Role"] != "Admin")
                return new HttpStatusCodeResult(403);

            if ((int)Session["Id"] == userId)
                return new HttpStatusCodeResult(400);

            User user = db.Users.Where(x => x.Id == userId).FirstOrDefault();
            if (user == null)
                return new HttpStatusCodeResult(404);

            user.Role = "Admin";
            db.SaveChanges();

            return RedirectToAction("Users");
        }
        [HttpGet]
        public ActionResult MakeUser(int userId = -1)
        {
            if (Session["Role"] == null || (string)Session["Role"] != "Admin")
                return new HttpStatusCodeResult(403);

            if ((int)Session["Id"] == userId)
                return new HttpStatusCodeResult(400);

            User user = db.Users.Where(x => x.Id == userId).FirstOrDefault();
            if (user == null)
                return new HttpStatusCodeResult(404);

            user.Role = "User";
            db.SaveChanges();

            return RedirectToAction("Users");
        }
        [HttpGet]
        public ActionResult DeleteTask(int id)
        {
            //Если юзер не залогинен, выдаем ошибку 403
            if (Session["Role"] == null || (string)Session["Role"] != "Admin")
                return new HttpStatusCodeResult(403);

            Task task = db.Tasks.Where(x => x.Id == id).FirstOrDefault();
            if (task == null)
                return new HttpStatusCodeResult(400);

            return View(task);
        }

        [HttpPost]
        public ActionResult DeleteTask([Bind(Include = "Id")] Task task)
        {
            //Если юзер не залогинен, выдаем ошибку 403
            if (Session["Role"] == null || (string)Session["Role"] != "Admin")
                return new HttpStatusCodeResult(403);

            //Берем задача, и если задача не найдена выдаем 404
            Task dbTask = db.Tasks.Where(x => x.Id == task.Id).FirstOrDefault();
            if (task == null)
                return new HttpStatusCodeResult(404);

            db.Tasks.Remove(dbTask);
            db.SaveChanges();

            return RedirectToAction("Tasks");
        }

        //Изменение задачи
        [HttpGet]
        public ActionResult EditTask(int id)
        {
            if (Session["Role"] == null || (string)Session["Role"] != "Admin")
                return new HttpStatusCodeResult(403);
   
            Task task = db.Tasks.Where(x => x.Id == id).FirstOrDefault();

            ViewBag.Errors = new string[] { };
            return View(task);
        }

        [HttpPost]
        public ActionResult EditTask([Bind(Include = "Id, Date, Status, Title, Description")] Task task)
        {
            if (Session["Role"] == null || (string)Session["Role"] != "Admin")
                return new HttpStatusCodeResult(403);

            Task dbTask = db.Tasks.Where(x => x.Id == task.Id).FirstOrDefault();
            if (dbTask == null)
                return new HttpStatusCodeResult(404);

            List<string> errors = new List<string>();

            if (!string.IsNullOrWhiteSpace(task.Title))
            {
                if (!string.IsNullOrWhiteSpace(task.Description))
                {
                    dbTask.Title = task.Title;
                    dbTask.Description = task.Description;
                    dbTask.Status = task.Status;

                    db.SaveChanges();

                    return RedirectToAction("Tasks");
                }
                else
                {
                    errors.Add("Description is required");
                }
            }
            else
            {
                errors.Add("Title is required");
            }

            ViewBag.Errors = errors.ToArray();
            return View(task);
        }
    }
}