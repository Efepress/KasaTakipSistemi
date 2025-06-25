using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KasaTakipSistemi.Data;
using KasaTakipSistemi.Models;
using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; 
using Microsoft.AspNetCore.Identity; 

namespace KasaTakipSistemi.Controllers
{
    [Authorize]
    public class SafesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; 

        public SafesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var safes = await _context.Safes
                .Where(s => s.UserId == userId)
                .ToListAsync();

            return View(safes);
        }

  
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var safe = await _context.Safes
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (safe == null)
                return NotFound();

            return View(safe);
        }

        
        public IActionResult Create()
        {
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Safe safe)
        {
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

           
            if (string.IsNullOrEmpty(userId))
            {
            
                ModelState.AddModelError("", "Kasa oluşturmak için giriş yapmalısınız.");
               
                return View(safe); 
            }

           
            safe.UserId = userId;

            
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Transactions");


            if (ModelState.IsValid) 
            {
                _context.Add(safe);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kasa başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            return View(safe); 
        }


     
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var safe = await _context.Safes
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (safe == null)
                return NotFound();

            return View(safe);
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Safe safe)
        {
            if (id != safe.Id)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            var existingSafe = await _context.Safes
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (existingSafe == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    existingSafe.Name = safe.Name;
                    _context.Update(existingSafe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SafeExists(safe.Id, userId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(safe);
        }

   
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var safe = await _context.Safes
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (safe == null)
                return NotFound();

            return View(safe);
        }

      
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var safe = await _context.Safes
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (safe == null)
                return NotFound();

            _context.Safes.Remove(safe);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SafeExists(int id, string userId)
        {
            return _context.Safes.Any(e => e.Id == id && e.UserId == userId);
        }
    }
}
