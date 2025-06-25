
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
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace KasaTakipSistemi.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        private async Task<List<Safe>> GetAccessibleSafesForUserAsync(string userId)
        {
            var ownedSafes = await _context.Safes
                                .Where(s => s.UserId == userId)
                                .ToListAsync();

            var authorizedSafesData = await _context.SafeUsers
                                        .Where(su => su.ApplicationUserId == userId && su.IsActive && su.Safe != null)
                                        .Include(su => su.Safe)
                                        .Select(su => su.Safe!)
                                        .ToListAsync();

            return ownedSafes.Concat(authorizedSafesData)
                             .GroupBy(s => s.Id)
                             .Select(g => g.First())
                             .OrderBy(s => s.Name)
                             .ToList();
        }

       
        private async Task<bool> CanUserAccessSafeAsync(string userId, int safeId)
        {
            var accessibleSafes = await GetAccessibleSafesForUserAsync(userId);
            return accessibleSafes.Any(s => s.Id == safeId);
        }

     
        private async Task PopulateDropdownsAsync(TransactionViewModel? model = null)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return;

            var accessibleSafes = await GetAccessibleSafesForUserAsync(userId);
            var userCurrentAccounts = await _context.CurrentAccounts
                                                .Where(ca => ca.UserId == userId)
                                                .OrderBy(ca => ca.Name)
                                                .ToListAsync();

            ViewData["CurrencyId"] = new SelectList(await _context.Currencies.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", model?.CurrencyId);
            ViewData["SafeId"] = new SelectList(accessibleSafes, "Id", "Name", model?.SafeId);
            ViewData["PayeeOrPayerList"] = new SelectList(userCurrentAccounts, "Name", "Name", model?.PayeeOrPayer);
        }

       
        private async Task<(int? safeId, string? safeName)> GetValidSelectedSafeAsync(string userId)
        {
            var selectedSafeIdString = HttpContext.Session.GetString("SelectedSafeId");
            if (int.TryParse(selectedSafeIdString, out int sessionId))
            {
                if (await CanUserAccessSafeAsync(userId, sessionId))
                {
                    var safeName = HttpContext.Session.GetString("SelectedSafeName");
                    return (sessionId, safeName);
                }
            }

         
            var firstAccessibleSafe = (await GetAccessibleSafesForUserAsync(userId)).FirstOrDefault();
            if (firstAccessibleSafe != null)
            {
                HttpContext.Session.SetString("SelectedSafeId", firstAccessibleSafe.Id.ToString());
                HttpContext.Session.SetString("SelectedSafeName", firstAccessibleSafe.Name);
                return (firstAccessibleSafe.Id, firstAccessibleSafe.Name);
            }

            return (null, null);
        }


       
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var (safeId, safeName) = await GetValidSelectedSafeAsync(userId);

            if (!safeId.HasValue)
            {
                TempData["InfoMessage"] = "Lütfen önce bir kasa seçin veya erişebileceğiniz bir kasa oluşturun/yetki alın.";
                return RedirectToAction("Index", "Home");
            }

            var transactions = await _context.Transactions
                .Where(t => t.SafeId == safeId.Value)
                .Include(t => t.Currency)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            ViewBag.SelectedSafeName = safeName ?? "Bilinmeyen Kasa";
            return View(transactions);
        }


        private async Task<IActionResult> ShowCreateEditForm(int? id, TransactionType type)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            TransactionViewModel viewModel;

            if (id.HasValue) 
            {
                var transaction = await _context.Transactions.FindAsync(id.Value);
                if (transaction == null || !await CanUserAccessSafeAsync(userId, transaction.SafeId))
                {
                    return NotFound();
                }
                viewModel = new TransactionViewModel
                {
                    Id = transaction.Id,
                    SafeId = transaction.SafeId,
                    Type = transaction.Type,
                    Amount = transaction.Amount,
                    CurrencyId = transaction.CurrencyId,
                    Description = transaction.Description,
                    TransactionDate = transaction.TransactionDate,
                    PayeeOrPayer = transaction.PayeeOrPayer
                };
                ViewData["FormTitle"] = (transaction.Type == TransactionType.Gelir ? "Gelir" : "Gider") + " Düzenle";
            }
            else 
            {
                var (safeId, _) = await GetValidSelectedSafeAsync(userId);
                if (!safeId.HasValue)
                {
                    TempData["InfoMessage"] = "İşlem eklemek için lütfen önce bir kasa seçin.";
                    return RedirectToAction("Index", "Home");
                }
                viewModel = new TransactionViewModel
                {
                    SafeId = safeId.Value,
                    TransactionDate = DateTime.Now,
                    Type = type
                };
                ViewData["FormTitle"] = type == TransactionType.Gelir ? "Gelir Ekle" : "Gider Ekle";
            }

            await PopulateDropdownsAsync(viewModel);
            return View("CreateEdit", viewModel);
        }

   
        public async Task<IActionResult> CreateIncome() => await ShowCreateEditForm(null, TransactionType.Gelir);

      
        public async Task<IActionResult> CreateExpense() => await ShowCreateEditForm(null, TransactionType.Gider);

     
        public async Task<IActionResult> Edit(int id) => await ShowCreateEditForm(id, TransactionType.Gelir); // Tip önemli değil, içeriden okunacak

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(TransactionViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (!await CanUserAccessSafeAsync(userId, model.SafeId))
            {
                ModelState.AddModelError("SafeId", "Bu kasa üzerinde işlem yapma yetkiniz yok.");
            }

            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    var transaction = new Transaction
                    {
                        SafeId = model.SafeId,
                        Type = model.Type,
                        Amount = model.Amount,
                        CurrencyId = model.CurrencyId,
                        Description = model.Description,
                        TransactionDate = model.TransactionDate,
                        PayeeOrPayer = model.PayeeOrPayer,
                        UserId = userId
                    };
                    _context.Add(transaction);
                    TempData["SuccessMessage"] = (model.Type == TransactionType.Gelir ? "Gelir" : "Gider") + " başarıyla eklendi.";
                }
                else 
                {
                    var transactionToUpdate = await _context.Transactions.FindAsync(model.Id);
                    if (transactionToUpdate == null || !await CanUserAccessSafeAsync(userId, transactionToUpdate.SafeId))
                    {
                        return NotFound();
                    }

                    transactionToUpdate.SafeId = model.SafeId;
                    transactionToUpdate.Amount = model.Amount;
                    transactionToUpdate.CurrencyId = model.CurrencyId;
                    transactionToUpdate.Description = model.Description;
                    transactionToUpdate.TransactionDate = model.TransactionDate;
                    transactionToUpdate.PayeeOrPayer = model.PayeeOrPayer;
                    
                    _context.Update(transactionToUpdate);
                    TempData["SuccessMessage"] = "İşlem başarıyla güncellendi.";
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            
            await PopulateDropdownsAsync(model);
            ViewData["FormTitle"] = model.Id == 0
                ? (model.Type == TransactionType.Gelir ? "Gelir Ekle" : "Gider Ekle")
                : (model.Type == TransactionType.Gelir ? "Gelir Düzenle" : "Gider Düzenle");
            return View("CreateEdit", model);
        }

      
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var transaction = await _context.Transactions
                .Include(t => t.Currency)
                .Include(t => t.Safe)
                .FirstOrDefaultAsync(m => m.Id == id.Value);

            if (transaction == null || !await CanUserAccessSafeAsync(userId, transaction.SafeId))
            {
                return NotFound();
            }

            return View(transaction);
        }

     
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

          
            var transactionToDelete = await _context.Transactions.FindAsync(id);

            if (transactionToDelete == null)
            {
                TempData["ErrorMessage"] = "Silinecek işlem bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            if (!await CanUserAccessSafeAsync(userId, transactionToDelete.SafeId))
            {
                TempData["ErrorMessage"] = "Bu işlemi silme yetkiniz yok.";
                return RedirectToAction(nameof(Index));
            }

            using (var dbTransactionScope = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                
                    var relatedExchange = await _context.CurrencyExchanges
                        .FirstOrDefaultAsync(ce => (ce.ExpenseTransactionId == id || ce.IncomeTransactionId == id) && ce.UserId == userId);

                    if (relatedExchange != null)
                    {
                    
                        _context.CurrencyExchanges.Remove(relatedExchange);

                     
                        if (relatedExchange.ExpenseTransactionId != default && relatedExchange.ExpenseTransactionId != id)
                        {
                            var expenseTx = await _context.Transactions.FindAsync(relatedExchange.ExpenseTransactionId);
                            if (expenseTx != null) _context.Transactions.Remove(expenseTx);
                        }
                        else if (relatedExchange.ExpenseTransactionId == id) 
                        {
                        
                        }


                        
                        if (relatedExchange.IncomeTransactionId != default && relatedExchange.IncomeTransactionId != id)
                        {
                            var incomeTx = await _context.Transactions.FindAsync(relatedExchange.IncomeTransactionId);
                            if (incomeTx != null) _context.Transactions.Remove(incomeTx);
                        }
                        else if (relatedExchange.IncomeTransactionId == id) 
                        {
                        
                            if (relatedExchange.ExpenseTransactionId != default) 
                            {
                                var expenseTx = await _context.Transactions.FindAsync(relatedExchange.ExpenseTransactionId);
                                if (expenseTx != null) _context.Transactions.Remove(expenseTx);
                            }
                        }


                    
                        _context.Transactions.Remove(transactionToDelete);


                        TempData["SuccessMessage"] = "Para bozdurma işlemi ve ilgili kasa hareketleri başarıyla silindi.";
                    }
                    else
                    {

                        var relatedSalaryPayment = await _context.SalaryPayments
                                                         .FirstOrDefaultAsync(sp => sp.TransactionId == id && sp.UserId == userId);
                        if (relatedSalaryPayment != null)
                        {
                            _context.Transactions.Remove(transactionToDelete);
                            _context.SalaryPayments.Remove(relatedSalaryPayment); 
                            TempData["SuccessMessage"] = "Maaş ödemesiyle ilişkili kasa hareketi ve ödeme kaydı başarıyla silindi.";
                        }
                        else
                        {
                            _context.Transactions.Remove(transactionToDelete);
                            TempData["SuccessMessage"] = "İşlem başarıyla silindi.";
                        }
                    }

                    await _context.SaveChangesAsync();
                    await dbTransactionScope.CommitAsync();
                }
                catch (Exception ex)
                {
                    await dbTransactionScope.RollbackAsync();
                    TempData["ErrorMessage"] = "İşlem silinirken bir hata oluştu: " + ex.Message + (ex.InnerException != null ? " Detay: " + ex.InnerException.Message : "");
                  
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}