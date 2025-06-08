using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Command_Center.Controllers
{
    /// <summary>
    /// Controller for executing shell commands in a specified directory via HTTP API.
    /// </summary>
    [ApiController]
    [Route("commands")]
    public class CommandsController : ControllerBase
    {
        /// <summary>
        /// Request model for running commands.
        /// </summary>
        public class CommandRequest
        {
            /// <summary>
            /// The working directory where commands will be executed.
            /// </summary>
            public required string Path { get; set; }
            /// <summary>
            /// List of shell commands to execute in sequence.
            /// </summary>
            public required List<string> Commands { get; set; }
        }

        /// <summary>
        /// Executes the provided shell commands in the specified path.
        /// </summary>
        /// <param name="request">The command request containing path and commands.</param>
        /// <returns>JSON result with success, output, and error fields.</returns>
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
