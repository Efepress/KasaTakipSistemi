// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using KasaTakipSistemi.Models; // ApplicationUser için
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KasaTakipSistemi.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Display(Name = "Kullanıcı Adı (E-posta)")] // Etiketi güncelledim
        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            // ----- YENİ EKLENEN ALAN -----
            [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
            [StringLength(100, ErrorMessage = "{0} alanı en az {2} en fazla {1} karakter uzunluğunda olmalıdır.", MinimumLength = 3)]
            [Display(Name = "Ad Soyad")]
            public string FullName { get; set; }
            // ----- YENİ EKLENEN ALAN BİTTİ -----

            [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
            [Display(Name = "Telefon Numarası")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var fullName = user.FullName; // Doğrudan kullanıcı nesnesinden al

            Username = userName;

            Input = new InputModel
            {
                FullName = fullName, // Yüklerken ata
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            ViewData["ActivePageManageNav"] = "Profil"; // ManageNav için aktif sayfa bilgisi
            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            ViewData["ActivePageManageNav"] = "Profil"; // ManageNav için aktif sayfa bilgisi (Post sonrası da gerekli)

            if (!ModelState.IsValid)
            {
                await LoadAsync(user); // Hata durumunda formu eski değerlerle doldur
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Telefon numarası ayarlanırken beklenmedik bir hata oluştu.";
                    return RedirectToPage();
                }
            }

            // ----- FullName GÜNCELLEME -----
            if (Input.FullName != user.FullName)
            {
                user.FullName = Input.FullName;
                var updateResult = await _userManager.UpdateAsync(user); // Kullanıcıyı güncelle
                if (!updateResult.Succeeded)
                {
                    StatusMessage = "Ad Soyad güncellenirken beklenmedik bir hata oluştu.";
                    // Diğer hataları da ModelState'e ekleyebiliriz:
                    // foreach (var error in updateResult.Errors) { ModelState.AddModelError(string.Empty, error.Description); }
                    return RedirectToPage();
                }
            }
            // ----- FullName GÜNCELLEME BİTTİ -----


            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Profiliniz güncellendi.";
            return RedirectToPage();
        }
    }
}