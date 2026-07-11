using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinGuard.Api.Controllers
{
    [Route("api/transaction-files")]
    [ApiController]
    [Authorize]
    public class TransactionFilesController : ControllerBase
    {

        [HttpGet("transaction-template")]
        public IActionResult DownloadTemplate()
        {
            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "templates",
                "Transaction-Template.xlsx"
                );

            if (!System.IO.File.Exists(filePath))
                return NotFound("Template file not found.");

            var bytes = System.IO.File.ReadAllBytes( filePath );

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Transaction-Template.xlsx");
        }

    }
}
