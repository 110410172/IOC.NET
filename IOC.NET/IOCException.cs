using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace IOC.NET
{
	/// <summary>
	/// IOC Exception
	/// </summary>
	[Serializable]
    public class IOCException : Exception
    {
		/// <summary>
		/// CTOR
		/// </summary>
		internal IOCException()
		{
		}

		/// <summary>
		/// CTOR
		/// </summary>
		/// <param name="message"></param>
		internal IOCException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// CTOR
		/// </summary>
		/// <param name="messageFormat"></param>
		/// <param name="args"></param>
		internal IOCException(string messageFormat, params object[] args)
			: base(string.Format(messageFormat, args))
		{
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		internal IOCException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		internal IOCException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

	}
}
