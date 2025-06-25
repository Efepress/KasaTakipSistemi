
using KasaTakipSistemi.Data;
using KasaTakipSistemi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KasaTakipSistemi.ViewComponents
{
    public class AccessibleSafesDropdownViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccessibleSafesDropdownViewComponent(ApplicationDbContext context,
                                                UserManager<ApplicationUser> userManager,
                                                IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new AccessibleSafesDropdownViewModel();
            var httpCtx = _httpContextAccessor.HttpContext;
            var user = httpCtx?.User;

            if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
            {
                var currentUserId = _userManager.GetUserId(user);
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    var ownedSafes = await _context.Safes
                                        .Where(s => s.UserId == currentUserId)
                                        .Select(s => new { s.Id, s.Name })
                                        .ToListAsync();
                    var authorizedSafesData = await _context.SafeUsers
                                                    .Where(su => su.ApplicationUserId == currentUserId && su.IsActive && su.Safe != null)
                                                    .Select(su => new { Id = su.SafeId, Name = su.Safe!.Name })
                                                    .ToListAsync();
                    var combinedSafes = ownedSafes.Concat(authorizedSafesData)
                                                .GroupBy(s => s.Id).Select(g => g.First())
                                                .OrderBy(s => s.Name).ToList();
                    model.AccessibleSafes = combinedSafes.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();

                    var selectedSafeIdString = httpCtx?.Session.GetString("SelectedSafeId");
                    if (int.TryParse(selectedSafeIdString, out int selectedId))
                    {
                        model.SelectedSafeId = selectedId;
                    }
                    else if (model.AccessibleSafes.Any()) 
                    {
                        model.SelectedSafeId = int.Parse(model.AccessibleSafes.First().Value);
                  
                    }
                }
            }
            return View(model); 
        }
    }

    public class AccessibleSafesDropdownViewModel
    {
        public List<SelectListItem> AccessibleSafes { get; set; } = new List<SelectListItem>();
        public int SelectedSafeId { get; set; }
    }
}