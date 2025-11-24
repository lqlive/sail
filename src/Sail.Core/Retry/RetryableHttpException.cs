namespace Sail.Core.Retry;

public class RetryableHttpException : HttpRequestException
{
    public new int StatusCode { get; }

    public RetryableHttpException(int statusCode) 
        : base($"Request failed with status code {statusCode}")
    {
        StatusCode = statusCode;
    }
}

