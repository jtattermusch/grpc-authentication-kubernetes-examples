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
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Helloworld;

using JWT;
using JWT.Builder;
using JWT.Algorithms;

namespace GreeterServer
{
    class Program
    {
        const int Port = 8000;

        public static void Main(string[] args)
        {
            ServerCredentials serverCredentials = null;
            var securityOption = Environment.GetEnvironmentVariable("GREETER_SERVER_SECURITY");
            switch (securityOption)
            {
                case "insecure":
                    serverCredentials = ServerCredentials.Insecure;
                    break;
                case "tls":
                    serverCredentials = CreateSslServerCredentials(mutualTls: false);
                    break;
                case "mtls":
                    serverCredentials = CreateSslServerCredentials(mutualTls: true);
                    break;
                default:
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
