using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KasaTakipSistemi.Data;
using KasaTakipSistemi.Models;
using Microsoft.AspNetCore.Authorization; 

namespace KasaTakipSistemi.Controllers
{
    [Authorize] 
    public class CurrenciesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CurrenciesController(ApplicationDbContext context)
        {
            _context = context;
        }

      
        public async Task<IActionResult> Index()
        {
            return View(await _context.Currencies.ToListAsync());
        }

        
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currency = await _context.Currencies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (currency == null)
            {
                return NotFound();
            }

            return View(currency);
        }

      
        public IActionResult Create()
        {
            return View();
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Symbol")] Currency currency) 
        {
          
            ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                _context.Add(currency);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Para birimi başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            return View(currency);
        }

       
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currency = await _context.Currencies.FindAsync(id);
            if (currency == null)
            {
                return NotFound();
            }
            return View(currency);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Symbol")] Currency currency)
        {
            if (id != currency.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(currency);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Para birimi başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CurrencyExists(currency.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(currency);
        }

        
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currency = await _context.Currencies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (currency == null)
            {
                return NotFound();
            }

            return View(currency);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currency = await _context.Currencies.FindAsync(id);
            if (currency != null)
            {
              
                try
                {
                    _context.Currencies.Remove(currency);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Para birimi başarıyla silindi.";
                }
                catch (DbUpdateException ex) 
                {
                   
                    TempData["ErrorMessage"] = "Bu para birimi başka kayıtlarla ilişkili olduğu için silinemedi. Hata: " + ex.InnerException?.Message;
               
                    return RedirectToAction(nameof(Index));
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CurrencyExists(int id)
        {
            return _context.Currencies.Any(e => e.Id == id);
        }
    }
}