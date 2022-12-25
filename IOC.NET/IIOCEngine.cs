using System;
using System.Collections.Generic;
using System.Text;

namespace IOC.NET
{
    /// <summary>
    /// IOC Engin
    /// </summary>
    public interface IIOCEngine
    {
        /// <summary>
        /// Get specific type of service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Resolve<T>() where T : class;

        /// <summary>
        /// Get specific type of service
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        object Resolve(Type type);

        /// <summary>
        /// Get specific type of services
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<T> ResolveAll<T>();

        /// <summary>
        ///  Get specific type of unregistered services
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        object ResolveUnregistered(Type type);
    }
}
