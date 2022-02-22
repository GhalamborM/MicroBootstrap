using MicroBootstrap.Email.Options;

namespace MicroBootstrap.Email;

public static class Extensions
{
    public static WebApplicationBuilder AddEmailService(
        this WebApplicationBuilder builder,
        IConfiguration configuration,
        EmailProvider provider = EmailProvider.MimKit,
        Action<EmailOptions>? configure = null)
    {
        AddEmailService(builder.Services, configuration, provider, configure);

        return builder;
    }

    public static IServiceCollection AddEmailService(
        this IServiceCollection services,
        IConfiguration configuration,
        EmailProvider provider = EmailProvider.MimKit,
        Action<EmailOptions>? configureOptions = null)
    {
        var config = configuration.GetOptions<EmailOptions>(nameof(EmailOptions));

        if (provider == EmailProvider.SendGrid)
        {
            services.AddSingleton<IEmailSender, SendGridEmailSender>();
        }
        else
        {
            services.AddSingleton<IEmailSender, MimeKitEmailSender>();
        }

        if (configureOptions is { })
        {
            services.Configure(nameof(EmailOptions), configureOptions);
        }
        else
        {
            services.AddOptions<EmailOptions>().Bind(configuration.GetSection(nameof(EmailOptions)))
                .ValidateDataAnnotations();
        }

        return services;
    }
}
