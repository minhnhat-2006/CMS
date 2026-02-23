using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Pages.Admin // Sửa lại namespace này nếu nó báo lỗi nhé
{
    public class CustomizeModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CustomizeModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<TreeFolderDto> TreeData { get; set; } = new();

        public async Task OnGetAsync()
        {
            var allMenus = await _context.NavigationMenus.OrderBy(m => m.DisplayOrder).AsNoTracking().ToListAsync();
            var allArticles = await _context.ContentPages.AsNoTracking().ToListAsync();
            var allSidebars = await _context.SidebarItems.AsNoTracking().ToListAsync();

            var parentMenus = allMenus.Where(m => m.ParentId == null || m.ParentId == 0).ToList();

            foreach (var parent in parentMenus)
            {
                var parentArticle = allArticles.FirstOrDefault(a => a.Id == parent.ContentPageId);
                var parentSidebar = allSidebars.FirstOrDefault(s => s.MenuId == parent.Id);

                var folder = new TreeFolderDto
                {
                    ParentMenu = parent,
                    ArticleTitle = parentArticle?.Title ?? "⚠️ Chưa có bài viết",
                    ArticleId = parentArticle?.Id,
                    SidebarTitle = parentSidebar?.Title ?? "⚠️ Chưa có Sidebar",
                    SidebarId = parentSidebar?.Id,
                    ChildItems = new List<ChildFolderDto>()
                };

                var childMenus = allMenus.Where(m => m.ParentId == parent.Id).ToList();

                foreach (var child in childMenus)
                {
                    var article = allArticles.FirstOrDefault(a => a.Id == child.ContentPageId);
                    var sidebar = allSidebars.FirstOrDefault(s => s.MenuId == child.Id);

                    folder.ChildItems.Add(new ChildFolderDto
                    {
                        ChildMenu = child,
                        ArticleTitle = article?.Title ?? "⚠️ Chưa có bài viết",
                        ArticleId = article?.Id,
                        SidebarTitle = sidebar?.Title ?? "⚠️ Chưa có Sidebar",
                        SidebarId = sidebar?.Id
                    });
                }
                TreeData.Add(folder);
            }
        }
    }

    // ĐÂY CHÍNH LÀ 2 CLASS MÀ VISUAL STUDIO ĐANG BÁO THIẾU
    public class TreeFolderDto
    {
        public NavigationMenu ParentMenu { get; set; } = default!;
        public string ArticleTitle { get; set; } = string.Empty;
        public int? ArticleId { get; set; }
        public string SidebarTitle { get; set; } = string.Empty;
        public int? SidebarId { get; set; }
        public List<ChildFolderDto> ChildItems { get; set; } = new();
    }

    public class ChildFolderDto
    {
        public NavigationMenu ChildMenu { get; set; } = default!;
        public string ArticleTitle { get; set; } = string.Empty;
        public int? ArticleId { get; set; }
        public string SidebarTitle { get; set; } = string.Empty;
        public int? SidebarId { get; set; }
    }
}