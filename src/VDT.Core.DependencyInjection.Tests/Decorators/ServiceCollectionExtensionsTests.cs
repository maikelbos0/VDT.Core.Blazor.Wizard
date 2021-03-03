﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using VDT.Core.DependencyInjection.Decorators;
using Xunit;

namespace VDT.Core.DependencyInjection.Tests.Decorators {
    public sealed class ServiceCollectionExtensionsTests {
        private readonly ServiceCollection services;
        private readonly TestDecorator decorator;

        public ServiceCollectionExtensionsTests() {
            services = new ServiceCollection();
            decorator = new TestDecorator();

            services.AddSingleton(decorator);
        }

        [Fact]
        public async Task AddScoped_Adds_DecoratorInjectors() {
            services.AddScoped<IServiceCollectionTarget, ServiceCollectionTarget, ServiceCollectionTarget>(options => {
                options.AddDecorator<TestDecorator>();
                options.AddDecorator<TestDecorator>();
            });

            var serviceProvider = services.BuildServiceProvider();
            var proxy = serviceProvider.GetRequiredService<IServiceCollectionTarget>();

            Assert.Equal("Bar", await proxy.GetValue());

            Assert.Equal(2, decorator.Calls);
        }

        [Fact]
        public void AddScoped_Throws_Exception_For_Equal_Service_And_ImplementationService() {
            Assert.Throws<ServiceRegistrationException>(() => services.AddScoped<IServiceCollectionTarget, IServiceCollectionTarget, ServiceCollectionTarget>(options => { }));
        }

        [Fact]
        public async Task AddScoped_With_Factory_Adds_DecoratorInjectors() {
            services.AddScoped<IServiceCollectionTarget, ServiceCollectionTarget, ServiceCollectionTarget>(serviceProvider => new ServiceCollectionTarget {
                Value = "Foo"
            }, options => {
                options.AddDecorator<TestDecorator>();
                options.AddDecorator<TestDecorator>();
            });

            var serviceProvider = services.BuildServiceProvider();
            var proxy = serviceProvider.GetRequiredService<IServiceCollectionTarget>();

            Assert.Equal("Foo", await proxy.GetValue());

            Assert.Equal(2, decorator.Calls);
        }

        [Fact]
        public void AddScoped_With_Factory_Throws_Exception_For_Equal_Service_And_ImplementationService() {
            Assert.Throws<ServiceRegistrationException>(() => services.AddScoped<IServiceCollectionTarget, IServiceCollectionTarget, ServiceCollectionTarget>(serviceProvider => new ServiceCollectionTarget {
                Value = "Foo"
            }, options => { }));
        }
    }
}