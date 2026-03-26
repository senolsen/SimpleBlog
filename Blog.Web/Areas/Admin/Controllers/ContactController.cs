using Blog.Core.Entities;
using Blog.Service.Abstract;
using Blog.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editör")]
public class ContactController : Controller
{
    private readonly IGenericService<Contact> _contactService;

    public ContactController(IGenericService<Contact> contactService)
    {
        _contactService = contactService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Veritabanındaki ilk (ve tek) iletişim kaydını getir
        var contacts = await _contactService.WhereAsync(c => !c.IsDeleted);
        var contact = contacts.FirstOrDefault();

        var model = new ContactUpdateViewModel();

        // Eğer daha önce kayıt girilmişse modeli doldur (Güncelleme modu)
        if (contact != null)
        {
            model.Id = contact.Id;
            model.Phone = contact.Phone;
            model.Email = contact.Email;
            model.Address = contact.Address;
            model.MapIframeSrc = contact.MapIframeSrc;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactUpdateViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (model.Id == 0)
            {
                // İlk defa ekleniyorsa
                var newContact = new Contact
                {
                    Phone = model.Phone,
                    Email = model.Email,
                    Address = model.Address,
                    MapIframeSrc = model.MapIframeSrc
                };
                await _contactService.AddAsync(newContact);
                TempData["SuccessMessage"] = "İletişim bilgileri başarıyla eklendi!";
            }
            else
            {
                // Mevcut kayıt güncelleniyorsa
                var existingContact = await _contactService.GetByIdAsync(model.Id);
                if (existingContact != null)
                {
                    existingContact.Phone = model.Phone;
                    existingContact.Email = model.Email;
                    existingContact.Address = model.Address;
                    existingContact.MapIframeSrc = model.MapIframeSrc;

                    await _contactService.UpdateAsync(existingContact);
                    TempData["SuccessMessage"] = "İletişim bilgileri başarıyla güncellendi!";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }
}