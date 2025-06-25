
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
using Microsoft.AspNetCore.Http; 
using System.Collections.Generic; 


namespace KasaTakipSistemi.Controllers
{
    [Authorize]
    public class CurrencyExchangeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; 

        public CurrencyExchangeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) 
        {
            _context = context;
            _userManager = userManager;
        }

      
        private async Task<bool> CanUserAccessSafeAsync(string userId, int safeId)
        {
            bool isOwner = await _context.Safes.AnyAsync(s => s.Id == safeId && s.UserId == userId);
            if (isOwner) return true;
            return await _context.SafeUsers.AnyAsync(su => su.SafeId == safeId && su.ApplicationUserId == userId && su.IsActive);
        }


        private async Task PopulateDropdownsAsync(CurrencyExchangeViewModel? viewModel = null)
        {
            var userId = _userManager.GetUserId(User); 
            if (string.IsNullOrEmpty(userId)) 
            {
                ViewData["SoldCurrencyId"] = new SelectList(new List<Currency>(), "Id", "Name");
                ViewData["BoughtCurrencyId"] = new SelectList(new List<Currency>(), "Id", "Name");
                ViewData["CurrentAccountId"] = new SelectList(new List<CurrentAccount>(), "Id", "Name");
                ViewData["AccountSources"] = new SelectList(new List<SelectListItem>());
                return;
            }

            var currencies = await _context.Currencies.OrderBy(c => c.Name).ToListAsync();
            var currentAccounts = await _context.CurrentAccounts
                                            .Where(ca => ca.UserId == userId) 
                                            .OrderBy(ca => ca.Name).ToListAsync();

            
            var accountSources = Enum.GetValues(typeof(AccountSourceType))
                .Cast<AccountSourceType>()
                .Where(ast => ast == AccountSourceType.Nakit)
                .Select(v => new SelectListItem
                {
                    
                    Text = v.ToString(), 
                    Value = ((int)v).ToString()
                }).ToList();


            ViewData["SoldCurrencyId"] = new SelectList(currencies, "Id", "Name", viewModel?.SoldCurrencyId);
            ViewData["BoughtCurrencyId"] = new SelectList(currencies, "Id", "Name", viewModel?.BoughtCurrencyId);
            ViewData["CurrentAccountId"] = new SelectList(currentAccounts, "Id", "Name", viewModel?.CurrentAccountId);
            ViewData["AccountSources"] = new SelectList(accountSources, "Value", "Text", viewModel?.SoldAccountSource.ToString());
        }


       
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var selectedSafeIdString = HttpContext.Session.GetString("SelectedSafeId");
            if (string.IsNullOrEmpty(selectedSafeIdString) || !int.TryParse(selectedSafeIdString, out int selectedSafeId))
            {
                TempData["InfoMessage"] = "Para bozdurma işlemi için lütfen önce bir ana kasa seçin.";
                return RedirectToAction("Index", "Home");
            }

           
            if (!await CanUserAccessSafeAsync(userId, selectedSafeId))
            {
                TempData["ErrorMessage"] = "Seçili kasa üzerinde işlem yapma yetkiniz yok.";
                HttpContext.Session.Remove("SelectedSafeId"); 
                HttpContext.Session.Remove("SelectedSafeName");
                return RedirectToAction("Index", "Home");
            }

            var viewModel = new CurrencyExchangeViewModel
            {
                ExchangeDate = DateTime.Now,
                SelectedSafeId = selectedSafeId,
                SoldAccountSource = AccountSourceType.Nakit,
                BoughtAccountSource = AccountSourceType.Nakit
            };
            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CurrencyExchangeViewModel viewModel)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

           
            var selectedSafeIdString = HttpContext.Session.GetString("SelectedSafeId");
            if (string.IsNullOrEmpty(selectedSafeIdString) || !int.TryParse(selectedSafeIdString, out int mainSafeIdFromSession))
            {
                ModelState.AddModelError("", "Geçerli bir ana kasa seçimi bulunamadı. Lütfen ana sayfadan kasa seçin.");
            }
            else
            {
             
                viewModel.SelectedSafeId = mainSafeIdFromSession;
            }

            
            if (viewModel.SelectedSafeId == 0 || !await CanUserAccessSafeAsync(userId, viewModel.SelectedSafeId))
            {
                ModelState.AddModelError("", "Bu kasa üzerinde para bozdurma işlemi yapma yetkiniz yok.");
            }


            if (viewModel.SoldCurrencyId == viewModel.BoughtCurrencyId)
            {
                ModelState.AddModelError("SoldCurrencyId", "Bozdurulan para birimi ile alınan para birimi aynı olamaz.");
                ModelState.AddModelError("BoughtCurrencyId", "Bozdurulan para birimi ile alınan para birimi aynı olamaz.");
            }

            if (viewModel.SoldAccountSource != AccountSourceType.Nakit || viewModel.BoughtAccountSource != AccountSourceType.Nakit)
            {
                ModelState.AddModelError("", "Şu an için sadece Nakit (Ana Kasa) üzerinden para bozdurma desteklenmektedir.");
            }


            if (ModelState.IsValid)
            {
                var soldCurrencySymbol = (await _context.Currencies.FindAsync(viewModel.SoldCurrencyId))?.Symbol ?? "";
                var boughtCurrencySymbol = (await _context.Currencies.FindAsync(viewModel.BoughtCurrencyId))?.Symbol ?? "";
                var currentAccountName = viewModel.CurrentAccountId.HasValue ? (await _context.CurrentAccounts.FindAsync(viewModel.CurrentAccountId.Value))?.Name : "Döviz İşlemi";


                
                var expenseTransaction = new Transaction
                {
                    SafeId = viewModel.SelectedSafeId, 
                    Type = TransactionType.Gider,
                    Amount = viewModel.SoldAmount,
                    CurrencyId = viewModel.SoldCurrencyId,
                    Description = $"Para Bozdurma: {viewModel.SoldAmount:N2} {soldCurrencySymbol} verildi -> {viewModel.BoughtAmount:N2} {boughtCurrencySymbol} alındı.",
                    TransactionDate = viewModel.ExchangeDate,
                    PayeeOrPayer = currentAccountName,
                    UserId = userId
                };
                _context.Transactions.Add(expenseTransaction);

              
                var incomeTransaction = new Transaction
                {
                    SafeId = viewModel.SelectedSafeId, 
                    Type = TransactionType.Gelir,
                    Amount = viewModel.BoughtAmount,
                    CurrencyId = viewModel.BoughtCurrencyId,
                    Description = $"Para Bozdurma: {viewModel.SoldAmount:N2} {soldCurrencySymbol} verildi -> {viewModel.BoughtAmount:N2} {boughtCurrencySymbol} alındı.",
                    TransactionDate = viewModel.ExchangeDate.AddSeconds(1),
                    PayeeOrPayer = currentAccountName,
                    UserId = userId
                };
                _context.Transactions.Add(incomeTransaction);

       
                try
                {
                    await _context.SaveChangesAsync();

              
                    var currencyExchange = new CurrencyExchange
                    {
                        SoldAccountSource = viewModel.SoldAccountSource,
                        SoldCurrencyId = viewModel.SoldCurrencyId,
                        SoldAmount = viewModel.SoldAmount,
                        BoughtAccountSource = viewModel.BoughtAccountSource,
                        BoughtCurrencyId = viewModel.BoughtCurrencyId,
                        BoughtAmount = viewModel.BoughtAmount,
                        ExchangeDate = viewModel.ExchangeDate,
                        CurrentAccountId = viewModel.CurrentAccountId,
                        Description = string.IsNullOrEmpty(viewModel.Description) ? $"Döviz Alım/Satım - {currentAccountName}" : viewModel.Description,
                        ExpenseTransactionId = expenseTransaction.Id,
                        IncomeTransactionId = incomeTransaction.Id,
                        UserId = userId,
                        MainSafeId = viewModel.SelectedSafeId
                    };
                    _context.CurrencyExchanges.Add(currencyExchange);
                    await _context.SaveChangesAsync(); 

                    TempData["SuccessMessage"] = "Para bozdurma işlemi başarıyla kaydedildi.";
                    return RedirectToAction("Index", "Transactions"); 
                }
                catch (Exception ex)
                {
                
                    ModelState.AddModelError("", "İşlem kaydedilirken bir hata oluştu: " + ex.Message);
                }
            }

            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();

          
            var exchanges = await _context.CurrencyExchanges
               .Where(ce => ce.UserId == userId)
               .Include(ce => ce.SoldCurrency)
               .Include(ce => ce.BoughtCurrency)
               .Include(ce => ce.CurrentAccount) 
               .Include(ce => ce.MainSafe)     
               .Include(ce => ce.ExpenseTransaction) 
               .Include(ce => ce.IncomeTransaction)  
               .OrderByDescending(ce => ce.ExchangeDate)
               .ToListAsync();

           
            return View(exchanges);
        }

      
    }
}