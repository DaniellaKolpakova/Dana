using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DanaTask_2.Models;

namespace DanaTask_2.Controllers
{
    public class CalendarController : Controller
    {
        private DatabaseContext db = new DatabaseContext();

        //Обычный год
        private int[] DaysInMonths_28 = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        //Високосный
        private int[] DaysInMonths_29 = new int[] { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        //Календарь текущего юзера
        [HttpGet]
        public ActionResult Index(int month = -1, int year = -1)
        {
            //Если юзер не залогинен, выдаем ошибку 403
            if (Session["Id"] == null)
                return new HttpStatusCodeResult(403);

            //Если год = -1, то берем текущий год
            if (year == -1)
                year = DateTime.Now.Year;
            //Тоже самое с месяцем
            if (month == -1)
                month = DateTime.Now.Month;
            //Если кто сломал URL и вписал свое число, то делаем так, что бы оно было в диапазоне от 1 до 12
            if (month > 12)
                month = 12;
            else if (month < 1)
                month = 1;

            //Получаем Id текущего юзера
            int userId = (int)Session["Id"];

            //Получаем информацию о задачах в выбраном году и месяце
            Task[] tasks = db.Tasks.Where(x => x.UserId == userId && x.Date.Year == year && x.Date.Month == month).ToArray();

            //Високосный год или нет
            int[] usingCalendar = DaysInMonths_28;
            if (year % 4 == 0)
                usingCalendar = DaysInMonths_29;

            //Передаем значения на страницу
            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.MonthDays = usingCalendar[month - 1];
            ViewBag.MonthString = new DateTime(year, month, 1).ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.UsingCalendar = usingCalendar;
            ViewBag.Tasks = tasks;

            return View();
        }

        //Добавление задачи к выбранной дате
        [HttpGet]
        public ActionResult AddTask(int month, int year, int day)
        {
            //Если юзер не залогинен, выдаем ошибку 403
            if (Session["Id"] == null)
                return new HttpStatusCodeResult(403);

            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.Day = day;

            ViewBag.MonthString = new DateTime(year, month, day).ToString("MMM", CultureInfo.InvariantCulture);

            ViewBag.Errors = new string[] { };
            return View(new Task());
        }

        [HttpPost]
        public ActionResult AddTask([Bind(Include = "Title, Status, Description, Date")] Task task)
        {
            //Если юзер не залогинен, выдаем ошибку 403
            if (Session["Id"] == null)
                return new HttpStatusCodeResult(403);

            List<string> errors = new List<string>();

            if (!string.IsNullOrWhiteSpace(task.Title))
            {
                if (!string.IsNullOrWhiteSpace(task.Description))
                {
                    task.UserId = (int)Session["Id"];

                    db.Tasks.Add(task);
                    db.SaveChanges();

                    return RedirectToAction("Index", new { year = task.Date.Year, month = task.Date.Month });
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

            ViewBag.Year = task.Date.Year;
            ViewBag.Month = task.Date.Month;
            ViewBag.Day = task.Date.Day;

            ViewBag.MonthString = new DateTime(task.Date.Year, task.Date.Month, task.Date.Day).ToString("MMM", CultureInfo.InvariantCulture);

            ViewBag.Errors = errors.ToArray();
            return View(task);
        }

        [HttpGet]
        public ActionResult DeleteTask(int id)
        {
            //Если юзер не залогинен, выдаем ошибку 403
            if (Session["Id"] == null)
                return new HttpStatusCodeResult(403);

            Task task = db.Tasks.Where(x => x.Id == id).FirstOrDefault();
            if (task == null)
                return new HttpStatusCodeResult(400);

            int userId = (int)Session["Id"];
            if (task.UserId != userId)
                return new HttpStatusCodeResult(403);

            return View(task);
        }

        [HttpPost]
        public ActionResult DeleteTask([Bind(Include = "Id")] Task task)
        {
            //Если юзер не залогинен, выдаем ошибку 403
            if (Session["Id"] == null)
                return new HttpStatusCodeResult(403);

            //Берем задача, и если задача не найдена выдаем 404
            Task dbTask = db.Tasks.Where(x => x.Id == task.Id).FirstOrDefault();
            if (task == null)
                return new HttpStatusCodeResult(404);

            int userId = (int)Session["Id"];
            if (dbTask.UserId != userId)
                return new HttpStatusCodeResult(403);

            db.Tasks.Remove(dbTask);
            db.SaveChanges();

            return RedirectToAction("Tasks", new { year = dbTask.Date.Year, month = dbTask.Date.Month, day = dbTask.Date.Day });
        }

        //Изменение задачи
        [HttpGet]
        public ActionResult EditTask(int id)
        {
            
            if (Session["Id"] == null)
                return new HttpStatusCodeResult(403);

            int userId = (int)Session["Id"];
            Task task = db.Tasks.Where(x => x.Id == id).FirstOrDefault();

            if (task.UserId != userId)
                return new HttpStatusCodeResult(403);

            ViewBag.Errors = new string[] { };
            return View(task);
        }

        [HttpPost]
        public ActionResult EditTask([Bind(Include = "Id, Date, Status, Title, Description")] Task task)
        {
            if (Session["Id"] == null)
                return new HttpStatusCodeResult(403);

            int userId = (int)Session["Id"];
            Task dbTask = db.Tasks.Where(x => x.Id == task.Id).FirstOrDefault();
            if (dbTask == null)
                return new HttpStatusCodeResult(404);

            if (dbTask.UserId != userId)
                return new HttpStatusCodeResult(403);

            List<string> errors = new List<string>();

            if (!string.IsNullOrWhiteSpace(task.Title))
            {
                if (!string.IsNullOrWhiteSpace(task.Description))
                {
                    dbTask.Title = task.Title;
                    dbTask.Description = task.Description;
                    dbTask.Status = task.Status;
                    
                    db.SaveChanges();

                    return RedirectToAction("Tasks", new { year = dbTask.Date.Year, month = dbTask.Date.Month, day = dbTask.Date.Day });
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

        //Просмотр всех задач, привязанных к выбранной дате
        [HttpGet]
        public ActionResult Tasks(int year, int month, int day)
        {
            //Если юзер не залогинен, выдаем ошибку 403
            if (Session["Id"] == null)
                return new HttpStatusCodeResult(403);

            //Получаем Id текущего юзера
            int userId = (int)Session["Id"];

            //Получаем информацию о задачах в выбраном году и месяце
            Task[] tasks = db.Tasks.Where(x => x.UserId == userId && x.Date.Year == year && x.Date.Month == month && x.Date.Day == day).ToArray();

            //Передаем данные на страницу
            ViewBag.Tasks = tasks;

            ViewBag.Day = day;
            ViewBag.Month = month;
            ViewBag.Year = year;

            ViewBag.MonthString = new DateTime(year, month, day).ToString("MMM", CultureInfo.InvariantCulture);

            return View();
        }

        [HttpGet]
        public ActionResult AllTasks(DateTime? min, DateTime? max, string title = "", string desc = "", string status = "Any")
        {
            //Если юзер не залогинен, выдаем ошибку 403
            if (Session["Id"] == null)
                return new HttpStatusCodeResult(403);

            int userId = (int)Session["Id"];
            Task[] tasks = db.Tasks.Where(x => x.UserId == userId).ToArray();
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

            ViewBag.Min = min;
            ViewBag.Max = max;
            ViewBag.TaskTitle = title;
            ViewBag.Desc = desc;
            ViewBag.Status = status;

            return View();
        }
    }
}