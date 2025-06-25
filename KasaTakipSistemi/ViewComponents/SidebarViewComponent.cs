using KasaTakipSistemi.Data;
using KasaTakipSistemi.Models;
using KasaTakipSistemi.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KasaTakipSistemi.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SidebarViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = ((ClaimsPrincipal)User).FindFirstValue(ClaimTypes.NameIdentifier);
            var viewModel = new SidebarViewModel();

            if (!string.IsNullOrEmpty(userId))
            {
                var accessibleSafes = await GetAccessibleSafesAsync(userId);
                viewModel.AccessibleSafes = accessibleSafes;
                viewModel.SelectedSafeId = GetSelectedSafeId(accessibleSafes);
            }

            return View(viewModel);
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

        private int GetSelectedSafeId(List<Safe> accessibleSafes)
        {
            if (!accessibleSafes.Any()) return 0;

            var selectedIdStr = HttpContext.Session.GetString("SelectedSafeId");
            if (int.TryParse(selectedIdStr, out int selectedId) && accessibleSafes.Any(s => s.Id == selectedId))
            {
                return selectedId;
            }

      
            var firstSafe = accessibleSafes.First();
            HttpContext.Session.SetString("SelectedSafeId", firstSafe.Id.ToString());
            HttpContext.Session.SetString("SelectedSafeName", firstSafe.Name);
            return firstSafe.Id;
        }
    }

    public class SidebarViewModel
    {
        public List<Safe> AccessibleSafes { get; set; } = new List<Safe>();
        public int SelectedSafeId { get; set; }
    }
}