using System;

namespace ApiAggregatorAPI.Contracts
{
	public class ApiClientException : Exception
	{
		public ErrorType ErrorType { get; set; }

		public ApiClientException(ErrorType errorType, string message)
			: base(message)
		{
			ErrorType = errorType;
		}
	}

	public enum ErrorType
	{
		Transient = 0,
		Permanent
	}
}
