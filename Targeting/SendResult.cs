using System;

namespace Bugtracker.Targeting
{
    /// <summary>
    /// Represents the result of a target send operation with success status and details
    /// </summary>
    public class SendResult
    {
        /// <summary>
        /// Whether the send operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Human-readable message describing the result
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Exception that occurred during the operation, if any
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Optional URL associated with the result (e.g., web interface link for uploaded bugtracks)
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Create a successful result
        /// </summary>
        /// <param name="message">Optional success message</param>
        /// <returns>SendResult indicating success</returns>
        public static SendResult Ok(string message = null)
        {
            return new SendResult
            {
                Success = true,
                Message = message ?? "Operation completed successfully"
            };
        }

        /// <summary>
        /// Create a failed result
        /// </summary>
        /// <param name="message">Error message describing the failure</param>
        /// <param name="ex">Optional exception that caused the failure</param>
        /// <returns>SendResult indicating failure</returns>
        public static SendResult Fail(string message, Exception ex = null)
        {
            return new SendResult
            {
                Success = false,
                Message = message,
                Exception = ex
            };
        }

        /// <summary>
        /// Returns a string representation of this result
        /// </summary>
        public override string ToString()
        {
            if (Success)
                return $"Success: {Message}";
            else
                return $"Failed: {Message}" + (Exception != null ? $" ({Exception.Message})" : "");
        }
    }
}
