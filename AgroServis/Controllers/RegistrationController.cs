using AgroServis.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgroServis.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly IWorkerService _workerService;

        public RegistrationController(IWorkerService workerService)
        {
            _workerService = workerService;
        }

        [HttpGet]
        public async Task<IActionResult> Approve(string token)
        {
            var (success, message, firstName) = await _workerService.ApproveRegistrationByTokenAsync(token);

            if (!success)
            {
                return View("ApprovalError", message);
            }

            return View("ApprovalSuccess", firstName);
        }

        [HttpGet]
        public async Task<IActionResult> Reject(string token)
        {
            var (success, message, firstName) = await _workerService.RejectRegistrationByTokenAsync(token);

            if (!success)
            {
                return View("ApprovalError", message);
            }

            return View("RejectionSuccess", firstName);
        }
    }
}