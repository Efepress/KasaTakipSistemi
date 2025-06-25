
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
    public class BankAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BankAccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

      
        public async Task<IActionResult> Index(string searchTerm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var query = _context.BankAccounts
                .Where(ba => ba.UserId == userId)
                .Include(b => b.Bank)
                .Include(b => b.Currency)
                .Select(ba => new BankAccountViewModel
                {
                    Id = ba.Id,
                    AccountName = ba.AccountName,
                    BankName = ba.Bank != null ? ba.Bank.Name : "-",
                    AccountTypeName = CurrentAccountsController.GetDisplayName(ba.AccountType), 
                    CurrencyName = ba.Currency != null ? ba.Currency.Name : "-",
                    CurrencySymbol = ba.Currency != null ? ba.Currency.Symbol : "",
                    Iban = ba.Iban 
                });

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(vm => vm.AccountName.Contains(searchTerm) ||
                                           vm.BankName.Contains(searchTerm) ||
                                           vm.Iban.Contains(searchTerm)); 
            }

            var bankAccounts = await query.OrderBy(ba => ba.BankName).ThenBy(ba => ba.AccountName).ToListAsync();
            ViewBag.SearchTerm = searchTerm;
            return View(bankAccounts);
        }

        private async Task PopulateDropdownsAsync(BankAccountCreateEditViewModel? model = null)
        {
            ViewData["BankId"] = new SelectList(await _context.Banks.OrderBy(b => b.Name).ToListAsync(), "Id", "Name", model?.BankId);
            ViewData["CurrencyId"] = new SelectList(await _context.Currencies.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", model?.CurrencyId);
            ViewData["AccountTypes"] = new SelectList(Enum.GetValues(typeof(BankAccountType))
                .Cast<BankAccountType>()
                .Select(v => new SelectListItem
                {
                    Text = CurrentAccountsController.GetDisplayName(v),
                    Value = ((int)v).ToString()
                }).ToList(), "Value", "Text", model?.AccountType);
        }

       
        public async Task<IActionResult> Create()
        {
            var viewModel = new BankAccountCreateEditViewModel();
            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BankAccountCreateEditViewModel viewModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (ModelState.IsValid)
            {
                var bankAccount = new BankAccount
                {
                    AccountName = viewModel.AccountName,
                    BankId = viewModel.BankId,
                    AccountType = viewModel.AccountType,
                    CurrencyId = viewModel.CurrencyId,
                    BranchCode = viewModel.BranchCode,
                    AccountNumber = viewModel.AccountNumber,
                    Iban = viewModel.Iban,
                    IsActive = viewModel.IsActive,
                    Notes = viewModel.Notes,
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Add(bankAccount);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Banka hesabı başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

       
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var bankAccount = await _context.BankAccounts.FirstOrDefaultAsync(ba => ba.Id == id && ba.UserId == userId);
            if (bankAccount == null) return NotFound();

            var viewModel = new BankAccountCreateEditViewModel
            {
                Id = bankAccount.Id,
                AccountName = bankAccount.AccountName,
                BankId = bankAccount.BankId,
                AccountType = bankAccount.AccountType,
                CurrencyId = bankAccount.CurrencyId,
                BranchCode = bankAccount.BranchCode,
                AccountNumber = bankAccount.AccountNumber,
                Iban = bankAccount.Iban,
                IsActive = bankAccount.IsActive,
                Notes = bankAccount.Notes
            };

            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BankAccountCreateEditViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingAccount = await _context.BankAccounts.AsNoTracking().FirstOrDefaultAsync(ba => ba.Id == id && ba.UserId == userId);
            if (existingAccount == null) return NotFound();

            if (ModelState.IsValid)
            {
                var bankAccountToUpdate = new BankAccount
                {
                    Id = viewModel.Id,
                    AccountName = viewModel.AccountName,
                    BankId = viewModel.BankId,
                    AccountType = viewModel.AccountType,
                    CurrencyId = viewModel.CurrencyId,
                    BranchCode = viewModel.BranchCode,
                    AccountNumber = viewModel.AccountNumber,
                    Iban = viewModel.Iban,
                    IsActive = viewModel.IsActive,
                    Notes = viewModel.Notes,
                    UserId = existingAccount.UserId, 
                    CreatedAt = existingAccount.CreatedAt, 
                    UpdatedAt = DateTime.Now
                };

                try
                {
                    _context.Update(bankAccountToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Banka hesabı başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.BankAccounts.Any(e => e.Id == viewModel.Id && e.UserId == userId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }


      
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var bankAccount = await _context.BankAccounts
                .Include(b => b.Bank)
                .Include(b => b.Currency)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (bankAccount == null) return NotFound();

            return View(bankAccount);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bankAccount = await _context.BankAccounts.FirstOrDefaultAsync(ba => ba.Id == id && ba.UserId == userId);
            if (bankAccount != null)
            {
                
                _context.BankAccounts.Remove(bankAccount);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Banka hesabı başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}