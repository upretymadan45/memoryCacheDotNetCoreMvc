using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using caching.Models;

namespace caching.Data
{
    public class Seed
    {
        private readonly ApplicationDbContext _context;

        public Seed(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateMillionEmployee()
        {
            var employees = new List<Employee>();

            for (var i = 0; i < 1000000; i++)
            {
                employees.Add(
                    new Employee
                    {
                        FullName = $"Employee{i}",
                        Age = i + 10,
                        Dob = DateTime.Now.AddYears(-30)
                    });
            }

            await _context.Employees.AddRangeAsync(employees);
            await _context.SaveChangesAsync();
        }
    }
}