using FinGuard.Application.Commons.Interfaces;
using FinGuard.Application.Features.TransactionFiles.Commands.CreateTransactionFile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinGuard.Api.Controllers
{
    [Route("api/transaction-files")]
    [ApiController]
    [Authorize]
    public class TransactionFilesController : ControllerBase
    {
        private readonly IFileStorage _fileStorage;
        private readonly ISender _mediator;

        public TransactionFilesController(IFileStorage fileStorage, ISender mediator)
        {
            _fileStorage = fileStorage;
            _mediator = mediator;
        }

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

        [HttpPost]
        public async Task<IActionResult> Upload(
            IFormFile file,
            CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var extension = Path.GetExtension(file.FileName);
            var storedFileName = $"{Guid.NewGuid()}{extension}";

            string? storagePath = null;

            try
            {
                await using var stream = file.OpenReadStream();

                storagePath = await _fileStorage.SaveAsync(
                    stream,
                    storedFileName,
                    cancellationToken);

                var command = new CreateTransactionFileCommand(
                    file.FileName,
                    storedFileName,
                    storagePath,
                    file.Length);

                var id = await _mediator.Send(command, cancellationToken);

                return Ok(id);
            }
            catch
            {
                if (storagePath is not null)
                {
                    await _fileStorage.DeleteAsync(
                        storagePath,
                        cancellationToken);
                }

                throw;
            }
        }
    }
}
