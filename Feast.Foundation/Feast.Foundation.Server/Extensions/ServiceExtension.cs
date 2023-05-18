﻿using Feast.Foundation.Core.Attributes;
using Feast.Foundation.Core.Implements.Services;
using Feast.Foundation.Server.Filters;
using Feast.Foundation.Server.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

// ReSharper disable IdentifierTypo

namespace Feast.Foundation.Server.Extensions
{
    public static partial class ServiceExtension
    {
        public static void ConfigureJwt<TIdentity>(
           this IServiceCollection services,
           Action<TokenValidationParameters>? configure = null,
           Func<TIdentity, Task>? validation = null,
           Func<JwtBearerChallengeContext, string>? failed = null)
        {
            var config = new JwtConfigure<TIdentity>(configure);
            services
                .AddSingleton(config)
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(o =>
                {
                    o.IncludeErrorDetails = true;
                    o.TokenValidationParameters = config.Parameters;
                    o.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = validation == null
                            ? _ => Task.CompletedTask
                            : async context =>
                            {
                                var token = (context.SecurityToken as JwtSecurityToken)!.RawData;
                                if (JwtExtension<TIdentity>.FromToken(token) == null)
                                {
                                    context.Fail(
                                        new NullReferenceException($"Cannot resolve {typeof(TIdentity)} from token"));
                                }
                                await Task.CompletedTask;
                            },

                        OnChallenge = async context =>
                        {
                            if (failed == null) return;
                            context.HandleResponse();
                            context.Response.Clear();
                            context.Response.ContentType = "application/json";
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsync(failed(context));
                        }
                    };
                });
        }

    }

    public static partial class ServiceExtension
    {
        public static IServiceCollection AddJwtSwaggerGen(this IServiceCollection collection)
        {
            return collection.AddSwaggerGen(o =>
            {
                o.OperationFilter<AuthorizationFilter>();
                o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Jwt Token Format like [ Bearer {Token} ]",
                    Name = nameof(Authorization), 
                    In = ParameterLocation.Header, 
                    Type = SecuritySchemeType.ApiKey
                });
            });
        }

        /// <summary>
        /// 实现自动注入携带 <see cref="AutowiredAttribute"/> 注解的属性和字段
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHostBuilder UseAutowiredServiceProviderFactory(this IHostBuilder builder) =>
            builder.UseServiceProviderFactory(new AutowiredServiceProviderFactory(
                ServiceCollectionContainerBuilderExtensions.BuildServiceProvider));

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TAttribute">属性</typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHostBuilder UseAutowiredServiceProviderFactory<TAttribute>(this IHostBuilder builder)
            where TAttribute : Attribute =>
            builder.UseServiceProviderFactory(new AutowiredServiceProviderFactory<TAttribute>(
                ServiceCollectionContainerBuilderExtensions.BuildServiceProvider));
    }
}
