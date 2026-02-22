using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

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

        // 1. Thực thể Menu sẽ được lưu vào DB
        [BindProperty]
        public NavigationMenu Menu { get; set; } = new NavigationMenu();

        // 2. Chốt chặn ID Cha (Chỉ nhận từ URL, tuyệt đối không cho form HTML sửa đổi)
        [BindProperty]
        public int? TargetParentId { get; set; }

        [BindProperty]
        public string SubmitAction { get; set; } = "";

        public string? ParentName { get; set; }

        public async Task<IActionResult> OnGetAsync(int? parentId)
        {
            // Nhận ID cha từ URL và khóa lại
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
            // Gỡ bỏ validation rác của Entity Framework
            ModelState.Remove("Menu.Parent");
            ModelState.Remove("Menu.Children");
            ModelState.Remove("Menu.LinkedPage");
            ModelState.Remove("Menu.SidebarItems");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Gán ID Cha an toàn vào Menu trước khi lưu
            Menu.ParentId = TargetParentId > 0 ? TargetParentId : null;

            // LƯU VÀO DATABASE
            _context.NavigationMenus.Add(Menu);
            await _context.SaveChangesAsync();
            // Vừa lưu xong, biến Menu.Id sẽ tự động có số ID mới tinh từ PostgreSQL!

            // ------------------------------------------------------------------
            // 🔥 BƯỚC QUAN TRỌNG: TÌM TÊN MENU CHA ĐỂ KẾ THỪA CATEGORY CHO BƯỚC 2
            // ------------------------------------------------------------------
            string rootCategoryName = Menu.Name; // Mặc định: Nếu là Menu Gốc thì lấy tên của chính nó

            if (TargetParentId.HasValue && TargetParentId.Value > 0)
            {
                var parentMenu = await _context.NavigationMenus.FindAsync(TargetParentId.Value);
                if (parentMenu != null)
                {
                    rootCategoryName = parentMenu.Name; // Nếu là Menu con -> Kế thừa tên Menu Cha! (VD: Lấy tên "giới thiệu")
                }
            }

            // ĐIỀU HƯỚNG BẰNG LOGIC CỨNG (KHÔNG THỂ SAI)
            if (SubmitAction == "sibling")
            {
                // TẠO ANH EM: Gọi lại trang này, truyền URL là ID Cha gốc (TargetParentId)
                return RedirectToPage("./Step1_Menu", new { parentId = TargetParentId });
            }
            else if (SubmitAction == "child")
            {
                // TẠO CON: Gọi lại trang này, truyền URL là cái ID vừa mới ra lò (Menu.Id)
                return RedirectToPage("./Step1_Menu", new { parentId = Menu.Id });
            }

            // MẶC ĐỊNH: Qua bước 2 VÀ NÉM THEO TÊN CATEGORY ĐÃ KẾ THỪA
            return RedirectToPage("./Step2_ArticleModel", new { menuId = Menu.Id, categoryName = rootCategoryName });
        }
    }
}