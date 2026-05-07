using Blog.Core.Entities;
using Blog.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editör")]
public class CommentController : Controller
{
    private readonly IGenericService<Comment> _commentService;

    public CommentController(IGenericService<Comment> commentService)
    {
        _commentService = commentService;
    }

    public async Task<IActionResult> Index()
    {
        // Admin panelinde silinmemiş tüm yorumları, en yeni en üstte olacak şekilde getiriyoruz
        // (Gerçek projede burada Include(c => c.Post) ile hangi yazıya yapıldığını da çekeceğiz)
        var comments = await _commentService.WhereAsync(c => !c.IsDeleted);
        return View(comments.OrderByDescending(c => c.CreatedDate));
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int id)
    {
        var comment = await _commentService.GetByIdAsync(id);
        if (comment != null)
        {
            comment.IsApproved = true; // Yorumu onayla ve sitede yayınla
            await _commentService.UpdateAsync(comment);
            TempData["SuccessMessage"] = "Yorum onaylandı ve yayına alındı.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Reject(int id)
    {
        var comment = await _commentService.GetByIdAsync(id);
        if (comment != null)
        {
            comment.IsApproved = false; // Yorumun onayını kaldır
            await _commentService.UpdateAsync(comment);
            TempData["SuccessMessage"] = "Yorum yayından kaldırıldı.";
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var comment = await _commentService.GetByIdAsync(id);
        if (comment != null)
        {
            await _commentService.RemoveAsync(comment);
            TempData["SuccessMessage"] = "Yorum silindi.";
        }
        return RedirectToAction(nameof(Index));
    }
}