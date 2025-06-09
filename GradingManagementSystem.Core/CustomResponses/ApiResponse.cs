namespace GradingManagementSystem.Core.CustomResponses
{
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public object? Data { get; set; }

        public ApiResponse()
        {
        }
        public ApiResponse(int statusCode, string? message = null, object? data = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageForStatusCode(statusCode);
            Data = data;
        }

        private static string GetDefaultMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                100 => "Continue",
                200 => "OK",
                201 => "Created",
                204 => "No Content",
                400 => "Bad Request - The server could not understand the request due to invalid syntax.", // Validation errors
                401 => "Unauthorized - You are not authorized to access this resource.",
                403 => "Forbidden - You do not have permission to access this resource.",
                404 => "Not Found - The requested resource could not be found.",
                500 => "Internal Server Error - An unexpected error occurred on the server.",
                502 => "Bad Gateway - The server received an invalid response from an upstream server.",
                503 => "Service Unavailable - The server is temporarily unable to handle the request.",
                _ => "An unexpected error occurred."
            };
        }
    }
}
