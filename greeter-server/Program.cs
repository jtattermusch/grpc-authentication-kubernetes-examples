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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Helloworld;

using JWT;
using JWT.Builder;
using JWT.Algorithms;

namespace GreeterServer
{
    class GreeterImpl : Greeter.GreeterBase
    {
        // Server side handler of the SayHello RPC
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            var jwtToken = GetAuthBearerToken(context.RequestHeaders);

            if (jwtToken != null)
            {
                try
                {
                    var json = new JwtBuilder()
                        .WithSecret("dffaaf")
                        .MustVerifySignature()
                        .Decode(jwtToken);                    
                    Console.WriteLine(json);

                    // TODO: validate aud, iss, iat, exp, sub, ...
                }
                catch (TokenExpiredException)
                {
                    Console.WriteLine("Token has expired");
                }
                catch (SignatureVerificationException)
                {
                    Console.WriteLine("Token has invalid signature");
                }
            }
            
            // TODO: add note that we would normally use an interceptor
            Console.WriteLine("JWT token: " + jwtToken);
         
            return Task.FromResult(new HelloReply { Message = "Hello " + request.Name + " (" + context.Peer + ")" });
        }

        private static string GetAuthBearerToken(Metadata requestHeaders)
        {
            var authToken = requestHeaders.FirstOrDefault((entry) => entry.Key == "authorization")?.Value;

            if (authToken == null)
            {
                return null;
            }

            var parts = authToken.Split(" ", 2);
            if (parts.Length == 2 && parts[0] == "Bearer")
            {
                return parts[1];
                
            }
            return null;
        }
    }

    

    class Program
    {
        const int Port = 8000;

        public static void Main(string[] args)
        {
            ServerCredentials serverCredentials = null;
            var securityOption = Environment.GetEnvironmentVariable("GREETER_SERVER_SECURITY");
            if (securityOption == "insecure")
            {
                serverCredentials = ServerCredentials.Insecure;
            }
            else if (securityOption == "tls")
            {
                serverCredentials = CreateSslServerCredentials(mutualTls: false);
            }
            else if (securityOption == "mtls")
            {
                serverCredentials = CreateSslServerCredentials(mutualTls: true);
            }
            else 
            {
                throw new ArgumentException("Illegal security option.");
            }
            Console.WriteLine("Starting server with security: " + securityOption);

            Server server = new Server()
            {
                Services = { Greeter.BindService(new GreeterImpl()) },
                Ports = { new ServerPort("0.0.0.0", Port, serverCredentials) },
            };
            server.Start();
            Console.WriteLine("Started server on port " + Port);

            // wait forever
            server.ShutdownTask.Wait();
        }

        public static SslServerCredentials CreateSslServerCredentials(bool mutualTls)
        {
            var certsPath = Environment.GetEnvironmentVariable("CERTS_PATH");
            
            var keyCertPair = new KeyCertificatePair(
                File.ReadAllText(Path.Combine(certsPath, "server.pem")),
                File.ReadAllText(Path.Combine(certsPath, "server.key")));
            
            if (!mutualTls)
            {
                return new SslServerCredentials(new[] { keyCertPair });
            }

            var caRoots = File.ReadAllText(Path.Combine(certsPath, "ca.pem"));
            return new SslServerCredentials(new[] { keyCertPair }, caRoots, SslClientCertificateRequestType.RequestAndRequireAndVerify); 
        }
    }
}
