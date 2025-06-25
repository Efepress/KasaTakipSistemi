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
    public class SalaryPaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalaryPaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private void PopulatePaymentDropdowns(SalaryPaymentViewModel? viewModel = null, int? employeeId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
          
            ViewData["EmployeeId"] = new SelectList(_context.Employees.Where(e => e.UserId == userId && e.IsActive), "Id", "FullName", viewModel?.EmployeeId ?? employeeId);
            ViewData["CurrencyId"] = new SelectList(_context.Currencies, "Id", "Name", viewModel?.CurrencyId);
            ViewData["SafeId"] = new SelectList(_context.Safes.Where(s => s.UserId == userId), "Id", "Name", viewModel?.SafeId);
        }

       
        public async Task<IActionResult> Create(int employeeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = await _context.Employees
                                    .Include(e => e.SalaryCurrency)
                                    .FirstOrDefaultAsync(e => e.Id == employeeId && e.UserId == userId);
            if (employee == null) return NotFound("Personel bulunamadı.");

            var viewModel = new SalaryPaymentViewModel
            {
                EmployeeId = employee.Id,
                EmployeeFullName = employee.FullName,
                AmountPaid = employee.SalaryAmount, 
                CurrencyId = employee.SalaryCurrencyId, 
                SafeId = employee.DefaultSafeId ?? 0, 
                PaymentDate = DateTime.Now.Date
            };
            PopulatePaymentDropdowns(viewModel, employeeId);
            return View(viewModel);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SalaryPaymentViewModel viewModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == viewModel.EmployeeId && e.UserId == userId);
            if (employee == null) ModelState.AddModelError("EmployeeId", "Geçersiz personel seçimi.");

            var safeExists = await _context.Safes.AnyAsync(s => s.Id == viewModel.SafeId && s.UserId == userId);
            if (!safeExists) ModelState.AddModelError("SafeId", "Geçersiz kasa seçimi.");

            if (ModelState.IsValid)
            {
                var salaryPayment = new SalaryPayment
                {
                    EmployeeId = viewModel.EmployeeId,
                    PaymentDate = viewModel.PaymentDate,
                    AmountPaid = viewModel.AmountPaid,
                    CurrencyId = viewModel.CurrencyId,
                    SafeId = viewModel.SafeId,
                    Description = viewModel.Description,
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };

             
                var transaction = new Transaction
                {
                    SafeId = viewModel.SafeId,
                    Type = TransactionType.Gider, 
                    Amount = viewModel.AmountPaid,
                    CurrencyId = viewModel.CurrencyId,
                    Description = $"{employee?.FullName} - Maaş Ödemesi ({viewModel.PaymentDate:MMMM yyyy})",
                    TransactionDate = viewModel.PaymentDate.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute), 
                    PayeeOrPayer = employee?.FullName, 
                    UserId = userId
                };
                _context.Transactions.Add(transaction);
               

                _context.SalaryPayments.Add(salaryPayment);
                await _context.SaveChangesAsync(); 

                TempData["SuccessMessage"] = $"{employee?.FullName} için maaş ödemesi başarıyla kaydedildi.";
                return RedirectToAction("Details", "Employees", new { id = viewModel.EmployeeId });
            }

            PopulatePaymentDropdowns(viewModel, viewModel.EmployeeId);
            viewModel.EmployeeFullName = employee?.FullName; 
            return View(viewModel);
        }

       
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var salaryPayment = await _context.SalaryPayments
                                        .Include(sp => sp.Employee)
                                        .FirstOrDefaultAsync(sp => sp.Id == id && sp.UserId == userId);
            if (salaryPayment == null) return NotFound();

            var viewModel = new SalaryPaymentViewModel
            {
                Id = salaryPayment.Id,
                EmployeeId = salaryPayment.EmployeeId,
                EmployeeFullName = salaryPayment.Employee?.FullName,
                PaymentDate = salaryPayment.PaymentDate,
                AmountPaid = salaryPayment.AmountPaid,
                CurrencyId = salaryPayment.CurrencyId,
                SafeId = salaryPayment.SafeId,
                Description = salaryPayment.Description
            };
            PopulatePaymentDropdowns(viewModel, salaryPayment.EmployeeId);
            return View(viewModel);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SalaryPaymentViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingPayment = await _context.SalaryPayments.Include(sp => sp.Transaction).FirstOrDefaultAsync(sp => sp.Id == id && sp.UserId == userId);
            if (existingPayment == null) return NotFound();

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == viewModel.EmployeeId && e.UserId == userId);
            if (employee == null) ModelState.AddModelError("EmployeeId", "Geçersiz personel seçimi.");

            var safeExists = await _context.Safes.AnyAsync(s => s.Id == viewModel.SafeId && s.UserId == userId);
            if (!safeExists) ModelState.AddModelError("SafeId", "Geçersiz kasa seçimi.");


            if (ModelState.IsValid)
            {
            

                existingPayment.EmployeeId = viewModel.EmployeeId; 
                existingPayment.PaymentDate = viewModel.PaymentDate;
                existingPayment.AmountPaid = viewModel.AmountPaid;
                existingPayment.CurrencyId = viewModel.CurrencyId;
                existingPayment.SafeId = viewModel.SafeId;
                existingPayment.Description = viewModel.Description;

            
                if (existingPayment.Transaction != null)
                {
                    existingPayment.Transaction.SafeId = viewModel.SafeId;
                    existingPayment.Transaction.Amount = viewModel.AmountPaid;
                    existingPayment.Transaction.CurrencyId = viewModel.CurrencyId;
                    existingPayment.Transaction.Description = $"{employee?.FullName} - Maaş Ödemesi (Güncellendi)";
                    existingPayment.Transaction.TransactionDate = viewModel.PaymentDate.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute);
                    existingPayment.Transaction.PayeeOrPayer = employee?.FullName;
                    _context.Transactions.Update(existingPayment.Transaction);
                }


                try
                {
                    _context.Update(existingPayment);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Maaş ödemesi başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.SalaryPayments.Any(e => e.Id == viewModel.Id && e.UserId == userId)) return NotFound();
                    else throw;
                }
                return RedirectToAction("Details", "Employees", new { id = viewModel.EmployeeId });
            }
            PopulatePaymentDropdowns(viewModel, viewModel.EmployeeId);
            viewModel.EmployeeFullName = employee?.FullName;
            return View(viewModel);
        }


     
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int employeeId) 
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var salaryPayment = await _context.SalaryPayments.Include(sp => sp.Transaction).FirstOrDefaultAsync(sp => sp.Id == id && sp.UserId == userId);
            if (salaryPayment != null)
            {
              
                if (salaryPayment.Transaction != null)
                {
                    _context.Transactions.Remove(salaryPayment.Transaction);
                }
                _context.SalaryPayments.Remove(salaryPayment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Maaş ödemesi başarıyla silindi.";
            }
            return RedirectToAction("Details", "Employees", new { id = employeeId });
        }
    }
}