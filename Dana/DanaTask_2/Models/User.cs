using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DanaTask_2.Models
{
    [Table("tblUsers")]
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string RePassword { get; set; }
        public string Name { get; set; }
        public string SecondName { get; set; }
        public string Role { get; set; }

        public string FullName {
            get {
                return Name + " " + SecondName;
            }
        }
    }
}