using ClashRoyaleProject.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClashRoyaleProject.Web.Pages.Test
{
    public class AddClanModel : PageModel
    {
        private readonly IApplicationService _applicationService;

        public AddClanModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        [BindProperty]
        public string ClanTag { get; set; } = string.Empty;

        public string? Message { get; set; }
        public bool? IsSuccess { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(ClanTag))
            {
                Message = "Please enter a clan tag";
                IsSuccess = false;
                return Page();
            }

            var result = await _applicationService.AddClanAsync(ClanTag);
            
            Message = result.Message;
            IsSuccess = result.Success;

            return Page();
        }
    }
}