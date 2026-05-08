using Blog.Core.Entities;
using Blog.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editör")] // Yazar mesajları göremez
public class ContactMessageController : Controller
{
    private readonly IGenericService<ContactMessage> _messageService;

    public ContactMessageController(IGenericService<ContactMessage> messageService)
    {
        _messageService = messageService;
    }

    public async Task<IActionResult> Index()
    {
        // Silinmemiş mesajları en yenisi en üstte olacak şekilde getir
        var messages = await _messageService.WhereAsync(m => !m.IsDeleted);
        return View(messages.OrderByDescending(m => m.CreatedDate));
    }

    public async Task<IActionResult> Details(int id)
    {
        var message = await _messageService.GetByIdAsync(id);
        if (message == null || message.IsDeleted) return NotFound();

        // Mesaj ilk kez açılıyorsa "Okundu" olarak işaretle ve kaydet
        if (!message.IsRead)
        {
            message.IsRead = true;
            await _messageService.UpdateAsync(message);
        }

        return View(message);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var message = await _messageService.GetByIdAsync(id);
        if (message != null)
        {
            await _messageService.RemoveAsync(message);
            TempData["SuccessMessage"] = "Mesaj başarıyla silindi.";
        }
        return RedirectToAction(nameof(Index));
    }
}