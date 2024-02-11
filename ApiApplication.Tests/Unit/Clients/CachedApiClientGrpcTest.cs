using System;
using System.Threading;
using System.Threading.Tasks;
using ApiApplication.Clients;
using ApiApplication.Clients.Abstractions;
using ApiApplication.TypedExceptions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using ProtoDefinitions;
using Xunit;
using static ProtoDefinitions.MoviesApi;

namespace ApiApplication.Tests.Unit.Clients
{
    public class CachedApiClientGrpcTest
    {
        [Fact]
        public async Task GetAllAsync_Success_SetsCache()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var expectedResponse =  new showListResponse { Shows = { new showResponse { Id = "test" } }};
            var packedResponse = GrpcCallHelpers.CreateAsyncUnaryCall(new responseModel { Data = Any.Pack(expectedResponse) });
            var settings = Options.Create(new AppSettings
            {
                MoviesApi = new MoviesApiSettings
                {
                    Cache = new MoviesApiSettings.CacheSettings
                    {
                        TtlInSeconds = 1
                    }
                }
            });

            // Mocking the grpc client
            var grpcClient = Substitute.For<MoviesApiClient>();
            grpcClient.GetAllAsync(Arg.Any<ProtoDefinitions.Empty>(), cancellationToken: cancellationToken)
                      .Returns(packedResponse);

            // Mocking the cache client
            var cacheClient = Substitute.For<ICacheClient>();
            cacheClient.SetCacheValueAsync(Arg.Any<string>(), Arg.Any<showListResponse>(), cancellationToken, Arg.Any<TimeSpan>())
                       .Returns(Task.CompletedTask);

            var cachedApiClient = new CachedApiClientGrpc(grpcClient, cacheClient, settings);

            // Act
            var result = await cachedApiClient.GetAllAsync(cancellationToken);

            // Assert
            await cacheClient.Received(1).SetCacheValueAsync(Arg.Any<string>(), expectedResponse, cancellationToken, Arg.Any<TimeSpan>());
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task GetAllAsync_TransientError_OnSubsequentCall_ReturnsCache()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var expectedResponse = new showListResponse { Shows = { new showResponse { Id = "test" } } };
            var packedResponse = GrpcCallHelpers.CreateAsyncUnaryCall(new responseModel { Data = Any.Pack(expectedResponse) });
            var settings = Options.Create(new AppSettings
            {
                MoviesApi = new MoviesApiSettings
                {
                    Cache = new MoviesApiSettings.CacheSettings
                    {
                        TtlInSeconds = 1
                    }
                }
            });

            // Mocking the grpc client
            var grpcClient = Substitute.For<MoviesApiClient>();
            grpcClient.GetAllAsync(Arg.Any<ProtoDefinitions.Empty>(), cancellationToken: cancellationToken)
                      .Returns(
                        x => packedResponse,
                        x => { throw new RpcException(Status.DefaultCancelled); });

            // Mocking the cache client
            var cacheClient = Substitute.For<ICacheClient>();
            cacheClient.SetCacheValueAsync(Arg.Any<string>(), expectedResponse, cancellationToken, Arg.Any<TimeSpan>())
                       .Returns(Task.CompletedTask);
            cacheClient.GetCacheValueAsync<showListResponse>(Arg.Any<string>(), cancellationToken)
                       .Returns(expectedResponse);

            var cachedApiClient = new CachedApiClientGrpc(grpcClient, cacheClient, settings);

            // Act
            var responseFromApi = await cachedApiClient.GetAllAsync(cancellationToken);
            var responseFromCache = await cachedApiClient.GetAllAsync(cancellationToken);

            // Assert
            await cacheClient.Received(1).SetCacheValueAsync(Arg.Any<string>(), expectedResponse, cancellationToken, Arg.Any<TimeSpan>());
            await cacheClient.Received(1).GetCacheValueAsync<showListResponse>(Arg.Any<string>(), cancellationToken);
            Assert.Equal(expectedResponse, responseFromApi);
            Assert.Equal(expectedResponse, responseFromCache);
        }

        [Fact]
        public async Task GetAllAsync_SeriousError_ThrowsTypedException()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var expectedResponse = new showListResponse { Shows = { new showResponse { Id = "test" } } };
            var packedResponse = GrpcCallHelpers.CreateAsyncUnaryCall(new responseModel { Data = Any.Pack(expectedResponse) });
            var settings = Options.Create(new AppSettings
            {
                MoviesApi = new MoviesApiSettings
                {
                    Cache = new MoviesApiSettings.CacheSettings
                    {
                        TtlInSeconds = 1
                    }
                }
            });

            // Mocking the grpc client
            var grpcClient = Substitute.For<MoviesApiClient>();
            grpcClient.GetAllAsync(Arg.Any<ProtoDefinitions.Empty>(), cancellationToken: cancellationToken)
                      .Returns(packedResponse);

            // Mocking the cache client
            var cacheClient = Substitute.For<ICacheClient>();
            cacheClient.SetCacheValueAsync(Arg.Any<string>(), expectedResponse, cancellationToken, Arg.Any<TimeSpan>())
                       .Returns(x => { throw new Exception(); });

            var cachedApiClient = new CachedApiClientGrpc(grpcClient, cacheClient, settings);

            // Act and Assert
            await Assert.ThrowsAsync<UnexpectedErrorException>(() => cachedApiClient.GetAllAsync(cancellationToken));
        }

        [Fact]
        public async Task GetByIdAsync_Success_SetsCache()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var id = "test";
            var idRequest = new IdRequest { Id = id };
            var expectedResponse = new showResponse { Id = id };
            var packedResponse = GrpcCallHelpers.CreateAsyncUnaryCall(new responseModel { Data = Any.Pack(expectedResponse) });
            var settings = Options.Create(new AppSettings
            {
                MoviesApi = new MoviesApiSettings
                {
                    Cache = new MoviesApiSettings.CacheSettings
                    {
                        TtlInSeconds = 1
                    }
                }
            });

            // Mocking the grpc client
            var grpcClient = Substitute.For<MoviesApiClient>();
            grpcClient.GetByIdAsync(idRequest, cancellationToken: cancellationToken)
                      .Returns(packedResponse);

            // Mocking the cache client
            var cacheClient = Substitute.For<ICacheClient>();
            cacheClient.SetCacheValueAsync(Arg.Any<string>(), expectedResponse, cancellationToken, Arg.Any<TimeSpan>())
                       .Returns(Task.CompletedTask);

            var cachedApiClient = new CachedApiClientGrpc(grpcClient, cacheClient, settings);

            // Act
            var result = await cachedApiClient.GetByIdAsync(id, cancellationToken);

            // Assert
            await cacheClient.Received(1).SetCacheValueAsync(Arg.Any<string>(), expectedResponse, cancellationToken, Arg.Any<TimeSpan>());
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task GetByIdAsync_TransientError_OnSubsequentCall_ReturnsCache()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var id = "test";
            var idRequest = new IdRequest { Id = id };
            var expectedResponse = new showResponse { Id = id };
            var packedResponse = GrpcCallHelpers.CreateAsyncUnaryCall(new responseModel { Data = Any.Pack(expectedResponse) });
            var settings = Options.Create(new AppSettings
            {
                MoviesApi = new MoviesApiSettings
                {
                    Cache = new MoviesApiSettings.CacheSettings
                    {
                        TtlInSeconds = 1
                    }
                }
            });

            // Mocking the grpc client
            var grpcClient = Substitute.For<MoviesApiClient>();
            grpcClient.GetByIdAsync(idRequest, cancellationToken: cancellationToken)
                      .Returns(
                        x => packedResponse,
                        x => { throw new RpcException(Status.DefaultCancelled); });

            // Mocking the cache client
            var cacheClient = Substitute.For<ICacheClient>();
            cacheClient.SetCacheValueAsync(Arg.Any<string>(), expectedResponse, cancellationToken, Arg.Any<TimeSpan>())
                       .Returns(Task.CompletedTask);
            cacheClient.GetCacheValueAsync<showResponse>(Arg.Any<string>(), cancellationToken)
                       .Returns(expectedResponse);

            var cachedApiClient = new CachedApiClientGrpc(grpcClient, cacheClient, settings);

            // Act
            var responseFromApi = await cachedApiClient.GetByIdAsync(id, cancellationToken);
            var responseFromCache = await cachedApiClient.GetByIdAsync(id, cancellationToken);

            // Assert
            await cacheClient.Received(1).SetCacheValueAsync(Arg.Any<string>(), expectedResponse, cancellationToken, Arg.Any<TimeSpan>());
            await cacheClient.Received(1).GetCacheValueAsync<showResponse>(Arg.Any<string>(), cancellationToken);
            Assert.Equal(expectedResponse, responseFromApi);
            Assert.Equal(expectedResponse, responseFromCache);
        }

        [Fact]
        public async Task GetByIdAsync_SeriousError_ThrowsTypedException()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var id = "test";
            var idRequest = new IdRequest { Id = id };
            var expectedResponse = new showResponse { Id = id };
            var packedResponse = GrpcCallHelpers.CreateAsyncUnaryCall(new responseModel { Data = Any.Pack(expectedResponse) });
            var settings = Options.Create(new AppSettings
            {
                MoviesApi = new MoviesApiSettings
                {
                    Cache = new MoviesApiSettings.CacheSettings
                    {
                        TtlInSeconds = 1
                    }
                }
            });

            // Mocking the grpc client
            var grpcClient = Substitute.For<MoviesApiClient>();
            grpcClient.GetByIdAsync(idRequest, cancellationToken: cancellationToken)
                      .Returns(packedResponse);

            // Mocking the cache client
            var cacheClient = Substitute.For<ICacheClient>();
            cacheClient.SetCacheValueAsync(Arg.Any<string>(), expectedResponse, cancellationToken, Arg.Any<TimeSpan>())
                       .Returns(x => { throw new Exception(); });

            var cachedApiClient = new CachedApiClientGrpc(grpcClient, cacheClient, settings);

            // Act and Assert
            await Assert.ThrowsAsync<UnexpectedErrorException>(() => cachedApiClient.GetByIdAsync(id, cancellationToken));
        }
    }

    internal static class GrpcCallHelpers
    {
        public static AsyncUnaryCall<TResponse> CreateAsyncUnaryCall<TResponse>(TResponse response)
        {
            return new AsyncUnaryCall<TResponse>(
                Task.FromResult(response),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
        }

        public static AsyncUnaryCall<TResponse> CreateAsyncUnaryCall<TResponse>(StatusCode statusCode)
        {
            var status = new Status(statusCode, string.Empty);
            return new AsyncUnaryCall<TResponse>(
                Task.FromException<TResponse>(new RpcException(status)),
                Task.FromResult(new Metadata()),
                () => status,
                () => new Metadata(),
                () => { });
        }
    }
}
