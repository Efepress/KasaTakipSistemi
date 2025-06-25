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

namespace KasaTakipSistemi.Controllers
{
    [Authorize] 
    public class SafeAuthorizationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SafeAuthorizationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

       
        public async Task<IActionResult> Index(string searchTerm)
        {
            var currentOwnerUserId = _userManager.GetUserId(User); 
            if (string.IsNullOrEmpty(currentOwnerUserId))
            {
                return Challenge(); 
            }

            var query = _context.SafeUsers
                .Where(su => su.Safe != null && su.Safe.UserId == currentOwnerUserId) 
                .Include(su => su.ApplicationUser) 
                .Include(su => su.Safe)          
                .Select(su => new SafeAuthorizationViewModel
                {
                    UserId = su.ApplicationUserId,
                    SafeId = su.SafeId,
                    UserFullName = su.ApplicationUser != null ? (su.ApplicationUser.FullName ?? su.ApplicationUser.UserName ?? "Bilinmeyen Kullanıcı") : "Kullanıcı Bilgisi Yok",
                    UserEmail = su.ApplicationUser != null ? (su.ApplicationUser.Email ?? "-") : "-",
                    SafeName = su.Safe != null ? su.Safe.Name : "Kasa Bilgisi Yok",
                    IsActive = su.IsActive
                    
                });

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(vm =>
                    (vm.UserFullName != null && vm.UserFullName.Contains(searchTerm)) ||
                    (vm.UserEmail != null && vm.UserEmail.Contains(searchTerm)) ||
                    (vm.SafeName != null && vm.SafeName.Contains(searchTerm)));
            }

            var authorizations = await query.OrderBy(vm => vm.SafeName).ThenBy(vm => vm.UserFullName).ToListAsync();
            ViewBag.SearchTerm = searchTerm;
            return View(authorizations);
        }

        private async Task PopulateAssignDropdownsAsync(AssignSafeAuthorizationViewModel model)
        {
            var currentOwnerUserId = _userManager.GetUserId(User);

        
            model.UserList = await _userManager.Users
                .Where(u => u.Id != currentOwnerUserId) 
                .OrderBy(u => u.FullName ?? u.UserName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = (string.IsNullOrEmpty(u.FullName) ? u.UserName : u.FullName) + (string.IsNullOrEmpty(u.Email) ? "" : $" ({u.Email})")
                }).ToListAsync();

           
            model.SafeList = await _context.Safes
                .Where(s => s.UserId == currentOwnerUserId)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }) 
                .ToListAsync();
        }

   
        public async Task<IActionResult> Assign()
        {
            var model = new AssignSafeAuthorizationViewModel();
            await PopulateAssignDropdownsAsync(model);
            return View(model);
        }

    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignSafeAuthorizationViewModel model)
        {
            var currentOwnerUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentOwnerUserId)) return Unauthorized();

           
            var selectedSafe = await _context.Safes
                .FirstOrDefaultAsync(s => s.Id == model.SelectedSafeId && s.UserId == currentOwnerUserId);

            if (selectedSafe == null)
            {
                ModelState.AddModelError("SelectedSafeId", "Seçilen kasa üzerinde yetki verme hakkınız yok veya kasa bulunamadı.");
            }

          
            var existingAuthorization = await _context.SafeUsers
                .AnyAsync(su => su.ApplicationUserId == model.SelectedUserId && su.SafeId == model.SelectedSafeId);

            if (existingAuthorization)
            {
                ModelState.AddModelError("", "Bu kullanıcıya seçilen kasa için zaten bir yetki tanımlanmış. Mevcut yetkiyi düzenleyebilirsiniz.");
            }

            if (model.SelectedUserId == currentOwnerUserId)
            {
                ModelState.AddModelError("SelectedUserId", "Kasa sahibi kendisine ayrıca yetki atayamaz.");
            }


            if (ModelState.IsValid)
            {
                var safeUser = new SafeUser
                {
                    ApplicationUserId = model.SelectedUserId,
                    SafeId = model.SelectedSafeId,
                    IsActive = model.IsActive,
                    GrantedDate = DateTime.Now
                };
                _context.SafeUsers.Add(safeUser);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kasa yetkisi başarıyla atandı.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateAssignDropdownsAsync(model); 
            return View(model);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string userId, int safeId) 
        {
            var currentOwnerUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentOwnerUserId)) return Unauthorized();

            var safeUser = await _context.SafeUsers
                                    .Include(su => su.Safe) 
                                    .FirstOrDefaultAsync(su => su.ApplicationUserId == userId && su.SafeId == safeId);

            if (safeUser == null)
            {
                TempData["ErrorMessage"] = "Yetki kaydı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

          
            if (safeUser.Safe?.UserId != currentOwnerUserId)
            {
                TempData["ErrorMessage"] = "Bu yetki üzerinde değişiklik yapma hakkınız yok.";
                return RedirectToAction(nameof(Index));
            }

            safeUser.IsActive = !safeUser.IsActive;
            _context.Update(safeUser);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Yetki durumu başarıyla {(safeUser.IsActive ? "aktif" : "pasif")} hale getirildi.";
            return RedirectToAction(nameof(Index));
        }


        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Revoke(string userId, int safeId) 
        {
            var currentOwnerUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentOwnerUserId)) return Unauthorized();

            var safeUser = await _context.SafeUsers
                                    .Include(su => su.Safe)
                                    .FirstOrDefaultAsync(su => su.ApplicationUserId == userId && su.SafeId == safeId);

            if (safeUser == null)
            {
                TempData["ErrorMessage"] = "Yetki kaydı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            if (safeUser.Safe?.UserId != currentOwnerUserId)
            {
                TempData["ErrorMessage"] = "Bu yetkiyi kaldırma hakkınız yok.";
                return RedirectToAction(nameof(Index));
            }

            _context.SafeUsers.Remove(safeUser);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Kasa yetkisi başarıyla kaldırıldı.";
            return RedirectToAction(nameof(Index));
        }
    }
}