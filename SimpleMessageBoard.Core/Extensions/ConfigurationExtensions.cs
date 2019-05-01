namespace SimpleMessageBoard.Configuration
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public static class ConfigurationExtensions
    {
        public static IServiceCollection ConfigureSection<TOptions>(this IServiceCollection services, IConfiguration config, bool optional = false)
        where TOptions : class
        {
            var names = SectionNames.GetNames<TOptions>();
            var section = config.GetSection(names.PlainName) ?? config.GetSection(names.PostfixedName);

            if (section == null)
            {
                if (!optional)
                {
                    throw new Exception($"No configuration section named {names.PlainName} or {names.PostfixedName} exists");
                }

                return services;
            }

            return services.Configure<TOptions>(section);
        }

        public static TOptions GetSection<TOptions>(this IConfiguration config)
        where TOptions : class
        {
            var names = SectionNames.GetNames<TOptions>();
            var section = config.GetSection(names.PlainName) ?? config.GetSection(names.PostfixedName);
            if (section == null)
            {
                return null;
            }

            return section.Get<TOptions>();
        }

        private class SectionNames
        {
            public string PlainName;
            public string PostfixedName;

            public static SectionNames GetNames<TOptions>()
            {
                const string postFix = "Config";
                var sectionName = typeof(TOptions).Name;

                var postfixStart = sectionName.LastIndexOf(postFix, StringComparison.OrdinalIgnoreCase);
                if (sectionName.Length - postFix.Length == postfixStart)
                {
                    return new SectionNames
                    {
                        PlainName = sectionName.Substring(0, postfixStart),
                        PostfixedName = sectionName
                    };
                }

                return new SectionNames
                {
                    PlainName = sectionName,
                    PostfixedName = sectionName + postFix
                };
            }
        }
    }
}
