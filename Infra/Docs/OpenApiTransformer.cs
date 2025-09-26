using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenIddict.Server;

namespace IbraHabra.NET.Infra.Docs;

public class OpenApiTransformer : IOpenApiDocumentTransformer
{
    private readonly IOptions<OpenIddictServerOptions> _options;

    public OpenApiTransformer(IOptions<OpenIddictServerOptions> options)
    {
        _options = options;
    }

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var options = _options.Value;

        // Create the discovery path dynamically
        var discoveryPath = "/.well-known/openid-configuration";

        document.Paths[discoveryPath] = new OpenApiPathItem
        {
            Operations = new Dictionary<OperationType, OpenApiOperation>
            {
                [OperationType.Get] = new OpenApiOperation
                {
                    Tags = new List<OpenApiTag> { new OpenApiTag { Name = "OpenID Connect Discovery" } },
                    Summary = "OpenID Connect Discovery Document",
                    Description = "Returns the OpenID Connect discovery metadata",
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse
                        {
                            Description = "OpenID Connect metadata",
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Type = "object",
                                        Properties = new Dictionary<string, OpenApiSchema>
                                        {
                                            ["issuer"] = new OpenApiSchema
                                            {
                                                Type = "string", Description = "The issuer URL",
                                                Default = new Microsoft.OpenApi.Any.OpenApiString(
                                                    options.Issuer?.ToString())
                                            },
                                            ["authorization_endpoint"] = new OpenApiSchema
                                            {
                                                Type = "string",
                                                Default = new Microsoft.OpenApi.Any.OpenApiString(
                                                    options.AuthorizationEndpointUris.ToString())
                                            },
                                            ["token_endpoint"] = new OpenApiSchema
                                            {
                                                Type = "string",
                                                Default = new Microsoft.OpenApi.Any.OpenApiString(
                                                    options.TokenEndpointUris.ToString())
                                            },
                                            ["userinfo_endpoint"] = new OpenApiSchema
                                            {
                                                Type = "string",
                                                Default = new Microsoft.OpenApi.Any.OpenApiString(
                                                    options.UserInfoEndpointUris.ToString())
                                            },
                                            ["jwks_uri"] = new OpenApiSchema
                                            {
                                                Type = "string",
                                                Default = new Microsoft.OpenApi.Any.OpenApiString(
                                                    options.JsonWebKeySetEndpointUris.ToString())
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        return Task.CompletedTask;
    }
}