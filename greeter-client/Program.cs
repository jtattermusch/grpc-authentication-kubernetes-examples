// Copyright 2019 The gRPC Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using Grpc.Core;
using Helloworld;
using System.Threading;
using JWT;
using JWT.Builder;
using JWT.Algorithms;

namespace GreeterClient
{
    class Program
    {
        public static void Main(string[] args)
        {
            var channelTarget = Environment.GetEnvironmentVariable("GREETER_SERVICE_TARGET");
            Console.WriteLine("Creating channel with target " + channelTarget);

            ChannelCredentials channelCredentials = null;
            var securityOption = Environment.GetEnvironmentVariable("GREETER_CLIENT_SECURITY");
            switch (securityOption)
            {
                case "insecure":
                    channelCredentials = ChannelCredentials.Insecure;
                    break;
                case "tls":
                    channelCredentials = CreateCredentials(mutualTls: false, useJwt: false);
                    break;
                case "jwt":
                    channelCredentials = CreateCredentials(mutualTls: false, useJwt: true);
                    break;
                case "mtls":
                    channelCredentials = CreateCredentials(mutualTls: true, useJwt: false);
                    break;
                default:
                    throw new ArgumentException("Illegal security option.");
            }
            Console.WriteLine("Starting client with security: " + securityOption);

            Channel channel = new Channel(channelTarget, channelCredentials);

            var client = new Greeter.GreeterClient(channel);
            String user = "you";
            
            for (int i = 0; i < 10000; i++)
            {
                try
                {
                  var reply = client.SayHello(new HelloRequest { Name = user });
                  Console.WriteLine("Greeting: " + reply.Message);
                } 
                catch (RpcException e)
                {
                   Console.WriteLine("Error invoking greeting: " + e.Status);
                }
                
                Thread.Sleep(1000);
            }
            channel.ShutdownAsync().Wait();
            Console.WriteLine();
        }

        static ChannelCredentials CreateCredentials(bool mutualTls, bool useJwt)
        {
            var certsPath = Environment.GetEnvironmentVariable("CERTS_PATH");

            var caRoots = File.ReadAllText(Path.Combine(certsPath, "ca.pem"));
            ChannelCredentials channelCredentials;
            if (!mutualTls)
            {
                channelCredentials = new SslCredentials(caRoots);
            }
            else
            {
                var keyCertPair = new KeyCertificatePair(
                    File.ReadAllText(Path.Combine(certsPath, "client.pem")),
                    File.ReadAllText(Path.Combine(certsPath, "client.key")));
                channelCredentials = new SslCredentials(caRoots, keyCertPair);
            }
    
            if (useJwt)
            {
                var authInterceptor = new AsyncAuthInterceptor(async (context, metadata) =>
                {
                    metadata.Add(
                        new Metadata.Entry("authorization", "Bearer " + GenerateJwt()));
                });

                var metadataCredentials = CallCredentials.FromInterceptor(authInterceptor);
                channelCredentials = ChannelCredentials.Create(channelCredentials, metadataCredentials); 
            }
            return channelCredentials;
        }

        // generates a signed JWT token
        static string GenerateJwt()
        {
            var token = new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                // secret good for testing only!
                // normally stored as kubernetes secret, but for simplicity it's hardcoded
                .WithSecret("GrpcAuthDemoTestOnlySecret12345")
                .AddClaim("exp", DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600)  // valid for 1hr
                .AddClaim("iss", "demo-jwt-issuer@cluster.local")
                .AddClaim("sub", "demo-jwt-subject@cluster.local")
                .AddClaim("aud", "helloworld.Greeter")  // request is for greeter service
                .Build();
            return token;
        }
    }
}
