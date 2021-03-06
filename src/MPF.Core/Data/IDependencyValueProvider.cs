﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MPF.Data
{
    public interface IDependencyValueProvider
    {
        float Priority { get; }
    }

    public interface IEffectiveValueProvider
    {
        IEffectiveValue ProviderValue(DependencyObject d);
    }

    public interface IEffectiveValueProvider<T> : IEffectiveValueProvider
    {

    }
}
