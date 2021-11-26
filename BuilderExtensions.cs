using DotNetCore.CAP;
using JKang.EventSourcing;
using JKang.EventSourcing.Domain;
using JKang.EventSourcing.Persistence;
using JKang.EventSourcing.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for the <see cref="IEventSourcingBuilder"/>.
/// </summary>
public static class BuilderExtensions
{
    /// <summary>
    /// Configures
    /// </summary>
    /// <typeparam name="TEventDbContext"></typeparam>
    /// <typeparam name="TAggregate"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="builder">The event sourcing builder used to configure the application's use of the
    /// <see cref="JKang.EventSourcing"/> library.</param>
    /// <param name="capSetupAction">An action to configure the use of the use of the <see cref="DotNetCore.CAP"/> library.</param>
    /// <returns>An <see cref="IEventConversionStage"/> to configure the mapping of domain events to integration events.</returns>
    public static IEventConversionStage UseEfCoreEventStoreWithCap<TEventDbContext, TAggregate, TKey>(
        this IEventSourcingBuilder builder,
        Action<CapOptions> capSetupAction)
        where TEventDbContext : DbContext, IEventDbContext<TAggregate, TKey>
        where TAggregate : IAggregate<TKey>
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services
            .AddScoped<IEventStore<TAggregate, TKey>, EfCoreEventStore<TEventDbContext, TAggregate, TKey>>()
            .Decorate<IEventStore<TAggregate, TKey>, EventStoreCapEFCoreDecorator<TEventDbContext, TAggregate, TKey>>()
            .AddScoped<IEventStoreInitializer<TAggregate, TKey>, EfCoreEventStoreInitializer<TEventDbContext, TAggregate, TKey>>();

        builder.Services.AddCap(capSetupAction);

        builder.TryUseDefaultSnapshotStore<TAggregate, TKey>();

        return new EventConversionStage(builder);
    }
}