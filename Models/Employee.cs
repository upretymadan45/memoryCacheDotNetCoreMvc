using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using caching.Data;

namespace caching.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public DateTime Dob { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}