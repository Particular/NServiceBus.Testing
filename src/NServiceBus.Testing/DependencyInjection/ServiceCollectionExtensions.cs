namespace NServiceBus.Testing.DependencyInjection
{
    using System;
    using LightInject;
    using LightInject.Microsoft.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;

    static class ServiceCollectionExtensions
    {
        public static IServiceProvider BuildDefaultNServiceBusProvider(this IServiceCollection serviceCollection)
        {
            var defaultOptions = new ContainerOptions {EnableVariance = false}.WithMicrosoftSettings();
            return serviceCollection.CreateLightInjectServiceProvider(defaultOptions);
        }
    }
}