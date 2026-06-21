namespace Backend.DTOs
{
    /// <summary>
    /// Standard API response wrapper.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the request was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the response data.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Helper method to create a successful response.
        /// </summary>
        public static ApiResponse<T> Ok(T data, string message = "Request processed successfully.")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// Helper method to create a failed response.
        /// </summary>
        public static ApiResponse<T> Fail(string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default
            };
        }
    }
}
