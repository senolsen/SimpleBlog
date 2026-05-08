using Blog.Core.Entities;
using Blog.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Areas.Admin.ViewComponents;

public class UnreadMessageCountViewComponent : ViewComponent
{
    private readonly IGenericService<ContactMessage> _messageService;

    public UnreadMessageCountViewComponent(IGenericService<ContactMessage> messageService)
    {
        _messageService = messageService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Okunmamış ve silinmemiş mesajların sayısını al
        var count = (await _messageService.WhereAsync(m => !m.IsRead && !m.IsDeleted)).Count();

        // Sayıyı View'a gönder
        return View(count);
    }
}