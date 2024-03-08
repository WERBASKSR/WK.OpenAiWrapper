using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;

namespace OpenAiWrapper
{
    public static class InitializeExtensions
    {
        public static ServiceCollection RegisterPilots(this ServiceCollection serviceCollection, params Pilot[] pilots)
        {
            foreach (Pilot pilot in pilots)
            {
                serviceCollection.AddKeyedSingleton(pilot.Name, pilot);
            }

            return serviceCollection;
        }
        
        public static ServiceCollection RegisterOnThreadExpired(this ServiceCollection serviceCollection, Action<string,string> onThreadExpiredDelegate)
        {
            Constants.OnThreadExpiredDelegate = onThreadExpiredDelegate;
            return serviceCollection;
        }

        public static IServiceProvider UsePilots(this IServiceProvider serviceProvider)
        {
            Constants.ServiceProvider = serviceProvider;
            return serviceProvider;
        }
    }
}