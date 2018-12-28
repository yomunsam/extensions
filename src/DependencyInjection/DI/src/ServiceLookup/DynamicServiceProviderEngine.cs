// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class DynamicServiceProviderEngine : CompiledServiceProviderEngine
    {
        public DynamicServiceProviderEngine(IEnumerable<ServiceDescriptor> serviceDescriptors, IServiceProviderEngineCallback callback) : base(serviceDescriptors, callback)
        {
        }

        protected override Func<ServiceProviderEngineScope, object> RealizeService(ServiceCallSite callSite, bool requestedForActivator)
        {
            // Skip runtime resolutions if service is going to be used in activator
            if (requestedForActivator)
            {
                return base.RealizeService(callSite, true);
            }

            var callCount = 0;
            return scope =>
            {
                if (Interlocked.Increment(ref callCount) == 2)
                {
                    Task.Run(() => base.RealizeService(callSite, false));
                }
                return RuntimeResolver.Resolve(callSite, scope);
            };
        }
    }
}
