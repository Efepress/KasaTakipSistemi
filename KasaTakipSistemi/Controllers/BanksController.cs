
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KasaTakipSistemi.Data;
using KasaTakipSistemi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace KasaTakipSistemi.Controllers
{
    [Authorize] 
    public class BanksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BanksController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Banks.OrderBy(b => b.Name).ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Bank bank)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bank);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Banka başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(bank);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var bank = await _context.Banks.FindAsync(id);
            if (bank == null) return NotFound();
            return View(bank);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Bank bank)
        {
            if (id != bank.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bank);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Banka başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Banks.Any(e => e.Id == bank.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(bank);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var bank = await _context.Banks.FirstOrDefaultAsync(m => m.Id == id);
            if (bank == null) return NotFound();
            return View(bank);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bank = await _context.Banks.FindAsync(id);
            if (bank != null)
            {
               
                var hasAccounts = await _context.BankAccounts.AnyAsync(ba => ba.BankId == id);
                if (hasAccounts)
                {
                    TempData["ErrorMessage"] = "Bu bankaya ait hesaplar bulunduğu için silinemez.";
                    return RedirectToAction(nameof(Index));
                }
                _context.Banks.Remove(bank);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Banka başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}