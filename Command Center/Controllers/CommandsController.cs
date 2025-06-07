using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Command_Center.Controllers
{
    [ApiController]
    [Route("commands")]
    public class CommandsController : ControllerBase
    {
        public class CommandRequest
        {
            public required string Path { get; set; }
            public required List<string> Commands { get; set; }
        }

        [HttpPost("run")]
        public IActionResult RunCommandsInPath([FromBody] CommandRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Path) || request.Commands == null || request.Commands.Count == 0)
            {
                return BadRequest("Missing path or commands.");
            }

            string output = "";
            string error = "";

            foreach (var cmd in request.Commands)
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"cd '{request.Path}' && {cmd}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false
                    }
                };

                process.Start();
                output += $"$ {cmd}\n" + process.StandardOutput.ReadToEnd();
                error += process.StandardError.ReadToEnd();
                process.WaitForExit();
            }

            return Ok(new
            {
                success = true,
                output,
                error
            });
        }
    }
}
