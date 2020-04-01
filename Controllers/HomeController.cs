using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using caching.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Threading.Tasks;
using caching.Data;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace caching.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMemoryCache memoryCache;
        private readonly ApplicationDbContext context;

        public HomeController(IMemoryCache memoryCache,
                                ApplicationDbContext context)
        {
            this.memoryCache = memoryCache;
            this.context = context;
        }

        public IActionResult Index(int startPage = 0)
        {
            var pageSize = 20;

            var fromPage = startPage * pageSize;

            ViewBag.NextPage = startPage + 1;
            ViewBag.PreviousPage = startPage - 1;

            List<Employee> employees;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (!memoryCache.TryGetValue("Employees", out employees))
            {
                // var memoryCacheEntryOptions = new MemoryCacheEntryOptions()
                //                             .SetAbsoluteExpiration(TimeSpan.FromMinutes(60));

                memoryCache.Set("Employees", context.Employees.ToList());
            }

            employees = memoryCache.Get("Employees") as List<Employee>;

            stopwatch.Stop();

            ViewBag.TimeToLoadData = stopwatch.Elapsed;

            ViewBag.TotalRows = employees.Count;

            employees = employees.Skip(fromPage).Take(pageSize).ToList();

            return View(employees);
        }

        [HttpGet]
        public IActionResult New()
        {
            return View(new Employee());
        }

        [HttpPost]
        public IActionResult New(Employee employee)
        {
            if (!ModelState.IsValid)
                return View(employee);

            context.Employees.Add(employee);
            context.SaveChanges();

            var employeesInCache = memoryCache.Get("Employees") as List<Employee>;
            employeesInCache.Add(employee);

            memoryCache.Remove("Employees");
            memoryCache.Set("Employees", employeesInCache);


            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id, int currentPage)
        {
            var employee = context.Employees.FirstOrDefault(x => x.Id == id);
            ViewBag.CurrentPage = currentPage;
            return View(employee);
        }

        [HttpPost]
        public IActionResult Edit(Employee employee, int currentPage)
        {
            if (!ModelState.IsValid)
                return View(employee);

            context.Employees.Update(employee);
            context.SaveChanges();

            var employeesInCache = memoryCache.Get("Employees") as List<Employee>;
            var oldEmployee = employeesInCache.FirstOrDefault(x => x.Id == employee.Id);
            var oldEmployeePosition = employeesInCache.FindIndex(x => x.Id == employee.Id);

            employeesInCache.Remove(oldEmployee);
            employeesInCache.Insert(oldEmployeePosition, employee);

            memoryCache.Remove("Employees");
            memoryCache.Set("Employees", employeesInCache);


            return RedirectToAction(nameof(Index), new { startPage = currentPage });
        }

        public IActionResult Delete(int id, int currentPage)
        {
            var employee = context.Employees.FirstOrDefault(x => x.Id == id);
            context.Employees.Remove(employee);
            context.SaveChanges();

            var employeesInCache = memoryCache.Get("Employees") as List<Employee>;
            var oldEmployee = employeesInCache.FirstOrDefault(x => x.Id == id);
            employeesInCache.Remove(oldEmployee);

            memoryCache.Remove("Employees");
            memoryCache.Set("Employees", employeesInCache);

            return RedirectToAction(nameof(Index), new { startPage = currentPage });
        }

        public async Task<IActionResult> AddEmployees()
        {
            try
            {
                await new Seed(this.context).CreateMillionEmployee();
                return Ok("Success");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
