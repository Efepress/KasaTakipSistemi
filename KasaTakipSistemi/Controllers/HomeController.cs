using KasaTakipSistemi.Data;
using KasaTakipSistemi.Models;
using KasaTakipSistemi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;

namespace KasaTakipSistemi.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

      
        private async Task<List<Safe>> GetAccessibleSafesAsync(string userId)
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

     
        private async Task<Safe?> GetValidSelectedSafeAsync(string userId)
        {
            var accessibleSafes = await GetAccessibleSafesAsync(userId);
            if (!accessibleSafes.Any()) return null;

            var selectedSafeIdString = HttpContext.Session.GetString("SelectedSafeId");
            if (int.TryParse(selectedSafeIdString, out int sessionId) && accessibleSafes.Any(s => s.Id == sessionId))
            {
                return accessibleSafes.First(s => s.Id == sessionId);
            }

            var firstSafe = accessibleSafes.First();
            HttpContext.Session.SetString("SelectedSafeId", firstSafe.Id.ToString());
            HttpContext.Session.SetString("SelectedSafeName", firstSafe.Name);
            return firstSafe;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var accessibleSafes = await GetAccessibleSafesAsync(user.Id);
            var selectedSafe = await GetValidSelectedSafeAsync(user.Id);

            var viewModel = new DashboardViewModel
            {
                CurrentUserName = user.FullName ?? user.UserName,
                SelectedSafeName = selectedSafe?.Name ?? "Kasa Seçilmedi",
                UserSafes = accessibleSafes 
            };

            if (selectedSafe != null)
            {
                var transactions = await _context.Transactions
                    .Where(t => t.SafeId == selectedSafe.Id)
                    .Include(t => t.Currency)
                    .ToListAsync();

                var allCurrencies = await _context.Currencies.ToListAsync();

               
                viewModel.SafeBalances = allCurrencies.Select(curr => new SafeBalanceViewModel
                {
                    CurrencyName = curr.Name,
                    Symbol = curr.Symbol,
                    TotalAmount = transactions.Where(t => t.CurrencyId == curr.Id)
                                              .Sum(t => t.Type == TransactionType.Gelir ? t.Amount : -t.Amount)
                }).ToList();


                viewModel.RecentTransactions = transactions
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(10)
                    .Select(t => new RecentTransactionViewModel
                    {
                        Description = t.Description,
                        Amount = t.Amount,
                        CurrencySymbol = t.Currency.Symbol,
                        TransactionDate = t.TransactionDate,
                        IsIncome = t.Type == TransactionType.Gelir,
                        PayeeOrPayer = t.PayeeOrPayer
                    }).ToList();

                var anlikAy = DateTime.Now.Month;
                var anlikYil = DateTime.Now.Year;
                var tlCurrencyId = (await _context.Currencies.FirstOrDefaultAsync(c => c.Name.Contains("Türk Lirası")))?.Id ?? 1;

                var monthlyTransactionsTl = transactions
                    .Where(t => t.TransactionDate.Month == anlikAy && t.TransactionDate.Year == anlikYil && t.CurrencyId == tlCurrencyId)
                    .ToList();

                viewModel.MonthlyIncomeTl = monthlyTransactionsTl.Where(t => t.Type == TransactionType.Gelir).Sum(t => t.Amount);
                viewModel.MonthlyExpenseTl = monthlyTransactionsTl.Where(t => t.Type == TransactionType.Gider).Sum(t => t.Amount);

        
                viewModel.MonthlyChartData = new MonthlyChartDataViewModel
                {
                    Gelirler = monthlyTransactionsTl
                                .Where(t => t.Type == TransactionType.Gelir)
                                .GroupBy(t => t.TransactionDate.Day)
                                .Select(g => new Tuple<int, decimal>(g.Key, g.Sum(t => t.Amount)))
                                .ToList(),
                    Giderler = monthlyTransactionsTl
                                .Where(t => t.Type == TransactionType.Gider)
                                .GroupBy(t => t.TransactionDate.Day)
                                .Select(g => new Tuple<int, decimal>(g.Key, g.Sum(t => t.Amount)))
                                .ToList()
                };
            }
            else if (!accessibleSafes.Any())
            {
                TempData["InfoMessage"] = "Henüz bir kasanız veya atanmış bir kasa yetkiniz yok. Lütfen 'Ayarlar > Kasalar' menüsünden yeni bir kasa oluşturun.";
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SelectSafe(int safeId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var accessibleSafes = await GetAccessibleSafesAsync(userId);
            var selectedSafe = accessibleSafes.FirstOrDefault(s => s.Id == safeId);

            if (selectedSafe != null)
            {
                HttpContext.Session.SetString("SelectedSafeId", safeId.ToString());
                HttpContext.Session.SetString("SelectedSafeName", selectedSafe.Name);
            }
            else
            {
                TempData["ErrorMessage"] = "Seçilen kasa üzerinde yetkiniz bulunmamaktadır.";
                HttpContext.Session.Remove("SelectedSafeId");
                HttpContext.Session.Remove("SelectedSafeName");
            }
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}