using System;
using System.Collections.Generic;

namespace OutlookTfs
{
    /// <summary>
    /// Simple and ...very simple DI implementaton inspired by @ayende's one
    /// http://ayende.com/blog/2886/building-an-ioc-container-in-15-lines-of-code
    /// </summary>
    public class SimpleContainer : IContainer
    {
        private readonly Dictionary<Type, Func<SimpleContainer, object>> _typeToCreator;

        public SimpleContainer()
        {
            Configuration = new Dictionary<string, object>();
            _typeToCreator = new Dictionary<Type, Func<SimpleContainer, object>>();
        }

        public Dictionary<string, object> Configuration { get; set; }

        public SimpleContainer Register<T>(Func<SimpleContainer, object> creator)
        {
            _typeToCreator.Add(typeof(T), creator);
            return this;
        }
        
        public SimpleContainer RegisterSingle<T>(T instance)
        {
            _typeToCreator.Add(typeof(T), c => instance);
            return this;
        }
        
        public T Create<T>()
        {
            Func<SimpleContainer, object> creator;
            if (!_typeToCreator.TryGetValue(typeof(T), out creator))
            {
                throw new InvalidOperationException("No registration for " + typeof(T));
            }
            return (T) creator.Invoke(this);
        }
    }
}