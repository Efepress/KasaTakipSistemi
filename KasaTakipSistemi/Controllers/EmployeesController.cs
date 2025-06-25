
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KasaTakipSistemi.Data;
using KasaTakipSistemi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using KasaTakipSistemi.ViewModels; 

namespace KasaTakipSistemi.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

     
        public async Task<IActionResult> Index(string searchTerm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var query = _context.Employees
                                .Where(e => e.UserId == userId)
                                .Include(e => e.SalaryCurrency)
                                .Include(e => e.DefaultSafe)
                                .Select(e => new EmployeeViewModel
                                {
                                    Id = e.Id,
                                    FullName = e.FullName,
                                    MaasBilgiStr = e.SalaryPeriod.ToString() + " " + e.SalaryAmount.ToString("N2") + " " + (e.SalaryCurrency != null ? e.SalaryCurrency.Symbol : ""),
                                    KasaAd = e.DefaultSafe != null ? e.DefaultSafe.Name : "-",
                                    IsActive = e.IsActive
                                });

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => e.FullName.Contains(searchTerm));
            }

            var employees = await query.OrderBy(e => e.FullName).ToListAsync();
            ViewBag.SearchTerm = searchTerm;
            return View(employees);
        }

     
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var employee = await _context.Employees
                .Include(e => e.SalaryCurrency)
                .Include(e => e.DefaultSafe)
                .Include(e => e.SalaryPayments)
                    .ThenInclude(sp => sp.Currency)
                .Include(e => e.SalaryPayments)
                    .ThenInclude(sp => sp.Safe)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (employee == null) return NotFound();

            return View(employee);
        }

        private void PopulateDropdowns(Employee? employee = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["SalaryCurrencyId"] = new SelectList(_context.Currencies, "Id", "Name", employee?.SalaryCurrencyId);
            ViewData["DefaultSafeId"] = new SelectList(_context.Safes.Where(s => s.UserId == userId), "Id", "Name", employee?.DefaultSafeId);
            ViewData["SalaryPeriods"] = new SelectList(Enum.GetValues(typeof(SalaryPeriod))
                .Cast<SalaryPeriod>()
                .Select(v => new SelectListItem
                {
                    Text = CurrentAccountsController.GetDisplayName(v),
                    Value = ((int)v).ToString()
                }).ToList(), "Value", "Text", employee?.SalaryPeriod);
        }

        
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Position,SalaryAmount,SalaryCurrencyId,SalaryPeriod,HireDate,IsActive,Notes,DefaultSafeId")] Employee employee)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            employee.UserId = userId;
            employee.CreatedAt = DateTime.Now;
            employee.UpdatedAt = DateTime.Now;

            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("SalaryCurrency");
            ModelState.Remove("DefaultSafe");
            ModelState.Remove("SalaryPayments");


            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Personel başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            PopulateDropdowns(employee);
            return View(employee);
        }

      
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
            if (employee == null) return NotFound();

            PopulateDropdowns(employee);
            return View(employee);
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Position,SalaryAmount,SalaryCurrencyId,SalaryPeriod,HireDate,IsActive,Notes,DefaultSafeId")] Employee employee)
        {
            if (id != employee.Id) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingEmployee = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
            if (existingEmployee == null) return NotFound();

            employee.UserId = existingEmployee.UserId;
            employee.CreatedAt = existingEmployee.CreatedAt;
            employee.UpdatedAt = DateTime.Now;

            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("SalaryCurrency");
            ModelState.Remove("DefaultSafe");
            ModelState.Remove("SalaryPayments");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Personel bilgileri başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Employees.Any(e => e.Id == employee.Id && e.UserId == userId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateDropdowns(employee);
            return View(employee);
        }

       
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var employee = await _context.Employees
                .Include(e => e.SalaryCurrency)
                .Include(e => e.DefaultSafe)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (employee == null) return NotFound();

            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = await _context.Employees.Include(e => e.SalaryPayments).FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
            if (employee != null)
            {
            
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Personel başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}