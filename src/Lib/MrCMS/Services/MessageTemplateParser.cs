using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MrCMS.Helpers;
using MrCMS.Messages;

namespace MrCMS.Services
{
    public class MessageTemplateParser : IMessageTemplateParser
    {
        private static readonly MethodInfo GetMessageTemplateMethod = typeof(MessageTemplateParser)
            .GetMethodExt("GetAllTokens");

        private readonly IServiceProvider _serviceProvider;

        public MessageTemplateParser(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<string> Parse<T>(string template, T instance)
        {
            var stringBuilder = new StringBuilder(template);

            await ApplyTypeSpecificTokens(instance, stringBuilder);

            await ApplySystemWideTokens(stringBuilder);

            return stringBuilder.ToString();
        }

        public async Task<string> Parse(string template)
        {
            var stringBuilder = new StringBuilder(template);

            await ApplySystemWideTokens(stringBuilder);

            return stringBuilder.ToString();
        }

        private async Task ApplySystemWideTokens(StringBuilder stringBuilder)
        {
            var providers = _serviceProvider.GetServices<ITokenProvider>();
            foreach (var token in providers.SelectMany(provider => provider.Tokens))
            {
                stringBuilder.Replace("{" + token.Key + "}", await token.Value());
            }
        }

        private async Task ApplyTypeSpecificTokens<T>(T instance, StringBuilder stringBuilder)
        {
            IEnumerable<ITokenProvider<T>> tokenProviders = _serviceProvider.GetServices<ITokenProvider<T>>();

            tokenProviders = tokenProviders.OrderBy(provider =>
                provider.GetType().FullName == typeof(PropertyTokenProvider<T>).FullName ? 1 : 0);

            foreach (var token in tokenProviders.SelectMany(tokenProvider => tokenProvider.Tokens))
            {
                stringBuilder.Replace("{" + token.Key + "}", await token.Value(instance));
            }
        }

        public HashSet<string> GetAllTokens<T>()
        {
            IEnumerable<ITokenProvider<T>> tokenProviders = _serviceProvider.GetServices<ITokenProvider<T>>();
            return tokenProviders.SelectMany(provider => provider.Tokens.Select(pair => pair.Key)).ToHashSet();
        }

        public HashSet<string> GetAllTokens(MessageTemplate template)
        {
            var tokens = new HashSet<string>();
            tokens.AddRange(GetAllTokens(template.ModelType));
            tokens.AddRange(GetAllStandardTokens());
            return tokens;
        }

        private HashSet<string> GetAllTokens(Type type)
        {
            if (type == null)
                return new HashSet<string>();
            return GetMessageTemplateMethod.MakeGenericMethod(type).Invoke(this, new object[] { }) as HashSet<string>;
        }

        public HashSet<string> GetAllStandardTokens()
        {
            IEnumerable<ITokenProvider> tokenProviders = _serviceProvider.GetServices<ITokenProvider>();
            return tokenProviders.SelectMany(provider => provider.Tokens.Select(pair => pair.Key)).ToHashSet();
        }
    }
}