// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions
{
    /// <summary>
    /// This exception is thrown when the user is not authorized to perform the action.
    /// </summary>
    public class NoAuthorizationException : Exception
    {
        public NoAuthorizationException() : base()
        {
        }

        public NoAuthorizationException(string message) : base(message)
        {
        }

        public NoAuthorizationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}