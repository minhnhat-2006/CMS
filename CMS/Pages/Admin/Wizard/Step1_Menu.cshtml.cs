using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq; // Thêm thư viện này nếu cần
using System.Threading.Tasks;
using YourProjectName.Models;

namespace CMS.Pages.Admin.Wizard
{
    [Authorize]
    public class Step1_MenuModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public Step1_MenuModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public NavigationMenu Menu { get; set; } = new NavigationMenu();

        [BindProperty]
        public int? TargetParentId { get; set; }

        [BindProperty]
        public string SubmitAction { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public int MenuType { get; set; } = 1;

        public string? ParentName { get; set; }

        public async Task<IActionResult> OnGetAsync(int? parentId, int type = 1)
        {
            MenuType = type;
            TargetParentId = parentId;

            if (parentId.HasValue && parentId.Value > 0)
            {
                var parent = await _context.NavigationMenus.FindAsync(parentId.Value);
                ParentName = parent?.Name;
            }

            Menu.IsVisible = true;
            Menu.DisplayOrder = 1;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Menu.Parent");
            ModelState.Remove("Menu.Children");
            ModelState.Remove("Menu.LinkedPage");
            ModelState.Remove("Menu.SidebarItems");
            ModelState.Remove("Menu.ChuyenMuc");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Ensure correct handling of nullable TargetParentId when assigning ParentId
            Menu.ParentId = (TargetParentId.HasValue && TargetParentId.Value > 0) ? TargetParentId : null;

            // Hàm tạo Slug linh hoạt từ tên Menu (Xóa dấu cách, đổi chữ thường)
            var slug = (Menu.Name ?? string.Empty).Replace(" ", "-").ToLowerInvariant();

            // Lấy thông tin Menu Cha (nếu có)
            NavigationMenu? parentMenu = null;
            if (Menu.ParentId.HasValue)
            {
                parentMenu = await _context.NavigationMenus.FindAsync(Menu.ParentId.Value);
            }

            // ==================================================================
            // 🔥 LOGIC MỚI: TỰ ĐỘNG ÉP KIỂU VÀ KẾ THỪA URL THÔNG MINH
            // ==================================================================

            // TỰ ĐỘNG ÉP KIỂU: Nếu mục cha là Chuyên mục, mục con tạo ra tự động làm Hub con
            if (parentMenu != null && parentMenu.ChuyenMucId.HasValue)
            {
                MenuType = 2;
            }

            if (MenuType == 2)
            {
                if (parentMenu != null && parentMenu.ChuyenMucId.HasValue)
                {
                    // [TRƯỜNG HỢP 1]: LÀ MỤC CON
                    // Kế thừa ID Chuyên Mục từ cha
                    Menu.ChuyenMucId = parentMenu.ChuyenMucId;

                    // Tạo URL con kế thừa URL cha (VD: /thong-bao/nha-truong)
                    string parentPath = !string.IsNullOrEmpty(parentMenu.Url) && parentMenu.Url != "#" ? parentMenu.Url.TrimEnd('/') : "";
                    Menu.Url = $"{parentPath}/{slug}";
                }
                else
                {
                    // [TRƯỜNG HỢP 2]: LÀ MỤC CHA GỐC
                    // Phải tạo mới hoàn toàn ChuyenMuc để có ID độc lập
                    var newChuyenMuc = new ChuyenMuc
                    {
                        Name = Menu.Name,
                        Slug = slug
                    };
                    _context.ChuyenMucs.Add(newChuyenMuc);
                    await _context.SaveChangesAsync(); // Lưu để lấy ID thật

                    Menu.ChuyenMucId = newChuyenMuc.Id;
                    Menu.Url = $"/{slug}";
                }
            }
            else if (MenuType == 1)
            {
                // [TRƯỜNG HỢP 3]: LÀ TRANG TĨNH ĐỘC LẬP
                if (parentMenu != null && parentMenu.ChuyenMucId.HasValue)
                {
                    Menu.ChuyenMucId = parentMenu.ChuyenMucId;
                }
                Menu.Url = "#"; // Đánh dấu là trang tĩnh
            }

            // LƯU MENU VÀO DATABASE
            _context.NavigationMenus.Add(Menu);
            await _context.SaveChangesAsync();

            // ------------------------------------------------------------------
            // ĐIỀU HƯỚNG SAU KHI LƯU
            // ------------------------------------------------------------------
            if (SubmitAction == "sibling")
            {
                return RedirectToPage("./Step1_Menu", new { parentId = TargetParentId, type = MenuType });
            }
            else if (SubmitAction == "child")
            {
                return RedirectToPage("./Step1_Menu", new { parentId = Menu.Id, type = MenuType });
            }

            return RedirectToPage("./Step2_ArticleModel", new
            {
                menuId = Menu.Id,
                chuyenMucId = MenuType == 2 ? Menu.ChuyenMucId : null,
                type = MenuType
            });
        }
    }
}