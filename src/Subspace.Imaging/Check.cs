// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Subspace" file="Check.cs">
//   Copyright (c) Subspace. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Subspace.Imaging
{
    using System;

    /// <summary>
    ///     Contains methods that perform condition checking.
    /// </summary>
    internal static class Check
    {
        /// <summary>
        ///     Throws an exception of the specified type if the specified condition is false.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="condition">The condition.</param>
        /// <param name="argumentName">The name of the argument, if applicable for the specified exception.</param>
        /// <param name="message">The message.</param>
        internal static void Require<TException>(bool condition, string argumentName = null, string message = null)
            where TException : Exception, new()
        {
            if (!condition)
            {
                if (argumentName == null)
                {
                    throw (Exception)Activator.CreateInstance(typeof(TException), message ?? string.Empty);
                }

                throw (Exception)Activator.CreateInstance(typeof(TException), message ?? string.Empty, argumentName);
            }
        }
    }
}
