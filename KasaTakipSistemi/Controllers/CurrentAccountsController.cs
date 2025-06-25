
using System;
using System.Collections.Generic;
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
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace KasaTakipSistemi.Controllers
{
    [Authorize]
    public class CurrentAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CurrentAccountsController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(string searchTerm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

          
            var currentAccountsQuery = _context.CurrentAccounts
                                          .Where(ca => ca.UserId == userId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                currentAccountsQuery = currentAccountsQuery.Where(ca => ca.Name.Contains(searchTerm) ||
                                                               (ca.TaxNumber != null && ca.TaxNumber.Contains(searchTerm)) ||
                                                               (ca.PhoneNumber != null && ca.PhoneNumber.Contains(searchTerm)) ||
                                                               (ca.Email != null && ca.Email.Contains(searchTerm)));
            }

            var currentAccounts = await currentAccountsQuery.OrderBy(ca => ca.Name).ToListAsync();


            var viewModelList = new List<CurrentAccountViewModel>();
            var allCurrencies = await _context.Currencies.ToListAsync(); 

            foreach (var ca in currentAccounts)
            {
                var vm = new CurrentAccountViewModel
                {
                    Id = ca.Id,
                    Name = ca.Name,
                    AccountTypeStr = ca.AccountType.ToString(), 
                    TaxNumber = ca.TaxNumber,
                    PhoneNumber = ca.PhoneNumber,
                    Email = ca.Email
                
                };

              
                var transactionsForAccount = await _context.Transactions
                    .Where(t => t.UserId == userId && t.PayeeOrPayer == ca.Name) 
                    .Include(t => t.Currency)
                    .ToListAsync();

                var summaryHtmlBuilder = new StringBuilder();

                foreach (var currency in allCurrencies)
                {
                    decimal income = transactionsForAccount
                                        .Where(t => t.CurrencyId == currency.Id && t.Type == TransactionType.Gelir)
                                        .Sum(t => t.Amount);
                 

                    decimal expense = transactionsForAccount
                                        .Where(t => t.CurrencyId == currency.Id && t.Type == TransactionType.Gider)
                                        .Sum(t => t.Amount);

                    decimal balance = 0;
                    if (ca.AccountType == AccountType.Musteri)
                    {
            
                        balance = income - expense; 
                        balance = transactionsForAccount.Where(t => t.CurrencyId == currency.Id && t.Type == TransactionType.Gelir).Sum(t => t.Amount)
                                - transactionsForAccount.Where(t => t.CurrencyId == currency.Id && t.Type == TransactionType.Gider).Sum(t => t.Amount);


                    }
                 


                    if (balance != 0 || transactionsForAccount.Any(t => t.CurrencyId == currency.Id)) 
                    {
                        vm.Balances.Add(new AccountBalanceViewModel
                        {
                            CurrencyName = currency.Name,
                            CurrencySymbol = currency.Symbol,
                            Amount = balance
                        });

                        string colorClass = "";
                        if (balance > 0) colorClass = "green";
                        else if (balance < 0) colorClass = "red";
                        else colorClass = "black";

                        summaryHtmlBuilder.Append($"<small style=\"color: {colorClass};\">{balance:N2} </small><small style=\"color: black;\">{currency.Name}</small><br>");
                    }
                }
                vm.AllBalancesSummaryHtml = summaryHtmlBuilder.ToString();
                if (string.IsNullOrEmpty(vm.AllBalancesSummaryHtml)) 
                {
                
                    var defaultCurrency = allCurrencies.FirstOrDefault();
                    if (defaultCurrency != null)
                    {
                        vm.AllBalancesSummaryHtml = $"<small style=\"color: black;\">0.00 </small><small style=\"color: black;\">{defaultCurrency.Name}</small><br>";
                        vm.Balances.Add(new AccountBalanceViewModel { CurrencyName = defaultCurrency.Name, CurrencySymbol = defaultCurrency.Symbol, Amount = 0 });
                    }
                }


                viewModelList.Add(vm);
            }

            ViewBag.SearchTerm = searchTerm;
            return View(viewModelList);
        }


      
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var currentAccount = await _context.CurrentAccounts
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (currentAccount == null) return NotFound();

          
            return View(currentAccount);
        }

    
        public IActionResult Create()
        {
          
            ViewBag.AccountTypes = Enum.GetValues(typeof(AccountType))
                .Cast<AccountType>()
                .Select(v => new SelectListItem
                {
                    Text = GetDisplayName(v),
                    Value = ((int)v).ToString()
                }).ToList();
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,AccountType,TaxNumber,IdentificationNumber,Address,PhoneNumber,Email,Notes")] CurrentAccount currentAccount)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            currentAccount.UserId = userId;
            currentAccount.CreatedAt = DateTime.Now;
            currentAccount.UpdatedAt = DateTime.Now;

            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                _context.Add(currentAccount);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cari hesap başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AccountTypes = Enum.GetValues(typeof(AccountType))
               .Cast<AccountType>()
               .Select(v => new SelectListItem
               {
                   Text = GetDisplayName(v),
                   Value = ((int)v).ToString(),
                   Selected = (int)v == (int)currentAccount.AccountType
               }).ToList();
            return View(currentAccount);
        }

      
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var currentAccount = await _context.CurrentAccounts.FirstOrDefaultAsync(ca => ca.Id == id && ca.UserId == userId);
            if (currentAccount == null) return NotFound();

            ViewBag.AccountTypes = Enum.GetValues(typeof(AccountType))
               .Cast<AccountType>()
               .Select(v => new SelectListItem
               {
                   Text = GetDisplayName(v),
                   Value = ((int)v).ToString(),
                   Selected = (int)v == (int)currentAccount.AccountType
               }).ToList();
            return View(currentAccount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,AccountType,TaxNumber,IdentificationNumber,Address,PhoneNumber,Email,Notes")] CurrentAccount currentAccount)
        {
            if (id != currentAccount.Id) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
       
            var existingAccount = await _context.CurrentAccounts.AsNoTracking().FirstOrDefaultAsync(ca => ca.Id == id && ca.UserId == userId);
            if (existingAccount == null) return NotFound();

            currentAccount.UserId = existingAccount.UserId; 
            currentAccount.CreatedAt = existingAccount.CreatedAt; 
            currentAccount.UpdatedAt = DateTime.Now;

            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(currentAccount);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cari hesap başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CurrentAccountExists(currentAccount.Id, userId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AccountTypes = Enum.GetValues(typeof(AccountType))
               .Cast<AccountType>()
               .Select(v => new SelectListItem
               {
                   Text = GetDisplayName(v),
                   Value = ((int)v).ToString(),
                   Selected = (int)v == (int)currentAccount.AccountType
               }).ToList();
            return View(currentAccount);
        }

       
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var currentAccount = await _context.CurrentAccounts
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (currentAccount == null) return NotFound();

            return View(currentAccount);
        }

       
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentAccount = await _context.CurrentAccounts.FirstOrDefaultAsync(ca => ca.Id == id && ca.UserId == userId);
            if (currentAccount != null)
            {
             
                var transactionsExist = await _context.Transactions.AnyAsync(t => t.UserId == userId && t.PayeeOrPayer == currentAccount.Name);
                if (transactionsExist)
                {
                    TempData["ErrorMessage"] = "Bu cari hesaba ait işlemler bulunduğu için silinemez. Önce işlemleri silin veya başka bir cariye aktarın.";
                    return RedirectToAction(nameof(Index));
                }

                _context.CurrentAccounts.Remove(currentAccount);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cari hesap başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CurrentAccountExists(int id, string userId)
        {
            return _context.CurrentAccounts.Any(e => e.Id == id && e.UserId == userId);
        }

      
        public static string GetDisplayName(Enum enumValue)
        {
            return enumValue.GetType()?
                            .GetMember(enumValue.ToString())?
                            .First()?
                            .GetCustomAttribute<DisplayAttribute>()?
                            .GetName() ?? enumValue.ToString();
        }
    }
}