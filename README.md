# MicroBootstrap

> A Full Stack framework written in .NET Core to speed up your development process in microservices and modular monolith apps. It gathers most widely used frameworks in .NET world and pack them into a simple bootstrap package.

Branch | Status
--- | :---:
main | ![build-test-main](https://github.com/mehdihadeli/micro-bootstrap/actions/workflows/build-test.yml/badge.svg?branch=main)
develop |![build-test-develop](https://github.com/mehdihadeli/micro-bootstrap/actions/workflows/build-test.yml/badge.svg?branch=develop)

## Installation

MicroBootstrap is seperated into multiple nuget packages available on nuget.org.

| Packages | Stats |
:--- | :---
[MicroBootstrap.Core](https://www.nuget.org/packages/MicroBootstrap.Core) | [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Core?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Core)
[MicroBootstrap.Abstractions](https://www.nuget.org/packages/MicroBootstrap.Abstraction)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Abstractions?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Abstraction)
[MicroBootstrap.Caching.InMemory](https://www.nuget.org/packages/MicroBootstrap.Caching.InMemory) | [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Caching.InMemory?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Caching.InMemory)
[MicroBootstrap.Caching.Redis](https://www.nuget.org/packages/MicroBootstrap.Caching.Redis)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Caching.Redis?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Caching.Redis)
[MicroBootstrap.CQRS](https://www.nuget.org/packages/MicroBootstrap.CQRS)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.CQRS?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.CQRS)
[MicroBootstrap.Email](https://www.nuget.org/packages/MicroBootstrap.Email)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Email?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Email)
[MicroBootstrap.Logging](https://www.nuget.org/packages/MicroBootstrap.Logging)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Logging?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Logging)
[MicroBootstrap.Messaging.Postgres](https://www.nuget.org/packages/MicroBootstrap.Messaging.Postgres)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Messaging.Postgres?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Messaging.Postgres)
[MicroBootstrap.Messaging.InMemory](https://www.nuget.org/packages/MicroBootstrap.Messaging.InMemory)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Messaging.InMemory?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Messaging.InMemory)
[MicroBootstrap.Messaging.Transport.Rabbitmq](https://www.nuget.org/packages/MicroBootstrap.Messaging.Transport.Rabbitmq)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Messaging.Transport.Rabbitmq?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Messaging.Transport.Rabbitmq)
[MicroBootstrap.Messaging.Transport.Kafka](https://www.nuget.org/packages/MicroBootstrap.Messaging.Transport.Kafka)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Messaging.Transport.Kafka?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Messaging.Transport.Kafka)
[MicroBootstrap.Messaging.Transport.InMemory](https://www.nuget.org/packages/MicroBootstrap.Messaging.Transport.InMemory) | [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Messaging.Transport.InMemory?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Messaging.Transport.InMemory)
[MicroBootstrap.Monitoring](https://www.nuget.org/packages/MicroBootstrap.Monitoring)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Monitoring?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Monitoring)
[MicroBootstrap.Persistence.EfCore.Postgres](https://www.nuget.org/packages/MicroBootstrap.Persistence.EfCore.Postgres)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Persistence.EfCore.Postgres?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Persistence.EfCore.Postgres)
[MicroBootstrap.Persistence.EventStoreDB](https://www.nuget.org/packages/MicroBootstrap.Persistence.EventStoreDB)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Persistence.EventStoreDB?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Persistence.EventStoreDB)
[MicroBootstrap.Persistence.Mongo](https://www.nuget.org/packages/MicroBootstrap.Persistence.Mongo)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Persistence.Mongo?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Persistence.Mongo)
[MicroBootstrap.Persistence.ElasticSearch](https://www.nuget.org/packages/MicroBootstrap.Persistence.ElasticSearch)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Persistence.ElasticSearch?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Persistence.ElasticSearch)
[MicroBootstrap.Persistence.Dapper](https://www.nuget.org/packages/MicroBootstrap.Persistence.Dapper) | [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Persistence.Dapper?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Persistence.Dapper)
[MicroBootstrap.Resiliency](https://www.nuget.org/packages/MicroBootstrap.Resiliency)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Resiliency?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Resiliency)
[MicroBootstrap.Scheduling.Internal](https://www.nuget.org/packages/MicroBootstrap.Scheduling.Internal)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Scheduling.Internal?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Scheduling.Internal)
[MicroBootstrap.Scheduling.Hangfire](https://www.nuget.org/packages/MicroBootstrap.Scheduling.Hangfire)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Scheduling.Hangfire?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Scheduling.Hangfire)
[MicroBootstrap.Security](https://www.nuget.org/packages/MicroBootstrap.Security)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Security?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Security)
[MicroBootstrap.Serialization](https://www.nuget.org/packages/MicroBootstrap.Serialization)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Serialization?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Serialization)
[MicroBootstrap.Swagger](https://www.nuget.org/packages/MicroBootstrap.Swagger)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Swagger?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Swagger)
[MicroBootstrap.Tracing](https://www.nuget.org/packages/MicroBootstrap.Tracing)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Tracing?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Tracing)
[MicroBootstrap.Validation](https://www.nuget.org/packages/MicroBootstrap.Validation)| [![NuGet](https://buildstats.info/nuget/MicroBootstrap.Validation?includePreReleases=true)](https://www.nuget.org/packages/MicroBootstrap.Validation)

## Give a Star! ⭐️
If you like or are using this repository to learn or start your solution, please give it a star. Thanks!

## 🤝 Contributing
----------------
1. Fork it ( https://github.com/mehdihadeli/MicroBootstrap/fork )
2. Create your feature branch (`git checkout -b my-new-feature`)
3. Commit your changes (`git commit -am 'Add some feature'`)
4. Push to the branch (`git push origin my-new-feature`)
5. Create a new Pull Request 

