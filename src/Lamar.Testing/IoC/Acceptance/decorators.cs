﻿using System;
using System.Linq;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Acceptance;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class decorators
    {
        [Fact]
        public void decorator_example()
        {
            var container = new Container(_ =>
            {
                // This usage adds the WidgetHolder as a decorator
                // on all IWidget registrations
                _.For<IWidget>().DecorateAllWith<WidgetHolder>();
                
                // The AWidget type will be decorated w/ 
                // WidgetHolder when you resolve it from the container
                _.For<IWidget>().Use<AWidget>();
                
                _.For<IThing>().Use<Thing>();
            });

            // Just proving that it actually works;)
            container.GetInstance<IWidget>()
                .ShouldBeOfType<WidgetHolder>()
                .Inner.ShouldBeOfType<AWidget>();
        }
        
        [Fact]
        public void decorator_example_alternative_registration()
        {
            var container = new Container(_ =>
            {
                // This usage adds the WidgetHolder as a decorator
                // on all IWidget registrations and makes AWidget
                // the default
                _.Policies.DecorateWith<DecoratorPolicy<IWidget, WidgetHolder>>();
                _.For<IWidget>().Use<AWidget>();
                _.For<IThing>().Use<Thing>();
            });

            container.GetInstance<IWidget>()
                .ShouldBeOfType<WidgetHolder>()
                .Inner.ShouldBeOfType<AWidget>();
        }
        
        [Fact]
        public void multiple_decorators()
        {
            var container = new Container(_ =>
            {
                // This usage adds the WidgetHolder as a decorator
                // on all IWidget registrations and makes AWidget
                // the default
                _.For<IWidget>().DecorateAllWith<WidgetHolder>();
                _.For<IWidget>().DecorateAllWith<OtherWidgetHolder>();
                _.For<IWidget>().Use<AWidget>();
                _.For<IThing>().Use<Thing>();
            });

            container.GetInstance<IWidget>()
                .ShouldBeOfType<OtherWidgetHolder>()
                .Inner.ShouldBeOfType<WidgetHolder>()
                .Inner.ShouldBeOfType<AWidget>();
        }
        
        [Fact]
        public void decorator_is_applied_on_configure_when_it_is_a_new_family()
        {
            var container = new Container(_ =>
            {
                // This usage adds the WidgetHolder as a decorator
                // on all IWidget registrations and makes AWidget
                // the default
                _.For<IWidget>().DecorateAllWith<WidgetHolder>();
                _.For<IThing>().Use<Thing>();
            });
            
            container.Configure(x => x.AddTransient<IWidget, BWidget>());
            
            container.GetInstance<IWidget>()
                .ShouldBeOfType<WidgetHolder>()
                .Inner.ShouldBeOfType<BWidget>();
        }
        
        [Fact]
        public void decorator_is_applied_on_configure_when_it_is_an_existing_family()
        {
            var container = new Container(_ =>
            {
                // This usage adds the WidgetHolder as a decorator
                // on all IWidget registrations and makes AWidget
                // the default
                _.For<IWidget>().DecorateAllWith<WidgetHolder>();
                _.For<IThing>().Use<Thing>();
                _.For<IWidget>().Use<AWidget>();
            });
            
            container.Configure(x => x.AddTransient<IWidget, BWidget>());
            
            container.GetInstance<IWidget>()
                .ShouldBeOfType<WidgetHolder>()
                .Inner.ShouldBeOfType<BWidget>();
        }
        
        [Fact]
        public void copy_the_name_and_lifetime_from_the_inner_as_transient()
        {
            var container = new Container(_ =>
            {
                // This usage adds the WidgetHolder as a decorator
                // on all IWidget registrations and makes AWidget
                // the default
                _.For<IWidget>().DecorateAllWith<WidgetHolder>();
                _.For<IWidget>().Use<AWidget>().Named("A").Transient();
                _.For<IThing>().Use<Thing>();
            });
            
            container.GetInstance<IWidget>()
                .ShouldBeOfType<WidgetHolder>()
                .Inner.ShouldBeOfType<AWidget>();

            var @default = container.Model.For<IWidget>().Default.ShouldBeOfType<ConstructorInstance>();
            @default.Name.ShouldBe("A");
            @default.Lifetime.ShouldBe(ServiceLifetime.Transient);
        }
        
        [Fact]
        public void copy_the_name_and_lifetime_from_the_inner()
        {
            var container = new Container(_ =>
            {
                // This usage adds the WidgetHolder as a decorator
                // on all IWidget registrations and makes AWidget
                // the default
                _.For<IWidget>().DecorateAllWith<WidgetHolder>();
                _.For<IWidget>().Use<AWidget>().Named("A").Singleton();
                _.For<IThing>().Use<Thing>();
            });
            
            container.GetInstance<IWidget>()
                .ShouldBeOfType<WidgetHolder>()
                .Inner.ShouldBeOfType<AWidget>();

            var @default = container.Model.For<IWidget>().Default.ShouldBeOfType<ConstructorInstance>();
            @default.Name.ShouldBe("A");
            @default.Lifetime.ShouldBe(ServiceLifetime.Singleton);
        }
        
        [Fact]
        public void copy_the_name_and_lifetime_from_the_inner_2()
        {
            var container = new Container(_ =>
            {
                // This usage adds the WidgetHolder as a decorator
                // on all IWidget registrations and makes AWidget
                // the default
                _.For<IWidget>().DecorateAllWith<WidgetHolder>();
                _.For<IWidget>().Use<AWidget>().Named("A").Scoped();
                _.For<IThing>().Use<Thing>();
            });
            
            container.GetInstance<IWidget>()
                .ShouldBeOfType<WidgetHolder>()
                .Inner.ShouldBeOfType<AWidget>();

            var @default = container.Model.For<IWidget>().Default.ShouldBeOfType<ConstructorInstance>();
            @default.Name.ShouldBe("A");
            @default.Lifetime.ShouldBe(ServiceLifetime.Scoped);
        }
        
        
        public class WidgetHolder : IWidget
        {
            public WidgetHolder(IThing thing, IWidget inner)
            {
                Inner = inner;
            }

            public IWidget Inner { get; }
            
            public void DoSomething()
            {
                
            }
        }

        public class OtherWidgetHolder : WidgetHolder
        {
            public OtherWidgetHolder(IThing thing, IWidget inner2) : base(thing, inner2)
            {
            }
        }

    }
}