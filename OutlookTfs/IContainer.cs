using System;
using System.Collections.Generic;

namespace OutlookTfs
{
    public interface IContainer
    {
        Dictionary<string, object> Configuration { get; set; }
        SimpleContainer Register<T>(Func<SimpleContainer, object> creator);
        SimpleContainer RegisterSingle<T>(T instance);
        T Create<T>();
    }
}