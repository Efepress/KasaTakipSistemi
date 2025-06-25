using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KasaTakipSistemi.Data;
using KasaTakipSistemi.Models;
using KasaTakipSistemi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Globalization; 

namespace KasaTakipSistemi.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task PopulateFilterDropdowns(ReportFilterViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) 
            {
                model.AccountList = new List<SelectListItem> { new SelectListItem { Value = "", Text = "Tümü" } };
                model.TransactionTypeList = new List<SelectListItem>(); 
                return;
            }

          
            var currentAccounts = await _context.CurrentAccounts
                .Where(ca => ca.UserId == userId)
                .OrderBy(ca => ca.Name)
                .Select(ca => new SelectListItem { Value = ca.Id.ToString(), Text = ca.Name + " (Cari)" })
                .ToListAsync();

          
            var employees = await _context.Employees
                .Where(e => e.UserId == userId && e.IsActive) 
                .OrderBy(e => e.FullName)
                .Select(e => new SelectListItem { Value = "P-" + e.Id.ToString(), Text = e.FullName + " (Personel)" })
                .ToListAsync();

            model.AccountList = new List<SelectListItem> { new SelectListItem { Value = "", Text = "Tümü" } };
            model.AccountList.AddRange(currentAccounts);
            model.AccountList.AddRange(employees);


            model.TransactionTypeList = Enum.GetValues(typeof(ReportTransactionType))
                .Cast<ReportTransactionType>()
                .Select(v => new SelectListItem
                {
                    Text = CurrentAccountsController.GetDisplayName(v), 
                    Value = ((int)v).ToString()
                }).ToList();
        }


        
        public async Task<IActionResult> Index(ReportFilterViewModel? filterModel = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Raporları görüntülemek için giriş yapmalısınız.");

            var model = filterModel ?? new ReportFilterViewModel();

      
            if (model.EndDate == DateTime.MinValue) model.EndDate = DateTime.Now.Date;
            if (model.StartDate == DateTime.MinValue) model.StartDate = model.EndDate.AddMonths(-1);

            await PopulateFilterDropdowns(model);

          
            if (Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) && filterModel != null)
            {
            
            }
            else if (filterModel == null) 
            {
                model.HasResults = false;
            }
           

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateReport(ReportFilterViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Rapor oluşturmak için giriş yapmalısınız.");

            
            if (model.StartDate > model.EndDate)
            {
                ModelState.AddModelError("StartDate", "Başlangıç tarihi bitiş tarihinden sonra olamaz.");
            }

            
            if (!ModelState.IsValid)
            {
                await PopulateFilterDropdowns(model);
                model.HasResults = false; 
                return View("Index", model);
            }

            var endDateWithTime = model.EndDate.Date.AddDays(1).AddTicks(-1);

            var query = _context.Transactions
                                .Where(t => t.UserId == userId &&
                                            t.TransactionDate >= model.StartDate.Date &&
                                            t.TransactionDate <= endDateWithTime)
                                .Include(t => t.Currency)
                                .Include(t => t.Safe)
                                .OrderBy(t => t.TransactionDate)
                                .AsQueryable();

            if (model.TransactionType == ReportTransactionType.Gelirler)
            {
                query = query.Where(t => t.Type == TransactionType.Gelir);
            }
            else if (model.TransactionType == ReportTransactionType.Giderler)
            {
                query = query.Where(t => t.Type == TransactionType.Gider);
            }

           
            if (!string.IsNullOrEmpty(model.AccountId))
            {
                if (model.AccountId.StartsWith("P-")) 
                {
                    if (int.TryParse(model.AccountId.Substring(2), out int employeeId))
                    {
                        var employee = await _context.Employees.FindAsync(employeeId);
                        if (employee != null)
                        {
                            query = query.Where(t => t.PayeeOrPayer == employee.FullName);
                        }
                    }
                }
                else 
                {
                    if (int.TryParse(model.AccountId, out int currentAccountId))
                    {
                        var currentAccount = await _context.CurrentAccounts.FindAsync(currentAccountId);
                        if (currentAccount != null)
                        {
                            query = query.Where(t => t.PayeeOrPayer == currentAccount.Name);
                        }
                    }
                }
            }

            var results = await query.ToListAsync();

            model.ReportResults = results.Select(t => new TransactionReportViewModel
            {
                TransactionDate = t.TransactionDate,
                Description = t.Description,
                PayeeOrPayer = t.PayeeOrPayer,
                IncomeAmount = t.Type == TransactionType.Gelir ? t.Amount : (decimal?)null,
                ExpenseAmount = t.Type == TransactionType.Gider ? t.Amount : (decimal?)null,
                CurrencySymbol = t.Currency?.Symbol ?? "-",
                SafeName = t.Safe?.Name ?? "-",
                OriginalTransactionType = t.Type
            }).ToList();

            
            if (model.ReportResults.Any())
            {
                
                var mostFrequentCurrency = model.ReportResults
                                            .GroupBy(r => r.CurrencySymbol)
                                            .OrderByDescending(g => g.Count())
                                            .Select(g => g.Key)
                                            .FirstOrDefault();

                if (!string.IsNullOrEmpty(mostFrequentCurrency))
                {
                    model.TotalIncome = model.ReportResults
                                            .Where(r => r.CurrencySymbol == mostFrequentCurrency && r.IncomeAmount.HasValue)
                                            .Sum(r => r.IncomeAmount.Value);
                    model.TotalExpense = model.ReportResults
                                             .Where(r => r.CurrencySymbol == mostFrequentCurrency && r.ExpenseAmount.HasValue)
                                             .Sum(r => r.ExpenseAmount.Value);
                    model.SelectedCurrencySymbolForTotals = mostFrequentCurrency;
                }
                else 
                {
                    model.TotalIncome = 0;
                    model.TotalExpense = 0;
                    model.SelectedCurrencySymbolForTotals = "-";
                }
            }
            else
            {
                model.TotalIncome = 0;
                model.TotalExpense = 0;
                model.SelectedCurrencySymbolForTotals = "-";
            }

            model.HasResults = true;
            await PopulateFilterDropdowns(model);
            return View("Index", model);
        }
    }
}